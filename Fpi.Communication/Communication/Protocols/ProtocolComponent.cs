using Fpi.Communication;
using Fpi.Communication.Manager;
using Fpi.Communication.Interfaces;
using Fpi.Communication.Protocols.Interfaces;

namespace Fpi.Communication.Protocols
{
    public abstract class ProtocolComponent : ISupportPipe, IProtocolComponent
    {
        public ProtocolComponent()
        {
        }

        #region IProtocolComponent ��Ա

        protected Protocol owner;

        public Protocol Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        #endregion

        #region ISupportPipe ��Ա

        protected Pipe pipe;

        public Pipe Pipe
        {
            get { return pipe; }
            set
            {
                ActionPipe(value);
                pipe = value;
            }
        }

        protected virtual void ActionPipe(Pipe pipe)
        {
        }

        #endregion
    }
}