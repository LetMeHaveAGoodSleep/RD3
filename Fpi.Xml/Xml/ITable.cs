namespace Fpi.Xml
{
    public interface ITable
    {
        bool Add(object newRecord);
        bool Remove(object existRecord);
        bool Update(object oldRecord, object newRecord);

        int GetIndex(object record);
        object GetRecord(int index);

        void Sort();
        void Reverse();

        void Clear();
        void Dispose();
    }
}