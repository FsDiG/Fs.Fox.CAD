using System.Linq;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public abstract class ThDistributionElementExtractionVisitor
    {
        public List<string> LayerFilter { get; set; }
        public List<ThRawIfcDistributionElementData> Results { get; protected set; }

        public ThDistributionElementExtractionVisitor()
        {
            Results = new List<ThRawIfcDistributionElementData>();
        }

        public abstract void DoExtract(List<ThRawIfcDistributionElementData> elements, Entity dbObj, Matrix3d matrix);

        public abstract void DoXClip(List<ThRawIfcDistributionElementData> elements, BlockReference blockReference, Matrix3d matrix);

        public virtual bool IsBuildElement(Entity entity)
        {
            return entity.ObjectId.IsValid;
        }
        public virtual bool IsDistributionElement(Entity entity)
        {
            return false;
        }
        public virtual bool CheckLayerValid(Entity curve)
        {
            return LayerFilter.Where(o => string.Compare(curve.Layer, o, true) == 0).Any();
        }
        public virtual bool IsBuildElementBlock(BlockTableRecord blockTableRecord)
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
        public virtual bool IsBuildElementBlockReference(BlockReference blockReference)
        {
            return blockReference.BlockTableRecord.IsValid;
        }
    }
}
