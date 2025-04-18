using Fpi.Communication.Interfaces;
using Fpi.Util.Sundry;

namespace Fpi.Communication
{
    public class ByteArrayWrap : IByteStream
    {
        private byte[] data;

        public ByteArrayWrap(byte[] data)
        {
            this.data = data;
        }

        public static ByteArrayWrap Build(byte[] data)
        {
            return new ByteArrayWrap(data);
        }

        public byte[] GetBytes()
        {
            return data;
        }

        public override string ToString()
        {
            return StringUtil.BytesToString(data);
        }
    }
}