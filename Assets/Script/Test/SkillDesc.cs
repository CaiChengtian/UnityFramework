/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:
*/
using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Reflection;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace UnityFramework
{
    public class SkillDesc : MonoBehaviour
    {
        public TextAsset SkillCsv;

        public TextAsset StatusCsv;

        private string fileName = "D:/SkillDesc.txt";

        private Dictionary<int, SkillTable> dicSkill;
        private Dictionary<int, StatusTable> dicStatus;

        private Dictionary<int, string> dicDesc;
        private Dictionary<int, string> dicBuff;

        private SkillTable table;
        private SkillTable next;
        private bool round;

        void Start()
        {
            dicDesc = new Dictionary<int, string>();
            dicBuff = new Dictionary<int, string>();
            dicSkill = new Dictionary<int, SkillTable>();
            dicStatus = new Dictionary<int, StatusTable>();
            LoadCsvData();
            InitBuff();
            CreateDesc();
            Save();
        }

        public void CreateDesc()
        {
            StringBuilder desc = new StringBuilder();
            foreach (SkillTable table in dicSkill.Values)
            {
                if (table.Type == 2)
                {
                    continue;
                }
                desc.Remove(0, desc.Length);
                desc.Append("对");
                desc.Append(Target(table.SkillTarget, table.TargetNum));
                desc.Append(Attack(table.Aval, table.Nval));
                desc.Append(Result(
                    table.Result1_ResultID,
                    table.Result1_Val1,
                    table.Result1_Val2,
                    table.Result1_Val3
                    ));
                desc.Append(Result(
                    table.Result2_ResultID,
                    table.Result2_Val1,
                    table.Result2_Val2,
                    table.Result2_Val3
                    ));
                desc.Append(Round());
                if (table.NextSkillID > 0)
                {
                    //有后续技能
                    next = dicSkill[table.NextSkillID];
                    desc.Append(",对");
                    desc.Append(Target(next.SkillTarget, next.TargetNum));
                    desc.Append(Attack(next.Aval, next.Nval, "额外"));
                    desc.Append(Result(
                        next.Result1_ResultID,
                        next.Result1_Val1,
                        next.Result1_Val2,
                        next.Result1_Val3
                        ));
                    desc.Append(Result(
                        next.Result2_ResultID,
                        next.Result2_Val1,
                        next.Result2_Val2,
                        next.Result2_Val3
                        ));
                    desc.Append(Round());
                }
                desc.Append(Ex(table.AddMingZhong, table.AddBaoJiLv));
                desc.Append("。");
                dicDesc.Add(table.ID, desc.ToString());
            }
        }

        private string Target(int target, int num)
        {
            string str;
            switch (target)
            {
                case 1:
                    str = "自己";
                    break;
                case 2:
                    str = "所有队友";
                    break;
                case 3:
                    str = "所有敌人";
                    break;
                case 4:
                    str = "单个敌人";
                    break;
                case 5:
                    str = "前排敌人";
                    break;
                case 6:
                    str = "后排敌人";
                    break;
                case 7:
                    str = "一列敌人";
                    break;
                case 8:
                    str = String.Format("随机{0}个敌人", num);
                    break;
                case 9:
                    str = String.Format("随机{0}个队友", num);
                    break;
                case 10:
                    str = String.Format("血量最少的{0}个敌人", num);
                    break;
                case 11:
                    str = String.Format("血量最少的{0}个队友", num);
                    break;
                case 12:
                    str = "后排单个敌人";
                    break;
                case 18:
                    str = "敌人及其相邻目标";
                    break;
                case 19:
                    str = "血量低于60%的队友";
                    break;
                default:
                    str = "";
                    Debug.LogError("Unkonwn Target:" + target);
                    break;
            }
            return str;
        }

        private string Attack(int aVal, int nVal, string add = "")
        {
            string str;
            if (aVal > 0)
            {
                //伤害
                if (nVal == 0)
                {
                    str = String.Format("{0}造成%s%%伤害", add, aVal);
                }
                else
                {
                    str = String.Format("{0}造成(%s%% + %s)伤害", add, aVal, nVal);
                }

            }
            else if (aVal < 0)
            {
                //治疗
                if (nVal == 0)
                {
                    str = String.Format("{0}治疗%s%%", add, aVal * -1);
                }
                else
                {
                    str = String.Format("{0}治疗(%s%% + %s)", add, aVal * -1, nVal * -1);
                }
            }
            else
            {
                //
                str = "";
                Debug.LogError("Aval == 0");
            }
            return str;
        }

        private string Result(int result, int value1, int value2, int value3)
        {
            string str;
            string rate;
            switch (result)
            {
                case 1:
                    rate = value3 < 100 ? value3 + "%几率使" : "";
                    str = String.Format(",{0}自身{1}", rate, Buff(value1));
                    if (value2 == 2)
                    {
                        round = true;
                    }
                    break;
                case 2:
                    rate = value3 < 100 ? value3 + "%几率使" : "";
                    str = String.Format(",{0}目标{1}", rate, Buff(value1));
                    if (value2 == 2)
                    {
                        round = true;
                    }
                    break;
                case 3:
                    rate = value2 < 100 ? value2 + "%几率" : "";
                    string add = value1 > 0 ? "增加" + value1 : "减少" + value1*-1;
                    str = String.Format(",{0}{1}点怒气", rate, add);
                    break;
                case 4:
                    str = ",清除所有负面状态";
                    break;
                case 5:
                    str = ",清除所有增益状态";
                    break;
                default:
                    str = "";
                    Debug.LogError("Unkonwn Result:" + result);
                    break;
            }
            return str;
        }

        private string Buff(int id)
        {
            return dicBuff[id];
        }

        private string Ex(int mingzhong, int baoji)
        {
            if (mingzhong > 0 && baoji > 0)
            {
                return String.Format(",本次攻击命中率和暴击率提升{0}%", mingzhong);
            }
            else if (mingzhong > 0)
            {
                return String.Format(",本次攻击命中率提升{0}%", mingzhong);
            }
            else if (baoji > 0)
            {
                return String.Format(",本次攻击暴击率提升{0}%", baoji);
            }
            else
            {
                return "";
            }

        }

        private string Round()
        {
            if (round)
            {
                round = false;
                return ",持续2回合";
            }
            else
            {
                return "";
            }
        }

        private void InitBuff()
        {
            foreach (StatusTable item in dicStatus.Values)
            {
                switch (item.ResultID)
                {
                    case 1:
                        dicBuff.Add(item.ID, String.Format("眩晕", item.Aval));
                        break;
                    case 2:
                        dicBuff.Add(item.ID, String.Format("造成灼烧({0}%)", item.Aval));
                        break;
                    case 3:
                        dicBuff.Add(item.ID, String.Format("无敌一回合", item.Aval));
                        break;
                    case 4:
                        dicBuff.Add(item.ID, String.Format("受到的伤害减少{0}%", item.Aval));
                        break;
                    case 5:
                        dicBuff.Add(item.ID, String.Format("受到的伤害增加{0}%", item.Aval));
                        break;
                    case 6:
                        dicBuff.Add(item.ID, String.Format("伤害减免增加{0}%", item.Aval));
                        break;
                    case 8:
                        dicBuff.Add(item.ID, String.Format("攻击力增加{0}%", item.Aval));
                        break;
                    case 9:
                        dicBuff.Add(item.ID, String.Format("攻击力减少{0}%", item.Aval));
                        break;
                    case 11:
                        dicBuff.Add(item.ID, String.Format("暴击率增加{0}%", item.Aval));
                        break;
                    case 12:
                        dicBuff.Add(item.ID, String.Format("命中率增加{0}%", item.Aval));
                        break;
                    case 13:
                        dicBuff.Add(item.ID, String.Format("闪避率增加{0}%", item.Aval));
                        break;
                    case 14:
                        dicBuff.Add(item.ID, String.Format("防御减少{0}%", item.Aval));
                        break;
                    case 17:
                        dicBuff.Add(item.ID, String.Format("伤害增加{0}%", item.Aval));
                        break;
                    default:
                        Debug.LogError("Unknown buff result,ID:" + item.ID);
                        break;
                }
            }
        }

        //保存技能描述到文件
        private void Save()
        {
            Assert.IsNotNull<Dictionary<int, string>>(dicDesc);
            string[] datas = new string[dicDesc.Count];
            StringBuilder line = new StringBuilder();

            int index = 0;
            foreach (KeyValuePair<int, string> item in dicDesc)
            {
                datas[index++] = item.Key + "!" + item.Value;
            }
            //写入文件
            FileStream fs = new FileStream(fileName, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            for (int i = 0; i < datas.Length; i++)
            {
                sw.WriteLine(datas[i]);
                sw.Flush();
            }
            sw.Close();
            fs.Close();
        }

        //加载数据
        private void LoadCsvData()
        {
            Dictionary<int, Dictionary<string, string>> result = LoadCsvFile(SkillCsv);
            foreach (Dictionary<string, string> item in result.Values)
            {
                FieldInfo[] props = typeof(SkillTable).GetFields();
                SkillTable obj = Activator.CreateInstance<SkillTable>();
                foreach (FieldInfo fi in props)
                {
                    string temp = item[fi.Name];
                    object value = Convert.ChangeType(item[fi.Name], fi.FieldType);
                    fi.SetValue(obj, value);
                }
                dicSkill.Add(obj.ID, obj);
            }

            result = LoadCsvFile(StatusCsv);
            foreach (Dictionary<string, string> item in result.Values)
            {
                FieldInfo[] props = typeof(StatusTable).GetFields();
                StatusTable obj = Activator.CreateInstance<StatusTable>();
                foreach (FieldInfo fi in props)
                {
                    string temp = item[fi.Name];
                    object value = Convert.ChangeType(item[fi.Name], fi.FieldType);
                    fi.SetValue(obj, value);
                }
                dicStatus.Add(obj.ID, obj);
            }
        }

        //加载csv文件
        private Dictionary<int, Dictionary<string, string>> LoadCsvFile(TextAsset asset)
        {
            if (asset == null)
            {
                Debug.LogError("请将技能表的csv文件拖到CsvFile参数上");
                return null;
            }

            string text = asset.text;
            Dictionary<int, Dictionary<string, string>> result = new Dictionary<int, Dictionary<string, string>>();
            //CSV文件的第一行为Key字段,第二行开始是数据;第一列一定是ID
            string[] fileData = text.Split('\n');
            string[] keys = fileData[0].Split(',');
            for (int i = 1; i < fileData.Length; i++)
            {
                if (String.IsNullOrEmpty(fileData[i]))
                {
                    continue;
                }
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
    }
}
