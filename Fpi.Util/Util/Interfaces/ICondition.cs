namespace Fpi.Util.Interfaces
{
    public interface ICondition
    {
        /// <summary>
        /// ��������ϸ����
        /// </summary>
        string Description { get; }

        /// <summary>
        /// ִ���ж�
        /// </summary>
        /// <param name="conditionStr">���������ַ���</param>
        /// <returns>�����жϽ��</returns>
        bool Judge(string conditionStr);
    }
}