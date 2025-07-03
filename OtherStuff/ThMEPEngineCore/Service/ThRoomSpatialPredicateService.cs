using NFox.Cad;
using System.Linq;
using ThCADCore.NTS;
using ThMEPEngineCore.Model;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service
{
    public class ThRoomSpatialPredicateService
    {
        public List<ThIfcRoom> Rooms { get; set; }
        public ThCADCoreNTSSpatialIndex SpatialIndex { get; set; }
        /// <summary>
        /// 对于子空间的判断，通过偏移以满足条件
        /// </summary>
        public double BufferLength { get; set; }
        public ThRoomSpatialPredicateService(List<ThIfcRoom> rooms)
        {
            Rooms = rooms;
            SpatialIndex = new ThCADCoreNTSSpatialIndex(Rooms.Select(o => o.Boundary).ToCollection());
        }

        /// <summary>
        /// 包含关系
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public List<ThIfcRoom> Contains(ThIfcRoom room)
        {
            var results = new List<ThIfcRoom>();
            if (room.Boundary is Polyline polyline)
            {
                var dbObjs = SpatialIndex.SelectCrossingPolygon(polyline);
                dbObjs.Remove(polyline);
                var fiterSpaces = Rooms.Where(o => dbObjs.Contains(o.Boundary)).ToList();
                results = fiterSpaces.Where(o => o.Boundary is Polyline subPoly && IsContains(polyline, subPoly)).ToList();
            }
            return results;
        }

        /// <summary>
        /// 附近
        /// </summary>
        /// <param name="room"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public List<ThIfcRoom> Nears(ThIfcRoom room, double distance)
        {
            var results = new List<ThIfcRoom>();
            if (room.Boundary is Polyline polyline)
            {
                var enlarge = polyline.Buffer(distance)[0] as Polyline;
                var dbObjs = SpatialIndex.SelectCrossingPolygon(enlarge);
                dbObjs.Remove(polyline);
                var filterObjs =dbObjs
                    .Cast<Curve>()
                    .Where(o => o is Polyline)
                    .Cast<Polyline>()
                    .Where(o => IsNeighbor(enlarge, o, distance))
                    .Where(o =>
                    {
                        var dis = polyline.Distance(o);
                        return dis >= 0 && dis <= distance;
                    }).ToCollection();

                results = Rooms.Where(o => filterObjs.Contains(o.Boundary)).ToList();                
            }
            return results;
        }

        private bool IsNeighbor(Polyline first, Polyline second,double distance)
        {
            if(IsContains(first, second) || IsContains(second,first))
            {
                return false;
            }
            var relate = new ThCADCoreNTSRelate(first, second);
            if(relate.IsIntersects==false)
            {
                var dis = first.Distance(second);
                return dis >= 0 && dis <= distance;
            }
            else
            {
                if (relate.IsOverlaps == false)
                {
                    return true;
                }
                else if(distance!=0.0)
                {
                    var narrow = first.Buffer(-distance)[0] as Polyline;
                    return IsNeighbor(narrow, second, 0.0);
                }
            }
            return false;
        }

        private bool IsContains(Polyline first, Polyline second)
        {
            var firstEnlarge = second;
            var objs = first.Buffer(BufferLength);
            if (objs.Count > 0)
            {
                firstEnlarge = objs[0] as Polyline;
            }
            var relate = new ThCADCoreNTSRelate(firstEnlarge, second);
            return relate.IsContains || relate.IsCovers;
        }
    }
}
