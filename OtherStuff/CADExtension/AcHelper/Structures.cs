namespace AcHelper
{
    /// <summary>
    /// 
    /// </summary>
    public struct TextStyle
    {
        public string Name;
        public string FileName;
        public double TextSize;
        public double ObliquingAngle;
    }
    /// <summary>
    /// 
    /// </summary>
    public struct DimStyle
    {
        public string Name;
        public string TextStyle;
        public double TextHeight;
        public double DimScale;
        public string PreferredLayers;
    }
    /// <summary>
    /// 
    /// </summary>
    public struct Layer
    {
        public string Name;
        public string LineType;
        public short LineWeight;
        public short ColorIndex;
        public AcObjects PreferredObjects;
    }
    /// <summary>
    /// 
    /// </summary>
    public struct LineType
    {
        public string Name;
        public string FileName;
    }
    /// <summary>
    /// 
    /// </summary>
    public struct AcBlock
    {
        public string Name;
        public string PathName;
        public string PreferredLayers;
        public bool TitleBlock;
    }
    /// <summary>
    /// 
    /// </summary>
    public struct AcMultiLine
    {
        public string Name;
        public string FileName;
        public string PreferredLayers;
    }
    /// <summary>
    /// 
    /// </summary>
    public struct AcHatch
    {
        public string Name;
        public string FileName;
        public string PatternType;
        public string PreferredLayers;
    }
    /// <summary>
    /// 
    /// </summary>
    public struct AcAttribute
    {
        public string Tag;
        public string Prompt;
        public string Default;
        public string Format;
        public string Remarks;
        public int TitleblockID;
        public bool Invisible;
        public bool Constant;
        public bool Verify;
        public bool Preset;
        public bool LockPosition;
        public bool Required;
    }
    /// <summary>
    /// 
    /// </summary>
    public struct AcObjects
    {
        public int Arc;
        public int Block;
        public int Circle;
        public int Text;
        public int Dimension;
        public int Hatch;
        public int Line;
        public int MLine;
        public int MText;
        public int Polyline;
        public int Solid;
        public int Viewport;
        public int Wipeout;
        public int XLine;
    }
}
