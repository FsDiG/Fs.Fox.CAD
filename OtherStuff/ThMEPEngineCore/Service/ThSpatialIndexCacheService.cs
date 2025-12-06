using System;
using System.Linq;
using ThCADCore.NTS;
using ThMEPEngineCore.Model;
using ThMEPEngineCore.Engine;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using NFox.Cad;

namespace ThMEPEngineCore.Service
{
    public class ThSpatialIndexCacheService
    {       
        private static readonly ThSpatialIndexCacheService instance = new ThSpatialIndexCacheService() { };
        static ThSpatialIndexCacheService() { }
        internal ThSpatialIndexCacheService() 
        {
            Factories = new List<SpatialIndexFactory>();
        }
        public static ThSpatialIndexCacheService Instance { get { return instance; } }
        public List<SpatialIndexFactory> Factories { get; set; }
        public void Build(Database database, Point3dCollection polygon)
        {
            foreach(var factory in Factories)
            {
                factory.Create(database, polygon);
            }
        }
        public void Add(List<BuiltInCategory> builtInCategories)
        {
            builtInCategories.ForEach(o => Add(o));
        }
        public void Add(BuiltInCategory builtInCategory)
        {
            if(Factories.Where(o => o.BuiltCategory == builtInCategory).Count()==0)
            {
                switch(builtInCategory)
                {
                    case BuiltInCategory.OST_ArchitectureWall:
                        Factories.Add(new ArchitectureWallSpatialIndexFactory());
                        break;
                    case BuiltInCategory.OST_Column:
                        Factories.Add(new ColumnSpatialIndexFactory());
                        break;
                    case BuiltInCategory.OST_CurtainWall:
                        Factories.Add(new CurtainWallSpatialIndexFactory());
                        break;
                    case BuiltInCategory.OST_ShearWall:
                        Factories.Add(new ShearWallSpatialIndexFactory());
                        break;
                    case BuiltInCategory.OST_Window:
                        Factories.Add(new WindowSpatialIndexFactory());
                        break;
                    default:
                        break;
                }                
            }
        }
        public void Remove(List<BuiltInCategory> builtInCategories)
        {
            builtInCategories.ForEach(o => Remove(o));
        }
        public void Remove(BuiltInCategory builtInCategory)
        {
            Factories = Factories.Where(o => o.BuiltCategory != builtInCategory).ToList();
        }
        public List<Entity> Find(BuiltInCategory builtInCategory, Polyline envelope)
        {
            var results = Factories.Where(o => o.BuiltCategory == builtInCategory);
            if (results.Count() == 1)
            {
                return results.First().Query(envelope);
            }
            else
            {
                return new List<Entity>();
            }
        }
    }
    #region ---------------创建索引工厂----------------
    public abstract class SpatialIndexFactory
    {
        public BuiltInCategory BuiltCategory { get; protected set; } = BuiltInCategory.UnNone;
        protected ThCADCoreNTSSpatialIndex SpatialIndex { get; set; }
        public abstract void Create(Database database, Point3dCollection polygon);
        public virtual List<Entity> Query(Polyline envelope)
        {
            if (SpatialIndex != null)
            {
                return SpatialIndex
                .SelectCrossingPolygon(envelope)
                .Cast<Entity>().ToList();
            }
            else
            {
                return new List<Entity>();
            }
        }
    }
    public class ColumnSpatialIndexFactory : SpatialIndexFactory
    {
        public ColumnSpatialIndexFactory()
        {
            BuiltCategory = BuiltInCategory.OST_Column;
        }
        public override void Create(Database database, Point3dCollection polygon)
        {
            using (var engine = new ThColumnRecognitionEngine())
            {
                // 识别结构柱
                engine.Recognize(database, polygon);
                SpatialIndex = new ThCADCoreNTSSpatialIndex(
                    engine.Elements.Select(o => o.Outline).ToCollection());
            }
        }
    }
    public class ArchitectureWallSpatialIndexFactory : SpatialIndexFactory
    {
        public ArchitectureWallSpatialIndexFactory()
        {
            BuiltCategory = BuiltInCategory.OST_ArchitectureWall;
        }
        public override void Create(Database database, Point3dCollection polygon)
        {
            using (var engine = new ThArchitectureWallRecognitionEngine())
            {
                // 识别结构柱
                engine.Recognize(database, polygon);
                SpatialIndex = new ThCADCoreNTSSpatialIndex(
                    engine.Elements.Select(o => o.Outline).ToCollection());
            }
        }
    }
    public class ShearWallSpatialIndexFactory : SpatialIndexFactory
    {
        public ShearWallSpatialIndexFactory()
        {
            BuiltCategory = BuiltInCategory.OST_ShearWall;
        }
        public override void Create(Database database, Point3dCollection polygon)
        {
            using (var engine = new ThShearWallRecognitionEngine())
            {
                // 识别结构柱
                engine.Recognize(database, polygon);
                SpatialIndex = new ThCADCoreNTSSpatialIndex(
                    engine.Elements.Select(o => o.Outline).ToCollection());
            }
        }
    }
    public class CurtainWallSpatialIndexFactory : SpatialIndexFactory
    {
        public CurtainWallSpatialIndexFactory()
        {
            BuiltCategory = BuiltInCategory.OST_CurtainWall;
        }
        public override void Create(Database database, Point3dCollection polygon)
        {
            using (var engine = new ThCurtainWallRecognitionEngine())
            {
                // 识别结构柱
                engine.Recognize(database, polygon);
                SpatialIndex = new ThCADCoreNTSSpatialIndex(
                    engine.Elements.Select(o => o.Outline).ToCollection());
            }
        }
    }
    public class WindowSpatialIndexFactory : SpatialIndexFactory
    {
        public WindowSpatialIndexFactory()
        {
            BuiltCategory = BuiltInCategory.OST_Window;
        }
        public override void Create(Database database, Point3dCollection polygon)
        {
            using (var engine = new ThWindowRecognitionEngine())
            {
                // 识别结构柱
                engine.Recognize(database, polygon);
                SpatialIndex = new ThCADCoreNTSSpatialIndex(
                    engine.Elements.Select(o => o.Outline).ToCollection());
            }
        }
    }
    #endregion
}
