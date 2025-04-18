namespace Fpi.Communication.Interfaces
{
    public interface IConnector
    {
        bool Open();

        bool Close();

        bool Connected { get; }
    }
}