using System.Collections;
using Fpi.Util.Sundry;
using Fpi.Communication.Converter;
using Fpi.Xml;

namespace Fpi.Communication.Commands.Config
{
    /// <summary>
    /// CommandExtend ��ժҪ˵����
    /// </summary>
    public class CommandExtend : IdNameNode
    {
        public int timeOut;
        public int tryTimes;
        public NodeList parameters;
        public ArrayList rules;
        public bool convertToZero;

        public ArrayList sections;

        public CommandExtend()
        {
            //
            // TODO: �ڴ˴���ӹ��캯���߼�
            //
        }

        public int GetCommandExtendId()
        {
            return StringUtil.ParseInt(id);
        }

        public int GetParamLength()
        {
            if (parameters == null)
            {
                return 0;
            }
            int sum = 0;
            foreach (Param param in parameters)
            {
                sum += DataConverter.GetInstance().GetTypeLength(param.type)*param.length;
            }
            return sum;
        }
    }
}