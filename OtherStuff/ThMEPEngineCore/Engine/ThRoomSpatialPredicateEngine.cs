using System;
using NFox.Cad;
using System.Linq;
using ThCADCore.NTS;
using ThMEPEngineCore.Model;
using System.Collections.Generic;

namespace ThMEPEngineCore.Engine
{
    public class ThRoomSpatialPredicateEngine
    {
        public List<ThIfcRoom> Rooms { get; set; }
        public ThCADCoreNTSSpatialIndex SpatialIndex { get; set; }
        public ThRoomSpatialPredicateEngine(List<ThIfcRoom> rooms)
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
            throw new NotSupportedException();
        }

        /// <summary>
        /// 附近
        /// </summary>
        /// <param name="room"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public List<ThIfcRoom> Nears(ThIfcRoom room, double distance)
        {
            throw new NotSupportedException();
        }
    }
}
