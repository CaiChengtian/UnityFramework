/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:上下文基类,用于关闭界面时保存界面内容,以及打开界面时加载内容(同时用作打开界面的参数).
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityFramework
{
    /// <summary>
    /// 上下文基类(UI界面的内容)
    /// </summary>
    public abstract class Context
    {
        //private PanelID panelID;
        //private Stack<Context> popupContexts;

        /// <summary>
        /// 界面ID
        /// </summary>
        public PanelID PanelID
        {
            get;
            protected set;
        }

        /// <summary>
        /// Popup导航栈
        /// (该属性仅对Normal界面有效)
        /// </summary>
        public Stack<Context> PopupNavigation
        {
            get;
            protected set;
        }
    }
}