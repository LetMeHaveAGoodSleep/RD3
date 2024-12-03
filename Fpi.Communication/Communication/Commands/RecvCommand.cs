using System;
using Fpi.Communication.Commands.Config;
using Fpi.Util.Sundry;
using Fpi.Communication.Converter;
using Fpi.Xml;

namespace Fpi.Communication.Commands
{
    public class RecvCommand : Command
    {
        protected byte[] cmdData;
        protected int paramIdPrefix;


        public RecvCommand(byte[] data)
        {
            Init(data);
        }

        public override string ExceptionMsg
        {
            get
            {
                return exceptionMsg;
            }
        }

        protected virtual void Init(byte[] data)
        {
            cmdCode = (int) data[0];
            extCode = (int) data[1];
            byte[] paraData = new byte[data.Length - 4];
            Buffer.BlockCopy(data, 4, paraData, 0, paraData.Length);
            cmdData = paraData;
        }

        public override byte[] GetBytes()
        {
            return cmdData;
        }

        //���ò������������յ���������
        public void Init(string cmdId, NodeList parameters, IDataConvertable converter, int paramIdPrefix)
        {
            this.cmdId = cmdId;
            this.paramIdPrefix = paramIdPrefix;

            //ָ���쳣��Ϣ���� add by DRH 2009.06.24
            if (extCode == CommandExtendId.ERROR_CODE)
            {
                exceptionMsg = converter.ToString(cmdData, 0, cmdData.Length);
            }
            else
            {
                parametersData = new ParametersData(parameters, converter, paramIdPrefix, cmdData);
            }
        }

        public byte[] GetData()
        {
            return parametersData.GetData();
        }

        public byte[] GetData(int stratIndex, int dataLength)
        {
            return parametersData.GetData(stratIndex, dataLength);
        }

        public byte GetData(int index)
        {
            return parametersData.GetData(index);
        }

        //���ݲ���id���õ�����
        public Param GetParam(int paramId)
        {
            return this.parametersData.GetParam(paramId);
        }

        public int GetParamId(int number)
        {
            return (paramIdPrefix << 8) + number;
        }

        public string GetParamName(int paramId)
        {
            return this.parametersData.GetParam(paramId).name;
        }

        //�ж��Ƿ�����Ĳ���
        public int GetParamCount()
        {
            return parametersData.GetParamCount();
        }

        //�õ�string
        public string GetString(int paramId)
        {
            return parametersData.GetString(paramId);
        }

        public string GetString(Param param)
        {
            return parametersData.GetString(param);
        }

        //�õ�byte
        public byte GetByte(int paramId)
        {
            return parametersData.GetByte(paramId);
        }

        public byte GetByte(Param param)
        {
            return parametersData.GetByte(param);
        }

        //�õ�int
        public int GetInt(int paramId)
        {
            return parametersData.GetInt(paramId);
        }

        public int GetInt(Param param)
        {
            return parametersData.GetInt(param);
        }

        //�õ�uint
        public uint GetUInt(int paramId)
        {
            return parametersData.GetUInt(paramId);
        }

        //�õ�uint
        public uint GetUInt(Param param)
        {
            return parametersData.GetUInt(param);
        }

        //�õ�long
        public long GetLong(int paramId)
        {
            return parametersData.GetLong(paramId);
        }

        //�õ�long
        public long GetLong(Param param)
        {
            return parametersData.GetLong(param);
        }

        //�õ�ulong
        public ulong GetULong(int paramId)
        {
            return parametersData.GetULong(paramId);
        }

        //�õ�ulong
        public ulong GetULong(Param param)
        {
            return parametersData.GetULong(param);
        }

        public int GetBits(int paramId)
        {
            return parametersData.GetBits(paramId);
        }

        public int GetBits(Param param)
        {
            return parametersData.GetBits(param);
        }

        //�õ�float
        public float GetSingle(int paramId)
        {
            return parametersData.GetSingle(paramId);
        }

        //�õ�float
        public float GetSingle(Param param)
        {
            return parametersData.GetSingle(param);
        }

        //�õ�byte����
        public byte[] GetBytes(int paramId)
        {
            return parametersData.GetBytes(paramId);
        }

        //�õ�byte����
        public byte[] GetBytes(Param param)
        {
            return parametersData.GetBytes(param);
        }

        //�õ�int����
        public int[] GetInts(int paramId)
        {
            return parametersData.GetInts(paramId);
        }

        //�õ�int����
        public int[] GetInts(Param param)
        {
            return parametersData.GetInts(param);
        }

        //�õ�uint����
        public uint[] GetUInts(int paramId)
        {
            return parametersData.GetUInts(paramId);
        }

        //�õ�uint����
        public uint[] GetUInts(Param param)
        {
            return parametersData.GetUInts(param);
        }

        //�õ�long����
        public long[] GetLongs(int paramId)
        {
            return parametersData.GetLongs(paramId);
        }

        //�õ�long����
        public long[] GetLongs(Param param)
        {
            return parametersData.GetLongs(param);
        }

        //�õ�ulong����
        public ulong[] GetULongs(int paramId)
        {
            return parametersData.GetULongs(paramId);
        }

        //�õ�ulong����
        public ulong[] GetULongs(Param param)
        {
            return parametersData.GetULongs(param);
        }

        //�õ�float����
        public float[] GetSingles(int paramId)
        {
            return parametersData.GetSingles(paramId);
        }

        //�õ�float����
        public float[] GetSingles(Param param)
        {
            return parametersData.GetSingles(param);
        }

        public int GetIntegerValue(int paramId)
        {
            return parametersData.GetIntegerValue(paramId);
        }

        //����bit��byte��int��uint��long��ulong
        public int GetIntegerValue(Param param)
        {
            return parametersData.GetIntegerValue(param);
        }

        public string GetObjectValue(int paramId)
        {
            return parametersData.GetObjectValue(paramId);
        }

        public string GetObjectValue(Param param)
        {
            return parametersData.GetObjectValue(param);
        }

        public bool IsFalseData()
        {
            return parametersData.IsFalseData();
        }

        public void ResetData()
        {
            parametersData.ResetData();
        }

        public int GetParamLength(Param param)
        {
            return this.parametersData.GetParamLength(param);
        }

        public int GetParamLength(int number)
        {
            return this.parametersData.GetParamLength(number);
        }

        public int GetDataLength()
        {
            byte[] data = this.parametersData.GetData();
            return data.Length;
        }

        public byte[] GetDataByEndTrim(int validDataLength)
        {
            return parametersData.GetDataByEndTrim(validDataLength);
        }

        //�õ�value
        public string GetValue(int paramId)
        {
            Param param = GetParam(paramId);
            switch (param.type)
            {
                case "bit":
                    return this.GetBits(paramId).ToString();
                case "string":
                    return this.GetString(paramId);
                case "byte":
                    if (param.length > 1)
                        return StringUtil.BytesToString(GetBytes(paramId));
                    else
                        return StringUtil.ByteToString(GetByte(paramId));
                case "uint":
                    if (param.length > 1)
                        return StringUtil.UIntsToString(GetUInts(paramId));
                    else
                        return this.GetUInt(paramId).ToString();
                case "ulong":
                    if (param.length > 1)
                        return StringUtil.ULongsToString(GetULongs(paramId));
                    else
                        return this.GetULong(paramId).ToString();
                case "int":
                    if (param.length > 1)
                        return StringUtil.IntsToString(GetInts(paramId));
                    else
                        return this.GetInt(paramId).ToString();
                case "long":
                    if (param.length > 1)
                        return StringUtil.LongsToString(GetLongs(paramId));
                    else
                        return this.GetLong(paramId).ToString();
                case "float":
                    if (param.length > 1)
                        return StringUtil.FloatsToString(GetSingles(paramId));
                    else
                        return StringUtil.FloatToString(this.GetSingle(paramId));
                default:
                    throw new XmlException(param.type + "type not be supported");
            }
        }

        public string GetDisplayItem(int paramId)
        {
            return parametersData.GetDisplayItem(paramId);
        }

        public string GetDisplayItem(Param param)
        {
            return parametersData.GetDisplayItem(param);
        }


        public string GetByteInString(int paramId)
        {
            return parametersData.GetByteInString(paramId);
        }

        public string GetByteInString(Param param)
        {
            return parametersData.GetByteInString(param);
        }


        public string GetUIntInString(int paramId)
        {
            return parametersData.GetUIntInString(paramId);
        }

        public string GetUIntInString(Param param)
        {
            return parametersData.GetUIntInString(param);
        }

        public override string ToString()
        {
            try
            {
                byte[] data = GetBytes();
                return StringUtil.BytesToString(data);
            }
            catch
            {
                return cmdId;
            }
        }
    }
}