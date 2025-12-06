using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace ThCADCore.NTS
{
    public class ThCADCoreNTSGeometrySmoother
    {
        public class SmootherControl
        {
            public double MinLength { get; set; }
            public int NumVertices { get; set; }
        }

        public class InterpPoint
        {
            public double[] t;
            public double tsum;

            public InterpPoint()
            {
                t = new double[4];
            }
        }

        public SmootherControl Control { get; set; }

        private Dictionary<int, InterpPoint[]> lookup;

        public ThCADCoreNTSGeometrySmoother()
        {
            Control = new SmootherControl()
            {
                MinLength = 0,
                NumVertices = 10,
            };
            lookup = new Dictionary<int, InterpPoint[]>();
        }

        public LineString Smooth(LineString ls, double alpha)
        {
            Coordinate[] coords = ls.Coordinates;

            Coordinate[,] controlPoints = GetLineControlPoints(coords, alpha);

            int N = coords.Length;
            List<Coordinate> smoothCoords = new List<Coordinate>();
            double dist;
            for (int i = 0; i < N - 1; i++)
            {
                dist = coords[i].Distance(coords[i + 1]);
                
                if (dist < Control.MinLength)
                {
                    // segment too short - just copy input coordinate
                    smoothCoords.Add(new Coordinate(coords[i]));
                }
                else
                {
                    int smoothN = Control.NumVertices;
                    Coordinate[] segment =
                            CubicBezier(
                                    coords[i],
                                    coords[i + 1],
                                    controlPoints[i,1],
                                    controlPoints[i + 1,0],
                                    smoothN);

                    int copyN = i < N - 1 ? segment.Length - 1 : segment.Length;
                    for (int k = 0; k < copyN; k++)
                    {
                        smoothCoords.Add(segment[k]);
                    }
                }
            }
            smoothCoords.Add(coords[N - 1]);

            return ThCADCoreNTSService.Instance.GeometryFactory.CreateLineString(smoothCoords.ToArray());
        }

        public Polygon Smooth(Polygon p, double alpha)
        {
            Coordinate[] coords = p.ExteriorRing.Coordinates;
            int N = coords.Length - 1; 

            Coordinate[,] controlPoints = GetPolygonControlPoints(coords, N, alpha);

            List<Coordinate> smoothCoords = new List<Coordinate>();
            double dist;
            for (int i = 0; i < N; i++)
            {
                int next = (i + 1) % N;

                dist = coords[i].Distance(coords[next]);
                if (dist < Control.MinLength)
                {
                    // segment too short - just copy input coordinate
                    smoothCoords.Add(new Coordinate(coords[i]));
                }
                else
                {
                    int smoothN = Control.NumVertices;
                    Coordinate[] segment =
                            CubicBezier(
                                    coords[i],
                                    coords[next],
                                    controlPoints[i,1],
                                    controlPoints[next,0],
                                    smoothN);

                    int copyN = i < N - 1 ? segment.Length - 1 : segment.Length;
                    for (int k = 0; k < copyN; k++)
                    {
                        smoothCoords.Add(segment[k]);
                    }
                }
            }

            var shell = ThCADCoreNTSService.Instance.GeometryFactory.CreateLinearRing(smoothCoords.ToArray());
            return ThCADCoreNTSService.Instance.GeometryFactory.CreatePolygon(shell);
        }

        private Coordinate[,] GetLineControlPoints(Coordinate[] coords, double alpha)
        {
            if (alpha < 0.0 || alpha > 1.0)
            {
                throw new ArgumentException("alpha must be a value between 0 and 1 inclusive");
            }

            int N = coords.Length;
            Coordinate[,] ctrl = new Coordinate[N, 2];

            Coordinate[] v = new Coordinate[3];

            Coordinate[] mid = new Coordinate[2];
            mid[0] = new Coordinate();
            mid[1] = new Coordinate();

            Coordinate anchor = new Coordinate();
            double[] vdist = new double[2];
            // double mdist;

            // Start with dummy coordinate preceding first real coordinate
            v[1] = new Coordinate(2 * coords[0].X - coords[1].X, 2 * coords[0].Y - coords[1].Y);
            v[2] = coords[0];

            // Dummy coordinate for end of line
            Coordinate vN =
                    new Coordinate(
                2 * coords[N - 1].X - coords[N - 2].X,
                2 * coords[N - 1].Y - coords[N - 2].Y);

            mid[1].X = (v[1].X + v[2].X) / 2.0;
            mid[1].Y = (v[1].Y + v[2].Y) / 2.0;
            vdist[1] = v[1].Distance(v[2]);

            for (int i = 0; i < N; i++)
            {
                v[0] = v[1];
                v[1] = v[2];
                v[2] = (i < N - 1 ? coords[i + 1] : vN);

                mid[0].X = mid[1].X;
                mid[0].Y = mid[1].Y;
                mid[1].X = (v[1].X + v[2].X) / 2.0;
                mid[1].Y = (v[1].Y + v[2].Y) / 2.0;

                vdist[0] = vdist[1];
                vdist[1] = v[1].Distance(v[2]);

                double p = vdist[0] / (vdist[0] + vdist[1]);
                anchor.X = mid[0].X + p * (mid[1].X - mid[0].X);
                anchor.Y = mid[0].Y + p * (mid[1].Y - mid[0].Y);

                double xdelta = anchor.X - v[1].X;
                double ydelta = anchor.Y - v[1].Y;

                ctrl[i,0] =
                        new Coordinate(
                                alpha * (v[1].X - mid[0].X + xdelta) + mid[0].X - xdelta,
                                alpha * (v[1].Y - mid[0].Y + ydelta) + mid[0].Y - ydelta);

                ctrl[i,1] =
                        new Coordinate(
                                alpha * (v[1].X - mid[1].X + xdelta) + mid[1].X - xdelta,
                                alpha * (v[1].Y - mid[1].Y + ydelta) + mid[1].Y - ydelta);
            }

            return ctrl;
        }

        private Coordinate[,] GetPolygonControlPoints(Coordinate[] coords, int N, double alpha)
        {
            if (alpha < 0.0 || alpha > 1.0)
            {
                throw new ArgumentException("alpha must be a value between 0 and 1 inclusive");
            }

            Coordinate[,] ctrl = new Coordinate[N,2];

            Coordinate[] v = new Coordinate[3];

            Coordinate[] mid = new Coordinate[2];
            mid[0] = new Coordinate();
            mid[1] = new Coordinate();

            Coordinate anchor = new Coordinate();
            double[] vdist = new double[2];
            // double mdist;

            v[1] = coords[N - 1];
            v[2] = coords[0];
            mid[1].X = (v[1].X + v[2].X) / 2.0;
            mid[1].Y = (v[1].Y + v[2].Y) / 2.0;
            vdist[1] = v[1].Distance(v[2]);

            for (int i = 0; i < N; i++)
            {
                v[0] = v[1];
                v[1] = v[2];
                v[2] = coords[(i + 1) % N];

                mid[0].X = mid[1].X;
                mid[0].Y = mid[1].Y;
                mid[1].X = (v[1].X + v[2].X) / 2.0;
                mid[1].Y = (v[1].Y + v[2].Y) / 2.0;

                vdist[0] = vdist[1];
                vdist[1] = v[1].Distance(v[2]);

                double p = vdist[0] / (vdist[0] + vdist[1]);
                anchor.X = mid[0].X + p * (mid[1].X - mid[0].X);
                anchor.Y = mid[0].Y + p * (mid[1].Y - mid[0].Y);

                double xdelta = anchor.X - v[1].X;
                double ydelta = anchor.Y - v[1].Y;

                ctrl[i,0] =
                        new Coordinate(
                                alpha * (v[1].X - mid[0].X + xdelta) + mid[0].X - xdelta,
                                alpha * (v[1].Y - mid[0].Y + ydelta) + mid[0].Y - ydelta);

                ctrl[i,1] =
                        new Coordinate(
                                alpha * (v[1].X - mid[1].X + xdelta) + mid[1].X - xdelta,
                                alpha * (v[1].Y - mid[1].Y + ydelta) + mid[1].Y - ydelta);
            }

            return ctrl;
        }

        private Coordinate[] CubicBezier(
            Coordinate start,
            Coordinate end,
            Coordinate ctrl1,
            Coordinate ctrl2,
            int nv)
        {

            Coordinate[] curve = new Coordinate[nv];

            Coordinate[] buf = new Coordinate[3];
            for (int i = 0; i < buf.Length; i++)
            {
                buf[i] = new Coordinate();
            }

            curve[0] = new Coordinate(start);
            curve[nv - 1] = new Coordinate(end);
            InterpPoint[] ip = GetInterpPoints(nv);

            for (int i = 1; i < nv - 1; i++)
            {
                Coordinate c = new Coordinate
                {
                    X =
                        ip[i].t[0] * start.X
                                + ip[i].t[1] * ctrl1.X
                                + ip[i].t[2] * ctrl2.X
                                + ip[i].t[3] * end.X
                };
                c.X /= ip[i].tsum;
                c.Y =
                        ip[i].t[0] * start.Y
                                + ip[i].t[1] * ctrl1.Y
                                + ip[i].t[2] * ctrl2.Y
                                + ip[i].t[3] * end.Y;
                c.Y /= ip[i].tsum;

                curve[i] = c;
            }

            return curve;
        }

        private InterpPoint[] GetInterpPoints(int npoints)
        {
            InterpPoint[] ip = null;
            InterpPoint[] refer;
            lookup.TryGetValue(npoints, out refer);
            if (refer != null) ip = refer;

            if (ip == null)
            {
                ip = new InterpPoint[npoints];

                for (int i = 0; i < npoints; i++)
                {
                    double t = (double)i / (npoints - 1);
                    double tc = 1.0 - t;

                    ip[i] = new InterpPoint();
                    ip[i].t[0] = tc * tc * tc;
                    ip[i].t[1] = 3.0 * tc * tc * t;
                    ip[i].t[2] = 3.0 * tc * t * t;
                    ip[i].t[3] = t * t * t;
                    ip[i].tsum = ip[i].t[0] + ip[i].t[1] + ip[i].t[2] + ip[i].t[3];
                }

                lookup.Add(npoints, ip);
            }

            return ip;
        }
    }
}
