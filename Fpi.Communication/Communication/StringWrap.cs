using System.Text;
using Fpi.Communication.Interfaces;

namespace Fpi.Communication
{
    public class StringWrap : IByteStream
    {
        private string data;
        private static Encoding encoding = Encoding.UTF8;

        public StringWrap(string data)
        {
            this.data = data;
        }

        public StringWrap(byte[] data)
        {
            this.data = encoding.GetString(data, 0, data.Length);
        }

        public static StringWrap Build(byte[] data)
        {
            return new StringWrap(encoding.GetString(data, 0, data.Length));
        }

        public byte[] GetBytes()
        {
            return encoding.GetBytes(data);
        }

        public string GetData()
        {
            return this.data;
        }

        public override string ToString()
        {
            return data;
        }
    }
}