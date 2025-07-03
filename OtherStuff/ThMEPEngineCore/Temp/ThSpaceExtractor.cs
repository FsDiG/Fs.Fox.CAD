using System;
using System.Linq;
using System.Collections.Generic;
using DotNetARX;
using Linq2Acad;
using ThMEPEngineCore.Model;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Dreambuild.AutoCAD;

namespace ThMEPEngineCore.Temp
{
    public class ThSpaceExtractor :ThExtractorBase,IExtract,IPrint,IBuildGeometry
    {
        public List<ThIfcSpace> Spaces { get; private set; }
        public Dictionary<Polyline, List<Polyline>> Obstacles { get; set; }
        /// <summary>
        /// 障碍物颜色
        /// </summary>
        public short ObstacleColorIndex { get; set; }
        /// <summary>
        /// 是否要创建阻碍物
        /// </summary>
        public bool IsBuildObstacle { get; set; }
        /// <summary>
        /// 空间轮廓图层名
        /// </summary>
        public string SpaceLayer { get; set; }
        /// <summary>
        /// 空间标识名称
        /// </summary>
        public string NameLayer { get; set; }
        /// <summary>
        /// 障碍物种类
        /// </summary>
        public string ObstacleCategory { get; set; }
        public ThSpaceExtractor()
        {
            Spaces = new List<ThIfcSpace>();
            Obstacles = new Dictionary<Polyline, List<Polyline>>();
            IsBuildObstacle = false;
            Category = "Space";
            SpaceLayer = "AD-AREA-OUTL";
            NameLayer = "AD-NAME-ROOM";
            ObstacleColorIndex = 211;
            ObstacleCategory = "Obstacle";
        }

        public void Extract(Database database, Point3dCollection pts)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            using (var spaceEngine = new ThExtractSpaceRecognitionEngine())
            {
                spaceEngine.SpaceLayer = SpaceLayer;
                spaceEngine.NameLayer = NameLayer;
                //spaceEngine.NameLayer = "空间名称";

                spaceEngine.Recognize(database, pts);
                Spaces = spaceEngine.Spaces;

                if(IsBuildObstacle)
                {
                    BuildObstacles();
                }
            }
        }

        private void BuildObstacles()
        {
            //在停车区域空间
            var spaces = Spaces
                 .Where(o => o.Tags.Where(g => g.Contains("停车区域")).Any())
                 .Select(o => o.Boundary).ToList();
            spaces.ForEach(o => Obstacles.Add(o.Clone() as Polyline, ThArrangeObstacleService.Arrange(o as Polyline)));
        }

        public List<ThGeometry> BuildGeometries()
        {
            var geos = new List<ThGeometry>();
            geos.AddRange(BuildSpaceGeometries());
            geos.AddRange(BuildObstacleGeometries());
            return geos;
        }

        private List<ThGeometry> BuildSpaceGeometries()
        {
            var geos = new List<ThGeometry>();
            Spaces.ForEach(o =>
            {
                var geometry = new ThGeometry();
                geometry.Properties.Add("Category", Category);
                geometry.Properties.Add("Name", string.Join(";", o.Tags.ToArray()));
                for (int i = 1; i <= o.SubSpaces.Count; i++)
                {
                    string key = "SubSpace" + i + " ID=";
                    geometry.Properties.Add(key, o.SubSpaces[i - 1].Uuid);
                }
                geometry.Boundary = o.Boundary;
                geos.Add(geometry);
            });
            return geos;
        }

        private List<ThGeometry> BuildObstacleGeometries()
        {
            var geos = new List<ThGeometry>();
            Obstacles.ForEach(o =>
            {
                o.Value.ForEach(v =>
                {
                    var geometry = new ThGeometry();
                    geometry.Properties.Add("Category", ObstacleCategory);
                    geometry.Boundary = v;
                    geos.Add(geometry);
                });                
            });
            return geos;
        }

        public void Print(Database database)
        {
            PrintSpace(database);
            PrintObstacles(database);
        }

        private void PrintSpace(Database database)
        {
            using (var db = AcadDatabase.Use(database))
            {
                var spaceIds = new ObjectIdList();
                Spaces.ForEach(o =>
                {
                    o.Boundary.ColorIndex = ColorIndex;
                    o.Boundary.SetDatabaseDefaults();
                    spaceIds.Add(db.ModelSpace.Add(o.Boundary));
                });
                if (spaceIds.Count > 0)
                {
                    GroupTools.CreateGroup(db.Database, Guid.NewGuid().ToString(), spaceIds);
                }
            }
        }

        private void PrintObstacles(Database database)
        {
            using (var db = AcadDatabase.Use(database))
            {
                Obstacles.ForEach(o =>
                {
                    var obstructIds = new ObjectIdList();
                    o.Key.ColorIndex = ColorIndex;
                    o.Key.SetDatabaseDefaults();
                    obstructIds.Add(db.ModelSpace.Add(o.Key));
                    o.Value.ForEach(v =>
                    {
                        v.ColorIndex = ObstacleColorIndex;
                        v.SetDatabaseDefaults();
                        obstructIds.Add(db.ModelSpace.Add(v));
                    });
                    if (obstructIds.Count > 0)
                    {
                        GroupTools.CreateGroup(db.Database, Guid.NewGuid().ToString(), obstructIds);
                    }
                });

            }
        }
    }
}
