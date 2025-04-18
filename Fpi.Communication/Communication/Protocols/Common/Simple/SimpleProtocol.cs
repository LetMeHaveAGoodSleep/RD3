using System;
using System.Collections.Generic;
using System.Text;
using Fpi.Communication.Ports.CommPorts;
using Fpi.Communication.Interfaces;

namespace Fpi.Communication.Protocols.Common.Simple
{
    public class SimpleProtocol : Protocol
    {
        public override string FriendlyName
        {
            get { return "ºÚµ•–≠“È"; }
        }

        protected override Parser ConstructParser()
        {
            return new SimpleParser();
        }
    }
}
