namespace Fpi.Communication.Commands.Config
{
    public class WebCommandManager : CommandManager
    {
        private WebCommandManager()
            : base()
        {
        }

        private static object syncObj = new object();
        private static WebCommandManager instance = null;

        public new static WebCommandManager GetInstance()
        {
            lock (syncObj)
            {
                if (instance == null)
                {
                    instance = new WebCommandManager();
                }
            }
            return instance;
        }
    }
}