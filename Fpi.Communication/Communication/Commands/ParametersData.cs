using System;
using System.Text;
using Fpi.Communication.Commands.Config;
using Fpi.Util.Sundry;
using Fpi.Communication.Converter;
using Fpi.Xml;

namespace Fpi.Communication.Commands
{
    /// <summary>
    /// ParametersData ��ժҪ˵����
    /// </summary>
    public class ParametersData
    {
        private NodeList parameters;
        protected IDataConvertable converter;
        private int paramPrefix;

        private byte[] data;

        public ParametersData(NodeList parameters, IDataConvertable converter, int paramPrefix, byte[] data)
        {
            this.parameters = parameters;
            this.converter = converter;
            this.paramPrefix = paramPrefix;

            int dataLength = GetDataLength();
            if (data == null)
            {
                this.data = new byte[dataLength];
                //����ȱʡֵ
                SetDefaultValue();
            }
            else
            {
                this.data = new byte[data.Length];
                Buffer.BlockCopy(data, 0, this.data, 0, data.Length);
            }
        }

        private void SetDefaultValue()
        {
            if (this.parameters == null)
            {
                return;
            }
            foreach (Param param in this.parameters)
            {
                if (param.GetValid() && (param.display != null) && (param.display.defaultValue != null))
                {
                    SetObjectValue(param, param.display.defaultValue);
                }
            }
        }

        public byte[] GetData()
        {
            return data;
        }

        public byte[] GetData(int stratIndex, int dataLength)
        {
            if (stratIndex + dataLength > data.Length)
            {
                throw new Exception("over than source data length");
            }
            byte[] rv = new byte[dataLength];
            Buffer.BlockCopy(data, stratIndex, rv, 0, rv.Length);
            return rv;
        }

        public byte[] GetDataByEndTrim(int validDataLength)
        {
            if (validDataLength < 0 || validDataLength > data.Length)
            {
                throw new Exception("not valid data length");
            }

            byte[] resultData = new byte[validDataLength];
            Buffer.BlockCopy(data, 0, resultData, 0, resultData.Length);

            return resultData;
        }

        public void SetDataByEndTrim(int validDataLength)
        {
            SetData(GetDataByEndTrim(validDataLength));
        }

        public byte GetData(int index)
        {
            return data[index];
        }

        public int GetParamId(int number)
        {
            return (paramPrefix << 8) + number;
        }

        //���ݲ���id���õ�����
        public Param GetParam(int paramId)
        {
            //�ж��Ƿ�ǰ����Ĳ���
            if ((paramId >> 16) != (paramPrefix >> 8))
                throw new InvalidOperationException("param prefix error.");
            return (Param) parameters[paramId & 0xff];
        }

        //�ж��Ƿ�����Ĳ���
        public int GetParamCount()
        {
            return parameters.GetCount();
        }


        //�õ�����byte����
        public int GetParamLength(Param param)
        {
            return converter.GetTypeLength(param.type)*param.length;
        }

        //�õ�����byte����
        public int GetParamLength(int number)
        {
            int paramId = this.GetParamId(number);
            Param param = GetParam(paramId);
            return GetParamLength(param);
        }


        public void SetParamLength(int paramId, int paramLength)
        {
            Param param = GetParam(paramId);
            param.length = paramLength;
        }

        public void SetParamValid(int paramId, bool valid, bool resetData)
        {
            Param param = GetParam(paramId);
            param.SetValid(valid);
            int dataLength = this.GetDataLength();
            if (resetData)
                data = new byte[dataLength];
        }

        public void SetParamValid(int paramId, bool valid)
        {
            SetParamValid(paramId, valid, true);
        }

        //�õ�command�����ֽ���
        private int GetDataLength()
        {
            if (parameters == null)
                return 0;

            int length = 0;
            int bitCount = 0;
            int count = parameters.GetCount();
            for (int i = 0; i < count; i++)
            {
                Param param = (Param) parameters[i];
                if (param.GetValid())
                {
                    int tempLength = GetParamLength(param);
                    if (tempLength > 0)
                    {
                        if (bitCount > 0)
                        {
                            length++;
                            bitCount = 0;
                        }
                        param.SetOffset(length);
                        length += tempLength;
                    }
                    else if (tempLength < 0)
                    {
                        param.SetOffset(length);
                        param.SetBitOffset(bitCount);

                        bitCount -= tempLength;
                        if (bitCount >= 8)
                        {
                            length += bitCount/8;
                            bitCount = bitCount%8;
                        }
                    }
                }
            }

            if (bitCount > 0)
                length++;
            return length;
        }

        //�����������Ƿ�Ϸ�
        private void CheckType(Param param, string typeName, bool isArray)
        {
            if (!param.type.Equals(typeName))
                throw new InvalidCastException(param.type + " is not " + typeName + " type��");
            if (!isArray && (param.length > 1))
                throw new InvalidCastException(param.name + "is array. length = " + param.length);
            if (isArray && (param.length <= 1))
                throw new InvalidCastException(param.name + "is not array. length = " + param.length);
        }

        //�����������Ƿ�Ϸ�
        private void CheckBitType(Param param)
        {
            if (!param.type.Equals("bit"))
                throw new InvalidCastException(param.type + " is not bit type��");
        }


        //�жϳ����Ƿ�Ϸ�
        private void CheckLength(Param param, int inputLength, int offset)
        {
            if (inputLength > param.length)
                throw new InvalidCastException("param is longer than defined. param.length = "
                                               + param.length + ", input length = " + inputLength);
            else if (inputLength < param.length)
            {
                int start = offset + converter.GetTypeLength(param.type)*inputLength;
                int end = offset + GetParamLength(param);
                for (int i = start; i < end; i++)
                {
                    data[i] = 0;
                }
            }
        }

        //�жϳ����Ƿ�Ϸ�
        private void CheckBitValue(Param param, int value)
        {
            if (value >= (1 << param.length))
                throw new InvalidCastException("param is too large than defined. param.length = "
                                               + param.length + ", value = " + value);
        }


        //�õ�string
        public string GetString(int paramId)
        {
            Param param = GetParam(paramId);
            CheckType(param, "string", true);
            return GetString(param);
        }

        public string GetString(Param param)
        {
            int offset = param.GetOffset();
            return converter.ToString(data, offset, param.length);
        }

        //�õ�byte
        public byte GetByte(int paramId)
        {
            Param param = GetParam(paramId);
            CheckType(param, "byte", false);
            return GetByte(param);
        }

        public byte GetByte(Param param)
        {
            int offset = param.GetOffset();
            return data[offset];
        }

        //�õ�int
        public int GetInt(int paramId)
        {
            Param param = GetParam(paramId);
            CheckType(param, "int", false);
            return GetInt(param);
        }

        public int GetInt(Param param)
        {
            int offset = param.GetOffset();
            return converter.ToInt32(data, offset);
        }

        //�õ�uint
        public uint GetUInt(int paramId)
        {
            Param param = GetParam(paramId);
            CheckType(param, "uint", false);
            return GetUInt(param);
        }

        //�õ�uint
        public uint GetUInt(Param param)
        {
            int offset = param.GetOffset();
            return converter.ToUInt32(data, offset);
        }

        //�õ���ʾֵ
        public string GetDisplayItem(Param param)
        {
            int offset = param.GetOffset();
            int val = this.GetIntegerValue(param);
            return param.display.GetItemByValue((uint) val);
        }

        public string GetDisplayItem(int paramId)
        {
            Param param = GetParam(paramId);
            return GetDisplayItem(param);
        }

        //�õ�long
        public long GetLong(int paramId)
        {
            Param param = GetParam(paramId);
            CheckType(param, "long", false);
            return GetLong(param);
        }

        //�õ�long
        public long GetLong(Param param)
        {
            int offset = param.GetOffset();
            return converter.ToInt64(data, offset);
        }

        //�õ�ulong
        public ulong GetULong(int paramId)
        {
            Param param = GetParam(paramId);
            CheckType(param, "ulong", false);
            return GetULong(param);
        }

        //�õ�ulong
        public ulong GetULong(Param param)
        {
            int offset = param.GetOffset();
            return converter.ToUInt64(data, offset);
        }

        //�õ�byte���ݵĵ�offsetλ
        private int GetBit(int value, int offset)
        {
            return (value & (1 << offset)) >> offset;
        }

        private void SetBit(ref byte value, int offset, int bitValue)
        {
            if (bitValue != 0)
                value |= (byte) (1 << offset);
            else
                value &= (byte) (~(1 << offset));
        }

        public int GetBits(int paramId)
        {
            Param param = GetParam(paramId);
            CheckBitType(param);
            return GetBits(param);
        }

        public int GetBits(Param param)
        {
            int offset = param.GetOffset();
            int bitOffset = param.GetBitOffset();
            int result = 0;
            for (int i = 0; i < param.length; i++)
            {
                result += GetBit(data[(bitOffset + i)/8 + offset], (bitOffset + i)%8) << i;
            }
            return result;
        }

        public void SetBitsValue(int paramId, int value)
        {
            Param param = GetParam(paramId);
            CheckBitType(param);
            CheckBitValue(param, value);
            SetBitsValue(param, value);
        }

        public void SetBitsValue(Param param, int value)
        {
            int offset = param.GetOffset();
            int bitOffset = param.GetBitOffset();

            for (int i = 0; i < param.length; i++)
            {
                SetBit(ref data[(bitOffset + i)/8 + offset], (bitOffset + i)%8, GetBit(value, i));
            }
        }

        //�õ�float
        public float GetSingle(int paramId)
        {
            Param param = GetParam(paramId);
            CheckType(param, "float", false);
            return GetSingle(param);
        }

        //�õ�float
        public float GetSingle(Param param)
        {
            int offset = param.GetOffset();
            return converter.ToSingle(data, offset);
        }

        //�õ�byte����
        public byte[] GetBytes(int paramId)
        {
            Param param = GetParam(paramId);
            CheckType(param, "byte", true);
            return GetBytes(param);
        }

        //�õ�byte����
        public byte[] GetBytes(Param param)
        {
            int offset = param.GetOffset();
            int length = Math.Min(param.length, data.Length - offset);
            byte[] result = new byte[length];
            Buffer.BlockCopy(data, offset, result, 0, result.Length);
            return result;
        }

        //�õ�int����
        public int[] GetInts(int paramId)
        {
            Param param = GetParam(paramId);
            CheckType(param, "int", true);
            return GetInts(param);
        }

        //�õ�int����
        public int[] GetInts(Param param)
        {
            int offset = param.GetOffset();
            int typeLength = converter.GetTypeLength(param.type);
            //����ĳ��ȣ���ʼΪ�趨����
            int length = Math.Min(param.length, (data.Length - offset)/typeLength);
            int[] result = new int[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = converter.ToInt32(data, offset);
                offset += typeLength;
            }
            return result;
        }

        //�õ�uint����
        public uint[] GetUInts(int paramId)
        {
            Param param = GetParam(paramId);
            CheckType(param, "uint", true);
            return GetUInts(param);
        }

        //�õ�uint����
        public uint[] GetUInts(Param param)
        {
            int offset = param.GetOffset();
            int typeLength = converter.GetTypeLength(param.type);
            //����ĳ��ȣ���ʼΪ�趨����
            int length = Math.Min(param.length, (data.Length - offset)/typeLength);
            uint[] result = new uint[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = converter.ToUInt32(data, offset);
                offset += typeLength;
            }
            return result;
        }

        //�õ�long����
        public long[] GetLongs(int paramId)
        {
            Param param = GetParam(paramId);
            CheckType(param, "long", true);
            return GetLongs(param);
        }

        //�õ�long����
        public long[] GetLongs(Param param)
        {
            int offset = param.GetOffset();
            int typeLength = converter.GetTypeLength(param.type);
            //����ĳ��ȣ���ʼΪ�趨����
            int length = Math.Min(param.length, (data.Length - offset)/typeLength);
            long[] result = new long[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = converter.ToInt64(data, offset);
                offset += typeLength;
            }
            return result;
        }

        //�õ�ulong����
        public ulong[] GetULongs(int paramId)
        {
            Param param = GetParam(paramId);
            CheckType(param, "ulong", true);
            return GetULongs(param);
        }

        //�õ�ulong����
        public ulong[] GetULongs(Param param)
        {
            int offset = param.GetOffset();
            int typeLength = converter.GetTypeLength(param.type);
            //����ĳ��ȣ���ʼΪ�趨����
            int length = Math.Min(param.length, (data.Length - offset)/typeLength);
            ulong[] result = new ulong[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = converter.ToUInt64(data, offset);
                offset += typeLength;
            }
            return result;
        }

        //�õ�float����
        public float[] GetSingles(int paramId)
        {
            Param param = GetParam(paramId);
            CheckType(param, "float", true);
            return GetSingles(param);
        }

        //�õ�float����
        public float[] GetSingles(Param param)
        {
            int offset = param.GetOffset();
            int typeLength = converter.GetTypeLength(param.type);
            //����ĳ��ȣ���ʼΪ�趨����
            int length = Math.Min(param.length, (data.Length - offset)/typeLength);
            float[] result = new float[length];
            for (int i = 0; i < length; i++)
            {
                if (data.Length - offset < typeLength)
                {
                    break;
                }
                result[i] = converter.ToSingle(data, offset);
                offset += typeLength;
            }
            return result;
        }

        //����byteֵ
        public void SetValue(Param param, string value)
        {
            int offset = param.GetOffset();
            CheckLength(param, value.Length, offset);
            byte[] bytes = converter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
        }


        //����byteֵ
        public void SetValue(Param param, byte value)
        {
            int offset = param.GetOffset();
            data[offset] = value;
        }

        //����intֵ
        public void SetValue(Param param, int value)
        {
            int offset = param.GetOffset();
            byte[] bytes = converter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
        }

        //����uintֵ
        public void SetValue(Param param, uint value)
        {
            int offset = param.GetOffset();
            byte[] bytes = converter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
        }

        //����longֵ
        public void SetValue(Param param, long value)
        {
            int offset = param.GetOffset();
            byte[] bytes = converter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
        }

        //����ulongֵ
        public void SetValue(Param param, ulong value)
        {
            int offset = param.GetOffset();
            byte[] bytes = converter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
        }

        //����floatֵ
        public void SetValue(Param param, float value)
        {
            int offset = param.GetOffset();
            byte[] bytes = converter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
        }

        //����byte����
        public void SetValue(Param param, byte[] value)
        {
            int offset = param.GetOffset();
            CheckLength(param, value.Length, offset);
            Buffer.BlockCopy(value, 0, data, offset, value.Length);
        }

        //����int����
        public void SetValue(Param param, int[] value)
        {
            int offset = param.GetOffset();
            CheckLength(param, value.Length, offset);
            int typeLength = converter.GetTypeLength(param.type);
            for (int i = 0; i < value.Length; i++)
            {
                byte[] bytes = converter.GetBytes(value[i]);
                Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
                offset += typeLength;
            }
        }

        //����uint����
        public void SetValue(Param param, uint[] value)
        {
            int offset = param.GetOffset();
            CheckLength(param, value.Length, offset);
            int typeLength = converter.GetTypeLength(param.type);
            for (int i = 0; i < value.Length; i++)
            {
                byte[] bytes = converter.GetBytes(value[i]);
                Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
                offset += typeLength;
            }
        }

        //����long����
        public void SetValue(Param param, long[] value)
        {
            int offset = param.GetOffset();
            CheckLength(param, value.Length, offset);
            int typeLength = converter.GetTypeLength(param.type);
            for (int i = 0; i < value.Length; i++)
            {
                byte[] bytes = converter.GetBytes(value[i]);
                Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
                offset += typeLength;
            }
        }

        //����ulong����
        public void SetValue(Param param, ulong[] value)
        {
            int offset = param.GetOffset();
            CheckLength(param, value.Length, offset);
            int typeLength = converter.GetTypeLength(param.type);
            for (int i = 0; i < value.Length; i++)
            {
                byte[] bytes = converter.GetBytes(value[i]);
                Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
                offset += typeLength;
            }
        }

        //����float����
        public void SetValue(Param param, float[] value)
        {
            int offset = param.GetOffset();
            CheckLength(param, value.Length, offset);
            int typeLength = converter.GetTypeLength(param.type);
            for (int i = 0; i < value.Length; i++)
            {
                byte[] bytes = converter.GetBytes(value[i]);
                Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
                offset += typeLength;
            }
        }

        //����byteֵ
        public void SetValue(int paramId, string value)
        {
            Param param = GetParam(paramId);
            CheckType(param, "string", true);
            SetValue(param, value);
        }


        //����byteֵ
        public void SetValue(int paramId, byte value)
        {
            Param param = GetParam(paramId);
            CheckType(param, "byte", false);
            SetValue(param, value);
        }

        //����intֵ
        public void SetValue(int paramId, int value)
        {
            Param param = GetParam(paramId);
            CheckType(param, "int", false);
            SetValue(param, value);
        }

        //����uintֵ
        public void SetValue(int paramId, uint value)
        {
            Param param = GetParam(paramId);
            CheckType(param, "uint", false);
            SetValue(param, value);
        }

        //����longֵ
        public void SetValue(int paramId, long value)
        {
            Param param = GetParam(paramId);
            CheckType(param, "long", false);
            SetValue(param, value);
        }

        //����ulongֵ
        public void SetValue(int paramId, ulong value)
        {
            Param param = GetParam(paramId);
            CheckType(param, "ulong", false);
            SetValue(param, value);
        }

        //����floatֵ
        public void SetValue(int paramId, float value)
        {
            Param param = GetParam(paramId);
            CheckType(param, "float", false);
            SetValue(param, value);
        }

        //����byte����
        public void SetValue(int paramId, byte[] value)
        {
            Param param = GetParam(paramId);
            CheckType(param, "byte", true);
            SetValue(param, value);
        }

        //����int����
        public void SetValue(int paramId, int[] value)
        {
            Param param = GetParam(paramId);
            CheckType(param, "int", true);
            SetValue(param, value);
        }

        //����uint����
        public void SetValue(int paramId, uint[] value)
        {
            Param param = GetParam(paramId);
            CheckType(param, "uint", true);
            SetValue(param, value);
        }

        //����long����
        public void SetValue(int paramId, long[] value)
        {
            Param param = GetParam(paramId);
            CheckType(param, "long", true);
            SetValue(param, value);
        }

        //����ulong����
        public void SetValue(int paramId, ulong[] value)
        {
            Param param = GetParam(paramId);
            CheckType(param, "ulong", true);
            SetValue(param, value);
        }

        //����float����
        public void SetValue(int paramId, float[] value)
        {
            Param param = GetParam(paramId);
            CheckType(param, "float", true);
            SetValue(param, value);
        }

        public bool IsFalseData()
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != 255)
                    return false;
            }
            return true;
        }

        public void ResetData()
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = 0;
            }
        }

        public void SetRandomData()
        {
            if (data != null)
            {
                Random rd = new Random();
                rd.NextBytes(data);
            }
        }

        public void SetData(byte[] data)
        {
            this.data = data;
        }

        public void SetObjectValue(int paramId, object value)
        {
            Param param = GetParam(paramId);
            SetObjectValue(param, value);
        }

        public void SetObjectValue(Param param, object value)
        {
            string values = "";
            if (value.GetType() == typeof (byte[]))
            {
                if ((param.type == "byte") && (param.length > 1))
                {
                    SetValue(param, (byte[])value);
                    return;
                }
                 
                StringBuilder sb = new StringBuilder("");
                foreach (byte a in (Array) value)
                {
                    sb.Append(a.ToString("X2") + " ");
                }
                values = sb.ToString().TrimEnd();
            }
            else if (value.GetType().IsArray)
            {
                StringBuilder sb = new StringBuilder("");
                foreach (object a in (Array) value)
                {
                    sb.Append(a.ToString() + ",");
                }
                values = sb.ToString().TrimEnd();
            }
            else
            {
                values = value.ToString();
            }
            SetStringValue(param, values);
        }

        public void SetStringValue(int paramId, string value)
        {
            Param param = GetParam(paramId);
            SetStringValue(param, value);
        }


        public void SetStringValue(Param param, string value)
        {
            switch (param.type)
            {
                case "bit":
                    SetBitsValue(param, StringUtil.ParseInt(value));
                    break;
                case "string":
                    SetValue(param, value);
                    break;
                case "byte":
                    if (param.length > 1)
                    {
                        SetValue(param, StringUtil.ParseBytes(value));
                    }
                    else
                    {
                        SetValue(param, StringUtil.ParseByte(value));
                    }
                    break;
                case "uint":
                    if (param.length > 1)
                    {
                        SetValue(param, StringUtil.ParseUInts(value));
                    }
                    else
                    {
                        SetValue(param, StringUtil.ParseUInt(value));
                    }
                    break;
                case "ulong":
                    if (param.length > 1)
                    {
                        SetValue(param, StringUtil.ParseULongs(value));
                    }
                    else
                    {
                        SetValue(param, StringUtil.ParseULong(value));
                    }
                    break;
                case "int":
                    if (param.length > 1)
                    {
                        SetValue(param, StringUtil.ParseInts(value));
                    }
                    else
                    {
                        SetValue(param, StringUtil.ParseInt(value));
                    }
                    break;
                case "long":
                    if (param.length > 1)
                    {
                        SetValue(param, StringUtil.ParseLongs(value));
                    }
                    else
                    {
                        SetValue(param, StringUtil.ParseLong(value));
                    }
                    break;
                case "float":
                    if (param.length > 1)
                    {
                        SetValue(param, StringUtil.ParseFloats(value));
                    }
                    else
                    {
                        SetValue(param, Single.Parse(value));
                    }
                    break;
                default:
                    throw new NotSupportedException(param.type + " not suported.");
            }
        }

        public int GetIntegerValue(int paramId)
        {
            Param param = GetParam(paramId);
            return GetIntegerValue(param);
        }

        public int GetIntegerValue(Param param)
        {
            switch (param.type)
            {
                case "bit":
                    return this.GetBits(param);
                case "byte":
                    if (param.length > 1)
                    {
                        throw new NotSupportedException(param.type + " length > 1.");
                    }
                    else
                    {
                        return (int) this.GetByte(param);
                    }
                case "uint":
                    if (param.length > 1)
                    {
                        throw new NotSupportedException(param.type + " length > 1.");
                    }
                    else
                    {
                        return (int) this.GetUInt(param);
                    }
                case "ulong":
                    if (param.length > 1)
                    {
                        throw new NotSupportedException(param.type + " length > 1.");
                    }
                    else
                    {
                        return (int) this.GetULong(param);
                    }
                case "int":
                    if (param.length > 1)
                    {
                        throw new NotSupportedException(param.type + " length > 1.");
                    }
                    else
                    {
                        return (int) this.GetInt(param);
                    }

                case "long":
                    if (param.length > 1)
                    {
                        throw new NotSupportedException(param.type + " length > 1.");
                    }
                    else
                    {
                        return (int) this.GetLong(param);
                    }
                default:
                    throw new NotSupportedException(param.type + " not suported.");
            }
        }

        public string GetObjectValue(int paramId)
        {
            Param param = GetParam(paramId);
            return GetObjectValue(param);
        }

        public string GetObjectValue(Param param)
        {
            switch (param.type)
            {
                case "bit":
                    return this.GetBits(param).ToString();
                case "byte":
                    if (param.length > 1)
                    {
                        return StringUtil.BytesToString(this.GetBytes(param));
                    }
                    else
                    {
                        return StringUtil.ByteToString(this.GetByte(param));
                    }
                case "uint":
                    if (param.length > 1)
                    {
                        return StringUtil.UIntsToString(this.GetUInts(param));
                    }
                    else
                    {
                        return StringUtil.UIntToString(this.GetUInt(param));
                    }
                case "ulong":
                    if (param.length > 1)
                    {
                        return StringUtil.ULongsToString(this.GetULongs(param));
                    }
                    else
                    {
                        return StringUtil.ULongToString(this.GetULong(param));
                    }
                case "int":
                    if (param.length > 1)
                    {
                        return StringUtil.IntsToString(this.GetInts(param));
                    }
                    else
                    {
                        return StringUtil.IntToString(this.GetInt(param));
                    }

                case "long":
                    if (param.length > 1)
                    {
                        return StringUtil.LongsToString(this.GetLongs(param));
                    }
                    else
                    {
                        return StringUtil.LongToString(this.GetLong(param));
                    }
                case "float":
                    if (param.length > 1)
                    {
                        return StringUtil.FloatsToString(this.GetSingles(param));
                    }
                    else
                    {
                        return StringUtil.FloatToString(this.GetSingle(param));
                    }
                default:
                    throw new NotSupportedException(param.type + " not suported.");
            }
        }

        public string GetUIntInString(int paramId)
        {
            Param param = GetParam(paramId);
            CheckType(param, "uint", false);
            return GetUIntInString(param);
        }

        public string GetUIntInString(Param param)
        {
            int offset = param.GetOffset();
            uint val = converter.ToUInt32(data, offset);
            if (param.display != null && param.display.items != null && param.display.values != null)
            {
                string v = param.display.GetItemByValue(val);
                if (v == "")
                    return val.ToString();
                else
                    return v;
            }
            return val.ToString();
        }

        public string GetByteInString(int paramId)
        {
            Param param = GetParam(paramId);
            CheckType(param, "byte", false);
            return GetByteInString(param);
        }

        public string GetByteInString(Param param)
        {
            uint val = Convert.ToUInt32(GetByte(param));
            if (param.display != null && param.display.items != null && param.display.values != null)
            {
                string v = param.display.GetItemByValue(val);
                if (v == "")
                    return val.ToString();
                else
                    return v;
            }
            return val.ToString();
        }
    }
}