/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityFramework
{


    /// <summary>
    /// 技能表数据结构
    /// </summary>
    public class SkillTable
    {
        public int ID;
        public int Type;
        public int SkillTarget;
        public int TargetNum;
        public int Result1_ResultID;
        public int Result1_Val1;
        public int Result1_Val2;
        public int Result1_Val3;
        public int Result2_ResultID;
        public int Result2_Val1;
        public int Result2_Val2;
        public int Result2_Val3;
        public int NextSkillID;
        public int NextSkillRate;
        public int Aval;
        public int Nval;
        public int AddMingZhong;
        public int AddBaoJiLv;

    }

    /// <summary>
    /// Buff表数据结构
    /// </summary>
    public class StatusTable
    {
        public int ID;
        public int ResultID;
        public int Aval;
    }
}
