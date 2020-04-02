/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:
*/
using UnityEngine;
using System.Collections;

namespace UnityFramework
{
    /// <summary>
    /// 客户端发送数据的基类
    /// </summary>
    public abstract class BaseSend : INetPack
    {
        public abstract Protocol Protocol
        {
            get;
        }
    }
}
