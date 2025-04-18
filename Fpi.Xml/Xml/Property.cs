using System;
namespace Fpi.Xml
{
    /// <summary>
    /// Property 的摘要说明。
    /// </summary>
    [Serializable]
    public class Property : IdNameNode
    {
        public string value;

        public Property()
            : base()
        {
        }

        public Property(string id)
            : base(id)
        {
        }

        public Property(string id, string name)
            : base(id, name)
        {
        }
        public Property(string id, string name, string value)
            : base(id, name)
        {
            this.value = value;
        }
    }
}