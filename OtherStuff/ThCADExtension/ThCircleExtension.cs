using DotNetARX;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADExtension
{
    public static class ThCircleExtension
    {
        public static Polyline ToPolyCircle(this Circle circle)
        {
            var poly = new Polyline();
            poly.CreatePolyCircle(circle.Center.ToPoint2D(), circle.Radius);
            return poly;
        }
    }
}
