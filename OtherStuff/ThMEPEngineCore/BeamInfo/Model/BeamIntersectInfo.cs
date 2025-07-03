using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.BeamInfo.Model
{
    public class BeamIntersectInfo
    {
        public IntersectType EntityType { get; set; }

        public List<Curve> EntityCurve = new List<Curve>();
    }

    public enum IntersectType
    {
        Column,  //与柱搭接

        Wall,    //与墙搭接

        Beam,    //与梁搭接
    }
}
