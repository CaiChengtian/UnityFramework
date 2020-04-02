/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:打包AssetBundles相关的工具，以及导入资源时自动设置资源属性(主要是AssetBundle名称)。
*/
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace UnityFramework
{
    public class AssetBundleEditor : AssetPostprocessor
    {
        /// <summary>
        /// 打包AssetBundles
        /// </summary>
        [MenuItem("Tools/Build Asset Bundles")]
        public static void BuildAssetBundles()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            Directory.CreateDirectory(Application.streamingAssetsPath);
            BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, BuildAssetBundleOptions.ChunkBasedCompression, target);
            AssetDatabase.Refresh();
        }

        //资源根目录
        private static string ResDirectory = Utility.CombinePath("Assets", ResConfig.ResRelativeDirectory);

        /// <summary>
        /// 自动设置AssetBundle名称
        /// 更改资源时自动触发
        /// </summary>
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            /*
            for (int i = 0; i < deletedAssets.Length; ++i)
            {
                Debug.Log("Deleted asset: " + deletedAssets[i]);
            }
            */
            for (int i = 0; i < importedAssets.Length; ++i)
            {
                Debug.Log("Imported asset: " + importedAssets[i]);
                UpdateAssetBundleName(importedAssets[i]);
            }
            for (int i = 0; i < movedAssets.Length; ++i)
            {
                Debug.Log("Moved asset: " + movedAssets[i] + " ---- From: " + movedFromAssetPaths[i]);
                UpdateAssetBundleName(movedAssets[i]);
            }
            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 自动设置Texture属性
        /// 导入图片资源时自动触发,并先于OnPostprocessAllAssets触发
        /// </summary>
        /// <param name="texture"></param>
        private void OnPostprocessTexture(Texture2D texture)
        {
            TextureImporter import = assetImporter as TextureImporter;
            string relativePath = import.assetPath.Remove(0, ResDirectory.Length + 1);
            string relativeDirectory = relativePath.Remove(relativePath.LastIndexOf("/"));
            if (relativeDirectory.StartsWith("Texture"))
            {
                import.textureType = TextureImporterType.Default;
                Debug.Log("Imported Texture: " + relativePath);
            }
            else if (relativeDirectory.StartsWith("Sprite"))
            {
                import.textureType = TextureImporterType.Sprite;
                import.spritePackingTag = relativeDirectory.Replace('/', '_').ToLower();
                Debug.Log("Imported Sprite: " + relativePath);
            }
            else
            {
                import.textureType = TextureImporterType.Default;
                Debug.Log("Imported OtherTexture: " + relativePath);
            }
        }

        /// <summary>
        /// 自动设置Sprite属性
        /// 导入(包含meta文件,且TextureType为Sprite的)图片资源时自动触发,并先于OnPostprocessTexture及OnPostprocessAllAssets触发)
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="sprites"></param>
        private void OnPostprocessSprites(Texture2D texture, Sprite[] sprites)
        {
            //
        }

        /// <summary>
        /// 更新AssetBundle名称(指定路径)
        /// </summary>
        /// <param name="filePath"></param>
        private static void UpdateAssetBundleName(string filePath)
        {
            //只为ResDirectory目录下的资源设置AssetBundleName
            if (!filePath.StartsWith(ResDirectory))
            {
                return;
            }
            //只为文件夹设置AssetBundleName
            if (!Directory.Exists(filePath))
            {
                return;
            }
            //判断是否在过滤列表中
            if (IsBlocked(filePath))
            {
                ClearAssetBundleNameByPath(filePath);
                return;
            }
            SetAssetBundleNameByPath(filePath);
        }

        private static void SetAssetBundleNameByPath(string filePath)
        {
            AssetImporter importer = AssetImporter.GetAtPath(filePath);
            string assetBundleName = filePath.Remove(0, ResDirectory.Length + 1).Replace('/', '_').ToLower();
            importer.SetAssetBundleNameAndVariant(assetBundleName, String.Empty);
            Debug.Log("Set AssetBundleName: " + importer.assetBundleName);
        }

        private static void ClearAssetBundleNameByPath(string filePath)
        {
            AssetImporter importer = AssetImporter.GetAtPath(filePath);
            importer.SetAssetBundleNameAndVariant(String.Empty, String.Empty);
        }


        private static string[] blocks =
        {
            "Test"  //完整目录为Assets/Res/Test
        };
        private static List<string> blockList = new List<string>(blocks);

        /// <summary>
        /// /// </summary>
        /// 判断是否被黑名单文件夹列表过滤
        static bool IsBlocked(string filePath)
        {
            foreach (string block in blockList)
            {
                string path = Utility.CombinePath(ResDirectory, block);
                if (filePath == path || filePath.StartsWith(path + "/"))
                {
                    return true;
                }
            }
            return false;
        }

    }
}