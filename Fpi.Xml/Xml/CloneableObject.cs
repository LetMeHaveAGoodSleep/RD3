//==================================================================================================
//类名：     CloneableObject   
//创建人:    hongbing_mao
//创建时间:  2012-12-3 17:14:19
//
//修改人    修改时间    修改后版本              修改内容
//
//
//==================================================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;
using System.ComponentModel;

namespace Fpi.Xml
{

    /// <summary>
    /// CloneableObject类是一个用来继承的抽象类。 
    /// 每一个由此类继承而来的类将自动支持克隆方法。
    /// 该类实现了Icloneable接口，并且每个从该对象继承而来的对象都将同样地
    /// 支持Icloneable接口。 
    /// </summary> 
    public abstract class CloneableObject : ICloneable
    {
        /// <summary>    
        /// 克隆对象，并返回一个已克隆对象的引用    
        /// </summary>    
        /// <returns>引用新的克隆对象</returns>     
        public virtual object Clone()
        {
            //建立指定类型的一个实例         
            object newObject = Activator.CreateInstance(this.GetType(), true);

            //取得新的类型实例的字段数组。         
            FieldInfo[] fields = newObject.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            #region field copy
            foreach (FieldInfo fi in fields)
            {
                bool needClone = true;
                object[] attributes = fi.GetCustomAttributes(true);
                if (attributes != null)
                {
                    foreach (Attribute attr in attributes)
                    {
                        if (attr is UnSerializeFieldAttribute)
                        {
                            needClone = false;
                            break;
                        }
                    }
                }
                if (!needClone) continue;
                //判断字段是否支持ICloneable接口。             
                Type ICloneType = fi.FieldType.GetInterface("ICloneable", true);
                //if (this.GetType().Name == "DataManager")
                //{
                //}
                if (ICloneType != null)
                {
                    //取得对象的Icloneable接口。                 
                    ICloneable IClone = (ICloneable)fi.GetValue(this);
                    //使用克隆方法给字段设定新值。  
                    if (IClone == null)
                    {
                        fi.SetValue(newObject, IClone);
                    }
                    else
                    {
                        fi.SetValue(newObject, IClone.Clone());
                    }
                }
                else
                {
                    // 如果该字段不支持Icloneable接口，直接设置即可。                 
                    fi.SetValue(newObject, fi.GetValue(this));
                }
                //判断字段是否支持IParentNode接口。             
                Type IParentNodType = fi.FieldType.GetInterface("IParentNode", true);
                if (IParentNodType != null)
                {
                    IParentNode IParentNode = (IParentNode)fi.GetValue(newObject);
                    if (IParentNode != null)
                    {
                        IParentNode.SetParentNode(this);
                    }
                }

                //检查该对象是否支持IEnumerable接口，如果支持，             
                //检查是否支持IList 或 IDictionary 接口。            
                Type IEnumerableType = fi.FieldType.GetInterface("IEnumerable", true);
                if (IEnumerableType != null)
                {
                    //取得该字段的IEnumerable接口                
                    IEnumerable IEnum = null;
                    try
                    {
                        IEnum = (IEnumerable)fi.GetValue(this);
                    }
                    catch 
                    {
                    }
                    
                    if (IEnum == null) continue;
                    //这个版本支持IList 或 IDictionary 接口来迭代集合。                
                    Type IListType = fi.FieldType.GetInterface("IList", true);
                    Type IDicType = fi.FieldType.GetInterface("IDictionary", true);
                    int j = 0;
                    if (IListType != null)
                    {
                        //取得IList接口。                     
                        IList list = (IList)fi.GetValue(newObject);
                        foreach (object obj in IEnum)
                        {
                            //查看当前项是否支持支持ICloneable 接口。                         
                            ICloneType = obj.GetType().
                            GetInterface("ICloneable", true);
                            if (ICloneType != null)
                            {
                                //如果支持ICloneable 接口，    
                                //我们用它设置列表中的对象的克隆    
                                ICloneable clone = (ICloneable)obj;
                                list[j] = clone.Clone();
                            }
                            //注意：如果列表中的项不支持ICloneable接口，那么                      
                            //在克隆列表的项将与原列表对应项相同                      
                            //（只要该类型是引用类型）                        
                            j++;
                        }
                    }
                    else if (IDicType != null)
                    {
                        //取得IDictionary 接口                    
                        IDictionary dic = (IDictionary)fi.GetValue(newObject);
                        j = 0;
                        foreach (DictionaryEntry de in IEnum)
                        {
                            //查看当前项是否支持支持ICloneable 接口。                         
                            ICloneType = de.Value.GetType().
                            GetInterface("ICloneable", true);
                            if (ICloneType != null)
                            {
                                ICloneable clone = (ICloneable)de.Value;
                                dic[de.Key] = clone.Clone();
                            }
                            j++;
                        }
                    }
                }
            }
            #endregion

            #region event copy 
            //EventInfo[] events = this.GetType().GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            //foreach (EventInfo eventinfo in events)
            //{
            //    Delegate[] des = GetObjectEventList(this, eventinfo.Name);

            //    if (des != null && des.Length > 0)
            //    {
            //        for (int i = 0; i < des.Length; i++)
            //        {
            //            eventinfo.AddEventHandler(newObject, des[i]);
            //        }
            //    }
            //}
            #endregion
            return newObject;
        }

        /// <summary>   
        /// 获取对象事件
        /// </summary>   
        /// <param name="p_Object">对象</param>   
        /// <param name="p_EventName">事件名</param>   
        /// <returns>委托列</returns>   
        public Delegate[] GetObjectEventList(object p_Object, string p_EventName)
        {
            System.Reflection.FieldInfo _Field = p_Object.GetType().GetField(p_EventName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
            if (_Field == null)
            {
                return null;
            }
            object _FieldValue = _Field.GetValue(p_Object);

            if (_FieldValue != null && _FieldValue is Delegate)
            {
                Delegate _ObjectDelegate = (Delegate)_FieldValue;
                return _ObjectDelegate.GetInvocationList();
            }
            return null;
        }  

        /// <summary>
        /// 保存对象到指定对象
        /// </summary>
        /// <param name="desobject"></param>
        public virtual void Save(Object desobject)
        {
            if (desobject.GetType().FullName != this.GetType().FullName)
            {
                return;
            }
            FieldInfo[] fields = desobject.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                field.SetValue(desobject, field.GetValue(this));
            }
        }
    }
}
