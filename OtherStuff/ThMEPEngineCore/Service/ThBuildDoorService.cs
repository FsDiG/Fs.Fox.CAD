using System;
using System.Linq;
using System.Collections.Generic;
using ThCADCore.NTS;
using ThCADExtension;
using ThMEPEngineCore.CAD;
using Autodesk.AutoCAD.DatabaseServices;
using ThMEPEngineCore.Model;

namespace ThMEPEngineCore.Service
{
    public class ThBuildDoorService
    {
        public List<Polyline> Outlines { get; private set; }
        private Dictionary<DBText, List<Polyline>> DoorMarkStones { get; set; }
        private double Interval { get; set; }
        public ThBuildDoorService()
        {
            Outlines = new List<Polyline>();
            Interval = ThMEPEngineCoreCommon.DoorStoneInterval;
        }
        public void Build(List<Tuple<Entity, List<Polyline>, double>> doorMarkStones)
        {
            foreach(var group in doorMarkStones)
            {
                if (group.Item2.Count == 2)
                {
                    Outlines.Add(BuildDoor(group.Item2[0], group.Item2[1], group.Item3));
                }
            }
            Outlines = Outlines.Where(o => o.Area > 0.0).ToList();
        }
        private Polyline BuildDoor(Polyline firstStone,Polyline secondStone, double length)
        {
            //检查两个门垛的两端是否与Length在指定容差内
            if(!IsValid(firstStone, secondStone, length))
            {
                return new Polyline();
            }
            //获取门垛的邻居
            var firstNeibourService = new ThFindDoorstoneNeighborService();
            firstNeibourService.Find(firstStone);

            var secondNeibourService = new ThFindDoorstoneNeighborService();
            secondNeibourService.Find(secondStone);

            if(firstNeibourService.Neighbor.Area==0 || secondNeibourService.Neighbor.Area == 0)
            {
                return new Polyline();
            }

            //找到门垛与相邻柱或墙的匹配边
            var firstPairs = FindPairEdges(firstStone, firstNeibourService.Neighbor);
            var secondPairs = FindPairEdges(secondStone, secondNeibourService.Neighbor);

            //找到适合创建门的两边
            var doorPairs = new List<Tuple<Line, Line>>();
            for(int i=0;i< firstPairs.Count;i++)
            {
                for (int j = 0; j < secondPairs.Count; j++)
                {
                    if(IsValid(firstPairs[i].Item1, secondPairs[j].Item1,length))
                    {
                        doorPairs.Add(Tuple.Create(firstPairs[i].Item2, secondPairs[j].Item2));
                    }                    
                }
            }

            // 创建门
            var doorOutlines = CreateDoor(doorPairs, firstNeibourService.Kind, secondNeibourService.Kind);
            return doorOutlines.Count > 0 ? doorOutlines.First():new Polyline();
        }

        private Polyline BuildDoor(Polyline single, double length,double markTextRotation)
        {
            throw new NotSupportedException();
        }
        private List<Polyline> CreateDoor(
            List<Tuple<Line, Line>> doorPairs, 
            BuiltInCategory firstNeighborType,
            BuiltInCategory secondNeighborType)
        {
            //后续根据产品文档再通过条件构件
            return CreateDoorByCompare(doorPairs);
            //if ((firstNeighborType  == BuiltInCategory.OST_ArchitectureWall && secondNeighborType  == BuiltInCategory.OST_ArchitectureWall) ||
            //    (firstNeighborType == BuiltInCategory.OST_ShearWall && secondNeighborType == BuiltInCategory.OST_ShearWall) ||
            //    (firstNeighborType == BuiltInCategory.OST_Column && secondNeighborType == BuiltInCategory.OST_Column) ||
            //    (firstNeighborType == BuiltInCategory.OST_Window && secondNeighborType == BuiltInCategory.OST_Window) ||
            //    (firstNeighborType == BuiltInCategory.OST_CurtainWall && secondNeighborType == BuiltInCategory.OST_CurtainWall)
            //    )
            //{
            //    return CreateDoorByCompare(doorPairs);
            //}
            //else if ((firstNeighborType == BuiltInCategory.OST_ArchitectureWall && secondNeighborType == BuiltInCategory.OST_ShearWall) ||
            //    (firstNeighborType == BuiltInCategory.OST_ArchitectureWall && secondNeighborType == BuiltInCategory.OST_Column) ||
            //    (firstNeighborType == BuiltInCategory.OST_ShearWall && secondNeighborType == BuiltInCategory.OST_Column))
            //{
            //    return CreateDoorByFirst(doorPairs);
            //}
            //else if((firstNeighborType == BuiltInCategory.OST_ShearWall && secondNeighborType == BuiltInCategory.OST_ArchitectureWall) ||
            //    (firstNeighborType == BuiltInCategory.OST_Column && secondNeighborType == BuiltInCategory.OST_ArchitectureWall) ||
            //    (firstNeighborType == BuiltInCategory.OST_Column && secondNeighborType == BuiltInCategory.OST_ShearWall))
            //{
            //    return CreateDoorBySecond(doorPairs);
            //}
            //else
            //{
            //    throw new NotSupportedException();
            //}
        }
        private List<Polyline> CreateDoorByCompare(List<Tuple<Line, Line>> doorPairs)
        {
            var results = new List<Polyline>();
            doorPairs.ForEach(o =>
            {
                if (o.Item1.Length <= o.Item2.Length)
                {
                    results.Add(CreateDoor(o.Item1, o.Item2));
                }
                else
                {
                    results.Add(CreateDoor(o.Item2, o.Item1));
                }
            });
            return results;
        }
        private List<Polyline> CreateDoorByFirst(List<Tuple<Line, Line>> doorPairs)
        {
            var results = new List<Polyline>();
            doorPairs.ForEach(o =>
            {
                results.Add(CreateDoor(o.Item1, o.Item2));
            });
            return results;
        }
        private List<Polyline> CreateDoorBySecond(List<Tuple<Line, Line>> doorPairs)
        {
            var results = new List<Polyline>();
            doorPairs.ForEach(o =>
            {
                results.Add(CreateDoor(o.Item2, o.Item1));
            });
            return results;
        }
        private Polyline CreateDoor(Line first,Line second)
        {
            if(first.Length==0 || second.Length == 0)
            {
                return new Polyline();
            }
            //外部控制传入的first和second要平行，且first.Length<=second.Length
            var perpendVec = first.LineDirection().GetPerpendicularVector().GetNormal();
            var firstMidPt = first.StartPoint.GetMidPt(first.EndPoint);
            var otherPt = firstMidPt + perpendVec.MultiplyBy(100);
            var secondMidPt = second.StartPoint.GetMidPt(second.EndPoint);
            secondMidPt = secondMidPt.GetProjectPtOnLine(firstMidPt, otherPt);
            var doorThick = ThDoorUtils.GetDoorThick(first);
            return ThDrawTool.ToRectangle(firstMidPt, secondMidPt, doorThick);
        }
        
        /// <summary>
        /// 查找门垛与障碍物相邻的边
        /// </summary>
        /// <param name="doorStone"></param>
        /// <param name="obstacle"></param>
        /// <returns></returns>
        private List<Tuple<Line,Line>> FindPairEdges(Polyline doorStone,Polyline neighbor)
        {
            //Tuple.Create(门垛边，邻居边);
            var results = new List<Tuple<Line, Line>>();
            var stoneLines = doorStone.ToLines();         
            var neighborLines = neighbor.ToLines();
            for (int i = 0; i < stoneLines.Count; i++)
            {
                for (int j = 0; j < neighborLines.Count; j++)
                {
                    if(ThDoorUtils.IsQualified(neighborLines[j], stoneLines[i]))
                    {
                        results.Add(Tuple.Create(stoneLines[i], neighborLines[j]));
                    }
                }
            }
            return results;
        }
        private bool IsValid(Polyline first, Polyline second, double length)
        {
            var firstLines = first.ToLines();
            var secondLines = second.ToLines();
            for (int i = 0; i < firstLines.Count; i++)
            {
                for (int j = 0; j < secondLines.Count; j++)
                {
                    if (IsValid(firstLines[i], secondLines[j], length))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private bool IsValid(Line first, Line second, double length)
        {
            return first.Length>0 && second.Length>0.0 &&
                first.IsParallelToEx(second) &&
                Math.Abs(first.Distance(second) - length) <= 2.0; //2.0用于解决小于点误差
        }
    }
}
