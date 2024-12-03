//===============================================================================================
//实现自定义通用的二进制序列化,支持对象中单个字段的反序列化
//创建人：张永强
//创建时间：2011-6-20
//===============================================================================================

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Runtime.InteropServices;

namespace Fpi.Util.Serializes
{
    /// <summary>
    /// 二进制序列化通用类
    /// 创建人：张永强
    /// 创建时间：2011-6-20
    /// </summary>
    public class BinarySerializer
    {
        private const int BlockSize = 16384;
        //可以重用的buffer
        private static byte[] arrayBuffer = new byte[BlockSize];

        #region Public Methods
        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="stream">序列化要入的流</param>
        /// <param name="obj">要序列化的对象</param>
        public static void Serialize(Stream stream, object obj)
        {
            SerializeObject(stream, obj);
        }

        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <param name="stream">字节流</param>
        /// <param name="objectType">对象类型</param>
        /// <returns></returns>
        public static object Deserialize(Stream stream, Type objectType)
        {
            return DeserializeObject(stream, objectType);
        }

        /// <summary>
        /// 反序列化对象中的某个字段
        /// </summary>
        /// <param name="stream">字节流</param>
        /// <param name="fieldType">序列化字段的类型</param>
        /// <param name="fieldNames">字段名(可以嵌套)</param>
        /// <returns></returns>
        public static object Deserialize(Stream stream, Type fieldType, string[] fieldNames)
        {
            for (int i = 0; i < fieldNames.Length; i++)
            {
                if (SeekFieldPosition(stream, fieldNames[i]) == -1)
                {
                    return null;
                }

            }
            return DeserializeObject(stream, fieldType);
        }

        #endregion

        #region private methods

        #region 序列化


        /// <summary>
        /// 序列化任何对象
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        private static void SerializeObject(Stream stream, object obj)
        {
            Type objectType;
            if (obj is Type)
            {
                objectType = typeof(Type);
            }
            else
            {
                objectType = obj.GetType();
            }

            SerializeObject(stream, obj, objectType);

        }

        private static void SerializeObject(Stream stream, object obj, Type objectType)
        {
            if (objectType.IsValueType)//值类型
            {
                SerializeValueType(stream, obj,objectType);
            }
            else if (objectType.IsArray) //数组类型
            {
                SerializeArray(stream, (Array)obj);
            }
            else if (objectType == typeof(String)) //字符串
            {
                SerializeString(stream, (string)obj);
            }
            else if (objectType == typeof(Type)) //Type类型
            {
                SerializeString(stream, (obj as Type).AssemblyQualifiedName);
            }
            else if (objectType == typeof(System.Globalization.CultureInfo)) //区域类型
            {
                System.Globalization.CultureInfo info = obj as System.Globalization.CultureInfo;
                SerializeValueType(stream, info.LCID,typeof(int));
            }
            else if (GetInterface(objectType,"System.Collections.IList") != null) //List类型
            {
                SerializeList(stream, obj);
            }
            else if (GetInterface(objectType,"System.Collections.IDictionary") != null) //Dictionary类型
            {
                SerializeDictionary(stream, obj);
            }
            else if (!objectType.Namespace.StartsWith("System.")) //用户自定义类
            {
                SerializeClass(stream, obj, objectType);
            }
            //else
            //{
            //    throw new Exception(string.Format("{0} type is not supported serialize.",objectType.Name));
            //}
        }
        private static void SerializeType(Stream stream, Type type)
        {
            SerializeString(stream, type.AssemblyQualifiedName);
        }
        /// <summary>
        /// 序列化用户自定义类
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        private static void SerializeClass(Stream stream, object obj,Type objectType)
        {
            //得到类中所有的字段
            List<FieldInfo> fields = new List<FieldInfo>();
            GetAllFieldsOfType(objectType, fields);

            //写入字段个数
            SerializeValueType(stream, (short)fields.Count,typeof(short));

            //写入每个字段信息
            List<FieldData> fieldDatas = new List<FieldData>();            
            long offset = 0;
            foreach (FieldInfo info in fields)
            {
                if (info.IsNotSerialized == true) 
                    continue;
                FieldData fieldData = new FieldData(info.Name);
                MemoryStream fieldstream = new MemoryStream();
                SerializeObject(fieldstream, info.GetValue(obj),info.FieldType);
                fieldData.Data = fieldstream.ToArray();
                SerializeString(stream, fieldData.FieldName);
                SerializeValueType(stream,offset,typeof(long));
                fieldData.OffSet = offset;
                fieldDatas.Add(fieldData);
                offset += fieldData.DataLength;
            }
            //写入序列化数据
            foreach (FieldData fielddata in fieldDatas)
            {
                stream.Write(fielddata.Data, 0, fielddata.DataLength);
            }
        }



        /// <summary>
        /// 序列化基元类型
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        private static void SerializePrimitive(Stream stream, object obj,Type objectType)
        {
            if (objectType == typeof(bool))
            {
                stream.WriteByte( (bool)obj == true ? (byte)1 : (byte)0 );
            }
            else if (objectType == typeof(byte))
            {
                stream.WriteByte((byte)obj);
            }
            else if (objectType == typeof(sbyte)) 
            {
                stream.WriteByte((byte)((sbyte)obj));
            }
            else if (objectType == typeof(short))
            {
                stream.Write(BitConverter.GetBytes((short)obj),0,2);
            }
            else if (objectType == typeof(ushort))
            {
                stream.Write( BitConverter.GetBytes((ushort)obj),0,2);
            }
            else if (objectType == typeof(int))
            {
                stream.Write( BitConverter.GetBytes((int)obj),0,4);
            }
            else if (objectType == typeof(uint))
            {
                stream.Write( BitConverter.GetBytes((uint)obj),0,4);
            }
            else if (objectType == typeof(long))
            {
                stream.Write(BitConverter.GetBytes((long)obj),0,8);
            }
            else if (objectType == typeof(ulong))
            {
                stream.Write(BitConverter.GetBytes((ulong)obj),0,8);
            }
            else if (objectType == typeof(float))
            {
                stream.Write(BitConverter.GetBytes((float)obj),0,4);
            }
            else if (objectType == typeof(double))
            { 
                stream.Write(BitConverter.GetBytes((double)obj),0,8);
            }
            else if (objectType == typeof(char))
            {
                stream.Write(BitConverter.GetBytes((char)obj),0,2);
            }
        }

        /// <summary>
        /// 序列化值类型
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        private static void SerializeValueType(Stream stream, object obj, Type objectType)
        {
            //支持值类型的泛型，如Nullable<>
            if (objectType.IsGenericType) 
            {
                objectType = objectType.GetGenericArguments()[0];
                if (obj == null)
                {
                    stream.WriteByte((byte)0x00);
                }
                stream.WriteByte((byte)0x01);
            }
            
            if (objectType.IsPrimitive)//基元类型
            {
                SerializePrimitive(stream, obj, objectType);
            }
            else if (objectType == typeof(DateTime))
            {
                string str = ((DateTime)obj).ToString("yyyy-MM-dd HH:mm:ss");
                SerializeString(stream, str);
                //long binary = ((DateTime)obj).ToBinary();
                //SerializePrimitive(stream, binary,typeof(long));
            }
            else if (objectType == typeof(TimeSpan))
            {
                SerializePrimitive(stream, ((TimeSpan)obj).Ticks,typeof(long));
            }
            else if (objectType == typeof(Guid))
            {
                stream.Write(((Guid)obj).ToByteArray(), 0, 16);
            }
            else if (objectType == typeof(Decimal))
            {
                int[] bits = Decimal.GetBits((decimal)obj);
                for (int i = 0; i < bits.Length; i++)
                {
                    SerializePrimitive(stream, bits[i],typeof(int));
                }
            }
            else if (objectType.IsEnum)
            {
                Type enumType = Enum.GetUnderlyingType(objectType);
                object realObject = Convert.ChangeType(obj, enumType, System.Globalization.CultureInfo.CurrentCulture);
                SerializeObject(stream, realObject);
            }
            else //结构体
            {
                int size = Marshal.SizeOf(objectType);
                byte[] bytes = new byte[size];
                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(obj, ptr, false);
                Marshal.Copy(ptr, bytes, 0, size);
                stream.Write(bytes, 0, size);
                Marshal.FreeHGlobal(ptr);

            }
        }

        /// <summary>
        /// 序列化字符串
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="str"></param>
        private static void SerializeString(Stream stream, string str)
        {
            if (str == null)
            {
                stream.Write(BitConverter.GetBytes((uint)0xFFFFFFFF), 0, 4);
                return;
            }

            // 写入字符串的长度
            int byteCount = Encoding.Unicode.GetByteCount(str);
            stream.Write(BitConverter.GetBytes((uint)byteCount), 0, 4);

            //大于BlockSize,则重新开辟内存
            if (byteCount > BlockSize)
            {
                byte[] bytes = new byte[byteCount];
                Encoding.Unicode.GetBytes(str, 0, str.Length, bytes, 0);
                stream.Write(bytes, 0, byteCount);
                
            }
            else //使用开辟好的内存，提高效率
            {
                lock (arrayBuffer.SyncRoot)
                {
                    Encoding.Unicode.GetBytes(str, 0, str.Length, arrayBuffer, 0);
                    stream.Write(arrayBuffer, 0, byteCount);
                }
            }

        }
        
        /// <summary>
        /// 序列化数组
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="objectArray"></param>
        private static void SerializeArray(Stream stream, Array objectArray)
        {
            //写入数组的维数
            SerializeValueType(stream,(byte)objectArray.Rank,typeof(byte));
            //写入数组元素的总个数
            SerializeValueType(stream, objectArray.Length,typeof(int));

            Type baseType = objectArray.GetType().GetElementType();
            //写入数组每维的个数
            int[] maxIndices = new int[objectArray.Rank];
            for (int i = 0; i < objectArray.Rank; i++)
            {
                maxIndices[i] = objectArray.GetLength(i);
                SerializeValueType(stream, maxIndices[i],typeof(int));
            }
            //序列化数组中的每个对象
            if (objectArray.Rank == 1 && baseType.IsPrimitive)//一维基元类型数组
            {
                int size = Marshal.SizeOf(baseType);
                if (baseType == typeof(bool))
                {
                    size = 1;
                }
                else if (baseType == typeof(char))
                {
                    size = 2;
                }

                int length = objectArray.Length * size;
                int ptr = 0;
                while (ptr < length)
                {
                    int copyAmount = BlockSize;
                    if (ptr + BlockSize > length)
                    {
                        copyAmount = length - ptr;
                    }
                    lock (arrayBuffer.SyncRoot)
                    {
                        Buffer.BlockCopy(objectArray, ptr, arrayBuffer, 0, copyAmount);
                        stream.Write(arrayBuffer, 0, copyAmount);
                    }
                    ptr += copyAmount;
                }
            }
            else
            {
                int count = objectArray.Length;
                int[] indices = new int[objectArray.Rank];
                for (int i = 0; i < count; i++)
                {
                    object arrayValue = objectArray.GetValue(indices);
                    SerializeObject(stream,arrayValue);

                    indices[indices.Length - 1]++;

                    for (int j = indices.Length - 1; j >= 0; j--)
                    {
                        if (indices[j] >= maxIndices[j] && j > 0)
                        {
                            indices[j] = 0;
                            indices[j - 1]++;
                        }
                    }
                    // loop
                }
            }
        }

        /// <summary>
        /// 序列化列表List
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="genericObject"></param>
        private static void SerializeList(Stream stream,object genericObject)
        {
            IList ilist = genericObject as IList;
            IEnumerator enumerator = ilist.GetEnumerator();
            
            SerializeValueType(stream, ilist.Count,typeof(int));

            //类型以第一个元素为准
            bool typeGetted = false;
            while (enumerator.MoveNext())
            {
                if (!typeGetted)
                {
                    SerializeObject(stream, enumerator.Current.GetType());
                    typeGetted = true;
                }
               
                SerializeObject(stream,enumerator.Current);
            }
            
        }

        /// <summary>
        /// 序列化哈希表
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="genericObject"></param>
        private static void SerializeDictionary(Stream stream,object genericObject)
        {
            IDictionary idict = genericObject as IDictionary;
            IDictionaryEnumerator enumerator = idict.GetEnumerator();

            SerializeValueType(stream, idict.Count,typeof(int));
            //类型以第一个元素为准
            bool typeGetted = false;
            while (enumerator.MoveNext())
            {
                if (!typeGetted)
                {
                    SerializeObject(stream, enumerator.Key.GetType());
                    SerializeObject(stream, enumerator.Value.GetType());
                    typeGetted = true;
                }

                SerializeObject(stream, enumerator.Key);
                SerializeObject(stream, enumerator.Value);
            }
        }

        #endregion

        #region 反序列化


        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        private static object DeserializeObject(Stream stream, Type objectType)
        {
            object returnObject = null;

            if (objectType.IsValueType)//值类型
            {
                returnObject = DeserializeValueType(stream, objectType);
            }
            else if (objectType.IsArray)//数组类型
            {
                returnObject = DeserializeArray(stream,objectType);
            }
            else if (objectType == typeof(String)) //字符串类型
            {
                returnObject = DeserializeString(stream);
            }
            else if (objectType == typeof(Type)) //Type类型
            {
                string typeName = DeserializeString(stream);
                returnObject = Type.GetType(typeName);
            }
            else if (objectType == typeof(System.Globalization.CultureInfo)) //区域信息类型
            {
                int LCID = (int)DeserializeValueType(stream,typeof(int));
                returnObject = System.Globalization.CultureInfo.GetCultureInfo(LCID);
            }
            else if (GetInterface(objectType,"System.Collections.IList") != null) //列表类型
            {
                returnObject = DeserializeList(stream,objectType);
            }
            else if (GetInterface(objectType,"System.Collections.IDictionary") != null) //哈希表
            {
                returnObject = DeserializeDictionary(stream,objectType);
            }
            else if (!objectType.Namespace.StartsWith("System")) //用户自定义类
            {
                returnObject = DeserializeClass(stream, objectType);
            }
            return returnObject;

        }
        
        /// <summary>
        /// 反序列化用户自定义类
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        private static object DeserializeClass(Stream stream, Type objectType)
        {
            object returnObject;
            //读取类的字段个数
            stream.Read(arrayBuffer, 0, 2);
            int fieldCount = BitConverter.ToInt16(arrayBuffer, 0);
            //读取字段信息(字段名，存储位置)
            List<FieldData> fieldDatas = new List<FieldData>(fieldCount);
            for (int i = 0; i < fieldCount; i++)
            {
                string fieldName = DeserializeString(stream);
                long offset = (long)DeserializePrimitive(stream,typeof(long));
                FieldData fieldData = new FieldData(fieldName);
                fieldDatas.Add(fieldData);
                fieldData.OffSet = offset;
            }
            long position = stream.Position;

            returnObject = Activator.CreateInstance(objectType);
            List<FieldInfo> fields = new List<FieldInfo>();
            GetAllFieldsOfType(objectType, fields);

            foreach (FieldInfo info in fields)
            {
                if (info.IsNotSerialized == true) continue;
                foreach (FieldData fieldData in fieldDatas)
                {
                    if (fieldData.FieldName.Equals(info.Name))
                    {
                        stream.Seek(fieldData.OffSet+position, SeekOrigin.Begin);
                        info.SetValue(returnObject,DeserializeObject(stream, info.FieldType));
                        break;
                    }
                }
            }
            return returnObject;
        }
        /// <summary>
        /// 设置字段存储在当前流的位置
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private static long SeekFieldPosition(Stream stream, string fieldName)
        {
            long position = -1;
            //读取字段个数
            stream.Read(arrayBuffer, 0, 2);
            int fieldCount = BitConverter.ToInt16(arrayBuffer, 0);

            for (int i = 0; i < fieldCount; i++)
            {
                string name = DeserializeString(stream);
                long offset = (long)DeserializePrimitive(stream, typeof(long));
                if (fieldName.Equals(name))
                {
                    position = offset;
                }
            }
            if (position == -1)
            {
                return -1;
            }
            position += stream.Position;
            stream.Seek(position, SeekOrigin.Begin);
            
            return position;
        }

        /// <summary>
        /// 反序列化基元类型
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        private static object DeserializePrimitive(Stream stream, Type objectType)
        {
            if (objectType == typeof(bool))
            {
                return stream.ReadByte() == 1 ? true : false;
            }
            else if (objectType == typeof(byte))
            {
                return (byte)stream.ReadByte();
            }
            else if (objectType == typeof(sbyte))
            {
                return (sbyte)stream.ReadByte();
            }
            else if (objectType == typeof(short))
            {
                stream.Read(arrayBuffer, 0, 2);
                return BitConverter.ToInt16(arrayBuffer, 0);
            }
            else if (objectType == typeof(ushort))
            {
                stream.Read(arrayBuffer, 0, 2);
                return BitConverter.ToUInt16(arrayBuffer, 0);
            }
            else if (objectType == typeof(int))
            {
                stream.Read(arrayBuffer, 0, 4);
                return BitConverter.ToInt32(arrayBuffer, 0);
            }
            else if (objectType == typeof(uint))
            {
                stream.Read(arrayBuffer, 0, 4);
                return BitConverter.ToUInt32(arrayBuffer, 0);
            }
            else if (objectType == typeof(long))
            {
                stream.Read(arrayBuffer, 0, 8);
                return BitConverter.ToInt64(arrayBuffer, 0);
            }
            else if (objectType == typeof(ulong))
            {
                stream.Read(arrayBuffer, 0, 8);

                return BitConverter.ToUInt64(arrayBuffer, 0);
            }
            else if (objectType == typeof(float))
            {
                stream.Read(arrayBuffer, 0, 4);
                return BitConverter.ToSingle(arrayBuffer, 0);
            }
            else if (objectType == typeof(double))
            {
                stream.Read(arrayBuffer, 0, 8);
                return BitConverter.ToDouble(arrayBuffer, 0);
            }
            else if (objectType == typeof(char))
            {
                stream.Read(arrayBuffer, 0, 2);
                return BitConverter.ToChar(arrayBuffer, 0);
            }
            else
            {
                throw new Exception("Could not retrieve bytes from the object type " + objectType.FullName + ".");
            }
        }

        /// <summary>
        /// 反序列化值类型
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        private static object DeserializeValueType(Stream stream,Type objectType)
        {
            if (objectType.IsGenericType)//值类型的泛型，如Nullable<>
            {
                Type genericTypeDef = objectType.GetGenericTypeDefinition();
                if (genericTypeDef == typeof(Nullable<>))
                {
                    objectType = objectType.GetGenericArguments()[0];
                }
                int flag = stream.ReadByte();
                if (flag == 0)
                {
                    return null;
                }
            }
            if (objectType.IsPrimitive) //基元类型
            {
                return DeserializePrimitive(stream, objectType);
            }
            else if (objectType == typeof(DateTime)) 
            {
                string str = DeserializeString(stream);
                return DateTime.Parse(str);
                //stream.Read(arrayBuffer, 0, 8);
                //long readLong = BitConverter.ToInt64(arrayBuffer, 0);
                //return DateTime.FromBinary(readLong);
            }
            else if (objectType == typeof(TimeSpan))
            {
                stream.Read(arrayBuffer, 0, 8);
                long readLong = BitConverter.ToInt64(arrayBuffer, 0);
                return TimeSpan.FromTicks(readLong);
            }
            else if (objectType == typeof(Guid))
            {
                byte[] guidArray = new byte[16];
                stream.Read(guidArray, 0, 16);
                return new Guid(guidArray);
            }
            else if (objectType == typeof(Decimal))
            {
                int[] bits = new int[4];
                for (int i = 0; i < 4; i++)
                {
                    stream.Read(arrayBuffer, 0, 4);
                    bits[i] = BitConverter.ToInt32(arrayBuffer, 0);
                }
                return new Decimal(bits);
            }
            else if (objectType.IsEnum)
            {
                Type enumType = Enum.GetUnderlyingType(objectType);
                object realObject = DeserializeObject(stream,enumType);
                return Enum.ToObject(objectType, realObject);
            }
            else //结构体
            {
                int size = Marshal.SizeOf(objectType);
                IntPtr ptr = Marshal.AllocHGlobal(size);
                byte[] bytes = new byte[size];
                stream.Read(bytes, 0, size);
                Marshal.Copy(bytes, 0, ptr, size);
                object returnObject = Marshal.PtrToStructure(ptr, objectType);
                Marshal.FreeHGlobal(ptr);
                return returnObject;
            }
        }

        /// <summary>
        /// 反序列化字符串
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static string DeserializeString(Stream stream)
        {
            stream.Read(arrayBuffer, 0, 4);
            uint byteCount = BitConverter.ToUInt32(arrayBuffer, 0);

            if (byteCount == 0xFFFFFFFF)
            {
                return null;
            }

            byte[] byteArray;
            if (byteCount > BlockSize)
            {
                byteArray = new byte[byteCount];
            }
            else
            {
                byteArray = arrayBuffer;
            }

            stream.Read(byteArray, 0, (int)byteCount);
            return Encoding.Unicode.GetString(byteArray, 0, (int)byteCount);
        }
        /// <summary>
        /// 反序列化数组
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        private static object DeserializeArray(Stream stream,Type objectType)
        {
            Type baseType = objectType.GetElementType();

            //读取数组的维数
            int arrayRank = stream.ReadByte();
            //读取数组元素的总个数
            stream.Read(arrayBuffer, 0, 4);
            int count = BitConverter.ToInt32(arrayBuffer, 0);
            //读取每维数组的个数
            int[] maxIndices = new int[arrayRank];
            for (int i = 0; i < arrayRank; i++)
            {
                stream.Read(arrayBuffer, 0, 4);
                maxIndices[i] = BitConverter.ToInt32(arrayBuffer, 0);
            }


            if (baseType.IsPrimitive && arrayRank == 1)
            {
                int size = Marshal.SizeOf(baseType);
                if (baseType == typeof(bool)) size = 1;
                else if (baseType == typeof(char)) size = 2;

                int ptr = 0;


                Array valueTypeArray = Array.CreateInstance(baseType, count);

                int length = count * size;
                while (ptr < length)
                {
                    int copyAmount = BlockSize;
                    if (ptr + copyAmount > length)
                    {
                        copyAmount = length - ptr;
                    }
                    stream.Read(arrayBuffer, 0, copyAmount);
                    Buffer.BlockCopy(arrayBuffer, 0, valueTypeArray, ptr, copyAmount);
                    ptr += copyAmount;
                }
                return valueTypeArray;
            }
            else
            {
                Array newArray = Array.CreateInstance(baseType, maxIndices);
                int[] indices = new int[arrayRank];
                for (int i = 0; i < count; i++)
                {
                    newArray.SetValue(DeserializeObject(stream,baseType), indices);

                    indices[indices.Length - 1]++;

                    for (int j = indices.Length - 1; j >= 0; j--)
                    {
                        if (indices[j] >= maxIndices[j] && j > 0)
                        {
                            indices[j] = 0;
                            indices[j - 1]++;
                        }
                    }
                    // loop
                }
                return newArray;
            }

        }
        /// <summary>
        /// 反序列化列表
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        private static object DeserializeList(Stream stream,Type objectType)
        {
            stream.Read(arrayBuffer, 0, 4);
            int count = BitConverter.ToInt32(arrayBuffer, 0);
            IList ilist = Activator.CreateInstance(objectType) as IList;

            Type baseType = typeof(object);

            //Type[] genericParameters = objectType.GetGenericArguments();
            //if (genericParameters != null && genericParameters.Length > 0)
            //{
            //    baseType = genericParameters[0];
            //}
            if (count > 0)
            {
                baseType = (Type)DeserializeObject(stream, typeof(Type));
            }
            for (int i = 0; i < count; i++)
            {
                //baseType = (Type)DeserializeObject(stream, typeof(Type));
                object deserializedObject = DeserializeObject(stream,baseType);
                ilist.Add(deserializedObject);
            }
            return ilist;
        }

        /// <summary>
        /// 反序列化哈希表
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        private static object DeserializeDictionary(Stream stream,Type objectType)
        {
            stream.Read(arrayBuffer, 0, 4);
            int count = BitConverter.ToInt32(arrayBuffer, 0);

            IDictionary idict = Activator.CreateInstance(objectType) as IDictionary;


            Type keyType = typeof(object);
            Type valueType = typeof(object);
            //Type[] genericParameters = objectType.GetGenericArguments();
            //if (genericParameters.Length > 0)
            //{
            //    keyType = genericParameters[0];
            //    valueType = genericParameters[1];
            //}
            if (count > 0)
            {
                keyType = (Type)DeserializeObject(stream, typeof(Type));
                valueType = (Type)DeserializeObject(stream, typeof(Type));
            }
            for (int i = 0; i < count; i++)
            {
                object desKey = DeserializeObject(stream,keyType);
                object value = DeserializeObject(stream,valueType);
                idict.Add(desKey, value);
            }
            return idict;
        }

        #endregion

        /// <summary>
        /// 得到类的所有字段信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fieldList"></param>
        private static void GetAllFieldsOfType(Type type, List<FieldInfo> fieldList)
        {
            if (type == null || type == typeof(object) || type == typeof(ValueType))
            {
                return;
            }

            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].IsNotSerialized) continue;
                fieldList.Add(fields[i]);
            }

            GetAllFieldsOfType(type.BaseType, fieldList);
        }

        /// <summary>
        /// 搜索类型具有指定名称的接口
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static Type GetInterface(Type type ,string name)
        {
            Type[] types = type.GetInterfaces();
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return types[i];
                }
            }
            return null;
        }

        #endregion
    }
    public class FieldData
    {
        public FieldData(string fieldname)
        {
            this.fieldName = fieldname;
        }
        private string fieldName;
        public string FieldName
        {
            get { return fieldName; }
            set { fieldName = value; }
        }
        
        private byte[] data;
        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }
        private long offset;
        public long OffSet
        {
            get { return offset; }
            set { offset = value; }
        }
        public int DataLength
        {
            get { return data.Length; }
        }

        public override string ToString()
        {
            return this.fieldName;
        }
    }
}
