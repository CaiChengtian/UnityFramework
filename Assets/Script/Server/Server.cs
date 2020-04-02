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
    /// (单例)模拟服务器，处理客户端的请求
    /// </summary>
    public class Server : MonoSingleton<Server>
    {
#if SIM
        //调试模式

        /// <summary>
        /// 执行协议(仅供ServerLogic注册)
        /// </summary>
        public Action<int, byte[]> process;
        //发送消息的委托,用于替代Socket
        public Action<byte[]> netLogic;
        //最大延时(秒)
        private float networkDelay = 0.2f;

        /// <summary>
        /// 消息序列号,用于保证消息同步
        /// (发送消息时取出,收到消息时赋值)
        /// </summary>
        private uint Sequence
        {
            get;
            set;
        }

        /// <summary>
        /// 服务器是否开启
        /// </summary>
        public bool IsAlive
        {
            get;
            private set;
        }

        void Awake()
        {
            instance = this;
            NetManager.Instance.netLogic += DelayReceive;
        }

        /// <summary>
        /// 关闭服务器
        /// </summary>
        public void StopServer()
        {
            IsAlive = false;
            ServerLogic.Instance.SaveAll();
        }

        /// <summary>
        /// 开启服务器
        /// </summary>
        public void StartServer()
        {
            IsAlive = true;
        }

        /// <summary>
        /// 发送消息到客户端
        /// </summary>
        /// <param name="protocol">协议号</param>
        public void Send(INetPack netPack)
        {
            Assert.AreNotEqual<Protocol>(Protocol.Invalid, netPack.Protocol);
            if (!IsAlive)
            {
                return;
            }
            byte[] data = Utility.ToBytes(netPack);
            Assert.IsTrue(data.Length < NetConfig.messageBodyLength);
            byte[] sequence = BitConverter.GetBytes(++Sequence);
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
        private IEnumerator Receive(byte[] bytes)
        {
            yield return new WaitForSeconds(networkDelay);
            byte[] sequence = new byte[NetConfig.MessageHeadSequenceLength];
            Array.Copy(bytes, sequence, NetConfig.MessageHeadSequenceLength);
            //过时的消息不予处理
            if (Sequence == BitConverter.ToUInt32(sequence, 0))
            {
                byte[] protocol = new byte[NetConfig.MessageHeadProtocolLength];
                Array.Copy(bytes, NetConfig.MessageHeadSequenceLength, protocol, 0, NetConfig.MessageHeadProtocolLength);
                int protocolNum = BitConverter.ToInt32(protocol, 0);

                int dataLength = bytes.Length - NetConfig.MessageHeadLength;
                byte[] data = new byte[dataLength];
                Array.Copy(bytes, NetConfig.MessageHeadLength, data, 0, dataLength);
                //处理
                Debug.LogWarning("Server Receive:" + ((Protocol)protocolNum).ToString());
                process(protocolNum, data);
            }
        }
#else
        //正常模式

        /// <summary>
        /// 执行协议(仅供ServerLogic注册)
        /// </summary>
        public Action<int, byte[]> process;

        //协议序列号,避免重复处理协议
        private uint sequence;
        private TcpListener server;
        private TcpClient client;
        private NetworkStream networkStream;
        private Queue<byte[]> messageQueue;

        private Thread thread;

        private void Begin()
        {
            thread = new Thread(ServerThread);
            thread.Start();
        }

        private void ServerThread()
        {
            client = server.AcceptTcpClient();
            networkStream = client.GetStream();
            sequence = 0;
            int length;
            while (true)
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

        /// <summary>
        /// 关闭服务器
        /// </summary>
        public void Stop()
        {
            if (thread.IsAlive)
            {
                thread.Abort();
            }
            client.Close();
            server.Stop();
            ServerLogic.Instance.SaveAll();
        }


        /// <summary>
        /// 消息序列号,用于保证消息同步
        /// (发送消息时取出,收到消息时赋值)
        /// </summary>
        private uint Sequence
        {
            get;
            set;
        }

        void Awake()
        {
            instance = this;
            messageQueue = new Queue<byte[]>();
            IPAddress ip = IPAddress.Parse(NetConfig.ServerIP);
            server = new TcpListener(ip, NetConfig.ServerPort);
            server.Start();
            Begin();
        }

        void Update()
        {
            if (messageQueue.Count > 0)
            {
                Receive(messageQueue.Dequeue());
            }
        }

        /// <summary>
        /// 发送消息到客户端
        /// </summary>
        /// <param name="protocol">协议号</param>
        public void Send(INetPack pack)
        {
            Assert.AreNotEqual<Protocol>(Protocol.Invalid, pack.Protocol);
            byte[] data = Utility.ToBytes(pack);
            Assert.IsTrue(data.Length < NetConfig.messageBodyLength);
            byte[] sequence = BitConverter.GetBytes(Sequence);
            byte[] protocol = BitConverter.GetBytes((int)pack.Protocol);
            byte[] head = Utility.CombineBytes(sequence, protocol);
            byte[] send = Utility.CombineBytes(head, data);
            networkStream.Write(send, 0, send.Length);
            networkStream.Flush();
        }

        private void Receive(byte[] bytes)
        {
            byte[] sequence = new byte[NetConfig.MessageHeadSequenceLength];
            Array.Copy(bytes, sequence, NetConfig.MessageHeadSequenceLength);
            //过时的消息不予处理
            if (Sequence == BitConverter.ToUInt32(sequence, 0))
            {
                byte[] protocol = new byte[NetConfig.MessageHeadProtocolLength];
                Array.Copy(bytes, NetConfig.MessageHeadSequenceLength, protocol, 0, NetConfig.MessageHeadProtocolLength);
                int protocolNum = BitConverter.ToInt32(protocol, 0);

                int dataLength = bytes.Length - NetConfig.MessageHeadLength;
                byte[] data = new byte[dataLength];
                Array.Copy(bytes, NetConfig.MessageHeadLength, data, 0, dataLength);
                //处理
                //ServerLogic.Instance.Process(protocol, data);
                process(protocolNum, data);
            }
        }

#endif
    }
}