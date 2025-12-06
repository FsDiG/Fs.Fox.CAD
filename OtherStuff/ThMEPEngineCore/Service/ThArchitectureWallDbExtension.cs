using System;
using Linq2Acad;
using System.Linq;
using ThCADCore.NTS;
using ThCADExtension;
using ThMEPEngineCore.CAD;
using ThMEPEngineCore.Algorithm;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using NetTopologySuite.Geometries;
using Dreambuild.AutoCAD;

namespace ThMEPEngineCore.Service
{
    public class ThArchitectureWallDbExtension : ThStructureDbExtension, IDisposable
    {
        public List<Curve> WallCurves { get; set; }
        public ThArchitectureWallDbExtension(Database db) : base(db)
        {
            LayerFilter = ThArchitectureWallLayerManager.CurveXrefLayers(db);
            WallCurves = new List<Curve>();
        }
        public void Dispose()
        {
        }

        public override void BuildElementCurves()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(HostDb))
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
                        WallCurves.AddRange(BuildElementCurves(blkRef, mcs2wcs));
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
                            else if (dbObj is Polyline polyline)
                            {
                                if (IsBuildElement(polyline) && 
                                    CheckLayerValid(polyline) &&
                                    IsArchitectureWall(polyline))
                                {
                                    curves.Add(polyline.GetTransformedCopy(matrix) as Polyline);
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
        protected override bool IsBuildElement(Entity entity)
        {
            return entity.Hyperlinks.Count > 0;
        }
        private bool IsArchitectureWall(Curve curve)
        {
            if (curve.Hyperlinks.Count > 0)
            {
                var thPropertySet = ThPropertySet.CreateWithHyperlink(curve.Hyperlinks[0].Description);
                return thPropertySet.IsArchWall;
            }
            return false;
        }
    }
}
