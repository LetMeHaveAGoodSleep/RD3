namespace Fpi.Xml
{
    /// <summary>
    /// IParentNode 的摘要说明。
    /// </summary>
    public interface IParentNode
    {
        object GetParentNode();
        void SetParentNode(object parentNode);
    }
}