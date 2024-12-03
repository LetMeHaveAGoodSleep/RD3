using System.Collections;

namespace Fpi.Communication.Protocols
{
    public class ProtocolComparer : IComparer
    {
        private static object syncObj = new object();
        private static ProtocolComparer instance = null;

        public static ProtocolComparer GetInstance()
        {
            lock (syncObj)
            {
                if (instance == null)
                {
                    instance = new ProtocolComparer();
                }
            }
            return instance;
        }

        private ProtocolComparer()
        {
        }

        #region IComparer 成员

        public int Compare(object x, object y)
        {
            if ((x is Protocol) && (y is Protocol))
            {
                Protocol a = (Protocol) x;
                Protocol b = (Protocol) y;

                //if (a.FriendlyName.StartsWith("[聚光科技] ") && !b.FriendlyName.StartsWith("[聚光科技] "))
                //{
                //    return -1;
                //}

                //if (!a.FriendlyName.StartsWith("[聚光科技] ") && b.FriendlyName.StartsWith("[聚光科技] "))
                //{
                //    return 1;
                //}


                return a.FriendlyName.CompareTo(b.FriendlyName);
            }
            return 0;
        }

        #endregion
    }
}