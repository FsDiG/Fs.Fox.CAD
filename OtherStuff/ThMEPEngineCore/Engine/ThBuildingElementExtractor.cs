using Linq2Acad;
using Dreambuild.AutoCAD;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public class ThBuildingElementExtractor
    {
        private List<ThBuildingElementExtractionVisitor> Visitors { get; set; }

        public ThBuildingElementExtractor()
        {
            Visitors = new List<ThBuildingElementExtractionVisitor>();
        }

        public void Accept(ThBuildingElementExtractionVisitor visitor)
        {
            Visitors.Add(visitor);
        }

        public void Extract(Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                foreach (var ent in acadDatabase.ModelSpace)
                {
                    if (ent is BlockReference blkRef)
                    {
                        if (blkRef.BlockTableRecord.IsNull)
                        {
                            continue;
                        }
                        var mcs2wcs = blkRef.BlockTransform.PreMultiplyBy(Matrix3d.Identity);
                        Visitors.ForEach(o =>
                        {
                            o.Results.AddRange(DoExtract(blkRef, mcs2wcs,o));
                        });
                    }
                }
            }
        }

        public void ExtractFromMS(Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                acadDatabase.ModelSpace
                    .OfType<Entity>()
                    .ForEach(e =>
                    {
                        Visitors.ForEach(v =>
                        {
                            v.Results.AddRange(DoExtract(e, v));
                        });
                    });
            }
        }

        private List<ThRawIfcBuildingElementData> DoExtract(Entity e, ThBuildingElementExtractionVisitor visitor)
        {
            var results = new List<ThRawIfcBuildingElementData>();
            if (visitor.IsBuildElement(e) && visitor.CheckLayerValid(e))
            {
                visitor.DoExtract(results, e, Matrix3d.Identity);
            }
            return results;
        }

        private List<ThRawIfcBuildingElementData> DoExtract(
            BlockReference blockReference,Matrix3d matrix,
            ThBuildingElementExtractionVisitor visitor)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(blockReference.Database))
            {
                var results = new List<ThRawIfcBuildingElementData>();
                if (visitor.IsBuildElementBlockReference(blockReference))
                {
                    var blockTableRecord = acadDatabase.Blocks.Element(blockReference.BlockTableRecord);
                    if (visitor.IsBuildElementBlock(blockTableRecord))
                    {
                        // 提取图元信息
                        foreach (var objId in blockTableRecord)
                        {
                            var dbObj = acadDatabase.Element<Entity>(objId);
                            if (dbObj is BlockReference blockObj)
                            {
                                if (blockObj.BlockTableRecord.IsNull)
                                {
                                    continue;
                                }
                                if (visitor.IsBuildElementBlockReference(blockObj))
                                {
                                    var mcs2wcs = blockObj.BlockTransform.PreMultiplyBy(matrix);
                                    results.AddRange(DoExtract(blockObj, mcs2wcs, visitor));
                                }
                            }
                            else
                            {
                                visitor.DoExtract(results, dbObj, matrix);
                            }
                        }

                        // 过滤XClip外的图元信息
                        visitor.DoXClip(results, blockReference, matrix);
                    }
                }
                return results;
            }
        }
    }
}
