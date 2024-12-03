using System;
using System.Xml;
using Fpi.Util.Interfaces.Initialize;
using Fpi.Util.Reflection;
using Fpi.Xml;

namespace Fpi.Assembly
{
    public class InitMember : IdNameNode
    {
        public string description;
        public bool active;
        public string ownerDLL;
        public bool initExceptionIsFatal;

        public InitMember() : base()
        {
        }

        public InitMember(string id) : base(id)
        {
        }

        public InitMember(string id, string name) : base(id, name)
        {
            active = true;
        }

        public override BaseNode Init(XmlNode node)
        {
            active = true;
            return base.Init(node);
        }


        private bool existed = false;

        public bool Existed
        {
            get { return this.existed; }
            set { this.existed = value; }
        }

        public IInitialization GetInitInstance()
        {
            try
            {
                IInitialization rv = (IInitialization) ReflectionHelper.CreateInstance(id);
                return rv;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(Resources.BuildException,id), ex);
            }
        }
    }
}