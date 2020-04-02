/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:界面的参数类,控制界面的显示方式.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFramework
{
    public class PanelData
    {

        //private bool isClearNavigation = false;
        //private PanelType type = PanelType.Normal;
        //private PanelPopupMode showMode = PanelPopupMode.DoNothing;
        //private PanelColliderMode colliderMode = PanelColliderMode.None;
        //private PanelNavigationMode navigationMode = PanelNavigationMode.Default;

        /// <summary>
        /// 界面类型
        /// </summary>
        public PanelType Type
        {
            get;
            set;
        }

        /// <summary>
        /// 显示模式
        /// (该属性仅对Popup界面有效)
        /// </summary>
        public PanelPopupMode PopupMode
        {
            get;
            set;
        }

        /// <summary>
        /// 阻挡层类型
        /// </summary>
        public PanelColliderMode ColliderMode
        {
            get;
            set;
        }

        /// <summary>
        /// 导航类型
        /// </summary>
        public PanelNavigationMode NavigationMode
        {
            get;
            set;
        }

        /// <summary>
        /// (打开该界面时)是否清除导航信息
        /// 注:通常只有主界面为true
        /// </summary>
        public bool IsClearNavigation
        {
            get;
            set;
        }

        public PanelData(PanelType type,
            PanelColliderMode colliderMode = PanelColliderMode.None,
            PanelNavigationMode navigationMode = PanelNavigationMode.Navigation,
            PanelPopupMode popupMode = PanelPopupMode.DoNothing,
            bool isClearNavigation = false)
        {
            Type = type;
            ColliderMode = colliderMode;
            NavigationMode = navigationMode;
            PopupMode = popupMode;
            IsClearNavigation = isClearNavigation;
        }
    }


    /// <summary>
    /// 界面类型
    /// </summary>
    public enum PanelType
    {
        Normal,    // 常规
        Fixed,     // 固定界面(货币栏,主角信息,小地图...)
        PopUp,     // 弹框(确认框,物品详情框...)
    }

    /// <summary>
    /// 界面的背景阻挡类型
    /// </summary>
    public enum PanelColliderMode
    {
        /// <summary>
        /// 无阻挡
        /// </summary>
        None,

        /// <summary>
        /// 透明
        /// </summary>
        Transparent,

        /// <summary>
        /// 半透明
        /// </summary>
        translucent,

        /// <summary>
        /// 图片
        /// </summary>
        Image,
    }

    /// <summary>
    /// 界面的导航模式
    /// (仅对Normal和Popup界面有效)
    /// </summary>
    public enum PanelNavigationMode
    {
        /// <summary>
        /// 导航
        /// </summary>
        Navigation,

        /// <summary>
        /// 不导航
        /// </summary>
        NoNavigation,
    }

    /// <summary>
    /// Popup界面的显示模式
    /// (打开Popup界面时,如何处理其它Popup界面)
    /// </summary>
    public enum PanelPopupMode
    {
        /// <summary>
        /// 不做任何处理
        /// </summary>
        DoNothing,

        /// <summary>
        /// 隐藏其它Popup界面
        /// </summary>
        HideOther,
    }
}
