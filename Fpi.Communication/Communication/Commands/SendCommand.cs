using System;
using System.Collections;
using Fpi.Communication.Commands.Config;
using Fpi.Util.Sundry;
using Fpi.Communication.Converter;
using Fpi.Xml;
using Fpi.Communication.Interfaces;

namespace Fpi.Communication.Commands
{
    public class SendCommand : Command, IOvertime
    {
        public SendCommand(string cmdId, int extCode)
        {
            this.cmdId = cmdId;
            this.extCode = extCode;
        }

        public override string ExceptionMsg
        {
            set
            {
                exceptionMsg = value;
            }
        }

        //参数表
        private Hashtable paramTable = new Hashtable();

        //参数是否有效表
        private Hashtable validTable = new Hashtable();

        protected CommandDesc commandDesc = null;

        public CommandDesc CommandDesc
        {
            get { return commandDesc; }
        }

        //转换器，给组装数据长度时用 add by zhangyq 2008.6.18.
        protected IDataConvertable converter;

        public override byte[] GetBytes()
        {
            byte[] paraData = parametersData.GetData();
            byte[] result = new byte[4 + paraData.Length];
            result[0] = (byte) cmdCode;
            result[1] = (byte) extCode;
            byte[] length = converter.GetBytes((uint) paraData.Length);
            //byte[] length = DataConverter.GetInstance().GetBytes((uint)paraData.Length);
            result[2] = length[0];
            result[3] = length[1];
            Buffer.BlockCopy(paraData, 0, result, 4, paraData.Length);
            return result;
        }

        //设置参数描述，接收到命令后调用
        public void BuildData(CommandDesc commandDesc, NodeList parameters, IDataConvertable converter,
                              int paramIdPrefix)
        {
            this.commandDesc = commandDesc;
            this.cmdCode = commandDesc.commandCode;
            //转换器，给组装数据长度时用 add by zhangyq 2008.6.18
            this.converter = converter;
            parametersData = new ParametersData(parameters, converter, paramIdPrefix, null);

            //指令异常信息处理 add by DRH 2009.06.24
            if ((extCode == CommandExtendId.ERROR_CODE) || (extCode == CommandExtendId.NOT_SUPPORTED_EXT_CODE))
            {
                if (_data == null)
                {
                    if (exceptionMsg == null)
                    {
                        exceptionMsg = string.Empty;
                    }

                    _data = converter.GetBytes(exceptionMsg);
                }               
                parametersData.SetData(_data);
                return;
            }

            //如果数据清空
            if (_resetData)
            {
                parametersData.ResetData();
            }
                //如果是随机数据
            else if (_randomData)
            {
                parametersData.SetRandomData();
            }
                //直接设置数据
            else if (_data != null)
            {
                parametersData.SetData(_data);
            }
            else
            {
                //设置参数是否有效
                foreach (int paramId in validTable.Keys)
                {
                    bool valid = (bool) validTable[paramId];
                    parametersData.SetParamValid(paramId, valid);
                }

                if (parameters != null)
                {
                    for (int i = 0; i < parameters.GetCount(); i++)
                    {
                        int paramId = (paramIdPrefix << 8) + i;
                        Param param = (Param) parameters[i];
                        bool valid = true;
                        object validSetting = validTable[paramId];
                        if (validSetting != null)
                        {
                            valid = (bool) validSetting;
                        }
                        //参数有效
                        if (valid)
                        {
                            object val = paramTable[paramId];
                            if (val == null)
                            {
                                val = paramTable[param];
                            }
                            if (val != null)
                            {
                                parametersData.SetObjectValue(param, val);
                            }
                                //是否有缺省值
                            else
                            {
                                //如果有缺省值
                                if ((param.display != null) && (param.display.defaultValue != null))
                                {
                                    parametersData.SetObjectValue(param, param.display.defaultValue);
                                }
                                else
                                {
                                    throw new CommandException("param value not setting: " + param.name);
                                }
                            }
                        }
                    }
                }
            }

            //末尾数据截断
            if (_endTrimData >= 0)
            {
                parametersData.SetDataByEndTrim(_endTrimData);
            }

            _data = parametersData.GetData();
        }

        //设置参数值
        public SendCommand SetValue(int paramId, object value)
        {
            paramTable[paramId] = value;
            return this;
        }

        public SendCommand SetValue(Param param, object value)
        {
            paramTable[param] = value;
            return this;
        }

        //设置参数是否有效，必须在调用SetValue之前调用
        public SendCommand SetParamValid(int paramId, bool valid)
        {
            validTable[paramId] = valid;
            return this;
        }

        private bool _resetData = false;
        private bool _randomData = false;
        private int _endTrimData = -1;
        private byte[] _data = null;

        //数据初始化
        public void ResetData()
        {
            _resetData = true;
        }

        //设置随机数
        public void SetRandomData()
        {
            _randomData = true;
        }

        //设置数据
        public void SetData(byte[] data)
        {
            _data = data;
        }

        public byte[] GetData()
        {
            return _data;
        }
        public byte[] GetData(int stratIndex, int dataLength)
        {
            byte[] res = new byte[dataLength];
            Buffer.BlockCopy(_data, stratIndex, res, 0, res.Length);
            return res;
        }

        //截断数据
        public void SetDataByEndTrim(int validDataLength)
        {
            _endTrimData = validDataLength;
        }

        public void SetBitsValue(int paramId, int value)
        {
            paramTable[paramId] = value;
        }

        public void SetBitsValue(Param param, int value)
        {
            paramTable[param] = value;
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

        #region ITimeOut 成员

        private int _timeOut = int.MinValue;
        private int _tryTimes = int.MinValue;

        public int TimeOut
        {
            get
            {
                if (_timeOut != int.MinValue)
                {
                    return _timeOut;
                }
                else
                {
                    if (this.commandDesc != null)
                    {
                        _timeOut = this.commandDesc.timeOut;
                    }

                    if (_timeOut == int.MinValue)
                    {
                        return 0;
                    }

                    return _timeOut;
                }
            }
            set { _timeOut = value; }
        }

        public int TryTimes
        {
            get
            {
                if (_tryTimes != int.MinValue)
                {
                    return _tryTimes;
                }
                else
                {
                    if (this.commandDesc != null)
                    {
                        _tryTimes = this.commandDesc.tryTimes;
                    }

                    if (_tryTimes == int.MinValue)
                    {
                        return 0;
                    }

                    return _tryTimes;
                }
            }

            set { _tryTimes = value; }
        }

        #endregion
    }
}