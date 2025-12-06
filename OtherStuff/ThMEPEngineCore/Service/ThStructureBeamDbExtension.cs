using System;
using Linq2Acad;
using System.Linq;
using ThCADCore.NTS;
using ThMEPEngineCore.CAD;
using ThMEPEngineCore.Algorithm;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Dreambuild.AutoCAD;

namespace ThMEPEngineCore.Service
{
    public class ThStructureBeamDbExtension : ThStructureDbExtension,IDisposable
    {
        public List<Curve> BeamCurves { get; set; }
        public ThStructureBeamDbExtension(Database db):base(db)
        {
            LayerFilter = ThBeamLayerManager.GeometryXrefLayers(db);
            BeamCurves = new List<Curve>();
        }
        public void Dispose()
        {
        }
        public override void BuildElementCurves()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(HostDb))
            {
                foreach(var ent in acadDatabase.ModelSpace)
                {
                    if (ent is BlockReference blkRef)
                    {
                        if (blkRef.BlockTableRecord.IsNull)
                        {
                            continue;
                        }
                        BlockTableRecord btr = acadDatabase.Element<BlockTableRecord>(blkRef.BlockTableRecord);
                        var mcs2wcs = blkRef.BlockTransform.PreMultiplyBy(Matrix3d.Identity);
                        BeamCurves.AddRange(BuildElementCurves(blkRef, mcs2wcs));
                    }
                }                
            }
        }
        private IEnumerable<Curve> BuildElementCurves(BlockReference blockReference, Matrix3d matrix)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(HostDb))
            {
                List<Curve> curves = new List<Curve>();
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
                                    curves.AddRange(BuildElementCurves(blockObj, mcs2wcs));
                                }
                            }
                            else if (dbObj is Curve curve)
                            {
                                if (CheckLayerValid(curve) && CheckCurveValid(curve))
                                {
                                    curves.Add(curve.GetTransformedCopy(matrix) as Curve);
                                }
                            }
                            else if(dbObj is Mline mline)
                            {
                                if (CheckLayerValid(mline))
                                {
                                    var mlineCopy = mline.GetTransformedCopy(matrix) as Mline;
                                    DBObjectCollection mlineCurves = new DBObjectCollection();
                                    mlineCopy.Explode(mlineCurves);
                                    mlineCurves.Cast<Curve>().ForEach(o=>curves.Add(o));
                                }
                            }
                        }
                        var xclip = blockReference.XClipInfo();
                        if (xclip.IsValid)
                        {
                            xclip.TransformBy(matrix);
                            return curves.Where(o => xclip.Contains(o));
                        }
                    }
                }
                return curves;
            }
        }
        public override void BuildElementTexts()
        {
            throw new NotImplementedException();
        }
    }
}
