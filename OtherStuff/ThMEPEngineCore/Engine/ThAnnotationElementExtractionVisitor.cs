using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public abstract class ThAnnotationElementExtractionVisitor
    {
        public List<string> LayerFilter { get; set; }
        public List<ThRawIfcAnnotationElementData> Results { get; set; }

        public ThAnnotationElementExtractionVisitor()
        {
            Results = new List<ThRawIfcAnnotationElementData>();
        }

        public abstract void DoExtract(List<ThRawIfcAnnotationElementData> elements, Entity dbObj, Matrix3d matrix);

        public abstract void DoExtract(List<ThRawIfcAnnotationElementData> elements, Entity dbObj);

        public abstract void DoXClip(List<ThRawIfcAnnotationElementData> elements, BlockReference blockReference, Matrix3d matrix);

        public virtual bool IsAnnotationElement (Entity entity)
        {
            return entity.ObjectId.IsValid;
        }
        public virtual bool CheckLayerValid(Entity curve)
        {
            return LayerFilter.Where(o => string.Compare(curve.Layer, o, true) == 0).Any();
        }
        public virtual bool IsAnnotationElementBlock(BlockTableRecord blockTableRecord)
        {
            // 暂时不支持动态块，外部参照，覆盖
            if (blockTableRecord.IsDynamicBlock)
            {
                return false;
            }

            // 忽略图纸空间和匿名块
            if (blockTableRecord.IsLayout || blockTableRecord.IsAnonymous)
            {
                return false;
            }

            // 忽略不可“炸开”的块
            if (!blockTableRecord.Explodable)
            {
                return false;
            }

            return true;
        }
        public virtual bool IsAnnotationElementBlockReference(BlockReference blockReference)
        {
            return blockReference.BlockTableRecord.IsValid;
        }
    }
}
