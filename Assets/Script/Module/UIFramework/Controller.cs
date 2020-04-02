/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:各个模块的控制器基类,新增的控制器需要在UIConfig中注册.
	UI界面的显示依赖于控制器,但控制器不能直接调用UI界面的方法,应该通过事件中心来完成.
	控制器只需要包含以下功能:
		1.存储数据,并提供获取数据的方法/属性
		2.处理服务器消息并更新数据
		3.收到消息或数据更新时,触发相应的事件
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityFramework
{
    /// <summary>
    /// 控制器基类
    /// 新增的控制器需要在UIConfig中注册
    /// </summary>
    public abstract class Controller<T> : Singleton<T> where T : Controller<T>
    {
        /// <summary>
        /// 初始化
        /// (注册服务器消息的处理逻辑)
        /// </summary>
        public abstract void Init();
    }
}
