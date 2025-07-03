using System;

namespace NFox.Cad
{
    /// <summary>
    /// 命令信息
    /// </summary>
    [Serializable]
    public class CommandInfo : IComparable<CommandInfo>
    {
        /// <summary>
        /// 全局名称
        /// </summary>
        public string GlobalName { get; set; }
        /// <summary>
        /// 本地化名称
        /// </summary>
        public string LocalizedName { get; set; }
        /// <summary>
        /// 组名
        /// </summary>
        public string GroupName { get; set; }
        /// <summary>
        /// 类型名
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// 方法名
        /// </summary>
        public string MethodName { get; set; }
        /// <summary>
        /// 说明
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 默认初始化
        /// </summary>
        public CommandInfo()
        {
        }
        /// <summary>
        /// 采用全局名称初始化
        /// </summary>
        /// <param name="globalName">全局名称</param>
        public CommandInfo(string globalName)
        {
            GlobalName = globalName;
        }
        /// <summary>
        /// 将当前实例与同一类型的另一个对象进行比较，并返回一个整数，该整数指示当前实例在排序顺序中的位置是位于另一个对象之前、之后还是与其位置相同。
        /// </summary>
        /// <param name="other">与此实例进行比较的对象。</param>
        /// <returns>
        /// 一个值，指示要比较的对象的相对顺序。 返回值的含义如下：
        /// 值  含义  小于零  此实例在排序顺序中位于 <paramref name="other" /> 之前。 零   此实例在排序顺序中出现在与 <paramref name="other" /> 的相同位置。 大于零   此实例在排序顺序中位于 <paramref name="other" /> 之后。
        /// </returns>
        public int CompareTo(CommandInfo other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }
            return GlobalName.CompareTo(other.GlobalName);
        }

        
    }
}