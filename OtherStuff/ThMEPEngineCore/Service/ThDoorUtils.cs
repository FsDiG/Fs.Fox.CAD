using ThCADCore.NTS;
using ThCADExtension;
using ThMEPEngineCore.CAD;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service
{
    public class ThDoorUtils
    {
        public static bool IsQualified(Line neighborLine, Line doorStoneLine)
        {
            //平行、间距小于间隔距离、门垛线的投影点要在相邻线上
            if (!neighborLine.IsParallelToEx(doorStoneLine))
            {
                return false;
            }
            if (neighborLine.Distance(doorStoneLine) > ThMEPEngineCoreCommon.DoorStoneInterval)
            {
                return false;
            }
            var shortLine = doorStoneLine.ExtendLine(-2.0);
            var sp = shortLine.StartPoint.GetProjectPtOnLine(neighborLine.StartPoint, neighborLine.EndPoint);
            var ep = shortLine.EndPoint.GetProjectPtOnLine(neighborLine.StartPoint, neighborLine.EndPoint);

            return sp.IsPointOnLine(neighborLine, 1.0) && ep.IsPointOnLine(neighborLine, 1.0);
        }
        public static bool IsQualified(Polyline neighbor, Polyline doorStone)
        {
            var doorStoneLines = doorStone.ToLines();
            var neighborLines = neighbor.ToLines();
            for (int i = 0; i < doorStoneLines.Count; i++)
            {
                for (int j = 0; j < neighborLines.Count; j++)
                {
                    if (IsQualified(neighborLines[j], doorStoneLines[i]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static double GetDoorThick(Line line)
        {
            if(line.Length<ThMEPEngineCoreCommon.DoorMinimumThick)
            {
                return ThMEPEngineCoreCommon.DoorMinimumThick;
            }
            else if (line.Length > ThMEPEngineCoreCommon.DoorMaximumThick)
            {
                return ThMEPEngineCoreCommon.DoorMaximumThick;
            }
            else
            {
                return line.Length;
            }
        }
    }
}
