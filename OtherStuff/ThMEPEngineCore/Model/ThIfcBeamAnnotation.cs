using ThMEPEngineCore.Service;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using ThCADExtension;

namespace ThMEPEngineCore.Model
{
    public class ThIfcBeamAnnotation
    {
        public string TextString { get; private set; }

        public Matrix3d Matrix { get; private set; }

        public Point3d Position { get; set; }

        public Dictionary<string, string> Attributes { get; private set; }

        public ThIfcBeamAnnotation(DBText dBText, Matrix3d matrix)
        {
            Matrix = matrix;
            Position = dBText.Position;
            TextString = dBText.TextString;
            Attributes = ParseAnnotationText(dBText.Hyperlinks[0].DisplayString);
        }

        public Point3d StartPoint
        {
            get
            {
                return ThStructureBeamUtils.Coordinate(Attributes[ThMEPEngineCoreCommon.BEAM_GEOMETRY_STARTPOINT]);
            }
        }

        public Point3d EndPoint
        {
            get
            {
                return ThStructureBeamUtils.Coordinate(Attributes[ThMEPEngineCoreCommon.BEAM_GEOMETRY_ENDPOINT]);
            }
        }

        public Scale2d Size
        {
            get
            {
                return ThStructureBeamUtils.Size(Attributes[ThMEPEngineCoreCommon.BEAM_GEOMETRY_SIZE]);
            }
        }

        private Dictionary<string, string> ParseAnnotationText(string text)
        {
            string[] patterns = text.Split('_');
            return new Dictionary<string, string>()
            {
                { ThMEPEngineCoreCommon.BEAM_GEOMETRY_STARTPOINT, patterns[1]},
                { ThMEPEngineCoreCommon.BEAM_GEOMETRY_ENDPOINT, patterns[2]},
                { ThMEPEngineCoreCommon.BEAM_GEOMETRY_SIZE, patterns[3]}
            };
        }
    }
}
