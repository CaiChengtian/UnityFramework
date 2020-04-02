/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:UI界面配置类,保存界面ID及预制件路径.
    新增的UI界面要执行以下步骤:
        1.添加PanelID
        2.(如果有的话)初始化Controller
*/
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace UnityFramework
{
    /// <summary>
    /// 界面ID
    /// </summary>
    public enum PanelID
    {
        /// <summary>
        /// 非法
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// 登陆
        /// </summary>
        Login,

        /// <summary>
        /// 主界面
        /// </summary>
        Main,

    }

    /// <summary>
    /// 界面配置
    /// </summary>
    public static class UIConfig
    {
        /// <summary>
        /// 初始化所有的Controller
        /// </summary>
        public static void InitControllers()
        {
            LoginController.Instance.Init();
        }

        /// <summary>
        /// UI界面的默认背景图
        /// </summary>
        public static readonly Sprite ColliderSprite = ResManager.Instance.Load<Sprite>("Texture/BG/Main/bg_panel");

        /// <summary>
        /// 界面预制件的后缀名称
        /// [PanelID.ToString() + Suffix]就是完整资源名
        /// </summary>
        public static readonly string Suffix = "Panel.prefab";

        /// <summary>
        /// 界面预制件的Bundle名称
        /// (将其中的_替换为/就是实际资源目录)
        /// </summary>
        public static readonly Dictionary<PanelID, string> BundleName
         = new Dictionary<PanelID, string>(new PanelIDComparer())
        {
            {PanelID.Main, "panel_main" },
            {PanelID.Login,"panel_login"}
        };

        /// <summary>
        /// PanelID比较器,用于解决使用枚举作为字典主键的性能问题
        /// </summary>
        private class PanelIDComparer : IEqualityComparer<PanelID>
        {
            bool IEqualityComparer<PanelID>.Equals(PanelID x, PanelID y)
            {
                if (x == y)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            int IEqualityComparer<PanelID>.GetHashCode(PanelID panelID)
            {
                return (int)panelID;
            }
        }
    }
}