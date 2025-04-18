//==================================================================================================
//类名：     UnSerializeFieldAttribute   
//创建人:    hongbing_mao
//创建时间:  2012-12-5 15:23:19
//
//修改人    修改时间    修改后版本              修改内容
//
//
//==================================================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Fpi.Xml
{   
    /// <summary>
    /// 不需要序列化字段特性类
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class UnSerializeFieldAttribute : Attribute
    {
    }
}
