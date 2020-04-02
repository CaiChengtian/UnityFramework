/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:定义游戏的各种事件
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityFramework
{
	/// <summary>
	/// 事件ID
	/// </summary>
	public enum EventID
	{
		Invalid = 0,

		#region System
		System_Login,
		System_Logout,
		System_AppQuit,
		#endregion
		
	}
}
