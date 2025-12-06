using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.BeamInfo.Model
{
    public class MarkingInfo
    {
        public Entity Marking { get; set; }

        public MarkingType Type { get; set; }

        public Vector3d MarkingNormal { get; set; } 

        public Point3d AlignmentPoint { get; set; }

        public Point3d Position { get; set; }
    }

    public enum MarkingType
    {
        /// <summary>
        /// 全类型标注
        /// </summary>
        All,

        /// <summary>
        /// 线性标注
        /// </summary>
        Line,

        /// <summary>
        /// 文字标注
        /// </summary>
        Text,
    }
}
