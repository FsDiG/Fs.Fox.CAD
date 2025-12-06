using System;
using Linq2Acad;
using System.Linq;
using System.Text;
using ThMEPEngineCore.CAD;
using System.Threading.Tasks;
using ThMEPEngineCore.Algorithm;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service
{
    public class ThSideEntryWaterBucketDbExtension : ThDbExtension, IDisposable
    {
        public void Dispose()
        {
        }
        public List<Entity> SideEntryWaterBuckets { get; set; }
        public ThSideEntryWaterBucketDbExtension(Database db) : base(db)
        {
            LayerFilter = ThSideEntryWaterBucketLayerManager.XrefLayers(db);
            SideEntryWaterBuckets = new List<Entity>();
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
                        SideEntryWaterBuckets.AddRange(BuildElementCurves(blkRef, mcs2wcs));
                    }
                }
            }
        }
        private IEnumerable<Entity> BuildElementCurves(BlockReference blockReference, Matrix3d matrix)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(HostDb))
            {
                List<Entity> ents = new List<Entity>();
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
                                    string blkName = "";
                                    if(blockObj.IsDynamicBlock)
                                    {
                                        var obj = acadDatabase.Element<BlockTableRecord>(blockObj.DynamicBlockTableRecord);
                                        blkName = obj.Name;
                                    }
                                    else
                                    {
                                        blkName = blockObj.Name;
                                    }
                                    if (CheckLayerValid(blockObj) &&
                                        ThSideEntryWaterBucketLayerManager.IsSideEntryWaterBucketBlockName(blkName))
                                    {
                                        ents.Add(blockObj.GetTransformedCopy(matrix));
                                    }
                                    var mcs2wcs = blockObj.BlockTransform.PreMultiplyBy(matrix);
                                    ents.AddRange(BuildElementCurves(blockObj, mcs2wcs));
                                }
                            }
                        }
                        var xclip = blockReference.XClipInfo();
                        if (xclip.IsValid)
                        {
                            xclip.TransformBy(matrix);
                            return ents.Where(o => IsContain(xclip, o));
                        }
                    }
                }
                return ents;
            }
        }
        private bool IsContain(ThMEPXClipInfo xclip, Entity ent)
        {
            if (ent is Curve curve)
            {
                return xclip.Contains(curve);
            }
            else if (ent is BlockReference br)
            {
                var minPt = br.GeometricExtents.MinPoint;
                var maxPt = br.GeometricExtents.MaxPoint;
                var second = new Point3d(maxPt.X, minPt.Y, minPt.Z);
                var fourth = new Point3d(minPt.X, maxPt.Y, minPt.Z);
                var pts = new Point3dCollection { minPt, second, maxPt, fourth };
                var outline = pts.CreatePolyline();
                return xclip.Contains(outline);
            }
            else
            {
                return false;
            }
        }
        public override void BuildElementTexts()
        {
            throw new NotImplementedException();
        }
    }
}

