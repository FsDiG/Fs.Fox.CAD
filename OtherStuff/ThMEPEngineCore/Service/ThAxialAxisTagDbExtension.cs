using System;
using Linq2Acad;
using System.Linq;
using ThMEPEngineCore.Algorithm;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service
{
  public  class ThAxialAxisTagDbExtension : ThDbExtension, IDisposable
    {
        public List<Line> Lines { get; set; }
        public List<Curve> Polylines { get; set; }
        public ThAxialAxisTagDbExtension(Database db) : base(db)
        {
            LayerFilter = ThAxialAxisTagLayerManager.XrefLayers(db);           
            Polylines = new List<Curve>();
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
                        Polylines.AddRange(BuildElementCurves(blkRef, mcs2wcs));
                    }
                }
            }
        }
        private IEnumerable<Curve> BuildElementCurves(BlockReference blockReference, Matrix3d matrix)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(HostDb))
            {
                var circles = new List<Curve>();
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
                                    circles.AddRange(BuildElementCurves(blockObj, mcs2wcs));
                                }
                            }
                            else if ((dbObj is Line circle))
                            {
                                if (IsBuildElement(circle) &&
                                    CheckLayerValid(circle))
                                {
                                    circles.Add(circle.GetTransformedCopy(matrix) as Line);
                                }
                            }
                            else if (dbObj is Polyline circle1)
                            {
                                if (IsBuildElement(circle1) &&
                                    CheckLayerValid(circle1))
                                {
                                    circles.Add(circle1.GetTransformedCopy(matrix) as Polyline);
                                }
                            }
                        }
                        var xclip = blockReference.XClipInfo();
                        if (xclip.IsValid)
                        {
                            xclip.TransformBy(matrix);
                            return circles.Where(o => xclip.Contains(o as Curve));
                        }
                    }
                }
                return circles;
            }
        }

        public override void BuildElementTexts()
        {
            throw new NotImplementedException();
        }
    }
}
