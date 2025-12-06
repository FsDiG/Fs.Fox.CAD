using System;
using Linq2Acad;
using ThMEPEngineCore.Service;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service
{
    public class ThLaneLineDbExtension : ThOtherDbExtension,IDisposable
    {
        public List<Curve> LaneCurves { get; set; }
        public ThLaneLineDbExtension(Database db):base(db)
        {
            LayerFilter = ThLaneLineLayerManager.GeometryXrefLayers(db);
            LaneCurves = new List<Curve>();
        }
        public void Dispose()
        {
            foreach (var curve in LaneCurves)
            {
                curve.Dispose();
            }
            LaneCurves.Clear();
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
                        LaneCurves.AddRange(BuildElementCurves(blkRef, mcs2wcs));
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
