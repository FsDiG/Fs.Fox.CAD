using System;
using System.Linq;
using System.Collections.Generic;
using ThCADCore.NTS;
using Dreambuild.AutoCAD;
using ThMEPEngineCore.CAD;
using ThMEPEngineCore.Model;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public class ThRoomBuilderEngine : IDisposable
    {
        public void Dispose()
        {            
        }

        public List<ThIfcRoom> Build(Database database, Point3dCollection polygon)
        {
            // 获取房间
            var rooms = GetRooms(database, polygon);
            // 获取标注
            var marks = GetMarks(database, polygon);
            // 获取文字在哪些房间内
            var textContainer = BuildTextContainers(marks, rooms);
            // 匹配文字属于哪个房间
            SpaceMatchText(textContainer);

            return rooms;
        }

        private List<ThIfcRoom> GetRooms(Database database, Point3dCollection polygon)
        {
            // 识别 ModelSpace下的 Room
            var roomEngine = new ThRoomRecognitionEngine();
            roomEngine.RecognizeMS(database, polygon);
            return roomEngine.Elements.Cast<ThIfcRoom>().ToList();
        }

        private List<ThIfcTextNote> GetMarks(Database database, Point3dCollection polygon)
        {
            // 识别 ModelSpace、外参块照中的 RoomName
            var roomMarkEngine = new ThRoomMarkRecognitionEngine();
            roomMarkEngine.Recognize(database, polygon);
            roomMarkEngine.RecognizeMS(database, polygon);
            return roomMarkEngine.Elements.Cast<ThIfcTextNote>().ToList();
        }

        private Dictionary<ThIfcTextNote, List<ThIfcRoom>> BuildTextContainers(
            List<ThIfcTextNote> textNotes, List<ThIfcRoom> rooms)
        {
            var textContainer = new Dictionary<ThIfcTextNote, List<ThIfcRoom>>();
            textNotes.ForEach(m =>
            {
                Point3d textCenterPt = ThGeometryTool.GetMidPt(
                     m.Geometry.GetPoint3dAt(0),
                     m.Geometry.GetPoint3dAt(2));
                var containers = SelectTextIntersectPolygon(rooms.Select(o => o.Boundary).ToList(), m.Geometry);
                containers = containers.Where(n => n is Polyline polyline && polyline.Contains(textCenterPt)).ToList();
                if (containers.Count > 0)
                {
                    var containerRooms = rooms.Where(o => containers.Contains(o.Boundary)).ToList();
                    textContainer.Add(m, containerRooms);
                }
            });
            return textContainer;
        }

        private List<Curve> SelectTextIntersectPolygon(List<Curve> curves, Polyline textOBB)
        {
            return curves.Where(o =>
            {
                if (o is Polyline polyline)
                {
                    var relation = new ThCADCoreNTSRelate(polyline, textOBB);
                    return relation.IsOverlaps || relation.IsCovers || relation.IsContains;
                }
                return false;
            }).ToList();
        }

        private void SpaceMatchText(Dictionary<ThIfcTextNote, List<ThIfcRoom>> textContainer)
        {
            textContainer.ForEach(o =>
            {
                if (o.Value.Count > 0)
                {
                    var smallestAreaRoom = o.Value.OrderBy(v=>(v.Boundary as Polyline).Area).First();
                    if(smallestAreaRoom.Tags.IndexOf(o.Key.Text)<0)
                    {
                        smallestAreaRoom.Tags.Add(o.Key.Text);
                    }                    
                }
            });
        }
    }
}
