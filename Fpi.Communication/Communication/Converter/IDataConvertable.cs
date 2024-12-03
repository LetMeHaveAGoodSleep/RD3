namespace Fpi.Communication.Converter
{
    /// <summary>
    ///�����������ͺ��ֽ�����ת���ӿ�
    /// </summary>
    public interface IDataConvertable
    {
        //�õ������ֽ���
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