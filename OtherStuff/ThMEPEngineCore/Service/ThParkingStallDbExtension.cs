using System;
using Linq2Acad;
using System.Linq;
using ThCADExtension;
using ThMEPEngineCore.CAD;
using ThMEPEngineCore.Algorithm;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service
{
    public class ThParkingStallDbExtension : ThDbExtension, IDisposable
    {
        public ThParkingStallDbExtension(Database db) : base(db)
        {
            LayerFilter = ThParkingStallLayerManager.XrefLayers(db);
        }
        public void Dispose()
        {
            throw new NotImplementedException();
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
                        DbObjects.AddRange(BuildElementCurves(blkRef, mcs2wcs));
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
                                    var mcs2wcs = blockObj.BlockTransform.PreMultiplyBy(matrix);

                                    // 车位块的块名无统一规范
                                    // 暂时只能用图层名来识别车位块
                                    if (CheckLayerValid(blockObj))
                                    {
                                        // 获取车位块的OBB
                                        // 用OBB创建一个矩形多段线来“代替”车位块
                                        // 在变换到WCS时，由于误差，变换可能包含NonUniform Scaling
                                        // 考虑到2d Solid支持NonUniform Scaling
                                        // 这里借用2d Solid来完成变换
                                        var btr = acadDatabase.Blocks.Element(blockObj.BlockTableRecord);
                                        var rectangle = btr.GeometricExtents().ToRectangle();
                                        ents.Add(GetTransformedRectangle(rectangle, mcs2wcs));
                                    }
                                    ents.AddRange(BuildElementCurves(blockObj, mcs2wcs));
                                }
                            }
                        }
                    }
                }
                return ents;
            }
        }

        private Polyline GetTransformedRectangle(Polyline rectangle, Matrix3d matrix)
        {
            var solid = rectangle.ToSolid();
            solid.TransformBy(matrix);
            return solid.ToPolyline();
        }

        public override void BuildElementTexts()
        {
            throw new NotImplementedException();
        }
    }
}
