using System;
using Fpi.Util.Exeptions;

namespace Fpi.Instruments
{
    public class InstrumentException : PlatformException
    {
        public InstrumentException()
            : base()
        {
        }

        public InstrumentException(String message)
            : base(message)
        {
        }

        public InstrumentException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}