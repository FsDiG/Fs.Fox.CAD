using System;
using Linq2Acad;
using System.Linq;
using ThMEPEngineCore.Algorithm;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;


namespace ThMEPEngineCore.Service
{
  public  class ThWellsDbExtension : ThDbExtension, IDisposable
    {
        public List<Entity> Wells { get; set; }
        public ThWellsDbExtension(Database db) : base(db)
        {
            LayerFilter = ThWellsLayerManager.XrefLayers(db);
            Wells = new List<Entity>();
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
                        Wells.AddRange(BuildElementCurves(blkRef, mcs2wcs));
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
                                    if (CheckLayerValid(blockObj))
                                    {
                                        var minPt = blockObj.GeometricExtents.MinPoint;
                                        var maxPt = blockObj.GeometricExtents.MaxPoint;
                                        Polyline polyline = new Polyline()
                                        {
                                            Closed = true
                                        };
                                        polyline.AddVertexAt(0, new Point2d(minPt.X, minPt.Y), 0.0, 0.0, 0.0);
                                        polyline.AddVertexAt(1, new Point2d(maxPt.X, minPt.Y), 0.0, 0.0, 0.0);
                                        polyline.AddVertexAt(2, new Point2d(maxPt.X, maxPt.Y), 0.0, 0.0, 0.0);
                                        polyline.AddVertexAt(3, new Point2d(minPt.X, maxPt.Y), 0.0, 0.0, 0.0);
                                        polyline.TransformBy(matrix);
                                        ents.Add(polyline);
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
                            return ents.Where(o => xclip.Contains(o as Polyline));
                        }
                    }
                }
                return ents;
            }
        }

        public override void BuildElementTexts()
        {
            throw new NotImplementedException();
        }
    }
}
