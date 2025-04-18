namespace Fpi.Communication.Converter
{
    /// <summary>
    ///常用数据类型和字节数组转换接口
    /// </summary>
    public interface IDataConvertable
    {
        //得到类型字节数
        int GetTypeLength(string typeName);

        int ToInt32(byte[] value, int startIndex);
        uint ToUInt32(byte[] value, int startIndex);
        long ToInt64(byte[] value, int startIndex);
        ulong ToUInt64(byte[] value, int startIndex);
        float ToSingle(byte[] value, int startIndex);
        string ToString(byte[] value, int startIndex, int length);

        byte[] GetBytes(int value);
        byte[] GetBytes(uint value);
        byte[] GetBytes(long value);
        byte[] GetBytes(ulong value);
        byte[] GetBytes(float value);
        byte[] GetBytes(string value);
    }
}