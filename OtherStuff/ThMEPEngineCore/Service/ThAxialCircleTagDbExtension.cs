using System;
using Linq2Acad;
using System.Linq;
using ThMEPEngineCore.Algorithm;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service
{
 public  class ThAxialCircleTagDbExtension: ThDbExtension,IDisposable
    {
        public List<Circle> Circles { get; set; }
        public ThAxialCircleTagDbExtension(Database db):base(db)
        {
            LayerFilter = ThAxialCircleTagLayerManager.XrefLayers(db);
            Circles = new List<Circle>();
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
                        Circles.AddRange(BuildElementCurves(blkRef, mcs2wcs));
                    }
                }
            }
        }
        private IEnumerable<Circle> BuildElementCurves(BlockReference blockReference, Matrix3d matrix)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(HostDb))
            {
                var circles = new List<Circle>();
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
                            else if(dbObj is Circle circle)
                            {
                                if (IsBuildElement(circle) &&
                                    CheckLayerValid(circle))
                                {
                                    circles.Add(circle.GetTransformedCopy(matrix) as Circle);
                                }
                            }
                        }
                        var xclip = blockReference.XClipInfo();
                        if (xclip.IsValid)
                        {
                            xclip.TransformBy(matrix);
                            return circles.Where(o => xclip.Contains(o as Circle));
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
