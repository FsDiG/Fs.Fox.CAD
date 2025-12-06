using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThCADCore.NTS;
using ThMEPEngineCore.BeamInfo.Model;
using ThMEPEngineCore.CAD;

namespace ThMEPEngineCore.Service
{
    public abstract class ThBeamMarkingService
    {
        protected Beam BeamEnt { get; set; }
        public double BeamWidth { get; set; }
        public Point3d Pt1 { get; set; }
        public Point3d Pt2 { get; set; }
        public Point3d Pt3 { get; set; }
        public Point3d Pt4 { get; set; }
        public ThBeamMarkingService(Beam beam)
        {
            BeamEnt = beam;
            BeamWidth = GetBeamWidth();           
        }
        public abstract List<DBText> Match(ThCADCoreNTSSpatialIndex dbtextSpatialIndex);
        protected abstract void GetBeamTextRangePt();

        /// <summary>
        /// 获取含有规格的文字
        /// </summary>
        /// <param name="dbTexts"></param>
        /// <returns></returns>
        protected virtual List<DBText> FilterDbTexts(List<DBText> dbTexts)
        {
            return dbTexts.Where(o => ThStructureUtils.ValidateSpec(o.TextString)).ToList();
        }
        /// <summary>
        /// 获取梁的宽度
        /// </summary>
        /// <returns></returns>
        private double GetBeamWidth()
        {
            List<double> widths = new List<double>();
            for (int i = 0; i < BeamEnt.BeamBoundary.NumberOfVertices; i++)
            {
                SegmentType st = BeamEnt.BeamBoundary.GetSegmentType(i);
                if (st == SegmentType.Line)
                {
                    LineSegment3d lineSegment = BeamEnt.BeamBoundary.GetLineSegmentAt(i);
                    widths.Add(lineSegment.Length);
                }
            }
            return widths.Where(o => o > 50.0).OrderBy(o => o).FirstOrDefault();
        }        
    }
}
