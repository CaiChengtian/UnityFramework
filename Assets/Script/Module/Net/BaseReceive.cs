/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:
*/
using UnityEngine;
using System;
using System.Collections;
using System.Text;

namespace UnityFramework
{
    /// <summary>
    /// 接收服务器返回的Json数据，执行对应的客户端逻辑
    /// </summary>
    [Serializable]
    public abstract class BaseReceive : INetPack
    {
        public abstract Protocol Protocol
        {
            get;
        }
    }
}