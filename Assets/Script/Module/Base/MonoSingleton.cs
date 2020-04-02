/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:继承了MonoBehaviour的单例基类
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityFramework
{
    /// <summary>
    /// 单例基类(继承了MonoBehaviour)
    /// </summary>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        protected static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject(typeof(T).ToString());
                    instance = go.AddComponent<T>();
                }
                return instance;
            }
        }
    }
}
