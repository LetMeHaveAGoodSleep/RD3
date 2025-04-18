using System;
using System.Collections;
using Fpi.Util.Sundry;
using Fpi.Xml;

namespace Fpi.Instruments
{
    public class InstrumentManager : BaseNode
    {
        public NodeList instrumentTypes = new NodeList();
        public NodeList instruments = new NodeList();

        private InstrumentManager()
        {
            loadXml();
        }

        private static object syncObj = new object();
        private static InstrumentManager instance = null;
        public static InstrumentManager GetInstance()
        {
            lock (syncObj)
            {
                if (instance == null)
                {
                    instance = new InstrumentManager();
                }
            }
            return instance;
        }
        public static void ReLoad()
        {
            lock (syncObj)
            {
                instance = null;
            }
        }

        public Instrument[] GetInstruments(string type)
        {
            ArrayList list = new ArrayList();
            foreach (Instrument ins in instruments)
            {
                if (ins.type.Equals(type))
                {
                    list.Add(ins);
                }
            }
            
            return (Instrument[]) list.ToArray(typeof (Instrument));
        }

        public Instrument GetInstrument(int address)
        {
            int count = instruments.GetCount();
            for (int i = 0; i < count; i++)
            {
                Instrument ins = (Instrument) instruments[i];
                if (ins.address == address)
                {
                    return ins;
                }
            }
            return null;
        }

        public Instrument GetInstrument(string insId)
        {
            if (this.instruments != null)
            {
                return this.instruments[insId] as Instrument;
            }
            return null;
        }

        public InstrumentType GetInstrumentType(string insId)
        {
            if (this.instruments != null)
            {
                Instrument ins = this.instruments[insId] as Instrument;
                if (this.instrumentTypes != null && ins != null)
                    return this.instrumentTypes[ins.type] as InstrumentType;
            }
            return null;
        }
    }
}