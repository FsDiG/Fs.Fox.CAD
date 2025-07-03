namespace NFox.Cad
{
    /// <summary>
    /// 划线类型枚举
    /// </summary>
    public enum RtfLimitNodeType
    {
        /// <summary>
        /// 下划线
        /// </summary>
        Underline,        //L        
        /// <summary>
        /// 上划线
        /// </summary>
        Strikeout,        //O        
        /// <summary>
        /// 删除线
        /// </summary>
        StikeThrough      //K
    }
    /// <summary>
    /// 划线型文本节点
    /// </summary>
    /// <seealso cref="NFox.Cad.RtfNode" />
    public class RtfLimitNode : RtfNode
    {
        private readonly static string[] _typeCodes =
            new string[] { "L", "O", "K"};
        /// <summary>
        /// 获取或设置划线文本.
        /// </summary>
        public string LimitString { get; set; }
        /// <summary>
        /// 初始化 <see cref="RtfLimitNode"/> 类.
        /// </summary>
        /// <param name="nodeType">划线枚举类型.</param>
        /// <param name="text">文本</param>
        public RtfLimitNode(RtfLimitNodeType nodeType, string text)
        {
            _nodeClassType = RtfNodeClassType.Limit;
            _key = (int)nodeType;
            LimitString = text;
        }
        /// <summary>
        /// 获取文本内容.
        /// </summary>
        public override string Contents => $"\\{_typeCodes[_key]}{LimitString}\\{_typeCodes[_key].ToLower()}";
    }
}