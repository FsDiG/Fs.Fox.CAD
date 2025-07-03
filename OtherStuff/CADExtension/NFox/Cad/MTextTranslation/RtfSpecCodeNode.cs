namespace NFox.Cad
{
    /// <summary>
    /// 说明性文本类型枚举
    /// </summary>
    public enum RtfSpecCodeNodeType
    {
        /// <summary>
        /// 换行
        /// </summary>
        Newline,             //P
        /// <summary>
        /// 空白
        /// </summary>
        Space,            //~
        /// <summary>
        /// 左大括号
        /// </summary>
        LeftBrace,            //{
        /// <summary>
        /// 右大括号
        /// </summary>
        RightBrace,     //}
    }
    /// <summary>
    /// 说明性文本节点
    /// </summary>
    public class RtfSpecCodeNode : RtfNode
    {
        private readonly static string[] _typeCodes =
            new string[] { "P", "~", "{", "}" };

        private readonly static string[] _specCodes =
            new string[] { "\n", " ", "{", "}" };
        /// <summary>
        /// 初始化文本节点
        /// </summary>
        /// <param name="nodeType">文本节点类型</param>
        public RtfSpecCodeNode(RtfSpecCodeNodeType nodeType)
        {
            _nodeClassType = RtfNodeClassType.SpecCode;
            _key = (int)nodeType;
        }
        /// <summary>
        /// 获取文本内容
        /// </summary>
        public override string Contents => $"\\{_typeCodes[_key]}";
    }
}