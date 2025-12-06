using AcPolygon = Autodesk.AutoCAD.DatabaseServices.Polyline;

namespace ThMEPEngineCore.BeamInfo.Model
{
    public class ThBeamSpanZone
    { 
        /// <summary>
        /// 原始区域
        /// </summary>
        public AcPolygon Region { get; private set; }

        /// <summary>
        /// 保护区域（扩大区域）
        /// </summary>
        public AcPolygon ProtectedRegion { get; private set; }

        /// <summary>
        /// 可布区域（内缩区域）
        /// </summary>
        public AcPolygon DistributableRegion{ get; private set; }
        public ThBeamSpanZone(AcPolygon polygon)
        {
            //
        }
    }
}
