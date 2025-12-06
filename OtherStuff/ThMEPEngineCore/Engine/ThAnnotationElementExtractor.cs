using Linq2Acad;
using Dreambuild.AutoCAD;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public class ThAnnotationElementExtractor
    {
        private List<ThAnnotationElementExtractionVisitor> Visitors { get; set; }

        public ThAnnotationElementExtractor()
        {
            Visitors = new List<ThAnnotationElementExtractionVisitor>();
        }

        public void Accept(ThAnnotationElementExtractionVisitor visitor)
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
                            o.Results.AddRange(DoExtract(blkRef, mcs2wcs, o));
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

        private List<ThRawIfcAnnotationElementData> DoExtract(Entity e, ThAnnotationElementExtractionVisitor visitor)
        {
            var results = new List<ThRawIfcAnnotationElementData>();
            if (visitor.CheckLayerValid(e))
            {
                visitor.DoExtract(results, e);
            }
            return results;
        }

        private List<ThRawIfcAnnotationElementData> DoExtract(
            BlockReference blockReference, Matrix3d matrix,
            ThAnnotationElementExtractionVisitor visitor)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(blockReference.Database))
            {
                var results = new List<ThRawIfcAnnotationElementData>();
                if (visitor.IsAnnotationElementBlockReference(blockReference))
                {
                    var blockTableRecord = acadDatabase.Blocks.Element(blockReference.BlockTableRecord);
                    if (visitor.IsAnnotationElementBlock(blockTableRecord))
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
                                if (visitor.IsAnnotationElementBlockReference(blockObj))
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
