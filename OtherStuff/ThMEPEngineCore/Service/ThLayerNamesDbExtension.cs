using System;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service
{
    public  class ThLayerNamesDbExtension : ThDbExtension, IDisposable
    {   
        
        public ThLayerNamesDbExtension(Database db) : base(db)
        {
            LayerFilter = ThLayerNamesLayerManager.XrefLayers(db);      
        }
        public void Dispose()
        {
        }
        public override void BuildElementTexts()
        {
            throw new NotImplementedException();
        }
        public override void BuildElementCurves()
        {
            throw new NotImplementedException();
        }
    }
}
