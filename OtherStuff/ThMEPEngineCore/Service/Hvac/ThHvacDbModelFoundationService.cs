using Linq2Acad;
using DotNetARX;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Dreambuild.AutoCAD;

namespace ThMEPEngineCore.Service.Hvac
{
    public class ThHvacDbModelFoundationService
    {
        public static void CleanAll(Database database)
        {
            using (var acadDatabase = AcadDatabase.Use(database))
            using (var manager = new ThHvacDbModelFoundationManager(database))
            {
                manager.Geometries.Cast<ObjectId>().ForEach(o =>
                {
                    acadDatabase.Element<Entity>(o, true).Erase();
                });
            }
        }

        public static void Generate(Database database)
        {
            using (var acadDatabase = AcadDatabase.Use(database))
            using (var manager = new ThHvacDbModelManager(database))
            {
                manager.Geometries.Cast<ObjectId>().ForEach(o =>
                {
                    var objs = new DBObjectCollection();
                    var foundation = new ObjectIdCollection();
                    var model = acadDatabase.Element<BlockReference>(o);
                    model.Explode(objs);
                    objs.Cast<Entity>()
                    .Where(e => e.IsModelFoundationLayer())
                    .ForEach(f =>
                    {
                        foundation.Add(acadDatabase.ModelSpace.Add(f));
                    });
                    foundation.Cast<ObjectId>().ForEach(f =>
                    {
                        TypedValueList valueList = new TypedValueList
                        {
                            { (int)DxfCode.ExtendedDataAsciiString, model.GetModelIdentifier() },
                        };
                        f.AddXData(ThHvacCommon.RegAppName_Model_Foundation, valueList);
                    });
                });
            }
        }
    }
}
