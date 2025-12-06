using NFox.Cad;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADExtension
{
    public static class ThSelectionFilterTool
    {
        public static SelectionFilter Build(string[] dxfNames)
        {
            return OpFilter.Bulid(o =>
                o.Dxf((int)DxfCode.Start) == string.Join(",", dxfNames));
        }

        public static SelectionFilter Build(string[] dxfNames, string[] layerNames)
        {
            return OpFilter.Bulid(o =>
            o.Dxf((int)DxfCode.Start) == string.Join(",", dxfNames) &
            o.Dxf((int)DxfCode.LayerName) == string.Join(",", layerNames));
        }
    }
}
