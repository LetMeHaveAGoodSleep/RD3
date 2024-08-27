using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class DisplayEnumConverter : EnumConverter
    {
        public DisplayEnumConverter(Type type)
            : base(type)
        {
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value != null)
                {
                    FieldInfo field = value.GetType().GetField(value.ToString());
                    if (field != null)
                    {
                        DisplayAttribute[] array = (DisplayAttribute[])field.GetCustomAttributes(typeof(DisplayAttribute), inherit: false);
                        if (array.Length == 0 || string.IsNullOrEmpty(array[0].Name))
                        {
                            return value.ToString();
                        }

                        return array[0].Name;
                    }
                }

                return string.Empty;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return base.ConvertFrom(context, culture, value);
        }
    }
}
