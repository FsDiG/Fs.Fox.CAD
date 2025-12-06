using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Model
{
    public class ThIfcDistributionElement : ThIfcElement
    {
        /// <summary>
        /// OCS到其他坐标系转换矩阵
        /// </summary>
        public Matrix3d Matrix { get; set; }
        /// <summary>
        /// OCS坐标系的中心线几何图元
        /// </summary>
        public DBObjectCollection Centerline { get; set; } = new DBObjectCollection();
        public DBObjectCollection FlangeLine { get; set; } = new DBObjectCollection();
        /// <summary>
        /// OCS坐标系下的几何图元
        /// </summary>
        public DBObjectCollection Representation { get; set; } = new DBObjectCollection();
        /// <summary>
        /// OCS坐标系下的图元信息
        /// </summary>
        public DBText InformationText { get; set; } = new DBText();
    }
}
