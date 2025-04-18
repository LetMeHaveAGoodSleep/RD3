using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared.Util
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static class CloneUtil
    {
        public static T Clone<T>(T original) where T : class
        {
            T clone = Activator.CreateInstance<T>();
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                if (!property.CanWrite || property.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                object value = property.GetValue(original, null);
                property.SetValue(clone, value, null);
            }

            return clone;
        }
    }
}
