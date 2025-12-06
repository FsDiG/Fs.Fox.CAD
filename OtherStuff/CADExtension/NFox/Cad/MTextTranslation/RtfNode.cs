namespace NFox.Cad
{
    /// <summary>
    /// 格式文本类型枚举
    /// </summary>
    public enum RtfNodeClassType
    {
        /// <summary>
        /// 区域文本
        /// </summary>
        Range = 0,
        /// <summary>
        /// 说明性文本
        /// </summary>
        SpecCode = 1,
        /// <summary>
        /// 简单格式文本
        /// </summary>
        Simple = 2,
        /// <summary>
        /// 划线型文本
        /// </summary>
        Limit = 3,
        /// <summary>
        /// 堆叠型文本
        /// </summary>
        Script = 4,
    }
    /// <summary>
    /// 格式文本节点抽象类
    /// </summary>
    public abstract class RtfNode
    {
        /// <summary>
        /// 格式文本类型
        /// </summary>
        protected RtfNodeClassType _nodeClassType;
        /// <summary>
        /// 键
        /// </summary>
        protected int _key;
        /// <summary>
        /// 获取或设置所在的区域节点.
        /// </summary>
        public RtfRangeNode Owner { get; set; }
        /// <summary>
        /// 获取格式文本类型.
        /// </summary>
        public RtfNodeClassType NodeClassType
        {
            get { return _nodeClassType; }
        }
        /// <summary>
        /// 获取键.
        /// </summary>
        public int Key
        {
            get { return _key; }
        }
        /// <summary>
        /// 获取文本内容.
        /// </summary>
        public abstract string Contents { get; }
    }
}