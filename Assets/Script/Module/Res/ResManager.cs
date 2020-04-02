/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:资源管理器,负责加载资源
*/
using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace UnityFramework
{
    /// <summary>
    /// 资源加载工具
    /// </summary>
    public class ResManager : Manager<ResManager>
    {
        //常驻内存的AssetBundle字典
        private Dictionary<string, AssetBundle> dicUnloadAssetBundles;

        void Awake()
        {
            dicUnloadAssetBundles = new Dictionary<string, AssetBundle>();
        }

        /// <summary>
        /// (同步)从Resources目录加载资源
        /// </summary>
        /// <param name="path">资源路径(相对Resources目录)</param>
        public T Load<T>(string path) where T : UnityEngine.Object
        {
            return Resources.Load<T>(path);
        }

        /// <summary>
        /// (异步)从resources目录加载资源
        /// </summary>
        /// <param name="path">资源路径(相对Resources目录)</param>
        /// <param name="callback">回调</param>
        public void LoadAsync<T>(string path, Action<T> callback = null) where T : UnityEngine.Object
        {
            IEnumerator coroutine = CoLoadAsync(path, callback);
            StartCoroutine(coroutine);
        }

        private IEnumerator CoLoadAsync<T>(string path, Action<T> callback) where T : UnityEngine.Object
        {
            ResourceRequest rq = Resources.LoadAsync<T>(path);
            yield return rq;
            T t = rq.asset as T;
            if (callback != null)
            {
                callback(t);
            }
        }

        /// <summary>
        /// (同步)从AssetBundle加载资源
        /// </summary>
        /// <param name="bundleName">Bundle名称</param>
        /// <param name="assetName">资源名称(包含后缀)</param>
        public T Load<T>(string bundleName, string assetName) where T : UnityEngine.Object
        {
            T t = null;
#if UNITY_EDITOR            
            if (AppConfig.IsDevelopMode)
            {
                t = LoadLocalAsset<T>(bundleName, assetName);
                Assert.IsNotNull<T>(t);
                return t;
            }
#endif
            AssetBundle bundle;
            if (ResConfig.UnloadAssetBundleNames.Contains(bundleName))
            {
                bundle = LoadBundleFromCache(bundleName);
                t = bundle.LoadAsset<T>(assetName);
            }
            else
            {
                bundle = LoadBundle(bundleName);
                t = bundle.LoadAsset<T>(assetName);
                bundle.Unload(false);
            }
            Assert.IsNotNull<T>(t);
            return t;
        }

        /// <summary>
        /// (异步)从AssetBundle加载资源
        /// </summary>
        /// <param name="bundleName">Bundle名称</param>
        /// <param name="assetName">资源名称(包含后缀)</param>
        /// <param name="callback">回调</param>
        public void LoadAsync<T>(string bundleName, string assetName, Action<T> callback) where T : UnityEngine.Object
        {
            IEnumerator coroutine = CoLoadBundleAsync<T>(bundleName, assetName, callback);
            StartCoroutine(coroutine);
        }

        private IEnumerator CoLoadBundleAsync<T>(string bundleName, string assetName, Action<T> callback) where T : UnityEngine.Object
        {
            T t = null;
#if UNITY_EDITOR
            if (AppConfig.IsDevelopMode)
            {
                t = LoadLocalAsset<T>(bundleName, assetName);
                Assert.IsNotNull<T>(t);
                Assert.IsNotNull<Action<T>>(callback);
                callback(t);
                yield return t;
            }
#endif
            AssetBundle bundle;
            if (ResConfig.UnloadAssetBundleNames.Contains(bundleName))
            {
                bundle = LoadBundleFromCache(bundleName);
                t = bundle.LoadAsset<T>(assetName);
            }
            else
            {
                bundle = LoadBundle(bundleName);
                t = bundle.LoadAsset<T>(assetName);
                bundle.Unload(false);
            }
            Assert.IsNotNull<T>(t);
            Assert.IsNotNull<Action<T>>(callback);
            callback(t);
            yield return t;
        }

        /// <summary>
        /// 加载AssetBundle
        /// </summary>
        private AssetBundle LoadBundle(string bundleName)
        {
            string fullName = Utility.CombinePath(ResConfig.AssetBundleDirectory, bundleName);
            AssetBundle bundle = AssetBundle.LoadFromFile(fullName);
            Assert.IsNotNull<AssetBundle>(bundle);
            return bundle;
        }

        /// <summary>
        /// 加载常驻内存的AssetBundle
        /// </summary>
        private AssetBundle LoadBundleFromCache(string bundleName)
        {
            AssetBundle bundle = null;
            if (dicUnloadAssetBundles.TryGetValue(bundleName, out bundle))
            {
                //内存中已有
                Assert.IsNotNull<AssetBundle>(bundle);
                return bundle;
            }
            else
            {
                //第一次加载
                string fullName = Utility.CombinePath(ResConfig.AssetBundleDirectory, bundleName);
                bundle = AssetBundle.LoadFromFile(fullName);
                Assert.IsNotNull<AssetBundle>(bundle);
                dicUnloadAssetBundles.Add(bundleName, bundle);
                return bundle;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// 直接从资源目录加载资源
        /// (仅用于开发模式)
        /// </summary>
        private T LoadLocalAsset<T>(string bundleName, string assetName) where T : UnityEngine.Object
        {
            string name = Utility.CombinePath(bundleName.Replace("_", "/"), assetName);
            string path = "Assets/" + ResConfig.ResRelativeDirectory;
            string fullName = Utility.CombinePath(path, name);
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(fullName);
        }
#endif

    }
}
