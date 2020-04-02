/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:资源配置
*/
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace UnityFramework
{
    /// <summary>
    /// 资源配置
    /// </summary>
    public static class ResConfig
    {
        /// <summary>
        /// 资源目录(绝对路径)
        /// </summary>
        public static readonly string ResDirectory = Application.dataPath + "/Res";

        /// <summary>
        /// 资源目录(相对Application.dataPath)
        /// </summary>
        public static readonly string ResRelativeDirectory = "Res";

        /// <summary>
        /// AssetBundle目录(绝对路径)
        /// </summary>
        public static readonly string AssetBundleDirectory = Application.streamingAssetsPath;

        /// <summary>
        /// Csv配置表目录(相对资源目录ResConfig.ResDirectory)
        /// 同时也是配置表的AssetBundle名称
        /// </summary>
        public static readonly string TableDirectory = "table";

        /// <summary>
        /// 不卸载的Assetbundle列表
        /// (例如经常需要动态加载的图标,头像)
        /// </summary>
        public static readonly List<string> UnloadAssetBundleNames = new List<string>
        {
			"ui_frame",
			"ui_avatar",
			"ui_item",
			"ui_skill",
			"ui_portrait",
        };
    }
}
