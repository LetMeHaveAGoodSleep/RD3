namespace Fpi.Communication.Commands.Config
{
    public class SpecCommandManager : CommandManager
    {
        private SpecCommandManager()
            : base()
        {
        }

        private static object syncObj = new object();
        private static SpecCommandManager instance = null;

        public new static SpecCommandManager GetInstance()
        {
            lock (syncObj)
            {
                if (instance == null)
                {
                    instance = new SpecCommandManager();
                }
            }
            return instance;
        }
    }
}