using Fpi.Util.Interfaces.Initialize;

namespace Fpi.Xml
{
    /// <summary>
    /// 属性树数据结构，可以替代Var.xml存储配置信息
    /// </summary>
    public class ParamManager : Property, IInitialization
    {
        private ParamManager()
        {
            loadXml();
        }

        private static readonly object syncObj = new object();
        private static ParamManager _instance;
        public static ParamManager GetInstance()
        {
            lock (syncObj)
            {
                if (_instance == null)
                {
                    _instance = new ParamManager();
                }
            }
            return _instance;
        }

        public void Reload()
        {
            _instance = null;
        }

        public Property GetTargetProperty(string proId)
        {
            if (this.propertys == null || this.propertys.GetCount() == 0)
                return null;

            return GetProperty(this, proId);
        }

        public string GetTargetPropertyValue(string proId)
        {
            if (this.propertys == null || this.propertys.GetCount() == 0)
                return null;

            Property p = GetProperty(this, proId);
            if (p != null)
                return p.value;
            return null;
        }

        public void SetTargetPropertyValue(string proId, string value)
        {
            if (this.propertys == null || this.propertys.GetCount() == 0)
                return;
            Property p = GetProperty(this, proId);
            if (p != null)
            {
                p.value = value;
            }
            else
            {
                this.SetProperty(proId, value);
            }
        }


        //递归搜索
        private Property GetProperty(Property node, string proId)
        {
            if (node == null)
                return null;
            if (node.id == proId)
                return node;

            if (node.propertys == null || node.propertys.GetCount() == 0)
                return null;

            foreach (Property pro in node.propertys)
            {
                Property n = GetProperty(pro, proId);
                if (n != null)
                    return n;
            }

            return null;

        }

        #region IInitialization 成员

        public void Initialize()
        {

        }

        #endregion
    }
}
