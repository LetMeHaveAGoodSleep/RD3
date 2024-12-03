using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Fpi.Util.Flash.ExternalInterfaceProxy
{
    /// <summary>
    /// �����ʾ��ActionScript��ActiveX�����ĺ�������
    /// ���к������Ͳ���������
    /// </summary>
    public class ExternalInterfaceCall
    {
        private string _functionName;
        private ArrayList _arguments;

        public ExternalInterfaceCall(string functionName)
        {
            _functionName = functionName;
        }

        /// <summary>
        /// ������
        /// </summary>
        public string FunctionName
        {
            get { return _functionName; }
        }

        /// <summary>
        /// ����
        /// </summary>
        public object[] Arguments
        {
            get { return (object[])_arguments.ToArray(typeof(object)); }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.AppendFormat("Function Name: {0}{1}", _functionName, Environment.NewLine);
            if (_arguments != null && _arguments.Count > 0)
            {
                result.AppendFormat("Arguments:{0}", Environment.NewLine);
                foreach (object arg in _arguments)
                {
                    result.AppendFormat("\t{0}{1}", arg, Environment.NewLine);
                }
            }
            return result.ToString();
        }

        internal void AddArgument(object argument)
        {
            if (_arguments == null)
            {
                _arguments = new ArrayList();
            }
            _arguments.Add(argument);
        }
    }
}
