/*
	Copyright (c) 20x17 Tiantian. All rights reserved.
	Description:模拟服务器处理游戏逻辑
*/
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using System.Reflection;
using System.IO;
using System.Text;

namespace UnityFramework
{
    /// <summary>
    /// 服务器逻辑类
    /// (该类仅供Server类调用)
    /// </summary>
    public class ServerLogic : MonoSingleton<ServerLogic>
    {
        //服务器数据表的目录
        private string serverDataPath;
        //服务器数据表AssetBundle名称
        private string bundleName = "serverdata";
        //服务器数据表后缀名
        private string fileSuffix = ".csv";
        //Table类的结尾词
        private string tableEnd = "Table";
        //索引服务器数据表的字典
        private Dictionary<Type, Dictionary<int, BaseTable>> dicTables;
        //协议号和委托的映射字典，收到客户端的消息后，根据协议号调用相应的方法
        private Dictionary<int, Action<byte[]>> dicLogic;


        void Awake()
        {
            instance = this;
            dicLogic = new Dictionary<int, Action<byte[]>>();
            dicTables = new Dictionary<Type, Dictionary<int, BaseTable>>();
            Server.Instance.process += Process;
            serverDataPath = Utility.CombinePath(Application.persistentDataPath, "ServerData");
            RegisterAllLogic();
        }

        /// <summary>
        /// 保存所有服务器数据
        /// </summary>
        public void SaveAll()
        {
            foreach (Type type in dicTables.Keys)
            {
                MethodInfo mi = this.GetType().GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(type);
                mi.Invoke(this, null);
            }
        }

        //保存数据表到csv文件
        private void Save<T>() where T : BaseTable
        {
            string t = typeof(T).Name;
            Dictionary<int, BaseTable> dic = new Dictionary<int, BaseTable>();
            UnityEngine.Assertions.Assert.IsTrue((dicTables.TryGetValue(typeof(T), out dic)));
            string[] datas = new string[dic.Count + 1];
            FieldInfo[] fields = typeof(T).GetFields();
            StringBuilder line = new StringBuilder();

            //首行为字段
            int i = 0;
            int row = 0;
            int length = fields.Length;
            //最后一列是ID,放到第一列
            line.Append(fields[length - 1].Name);
            line.Append(',');
            //中间列
            for (i = 0; i < length - 2; i++)
            {
                line.Append(fields[i].Name);
                line.Append(',');
            }
            //倒数第二列成为最后一列,单独加回车符
            line.Append(fields[i].Name);
            line.Append("\r\n");
            datas[row++] = line.ToString();

            //第二行开始为数据
            foreach (BaseTable item in dic.Values)
            {
                T table = item as T;
                line.Remove(0, line.Length);
                line.Append(fields[length - 1].GetValue(table).ToString());
                line.Append(',');
                for (i = 0; i < length - 2; i++)
                {
                    line.Append(fields[i].GetValue(table).ToString());
                    line.Append(',');
                }
                line.Append(fields[i].GetValue(table).ToString());
                line.Append("\r\n");
                datas[row++] = line.ToString();
            }
            //写入文件
            Directory.CreateDirectory(serverDataPath);
            string fileName = typeof(T).Name.Replace(tableEnd, fileSuffix);
            string fullName = Utility.CombinePath(serverDataPath, fileName);
            FileStream fs = new FileStream(fullName, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            for (i = 0; i < datas.Length; i++)
            {
                sw.Write(datas[i]);
            }
            sw.Flush();
            sw.Close();
            fs.Close();
        }

        /// <summary>
        /// 处理服务器收到的消息
        /// </summary>
        private void Process(int protocol, byte[] data)
        {
            Action<byte[]> action;
            if (dicLogic.TryGetValue(protocol, out action))
            {
                Assert.IsNotNull<Action<byte[]>>(action);
                action(data);
            }
        }

        //修改数据
        private void Modify<T>(T data) where T : BaseTable, new()
        {
            T t = Find<T>(data.ID);
            Assert.IsNotNull<T>(t);
            dicTables[typeof(T)][data.ID] = data;
        }

        /// <summary>
        /// 增加数据
        /// </summary>
        private void Add<T>(T data) where T : BaseTable, new()
        {
            T t = Find<T>(data.ID);
            Assert.IsNull<T>(t);
            dicTables[typeof(T)].Add(data.ID, data);
        }

        /// <summary>
        /// 查找数据
        /// </summary>
        private T Find<T>(int id) where T : BaseTable, new()
        {
            Dictionary<int, BaseTable> dic;
            if (!dicTables.TryGetValue(typeof(T), out dic))
            {
                dic = LoadCsvData<T>();
                Assert.IsNotNull<Dictionary<int, BaseTable>>(dic);
                dicTables.Add(typeof(T), dic);
            }

            BaseTable table;
            if (dic.TryGetValue(id, out table))
            {
                Assert.IsNotNull<BaseTable>(table);
                return table as T;
            }
            else
            {
                return null;
            }
        }

        //加载Table数据
        private Dictionary<int, BaseTable> LoadCsvData<T>() where T : BaseTable, new()
        {
            string fileName = typeof(T).Name.Replace(tableEnd, fileSuffix);
            Dictionary<int, Dictionary<string, string>> result = LoadCsvFile(fileName);
            Assert.IsNotNull<Dictionary<int, Dictionary<string, string>>>(result);
            Dictionary<int, BaseTable> dic = new Dictionary<int, BaseTable>();
            foreach (Dictionary<string, string> item in result.Values)
            {
                FieldInfo[] props = typeof(T).GetFields();
                T obj = Activator.CreateInstance<T>();
                foreach (FieldInfo fi in props)
                {
                    object value = Convert.ChangeType(item[fi.Name], fi.FieldType);
                    fi.SetValue(obj, value);
                }
                dic.Add(obj.ID, obj as BaseTable);
            }
            return dic;
        }

        //加载csv文件
        private Dictionary<int, Dictionary<string, string>> LoadCsvFile(string fileName)
        {
            string text = null;
            string path = Utility.CombinePath(serverDataPath, fileName);
            if (File.Exists(path))
            {
                FileStream fs = new FileStream(path, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                text = sr.ReadToEnd();
                sr.Close();
                fs.Close();
            }
            else
            {
                TextAsset textAsset = ResManager.Instance.Load<TextAsset>(bundleName, fileName);
                text = textAsset.text;
            }
            Assert.IsNotNull<string>(text);
            Dictionary<int, Dictionary<string, string>> result = new Dictionary<int, Dictionary<string, string>>();
            //CSV文件的第一行为Key字段,第二行开始是数据;第一列一定是ID
            string[] fileData = text.Split('\n');
            string[] keys = fileData[0].Split(',');
            for (int i = 1; i < fileData.Length; i++)
            {
                string[] line = fileData[i].Split(',');
                if (String.IsNullOrEmpty(line[0]))
                {
                    continue;
                }
                int ID = int.Parse(line[0]);
                result.Add(ID, new Dictionary<string, string>());
                int j;
                for (j = 0; j < line.Length - 1; j++)
                {
                    //每一行的数据存储规则:Key字段-Value值
                    result[ID].Add(keys[j], line[j]);
                }
                //csv文件的换行其实是\r\n,所以最后一个字段会多出一个\r
                string key = keys[j].Replace("\r", "");
                string value = line[j].Replace("\r", "");
                result[ID].Add(key, value);
            }
            return result;
        }

        private void Register(Protocol protocol, Action<byte[]> logic)
        {
            dicLogic.Add((int)protocol, logic);
        }


        //所有服务器逻辑都要在这里注册
        private void RegisterAllLogic()
        {
            //System
            Register(Protocol.System_Login, System_Login);

        }


        #region System Logic
        private void System_Login(byte[] bytes)
        {
            LoginSend loginSend = Utility.FromBytes<LoginSend>(bytes);
            Debug.Log(loginSend.Account);
            Debug.Log(loginSend.Password);
            LoginReceive loginReceive = new LoginReceive();
            loginReceive.IsSuccess = true;
            loginReceive.Name = "cct";
            loginReceive.Level = 7;
            Server.Instance.Send(loginReceive);
        }

        #endregion
    }
}
