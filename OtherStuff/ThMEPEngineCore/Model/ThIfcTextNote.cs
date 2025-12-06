using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Model
{
    public class ThIfcTextNote : ThIfcAnnotation
    {
        public string Text { get; set; } = "";
        public Polyline Geometry { get; set; }
        
        private ThIfcTextNote(string text, Polyline geometry)
        {
            Text = text;
            Geometry = geometry;
        }

        public static ThIfcTextNote Create(string text, Polyline geometry)
        {
            return new ThIfcTextNote(text, geometry);
        }
    }
}
