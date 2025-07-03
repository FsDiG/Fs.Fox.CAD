namespace NFox.Cad
{
    /// <summary>
    /// 简单文本类型枚举
    /// </summary>
    public enum RtfSimpleNodeType
    {
        /// <summary>
        /// 对齐方式
        /// </summary>
        Alignment,        //A        
        /// <summary>
        /// 颜色
        /// </summary>
        Color,            //C        
        /// <summary>
        /// 字体
        /// </summary>
        Font,             //F        
        /// <summary>
        /// 字体高度
        /// </summary>
        Height,           //H        
        /// <summary>
        /// 字体宽度
        /// </summary>
        Width,            //W        
        /// <summary>
        /// 字体角度
        /// </summary>
        Angle,            //Q        
        /// <summary>
        /// 字体间距
        /// </summary>
        Interval,         //T
    }
    /// <summary>
    /// 简单文本格式节点
    /// </summary>
    /// <seealso cref="NFox.Cad.RtfNode" />
    /// TODO Edit XML Comment Template for RtfSimpleNode
    public class RtfSimpleNode : RtfNode
    {
        /// <summary>
        /// 控制码
        /// </summary>
        protected readonly static string[] _typeCodes =
            new string[] { "A", "C", "F", "H", "W", "Q", "T" };
        /// <summary>
        /// 获取和设置说明性文本
        /// </summary>
        public virtual string SpecString { get; set; }
        /// <summary>
        /// 初始化 <see cref="RtfSimpleNode"/> 类.
        /// </summary>
        /// <param name="nodeType">文本类型</param>
        public RtfSimpleNode(RtfSimpleNodeType nodeType)
        {
            _nodeClassType = RtfNodeClassType.Simple;
            _key = (int)nodeType;
        }
        /// <summary>
        /// 初始化 <see cref="RtfSimpleNode"/> 类.
        /// </summary>
        /// <param name="nodeType">文本类型</param>
        /// <param name="specString">说明性文本</param>
        public RtfSimpleNode(RtfSimpleNodeType nodeType, string specString)
        {
            _nodeClassType = RtfNodeClassType.Simple;
            _key = (int)nodeType;
            SpecString = specString;
        }
        /// <summary>
        /// 获取文本内容.
        /// </summary>
        /// TODO 根据控制码来细化不同的格式，比如\F格式后面是有字体的具体控制的
        public override string Contents =>
            string.Format(
                "\\{0}{1};",
                _typeCodes[_key],
                SpecString);
    }
}