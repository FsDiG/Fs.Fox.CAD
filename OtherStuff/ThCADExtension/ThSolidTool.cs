using System;
using GeometryExtensions;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADExtension
{
    public static class ThSolidTool
    {
        public static Polyline ToPolyline(this Solid solid)
        {
            //
            // We'll collect our points from the Solid
            var pts = new Point3dCollection();


            // Use a flipped indexing scheme: the 3rd & 4th vertices
            // need to be flipped to go clockwise/anti-clockwise
            foreach (var s in new short[] { 0, 1, 3, 2 })
            {
                var pt = solid.GetPointAt(s);

                // If we have only 3 points then the 4th is a repeat
                // of the 3rd (which in our case means the point in
                // pts with an index of 1, as we flipped the indices)
                if (s != 2 || pt.DistanceTo(pts[2]) > Tolerance.Global.EqualPoint)
                { 
                    pts.Add(pt);

                }
            }

            // We need a plane to define our polyline's 2D points
            var plane = new Plane(pts[0], solid.Normal);

            // Create the empty polyline in the plane of the solid
            var pl = new Polyline(pts.Count);
            pl.Normal = solid.Normal;

            // Fill it with 2D points
            for (int i = 0; i < pts.Count; i++)
            {
                // Add our converted 2D point to the vertex list
                var pt2 = pts[i].Convert2d(plane);
                pl.AddVertexAt(i, pt2, 0, 0, 0);
            }

            // Close the polyline
            pl.Closed = true;

            // Move it so that it overlaps the Solid
            pl.TransformBy(Matrix3d.Displacement(pts[0].GetAsVector()));

            return pl;
        }

        public static Solid ToSolid(this Polyline polyline)
        {            
            if(polyline.NumberOfVertices == 3)
            {
                return new Solid(
                    polyline.GetPoint3dAt(0),
                    polyline.GetPoint3dAt(1),
                    polyline.GetPoint3dAt(2));
            }
            else if(polyline.NumberOfVertices == 4)
            {
                return new Solid(
                    polyline.GetPoint3dAt(0),
                    polyline.GetPoint3dAt(1),
                    polyline.GetPoint3dAt(3),
                    polyline.GetPoint3dAt(2));
            }
            else if (polyline.NumberOfVertices == 5)
            {
                return new Solid(
                    polyline.GetPoint3dAt(0),
                    polyline.GetPoint3dAt(1),
                    polyline.GetPoint3dAt(3),
                    polyline.GetPoint3dAt(2));
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static Solid WashClone(this Solid solid)
        {
            return solid.ToPolyline().ToSolid();
        }
    }
}
