/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:数据处理工具类
*/
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UnityFramework
{
    public static class Utility
    {
        /// <summary>
        /// 合并字节数组
        /// </summary>
        /// <param name="bytes1">数组1(合并后位于前面)</param>
        /// <param name="bytes2">数组2</param>
        /// <returns>合并后的字节数组</returns>
        public static byte[] CombineBytes(byte[] bytes1, byte[] bytes2)
        {
            byte[] newBytes = new byte[bytes1.Length + bytes2.Length];
            Array.Copy(bytes1, newBytes, bytes1.Length);
            Array.Copy(bytes2, 0, newBytes, bytes1.Length, bytes2.Length);
            return newBytes;
        }

        /// <summary>
        /// 合并路径
        /// </summary>
        /// <param name="path1">路径1</param>
        /// <param name="path2">路径2</param>
        /// <param name="split">分割符(默认/)</param>
        /// <returns></returns>
        public static string CombinePath(string path1, string path2, string split = "/")
        {
            if (String.IsNullOrEmpty(path1) || String.IsNullOrEmpty(path2))
            {
                return path1 + path2;
            }

            if (path1.EndsWith(split))
            {
                if (path2.StartsWith(split))
                {
                    return path1 + path2.Remove(0, 1);
                }
            }
            else
            {
                if (!path2.StartsWith(split))
                {
                    return path1 + split + path2;
                }
            }
            return path1 + path2;
        }

        /// <summary>
        /// 协议数据转成字节流
        /// </summary>
        /// <param name="pack"></param>
        /// <returns></returns>
        public static byte[] ToBytes(INetPack pack)
        {
            string json = JsonUtility.ToJson(pack);
            return Encoding.UTF8.GetBytes(json);
        }

        /// <summary>
        /// 字节流解析成协议数据
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static T FromBytes<T>(byte[] bytes) where T : INetPack
        {
            string json = Encoding.UTF8.GetString(bytes);
            return JsonUtility.FromJson<T>(json);
        }
    }
}
