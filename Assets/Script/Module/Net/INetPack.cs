/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:网络数据包接口
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityFramework
{
    public interface INetPack
    {
        /// <summary>
        /// 协议号
        /// </summary>
        Protocol Protocol
        {
            get;
        }
    }
}
