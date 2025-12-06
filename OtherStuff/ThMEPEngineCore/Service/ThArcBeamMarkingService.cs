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
    public class ThArcBeamMarkingService: ThBeamMarkingService
    {
        public ThArcBeamMarkingService(ArcBeam arcBeam) : base(arcBeam)
        {
            GetBeamTextRangePt();
        }
        public override List<DBText> Match(ThCADCoreNTSSpatialIndex dbtextSpatialIndex)
        {
            List<DBText> searchDbTexts = new List<DBText>();
            List<Point3d> rangePts = ThGeometryTool.CalBoundingBox(new List<Point3d>() { Pt1, Pt2, Pt3, Pt4 });
            DBObjectCollection searchTexts = dbtextSpatialIndex.SelectCrossingWindow(rangePts[0], rangePts[1]);
            foreach (var text in searchTexts)
            {
                DBText dbtext = text as DBText;
                var textNormal = Vector3d.XAxis.RotateBy(dbtext.Rotation, Vector3d.ZAxis);
                if (textNormal.IsParallelToEx(BeamEnt.BeamNormal))
                {
                    searchDbTexts.Add(dbtext);
                }
            }
            searchDbTexts = FilterDbTexts(searchDbTexts);
            return searchDbTexts;
        }
        /// <summary>
        /// 获取
        /// </summary>
        protected override void GetBeamTextRangePt()
        {
            Vector3d beamDir = BeamEnt.StartPoint.GetVectorTo(BeamEnt.EndPoint);
            Vector3d uprightDir = beamDir.GetPerpendicularVector().GetNormal();
            double width = (0.5 + ThMEPEngineCoreCommon.BeamTextSearchTimes) * BeamWidth;
            this.Pt1 = BeamEnt.StartPoint + uprightDir * width;
            this.Pt2 = BeamEnt.EndPoint + uprightDir * width;
            this.Pt3 = BeamEnt.EndPoint - uprightDir * width;
            this.Pt4 = BeamEnt.StartPoint - uprightDir * width;
        }
    }
}
