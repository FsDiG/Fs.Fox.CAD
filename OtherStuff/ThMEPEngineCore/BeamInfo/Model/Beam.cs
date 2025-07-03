using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.BeamInfo.Model
{
    public abstract class Beam
    {
        //原始的构成梁的线
        public virtual Curve UpBeamLine { get; set; }
        //原始的构成梁的线
        public virtual Curve DownBeamLine { get; set; }

        public virtual BeamStandardsType BeamType
        {
            get
            {
                if (StartIntersect != null && EndIntersect != null &&
                    (StartIntersect.EntityType == IntersectType.Column || StartIntersect.EntityType == IntersectType.Wall) &&
                    (EndIntersect.EntityType == IntersectType.Column || EndIntersect.EntityType == IntersectType.Wall))
                {
                    return BeamStandardsType.PrimaryBeam;
                }
                else
                {
                    return BeamStandardsType.SecondaryBeam; 
                }
            }
        }

        public virtual BeamOverhangingType OverhaningType
        {
            get
            {
                if ((StartIntersect == null && EndIntersect == null) ||
                     (StartIntersect.EntityType == IntersectType.Beam && EndIntersect.EntityType == IntersectType.Beam))
                {
                    return BeamOverhangingType.TwoOverhangingBeam;
                }
                else if (StartIntersect == null || EndIntersect == null || StartIntersect.EntityType == IntersectType.Beam || EndIntersect.EntityType == IntersectType.Beam)
                {
                    return BeamOverhangingType.OneOverhangingBeam;
                }
                else
                {
                    return BeamOverhangingType.NoOverhangingBeam;
                }
            }
        }
        public Point3d StartPoint { get; set; }
        public Point3d EndPoint { get; set; }
        public Point3d UpStartPoint { get; set; }

        public Point3d UpEndPoint { get; set; }

        public Point3d DownStartPoint { get; set; }

        public Point3d DownEndPoint { get; set; }

        public Vector3d BeamNormal { get; set; }

        public abstract Polyline BeamBoundary { get; }

        /// <summary>
        /// 梁端点搭接的实体的信息
        /// </summary>
        public virtual BeamIntersectInfo StartIntersect { get; set; }

        /// <summary>
        /// 梁端点搭接的实体的信息
        /// </summary>
        public virtual BeamIntersectInfo EndIntersect { get; set; }

        /// <summary>
        /// 集中标注
        /// </summary>
        public virtual ThCentralizedMarking ThCentralizedMarkingP { get; set; }

        /// <summary>
        /// 原位标注
        /// </summary>
        public virtual ThOriginMarkingcs ThOriginMarkingcsP { get; set; }

        /// <summary>
        /// 梁起始端点
        /// </summary>
        public virtual Polyline BeamSPointSolid { get; set; }

        /// <summary>
        /// 梁终止端点
        /// </summary>
        public virtual Polyline BeamEPointSolid { get; set; }

        /// <summary>
        /// 所有集中标注
        /// </summary>
        public virtual List<MarkingInfo> CentralizeMarkings { get; set; }

        /// <summary>
        /// 所有原位标注
        /// </summary>
        public virtual List<MarkingInfo> OriginMarkings { get; set; }     

        protected Polyline CreatePolyline(Point3d p1, Point3d p2, Vector3d normal, double offset)
        {
            Vector3d moveV = (p1 - p2).GetNormal();
            p1 = p1 + moveV * offset;
            p2 = p2 - moveV * offset;
            Point3d newP1 = p1 + normal * offset;
            Point3d newp2 = p2 + normal * offset;
            Point3d newP3 = p2 - normal * offset;
            Point3d newp4 = p1 - normal * offset;

            Polyline polyline = new Polyline(4) { Closed = true };
            polyline.AddVertexAt(0, new Point2d(newP1.X, newP1.Y), 0, 0, 0);
            polyline.AddVertexAt(1, new Point2d(newp2.X, newp2.Y), 0, 0, 0);
            polyline.AddVertexAt(2, new Point2d(newP3.X, newP3.Y), 0, 0, 0);
            polyline.AddVertexAt(3, new Point2d(newp4.X, newp4.Y), 0, 0, 0);
            return polyline;
        }
    }

    public enum BeamStandardsType
    {
        //主梁
        PrimaryBeam,

        //次梁
        SecondaryBeam,
    }

    public enum BeamOverhangingType
    {
        //不是悬挑梁
        NoOverhangingBeam,

        //一端悬挑
        OneOverhangingBeam,

        //两端悬挑
        TwoOverhangingBeam,
    }
}
