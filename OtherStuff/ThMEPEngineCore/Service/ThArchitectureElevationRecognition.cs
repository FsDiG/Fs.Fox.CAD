using System;
using System.Collections.Generic;
using System.Linq;
using Linq2Acad;
using ThMEPEngineCore.Service;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThMEPEngineCore.Algorithm;

namespace ThMEPEngineCore.Engine
{
    public class ThArchitectureElevationRecognition : ThDbExtension, IDisposable
    {
        public List<DBText> Texts { get; set; }
        public ThArchitectureElevationRecognition(Database db) : base(db)
        {
            Texts = new List<DBText>();
            LayerFilter = ThArchitectureElevationLayerManager.TextXrefLayers(db);
        }
        public override void BuildElementTexts()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(HostDb))//检索主范围
            {
                foreach (var ent in acadDatabase.ModelSpace)
                {
                    if (ent is BlockReference blkRef)
                    {
                        if (blkRef.BlockTableRecord.IsNull)
                        {
                            continue;
                        }
                        BlockTableRecord btr = acadDatabase.Element<BlockTableRecord>(blkRef.BlockTableRecord);
                        var mcs2wcs = blkRef.BlockTransform.PreMultiplyBy(Matrix3d.Identity);
                        Texts.AddRange(BuildElementTexts(blkRef, mcs2wcs));
                    }
                }
            }
        }
        private IEnumerable<DBText> BuildElementTexts(BlockReference blockReference, Matrix3d matrix)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(HostDb))
            {
                List<DBText> dbTexts = new List<DBText>();
                if (IsBuildElementBlockReference(blockReference))
                {
                    var blockTableRecord = acadDatabase.Blocks.Element(blockReference.BlockTableRecord);
                    if (IsBuildElementBlock(blockTableRecord))
                    {
                        foreach (var objId in blockTableRecord)
                        {                         
                               var dbObj = acadDatabase.Element<Entity>(objId);
                            if (dbObj is BlockReference blockObj)
                            {
                                if (blockObj.BlockTableRecord.IsNull)
                                {
                                    continue;
                                }
                                if (IsBuildElementBlockReference(blockObj))
                                {
                                    var mcs2wcs = blockObj.BlockTransform.PreMultiplyBy(matrix);
                                    dbTexts.AddRange(BuildElementTexts(blockObj, mcs2wcs));
                                }
                            }
                            else if (dbObj is DBText dBText)
                            {
                                if (CheckLayerValid(dBText))
                                {
                                    var newText = dBText.GetTransformedCopy(matrix) as DBText;
                                    dbTexts.Add(newText);
                                }
                            }
                        }

                        var xclip = blockReference.XClipInfo();
                        if (xclip.IsValid)
                        {
                            xclip.TransformBy(matrix);
                            return dbTexts.Where(o => xclip.Contains(o.Position));
                        }
                    }
                }
                return dbTexts;
            }
        }
        public override void BuildElementCurves()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
