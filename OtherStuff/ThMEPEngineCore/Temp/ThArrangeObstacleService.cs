using System;
using System.Linq;
using System.Collections.Generic;
using ThCADCore.NTS;
using ThCADExtension;
using ThMEPEngineCore.CAD;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Temp
{
    public class ThArrangeObstacleService
    {
        private double Length { get; set; }
        private double Width { get; set; }
        private List<Polyline> Obstacles { get; set; }
        private Polyline RegionPoly { get; set; }
        private ThArrangeObstacleService(Polyline regionPoly)
        {
            RegionPoly = regionPoly;
            Length = 5000;
            Width = 5000;
            Obstacles = new List<Polyline>();
        }
        public static List<Polyline> Arrange(Polyline region)
        {
            var instance = new ThArrangeObstacleService(region);
            instance.Arrange();
            return instance.Obstacles;
        }
        private void Arrange()
        {
            var pts = RegionPoly.Vertices().Cast<Point3d>().ToList();
            double minX = pts.OrderBy(o => o.X).First().X;
            double minY = pts.OrderBy(o => o.Y).First().Y;

            double maxX = pts.OrderByDescending(o => o.X).First().X;
            double maxY = pts.OrderByDescending(o => o.Y).First().Y;

            Random random = new Random();
            while (Obstacles.Sum(o => o.Area) <= RegionPoly.Area * 0.15)
            {
                double x = NextDouble(random, minX, maxX);
                double y = NextDouble(random, minY, maxY);
                var center = new Point3d(x,y,0);
                if(RegionPoly.Contains(center))
                {
                   var rec = center.CreateRectangle(Length, Width);
                    if(RegionPoly.Contains(rec))
                    {
                        Obstacles.Add(rec);
                    }
                }
            }
        }

        private double NextDouble(Random random, double miniDouble, double maxiDouble)
        {
            if (random != null)
            {
                return random.NextDouble() * (maxiDouble - miniDouble) + miniDouble;
            }
            else
            {
                return 0.0d;
            }
        }
    }
}
