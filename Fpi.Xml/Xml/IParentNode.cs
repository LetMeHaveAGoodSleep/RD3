namespace Fpi.Xml
{
    /// <summary>
    /// IParentNode ��ժҪ˵����
    /// </summary>
    public interface IParentNode
    {
        object GetParentNode();
        void SetParentNode(object parentNode);
    }
}