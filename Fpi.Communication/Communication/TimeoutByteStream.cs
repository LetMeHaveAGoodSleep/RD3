using Fpi.Communication.Interfaces;
namespace Fpi.Communication
{
    public class TimeoutByteStream : IByteStream, IOvertime
    {
        private IByteStream data;
        private int timeOut;
        private int tryTimes;

        public TimeoutByteStream(IByteStream data, int timeOut, int tryTimes)
        {
            this.data = data;
            this.timeOut = timeOut;
            this.tryTimes = tryTimes;
        }

        public TimeoutByteStream(IByteStream data, IOvertime overtime)
            : this(data, overtime.TimeOut, overtime.TryTimes)
        {
        }

        #region ITimeOut 成员

        public int TimeOut
        {
            get { return this.timeOut; }
            set { this.timeOut = value; }
        }

        public int TryTimes
        {
            get { return this.tryTimes; }
            set { this.tryTimes = value; }
        }

        #endregion

        #region IByteStream 成员

        public byte[] GetBytes()
        {
            return data.GetBytes();
        }

        #endregion
    }
}