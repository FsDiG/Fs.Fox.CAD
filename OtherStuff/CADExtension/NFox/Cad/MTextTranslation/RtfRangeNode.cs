using System.Collections.Generic;

namespace NFox.Cad
{
    /// <summary>
    /// 格式文本区域节点
    /// </summary>
    /// <seealso cref="NFox.Cad.RtfNode" />
    public class RtfRangeNode : RtfNode, IEnumerable<RtfNode>
    {
        /// <summary>
        /// 获取或设置划线文本节点数量
        /// </summary>
        public short NumOfLimitNodes
        { get; set; }

        private readonly List<RtfNode> _lst = new List<RtfNode>();
        /// <summary>
        /// 初始化 <see cref="RtfRangeNode"/> 类.
        /// </summary>
        public RtfRangeNode()
        {
            _nodeClassType = RtfNodeClassType.Range;
            _key = -1;
        }
        
        internal RtfRangeNode(string contents)
        {
        }
        /// <summary>
        /// 添加格式文本节点
        /// </summary>
        /// <param name="node">文本节点</param>
        public void Add(RtfNode node)
        {
            node.Owner = this;
            _lst.Add(node);
        }
        /// <summary>
        /// 添加文本.
        /// </summary>
        /// <param name="textString">文本</param>
        public void Add(string textString)
        {
            Add(new RtfTextNode(textString));
        }
        /// <summary>
        /// 添加划线文本节点
        /// </summary>
        /// <param name="nodeType">划线文本节点</param>
        /// <param name="text">划线文本</param>
        public void Add(RtfLimitNodeType nodeType, string text)
        {
            NumOfLimitNodes++;
            Add(new RtfLimitNode(nodeType, text));
        }
        /// <summary>
        /// 添加堆叠类型节点
        /// </summary>
        /// <param name="nodeType">堆叠类型</param>
        /// <param name="upperScript">上文字</param>
        /// <param name="lowerScript">下文字</param>
        public void Add(RtfScriptNodeType nodeType, string upperScript, string lowerScript)
        {
            Add(new RtfScriptNode(nodeType, upperScript, lowerScript));
        }
        /// <summary>
        /// 添加简单文本
        /// </summary>
        /// <param name="nodeType">文本类型</param>
        /// <param name="specString">文本</param>
        public void Add(RtfSimpleNodeType nodeType, string specString)
        {
            Add(new RtfSimpleNode(nodeType, specString));
        }
        /// <summary>
        /// 添加说明性文本
        /// </summary>
        /// <param name="nodeType">说明性文本</param>
        public void Add(RtfSpecCodeNodeType nodeType)
        {
            Add(new RtfSpecCodeNode(nodeType));
        }
        /// <summary>
        /// 获取文本内容.
        /// </summary>
        public override string Contents
        {
            get
            {
                string s = "";
                foreach (RtfNode node in _lst)
                {
                    s += node.Contents;
                }
                if (Owner != null || NumOfLimitNodes > 0)
                {
                    s = "{" + s + "}";
                }
                return s;
            }
        }

        #region IEnumerable<RtfNode> 成员

        IEnumerator<RtfNode> IEnumerable<RtfNode>.GetEnumerator()
        {
            return _lst.GetEnumerator();
        }

        #endregion IEnumerable<RtfNode> 成员

        #region IEnumerable 成员

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _lst.GetEnumerator();
        }

        #endregion IEnumerable 成员
    }
}