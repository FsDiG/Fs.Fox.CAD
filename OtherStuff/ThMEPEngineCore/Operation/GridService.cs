using System;
using Linq2Acad;
using System.Linq;
using ThCADCore.NTS;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using DotNetARX;
using AcHelper;

namespace ThMEPEngineCore.Operation
{
    public class GridService
    {
        readonly double tol = 10;
        double minSpace = 4000;

        /// <summary>
        /// 构建柱轴网
        /// </summary>
        /// <param name="points"></param>
        /// <param name="xLength"></param>
        /// <param name="yLength"></param>
        public List<KeyValuePair<Vector3d, List<Polyline>>> CreateGrid(Polyline polyline, List<Polyline> colums, Matrix3d transMatrix, double spacingValue)
        {
            minSpace = spacingValue;

            List<Point3d> points = GetColumCenter(colums);
            Matrix3d matrix = ThMEPEngineCoreGeUtils.GetGridMatrix(Vector3d.XAxis);

            var firGrids = MoveClosedGrid(CreateGridLine(matrix, points, polyline));
            var secGrids = MoveClosedGrid(CreateGridLine(RotateMatrix(matrix), points, polyline));
#if DEBUG
            string GridLineLayer = "AD-Gird";     //轴网线图层
            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                LayerTools.AddLayer(acdb.Database, GridLineLayer);
                foreach (var fGird in firGrids.Value)
                {
                    var rotateGrid = fGird.Clone() as Polyline;
                    rotateGrid.TransformBy(transMatrix);
                    rotateGrid.Layer = GridLineLayer;
                    acdb.ModelSpace.Add(rotateGrid);
                }
                foreach (var sGird in secGrids.Value)
                {
                    var rotateGrid = sGird.Clone() as Polyline;
                    rotateGrid.TransformBy(transMatrix);
                    rotateGrid.Layer = GridLineLayer;
                    acdb.ModelSpace.Add(rotateGrid);
                }
            }
#endif

            return new List<KeyValuePair<Vector3d, List<Polyline>>>()
            {
                firGrids,
                secGrids,
            };
        }

        /// <summary>
        /// 构建某方向上的轴网线
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="points"></param>
        /// <param name="longLine"></param>
        /// <param name="shortLine"></param>
        /// <returns></returns>
        public KeyValuePair<Vector3d, List<Polyline>> CreateGridLine(Matrix3d matrix, List<Point3d> points, Polyline polyline)
        {
            points = points.Select(x => x.TransformBy(matrix.Inverse())).ToList();
            List<Point3d> linePts = new List<Point3d>();
            for (int i = 0; i < polyline.NumberOfVertices - 1; i++)
            {
                linePts.Add(polyline.GetPoint3dAt(i));
            }

            List<Point3d> polyPts = ThMEPEngineCoreGeUtils.CalBoundingBox(linePts);
            polyPts = polyPts.Select(x => x.TransformBy(matrix.Inverse())).ToList();
            double minY = polyPts.First().Y;
            double maxY = polyPts.Last().Y;
            points.AddRange(polyPts);

            List<Polyline> gridPoly = new List<Polyline>();
            points = points.OrderByDescending(x => x.X).ToList();
            while (points.Count > 0)
            {
                Point3d sp = points.First();
                points.Remove(sp);
                List<Point3d> groupPts = new List<Point3d>() { sp };
                while (true)
                {
                    var matchPts = points.Where(x =>
                    {
                        if (Math.Abs(x.X - sp.X) < tol)
                        {
                            return true;
                        }
                        return false;
                    }).ToList();
                    if (matchPts.Count <= 0)
                    {
                        break;
                    }

                    sp = matchPts.OrderBy(x => x.Y).First();
                    groupPts.Add(sp);
                    points.Remove(sp);
                }

                groupPts.Add(new Point3d(sp.X, maxY, 0));
                groupPts.Add(new Point3d(sp.X, minY, 0));
                groupPts = groupPts.OrderBy(x => x.Y).ToList();
                Polyline poly = new Polyline();
                for (int i = 0; i < groupPts.Count; i++)
                {
                    poly.AddVertexAt(i, groupPts[i].ToPoint2D(), 0, 0, 0);
                }
                gridPoly.Add(poly);
            }

            gridPoly.ForEach(x => x.TransformBy(matrix));
            return new KeyValuePair<Vector3d, List<Polyline>>(matrix.CoordinateSystem3d.Xaxis, gridPoly);
        }

        /// <summary>
        /// 将矩阵旋转90度
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public Matrix3d RotateMatrix(Matrix3d matrix)
        {
            var xDir = matrix.CoordinateSystem3d.Xaxis;
            var yDir = matrix.CoordinateSystem3d.Yaxis;
            var zDir = matrix.CoordinateSystem3d.Zaxis;
            Matrix3d roMatrix = new Matrix3d(
                new double[]{
                    yDir.X, xDir.X, zDir.X, 0,
                    yDir.Y, xDir.Y, zDir.Y, 0,
                    yDir.Z, xDir.Z, zDir.Z, 0,
                    0.0, 0.0, 0.0, 1.0});
            return roMatrix;
        }

        /// <summary>
        /// 计算柱中点
        /// </summary>
        /// <param name="colums"></param>
        /// <returns></returns>
        public List<Point3d> GetColumCenter(List<Polyline> colums)
        {
            List<Point3d> resPoints = new List<Point3d>();
            foreach (var col in colums)
            {
                List<Point3d> points = new List<Point3d>();
                for (int i = 0; i < col.NumberOfVertices; i++)
                {
                    points.Add(col.GetPoint3dAt(i));
                }

                List<Point3d> ptsLst = ThMEPEngineCoreGeUtils.CalBoundingBox(points);
                resPoints.Add(new Point3d((ptsLst.First().X + ptsLst.Last().X) / 2, (ptsLst.First().Y + ptsLst.Last().Y) / 2, 0));
            }

            return resPoints;
        }

        /// <summary>
        /// 去掉离得过近的轴网线
        /// </summary>
        /// <param name="grids"></param>
        private KeyValuePair<Vector3d, List<Polyline>> MoveClosedGrid(KeyValuePair<Vector3d, List<Polyline>> grids)
        {
            var gridLines = grids.Value;
            int index = 1;
            int indexJ = 2;
            List<Polyline> removePolys = new List<Polyline>();
            while (index < gridLines.Count)
            {
                if (indexJ >= gridLines.Count || gridLines.Count <= 2)
                {
                    break;
                }
                var poly = gridLines[index];
                var nextPoly = gridLines[indexJ];

                if (poly.Distance(nextPoly) < minSpace)
                {
                    if (index == 0)
                    {
                        removePolys.Add(nextPoly);
                    }
                    else if (indexJ == gridLines.Count - 1)
                    {
                        removePolys.Add(poly);
                    }
                    else
                    {
                        //removePolys.Add(nextPoly);
                        if (poly.NumberOfVertices >= nextPoly.NumberOfVertices)
                        {
                            removePolys.Add(nextPoly);
                        }
                        else
                        {
                            removePolys.Add(poly);
                            index = indexJ;
                        }
                    }
                }
                else
                {
                    index = indexJ;
                }
                indexJ++;
            }

            return new KeyValuePair<Vector3d, List<Polyline>>(grids.Key, gridLines.Except(removePolys).ToList());
        }

        /// <summary>
        /// 去掉离得过近的轴网线(边界线不参与合并)
        /// </summary>
        /// <param name="grids"></param>
        private KeyValuePair<Vector3d, List<Polyline>> MoveClosedGridNoBoundary(KeyValuePair<Vector3d, List<Polyline>> grids)
        {
            var gridLines = grids.Value;
            int index = 1;
            int indexJ = 2;
            List<Polyline> removePolys = new List<Polyline>();
            while (index < gridLines.Count - 1)
            {
                if (indexJ >= gridLines.Count - 1)
                {
                    break;
                }
                var poly = gridLines[index];
                var nextPoly = gridLines[indexJ];

                if (poly.Distance(nextPoly) < minSpace)
                {
                    removePolys.Add(nextPoly);
                    //if (poly.NumberOfVertices > nextPoly.NumberOfVertices)
                    //{
                    //    removePolys.Add(nextPoly);
                    //}
                    //else
                    //{
                    //    removePolys.Add(poly);
                    //    index = indexJ;
                    //}
                }
                else
                {
                    index = indexJ;
                }
                indexJ++;
            }

            return new KeyValuePair<Vector3d, List<Polyline>>(grids.Key, gridLines.Except(removePolys).ToList());
        }
    }
}
