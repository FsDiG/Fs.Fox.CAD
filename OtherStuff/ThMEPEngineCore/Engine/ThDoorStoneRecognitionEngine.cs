using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public class ThDoorStoneExtractionEngine : ThBuildingElementExtractionEngine
    {
        public override void Extract(Database database)
        {
            var visitor = new ThDoorStoneExtractionVisitor();            
            var extractor = new ThBuildingElementExtractor();
            extractor.Accept(visitor);
            extractor.Extract(database);
            Results = visitor.Results;
        }
    }
}
