using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service.Hvac
{
    public static class ThHvacDbModelFoundationExtension
    {
        public static bool IsModelFoundation(this Entity entity)
        {
            return entity.GetXDataForApplication(ThHvacCommon.RegAppName_Model_Foundation) != null;
        }

        public static bool IsModelFoundationLayer(this Entity entity)
        {
            return entity.Layer == ThHvacCommon.FOUNDATION_LAYER;
        }
    }
}
