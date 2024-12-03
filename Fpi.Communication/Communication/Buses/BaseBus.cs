using System;
using Fpi.Communication.Manager;
using Fpi.Util.Interfaces;
using Fpi.Xml;
using Fpi.Communication.Interfaces;


namespace Fpi.Communication.Buses
{
    public abstract class BaseBus : IBus, IDisposable, INaming
    {
        protected BaseNode config;

        public BaseBus()
        {
        }

        public string GetProperty(string id)
        {
            return GetProperty(id, null);            
        }

        public string GetProperty(string id, string defaultValue)
        {
            if (config == null)
            {
                return defaultValue;
            }
            else
            {
                return config.GetPropertyValue(id, defaultValue);
            }
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(FriendlyName))
            {
                return FriendlyName;
            }
            return this.GetType().Name;
        }

        #region IBus 成员

        public virtual void Init(BaseNode config)
        {
            this.config = config;
        }

        public abstract bool Write(byte[] buf);

        public abstract bool Read(byte[] buf, int count, ref int bytesread);


        #endregion

        #region IConnector 成员

        public abstract bool Open();

        public abstract bool Close();

        protected bool connected;

        public virtual bool Connected
        {
            get { return connected; }
        }

        #endregion

        #region IDisposable 成员

        virtual public void Dispose()
        {
            if (connected)
            {
                Close();
            }
        }

        #endregion

        #region INaming 成员

        virtual public string FriendlyName
        {
            get { return GetType().Name; }
        }
        virtual public string InstanceName
        {
            get { return string.Empty; }
        }
        #endregion

    }
}