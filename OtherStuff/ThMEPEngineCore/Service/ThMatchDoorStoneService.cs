using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThMEPEngineCore.CAD;

namespace ThMEPEngineCore.Service
{
    public class ThMatchDoorStoneService
    {
        private Tuple<Line,Line> Pair { get; set; }
        private List<Polyline> Stones { get; set; }
        private Vector3d Direction { get; set; }
        private double Length { get; set; }
        private double DistanceTolerance { get; set; } = 5.0;
        private double AngleTolerance { get; set; } = 2.0;
        private ThMatchDoorStoneService(List<Polyline> stones, Vector3d direction,double length)
        {
            Stones = stones;
            Direction = direction;
            Length = length;
        }
        public static Tuple<Line, Line> Match(List<Polyline> stones,Vector3d direction,double length)
        {
            var instance = new ThMatchDoorStoneService(stones, direction, length);
            instance.Match();
            return instance.Pair;
        }
        private void Match()
        {
            for(int i=0;i<Stones.Count-1;i++)
            {
                var firstLines = Stones[i].ToLines();
                for (int j = i + 1; j < Stones.Count; j++)
                {
                    var secondLines = Stones[j].ToLines();
                    var pairs = Match(firstLines, secondLines);
                    if(pairs.Count==1)
                    {
                        Pair = pairs[0];
                        break;
                    }
                }
                if(Pair != null)
                {
                    break;
                }
            }
        }
        private List<Tuple<Line, Line>> Match(List<Line> firstLines,List<Line> secondLines)
        {
            var pairs = new List<Tuple<Line, Line>>();
            firstLines.ForEach(f =>
            {
                var firstDir = f.StartPoint.GetVectorTo(f.EndPoint);
                secondLines.ForEach(s =>
                {
                    var secondDir = s.StartPoint.GetVectorTo(s.EndPoint);
                    //保持判断顺序
                    if(
                    ThGeometryTool.IsParallelToEx(firstDir, secondDir) &&
                    IsRightAngle(firstDir,AngleTolerance) &&
                    IsEqual(f,s,DistanceTolerance) && 
                    IsEqualToLength(f, s,DistanceTolerance))
                    {
                        pairs.Add(Tuple.Create(f,s));
                    }
                });
            });
            return pairs;
        }
        private bool IsRightAngle(Vector3d vec,double tolerance=2.0)
        {
            var rad = Direction.GetAngleTo(vec);
            var ang = (rad / Math.PI) * 180.0;
            ang %= 180.0;
            return Math.Abs(ang-90.0)<= tolerance;
        }
        private bool IsEqual(Line first, Line second, double tolerance = 5.0)
        {
            return Math.Abs(first.Length - second.Length) <= tolerance;
        }
        private bool IsEqualToLength(Line first, Line second, double tolerance = 5.0)
        {
            var firstMidPt = first.StartPoint.GetMidPt(first.EndPoint);
            var secondMidPt = second.StartPoint.GetMidPt(second.EndPoint);
            return Math.Abs(firstMidPt.DistanceTo(secondMidPt)-Length) <= tolerance;
        }
    }
}
