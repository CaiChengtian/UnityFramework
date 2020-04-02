/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:应用程序配置类,用来控制游戏的调试和打包.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFramework
{
	/// <summary>
	/// 应用程序配置
	/// </summary>
	public static class AppConfig
	{
		/// <summary>
		/// 是否显示日志
		/// </summary>
        public static readonly bool IsLog = true;

		/// <summary>
		/// 显示谁写的日志
		/// </summary>
        public static readonly LogNames LogName = LogNames.TT;

		/// <summary>
		/// 是否为开发模式
		/// true:优先加载本地资源(Editor平台有效)
		/// false:从AssetBundle加载资源
		/// /// </summary>
		public static readonly bool IsDevelopMode = true;
		
		/// <summary>
		/// 是否连接到外网(内网开发,外网发布)
		/// </summary>
		public static readonly bool IsOutNet = false;


    }
}
