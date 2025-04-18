//==================================================================================================
//类名：     SerializeFieldAttribute   
//创建人:    hongbing_mao
//创建时间:  2012-9-29 16:08:40
//
//修改人    修改时间    修改后版本              修改内容
//
//
//==================================================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Fpi.Util.Serializes.CustomSerializer
{
    /// <summary>
    /// 可序列化字段特性类
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class SerializableFieldAttribute : Attribute
    {
    }
}
