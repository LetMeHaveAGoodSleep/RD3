namespace Fpi.Util.Interfaces
{
    public interface ICondition
    {
        /// <summary>
        /// 条件的详细描述
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 执行判断
        /// </summary>
        /// <param name="conditionStr">输入条件字符串</param>
        /// <returns>返回判断结果</returns>
        bool Judge(string conditionStr);
    }
}