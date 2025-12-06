using System.Linq;
using System.Collections.Generic;
using ThCADCore.NTS;
using ThCADExtension;
using Autodesk.AutoCAD.DatabaseServices;
using ThMEPEngineCore.Model;

namespace ThMEPEngineCore.Service
{
    public class ThFindDoorstoneNeighborService
    {
        /// <summary>
        /// 门垛与墙、柱存在的空隙
        /// </summary>
        public Polyline Neighbor { get; private set; }
        public BuiltInCategory Kind { get; private set; }
        private double Interval { get; set; }
        public ThFindDoorstoneNeighborService()
        {
            Neighbor = new Polyline();
            Kind = BuiltInCategory.UnNone;
            Interval = ThMEPEngineCoreCommon.DoorStoneInterval;
        }
        public void Find(Polyline doorStone)
        {
            //获取门垛符合条件的邻居
            var nearArchwalls = FindElements(BuiltInCategory.OST_ArchitectureWall, doorStone);
            var nearShearwalls = FindElements(BuiltInCategory.OST_ShearWall, doorStone);
            var nearColumns = FindElements(BuiltInCategory.OST_Column, doorStone);
            var nearWindows = FindElements(BuiltInCategory.OST_Window, doorStone);
            var nearCurtainwalls = FindElements(BuiltInCategory.OST_CurtainWall, doorStone);

            //分析
            if (nearArchwalls.Count>0)
            {
                Neighbor = nearArchwalls[0];
                Kind = BuiltInCategory.OST_ArchitectureWall;
            }
            if (nearShearwalls.Count > 0)
            {
                Neighbor = nearShearwalls[0];
                Kind = BuiltInCategory.OST_ShearWall;
            }
            if (nearColumns.Count > 0)
            {
                Neighbor = nearColumns[0];
                Kind = BuiltInCategory.OST_Column;
            }
            if (nearWindows.Count > 0)
            {
                Neighbor = nearWindows[0];
                Kind = BuiltInCategory.OST_Window;
            }
            if (nearCurtainwalls.Count > 0)
            {
                Neighbor = nearCurtainwalls[0];
                Kind = BuiltInCategory.OST_CurtainWall;
            }
        }
        private List<Polyline> FindElements(BuiltInCategory builtInCategory, Polyline doorStone)
        {
            var results = ThSpatialIndexCacheService.Instance.Find(builtInCategory, doorStone.Buffer(Interval)[0] as Polyline);
            var polylines = TransPolyines(results);
            Sort(polylines, doorStone);
            polylines = polylines.Where(o => ThDoorUtils.IsQualified(o, doorStone)).ToList();
            return polylines;
        }
        private void Sort(List<Polyline> nears,Polyline doorStone)
        {
            nears=nears.OrderBy(o=>o.Distance(doorStone)).ToList();
        }
        private List<Polyline> TransPolyines(List<Entity> entities)
        {
            var results = new List<Polyline>();
            entities.ForEach(o =>
            {
                if (o is Polyline poly)
                {
                    results.Add(poly);
                }
                else if (o is MPolygon mPolygon)
                {
                    results.AddRange(mPolygon.Loops());
                }
            });
            return results;
        }
    }
}
