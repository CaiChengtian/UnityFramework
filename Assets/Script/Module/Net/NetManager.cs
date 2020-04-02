/*
	Copyright (c) 20xx XXX. All rights reserved.
	Description:消息格式 = 序列号 + 协议号 + 数据
    具体每一部分的长度,见配置NetConfig.cs
*/
using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace UnityFramework
{
    /// <summary>
    /// 网络管理器
    /// </summary>
    public class NetManager : Manager<NetManager>
    {
#if SIM
        //调试模式
        //消息处理逻辑字典
        private Dictionary<int, Action<byte[]>> dicLogic;
        //发送消息的委托,用于替代Socket
        public Action<byte[]> netLogic;
        //最大延时(秒)
        private float networkDelay = 0.2f;

        // 消息序列号,用于保证消息同步(发送消息时取出,收到消息时赋值)
        private uint Sequence
        {
            get;
            set;
        }

        /// <summary>
        /// 是否已连接服务器
        /// </summary>
        public bool IsConnected
        {
            get;
            private set;
        }


        void Awake()
        {
            Server.Instance.netLogic += DelayReceive;

            dicLogic = new Dictionary<int, Action<byte[]>>();
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        public void Connect()
        {
            Sequence = 0;
            IsConnected = true;
            Debug.LogWarning("Connected!");
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void DisConnect()
        {
            IsConnected = false;
            Debug.LogWarning("Disconneted!");
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="netPack">要发送的消息(Send)</param>
        public void Send(INetPack netPack)
        {
            Assert.AreNotEqual<Protocol>(Protocol.Invalid, netPack.Protocol);
            if (!IsConnected)
            {
                Connect();
            }
            byte[] data = Utility.ToBytes(netPack);
            Assert.IsTrue(data.Length < NetConfig.messageBodyLength);
            byte[] sequence = BitConverter.GetBytes(Sequence);
            byte[] protocol = BitConverter.GetBytes((int)netPack.Protocol);
            byte[] head = Utility.CombineBytes(sequence, protocol);
            byte[] send = Utility.CombineBytes(head, data);
            Debug.LogWarning("Client Send:" + netPack.Protocol.ToString());
            netLogic(send);
        }

        /// <summary>
        /// 延时接收消息
        /// </summary>
        /// <param name="bytes"></param>
        private void DelayReceive(byte[] bytes)
        {
            StartCoroutine(Receive(bytes));
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="bytes"></param>
        private IEnumerator Receive(byte[] bytes)
        {
            //模拟延时
            yield return new WaitForSeconds(networkDelay);

            byte[] sequence = new byte[NetConfig.MessageHeadSequenceLength];
            Array.Copy(bytes, sequence, NetConfig.MessageHeadSequenceLength);
            Sequence = BitConverter.ToUInt32(sequence, 0);

            byte[] protocol = new byte[NetConfig.MessageHeadProtocolLength];
            Array.Copy(bytes, NetConfig.MessageHeadSequenceLength, protocol, 0, NetConfig.MessageHeadProtocolLength);
            int protocolNum = BitConverter.ToInt32(protocol, 0);

            Action<byte[]> action;
            if (dicLogic.TryGetValue(protocolNum, out action))
            {
                if (action == null)
                {
                    Debug.LogError("Server logic is null:" + ((Protocol)protocolNum).ToString());
                }
                else
                {
                    int dataLength = bytes.Length - NetConfig.MessageHeadLength;
                    byte[] data = new byte[dataLength];
                    Array.Copy(bytes, NetConfig.MessageHeadLength, data, 0, dataLength);
                    Debug.LogWarning("Client Receive:" + ((Protocol)protocolNum).ToString());
                    action(data);
                }
            }
        }

        /// <summary>
        /// 注册消息处理逻辑
        /// </summary>
        /// <param name="protocol">协议号</param>
        /// <param name="callback">回调函数</param>
        public void Register(Protocol protocol, Action<byte[]> callback)
        {
            Action<byte[]> action;
            if (dicLogic.TryGetValue((int)protocol, out action))
            {
                if (action == null)
                {
                    action = callback;
                    return;
                }
            }
            else
            {
                dicLogic.Add((int)protocol, callback);
                return;
            }
        }

#else
//正常模式
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private Dictionary<int, Action<byte[]>> dicLogic;
        //消息队列
        private Queue<byte[]> messageQueue;
        private Thread thread;

        private void Begin()
        {
            if (thread == null)
            {
                thread = new Thread(ClientThread);
            }

            if (!thread.IsAlive)
            {
                thread.Start();
            }
        }

        private void ClientThread()
        {
            int length;
            while (true)
            {
                if (IsConnected)
                {
                    byte[] buffer = new byte[NetConfig.MessageLength];
                    length = networkStream.Read(buffer, 0, NetConfig.MessageLength);
                    if (length < NetConfig.MessageHeadLength)
                    {
                        continue;
                    }
                    messageQueue.Enqueue(buffer);
                }
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void DisConnect()
        {
            thread.Abort();
            tcpClient.Close();
        }

        void OnApplicationQuit()
        {
            if(thread.IsAlive)
            {
                thread.Abort();
            }
            tcpClient.Close();
        }


        void Awake()
        {
            messageQueue = new Queue<byte[]>();
            dicLogic = new Dictionary<int, Action<byte[]>>();
            tcpClient = new TcpClient();
        }

        void Update()
        {
            if (messageQueue.Count > 0)
            {
                Receive(messageQueue.Dequeue());
            }
        }

        /// <summary>
        /// 是否已连接服务器
        /// </summary>
        public bool IsConnected
        {
            get;
            private set;
        }

        /// <summary>
        /// 注册消息处理逻辑
        /// </summary>
        /// <param name="protocol">协议号</param>
        /// <param name="callback">回调函数</param>
        public void Register(Protocol protocol, Action<byte[]> callback)
        {
            Action<byte[]> action;
            if (dicLogic.TryGetValue((int)protocol, out action))
            {
                if (action == null)
                {
                    action = callback;
                    return;
                }
            }
            else
            {
                dicLogic.Add((int)protocol, callback);
                return;
            }
        }

        /// <summary>
        /// 向服务器发送消息
        /// </summary>
        /// <param name="sendPack">要发送的消息(Send)</param>
        public void Send(INetPack netPack)
        {
            Assert.AreNotEqual<Protocol>(Protocol.Invalid, netPack.Protocol);
            if(IsConnected == false)
            {
                Connect();
            }
            byte[] data = Utility.ToBytes(netPack);
            Assert.IsTrue(data.Length < NetConfig.messageBodyLength);
            byte[] sequence = BitConverter.GetBytes(Sequence);
            byte[] protocol = BitConverter.GetBytes((int)netPack.Protocol);
            byte[] head = Utility.CombineBytes(sequence, protocol);
            byte[] send = Utility.CombineBytes(head, data);
            networkStream.Write(send, 0, send.Length);
            networkStream.Flush();
        }


        /// <summary>
        /// 连接服务器
        /// </summary>
        public void Connect()
        {
            if (IsConnected)
            {
                tcpClient.Close();
            }
            IPAddress ipAddress = IPAddress.Parse(NetConfig.ServerIP);
            tcpClient.Connect(ipAddress, NetConfig.ServerPort);
            IsConnected = tcpClient.Connected;
            Assert.IsTrue(tcpClient.Connected);
            networkStream = tcpClient.GetStream();
            Sequence = 0;
            Begin();
        }



        // 消息序列号,用于保证消息同步(发送消息时取出,收到消息时赋值)
        private uint Sequence
        {
            get;
            set;
        }


        private void Receive(byte[] bytes)
        {
            byte[] sequence = new byte[NetConfig.MessageHeadSequenceLength];
            Array.Copy(bytes, sequence, NetConfig.MessageHeadSequenceLength);
            Sequence = BitConverter.ToUInt32(sequence, 0);

            byte[] protocol = new byte[NetConfig.MessageHeadProtocolLength];
            Array.Copy(bytes, NetConfig.MessageHeadSequenceLength, protocol, 0, NetConfig.MessageHeadProtocolLength);
            int protocolNum = BitConverter.ToInt32(protocol, 0);

            Action<byte[]> action;
            if (dicLogic.TryGetValue(protocolNum, out action))
            {
                if (action == null)
                {
                    Debug.LogError("Server logic is null:" + ((Protocol)protocolNum).ToString());
                    return;
                }
                int dataLength = bytes.Length - NetConfig.MessageHeadLength;
                byte[] data = new byte[dataLength];
                Array.Copy(bytes, NetConfig.MessageHeadLength, data, 0, dataLength);
                action(data);
            }
        }

#endif
    }
}
