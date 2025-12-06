using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.BoundaryRepresentation;

namespace ThCADExtension
{
    public static class ThRegionTool
    {
        ///<summary>
        /// Returns whether a Region contains a Point3d.
        ///</summary>
        ///<param name="pt">A points to test against the Region.</param>
        ///<returns>A Boolean indicating whether the Region contains
        /// the point.
        /// </returns>
        public static bool ContainsPoint(this Region reg, Point3d pt)
        {
            using (var brep = new Brep(reg))
            {
                var pc = new PointContainment();
                using (var brepEnt = brep.GetPointContainment(pt, out pc))
                {
                    return pc != PointContainment.Outside;
                }
            }
        }

        ///<summary>
        /// Returns whether a Region contains a set of Point3ds.
        ///</summary>
        ///<param name="pts">An array of points to test against the Region.</param>
        ///<returns>A Boolean indicating whether the Region contains
        /// all the points.
        /// </returns>
        public static bool ContainsPoints(this Region reg, Point3dCollection ptc)
        {
            var pts = new Point3d[ptc.Count];
            ptc.CopyTo(pts, 0);
            return reg.ContainsPoints(pts);
        }

        ///<summary>
        /// Returns whether a Region contains a set of Point3ds.
        ///</summary>
        ///<param name="pts">An array of points to test against the Region.</param>
        ///<returns>A Boolean indicating whether the Region contains
        /// all the points.
        /// </returns>
        public static bool ContainsPoints(this Region reg, Point3d[] pts)
        {
            using (var brep = new Brep(reg))
            {
                foreach (var pt in pts)
                {
                    var pc = new PointContainment();
                    using (var brepEnt = brep.GetPointContainment(pt, out pc))
                    {
                        if (pc == PointContainment.Outside)
                            return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 获取Region的顶点
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        public static Point3dCollection Vertices(this Region region)
        {
            var vertices = new Point3dCollection();
            if (!region.IsNull)
            {
                using (var brepRegion = new Brep(region))
                {
                    foreach (var face in brepRegion.Faces)
                    {
                        foreach (var loop in face.Loops)
                        {
                            foreach (var vertex in loop.Vertices)
                            {
                                vertices.Add(vertex.Point);
                            }
                        }
                    }
                }
            }
            return vertices;
        }

        /// <summary>
        /// 从Region获取Polylines
        /// </summary>
        /// <param name="reg"></param>
        /// <returns></returns>
        /// https://www.keanw.com/2008/08/creating-a-seri.html
        public static DBObjectCollection ToPolylines(this Region reg)
        {
            // We will return a collection of entities
            // (should include closed Polylines and other
            // closed curves, such as Circles)
            DBObjectCollection res = new DBObjectCollection();
            // Explode Region -> collection of Curves / Regions
            DBObjectCollection cvs = new DBObjectCollection();
            reg.Explode(cvs);

            // Create a plane to convert 3D coords
            // into Region coord system
            Plane pl = new Plane(new Point3d(0, 0, 0), reg.Normal);
            using (pl)
            {
                bool finished = false;
                while (!finished && cvs.Count > 0)
                {
                    // Count the Curves and the non-Curves, and find
                    // the index of the first Curve in the collection
                    int cvCnt = 0, nonCvCnt = 0, fstCvIdx = -1;
                    for (int i = 0; i < cvs.Count; i++)
                    {
                        Curve tmpCv = cvs[i] as Curve;
                        if (tmpCv == null)
                            nonCvCnt++;
                        else
                        {
                            // Closed curves can go straight into the
                            // results collection, and aren't added
                            // to the Curve count
                            if (tmpCv.Closed)
                            {
                                res.Add(tmpCv);
                                cvs.Remove(tmpCv);
                                // Decrement, so we don't miss an item
                                i--;
                            }
                            else
                            {
                                cvCnt++;
                                if (fstCvIdx == -1)
                                    fstCvIdx = i;
                            }
                        }
                    }

                    if (fstCvIdx >= 0)
                    {
                        // For the initial segment take the first
                        // Curve in the collection
                        Curve fstCv = (Curve)cvs[fstCvIdx];
                        // The resulting Polyline
                        Polyline p = new Polyline();
                        // Set common entity properties from the Region
                        p.SetPropertiesFrom(reg);

                        // Add the first two vertices, but only set the
                        // bulge on the first (the second will be set
                        // retroactively from the second segment)
                        // We also assume the first segment is counter-
                        // clockwise (the default for arcs), as we're
                        // not swapping the order of the vertices to
                        // make them fit the Polyline's order
                        p.AddVertexAt(
                          p.NumberOfVertices,
                          fstCv.StartPoint.Convert2d(pl),
                          fstCv.BulgeFromCurve(false), 0, 0

                        );
                        p.AddVertexAt(
                          p.NumberOfVertices,
                          fstCv.EndPoint.Convert2d(pl),
                          0, 0, 0
                        );

                        cvs.Remove(fstCv);
                        // The next point to look for
                        Point3d nextPt = fstCv.EndPoint;
                        // We no longer need the curve
                        fstCv.Dispose();
                        // Find the line that is connected to
                        // the next point
                        // If for some reason the lines returned were not
                        // connected, we could loop endlessly.
                        // So we store the previous curve count and assume
                        // that if this count has not been decreased by
                        // looping completely through the segments once,
                        // then we should not continue to loop.
                        // Hopefully this will never happen, as the curves
                        // should form a closed loop, but anyway...
                        // Set the previous count as artificially high,
                        // so that we loop once, at least.
                        int prevCnt = cvs.Count + 1;
                        while (cvs.Count > nonCvCnt && cvs.Count < prevCnt)
                        {
                            prevCnt = cvs.Count;
                            foreach (DBObject obj in cvs)
                            {
                                Curve cv = obj as Curve;
                                if (cv != null)
                                {
                                    // If one end of the curve connects with the
                                    // point we're looking for...
                                    if (cv.StartPoint == nextPt || cv.EndPoint == nextPt)
                                    {
                                        // Calculate the bulge for the curve and
                                        // set it on the previous vertex
                                        double bulge = cv.BulgeFromCurve(cv.EndPoint == nextPt);
                                        if (bulge != 0.0)
                                            p.SetBulgeAt(p.NumberOfVertices - 1, bulge);

                                        // Reverse the points, if needed
                                        if (cv.StartPoint == nextPt)
                                            nextPt = cv.EndPoint;
                                        else
                                            // cv.EndPoint == nextPt
                                            nextPt = cv.StartPoint;

                                        // Add out new vertex (bulge will be set next
                                        // time through, as needed)
                                        p.AddVertexAt(
                                          p.NumberOfVertices,
                                          nextPt.Convert2d(pl),
                                          0, 0, 0
                                        );

                                        // Remove our curve from the list, which
                                        // decrements the count, of course
                                        cvs.Remove(cv);
                                        cv.Dispose();

                                        break;
                                    }
                                }
                            }
                        }


                        // Once we have added all the Polyline's vertices,
                        // transform it to the original region's plane
                        p.TransformBy(Matrix3d.PlaneToWorld(pl));
                        res.Add(p);

                        if (cvs.Count == nonCvCnt)
                            finished = true;
                    }


                    // If there are any Regions in the collection,
                    // recurse to explode and add their geometry
                    if (nonCvCnt > 0 && cvs.Count > 0)
                    {
                        foreach (DBObject obj in cvs)
                        {
                            Region subReg = obj as Region;
                            if (subReg != null)
                            {
                                DBObjectCollection subRes = subReg.ToPolylines();
                                foreach (DBObject o in subRes)
                                    res.Add(o);

                                cvs.Remove(subReg);
                                subReg.Dispose();
                            }
                        }
                    }

                    if (cvs.Count == 0)
                        finished = true;
                }
            }
            return res;
        }
    }
}
