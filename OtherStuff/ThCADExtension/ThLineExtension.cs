using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADExtension
{
    public static class ThLineExtension
    {
        public static Vector3d LineDirection(this Line line)
        {
            return line.StartPoint.GetVectorTo(line.EndPoint).GetNormal();
        }

        public static Line ExtendLine(this Line line, double distance)
        {
            var direction = line.LineDirection();
            return new Line(line.StartPoint - direction * distance, line.EndPoint + direction * distance);
        }
    }
}
