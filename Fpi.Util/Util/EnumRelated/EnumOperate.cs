using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Fpi.Util.EnumRelated
{
    /// <summary>
    /// 枚举操作
    /// </summary>
    public static class EnumOperate
    {
        /// <summary>
        /// 把枚举绑定到ComboBox
        /// </summary>
        /// <param name="cmb"></param>
        /// <param name="type"></param>
        public static void BandEnumToCmb(ComboBox cmb, Type type)
        {
            IList list = ToList(type);
            cmb.DisplayMember = "Value";
            cmb.ValueMember = "Key";
            cmb.DataSource = list;
            cmb.SelectedIndex = -1;
        }

        /// <summary>
        /// 得到枚举类型的描述属性
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetEnumDesc(object obj)
        {
            //获取字段信息
            System.Reflection.FieldInfo[] fs = obj.GetType().GetFields();

            foreach (System.Reflection.FieldInfo f in fs)
            {
                //判断名称是否相等x
                if (f.Name == obj.ToString())
                {
                    //反射出自定义属性
                    object[] attrs = f.GetCustomAttributes(true);
                    foreach (Attribute attr in attrs)
                    {
                        //类型转换找到一个Description，用Description作为成员名称
                        if (attr is DescriptionAttribute)
                        {
                            return (attr as DescriptionAttribute).Description;
                        }
                    }
                }
            }
            //如果没有检测到合适的注释，则用默认名称
            return "";
        }

        /// <summary>
        /// 获取枚举各(字段值、描述)组成的列表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static IList ToList(Type type)
        {
            var list = new ArrayList();
            if (type == null) return list;
            if (!type.IsEnum) return list;

            foreach (Enum value in Enum.GetValues(type))
            {
                list.Add(new KeyValuePair<int, string>(value.GetHashCode(), GetEnumDesc(value)));
            }
            return list;
        }

        private static readonly Random rd = new Random();

        /// <summary>
        /// 获取枚举中随机值
        /// </summary>
        /// <param name="type">枚举类型</param>
        /// <returns>枚举值的int值。强制转换为枚举值即可达到枚举中随机值</returns>
        public static int GetRandomEnum(Type type)
        {
            Array enumsData = Enum.GetValues(type);
            return (int)enumsData.GetValue(rd.Next(enumsData.Length));
        }

    }
}
