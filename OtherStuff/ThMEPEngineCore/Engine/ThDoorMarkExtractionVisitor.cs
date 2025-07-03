using System;
using System.Linq;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using ThMEPEngineCore.CAD;
using ThMEPEngineCore.Algorithm;

namespace ThMEPEngineCore.Engine
{
    public class ThRawDoorMark : ThRawIfcBuildingElementData
    {
        //
    }

    public class ThDoorMarkExtractionVisitor : ThBuildingElementExtractionVisitor
    {
        public override void DoExtract(List<ThRawIfcBuildingElementData> elements, Entity dbObj, Matrix3d matrix)
        {
            if (dbObj is DBText dbText)
            {
                elements.AddRange(HandleDbText(dbText, matrix));
            }
            else if (dbObj is MText mText)
            {
                elements.AddRange(HandleMText(mText, matrix));
            }
        }

        public override void DoXClip(List<ThRawIfcBuildingElementData> elements, BlockReference blockReference, Matrix3d matrix)
        {
            var xclip = blockReference.XClipInfo();
            if (xclip.IsValid)
            {
                xclip.TransformBy(matrix);
                elements.RemoveAll(o => !xclip.Contains(GetTextPosition(o.Data)));
            }
        }

        private List<ThRawIfcBuildingElementData> HandleDbText(DBText dbText, Matrix3d matrix)
        {
            var texts = new List<DBText>();
            if (IsBuildElement(dbText) && IsDoorMark(dbText))
            {
                var clone = dbText.Clone() as DBText;
                clone.TransformBy(matrix);
                texts.Add(clone);
            }
            return texts.Select(o => CreateBuildingElementData(o)).ToList();
        }
        private List<ThRawIfcBuildingElementData> HandleMText(MText mText, Matrix3d matrix)
        {
            var texts = new List<MText>();
            if (IsBuildElement(mText) && IsDoorMark(mText))
            {
                var clone = mText.Clone() as MText;
                clone.TransformBy(matrix);
                texts.Add(clone);
            }
            return texts.Select(o => CreateBuildingElementData(o)).ToList();
        }

        private ThRawIfcBuildingElementData CreateBuildingElementData(DBText dbText)
        {
            return new ThRawDoorMark()
            {
                Data = dbText,
                Geometry = dbText.TextOBB(),
            };
        }
        private ThRawIfcBuildingElementData CreateBuildingElementData(MText mText)
        {
            return new ThRawDoorMark()
            {
                Data = mText,
                Geometry = mText.TextOBB(),
            };
        }
        private new bool IsBuildElement(Entity entity)
        {
            return entity.Hyperlinks.Count > 0;
        }
        private bool IsDoorMark(Entity entity)
        {
            var thPropertySet = ThPropertySet.CreateWithHyperlink(entity.Hyperlinks[0].Description);
            return thPropertySet.IsDoor;
        }
        private Point3d GetTextPosition(object ent)
        {
            if (ent is DBText dbText)
            {
                return dbText.Position;
            }
            else if (ent is MText mText)
            {
                return mText.Location;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
