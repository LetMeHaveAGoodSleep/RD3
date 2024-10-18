using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public static class ObjectCloner
    {
        public static T DeepCopy<T>(T obj)
        {
            if (obj == null)
                return default;

            var type = obj.GetType();

            if (type.IsValueType || type == typeof(string))
            {
                return obj;
            }

            var clone = Activator.CreateInstance(type);

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (property.CanWrite)
                {
                    var propertyValue = property.GetValue(obj);
                    var clonedValue = DeepCopy(propertyValue);
                    property.SetValue(clone, clonedValue);
                }
            }

            return (T)clone;
        }
    }
}
