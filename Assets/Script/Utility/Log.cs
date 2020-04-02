/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:日志类,每个开发人员拥有自己显示日志的方法,也只会看到自己的日志.
    该类所有方法都声明为[Conditional("DEBUG")],仅在开发模式有效,正式发布时编译器会自动忽略对这些方法的调用.
    注:DEBUG为C#保留的预定义指令,无需手动添加.
    另外,尽可能使用断言(UnityEngine.Assertions.Assert)来替代错误日志(Debug.LogError).
    因为断言在发布时会被编译器忽略,并且提供了更明确易懂的错误提示.
*/
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnityFramework
{
    /// <summary>
    /// 开发人员列表
    /// </summary>
    public enum LogNames
    {
        other = 0,
        TT,
    }

    /// <summary>
    /// 日志类
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// Tiantian
        /// </summary>
        [Conditional("DEBUG")]
        public static void TT(string str)
        {
            if (AppConfig.IsLog && AppConfig.LogName == LogNames.TT)
            {
				UnityEngine.Debug.Log(str);
            }
        }
        /// <summary>
        /// 警告日志
        /// </summary>
        [Conditional("DEBUG")]
        public static void Warn(string str)
        {
            if (AppConfig.IsLog)
            {
				UnityEngine.Debug.LogWarning(str);
            }
        }
        /// <summary>
        /// 错误日志
        /// (建议使用UnityEngine.Assertions.Assert类进行变量的状态检查)
        /// </summary>
        /// <param name="str"></param>
        [Conditional("DEBUG")]
        public static void Error(string str)
        {
            if (AppConfig.IsLog)
            {
				UnityEngine.Debug.LogError(str);
            }
        }
    }
}