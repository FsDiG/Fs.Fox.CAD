using System;

namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    ///
    /// </summary>
    public enum MleaderScaling
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,

        /// <summary>
        /// The use non annotative
        /// </summary>
        UseNonAnnotative = 1,

        /// <summary>
        /// The use annotative
        /// </summary>
        UseAnnotative = 2,

        /// <summary>
        /// The use and scale non annotative
        /// </summary>
        UseAndScaleNonAnnotative = 3
    }

    /// <summary>
    ///
    /// </summary>
    public enum TextScaling
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,

        /// <summary>
        /// The use non annotative
        /// </summary>
        UseNonAnnotative = 1,

        /// <summary>
        /// The use annotative
        /// </summary>
        UseAnnotative = 2,

        /// <summary>
        /// The use and scale non annotative
        /// </summary>
        UseAndScaleNonAnnotative = 3
    }

    /// <summary>
    ///
    /// </summary>
    public enum ObjectIdTypeCode
    {
        /// <summary>
        /// The soft pointer identifier
        /// </summary>
        SoftPointerId = 330,

        /// <summary>
        /// The hard pointer identifier
        /// </summary>
        HardPointerId = 340,

        /// <summary>
        /// The soft ownership identifier
        /// </summary>
        SoftOwnershipId = 350,

        /// <summary>
        /// The hard ownership identifier
        /// </summary>
        HardOwnershipId = 360
    }

    /// <summary>
    ///
    /// </summary>
    public enum OverruleStatus
    {
        /// <summary>
        /// The off
        /// </summary>
        Off = 0,

        /// <summary>
        /// The on
        /// </summary>
        On = 1
    }

    /// <summary>
    ///
    /// </summary>
    [Flags]
    public enum OsnapMode : short
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,

        /// <summary>
        /// The endpoint
        /// </summary>
        Endpoint = 1,

        /// <summary>
        /// The midpoint
        /// </summary>
        Midpoint = 2,

        /// <summary>
        /// The center
        /// </summary>
        Center = 4,

        /// <summary>
        /// The node
        /// </summary>
        Node = 8,

        /// <summary>
        /// The quadrant
        /// </summary>
        Quadrant = 16,

        /// <summary>
        /// The intersection
        /// </summary>
        Intersection = 32,

        /// <summary>
        /// The insertion
        /// </summary>
        Insertion = 64,

        /// <summary>
        /// The perpendicular
        /// </summary>
        Perpendicular = 128,

        /// <summary>
        /// The tangent
        /// </summary>
        Tangent = 256,

        /// <summary>
        /// The nearest
        /// </summary>
        Nearest = 512,

        /// <summary>
        /// The quick
        /// </summary>
        Quick = 1024,

        /// <summary>
        /// The apparent intersection
        /// </summary>
        ApparentIntersection = 2048,

        /// <summary>
        /// The extension
        /// </summary>
        Extension = 4096,

        /// <summary>
        /// The parallel
        /// </summary>
        Parallel = 8192,
    }

    /// <summary>
    ///
    /// </summary>
    public enum ColorIndex : short
    {
        /// <summary>
        /// The byblock
        /// </summary>
        BYBLOCK = 0,

        /// <summary>
        /// The red
        /// </summary>
        Red = 1,

        /// <summary>
        /// The yellow
        /// </summary>
        Yellow = 2,

        /// <summary>
        /// The green
        /// </summary>
        Green = 3,

        /// <summary>
        /// The cyan
        /// </summary>
        Cyan = 4,

        /// <summary>
        /// The blue
        /// </summary>
        Blue = 5,

        /// <summary>
        /// The magenta
        /// </summary>
        Magenta = 6,

        /// <summary>
        /// The white
        /// </summary>
        White = 7,

        /// <summary>
        /// The bylayer
        /// </summary>
        BYLAYER = 256
    }

    /// <summary>
    ///
    /// </summary>
    public enum TypeCode
    {
        /// <summary>
        /// The invalid
        /// </summary>
        Invalid = -9999,

        /// <summary>
        /// The x dictionary
        /// </summary>
        XDictionary = -6,

        /// <summary>
        /// The p reactors
        /// </summary>
        PReactors = -5,

        /// <summary>
        /// The operator
        /// </summary>
        Operator = -4,

        /// <summary>
        /// The x data start
        /// </summary>
        XDataStart = -3,

        /// <summary>
        /// The first entity identifier
        /// </summary>
        FirstEntityId = -2,

        /// <summary>
        /// The header identifier
        /// </summary>
        HeaderId = -2,

        /// <summary>
        /// The end
        /// </summary>
        End = -1,

        /// <summary>
        /// The start
        /// </summary>
        Start = 0,

        /// <summary>
        /// The x reference path
        /// </summary>
        XRefPath = 1,

        /// <summary>
        /// The text
        /// </summary>
        Text = 1,

        /// <summary>
        /// The attribute tag
        /// </summary>
        AttributeTag = 2,

        /// <summary>
        /// The shape name
        /// </summary>
        ShapeName = 2,

        /// <summary>
        /// The block name
        /// </summary>
        BlockName = 2,

        /// <summary>
        /// The symbol table name
        /// </summary>
        SymbolTableName = 2,

        /// <summary>
        /// The mline style name
        /// </summary>
        MlineStyleName = 2,

        /// <summary>
        /// The symbol table record name
        /// </summary>
        SymbolTableRecordName = 2,

        /// <summary>
        /// The description
        /// </summary>
        Description = 3,

        /// <summary>
        /// The text font file
        /// </summary>
        TextFontFile = 3,

        /// <summary>
        /// The attribute prompt
        /// </summary>
        AttributePrompt = 3,

        /// <summary>
        /// The linetype prose
        /// </summary>
        LinetypeProse = 3,

        /// <summary>
        /// The dim style name
        /// </summary>
        DimStyleName = 3,

        /// <summary>
        /// The dim post string
        /// </summary>
        DimPostString = 3,

        /// <summary>
        /// The cl shape name
        /// </summary>
        CLShapeName = 4,

        /// <summary>
        /// The dimension alternative prefix suffix
        /// </summary>
        DimensionAlternativePrefixSuffix = 4,

        /// <summary>
        /// The text big font file
        /// </summary>
        TextBigFontFile = 4,

        /// <summary>
        /// The symbol table record comments
        /// </summary>
        SymbolTableRecordComments = 4,

        /// <summary>
        /// The handle
        /// </summary>
        Handle = 5,

        /// <summary>
        /// The dimension block
        /// </summary>
        DimensionBlock = 5,

        /// <summary>
        /// The linetype name
        /// </summary>
        LinetypeName = 6,

        /// <summary>
        /// The dim BLK1
        /// </summary>
        DimBlk1 = 6,

        /// <summary>
        /// The dim BLK2
        /// </summary>
        DimBlk2 = 7,

        /// <summary>
        /// The text style name
        /// </summary>
        TextStyleName = 7,

        /// <summary>
        /// The layer name
        /// </summary>
        LayerName = 8,

        /// <summary>
        /// The cl shape text
        /// </summary>
        CLShapeText = 9,

        /// <summary>
        /// The x coordinate
        /// </summary>
        XCoordinate = 10,

        /// <summary>
        /// The y coordinate
        /// </summary>
        YCoordinate = 20,

        /// <summary>
        /// The z coordinate
        /// </summary>
        ZCoordinate = 30,

        /// <summary>
        /// The elevation
        /// </summary>
        Elevation = 38,

        /// <summary>
        /// The thickness
        /// </summary>
        Thickness = 39,

        /// <summary>
        /// The text size
        /// </summary>
        TxtSize = 40,

        /// <summary>
        /// The viewport height
        /// </summary>
        ViewportHeight = 40,

        /// <summary>
        /// The real
        /// </summary>
        Real = 40,

        /// <summary>
        /// The view width
        /// </summary>
        ViewWidth = 41,

        /// <summary>
        /// The text style x scale
        /// </summary>
        TxtStyleXScale = 41,

        /// <summary>
        /// The viewport aspect
        /// </summary>
        ViewportAspect = 41,

        /// <summary>
        /// The text style p size
        /// </summary>
        TxtStylePSize = 42,

        /// <summary>
        /// The view lens length
        /// </summary>
        ViewLensLength = 42,

        /// <summary>
        /// The view front clip
        /// </summary>
        ViewFrontClip = 43,

        /// <summary>
        /// The view back clip
        /// </summary>
        ViewBackClip = 44,

        /// <summary>
        /// The shape x offset
        /// </summary>
        ShapeXOffset = 44,

        /// <summary>
        /// The view height
        /// </summary>
        ViewHeight = 45,

        /// <summary>
        /// The shape y offset
        /// </summary>
        ShapeYOffset = 45,

        /// <summary>
        /// The shape scale
        /// </summary>
        ShapeScale = 46,

        /// <summary>
        /// The pixel scale
        /// </summary>
        PixelScale = 47,

        /// <summary>
        /// The linetype scale
        /// </summary>
        LinetypeScale = 48,

        /// <summary>
        /// The dash length
        /// </summary>
        DashLength = 49,

        /// <summary>
        /// The mline offset
        /// </summary>
        MlineOffset = 49,

        /// <summary>
        /// The linetype element
        /// </summary>
        LinetypeElement = 49,

        /// <summary>
        /// The viewport snap angle
        /// </summary>
        ViewportSnapAngle = 50,

        /// <summary>
        /// The angle
        /// </summary>
        Angle = 50,

        /// <summary>
        /// The viewport twist
        /// </summary>
        ViewportTwist = 51,

        /// <summary>
        /// The visibility
        /// </summary>
        Visibility = 60,

        /// <summary>
        /// The layer linetype
        /// </summary>
        LayerLinetype = 61,

        /// <summary>
        /// The color
        /// </summary>
        Color = 62,

        /// <summary>
        /// The has subentities
        /// </summary>
        HasSubentities = 66,

        /// <summary>
        /// The viewport visibility
        /// </summary>
        ViewportVisibility = 67,

        /// <summary>
        /// The viewport active
        /// </summary>
        ViewportActive = 68,

        /// <summary>
        /// The viewport number
        /// </summary>
        ViewportNumber = 69,

        /// <summary>
        /// The int16
        /// </summary>
        Int16 = 70,

        /// <summary>
        /// The view mode
        /// </summary>
        ViewMode = 71,

        /// <summary>
        /// The text style flags
        /// </summary>
        TxtStyleFlags = 71,

        /// <summary>
        /// The reg application flags
        /// </summary>
        RegAppFlags = 71,

        /// <summary>
        /// The circle sides
        /// </summary>
        CircleSides = 72,

        /// <summary>
        /// The linetype align
        /// </summary>
        LinetypeAlign = 72,

        /// <summary>
        /// The viewport zoom
        /// </summary>
        ViewportZoom = 73,

        /// <summary>
        /// The linetype PDC
        /// </summary>
        LinetypePdc = 73,

        /// <summary>
        /// The viewport icon
        /// </summary>
        ViewportIcon = 74,

        /// <summary>
        /// The viewport snap
        /// </summary>
        ViewportSnap = 75,

        /// <summary>
        /// The viewport grid
        /// </summary>
        ViewportGrid = 76,

        /// <summary>
        /// The viewport snap style
        /// </summary>
        ViewportSnapStyle = 77,

        /// <summary>
        /// The viewport snap pair
        /// </summary>
        ViewportSnapPair = 78,

        /// <summary>
        /// The int32
        /// </summary>
        Int32 = 90,

        /// <summary>
        /// The subclass
        /// </summary>
        Subclass = 100,

        /// <summary>
        /// The embedded object start
        /// </summary>
        EmbeddedObjectStart = 101,

        /// <summary>
        /// The control string
        /// </summary>
        ControlString = 102,

        /// <summary>
        /// The dim variable handle
        /// </summary>
        DimVarHandle = 105,

        /// <summary>
        /// The ucs org
        /// </summary>
        UcsOrg = 110,

        /// <summary>
        /// The ucs orientation x
        /// </summary>
        UcsOrientationX = 111,

        /// <summary>
        /// The ucs orientation y
        /// </summary>
        UcsOrientationY = 112,

        /// <summary>
        /// The x real
        /// </summary>
        XReal = 140,

        /// <summary>
        /// The view brightness
        /// </summary>
        ViewBrightness = 141,

        /// <summary>
        /// The view contrast
        /// </summary>
        ViewContrast = 142,

        /// <summary>
        /// The int64
        /// </summary>
        Int64 = 160,

        /// <summary>
        /// The x int16
        /// </summary>
        XInt16 = 170,

        /// <summary>
        /// The normal x
        /// </summary>
        NormalX = 210,

        /// <summary>
        /// The normal y
        /// </summary>
        NormalY = 220,

        /// <summary>
        /// The normal z
        /// </summary>
        NormalZ = 230,

        /// <summary>
        /// The xx int16
        /// </summary>
        XXInt16 = 270,

        /// <summary>
        /// The int8
        /// </summary>
        Int8 = 280,

        /// <summary>
        /// The render mode
        /// </summary>
        RenderMode = 281,

        /// <summary>
        /// The bool
        /// </summary>
        Bool = 290,

        /// <summary>
        /// The x text string
        /// </summary>
        XTextString = 300,

        /// <summary>
        /// The binary chunk
        /// </summary>
        BinaryChunk = 310,

        /// <summary>
        /// The arbitrary handle
        /// </summary>
        ArbitraryHandle = 320,

        /// <summary>
        /// The soft pointer identifier
        /// </summary>
        SoftPointerId = 330,

        /// <summary>
        /// The hard pointer identifier
        /// </summary>
        HardPointerId = 340,

        /// <summary>
        /// The soft ownership identifier
        /// </summary>
        SoftOwnershipId = 350,

        /// <summary>
        /// The hard ownership identifier
        /// </summary>
        HardOwnershipId = 360,

        /// <summary>
        /// The line weight
        /// </summary>
        LineWeight = 370,

        /// <summary>
        /// The plot style name type
        /// </summary>
        PlotStyleNameType = 380,

        /// <summary>
        /// The plot style name identifier
        /// </summary>
        PlotStyleNameId = 390,

        /// <summary>
        /// The extended int16
        /// </summary>
        ExtendedInt16 = 400,

        /// <summary>
        /// The layout name
        /// </summary>
        LayoutName = 410,

        /// <summary>
        /// The color RGB
        /// </summary>
        ColorRgb = 420,

        /// <summary>
        /// The color name
        /// </summary>
        ColorName = 430,

        /// <summary>
        /// The alpha
        /// </summary>
        Alpha = 440,

        /// <summary>
        /// The gradient object type
        /// </summary>
        GradientObjType = 450,

        /// <summary>
        /// The gradient pat type
        /// </summary>
        GradientPatType = 451,

        /// <summary>
        /// The gradient tint type
        /// </summary>
        GradientTintType = 452,

        /// <summary>
        /// The gradient col count
        /// </summary>
        GradientColCount = 453,

        /// <summary>
        /// The gradient angle
        /// </summary>
        GradientAngle = 460,

        /// <summary>
        /// The gradient shift
        /// </summary>
        GradientShift = 461,

        /// <summary>
        /// The gradient tint value
        /// </summary>
        GradientTintVal = 462,

        /// <summary>
        /// The gradient col value
        /// </summary>
        GradientColVal = 463,

        /// <summary>
        /// The gradient name
        /// </summary>
        GradientName = 470,

        /// <summary>
        /// The comment
        /// </summary>
        Comment = 999
    }

    /// <summary>
    ///
    /// </summary>
    public enum XDataTypeCode
    {
        /// <summary>
        /// The ASCII string
        /// </summary>
        AsciiString = 1000,

        /// <summary>
        /// The reg application name
        /// </summary>
        RegAppName = 1001,

        /// <summary>
        /// The control string
        /// </summary>
        ControlString = 1002,

        /// <summary>
        /// The layer name
        /// </summary>
        LayerName = 1003,

        /// <summary>
        /// The binary chunk
        /// </summary>
        BinaryChunk = 1004,

        /// <summary>
        /// The handle
        /// </summary>
        Handle = 1005,

        /// <summary>
        /// The x coordinate
        /// </summary>
        XCoordinate = 1010,

        /// <summary>
        /// The world x coordinate
        /// </summary>
        WorldXCoordinate = 1011,

        /// <summary>
        /// The world x disp
        /// </summary>
        WorldXDisp = 1012,

        /// <summary>
        /// The world x dir
        /// </summary>
        WorldXDir = 1013,

        /// <summary>
        /// The y coordinate
        /// </summary>
        YCoordinate = 1020,

        /// <summary>
        /// The world y coordinate
        /// </summary>
        WorldYCoordinate = 1021,

        /// <summary>
        /// The world y disp
        /// </summary>
        WorldYDisp = 1022,

        /// <summary>
        /// The world y dir
        /// </summary>
        WorldYDir = 1023,

        /// <summary>
        /// The z coordinate
        /// </summary>
        ZCoordinate = 1030,

        /// <summary>
        /// The world z coordinate
        /// </summary>
        WorldZCoordinate = 1031,

        /// <summary>
        /// The world z disp
        /// </summary>
        WorldZDisp = 1032,

        /// <summary>
        /// The world z dir
        /// </summary>
        WorldZDir = 1033,

        /// <summary>
        /// The real
        /// </summary>
        Real = 1040,

        /// <summary>
        /// The dist
        /// </summary>
        Dist = 1041,

        /// <summary>
        /// The scale
        /// </summary>
        Scale = 1042,

        /// <summary>
        /// The integer16
        /// </summary>
        Integer16 = 1070,

        /// <summary>
        /// The integer32
        /// </summary>
        Integer32 = 1071
    }

    /// <summary>
    ///
    /// </summary>
    public enum XrecordTypeCode
    {
        /// <summary>
        /// The string
        /// </summary>
        String = 1,

        /// <summary>
        /// The point3d
        /// </summary>
        Point3d = 10,

        /// <summary>
        /// The integer
        /// </summary>
        Integer = 90,

        /// <summary>
        /// The double
        /// </summary>
        Double = 140,

        /// <summary>
        /// The int64
        /// </summary>
        Int64 = 160,

        /// <summary>
        /// The int16
        /// </summary>
        Int16 = 170,

        /// <summary>
        /// The int8
        /// </summary>
        Int8 = 280,

        /// <summary>
        /// The handle
        /// </summary>
        Handle = 320,

        /// <summary>
        /// The soft pointer identifier
        /// </summary>
        SoftPointerId = 330,

        /// <summary>
        /// The hard pointer identifier
        /// </summary>
        HardPointerId = 340,

        /// <summary>
        /// The soft ownership identifier
        /// </summary>
        SoftOwnershipId = 350,

        /// <summary>
        /// The hard ownership identifier
        /// </summary>
        HardOwnershipId = 360
    }

    /// <summary>
    ///
    /// </summary>
    public enum ResultTypeCode : int
    {
        /// <summary>
        /// The rtnone
        /// </summary>
        RTNONE = 0x1388,

        /// <summary>
        /// The rtreal
        /// </summary>
        RTREAL = 0x1389,

        /// <summary>
        /// The rtpoint
        /// </summary>
        RTPOINT = 0x138a,

        /// <summary>
        /// The rtshort
        /// </summary>
        RTSHORT = 0x138b,

        /// <summary>
        /// The rtang
        /// </summary>
        RTANG = 5004,

        /// <summary>
        /// The RTSTR
        /// </summary>
        RTSTR = 0x138d,

        /// <summary>
        /// The rtename
        /// </summary>
        RTENAME = 0x138e,

        /// <summary>
        /// The rtpicks
        /// </summary>
        RTPICKS = 0x138f,

        /// <summary>
        /// The rtorint
        /// </summary>
        RTORINT = 5008,

        /// <summary>
        /// The r t3 dpoint
        /// </summary>
        RT3DPOINT = 0x1391,

        /// <summary>
        /// The rtlong
        /// </summary>
        RTLONG = 0x1392,

        /// <summary>
        /// The rtvoid
        /// </summary>
        RTVOID = 5014,

        /// <summary>
        /// The RTLB
        /// </summary>
        RTLB = 5016,

        /// <summary>
        /// The rtle
        /// </summary>
        RTLE = 5017,

        /// <summary>
        /// The rtdote
        /// </summary>
        RTDOTE = 5018,

        /// <summary>
        /// The rtnil
        /// </summary>
        RTNIL = 5019,

        /// <summary>
        /// The RTDX f0
        /// </summary>
        RTDXF0 = 5020,

        /// <summary>
        /// The RTT
        /// </summary>
        RTT = 5021,

        /// <summary>
        /// The rtresbuf
        /// </summary>
        RTRESBUF = 5023,

        /// <summary>
        /// The rtmodeless
        /// </summary>
        RTMODELESS = 5027,

        /// <summary>
        /// The rtin T64
        /// </summary>
        RTINT64 = 5031
    }

    /// <summary>
    ///
    /// </summary>
    [Flags]
    public enum SymbolTableRecordFilter
    {
        /// <summary>
        /// No filter is used
        /// </summary>
        None = 0,

        /// <summary>
        /// Includes erased entities
        /// </summary>
        IncludedErased = 1,

        /// <summary>
        /// This returns symboltablerecords from xref's
        /// </summary>
        IncludeDependent = 2,

        /// <summary>
        /// The included erased and dependent
        /// </summary>
        IncludedErasedAndDependent = IncludedErased | IncludeDependent
    }

    /// <summary>
    ///
    /// </summary>
    public static class SymbolTableRecordFilterExtensions
    {
        /// <summary>
        /// Determines whether the specified test flag is set.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="testFlag">The test flag.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">testFlag</exception>
        public static bool IsSet(this SymbolTableRecordFilter filter, SymbolTableRecordFilter testFlag)
        {
            if (testFlag == 0)
            {
                throw new ArgumentNullException("testFlag");
            }
            return (filter & testFlag) == testFlag;
        }
    }
}