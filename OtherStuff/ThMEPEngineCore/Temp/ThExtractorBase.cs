
namespace ThMEPEngineCore.Temp
{
    public abstract class ThExtractorBase
    {
        public string Category { get; set; }
        public short ColorIndex { get; set; }
        public ThExtractorBase()
        {
            Category = "";
        }
    }
}
