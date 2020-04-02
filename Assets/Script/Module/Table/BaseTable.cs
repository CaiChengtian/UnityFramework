/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:CSV配置表基类.
		每个CSV配置表都必须有一个配置表类与之对应,这些配置表类必须派生自BaseTable.
		CSV文件名 = 类名 + "Table".(例: Pet.csv - PetTable.cs)
		CSV配置表的首行为字段名,第二行开始是数据;
		CSV配置表的首列为ID
			如果第二列不是Index,则是单主键表;
			如果第二列为Index,则是双主键表.
		派生类的属性必须和CSV文件的首行字段名完全对应(大小写完全匹配),位置不限.
		派生类的属性是只读的.

*/

namespace UnityFramework
{
    /// <summary>
    /// 配置表基类
    /// </summary>
    public abstract class BaseTable
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int ID;
    }
}
