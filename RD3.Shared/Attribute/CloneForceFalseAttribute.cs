using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    /// <summary>
    /// 标记深克隆时需强制设为false的bool属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)] // 仅用于属性，支持继承
    public class CloneForceFalseAttribute : Attribute { }
}
