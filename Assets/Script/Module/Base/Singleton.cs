/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:单例基类
*/
using System;
using System.Reflection;

namespace UnityFramework
{
    /// <summary>
    /// 单例基类
    /// </summary>
    public abstract class Singleton<T> where T : Singleton<T>
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    ConstructorInfo[] ctors = typeof(T).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
                    ConstructorInfo ctor = Array.Find(ctors, c => c.GetParameters().Length == 0);
                    UnityEngine.Assertions.Assert.IsNotNull<ConstructorInfo>(ctor, "Cannot instantiate singleton class." + typeof(T).Name);
                    instance = ctor.Invoke(null) as T;
                }
                return instance;
            }
        }

        protected Singleton()
        {

        }
    }
}
