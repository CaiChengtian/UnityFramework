/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:网络配置类
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityFramework
{
	/// <summary>
	/// 网络配置
	/// </summary>
    public static class NetConfig
    {
		/// <summary>
		/// 服务器IP
		/// </summary>
        public static readonly string ServerIP = "127.0.0.1";
		
		/// <summary>
		/// 服务器端口号
		/// </summary>
        public static readonly int ServerPort = 5000;

		/// <summary>
		/// 消息的最大长度
		/// </summary>
		public static readonly int MessageLength = 1024;

		/// <summary>
		/// 消息头的序列号长度(字节数)
		/// </summary>
		public static readonly int MessageHeadSequenceLength = 4;

		/// <summary>
		/// 消息头的协议号长度(字节数)
		/// </summary>
		public static readonly int MessageHeadProtocolLength = 4;

		/// <summary>
		/// 消息头(序列号 + 协议号)的长度(字节数)
		/// </summary>
		public static readonly int MessageHeadLength = MessageHeadSequenceLength + MessageHeadProtocolLength;

		/// <summary>
		/// 消息体的长度(字节数)
		/// </summary>
		public static readonly int messageBodyLength = MessageLength - MessageHeadSequenceLength - MessageHeadProtocolLength;
    }
}
