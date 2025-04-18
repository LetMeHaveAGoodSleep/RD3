namespace Fpi.Communication.Interfaces
{
    public interface IOvertime
    {
        int TimeOut { get; set; }

        int TryTimes { get; set; }
    }
}