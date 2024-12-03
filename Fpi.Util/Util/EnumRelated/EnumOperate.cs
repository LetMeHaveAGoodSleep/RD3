using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Fpi.Util.EnumRelated
{
    /// <summary>
    /// ö�ٲ���
    /// </summary>
    public static class EnumOperate
    {
        /// <summary>
        /// ��ö�ٰ󶨵�ComboBox
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
        /// �õ�ö�����͵���������
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetEnumDesc(object obj)
        {
            //��ȡ�ֶ���Ϣ
            System.Reflection.FieldInfo[] fs = obj.GetType().GetFields();

            foreach (System.Reflection.FieldInfo f in fs)
            {
                //�ж������Ƿ����x
                if (f.Name == obj.ToString())
                {
                    //������Զ�������
                    object[] attrs = f.GetCustomAttributes(true);
                    foreach (Attribute attr in attrs)
                    {
                        //����ת���ҵ�һ��Description����Description��Ϊ��Ա����
                        if (attr is DescriptionAttribute)
                        {
                            return (attr as DescriptionAttribute).Description;
                        }
                    }
                }
            }
            //���û�м�⵽���ʵ�ע�ͣ�����Ĭ������
            return "";
        }

        /// <summary>
        /// ��ȡö�ٸ�(�ֶ�ֵ������)��ɵ��б�
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
        /// ��ȡö�������ֵ
        /// </summary>
        /// <param name="type">ö������</param>
        /// <returns>ö��ֵ��intֵ��ǿ��ת��Ϊö��ֵ���ɴﵽö�������ֵ</returns>
        public static int GetRandomEnum(Type type)
        {
            Array enumsData = Enum.GetValues(type);
            return (int)enumsData.GetValue(rd.Next(enumsData.Length));
        }

    }
}
