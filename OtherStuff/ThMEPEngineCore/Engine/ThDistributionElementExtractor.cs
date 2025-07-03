using Linq2Acad;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public class ThDistributionElementExtractor
    {
        private List<ThDistributionElementExtractionVisitor> Visitors { get; set; }

        public ThDistributionElementExtractor()
        {
            Visitors = new List<ThDistributionElementExtractionVisitor>();
        }

        public void Accept(ThDistributionElementExtractionVisitor visitor)
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

        private List<ThRawIfcDistributionElementData> DoExtract(BlockReference blockReference, Matrix3d matrix, 
            ThDistributionElementExtractionVisitor visitor)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(blockReference.Database))
            {
                var results = new List<ThRawIfcDistributionElementData>();
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
                                    if (visitor.CheckLayerValid(blockObj) && visitor.IsDistributionElement(blockObj))
                                    {
                                        visitor.DoExtract(results, blockObj, matrix);
                                        continue;
                                    }
                                    var mcs2wcs = blockObj.BlockTransform.PreMultiplyBy(matrix);
                                    results.AddRange(DoExtract(blockObj, mcs2wcs, visitor));
                                }
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
