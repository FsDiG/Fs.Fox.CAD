namespace NFox.Cad
{
    /// <summary>
    /// 堆叠类型枚举
    /// </summary>
    public enum RtfScriptNodeType
    {
        /// <summary>
        /// 水平分数
        /// </summary>
        Fraction,
        /// <summary>
        /// 倾斜分数
        /// </summary>
        Italic,
        /// <summary>
        /// 公差
        /// </summary>
        Tolerance
    }
    /// <summary>
    /// 堆叠节点
    /// </summary>
    /// <seealso cref="NFox.Cad.RtfNode" />
    public class RtfScriptNode : RtfNode
    {
        private static string[] _scriptTypStrings =
            new string[] { "/", "#", "^" };
        /// <summary>
        /// 获取或者设置上文字.
        /// </summary>
        public string UpperScript { get; set; }
        /// <summary>
        /// 获取或者设置下文字.
        /// </summary>
        public string LowerScript { get; set; }
        /// <summary>
        /// 初始化 <see cref="RtfScriptNode"/> 类.
        /// </summary>
        /// <param name="nodeType">堆叠类型</param>
        public RtfScriptNode(RtfScriptNodeType nodeType)
        {
            _nodeClassType = RtfNodeClassType.Script;
            _key = (int)nodeType;
        }
        /// <summary>
        /// 初始化 <see cref="RtfScriptNode"/> 类.
        /// </summary>
        /// <param name="nodeType">堆叠类型</param>
        /// <param name="upperScript">上文字</param>
        /// <param name="lowerScript">下文字</param>
        public RtfScriptNode(RtfScriptNodeType nodeType, string upperScript, string lowerScript)
        {
            _nodeClassType = RtfNodeClassType.Script;
            _key = (int)nodeType;
            UpperScript = upperScript;
            LowerScript = lowerScript;
        }
        /// <summary>
        /// 获取文本内容.
        /// </summary>
        public override string Contents =>
            string.Format(
                "\\S{0}{1}{2};",
                UpperScript,
                _scriptTypStrings[_key],
                LowerScript);
    }
}