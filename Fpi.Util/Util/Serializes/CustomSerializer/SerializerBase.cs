using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Fpi.Util.Serializes.CustomSerializer
{
    public abstract class SerializerBase<T>:ISerializationSurrogate 
    {  
        public abstract void GetData(T item, SerializationInfo info);

        public abstract T SetData(T item, SerializationInfo info);

        void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            if (obj is T)
            {
                T item = (T)obj;
                this.GetData(item,info);
            }
        }

        object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return this.SetData((T)obj, info);            
        }
    }
}
