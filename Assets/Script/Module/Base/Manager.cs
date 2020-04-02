/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:管理器基类,所有管理器类都必须继承它.
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityFramework
{
    /// <summary>
    /// 管理器基类
    /// (已实现单例模式)
    /// </summary>
    public abstract class Manager<T> : MonoBehaviour where T : Manager<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Game.CoreObject.GetComponent<T>();
                    if (instance == null)
                    {
                        instance = Game.CoreObject.AddComponent<T>();
                    }
                }
                return instance;
            }
        }
    }
}
