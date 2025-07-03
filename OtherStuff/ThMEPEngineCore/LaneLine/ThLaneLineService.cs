using System;
using System.Linq;
using ThCADCore.NTS;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.LaneLine
{
    public class ParkingLinesService
    {
        private readonly double tol = 0.9;
        public double pointTol = 1200;
        public double parkingLineTolerance = 1000;

        /// <summary>
        /// 将车道线分成主车道线和副车道线
        /// </summary>
        /// <param name="roomPoly"></param>
        /// <param name="parkingLines"></param>
        /// <param name="otherPLins"></param>
        /// <returns></returns>
        public List<List<Line>> CreateParkingLines(Polyline roomPoly, List<Line> parkingLines, out List<List<Line>> otherPLins)
        {
            parkingLines = parkingLines.SelectMany(x => roomPoly.Trim(x).Cast<Polyline>().Select(y => new Line(y.StartPoint, y.EndPoint))).ToList();
            var pLines = ClassifyParkingLines(parkingLines);
            var xPLines = pLines[0];
            var yPLines = pLines[1];
            var resLines = HandleLinesByDirection(xPLines);
            otherPLins = HandleLinesByDirection(yPLines);

            return resLines;
        }

        /// <summary>
        /// 将车道线分成主车道线和副车道线
        /// </summary>
        /// <param name="roomPoly"></param>
        /// <param name="parkingLines"></param>
        /// <param name="otherPLins"></param>
        /// <returns></returns>
        public List<List<Line>> CreateNodedParkingLines(Polyline roomPoly, List<Line> parkingLines, out List<List<Line>> otherPLins)
        {
            otherPLins = new List<List<Line>>();
            if (parkingLines.Count <= 0)
            {
                return new List<List<Line>>();
            }

            parkingLines = parkingLines.SelectMany(x => roomPoly.Trim(x).Cast<Polyline>()
                .Select(y =>
                {
                    var dir = (y.EndPoint - y.StartPoint).GetNormal();
                    return new Line(y.StartPoint - dir * 1, y.EndPoint + dir * 1);
                }))
                .ToList();
            var objs = new DBObjectCollection();
            parkingLines.ForEach(x => objs.Add(x));
            var nodeGeo = objs.ToNTSNodedLineStrings();
            var handleLines = new List<Line>();
            if (nodeGeo != null)
            {
                handleLines = nodeGeo.ToDbObjects()
                .SelectMany(x =>
                {
                    DBObjectCollection entitySet = new DBObjectCollection();
                    (x as Polyline).Explode(entitySet);
                    return entitySet.Cast<Line>().ToList();
                })
                .Where(x => x.Length > 2)
                .ToList();
            }

            var pLines = ClassifyParkingLines(handleLines);
            //总长度更长的分为主车道
            var xPLines = pLines[0];
            var yPLines = pLines[1];
            if (xPLines.Sum(x => x.Length) < yPLines.Sum(x => x.Length))
            {
                xPLines = pLines[1];
                yPLines = pLines[0];
            }

            var resLines = HandleLinesByDirection(xPLines);
            otherPLins = HandleLinesByDirection(yPLines);

            return resLines;
        }

        /// <summary>
        /// 将车道线分成主车道线和副车道线(z型车道分为一条车道。tips：疏散指示灯在用)
        /// </summary>
        /// <param name="roomPoly"></param>
        /// <param name="parkingLines"></param>
        /// <param name="otherPLins"></param>
        /// <returns></returns>
        public List<List<Line>> CreateNodedPLineToPolyByConnect(Polyline roomPoly, List<Line> parkingLines, out List<List<Line>> otherPLins)
        {
            otherPLins = new List<List<Line>>();
            if (parkingLines.Count <= 0)
            {
                return new List<List<Line>>();
            }

            parkingLines = parkingLines.SelectMany(x => roomPoly.Trim(x).Cast<Polyline>()
                .Select(y =>
                {
                    var dir = (y.EndPoint - y.StartPoint).GetNormal();
                    return new Line(y.StartPoint - dir * 1, y.EndPoint + dir * 1);
                }))
                .ToList();
            var objs = new DBObjectCollection();
            parkingLines.ForEach(x => objs.Add(x));
            var nodeGeo = objs.ToNTSNodedLineStrings();
            var handleLines = new List<Line>();
            if (nodeGeo != null)
            {
                handleLines = nodeGeo.ToDbObjects()
                .SelectMany(x =>
                {
                    DBObjectCollection entitySet = new DBObjectCollection();
                    (x as Polyline).Explode(entitySet);
                    return entitySet.Cast<Line>().ToList();
                })
                .Where(x => x.Length > 2)
                .ToList();
            }

            var pLines = ClassifyParkingLinesByConnect(handleLines);
            //总长度更长的分为主车道
            var xPLines = pLines[0];
            var yPLines = pLines[1];
            if (xPLines.Sum(x => x.Length) < yPLines.Sum(x => x.Length))
            {
                xPLines = pLines[1];
                yPLines = pLines[0];
            }

            var resLines = HandleLinesByDirection(xPLines);
            otherPLins = HandleLinesByDirection(yPLines);

            return resLines;
        }

        /// <summary>
        /// 根据x方向和y方向分类车位线
        /// </summary>
        /// <param name="parkingLines"></param>
        /// <returns></returns>
        public List<List<Line>> ClassifyParkingLines(List<Line> parkingLines)
        {
            List<Line> yPLines = new List<Line>();
            List<Line> xPLines = new List<Line>();
            var maxLengthLine = parkingLines.OrderByDescending(x => x.Length).First();
            var xAxis = (maxLengthLine.EndPoint - maxLengthLine.StartPoint).GetNormal();
            var yAxis = Vector3d.ZAxis.CrossProduct(xAxis);
            foreach (var pLine in parkingLines)
            {
                Vector3d pDir = (pLine.EndPoint - pLine.StartPoint).GetNormal();
                double yDotValue = pDir.DotProduct(yAxis);
                double xDotValue = pDir.DotProduct(xAxis);
                if (Math.Abs(yDotValue) > Math.Abs(xDotValue))
                {
                    yPLines.Add(pLine);
                }
                else
                {
                    xPLines.Add(pLine);
                }
            }

            return new List<List<Line>>() { xPLines, yPLines };
        }

        /// <summary>
        /// 根据x方向和y方向分类车位线（处理z字车道）
        /// </summary>
        /// <param name="parkingLines"></param>
        /// <returns></returns>
        public List<List<Line>> ClassifyParkingLinesByConnect(List<Line> parkingLines)
        {
            var pLines = ClassifyParkingLines(parkingLines);
            List<Line> xPLines = pLines[0];
            List<Line> yPLines = pLines[1];

            List<Line> xResLines = new List<Line>();
            foreach (var line in xPLines)
            {
                if (xPLines.Where(x => line.StartPoint.IsEqualTo(x.StartPoint, new Tolerance(1, 1)) ||
                    line.EndPoint.IsEqualTo(x.StartPoint, new Tolerance(1, 1))).Count() <= 1 &&
                    xPLines.Where(x => line.StartPoint.IsEqualTo(x.EndPoint, new Tolerance(1, 1)) ||
                    line.EndPoint.IsEqualTo(x.EndPoint, new Tolerance(1, 1))).Count() <= 1)
                {
                    if (yPLines.Where(x => line.StartPoint.IsEqualTo(x.StartPoint, new Tolerance(1, 1)) ||
                    line.EndPoint.IsEqualTo(x.StartPoint, new Tolerance(1, 1))).Count() == 1 &&
                    yPLines.Where(x => line.StartPoint.IsEqualTo(x.EndPoint, new Tolerance(1, 1)) ||
                    line.EndPoint.IsEqualTo(x.EndPoint, new Tolerance(1, 1))).Count() == 1)
                    {
                        xResLines.Add(line);
                    }
                }
            }

            List<Line> yResLines = new List<Line>();
            foreach (var line in yPLines)
            {
                if (yPLines.Where(x => line.StartPoint.IsEqualTo(x.StartPoint, new Tolerance(1, 1)) ||
                    line.EndPoint.IsEqualTo(x.StartPoint, new Tolerance(1, 1))).Count() <= 1 &&
                    yPLines.Where(x => line.StartPoint.IsEqualTo(x.EndPoint, new Tolerance(1, 1)) ||
                    line.EndPoint.IsEqualTo(x.EndPoint, new Tolerance(1, 1))).Count() <= 1)
                {
                    if (xPLines.Where(x => line.StartPoint.IsEqualTo(x.StartPoint, new Tolerance(1, 1)) ||
                    line.EndPoint.IsEqualTo(x.StartPoint, new Tolerance(1, 1))).Count() == 1 &&
                    xPLines.Where(x => line.StartPoint.IsEqualTo(x.EndPoint, new Tolerance(1, 1)) ||
                    line.EndPoint.IsEqualTo(x.EndPoint, new Tolerance(1, 1))).Count() == 1)
                    {
                        yResLines.Add(line);
                    }
                }
            }

            xPLines = xPLines.Except(xResLines).ToList();
            yPLines = yPLines.Except(yResLines).ToList();
            yPLines.AddRange(xResLines);
            xPLines.AddRange(yResLines);

            return new List<List<Line>>() { xPLines, yPLines };
        }

        /// <summary>
        /// 找到首尾相接的线
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public List<List<Line>> HandleLinesByDirection(List<Line> lines)
        {
            List<List<Line>> resLines = new List<List<Line>>();
            while (lines.Count > 0)
            {
                List<Line> matchLines = new List<Line>();
                Line firLine = lines.First();
                lines.Remove(firLine);
                matchLines.Add(firLine);

                Point3d sPt = firLine.StartPoint;
                Point3d ePt = firLine.EndPoint;
                while (true)
                {
                    var sMLine = lines.FirstOrDefault(x => x.StartPoint.DistanceTo(sPt) < parkingLineTolerance);
                    if (sMLine != null)
                    {
                        matchLines.Add(sMLine);
                        lines.Remove(sMLine);
                        sPt = sMLine.EndPoint;
                    }

                    var eMLine = lines.FirstOrDefault(x => x.EndPoint.DistanceTo(sPt) < parkingLineTolerance);
                    if (eMLine != null)
                    {
                        matchLines.Add(eMLine);
                        lines.Remove(eMLine);
                        sPt = eMLine.StartPoint;
                    }

                    if (sMLine == null && eMLine == null)
                    {
                        break;
                    }
                }

                while (true)
                {
                    var sMLine = lines.FirstOrDefault(x => x.StartPoint.DistanceTo(ePt) < parkingLineTolerance);
                    if (sMLine != null)
                    {
                        matchLines.Add(sMLine);
                        lines.Remove(sMLine);
                        ePt = sMLine.EndPoint;
                    }

                    var eMLine = lines.FirstOrDefault(x => x.EndPoint.DistanceTo(ePt) < parkingLineTolerance);
                    if (eMLine != null)
                    {
                        matchLines.Add(eMLine);
                        lines.Remove(eMLine);
                        ePt = eMLine.StartPoint;
                    }

                    if (sMLine == null && eMLine == null)
                    {
                        break;
                    }
                }
                resLines.Add(matchLines);
            }

            return resLines;
        }

        /// <summary>
        /// 处理车道线
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public List<Line> HandleLines(List<Line> lines)
        {
            List<Line> resLines = new List<Line>();
            foreach (var line in lines)
            {
                var otherlines = lines.Where(x => !(x.StartPoint.IsEqualTo(line.StartPoint) && x.EndPoint.IsEqualTo(line.EndPoint)))
                    .Where(x =>
                    {
                        var p1 = x.GetClosestPointTo(line.StartPoint, false);
                        var p2 = x.GetClosestPointTo(line.EndPoint, false);
                        if (p1.DistanceTo(line.StartPoint) < pointTol || p2.DistanceTo(line.EndPoint) < pointTol)
                        {
                            return true;
                        }
                        return false;
                    })
                    .Where(x => Math.Abs(line.Delta.GetNormal().DotProduct(x.Delta.GetNormal())) > tol)
                    .OrderByDescending(x => x.Length)
                    .ToList();
                if (otherlines.Count > 0)
                {
                    var firLine = otherlines.First();
                    if (firLine.Length >= line.Length)
                    {
                        Point3d sp = firLine.GetClosestPointTo(line.StartPoint, true);
                        Point3d ep = firLine.GetClosestPointTo(line.EndPoint, true);
                        resLines.Add(new Line(sp, ep));
                    }
                    else
                    {
                        resLines.Add(line);
                    }
                }
                else
                {
                    resLines.Add(line);
                }
            }

            return resLines;
        }

        /// <summary>
        /// 按首尾相接的顺序将车道线添加到集合中
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public List<Line> HandleParkingLines(List<Line> lines, out Point3d sPt, out Point3d ePt)
        {
            var xDir = (lines.First().EndPoint - lines.First().StartPoint).GetNormal();
            var zDir = Vector3d.ZAxis;
            var yDir = zDir.CrossProduct(xDir);
            Matrix3d matrix = new Matrix3d(
                new double[] {
                    xDir.X, yDir.X, zDir.X, 0,
                    xDir.Y, yDir.Y, zDir.Y, 0,
                    xDir.Z, yDir.Z, zDir.Z, 0,
                    0.0, 0.0, 0.0, 1.0
            });

            List<Point3d> allPts = lines.SelectMany(x => new List<Point3d>() { x.StartPoint, x.EndPoint }).Select(x => x.TransformBy(matrix)).ToList();
            List<Point3d> oneNodePts = new List<Point3d>();
            foreach (var pt in allPts)
            {
                if (allPts.Where(x => x.IsEqualTo(pt, new Tolerance(1, 1))).Count() <= 1)
                {
                    oneNodePts.Add(pt);
                }
            }
            if (oneNodePts.Count <= 0)
            {
                oneNodePts = allPts;
            }
            sPt = oneNodePts.OrderBy(x => x.X).First().TransformBy(matrix.Inverse());
            ePt = oneNodePts.OrderByDescending(x => x.X).First().TransformBy(matrix.Inverse());

            var handleLines = new List<Line>(lines);
            Point3d comparePt = sPt;
            List<Line> resLines = new List<Line>();
            while (handleLines.Count > 0)
            {
                var matchLine = handleLines.Where(x => x.StartPoint.DistanceTo(comparePt) < parkingLineTolerance
                    || x.EndPoint.DistanceTo(comparePt) < parkingLineTolerance).FirstOrDefault();
                if (matchLine == null)
                {
                    break;
                }

                comparePt = matchLine.StartPoint.DistanceTo(comparePt) < parkingLineTolerance ? matchLine.EndPoint : matchLine.StartPoint;
                handleLines.Remove(matchLine);
                resLines.Add(matchLine);
            }

            return resLines;
        }

        /// <summary>
        /// 将线调整为从起点按顺着的方向走下去
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="sPt"></param>
        /// <returns></returns>
        public List<Line> HandleParkingLinesDir(List<Line> lines, Point3d sPt)
        {
            var handleLines = new List<Line>(lines);
            Point3d comparePt = sPt;
            List<Line> resLines = new List<Line>();
            while (handleLines.Count > 0)
            {
                var matchLine = handleLines.Where(x => x.StartPoint.DistanceTo(comparePt) < parkingLineTolerance
                    || x.EndPoint.DistanceTo(comparePt) < parkingLineTolerance).FirstOrDefault();
                if (matchLine == null)
                {
                    break;
                }

                handleLines.Remove(matchLine);
                if (matchLine.EndPoint.DistanceTo(comparePt) < parkingLineTolerance)
                {
                    matchLine = new Line(matchLine.EndPoint, matchLine.StartPoint);
                }
                comparePt = matchLine.EndPoint;
                resLines.Add(matchLine);
            }

            return resLines;
        }

        /// <summary>
        /// 将车道线创建为polyline
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public Polyline CreateParkingLineToPolyline(List<Line> lines)
        {
            HandleParkingLines(lines, out Point3d sp, out Point3d ep);
            lines = HandleParkingLinesDir(lines, sp);
            List<Point3d> allPts = new List<Point3d>();
            foreach (var line in lines)
            {
                if (allPts.Where(x => x.IsEqualTo(line.StartPoint)).Count() <= 0)
                {
                    allPts.Add(line.StartPoint);
                }

                if (allPts.Where(x => x.IsEqualTo(line.EndPoint)).Count() <= 0)
                {
                    allPts.Add(line.EndPoint);
                }
            }

            Polyline polyline = new Polyline();
            for (int i = 0; i < allPts.Count; i++)
            {
                polyline.AddVertexAt(i, allPts[i].ToPoint2D(), 0, 0, 0);
            }

            return polyline;
        }

        /// <summary>
        /// 将车道线创建为polyline(容差内合并成一根线)
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public Polyline CreateParkingLineToPolylineByTol(List<Line> lines)
        {
            lines = lines.OrderByDescending(x => x.Length).ToList();
            HandleParkingLines(lines, out Point3d sp, out Point3d ep);
            lines = HandleParkingLinesDir(lines, sp);
            List<Point3d> allPts = new List<Point3d>();
            foreach (var line in lines)
            {
                var sPtLst = allPts.Where(x => x.DistanceTo(line.StartPoint) < parkingLineTolerance).ToList();
                if (sPtLst.Count <= 0)
                {
                    allPts.Add(line.StartPoint);
                    allPts.Add(line.EndPoint);
                }
                else
                {
                    var dir = (line.EndPoint - line.StartPoint).GetNormal();
                    Ray ray = new Ray();
                    ray.UnitDir = dir;
                    ray.BasePoint = sPtLst.First();
                    var ePt = ray.GetClosestPointTo(line.EndPoint, true);
                    allPts.Add(ePt);
                }
            }

            Polyline polyline = new Polyline();
            for (int i = 0; i < allPts.Count; i++)
            {
                polyline.AddVertexAt(i, allPts[i].ToPoint2D(), 0, 0, 0);
            }

            return polyline;
        }
    }
}
