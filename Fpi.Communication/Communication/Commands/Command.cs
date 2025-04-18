using Fpi.Communication.Interfaces;
namespace Fpi.Communication.Commands
{
    public abstract class Command : IByteStream
    {
        protected string cmdId;
        protected int cmdCode;
        protected int extCode;
        protected ParametersData parametersData;

        protected string exceptionMsg;
        virtual public string ExceptionMsg
        {
            get { return exceptionMsg; }
            set { exceptionMsg = value; }
        }



        public string GetCmdId()
        {
            return cmdId;
        }

        public int GetCmdCode()
        {
            return this.cmdCode;
        }

        public int GetExtCode()
        {
            return this.extCode;
        }

        public abstract byte[] GetBytes();

        public byte[] GetParamData()
        {
            if (parametersData == null)
            {
                return null;
            }
            else
            {
                return parametersData.GetData();
            }
        }

        public void SetParamData(byte[] data)
        {
            if (parametersData != null)
            {
                parametersData.SetData(data);
            }
        }
    }
}