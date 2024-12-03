using Fpi.Util.Sundry;
using Fpi.Xml;

namespace Fpi.Instruments
{
    public class Instrument : IdNameNode
    {
        public string type;
        public string router;
        public int address = 1;
        public string desc;

        public Instrument()
            : base()
        {
        }

        public Instrument(string id)
            : base(id)
        {
        }

        public Instrument(string id, string name)
            : base(id, name)
        {
        }

        InstrumentType _InstrumentType;
        /// <summary>
        /// 仪器类型
        /// </summary>
        public InstrumentType InstrumentType
        {
            get 
            {
                //if (_InstrumentType == null)
                {
                    _InstrumentType = GetInstrumentType();
                }
                return _InstrumentType; 
            }
        }
        private InstrumentType GetInstrumentType()
        {
            InstrumentType it = null;
            if (InstrumentManager.GetInstance().instrumentTypes != null &&
                InstrumentManager.GetInstance().instrumentTypes.GetCount() > 0)
            {
                it = (InstrumentType)InstrumentManager.GetInstance().instrumentTypes[type];
            }
            return it;
        }


        Instrument _Router;
        /// <summary>
        /// 路由设备
        /// </summary>
        public Instrument Router
        {
            get 
            {
                //if (_Router == null)
                {
                    _Router = GetRouter();
                }
                return _Router; 
            }
        }
        private Instrument GetRouter()
        {
            Instrument ins = null;
            if (InstrumentManager.GetInstance().instruments != null &&
                InstrumentManager.GetInstance().instruments.GetCount() > 0)
            {
                ins = (Instrument)InstrumentManager.GetInstance().instruments[router];
            }
            return ins;
        }



    }
}