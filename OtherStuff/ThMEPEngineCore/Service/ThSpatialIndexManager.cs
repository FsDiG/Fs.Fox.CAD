using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThCADCore.NTS;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service
{
    public class ThSpatialIndexManager : IDisposable
    {
        public ThSpatialIndexManager()
        {
        }
        public void Dispose()
        {
            if (BeamSpatialIndex != null)
            {
                BeamSpatialIndex.Dispose();
            }
            if (ColumnSpatialIndex != null)
            {
                ColumnSpatialIndex.Dispose();
            }
            if (WallSpatialIndex != null)
            {
                WallSpatialIndex.Dispose();
            }
        }
        public ThCADCoreNTSSpatialIndex WallSpatialIndex { get; private set; }
        public ThCADCoreNTSSpatialIndex ColumnSpatialIndex { get; private set; }
        public ThCADCoreNTSSpatialIndex BeamSpatialIndex { get; private set; }

        public void CreateWallSpatialIndex(DBObjectCollection wallObjs)
        {
            WallSpatialIndex = ThSpatialIndexService.CreatWallSpatialIndex(wallObjs);
        }
        public void CreateBeamSpatialIndex(DBObjectCollection beamObjs)
        {
            BeamSpatialIndex = ThSpatialIndexService.CreateBeamSpatialIndex(beamObjs);
        }
        public void CreateColumnSpatialIndex(DBObjectCollection beamObjs)
        {
            ColumnSpatialIndex = ThSpatialIndexService.CreateBeamSpatialIndex(beamObjs);
        }
    }
}
