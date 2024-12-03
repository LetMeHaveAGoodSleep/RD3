//==================================================================================================
//类名：     BaseSerializableObject   
//创建人:    hongbing_mao
//创建时间:  2012-9-29 16:12:07
//
//修改人    修改时间    修改后版本              修改内容
//
//
//==================================================================================================
using System;
using System.Collections.Generic;
using System.Text;
using Fpi.Util.Serializes.CustomSerializer;
using System.Reflection;
using System.Collections;
using System.Runtime.Serialization;

namespace Fpi.Util.Serializes.CustomSerializer
{
    /// <summary>
    /// 可序列化对象基类
    /// </summary>
    [Serializable]
    public abstract class BaseSerializableObject : SerializerBase<BaseSerializableObject>
    { 
        #region 序列化实现方法
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="item"></param>
        /// <param name="info"></param>
        public override void GetData(BaseSerializableObject item, SerializationInfo info)
        {
            //得到类所有属性
            FieldInfo[] fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            //遍历属性
            foreach (FieldInfo field in fields)
            {
                object[] attributes = field.GetCustomAttributes(true);
                if (attributes != null)
                {
                    foreach (Attribute attr in attributes)
                    {
                        //判断该属性是否具备可序列化特性
                        if (attr is SerializableFieldAttribute)
                        {
                            object obj = field.GetValue(this);
                            if (obj != null)
                            {
                                info.AddValue(field.Name, obj);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="item"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public override BaseSerializableObject SetData(BaseSerializableObject item, SerializationInfo info)
        {
            SerializationInfoEnumerator itor = info.GetEnumerator();
         
            while (itor.MoveNext())
            {
                string name = itor.Name;
                SetIntanceValue(name, itor.Value);
            }

            return item;
        }
        /// <summary>
        /// 设置对象属性值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private void SetIntanceValue(string name, object value)
        {
            FieldInfo field = this.GetType().GetField(name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (field.FieldType.Name.StartsWith("List`1"))
            {
                IList newObjList = Activator.CreateInstance(field.FieldType) as IList;
                IList oldObjList = value as IList;
                foreach (object objtemp in oldObjList)
                {
                    newObjList.Add(objtemp);
                }
                value = newObjList;
            }
            field.SetValue(this, value);
        }
      
        #endregion

    }
}
