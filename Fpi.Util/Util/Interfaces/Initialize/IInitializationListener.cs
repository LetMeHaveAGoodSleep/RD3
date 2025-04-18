namespace Fpi.Util.Interfaces.Initialize
{
    public interface IInitializationListener
    {
        void OnInitException(object source, InitException ex, bool fatal);
        void BeforeInit(object source);
        void AfterInit(object source);
    }
}