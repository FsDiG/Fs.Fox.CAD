using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThMEPEngineCore.Algorithm;
using ThMEPEngineCore.Model;
using ThMEPEngineCore.Service;


namespace ThMEPEngineCore.Engine
{
    public class ThBeamExtractionVisitor : ThBuildingElementExtractionVisitor
    {
        public override void DoExtract(List<ThRawIfcBuildingElementData> elements, Entity dbObj, Matrix3d matrix)
        {
            if (dbObj is DBText dBText)
            {
                elements.AddRange(HandleDbText(dBText, matrix));
            }
        }

        public override void DoXClip(List<ThRawIfcBuildingElementData> elements, BlockReference blockReference, Matrix3d matrix)
        {
            var xclip = blockReference.XClipInfo();
            if (xclip.IsValid)
            {
                xclip.TransformBy(matrix);
                elements.RemoveAll(o =>
                {
                    var annotation = o.Data as ThIfcBeamAnnotation;
                    return !xclip.Contains(annotation.Position.TransformBy(annotation.Matrix));
                });
            }
        }

        public override bool IsBuildElement(Entity entity)
        {
            return entity.Hyperlinks.Count > 0;
        }

        private List<ThRawIfcBuildingElementData> HandleDbText(DBText dBText, Matrix3d matrix)
        {
            var results = new List<ThRawIfcBuildingElementData>();
            if (CheckLayerValid(dBText) &&
                IsBuildElement(dBText) &&
                IsBeamAnnotaion(dBText))
            {
                results.Add(new ThRawIfcBuildingElementData()
                {
                    Data = new ThIfcBeamAnnotation(dBText, matrix),
                });
            }
            return results;
        }

        private bool IsBeamAnnotaion(DBText dBText)
        {
            return ThStructureBeamUtils.IsBeamAnnotaion(dBText.TextString);
        }
    }
}
