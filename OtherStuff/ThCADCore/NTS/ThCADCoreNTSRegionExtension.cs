using System;
using System.Linq;
using ThCADExtension;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSRegionExtension
    {
        public static Polygon ToNTSPolygon(this Region region)
        {
            // 暂时不支持"复杂面域"
            var plines = region.ToPolylines();
            if (plines.Count != 1)
            {
                throw new NotSupportedException();
            }

            // 返回由面域外轮廓线封闭的多边形区域
            var pline = plines[0] as Polyline;
            return pline.ToNTSPolygon();
        }

        public static Region ToDbRegion(this Polygon polygon)
        {
            try
            {
                // 暂时不考虑有“洞”的情况
                var curves = new DBObjectCollection
                {
                    polygon.Shell.ToDbPolyline()
                };
                return Region.CreateFromCurves(curves)[0] as Region;
            }
            catch
            {
                // 未知错误
                return null;
            }
        }

        public static Region Union(this Region pRegion, Region sRegion)
        {
            var pGeometry = pRegion.ToNTSPolygon();
            var sGeometry = sRegion.ToNTSPolygon();
            if (pGeometry == null || sGeometry == null)
            {
                return null;
            }

            // 检查是否相交
            if (!pGeometry.Intersects(sGeometry))
            {
                return null;
            }

            // 若相交，则计算共用部分
            var rGeometry = pGeometry.Union(sGeometry);
            if (rGeometry is Polygon polygon)
            {
                return polygon.ToDbRegion();
            }

            return null;
        }

        public static Region Intersection(this Region pRegion, Region sRegion)
        {
            var pGeometry = pRegion.ToNTSPolygon();
            var sGeometry = sRegion.ToNTSPolygon();
            if (pGeometry == null || sGeometry == null)
            {
                return null;
            }

            // 检查是否相交
            if (!pGeometry.Intersects(sGeometry))
            {
                return null;
            }

            // 若相交，则计算相交部分
            var rGeometry = pGeometry.Intersection(sGeometry);
            if (rGeometry is Polygon polygon)
            {
                return polygon.ToDbRegion();
            }

            return null;
        }

        public static List<Polyline> Difference(this Region pRegion, Region sRegion)
        {
            var regions = new List<Polyline>();
            var pGeometry = pRegion.ToNTSPolygon();
            var sGeometry = sRegion.ToNTSPolygon();
            if (pGeometry == null || sGeometry == null)
            {
                return regions;
            }

            // 检查是否相交
            if (!pGeometry.Intersects(sGeometry))
            {
                return regions;
            }

            // 若相交，则计算在pRegion，但不在sRegion的部分
            var rGeometry = pGeometry.Difference(sGeometry);
            if (rGeometry is Polygon polygon)
            {
                regions.Add(polygon.Shell.ToDbPolyline());
            }
            else if (rGeometry is MultiPolygon mPolygon)
            {
                regions.AddRange(mPolygon.ToDbPolylines());
            }
            else
            {
                // 为止情况，抛出异常
                throw new NotSupportedException();
            }
            return regions;
        }

        public static Geometry Intersect(this Region pRegion, Region sRegion)
        {
            var pGeometry = pRegion.ToNTSPolygon();
            var sGeometry = sRegion.ToNTSPolygon();
            if (pGeometry == null || sGeometry == null)
            {
                return null;
            }

            // 检查是否相交
            if (!pGeometry.Intersects(sGeometry))
            {
                return null;
            }

            // 若相交，则计算相交部分
            return pGeometry.Intersection(sGeometry);
        }

        public static List<Polyline> Difference(this Region pRegion, DBObjectCollection sRegions)
        {
            var regions = new List<Polyline>();
            try
            {
                var pGeometry = pRegion.ToNTSPolygon();
                var sGeometry = sRegions.ToNTSMultiPolygon();
                if (pGeometry == null || sGeometry == null)
                {
                    return regions;
                }

                // 检查是否相交
                if (!pGeometry.Intersects(sGeometry))
                {
                    return regions;
                }

                // 若相交，则计算在pRegion，但不在sRegion的部分
                var rGeometry = pGeometry.Difference(sGeometry);
                if (rGeometry is Polygon polygon)
                {
                    regions.Add(polygon.Shell.ToDbPolyline());
                }
                else if (rGeometry is MultiPolygon mPolygon)
                {
                    regions.AddRange(mPolygon.ToDbPolylines());
                }
                else
                {
                    // 为止情况，抛出异常
                    throw new NotSupportedException();
                }
            }
            catch
            {
                // 在某些情况下，NTS会抛出异常
                // 这里只捕捉异常，不做特殊的处理
            }
            return regions;
        }

        public static List<Polyline> Differences(this Region pRegion, DBObjectCollection sRegions)
        {
            var pGeometrys = new List<Polygon>();
            try
            {
                pGeometrys.Add(pRegion.ToNTSPolygon());
                foreach (DBObject sGe in sRegions)
                {
                    var sGeometry = new DBObjectCollection() { sGe }.ToNTSMultiPolygon();
                    foreach (var pGeometry in pGeometrys)
                    {
                        if (pGeometry == null || sGeometry == null)
                        {
                            continue;
                        }

                        // 检查是否相交
                        if (!pGeometry.Intersects(sGeometry))
                        {
                            continue;
                        }

                        // 若相交，则计算在pRegion，但不在sRegion的部分
                        var rGeometry = pGeometry.Difference(sGeometry);
                        if (rGeometry is Polygon polygon)
                        {
                            pGeometrys = new List<Polygon>() { polygon };
                        }
                        else if (rGeometry is MultiPolygon mPolygon)
                        {
                            pGeometrys = new List<Polygon>();
                            foreach (Polygon rPolygon in mPolygon.Geometries)
                            {
                                pGeometrys.Add(rPolygon);
                            }
                        }
                        else
                        {
                            // 为止情况，抛出异常
                            throw new NotSupportedException();
                        }
                    }
                }
            }
            catch
            {
                // 在某些情况下，NTS会抛出异常
                // 这里只捕捉异常，不做特殊的处理
            }
            return pGeometrys.Select(x => x.Shell.ToDbPolyline()).ToList();
        }
    }
}
