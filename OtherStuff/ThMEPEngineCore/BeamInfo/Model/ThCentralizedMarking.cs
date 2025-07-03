namespace ThMEPEngineCore.BeamInfo.Model
{
    public class ThCentralizedMarking
    {
        /// <summary>
        /// 梁编号
        /// </summary>
        public string BeamNum { get; set; }

        /// <summary>
        /// 截面尺寸
        /// </summary>
        public string SectionSize { get; set; }

        /// <summary>
        /// 梁箍筋
        /// </summary>
        public string Hooping { get; set; }

        /// <summary>
        /// 梁上部、下部通长筋或架立筋
        /// </summary>
        public string ExposedReinforcement { get; set; }

        /// <summary>
        /// 构造钢筋或受扭钢筋
        /// </summary>
        public string TwistedSteel { get; set; }

        /// <summary>
        /// 顶面标高与结构楼面标高差值
        /// </summary>
        public string LevelDValue { get; set; }
    }
}
