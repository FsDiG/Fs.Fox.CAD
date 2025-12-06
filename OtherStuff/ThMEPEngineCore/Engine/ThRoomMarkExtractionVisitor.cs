using System;
using System.Linq;
using System.Collections.Generic;
using ThMEPEngineCore.CAD;
using ThMEPEngineCore.Algorithm;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public class ThRoomMarkExtractionVisitor : ThAnnotationElementExtractionVisitor
    {
        public override void DoExtract(List<ThRawIfcAnnotationElementData> elements, Entity dbObj, Matrix3d matrix)
        {
            if (dbObj is DBText dbText)
            {
                elements.AddRange(Handle(dbText, matrix));
            }
            else if (dbObj is MText mText)
            {
                elements.AddRange(Handle(mText, matrix));
            }
        }
        public override void DoExtract(List<ThRawIfcAnnotationElementData> elements, Entity dbObj)
        {
            if (dbObj is DBText dbText)
            {
                elements.AddRange(Handle(dbText));
            }
            else if (dbObj is MText mText)
            {
                elements.AddRange(Handle(mText));
            }
        }


        public override void DoXClip(List<ThRawIfcAnnotationElementData> elements, BlockReference blockReference, Matrix3d matrix)
        {
            var xclip = blockReference.XClipInfo();
            if (xclip.IsValid)
            {
                xclip.TransformBy(matrix);
                elements.RemoveAll(o => !xclip.Contains(GetTextPosition(o.Data)));
            }
        }
        private List<ThRawIfcAnnotationElementData> Handle(DBText dbText, Matrix3d matrix)
        {
            var texts = new List<DBText>();
            if (IsAnnotationElement(dbText) && CheckLayerValid(dbText))
            {
                var clone = dbText.Clone() as DBText;
                clone.TransformBy(matrix);
                texts.Add(clone);
            }
            return texts.Select(o => CreateAnnotationElementData(o)).ToList();
        }

        private List<ThRawIfcAnnotationElementData> Handle(DBText dbText)
        {
            var texts = new List<DBText>();
            if (IsAnnotationElement(dbText) && CheckLayerValid(dbText))
            {
                var clone = dbText.Clone() as DBText;               
                texts.Add(clone);
            }
            return texts.Select(o => CreateAnnotationElementData(o)).ToList();
        }

        private List<ThRawIfcAnnotationElementData> Handle(MText mText, Matrix3d matrix)
        {
            var texts = new List<MText>();
            if (IsAnnotationElement(mText) && CheckLayerValid(mText))
            {
                var clone = mText.Clone() as MText;
                clone.TransformBy(matrix);
                texts.Add(clone);
            }
            return texts.Select(o => CreateAnnotationElementData(o)).ToList();
        }

        private List<ThRawIfcAnnotationElementData> Handle(MText mText)
        {
            var texts = new List<MText>();
            if (IsAnnotationElement(mText) && CheckLayerValid(mText))
            {
                var clone = mText.Clone() as MText;
                texts.Add(clone);
            }
            return texts.Select(o => CreateAnnotationElementData(o)).ToList();
        }

        private ThRawIfcAnnotationElementData CreateAnnotationElementData(DBText dbText)
        {
            return new ThRawIfcAnnotationElementData()
            {
                Data = dbText,
                Geometry = dbText.TextOBB(),
            };
        }

        private ThRawIfcAnnotationElementData CreateAnnotationElementData(MText mText)
        {
            return new ThRawIfcAnnotationElementData()
            {
                Data = mText,
                Geometry = mText.TextOBB(),
            };
        }

        private new bool IsAnnotationElement(Entity entity)
        {
            return base.IsAnnotationElement(entity);
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
