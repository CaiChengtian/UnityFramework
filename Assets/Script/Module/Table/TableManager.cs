/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:表格管理器,加载CSV配置文件
*/
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace UnityFramework
{
    /// <summary>
    /// 表格管理器
    /// </summary>
    public class TableManager : Manager<TableManager>
    {
        //存储所有的单主键表格数据
        private Dictionary<Type, Dictionary<int, BaseTable>> dicTables;

        //存储所有的双主键表格数据
        private Dictionary<Type, Dictionary<long, BaseTable>> dicDoubleTables;

        //以数组形式存储的表格数据,用于按索引(行号)获取数据
        private Dictionary<Type, BaseTable[]> dicRowTables;

        //双主键数据表,双主键合成一个Key所需要移动的位数
        private int bitNum = 32;

        void Awake()
        {
            dicTables = new Dictionary<Type, Dictionary<int, BaseTable>>();
            dicDoubleTables = new Dictionary<Type, Dictionary<long, BaseTable>>();
            dicRowTables = new Dictionary<Type, BaseTable[]>();
        }


        /// <summary>
        /// 查找Table数据
        /// </summary>
        public T Find<T>(int id) where T : BaseTable, new()
        {
            Dictionary<int, BaseTable> dic;
            if (!dicTables.TryGetValue(typeof(T), out dic))
            {
                dic = LoadCsvData<T>();

                dicTables.Add(typeof(T), dic);
            }

            BaseTable table;
            if (dic.TryGetValue(id, out table))
            {
                return table as T;
            }
            else
            {
                Log.Error(String.Format("Cannot find table data : {0} - {1}", typeof(T).ToString(), id));
                return default(T);
            }
        }

        /// <summary>
        /// 查找Table数据(双主键)
        /// </summary>
        public T Find<T>(int id, int index) where T : BaseTable, new()
        {
            Dictionary<long, BaseTable> dic;
            if (!dicDoubleTables.TryGetValue(typeof(T), out dic))
            {
                dic = LoadDoubleCsvData<T>();
                dicDoubleTables.Add(typeof(T), dic);
            }

            BaseTable table;
            long longID = id;
            long fullID = (longID << bitNum) + index;
            if (dic.TryGetValue(fullID, out table))
            {
                return table as T;
            }
            else
            {
                Log.Error(String.Format("Cannot find table data : {0} - {1} - {2}", typeof(T).ToString(), id, index));
                return default(T);
            }
        }

        /// <summary>
        /// 获取Table(数组形式)
        /// </summary>
        public T[] Table<T>() where T : BaseTable, new()
        {
            BaseTable[] array;
            if (!dicRowTables.TryGetValue(typeof(T), out array))
            {
                array = LoadCsvRowData<T>();
                dicRowTables.Add(typeof(T), array);
            }

            return array as T[];
        }

        private string GetFileName<T>() where T : BaseTable
        {
            return typeof(T).Name.Replace("Table", "") + ".csv";
        }

        /// <summary>
        /// 获取配置表数据
        /// </summary>
        private Dictionary<int, BaseTable> LoadCsvData<T>() where T : BaseTable, new()
        {
            /* 从CSV文件读取数据 */
            string fileName = GetFileName<T>();
            Dictionary<int, Dictionary<string, string>> result = LoadCsvFile(fileName);
            if (result == null)
            {
                return null;
            }

            /* 遍历每一行数据 */
            Dictionary<int, BaseTable> dic = new Dictionary<int, BaseTable>();
            foreach (int ID in result.Keys)
            {
                //CSV的一行数据
                Dictionary<string, string> datas = result[ID];
                //读取Csv数据对象的字段
                FieldInfo[] fields = typeof(T).GetFields();
                //使用反射，将CSV文件的数据赋值给CSV数据对象的相应字段，要求CSV文件的字段名和CSV数据对象的字段名完全相同
                T obj = Activator.CreateInstance<T>();
                foreach (FieldInfo fi in fields)
                {
                    object value = Convert.ChangeType(datas[fi.Name], fi.FieldType);
                    fi.SetValue(obj, value);
                }
                //按ID-数据的形式存储
                dic.Add(Convert.ToInt32(ID), obj as BaseTable);
            }
            return dic;
        }

        /// <summary>
        /// 获取配置表数据(双主键)
        /// </summary>
        /// <returns></returns>
        private Dictionary<long, BaseTable> LoadDoubleCsvData<T>() where T : BaseTable, new()
        {
            /* 从CSV文件读取数据 */
            string fileName = GetFileName<T>();
            Dictionary<long, Dictionary<string, string>> result = LoadDoubleCsvFile(fileName);
            if (result == null)
            {
                return null;
            }

            /* 遍历每一行数据 */
            Dictionary<long, BaseTable> dic = new Dictionary<long, BaseTable>();
            foreach (long ID in result.Keys)
            {
                /* CSV的一行数据 */
                Dictionary<string, string> datas = result[ID];
                /* 读取Csv数据对象的属性 */
                FieldInfo[] fields = typeof(T).GetFields();
                /* 使用反射，将CSV文件的数据赋值给CSV数据对象的相应字段，要求CSV文件的字段名和CSV数据对象的字段名完全相同 */
                T obj = Activator.CreateInstance<T>();
                foreach (FieldInfo fi in fields)
                {
                    object value = Convert.ChangeType(datas[fi.Name], fi.FieldType);
                    fi.SetValue(obj, value);
                }
                /* 按ID-数据的形式存储 */
                dic.Add(Convert.ToInt64(ID), obj as BaseTable);
            }
            return dic;
        }

        /// <summary>
        /// 加载Csv文件
        /// </summary>
        private Dictionary<int, Dictionary<string, string>> LoadCsvFile(string fileName)
        {
            TextAsset textAsset = ResManager.Instance.Load<TextAsset>(ResConfig.TableDirectory, fileName);
            if (textAsset == null)
            {
                Log.Error(String.Format("Failed to load csv file - {0} : Cannot find the file.", fileName));
                return null;
            }
            //校验主键:CSV文件的第一行为Key字段,第一列是ID
            string[] fileData = textAsset.text.Split('\n');
            string[] keys = fileData[0].Split(',');
            if (keys.Length < 1)
            {
                Log.Error(String.Format("Failed to load csv file - {0} : The Number of fields is fewer than 1.", fileName));
                return null;
            }
            //读取数据
            Dictionary<int, Dictionary<string, string>> result = new Dictionary<int, Dictionary<string, string>>();
            for (int i = 1; i < fileData.Length; i++)
            {
                string[] line = fileData[i].Split(',');
                if (String.IsNullOrEmpty(line[0]))
                {
                    //删除空行
                    continue;
                }
                int ID = int.Parse(line[0]);
                result[ID] = new Dictionary<string, string>();
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

        /// <summary>
        /// 加载Csv文件(双主键)
        /// </summary>
        private Dictionary<long, Dictionary<string, string>> LoadDoubleCsvFile(string fileName)
        {
            TextAsset textAsset = ResManager.Instance.Load<TextAsset>(ResConfig.TableDirectory, fileName);
            if (textAsset == null)
            {
                Log.Error(String.Format("Failed to load csv file - {0} : Cannot find the file.", fileName));
                return null;
            }
            //校验双主键
            string[] fileData = textAsset.text.Split('\n');
            string[] keys = fileData[0].Split(',');
            if (keys.Length < 2)
            {
                Log.Error(String.Format("Failed to load csv file - {0} : The Number of fields is fewer than 2.", fileName));
                return null;
            }
            //读取数据
            Dictionary<long, Dictionary<string, string>> result = new Dictionary<long, Dictionary<string, string>>();
            for (int i = 1; i < fileData.Length; i++)
            {
                string[] line = fileData[i].Split(',');
                if (String.IsNullOrEmpty(line[0]) || String.IsNullOrEmpty(line[1]))
                {
                    continue;
                }
                //双主键拼合成一个Key
                long ID1 = long.Parse(line[0]);
                long ID2 = long.Parse(line[1]);
                long ID = (ID1 << bitNum) + ID2;
                result[ID] = new Dictionary<string, string>();
                int j;
                for (j = 0; j < line.Length - 1; j++)
                {
                    result[ID].Add(keys[j], line[j]);
                }
                //csv文件的换行其实是\r\n,所以最后一个字段会多出一个\r
                string key = keys[j].Replace("\r", "");
                string value = line[j].Replace("\r", "");
                result[ID].Add(key, value);
            }
            return result;
        }


        private BaseTable[] LoadCsvRowData<T>() where T : BaseTable, new()
        {
            /* 从CSV文件读取数据 */
            string fileName = typeof(T).Name;
            Dictionary<string, string>[] result = LoadCsvRowFile(fileName);
            if (result == null || result.Length <= 1)
            {
                return null;
            }

            BaseTable[] array = new BaseTable[result.Length - 1];
            /* 遍历每一行数据 */
            for (int i = 0; i < result.Length; i++)
            {
                /* CSV的一行数据 */
                Dictionary<string, string> datas = result[i];
                /* 读取Csv数据对象的属性 */
                FieldInfo[] fields = typeof(T).GetFields();
                /* 使用反射，将CSV文件的数据赋值给CSV数据对象的相应字段，要求CSV文件的字段名和CSV数据对象的字段名完全相同 */
                T obj = Activator.CreateInstance<T>();
                foreach (FieldInfo fi in fields)
                {
                    object value = Convert.ChangeType(datas[fi.Name], fi.FieldType);
                    fi.SetValue(obj, value);
                }
                /* 按ID-数据的形式存储 */
                array[i] = obj as BaseTable;
            }
            return array;
        }

        private Dictionary<string, string>[] LoadCsvRowFile(string fileName)
        {
            TextAsset textAsset = ResManager.Instance.Load<TextAsset>("table", fileName);
            if (textAsset == null)
            {
                Log.Error(String.Format("Cannot find data table : {0}", fileName));
                return null;
            }

            string[] fileData = textAsset.text.Split('\n');
            string[] keys = fileData[0].Split(',');
            Dictionary<string, string>[] result = new Dictionary<string, string>[fileData.Length - 1];
            for (int i = 1; i < fileData.Length; i++)
            {
                string[] line = fileData[i].Split(',');
                result[i - 1] = new Dictionary<string, string>();
                for (int j = 0; j < line.Length; j++)
                {
                    //每一行的数据存储规则:Key字段-Value值
                    result[i - 1][keys[j]] = line[j];
                }
            }
            return result;
        }
    }

}
