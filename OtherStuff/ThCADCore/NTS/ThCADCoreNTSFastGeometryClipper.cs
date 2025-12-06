using System;
using System.Linq;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Precision;

namespace ThCADCore.NTS
{
    public class ThCADCoreNTSFastGeometryClipper
    {
        private const int RIGHT = 2;
        private const int TOP = 8;
        private const int BOTTOM = 4;
        private const int LEFT = 1;

        private double xmin;
        private double ymin;
        private double xmax;
        private double ymax;

        public Envelope bounds { get; set; }

        public ThCADCoreNTSFastGeometryClipper(Envelope bounds)
        {
            this.xmin = bounds.MinX;
            this.ymin = bounds.MinY;
            this.xmax = bounds.MaxX;
            this.ymax = bounds.MaxY;
            this.bounds = bounds;
        }

        public Geometry clipSafe(Geometry g, bool ensureValid, double scale)
        {
            try
            {
                return clip(g, ensureValid);
            }
            catch (TopologyException)
            {
                try
                {
                    if ((g is Polygon || g is MultiPolygon) && !g.IsValid)
                    {
                        // its an invalid Polygon or MultiPolygon. Use buffer(0) to attempt to fix it
                        // do not use Buffer(0) on points or lines - it returns an empty polygon
                        return clip(g.Buffer(0), ensureValid);
                    }
                }
                catch (TopologyException)
                {
                }

                if (scale != 0)
                {
                    // Step 2: Snap to provided scale
                    try
                    {
                        GeometryPrecisionReducer reducer =
                                new GeometryPrecisionReducer(new PrecisionModel(scale));

                        // reduce method already tries to fix problems with geometry (ie buffer(0) if
                        // invalid)
                        Geometry reduced = reducer.Reduce(g);
                        if (reduced.IsEmpty)
                        {
                            throw new TopologyException("Could not snap geometry to precision model");
                        }
                        return clip(reduced, ensureValid);
                    }
                    catch (TopologyException)
                    {
                        // if this fails, continue with other methods
                    }
                }
                if (ensureValid)
                {
                    try
                    {
                        // Step 3: try again with ensureValid false
                        return clip(g, false);
                    }
                    catch (TopologyException)
                    {
                    }
                }
                return g; // unable to clip geometry
            }
        }

        public Geometry clip(Geometry g, bool ensureValid)
        {
            // basic pre-flight checks
            if (g == null)
            {
                return null;
            }
            Envelope geomEnvelope = g.EnvelopeInternal;
            if (geomEnvelope.IsNull())
            {
                return null;
            }
            if (bounds.Contains(geomEnvelope))
            {
                return g;
            }
            else if (!bounds.Intersects(geomEnvelope))
            {
                return null;
            }

            // clip for good
            if (g is LineString lineString)
            {
                return clipLineString(lineString);
            }
            else if (g is Polygon polygon)
            {
                if (ensureValid)
                {
                    GeometryFactory gf = polygon.Factory;
                    Polygon fence = gf.CreatePolygon(buildBoundsString(gf), null);
                    return polygon.Intersection(fence);
                }
                else
                {
                    return clipPolygon(polygon);
                }
            }
            else if (g is GeometryCollection collection)
            {
                return clipCollection(collection, ensureValid);
            }
            else
            {
                // still don't know how to clip this
                return g;
            }
        }

        /** Cohen-Sutherland outcode, see http://en.wikipedia.org/wiki/Cohen%E2%80%93Sutherland */
        private int computeOutCode(double x, double y, double xmin, double ymin, double xmax, double ymax)
        {
            int code = 0;
            if (y > ymax) code |= TOP;
            else if (y < ymin) code |= BOTTOM;
            if (x > xmax) code |= RIGHT;
            else if (x < xmin) code |= LEFT;
            return code;
        }


        /** Cohen sutherland based segment clipping */
        private double[] clipSegment(double[] segment)
        {
            // dump to local variables to avoid the array access check overhead
            double x0 = segment[0];
            double y0 = segment[1];
            double x1 = segment[2];
            double y1 = segment[3];

            // compute outcodes
            int outcode0 = computeOutCode(x0, y0, xmin, ymin, xmax, ymax);
            int outcode1 = computeOutCode(x1, y1, xmin, ymin, xmax, ymax);

            int step = 0;
            do
            {
                if ((outcode0 | outcode1) == 0)
                {
                    // check if we got a degenerate segment
                    if (x0 == x1 && y0 == y1)
                    {
                        return null;
                    }

                    // both points are inside the clip area
                    segment[0] = x0;
                    segment[1] = y0;
                    segment[2] = x1;
                    segment[3] = y1;
                    return segment;
                }
                else if ((outcode0 & outcode1) > 0)
                {
                    // both points are outside of the clip area,
                    // and on a same side (both top, both bottom, etc)
                    return null;
                }
                else
                {
                    // failed both tests, so calculate the line segment to clip
                    // from an outside point to an intersection with clip edge

                    // At least one endpoint is outside the clip rectangle; pick it.
                    int outcodeOut = outcode0 != 0 ? outcode0 : outcode1;
                    // Now find the intersection point;
                    // use formulas y = y0 + slope * (x - x0),
                    // x = x0 + (1/slope) * (y - y0)
                    // depending on which side we're clipping
                    // Note we might end up getting a point that is still outside (touches one side
                    // but out on the other)
                    double x, y;
                    if ((outcodeOut & TOP) > 0)
                    {
                        x = x0 + (x1 - x0) * (ymax - y0) / (y1 - y0);
                        y = ymax;
                    }
                    else if ((outcodeOut & BOTTOM) > 0)
                    {
                        x = x0 + (x1 - x0) * (ymin - y0) / (y1 - y0);
                        y = ymin;
                    }
                    else if ((outcodeOut & RIGHT) > 0)
                    {
                        y = y0 + (y1 - y0) * (xmax - x0) / (x1 - x0);
                        x = xmax;
                    }
                    else
                    { // LEFT
                        y = y0 + (y1 - y0) * (xmin - x0) / (x1 - x0);
                        x = xmin;
                    }
                    // We sliced at least one ordinate, recompute the outcode for the end we
                    // modified
                    if (outcodeOut == outcode0)
                    {
                        x0 = x;
                        y0 = y;
                        outcode0 = computeOutCode(x0, y0, xmin, ymin, xmax, ymax);
                    }
                    else
                    {
                        x1 = x;
                        y1 = y;
                        outcode1 = computeOutCode(x1, y1, xmin, ymin, xmax, ymax);
                    }
                }

                step++;
            } while (step < 5);

            // we should really never get here, the algorithm must at most clip two ends,
            // at worst one ordinate at a time, so at most 4 steps
            // tag
            throw new SystemException("Algorithm did not converge");
        }

        /** Checks if the specified segment it outside the clipping bounds */
        private bool outside(double x0, double y0, double x1, double y1)
        {
            int outcode0 = computeOutCode(x0, y0, xmin, ymin, xmax, ymax);
            int outcode1 = computeOutCode(x1, y1, xmin, ymin, xmax, ymax);

            return ((outcode0 & outcode1) > 0);
        }

        /** Checks if the point is inside the clipping bounds */
        private bool contained(double x, double y)
        {
            return x > xmin && x < xmax && y > ymin && y < ymax;
        }

        /**
         * Clips a polygon using the Liang-Barsky helper routine. Does not generate, in general, valid
         * polygons (but still does generate polygons good enough for rendering)
         */
        private Geometry clipPolygon(Polygon polygon)
        {
            GeometryFactory gf = polygon.Factory;

            LinearRing exterior = (LinearRing)polygon.ExteriorRing;
            LinearRing shell = polygonClip(exterior);
            shell = cleanupRings(shell);
            if (shell == null)
            {
                return null;
            }

            List<LinearRing> holes = new List<LinearRing>();
            for (int i = 0; i < polygon.NumInteriorRings; i++)
            {
                LinearRing hole = (LinearRing)polygon.GetInteriorRingN(i);
                hole = polygonClip(hole);
                hole = cleanupRings(hole);
                if (hole != null)
                {
                    holes.Add(hole);
                }
            }
            //return gf.createPolygon(shell, holes.toArray(new LinearRing[holes.size()]));
            return gf.CreatePolygon(shell, holes.ToArray());
        }

        /**
         * The {@link #polygonClip(LinearRing)} routine can generate invalid rings fully on top of the
         * clipping area borders (with no inside). Do a quick check that does not involve an expensive
         * isValid() call
         *
         * @return The ring, or null if the ring was not valid
         */
        private LinearRing cleanupRings(LinearRing ring)
        {
            if (ring == null || ring.IsEmpty)
            {
                return null;
            }

            CoordinateSequence cs = ring.CoordinateSequence;
            double px = cs.GetX(0);
            double py = cs.GetY(0);
            bool fullyOnBorders = true;
            for (int i = 1; i < cs.Count && fullyOnBorders; i++)
            {
                double x = cs.GetX(i);
                double y = cs.GetY(i);
                // check if the current segment lies on the bbox side fully
                if ((x == px && (x == xmin || x == xmax)) || (y == py && (y == ymin || y == ymax)))
                {
                    px = x;
                    py = y;
                }
                else
                {
                    fullyOnBorders = false;
                }
            }
            // all sides are sitting on the bbox borders, this is the degenerate case
            // we are trying to filter out
            if (fullyOnBorders)
            {
                // could still be a case of a polygon equal to the clipping border itself
                // This area test could actually replace the whole method,
                // but it's more expensive to run, so we use it as a last resort for a specific case
                if (ring.Factory.CreatePolygon(ring).Area > 0)
                {
                    return ring;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return ring;
            }
        }

        /**
         * This routine uses the Liang-Barsky algorithm for polygon clipping as described in Foley & van
         * Dam. It's more efficient Sutherland-Hodgman version, but produces redundent turning vertices
         * at the corners of the clip region. This can make rendering as a series of triangles very
         * awkward, but it's fine of your underlying graphics mechanism has a forgiving drawPolygon
         * routine. This algorithm comes from http://www.longsteve.com/fixmybugs/?p=359, under a "DO
         * WHAT THE FUCK YOU WANT TO PUBLIC LICENSE" (no kidding!)
         */
        private LinearRing polygonClip(LinearRing ring)
        {
            double INFINITY = Double.MaxValue;

            CoordinateSequence cs = ring.CoordinateSequence;

            //tag
            var outTidal = new List<double>();

            // Coordinates of intersection between the infinite line hosting the segment and the clip
            // area
            double xIn, xOut, yIn, yOut;
            // Parameter values of same, they are in [0,1] if the intersections are inside the segment,
            // < 0 or > 1 otherwise
            double tInX, tOutX, tInY, tOutY;
            // tOut2: max between tOutX and tOutY, tIn2: max between tInX and tinY
            double tOut1, tOut2, tIn2;

            // Direction of edge
            double deltaX, deltaY;
            int i;

            // for each edge
            for (i = 0; i < cs.Count - 1; i++)
            {
                // extract the edge
                double x0 = cs.GetOrdinate(i, 0);
                double x1 = cs.GetOrdinate(i + 1, 0);
                double y0 = cs.GetOrdinate(i, 1);
                double y1 = cs.GetOrdinate(i + 1, 1);

                // determine direction of edge
                deltaX = x1 - x0;
                deltaY = y1 - y0;

                // use this to determine which bounding lines for the clip region the
                // containing line hits first (from which side, to which other side)
                if ((deltaX > 0) || (deltaX == 0 && x0 > xmax))
                {
                    xIn = xmin;
                    xOut = xmax;
                }
                else
                {
                    xIn = xmax;
                    xOut = xmin;
                }
                if ((deltaY > 0) || (deltaY == 0 && y0 > ymax))
                {
                    yIn = ymin;
                    yOut = ymax;
                }
                else
                {
                    yIn = ymax;
                    yOut = ymin;
                }

                // find the t values for the x and y exit points
                if (deltaX != 0)
                {
                    tOutX = (xOut - x0) / deltaX;
                }
                else if (x0 <= xmax && xmin <= x0)
                {
                    // vertical line crossing the clip box
                    tOutX = INFINITY;
                }
                else
                {
                    // vertical line outside the clip box
                    tOutX = -INFINITY;
                }

                if (deltaY != 0)
                {
                    tOutY = (yOut - y0) / deltaY;
                }
                else if (y0 <= ymax && ymin <= y0)
                {
                    // horizontal line crossing the clip box
                    tOutY = INFINITY;
                }
                else
                {
                    // horizontal line outside the clip box
                    tOutY = -INFINITY;
                }

                // Order the two exit points
                if (tOutX < tOutY)
                {
                    tOut1 = tOutX;
                    tOut2 = tOutY;
                }
                else
                {
                    tOut1 = tOutY;
                    tOut2 = tOutX;
                }

                // skip tests if exit intersection points are before the
                // beginning of the segment
                if (tOut2 > 0)
                {

                    // now compute the params of the first intersection point
                    if (deltaX != 0)
                    {
                        tInX = (xIn - x0) / deltaX;
                    }
                    else
                    {
                        tInX = -INFINITY;
                    }

                    if (deltaY != 0)
                    {
                        tInY = (yIn - y0) / deltaY;
                    }
                    else
                    {
                        tInY = -INFINITY;
                    }

                    // sort them
                    if (tInX < tInY)
                    {
                        tIn2 = tInY;
                    }
                    else
                    {
                        tIn2 = tInX;
                    }

                    if (tOut1 < tIn2)
                    {
                        // no visible segment
                        if (0 < tOut1 && tOut1 <= 1.0)
                        {
                            // line crosses over intermediate corner region
                            if (tInX < tInY)
                            {
                                outTidal.Add(xOut);
                                outTidal.Add(yIn);
                            }
                            else
                            {
                                outTidal.Add(xIn);
                                outTidal.Add(yOut);
                            }
                        }
                    }
                    else
                    {
                        // line crosses though window
                        if (0 < tOut1 && tIn2 <= 1.0)
                        {
                            if (0 <= tIn2)
                            { // visible segment
                                if (tInX > tInY)
                                {
                                    outTidal.Add(xIn);
                                    outTidal.Add(y0 + (tInX * deltaY));
                                }
                                else
                                {
                                    outTidal.Add(x0 + (tInY * deltaX));
                                    outTidal.Add(yIn);
                                }
                            }

                            if (1.0 >= tOut1)
                            {
                                if (tOutX < tOutY)
                                {
                                    outTidal.Add(xOut);
                                    outTidal.Add(y0 + (tOutX * deltaY));
                                }
                                else
                                {
                                    outTidal.Add(x0 + (tOutY * deltaX));
                                    outTidal.Add(yOut);
                                }
                            }
                            else
                            {
                                outTidal.Add(x1);
                                outTidal.Add(y1);
                            }
                        }
                    }

                    if ((0 < tOut2 && tOut2 <= 1.0))
                    {
                        outTidal.Add(xOut);
                        outTidal.Add(yOut);
                    }
                }
            }

            if (outTidal.Count < 3)
            {
                return null;
            }

            if (outTidal[0] != outTidal[outTidal.Count - 2]
                    || outTidal[1] != outTidal[outTidal.Count - 1])
            {
                outTidal.Add(outTidal[0]);
                outTidal.Add(outTidal[1]);
            }
            else if (outTidal.Count == 3)
            {
                return null;
            }

            return ring.Factory.CreateLinearRing(ToCoordinates(outTidal));
        }

        /** Builds a linear ring representing the clipping area */
        public LinearRing buildBoundsString(GeometryFactory gf)
        {
            CoordinateSequence cs = gf.CoordinateSequenceFactory.Create(5, 2);

            cs.SetOrdinate(0, 0, xmin);
            cs.SetOrdinate(0, 1, ymin);
            cs.SetOrdinate(1, 0, xmin);
            cs.SetOrdinate(1, 1, ymax);
            cs.SetOrdinate(2, 0, xmax);
            cs.SetOrdinate(2, 1, ymax);
            cs.SetOrdinate(3, 0, xmax);
            cs.SetOrdinate(3, 1, ymin);
            cs.SetOrdinate(4, 0, xmin);
            cs.SetOrdinate(4, 1, ymin);
            return gf.CreateLinearRing(cs.ToCoordinateArray());
        }

        /** Recursively clips a collection */
        private Geometry clipCollection(GeometryCollection gc, bool ensureValid)
        {
            if (gc.NumGeometries == 1)
            {
                return clip(gc.GetGeometryN(0), ensureValid);
            }
            else
            {
                List<Geometry> result = new List<Geometry>(gc.NumGeometries);
                for (int i = 0; i < gc.NumGeometries; i++)
                {
                    Geometry clipped = clip(gc.GetGeometryN(i), ensureValid);
                    if (clipped != null)
                    {
                        result.Add(clipped);
                    }
                }

                flattenCollection(result);

                if (gc is MultiPoint)
                {
                    result = result.Where(o => o is Point).ToList();
                }
                else if (gc is MultiLineString)
                {
                    result = result.Where(o => o is LineString).ToList();
                }
                else if (gc is MultiPolygon)
                {
                    result = result.Where(o => o is Polygon).ToList();
                }

                if (result.Count == 0)
                {
                    return null;
                }
                else if (result.Count == 1)
                {
                    return result[0];
                }

                flattenCollection(result);

                if (gc is MultiPoint)
                {
                    return gc.Factory.CreateMultiPoint((Point[])result.ToArray());
                }
                else if (gc is MultiLineString)
                {
                    return gc.Factory.CreateMultiLineString((LineString[])result.ToArray());
                }
                else if (gc is MultiPolygon)
                {
                    return gc.Factory.CreateMultiPolygon((Polygon[])result.ToArray());
                }
                else
                {
                    return gc.Factory.CreateGeometryCollection(result.ToArray());
                }
            }
        }

        private void flattenCollection(List<Geometry> result)
        {
            for (int i = 0; i < result.Count;)
            {
                Geometry g = result[i];
                if (g is GeometryCollection)
                {
                    GeometryCollection gc = (GeometryCollection)g;
                    for (int j = 0; j < gc.NumGeometries; j++)
                    {
                        result.Add(gc.GetGeometryN(j));
                    }
                    result.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        private Coordinate[] ToCoordinates(List<double> ordinates)
        {
            var coordinates = new List<Coordinate>();
            for (int i = 0; i < ordinates.Count; i += 2)
            {
                coordinates.Add(new Coordinate(ordinates[i], ordinates[i + 1]));
            }
            return coordinates.ToArray();
        }

        public Geometry clipLineString(LineString line)
        {
            // the result
            List<LineString> clipped = new List<LineString>();

            // grab all the factories a
            GeometryFactory gf = line.Factory;
            CoordinateSequenceFactory csf = gf.CoordinateSequenceFactory;
            CoordinateSequence coords = line.CoordinateSequence;

            // first step
            //Ordinates ordinates = new Ordinates();
            var ordinates = new List<double>();
            double x0 = coords.GetX(0);
            double y0 = coords.GetY(0);
            bool prevInside = contained(x0, y0);
            if (prevInside)
            {
                ordinates.Add(x0);
                ordinates.Add(y0);
            }
            double[] segment = new double[4];
            int size = coords.Count;
            // loop over the other coordinates
            for (int i = 1; i < size; i++)
            {
                double x1 = coords.GetX(i);
                double y1 = coords.GetY(i);

                bool inside = contained(x1, y1);
                if (inside == prevInside)
                {
                    if (inside)
                    {
                        // both segments were inside, not need for clipping
                        ordinates.Add(x1);
                        ordinates.Add(y1);
                    }
                    else
                    {
                        // both were outside, this might still be caused by a line
                        // crossing the envelope but whose endpoints lie outside
                        if (!outside(x0, y0, x1, y1))
                        {
                            segment[0] = x0;
                            segment[1] = y0;
                            segment[2] = x1;
                            segment[3] = y1;
                            double[] clippedSegment = clipSegment(segment);
                            if (clippedSegment != null)
                            {
                                CoordinateSequence cs = csf.Create(2, coords.Dimension, coords.Measures);
                                cs.SetOrdinate(0, 0, clippedSegment[0]);
                                cs.SetOrdinate(0, 1, clippedSegment[1]);
                                cs.SetOrdinate(1, 0, clippedSegment[2]);
                                cs.SetOrdinate(1, 1, clippedSegment[3]);
                                clipped.Add(gf.CreateLineString(cs.ToCoordinateArray()));
                            }
                        }
                    }
                }
                else
                {
                    // one inside, the other outside, a clip must occurr
                    segment[0] = x0;
                    segment[1] = y0;
                    segment[2] = x1;
                    segment[3] = y1;
                    double[] clippedSegment = clipSegment(segment);
                    if (clippedSegment != null)
                    {
                        if (prevInside)
                        {
                            ordinates.Add(clippedSegment[2]);
                            ordinates.Add(clippedSegment[3]);
                        }
                        else
                        {
                            ordinates.Add(clippedSegment[0]);
                            ordinates.Add(clippedSegment[1]);
                            ordinates.Add(clippedSegment[2]);
                            ordinates.Add(clippedSegment[3]);
                        }
                        // if we are going from inside to outside it's time to cut a linestring
                        // into the results
                        if (prevInside)
                        {
                            // if(closed) {
                            // addClosingPoints(ordinates, shell);
                            // clipped.add(gf.createLinearRing(ordinates.toCoordinateSequence(csf)));
                            // } else {
                            // clipped.Add(gf.CreateLineString(ordinates.ToList());
                            // }
                            clipped.Add(gf.CreateLineString(ToCoordinates(ordinates)));
                            ordinates.Clear();
                        }
                    }
                    else
                    {
                        prevInside = false;
                    }
                }
                prevInside = inside;
                x0 = x1;
                y0 = y1;
            }
            // don't forget the last linestring
            if (ordinates.Count > 1)
            {
                clipped.Add(gf.CreateLineString(ToCoordinates(ordinates)));
            }

            if (line.IsClosed && clipped.Count > 1)
            {
                // the first and last strings might be adjacent, in that case fuse them
                CoordinateSequence cs0 = clipped[0].CoordinateSequence;
                CoordinateSequence cs1 = clipped[clipped.Count - 1].CoordinateSequence;
                if (cs0.GetOrdinate(0, 0) == cs1.GetOrdinate(cs1.Count - 1, 0)
                        && cs0.GetOrdinate(0, 1) == cs1.GetOrdinate(cs1.Count - 1, 1))
                {
                    var cs = csf.Create(cs0.Count + cs1.Count - 1, 2);
                    for (int i = 0; i < cs1.Count; i++)
                    {
                        cs.SetOrdinate(i, 0, cs1.GetOrdinate(i, 0));
                        cs.SetOrdinate(i, 1, cs1.GetOrdinate(i, 1));
                    }
                    for (int i = 1; i < cs0.Count; i++)
                    {
                        cs.SetOrdinate(i + cs1.Count - 1, 0, cs0.GetOrdinate(i, 0));
                        cs.SetOrdinate(i + cs1.Count - 1, 1, cs0.GetOrdinate(i, 1));
                    }
                    clipped.RemoveAt(0);
                    clipped.RemoveAt(clipped.Count - 1);
                    clipped.Add(gf.CreateLineString(cs.ToCoordinateArray()));
                }
            }

            // return the results
            if (clipped.Count > 1)
            {
                return gf.CreateMultiLineString(clipped.ToArray());
            }
            else if (clipped.Count == 1)
            {
                return clipped[0];
            }
            else
            {
                return null;
            }
        }
    }
}