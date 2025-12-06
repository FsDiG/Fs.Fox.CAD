using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using System.Reflection;
using System.Drawing;

namespace DotNetARX
{
    /// <summary>
    /// 文字的字体样式
    /// </summary>
    public enum FontStyle
    {
        /// <summary>
        /// 常规
        /// </summary>
        Regular,
        /// <summary>
        /// 斜体
        /// </summary>
        Italic,
        /// <summary>
        /// 加粗
        /// </summary>
        Bold,
        /// <summary>
        /// 加粗斜体
        /// </summary>
        BoldItalic
    }

    /// <summary>
    /// 如何获取自动对齐点
    /// </summary>
    public enum AlignmentPointAcquisition
    {
        /// <summary>
        /// 自动获取自动对齐点
        /// </summary>
        Automatic,
        /// <summary>
        /// 用户必须使用shift键获取自动对齐点
        /// </summary>
        ShiftToAcquire
    }

    /// <summary>
    /// 指定若图形包含第三方应用程序所创建的自定义对象时，是否以及何时由 AutoCAD 按需加载该应用程序
    /// </summary>
    public enum ARXDemandLoad
    {
        /// <summary>
        /// 关闭按需加载
        /// </summary>
        Disabled,
        /// <summary>
        /// 在打开包含自定义对象的图形时按需加载源应用程序
        /// </summary>
        OnObjectDetect,
        /// <summary>
        /// 在调用应用程序的某个命令时按需加载源应用程序
        /// </summary>
        CmdInvoke
    }

    /// <summary>
    /// 指定外部参照对象的按需加载性能
    /// </summary>
    public enum XRefDemandLoad
    {
        /// <summary>
        /// 关闭按需加载，加载整个参照图形
        /// </summary>
        Disabled,
        /// <summary>
        /// 打开按需加载来提高AutoCAD性能
        /// </summary>
        LoadEnabled,
        /// <summary>
        /// 打开按需加载但使用参照图形的临时副本
        /// </summary>
        EnabledWithCopy
    }

    /// <summary>
    /// 控制图形中由第三方应用程序所创建对象的显示
    /// </summary>
    public enum ProxyImage
    {
        /// <summary>
        /// 不显示代理对象
        /// </summary>
        NotShow,
        /// <summary>
        /// 显示所有代理对象的图形图像
        /// </summary>
        Show,
        /// <summary>
        /// 仅显示所有代理对象的边框
        /// </summary>
        BoundingBox
    }

    /// <summary>
    /// 指定图形的保存类型
    /// </summary>
    public enum SaveAsType
    {
        /// <summary>
        /// AutoCAD R14 DWG (*.dwg)
        /// </summary>
        acR14_dwg = 8,
        /// <summary>
        /// AutoCAD 2000 DWG (*.dwg)
        /// </summary>
        ac2000_dwg = 12,
        /// <summary>
        /// AutoCAD 2000 DXF (*.dxf)
        /// </summary>
        ac2000_dxf = 13,
        /// <summary>
        /// AutoCAD 2000 图形样板文件 (*.dwt)
        /// </summary>
        ac2000_Template = 14,
        /// <summary>
        /// AutoCAD 2004 DWG (*.dwg)
        /// </summary>
        ac2004_dwg = 24,
        /// <summary>
        /// AutoCAD 2004 DXF (*.dxf)
        /// </summary>
        ac2004_dxf = 25,
        /// <summary>
        /// AutoCAD 2004 图形样板文件 (*.dwt)
        /// </summary>
        ac2004_Template = 26,
        /// <summary>
        /// AutoCAD 2007 DWG (*.dwg)
        /// </summary>
        ac2007_dwg = 36,
        /// <summary>
        /// AutoCAD 2007 DXF (*.dxf)
        /// </summary>
        ac2007_dxf = 37,
        /// <summary>
        /// AutoCAD 2007 图形样板文件 (*.dwt)
        /// </summary>
        ac2007_Template = 38,
        /// <summary>
        /// AutoCAD 2010 DWG (*.dwg)
        /// </summary>
        ac2010_dwg = 48,
        /// <summary>
        /// AutoCAD 2010 DXF (*.dxf)
        /// </summary>
        ac2010_dxf,
        /// <summary>
        /// AutoCAD 2010 图形样板文件 (*.dwt)
        /// </summary>
        ac2010_Template,
        /// <summary>
        /// 与当前图形版本格式相同
        /// </summary>
        acNative = 36,
        /// <summary>
        /// 图形类型未知或无效
        /// </summary>
        acUnknown = -1,
    }

    /// <summary>
    /// 指定OLE对象的打印质量 
    /// </summary>
    public enum OleQuality
    {
        /// <summary>
        /// 线条
        /// </summary>
        LineArt,
        /// <summary>
        /// 文字
        /// </summary>
        Text,
        /// <summary>
        /// 图形
        /// </summary>
        Graphics,
        /// <summary>
        /// 照片
        /// </summary>
        Photo,
        /// <summary>
        /// 高质量照片
        /// </summary>
        HighPhoto
    }

    /// <summary>
    /// 确定在创建新图形时对象颜色属性是否与打印样式名称关联
    /// </summary>
    public enum PlotPolicy
    {
        /// <summary>
        /// 为新图形或早期的AutoCAD版本下的图形指定颜色相关打印样式
        /// </summary>
        Legacy,
        /// <summary>
        /// 为新图形或早期的AutoCAD版本下的图形指定命名打印样式
        /// </summary>
        Named
    }

    /// <summary>
    /// 指定当由于I/O 端口冲突造成向设备的输出必须通过系统打印机缓冲时是否警告用户
    /// </summary>
    public enum PrinterSpoolAlert
    {
        /// <summary>
        /// 始终警告并创建错误日志
        /// </summary>
        AlwaysAlert,
        /// <summary>
        /// 仅第一次警告但创建所有错误日志
        /// </summary>
        AlertOnce,
        /// <summary>
        /// 从不警告但创建所有错误日志
        /// </summary>
        NeverAlertLogOnce,
        /// <summary>
        /// 从不警告也从不创建任何错误日志
        /// </summary>
        NeverAlert
    }

    /// <summary>
    /// 确定在 AutoCAD 设计中心源图形未分配插入单位的对象自动使用的单位
    /// </summary>
    public enum InsertUnits
    {
        /// <summary>
        /// 不指定（无单位）
        /// </summary>
        Unitless,
        /// <summary>
        /// 英寸
        /// </summary>
        Inches,
        /// <summary>
        /// 英尺
        /// </summary>
        Feet,
        /// <summary>
        /// 英里
        /// </summary>
        Miles,
        /// <summary>
        /// 毫米
        /// </summary>
        Millimeters,
        /// <summary>
        /// 厘米
        /// </summary>
        Centimeters,
        /// <summary>
        /// 米
        /// </summary>
        Meters,
        /// <summary>
        /// 公里
        /// </summary>
        Kilometers,
        /// <summary>
        /// 微英寸
        /// </summary>
        Microinches,
        /// <summary>
        /// 英里
        /// </summary>
        Mils,
        /// <summary>
        /// 码
        /// </summary>
        Yards,
        /// <summary>
        /// 埃
        /// </summary>
        Angstroms,
        /// <summary>
        /// 纳米
        /// </summary>
        Nanometers,
        /// <summary>
        /// 微米
        /// </summary>
        Microns,
        /// <summary>
        /// 分米
        /// </summary>
        Decimeters,
        /// <summary>
        /// 十米
        /// </summary>
        Decameters,
        /// <summary>
        /// 百米
        /// </summary>
        Hectometers,
        /// <summary>
        /// 百万公里
        /// </summary>
        Gigameters,
        /// <summary>
        /// 天文单位
        /// </summary>
        AstronomicalUnits,
        /// <summary>
        /// 光年
        /// </summary>
        LightYears,
        /// <summary>
        /// 秒差距
        /// </summary>
        Parsecs
    }

    /// <summary>
    /// 指定 Windows 标准或 AutoCAD 传统快捷键
    /// </summary>
    public enum KeyboardAccelerator
    {
        /// <summary>
        /// 使用 AutoCAD 传统键盘设置
        /// </summary>
        PreferenceClassic,
        /// <summary>
        /// 使用 Windows 标准键盘设置
        /// </summary>
        PreferenceCustom
    }

    /// <summary>
    /// 控制 AutoCAD 如何响应坐标数据的输入
    /// </summary>
    public enum KeyboardPriority
    {
        /// <summary>
        /// 当输入坐标时，严格遵照对象捕捉模式
        /// </summary>
        RunningObjSnap,
        /// <summary>
        /// 当输入坐标时，严格遵照键盘输入模式
        /// </summary>
        Entry,
        /// <summary>
        /// 当输入坐标时，严格按照键盘输入模式。但当坐标是通过脚本输入时，遵照对象捕捉模式
        /// </summary>
        EntryExceptScripts
    }

    /// <summary>
    /// 决定处于命令模式 (有一个命令正在执行) 时绘图区域中右键单击的功能
    /// </summary>
    public enum DrawingAreaSCMCommand
    {
        /// <summary>
        /// 禁用命令快捷菜单,结果为当命令期间按右键为回车
        /// </summary>
        Enter,
        /// <summary>
        /// 启用命令快捷菜单
        /// </summary>
        EnableSCM,
        /// <summary>
        /// 只在命令行提示中选项当前有效时启用命令快捷菜单。在命令行提示中，选项是在方括号[]中。如果没有有效的选项，右键为回车
        /// </summary>
        EnableSCMOptions
    }

    /// <summary>
    /// 决定处于默认模式 (未选中对象且无命令在执行) 时绘图区域中右键单击的功能
    /// </summary>
    public enum DrawingAreaSCMDefault
    {
        /// <summary>
        /// 禁用默认快捷菜单
        /// </summary>
        RepeatLastCommand,
        /// <summary>
        /// 启用默认快捷菜单
        /// </summary>
        SCM
    }

    /// <summary>
    /// 决定处于编辑模式 (选中一个或多个对象且无命令在执行) 时绘图区域中右键单击的功能
    /// </summary>
    public enum DrawingAreaSCMEdit
    {
        /// <summary>
        /// 禁用编辑快捷菜单
        /// </summary>
        EdRepeatLastCommand,
        /// <summary>
        /// 启用编辑快捷菜单
        /// </summary>
        EdSCM
    }

    /// <summary>
    /// 封装COM的Preferences类，该类指定当前 AutoCAD 设置
    /// </summary>
    public static class Preferences
    {
        //获取Preferences对象(COM类）
        static Type AcadPreferences = Type.GetTypeFromHandle(Type.GetTypeHandle(Application.Preferences));  
      
        /// <summary>
        /// 获取“选项”对话框对应选项卡上选项的属性值
        /// </summary>
        /// <param name="ProjectName">选项卡名称</param>
        /// <param name="PropertyName">属性名称</param>
        /// <returns>返回选项的属性值</returns>
        public static object GetProperties(string ProjectName, string PropertyName)
        {
            try
            {
                //通过后期绑定的方式调用Preferences对象的ProjectName属性
                object obj = AcadPreferences.InvokeMember(ProjectName, BindingFlags.GetProperty, null, Application.Preferences, new object[0]);
                //获取ProjectName属性对应的COM类
                Type AcadPreferencesUnknown = Type.GetTypeFromHandle(Type.GetTypeHandle(obj));
                //获取ProjectName属性对应的COM类的PropertyName属性
                return (object)AcadPreferencesUnknown.InvokeMember(PropertyName, BindingFlags.GetProperty, null, obj, new object[0]);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 设置“选项”对话框对应选项卡上选项的属性值
        /// </summary>
        /// <param name="ProjectName">选项卡名称</param>
        /// <param name="PropertyName">属性名称</param>
        /// <param name="Value">属性值</param>
        /// <returns>如果属性设置成功，则返回true，否则返回false</returns>
        public static bool SetProperties(string ProjectName, string PropertyName, object Value)
        {
            try
            {
                object obj = AcadPreferences.InvokeMember(ProjectName, BindingFlags.GetProperty, null, Application.Preferences, new object[0]);
                Type AcadPreferencesUnknown = Type.GetTypeFromHandle(Type.GetTypeHandle(obj));
                AcadPreferencesUnknown.InvokeMember(PropertyName, BindingFlags.SetProperty, null, obj, new object[1] { Value });
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// “选项”对话框“显示”选项卡的所有选项
        /// </summary>
        public static class Display
        {
            /// <summary>
            /// 获取或设置自动追踪矢量的颜色
            /// </summary>
            public static Color AutoTrackingVecColor
            {
                get { return ColorTranslator.FromOle(Convert.ToInt32(Preferences.GetProperties("Display", "AutoTrackingVecColor"))); }
                set { Preferences.SetProperties("Display", "AutoTrackingVecColor", ColorTranslator.ToOle(value)); }
            }

            /// <summary>
            /// 获取或设置十字光标的大小（屏幕大小的百分比）
            /// </summary>
            public static int CursorSize
            {
                get { return (int)Preferences.GetProperties("Display", "CursorSize"); }
                set { Preferences.SetProperties("Display", "CursorSize", value); }
            }

            /// <summary>
            /// 获取或设置是否在图形编辑器中显示模型和布局选项卡
            /// </summary>
            public static bool DisplayLayoutTabs
            {
                get { return (bool)Preferences.GetProperties("Display", "DisplayLayoutTabs"); }
                set { Preferences.SetProperties("Display", "DisplayLayoutTabs", value); }
            }

            /// <summary>
            /// 获取或设置是否在图形窗口的右边显示屏幕菜单。
            /// </summary>
            public static bool DisplayScreenMenu
            {
                get { return (bool)Preferences.GetProperties("Display", "DisplayScreenMenu"); }
                set { Preferences.SetProperties("Display", "DisplayScreenMenu", value); }
            }

            /// <summary>
            /// 获取或设置是否在图形窗口的下边和右边显示滚动条
            /// </summary>
            public static bool DisplayScrollBars
            {
                get { return (bool)Preferences.GetProperties("Display", "DisplayScrollBars"); }
                set { Preferences.SetProperties("Display", "DisplayScrollBars", value); }
            }

            /// <summary>
            /// 获取或设置命令窗口所显示的文字行数
            /// </summary>
            public static int DockedVisibleLines
            {
                get { return (int)Preferences.GetProperties("Display", "DockedVisibleLines"); }
                set { Preferences.SetProperties("Display", "DockedVisibleLines", value); }
            }

            /// <summary>
            /// 获取或设置图纸空间布局的背景颜色
            /// </summary>
            public static Color GraphicsWinLayoutBackgrndColor
            {
                get { return ColorTranslator.FromOle(Convert.ToInt32(Preferences.GetProperties("Display", "GraphicsWinLayoutBackgrndColor"))); }
                set { Preferences.SetProperties("Display", "GraphicsWinLayoutBackgrndColor", ColorTranslator.ToOle(value)); }
            }

            /// <summary>
            /// 获取或设置模型空间窗口的背景颜色
            /// </summary>
            public static Color GraphicsWinModelBackgrndColor
            {
                get { return ColorTranslator.FromOle(Convert.ToInt32(Preferences.GetProperties("Display", "GraphicsWinModelBackgrndColor"))); }
                set { Preferences.SetProperties("Display", "GraphicsWinModelBackgrndColor", ColorTranslator.ToOle(value)); }
            }

            /// <summary>
            /// 获取或设置保留在内存中的文本窗口中的文字行数
            /// </summary>
            public static int HistoryLines
            {
                get { return (int)Preferences.GetProperties("Display", "HistoryLines"); }
                set { Preferences.SetProperties("Display", "HistoryLines", value); }
            }

            /// <summary>
            /// 获取或设置是否在选择过程中显示光栅图像
            /// </summary>
            public static bool ImageFrameHighlight
            {
                get { return (bool)Preferences.GetProperties("Display", "ImageFrameHighlight"); }
                set { Preferences.SetProperties("Display", "ImageFrameHighlight", value); }
            }

            /// <summary>
            /// 获取或设置是否自动为新布局创建视口
            /// </summary>
            public static bool LayoutCreateViewport
            {
                get { return (bool)Preferences.GetProperties("Display", "LayoutCreateViewport"); }
                set { Preferences.SetProperties("Display", "LayoutCreateViewport", value); }
            }

            /// <summary>
            /// 获取或设置图纸空间布局中十字光标和文字的颜色
            /// </summary>
            public static Color LayoutCrosshairColor
            {
                get { return ColorTranslator.FromOle(Convert.ToInt32(Preferences.GetProperties("Display", "LayoutCrosshairColor"))); }
                set { Preferences.SetProperties("Display", "LayoutCrosshairColor", ColorTranslator.ToOle(value)); }
            }

            /// <summary>
            /// 获取或设置是否在布局中显示页边距
            /// </summary>
            public static bool LayoutDisplayMargins
            {
                get { return (bool)Preferences.GetProperties("Display", "LayoutDisplayMargins"); }
                set { Preferences.SetProperties("Display", "LayoutDisplayMargins", value); }
            }

            /// <summary>
            /// 获取或设置是否在布局中显示图纸背景
            /// </summary>
            public static bool LayoutDisplayPaper
            {
                get { return (bool)Preferences.GetProperties("Display", "LayoutDisplayPaper"); }
                set { Preferences.SetProperties("Display", "LayoutDisplayPaper", value); }
            }

            /// <summary>
            /// 获取或设置是否在布局中显示图纸背景阴影
            /// </summary>
            public static bool LayoutDisplayPaperShadow
            {
                get { return (bool)Preferences.GetProperties("Display", "LayoutDisplayPaperShadow"); }
                set { Preferences.SetProperties("Display", "LayoutDisplayPaperShadow", value); }
            }

            /// <summary>
            /// 获取或设置是否在创建新布局时显示“打印设置”对话框
            /// </summary>
            public static bool LayoutShowPlotSetup
            {
                get { return (bool)Preferences.GetProperties("Display", "LayoutShowPlotSetup"); }
                set { Preferences.SetProperties("Display", "LayoutShowPlotSetup", value); }
            }

            /// <summary>
            /// 获取或设置在AutoCAD 启动时是否布满整个屏幕
            /// </summary>
            public static bool MaxAutoCADWindow
            {
                get { return (bool)Preferences.GetProperties("Display", "MaxAutoCADWindow"); }
                set { Preferences.SetProperties("Display", "MaxAutoCADWindow", value); }
            }

            /// <summary>
            /// 获取或设置模型空间十字光标和文字的颜色
            /// </summary>
            public static Color ModelCrosshairColor
            {
                get { return ColorTranslator.FromOle(Convert.ToInt32(Preferences.GetProperties("Display", "ModelCrosshairColor"))); }
                set { Preferences.SetProperties("Display", "ModelCrosshairColor", ColorTranslator.ToOle(value)); }
            }

            /// <summary>
            /// 获取或设置是否在实时平移和缩放时显示光栅图像
            /// </summary>
            public static bool ShowRasterImage
            {
                get { return (bool)Preferences.GetProperties("Display", "ShowRasterImage"); }
                set { Preferences.SetProperties("Display", "ShowRasterImage", value); }
            }

            /// <summary>
            /// 获取或设置文字的字号
            /// </summary>
            public static int TextFontSize
            {
                get { return (int)Preferences.GetProperties("Display", "TextFontSize"); }
                set { Preferences.SetProperties("Display", "TextFontSize", value); }
            }

            /// <summary>
            /// 获取或设置文字的字体
            /// </summary>
            public static string TextFont
            {
                get { return (string)Preferences.GetProperties("Display", "TextFont"); }
                set { Preferences.SetProperties("Display", "TextFont", value); }
            }

            /// <summary>
            /// 获取或设置文字的字体样式
            /// </summary>
            public static FontStyle TextFontStyle
            {
                get { return (FontStyle)Preferences.GetProperties("Display", "TextFontStyle"); }
                set { Preferences.SetProperties("Display", "TextFontStyle", value); }
            }

            /// <summary>
            /// 获取或设置文本窗口的背景颜色
            /// </summary>
            public static Color TextWinBackgrndColor
            {
                get { return ColorTranslator.FromOle(Convert.ToInt32(Preferences.GetProperties("Display", "TextWinBackgrndColor"))); }
                set { Preferences.SetProperties("Display", "TextWinBackgrndColor", ColorTranslator.ToOle(value)); }
            }

            /// <summary>
            /// 获取或设置文本窗口的文字颜色
            /// </summary>
            public static Color TextWinTextColor
            {
                get { return ColorTranslator.FromOle(Convert.ToInt32(Preferences.GetProperties("Display", "TextWinTextColor"))); }
                set { Preferences.SetProperties("Display", "TextWinTextColor", ColorTranslator.ToOle(value)); }
            }

            /// <summary>
            /// 获取或设置是否以真彩色显示光栅和渲染图像
            /// </summary>
            public static bool TrueColorImages
            {
                get { return (bool)Preferences.GetProperties("Display", "TrueColorImages"); }
                set { Preferences.SetProperties("Display", "TrueColorImages", value); }
            }

            /// <summary>
            /// 获取或设置照对象的褪色度百分比
            /// </summary>
            public static int XRefFadeIntensity
            {
                get { return (int)Preferences.GetProperties("Display", "XRefFadeIntensity"); }
                set { Preferences.SetProperties("Display", "XRefFadeIntensity", value); }
            }
        }

        /// <summary>
        /// “选项”对话框“文件”选项卡的所有选项
        /// </summary>
        public static class Files
        {
            /// <summary>
            /// 获取或设置AutoCAD 进程的活动配置
            /// </summary>
            public static string ActiveProfile
            {
                get { return (string)Preferences.GetProperties("Profiles", "ActiveProfile"); }
                set { Preferences.SetProperties("Profiles", "ActiveProfile", value); }
            }

            /// <summary>
            /// 当 AutoCAD 找不到源字体并且字体映射文件中未指定替换字体时所使用的字体文件的位置
            /// </summary>
            public static string AltFontFile
            {
                get { return (string)Preferences.GetProperties("Files", "AltFontFile"); }
                set { Preferences.SetProperties("Files", "AltFontFile", value); }
            }

            /// <summary>
            /// 获取或设置与标准 AutoCAD 数字化仪菜单交换的替换菜单的路径
            /// </summary>
            public static string AltTabletMenuFile
            {
                get { return (string)Preferences.GetProperties("Files", "AltTabletMenuFile"); }
                set { Preferences.SetProperties("Files", "AltTabletMenuFile", value); }
            }

            /// <summary>
            /// 获取或设置自动保存所创建文件的路径
            /// </summary>
            public static string AutoSavePath
            {
                get { return (string)Preferences.GetProperties("Files", "AutoSavePath"); }
                set { Preferences.SetProperties("Files", "AutoSavePath", value); }
            }

            /// <summary>
            /// 获取或设置配色系统路径
            /// </summary>
            public static string ColorBookPath
            {
                get { return (string)Preferences.GetProperties("Files", "ColorBookPath"); }
                set { Preferences.SetProperties("Files", "ColorBookPath", value); }
            }

            /// <summary>
            /// 获取或设置用于保存硬件设备驱动程序信息的配置文件的位置
            /// </summary>
            public static string ConfigFile
            {
                get { return (string)Preferences.GetProperties("Files", "ConfigFile"); }
            }

            /// <summary>
            /// 获取或设置自定义词典
            /// </summary>
            public static string CustomDictionary
            {
                get { return (string)Preferences.GetProperties("Files", "CustomDictionary"); }
                set { Preferences.SetProperties("Files", "CustomDictionary", value); }
            }

            /// <summary>
            /// 获取或设置默认 Internet 地址
            /// </summary>
            public static string DefaultInternetURL
            {
                get { return (string)Preferences.GetProperties("Files", "DefaultInternetURL"); }
                set { Preferences.SetProperties("Files", "DefaultInternetURL", value); }
            }

            /// <summary>
            /// 获取或设置AutoCAD 查找视频显示、定点设备、打印机和绘图仪所用 ADI 设备驱动程序的目录
            /// </summary>
            public static string DriversPath
            {
                get { return (string)Preferences.GetProperties("Files", "DriversPath"); }
                set { Preferences.SetProperties("Files", "DriversPath", value); }
            }

            /// <summary>
            /// 获取或设置用于定义当 AutoCAD 不能找到字体时替换字体的文件位置
            /// </summary>
            public static string FontFileMap
            {
                get { return (string)Preferences.GetProperties("Files", "FontFileMap"); }
                set { Preferences.SetProperties("Files", "FontFileMap", value); }
            }

            /// <summary>
            /// 获取或设置AutoCAD 帮助文件的位置
            /// </summary>
            public static string HelpFilePath
            {
                get { return (string)Preferences.GetProperties("Files", "HelpFilePath"); }
                set { Preferences.SetProperties("Files", "HelpFilePath", value); }
            }

            /// <summary>
            /// 获取或设置日志文件的位置
            /// </summary>
            public static string LogFilePath
            {
                get { return (string)Preferences.GetProperties("Files", "LogFilePath"); }
                set { Preferences.SetProperties("Files", "LogFilePath", value); }
            }

            /// <summary>
            /// 获取或设置用于拼写检查的当前词典
            /// </summary>
            public static string MainDictionary
            {
                get { return (string)Preferences.GetProperties("Files", "MainDictionary"); }
                set { Preferences.SetProperties("Files", "MainDictionary", value); }
            }

            /// <summary>
            /// 获取或设置进程的 AutoCAD 菜单或自定义文件的位置
            /// </summary>
            public static string MenuFile
            {
                get { return (string)Preferences.GetProperties("Files", "MenuFile"); }
                set { Preferences.SetProperties("Files", "MenuFile", value); }
            }

            /// <summary>
            /// 获取或设置页面设置中的替代样板文件的位置
            /// </summary>
            public static string PageSetupOverridesTemplateFile
            {
                get { return (string)Preferences.GetProperties("Files", "PageSetupOverridesTemplateFile"); }
                set { Preferences.SetProperties("Files", "PageSetupOverridesTemplateFile", value); }
            }

            /// <summary>
            /// 获取或设置打印日志文件的位置
            /// </summary>
            public static string PlotLogFilePath
            {
                get { return (string)Preferences.GetProperties("Files", "PlotLogFilePath"); }
                set { Preferences.SetProperties("Files", "PlotLogFilePath", value); }
            }

            /// <summary>
            /// 获取或设置acad.psf 文件中自定义前导部分的名称
            /// </summary>
            public static string PostScriptPrologFile
            {
                get { return (string)Preferences.GetProperties("Files", "PostScriptPrologFile"); }
                set { Preferences.SetProperties("Files", "PostScriptPrologFile", value); }
            }

            /// <summary>
            /// 获取或设置打印机配置文件的位置
            /// </summary>
            public static string PrinterConfigPath
            {
                get { return (string)Preferences.GetProperties("Files", "PrinterConfigPath"); }
                set { Preferences.SetProperties("Files", "PrinterConfigPath", value); }
            }

            /// <summary>
            /// 获取或设置打印机描述文件的位置
            /// </summary>
            public static string PrinterDescPath
            {
                get { return (string)Preferences.GetProperties("Files", "PrinterDescPath"); }
                set { Preferences.SetProperties("Files", "PrinterDescPath", value); }
            }

            /// <summary>
            /// 获取或设置打印机样式表文件的位置
            /// </summary>
            public static string PrinterStyleSheetPath
            {
                get { return (string)Preferences.GetProperties("Files", "PrinterStyleSheetPath"); }
                set { Preferences.SetProperties("Files", "PrinterStyleSheetPath", value); }
            }

            /// <summary>
            /// 获取或设置用作临时打印文件名的替换名称
            /// </summary>
            public static string PrintFile
            {
                get { return (string)Preferences.GetProperties("Files", "PrintFile"); }
                set { Preferences.SetProperties("Files", "PrintFile", value); }
            }

            /// <summary>
            /// 获取或设置后台打印文件的目录
            /// </summary>
            public static string PrintSpoolerPath
            {
                get { return (string)Preferences.GetProperties("Files", "PrintSpoolerPath"); }
                set { Preferences.SetProperties("Files", "PrintSpoolerPath", value); }
            }

            /// <summary>
            /// 获取或设置用于后台打印的应用程序
            /// </summary>
            public static string PrintSpoolExecutable
            {
                get { return (string)Preferences.GetProperties("Files", "PrintSpoolExecutable"); }
                set { Preferences.SetProperties("Files", "PrintSpoolExecutable", value); }
            }

            /// <summary>
            /// 获取或设置QNew命令的图形样板文件的位置
            /// </summary>
            public static string QNewTemplateFile
            {
                get { return (string)Preferences.GetProperties("Files", "QNewTemplateFile"); }
                set { Preferences.SetProperties("Files", "QNewTemplateFile", value); }
            }

            /// <summary>
            /// 获取或设置AutoCAD 搜索支持文件的目录
            /// </summary>
            public static string SupportPath
            {
                get { return (string)Preferences.GetProperties("Files", "SupportPath"); }
                set { Preferences.SetProperties("Files", "SupportPath", value); }
            }

            /// <summary>
            /// 获取或设置AutoCAD用于保存临时文件的目录
            /// </summary>
            public static string TempFilePath
            {
                get { return (string)Preferences.GetProperties("Files", "TempFilePath"); }
                set { Preferences.SetProperties("Files", "TempFilePath", value); }
            }

            /// <summary>
            /// 获取或设置“启动”向导所使用样板文件的路径
            /// </summary>
            public static string TemplateDWGPath
            {
                get { return (string)Preferences.GetProperties("Files", "TemplateDWGPath"); }
                set { Preferences.SetProperties("Files", "TemplateDWGPath", value); }
            }

            /// <summary>
            /// 获取或设置外部参照文件的位置
            /// </summary>
            public static string TempXRefPath
            {
                get { return (string)Preferences.GetProperties("Files", "TempXRefPath"); }
                set { Preferences.SetProperties("Files", "TempXRefPath", value); }
            }

            /// <summary>
            /// 获取或设置MTEXT 命令所使用文字编辑器的名称
            /// </summary>
            public static string TextEditor
            {
                get { return (string)Preferences.GetProperties("Files", "TextEditor"); }
                set { Preferences.SetProperties("Files", "TextEditor", value); }
            }

            /// <summary>
            /// 获取或设置AutoCAD 搜索渲染纹理贴图的目录
            /// </summary>
            public static string TextureMapPath
            {
                get { return (string)Preferences.GetProperties("Files", "TextureMapPath"); }
                set { Preferences.SetProperties("Files", "TextureMapPath", value); }
            }

            /// <summary>
            /// 获取或设置工具选项板路径
            /// </summary>
            public static string ToolPalettePath
            {
                get { return (string)Preferences.GetProperties("Files", "ToolPalettePath"); }
                set { Preferences.SetProperties("Files", "ToolPalettePath", value); }
            }

            /// <summary>
            /// 获取或设置工作空间文件的路径
            /// </summary>
            public static string WorkspacePath
            {
                get { return (string)Preferences.GetProperties("Files", "WorkspacePath"); }
                set { Preferences.SetProperties("Files", "WorkspacePath", value); }
            }

        }

        /// <summary>
        /// “选项”对话框“绘图”选项卡的所有选项
        /// </summary>
        public static class Drafting
        {
            /// <summary>
            /// 获取或设置获取自动对齐点的方式
            /// </summary>
            public static AlignmentPointAcquisition AlignmentPointAcquisition
            {
                get { return (AlignmentPointAcquisition)Preferences.GetProperties("Drafting", "AlignmentPointAcquisition"); }
                set { Preferences.SetProperties("Drafting", "AlignmentPointAcquisition", value); }
            }

            /// <summary>
            /// 获取或设置是否显示自动捕捉靶框
            /// </summary>
            public static bool AutoSnapAperture
            {
                get { return (bool)Preferences.GetProperties("Drafting", "AutoSnapAperture"); }
                set { Preferences.SetProperties("Drafting", "AutoSnapAperture", value); }
            }

            /// <summary>
            /// 获取或设置自动捕捉靶框的大小
            /// </summary>
            public static int AutoSnapApertureSize
            {
                get { return (int)Preferences.GetProperties("Drafting", "AutoSnapApertureSize"); }
                set { Preferences.SetProperties("Drafting", "AutoSnapApertureSize", value); }
            }

            /// <summary>
            /// 获取或设置是否打开自动捕捉磁吸
            /// </summary>
            public static bool AutoSnapMagnet
            {
                get { return (bool)Preferences.GetProperties("Drafting", "AutoSnapMagnet"); }
                set { Preferences.SetProperties("Drafting", "AutoSnapMagnet", value); }
            }

            /// <summary>
            /// 获取或设置是否打开自动捕捉标记
            /// </summary>
            public static bool AutoSnapMarker
            {
                get { return (bool)Preferences.GetProperties("Drafting", "AutoSnapMarker"); }
                set { Preferences.SetProperties("Drafting", "AutoSnapMarker", value); }
            }

            /// <summary>
            /// 获取或设置自动捕捉标记的颜色
            /// </summary>
            public static Color AutoSnapMarkerColor
            {
                get { return ColorTranslator.FromOle(Convert.ToInt32(Preferences.GetProperties("Drafting", "AutoSnapMarkerColor"))); }
                set { Preferences.SetProperties("Drafting", "AutoSnapMarkerColor", ColorTranslator.ToOle(value)); }
            }

            /// <summary>
            /// 获取或设置自动捕捉标记的大小
            /// </summary>
            public static int AutoSnapMarkerSize
            {
                get { return (int)Preferences.GetProperties("Drafting", "AutoSnapMarkerSize"); }
                set { Preferences.SetProperties("Drafting", "AutoSnapMarkerSize", value); }
            }

            /// <summary>
            /// 获取或设置是否打开自动捕捉工具提示
            /// </summary>
            public static bool AutoSnapToolTip
            {
                get { return (bool)Preferences.GetProperties("Drafting", "AutoSnapToolTip"); }
                set { Preferences.SetProperties("Drafting", "AutoSnapToolTip", value); }
            }

            /// <summary>
            /// 获取或设置是否打开自动追踪工具提示
            /// </summary>
            public static bool AutoTrackTooltip
            {
                get { return (bool)Preferences.GetProperties("Drafting", "AutoTrackTooltip"); }
                set { Preferences.SetProperties("Drafting", "AutoTrackTooltip", value); }
            }

            /// <summary>
            /// 获取或设置是否打开全屏追踪矢量
            /// </summary>
            public static bool FullScreenTrackingVector
            {
                get { return (bool)Preferences.GetProperties("Drafting", "FullScreenTrackingVector"); }
                set { Preferences.SetProperties("Drafting", "FullScreenTrackingVector", value); }
            }

            /// <summary>
            /// 获取或设置是否打开极轴追踪矢量
            /// </summary>
            public static bool PolarTrackingVector
            {
                get { return (bool)Preferences.GetProperties("Drafting", "PolarTrackingVector"); }
                set { Preferences.SetProperties("Drafting", "PolarTrackingVector", value); }
            }

        }

        /// <summary>
        /// “选项”对话框“打开和保存”选项卡的所有选项
        /// </summary>
        public static class OpenSave
        {
            /// <summary>
            /// 在渲染 DXFIN 或 DTBIN 交换命令后 AutoCAD 是否执行核查
            /// </summary>
            public static bool AutoAudit
            {
                get { return (bool)Preferences.GetProperties("OpenSave", "AutoAudit"); }
                set { Preferences.SetProperties("OpenSave", "AutoAudit", value); }
            }

            /// <summary>
            /// 获取或设置自动保存间隔的分钟数
            /// </summary>
            public static int AutoSaveInterval
            {
                get { return (int)Preferences.GetProperties("OpenSave", "AutoSaveInterval"); }
                set { Preferences.SetProperties("OpenSave", "AutoSaveInterval", value); }
            }

            /// <summary>
            /// 获取或设置是否使用备份文件
            /// </summary>
            public static bool CreateBackup
            {
                get { return (bool)Preferences.GetProperties("OpenSave", "CreateBackup"); }
                set { Preferences.SetProperties("OpenSave", "CreateBackup", value); }
            }

            /// <summary>
            /// 若图形包含第三方应用程序所创建的自定义对象时，是否以及何时由 AutoCAD 按需加载该应用程序
            /// </summary>
            public static ARXDemandLoad DemandLoadARXApp
            {
                get { return (ARXDemandLoad)Preferences.GetProperties("OpenSave", "DemandLoadARXApp"); }
                set { Preferences.SetProperties("OpenSave", "DemandLoadARXApp", value); }
            }

            /// <summary>
            /// 获取或设置是否每次对象被读入图形时都进行循环冗余码校验 (CRC) 
            /// </summary>
            public static bool FullCRCValidation
            {
                get { return (bool)Preferences.GetProperties("OpenSave", "FullCRCValidation"); }
                set { Preferences.SetProperties("OpenSave", "FullCRCValidation", value); }
            }

            /// <summary>
            /// 获取或设置图形文件中所允许的闲置空间的百分比 
            /// </summary>
            public static int IncrementalSavePercent
            {
                get { return (int)Preferences.GetProperties("OpenSave", "IncrementalSavePercent"); }
                set { Preferences.SetProperties("OpenSave", "IncrementalSavePercent", value); }
            }

            /// <summary>
            /// 获取或设置是否将文字窗口的内容写入日志文件 
            /// </summary>
            public static bool LogFileOn
            {
                get { return (bool)Preferences.GetProperties("OpenSave", "LogFileOn"); }
                set { Preferences.SetProperties("OpenSave", "LogFileOn", value); }
            }

            /// <summary>
            /// 获取或设置文件菜单中显示的最近使用文件的数目
            /// </summary>
            public static int MRUNumber
            {
                get { return (int)Preferences.GetProperties("OpenSave", "MRUNumber"); }
                set { Preferences.SetProperties("OpenSave", "MRUNumber", value); }
            }

            /// <summary>
            /// 获取或设置图形中由第三方应用程序所创建对象的显示
            /// </summary>
            public static ProxyImage ProxyImage
            {
                get { return (ProxyImage)Preferences.GetProperties("OpenSave", "ProxyImage"); }
                set { Preferences.SetProperties("OpenSave", "ProxyImage", value); }
            }

            /// <summary>
            /// 获取或设置图形的保存类型
            /// </summary>
            public static SaveAsType SaveAsType
            {
                get { return (SaveAsType)Preferences.GetProperties("OpenSave", "SaveAsType"); }
                set { Preferences.SetProperties("OpenSave", "SaveAsType", value); }
            }

            /// <summary>
            /// 获取或设置是否与图形一起保存 BMP 预览图像
            /// </summary>
            public static bool SavePreviewThumbnail
            {
                get { return (bool)Preferences.GetProperties("OpenSave", "SavePreviewThumbnail"); }
                set { Preferences.SetProperties("OpenSave", "SavePreviewThumbnail", value); }
            }

            /// <summary>
            /// 当用户打开包含自定义对象的图形时 AutoCAD 是否显示警告信息
            /// </summary>
            public static bool ShowProxyDialogBox
            {
                get { return (bool)Preferences.GetProperties("OpenSave", "ShowProxyDialogBox"); }
                set { Preferences.SetProperties("OpenSave", "ShowProxyDialogBox", value); }
            }

            /// <summary>
            /// 获取或设置临时文件的扩展名
            /// </summary>
            public static string TempFileExtension
            {
                get { return (string)Preferences.GetProperties("OpenSave", "TempFileExtension"); }
                set { Preferences.SetProperties("OpenSave", "TempFileExtension", value); }
            }

            /// <summary>
            /// 获取或设置外部参照对象的按需加载性能
            /// </summary>
            public static XRefDemandLoad XRefDemandLoad
            {
                get { return (XRefDemandLoad)Preferences.GetProperties("OpenSave", "XRefDemandLoad"); }
                set { Preferences.SetProperties("OpenSave", "XRefDemandLoad", value); }
            }

        }

        /// <summary>
        /// “选项”对话框“打印和发布”选项卡的所有选项
        /// </summary>
        public static class Output
        {
            /// <summary>
            /// 获取或设置是否自动生成打印日志
            /// </summary>
            public static bool AutomaticPlotLog
            {
                get { return (bool)Preferences.GetProperties("Output", "AutomaticPlotLog"); }
                set { Preferences.SetProperties("Output", "AutomaticPlotLog", value); }
            }

            /// <summary>
            /// 获取或设置是否连续生成打印日志
            /// </summary>
            public static bool ContinuousPlotLog
            {
                get { return (bool)Preferences.GetProperties("Output", "ContinuousPlotLog"); }
                set { Preferences.SetProperties("Output", "ContinuousPlotLog", value); }
            }

            /// <summary>
            /// 获取或设置布局和模型空间的默认输出设备
            /// </summary>
            public static string DefaultOutputDevice
            {
                get { return (string)Preferences.GetProperties("Output", "DefaultOutputDevice"); }
                set { Preferences.SetProperties("Output", "DefaultOutputDevice", value); }
            }

            /// <summary>
            /// 获取或设置打印到文件的缺省位置
            /// </summary>
            public static string DefaultPlotToFilePath
            {
                get { return (string)Preferences.GetProperties("Output", "DefaultPlotToFilePath"); }
                set { Preferences.SetProperties("Output", "DefaultPlotToFilePath", value); }
            }

            /// <summary>
            /// 获取或设置图形或用早期版本的 AutoCAD 创建且未用 AutoCAD 2000 格式保存的图形中的图层0的默认打印样式
            /// </summary>
            public static string DefaultPlotStyleForLayer
            {
                get { return (string)Preferences.GetProperties("Output", "DefaultPlotStyleForLayer"); }
                set { Preferences.SetProperties("Output", "DefaultPlotStyleForLayer", value); }
            }

            /// <summary>
            /// 获取或设置新创建对象的默认打印样式名称
            /// </summary>
            public static string DefaultPlotStyleForObjects
            {
                get { return (string)Preferences.GetProperties("Output", "DefaultPlotStyleForObjects"); }
                set { Preferences.SetProperties("Output", "DefaultPlotStyleForObjects", value); }
            }

            /// <summary>
            /// 获取或设置OLE对象的打印质量
            /// </summary>
            public static OleQuality OLEQuality
            {
                get { return (OleQuality)Preferences.GetProperties("Output", "OLEQuality"); }
                set { Preferences.SetProperties("Output", "OLEQuality", value); }
            }

            /// <summary>
            /// 获取或设置是否允许运行传统打印脚本
            /// </summary>
            public static bool PlotLegacy
            {
                get { return (bool)Preferences.GetProperties("Output", "PlotLegacy"); }
                set { Preferences.SetProperties("Output", "PlotLegacy", value); }
            }

            /// <summary>
            /// 获取或设置在创建新图形时对象颜色属性是否与打印样式名称关联
            /// </summary>
            public static PlotPolicy PlotPolicy
            {
                get { return (PlotPolicy)Preferences.GetProperties("Output", "PlotPolicy"); }
                set { Preferences.SetProperties("Output", "PlotPolicy", value); }
            }

            /// <summary>
            /// 获取或设置当布局配置的页面尺寸与PC3文件的默认设置不同时是否提醒用户
            /// </summary>
            public static bool PrinterPaperSizeAlert
            {
                get { return (bool)Preferences.GetProperties("Output", "PrinterPaperSizeAlert"); }
                set { Preferences.SetProperties("Output", "PrinterPaperSizeAlert", value); }
            }

            /// <summary>
            /// 获取或设置当由于I/O 端口冲突造成向设备的输出必须通过系统打印机缓冲时是否警告用户
            /// </summary>
            public static PrinterSpoolAlert PrinterSpoolAlert
            {
                get { return (PrinterSpoolAlert)Preferences.GetProperties("Output", "PrinterSpoolAlert"); }
                set { Preferences.SetProperties("Output", "PrinterSpoolAlert", value); }
            }

            /// <summary>
            /// 获取或设置是否使用最后一次成功打印时的打印设置
            /// </summary>
            public static bool UseLastPlotSettings
            {
                get { return (bool)Preferences.GetProperties("Output", "UseLastPlotSettings"); }
                set { Preferences.SetProperties("Output", "UseLastPlotSettings", value); }
            }

        }

        /// <summary>
        /// “选项”对话框“选择集”选项卡的所有选项
        /// </summary>
        public static class Selection
        {
            /// <summary>
            /// 获取或设置是否显示夹点
            /// </summary>        
            public static bool DisplayGrips
            {
                get { return (bool)Preferences.GetProperties("Selection", "DisplayGrips"); }
                set { Preferences.SetProperties("Selection", "DisplayGrips", value); }
            }

            /// <summary>
            /// 获取或设置是否在块中显示夹点
            /// </summary> 
            public static bool DisplayGripsWithinBlocks
            {
                get { return (bool)Preferences.GetProperties("Selection", "DisplayGripsWithinBlocks"); }
                set { Preferences.SetProperties("Selection", "DisplayGripsWithinBlocks", value); }
            }

            /// <summary>
            /// 获取或设置选定夹点的颜色
            /// </summary> 
            public static Color GripColorSelected
            {
                get { return ColorTranslator.FromOle(Convert.ToInt32(Preferences.GetProperties("Selection", "GripColorSelected"))); }
                set { Preferences.SetProperties("Selection", "GripColorSelected", ColorTranslator.ToOle(value)); }
            }

            /// <summary>
            /// 获取或设置未选中夹点的颜色
            /// </summary> 
            public static Color GripColorUnselected
            {
                get { return ColorTranslator.FromOle(Convert.ToInt32(Preferences.GetProperties("Selection", "GripColorUnselected"))); }
                set { Preferences.SetProperties("Selection", "GripColorUnselected", ColorTranslator.ToOle(value)); }
            }

            /// <summary>
            /// 获取或设置夹点的大小
            /// </summary> 
            public static int GripSize
            {
                get { return (int)Preferences.GetProperties("Selection", "GripSize"); }
                set { Preferences.SetProperties("Selection", "GripSize", value); }
            }

            /// <summary>
            /// 获取或设置对象是否要使用 Shift 键加入选择集
            /// </summary> 
            public static bool PickAdd
            {
                get { return (bool)Preferences.GetProperties("Selection", "PickAdd"); }
                set { Preferences.SetProperties("Selection", "PickAdd", value); }
            }

            /// <summary>
            /// 获取或设置在选择对象提示下是否自动显示选择窗口
            /// </summary> 
            public static bool PickAuto
            {
                get { return (bool)Preferences.GetProperties("Selection", "PickAuto"); }
                set { Preferences.SetProperties("Selection", "PickAuto", value); }
            }

            /// <summary>
            /// 获取或设置对象拾取框的大小
            /// </summary> 
            public static int PickBoxSize
            {
                get { return (int)Preferences.GetProperties("Selection", "PickBoxSize"); }
                set { Preferences.SetProperties("Selection", "PickBoxSize", value); }
            }

            /// <summary>
            /// 获取或设置绘制选择窗口的方式
            /// </summary> 
            public static bool PickDrag
            {
                get { return (bool)Preferences.GetProperties("Selection", "PickDrag"); }
                set { Preferences.SetProperties("Selection", "PickDrag", value); }
            }

            /// <summary>
            /// 获取或设置在发出命令之前（先选择后执行）还是之后选择对象
            /// </summary> 
            public static bool PickFirst
            {
                get { return (bool)Preferences.GetProperties("Selection", "PickFirst"); }
                set { Preferences.SetProperties("Selection", "PickFirst", value); }
            }

            /// <summary>
            /// 获取或设置是否在选择组合中的单个对象时将整个组合都选中
            /// </summary> 
            public static bool PickGroup
            {
                get { return (bool)Preferences.GetProperties("Selection", "PickGroup"); }
                set { Preferences.SetProperties("Selection", "PickGroup", value); }
            }

        }

        /// <summary>
        /// “选项”对话框“系统”选项卡的所有选项
        /// </summary>
        public static class System
        {
            /// <summary>
            /// 获取或设置AutoCAD 检测到无效输入时是否音响提示
            /// </summary>
            public static bool BeepOnError
            {
                get { return (bool)Preferences.GetProperties("System", "BeepOnError"); }
                set { Preferences.SetProperties("System", "BeepOnError", value); }
            }

            /// <summary>
            /// 获取或设置当图形中插入 OLE 对象时是否显示 OLE 缩放对话框
            /// </summary>
            public static bool DisplayOLEScale
            {
                get { return (bool)Preferences.GetProperties("System", "DisplayOLEScale"); }
                set { Preferences.SetProperties("System", "DisplayOLEScale", value); }
            }

            /// <summary>
            /// 获取或设置当 AutoCAD 启动时或新图创建时是否显示开始对话框
            /// </summary>
            public static bool EnableStartupDialog
            {
                get { return (bool)Preferences.GetProperties("System", "EnableStartupDialog"); }
                set { Preferences.SetProperties("System", "EnableStartupDialog", value); }
            }

            /// <summary>
            /// 获取或设置acad.lsp 是在启动时加载还是每一图形均加载
            /// </summary>
            public static bool LoadAcadLspInAllDocuments
            {
                get { return (bool)Preferences.GetProperties("System", "LoadAcadLspInAllDocuments"); }
                set { Preferences.SetProperties("System", "LoadAcadLspInAllDocuments", value); }
            }

            /// <summary>
            /// 获取或设置是否隐藏消息设置
            /// </summary>
            public static bool ShowWarningMessages
            {
                get { return (bool)Preferences.GetProperties("System", "ShowWarningMessages"); }
                set { Preferences.SetProperties("System", "ShowWarningMessages", value); }
            }

            /// <summary>
            /// 获取或设置是否在图形文件中保存链接索引
            /// </summary>
            public static bool StoreSQLIndex
            {
                get { return (bool)Preferences.GetProperties("System", "StoreSQLIndex"); }
                set { Preferences.SetProperties("System", "StoreSQLIndex", value); }
            }

            /// <summary>
            /// 获取或设置是否以只读模式打开表格
            /// </summary>
            public static bool TablesReadOnly
            {
                get { return (bool)Preferences.GetProperties("System", "TablesReadOnly"); }
                set { Preferences.SetProperties("System", "TablesReadOnly", value); }
            }

        }

        /// <summary>
        /// “选项”对话框“用户系统配置”选项卡的所有选项
        /// </summary>
        public static class User
        {
            /// <summary>
            /// 获取或设置在AutoCAD设计中心源图形未分配插入单位的对象自动使用的单位
            /// </summary>
            public static InsertUnits ADCInsertUnitsDefaultSource
            {
                get { return (InsertUnits)Preferences.GetProperties("User", "ADCInsertUnitsDefaultSource"); }
                set { Preferences.SetProperties("User", "ADCInsertUnitsDefaultSource", value); }
            }

            /// <summary>
            /// 获取或设置在AutoCAD设计中心目标图形未分配插入单位的对象自动使用的单位
            /// </summary>
            public static InsertUnits ADCInsertUnitsDefaultTarget
            {
                get { return (InsertUnits)Preferences.GetProperties("User", "ADCInsertUnitsDefaultTarget"); }
                set { Preferences.SetProperties("User", "ADCInsertUnitsDefaultTarget", value); }
            }

            /// <summary>
            /// 获取或设置是否显示超链接光标和快捷菜单
            /// </summary>
            public static bool HyperlinkDisplayCursor
            {
                get { return (bool)Preferences.GetProperties("User", "HyperlinkDisplayCursor"); }
                set { Preferences.SetProperties("User", "HyperlinkDisplayCursor", value); }
            }

            /// <summary>
            /// 获取或设置是否显示超链接工具提示
            /// </summary>
            public static bool HyperlinkDisplayTooltip
            {
                get { return (bool)Preferences.GetProperties("User", "HyperlinkDisplayTooltip"); }
                set { Preferences.SetProperties("User", "HyperlinkDisplayTooltip", value); }
            }

            /// <summary>
            /// 获取或设置Windows 标准或 AutoCAD 传统快捷键
            /// </summary>
            public static KeyboardAccelerator KeyboardAccelerator
            {
                get { return (KeyboardAccelerator)Preferences.GetProperties("User", "KeyboardAccelerator"); }
                set { Preferences.SetProperties("User", "KeyboardAccelerator", value); }
            }

            /// <summary>
            /// 获取或设置AutoCAD 如何响应坐标数据的输入
            /// </summary>
            public static KeyboardPriority KeyboardPriority
            {
                get { return (KeyboardPriority)Preferences.GetProperties("User", "KeyboardPriority"); }
                set { Preferences.SetProperties("User", "KeyboardPriority", value); }
            }

            /// <summary>
            /// 获取或设置处于命令模式 (有一个命令正在执行) 时绘图区域中右键单击的功能
            /// </summary>
            public static DrawingAreaSCMCommand SCMCommandMode
            {
                get { return (DrawingAreaSCMCommand)Preferences.GetProperties("User", "SCMCommandMode"); }
                set { Preferences.SetProperties("User", "SCMCommandMode", value); }
            }

            /// <summary>
            /// 获取或设置处于默认模式 (未选中对象且无命令在执行) 时绘图区域中右键单击的功能
            /// </summary>
            public static DrawingAreaSCMDefault SCMDefaultMode
            {
                get { return (DrawingAreaSCMDefault)Preferences.GetProperties("User", "SCMDefaultMode"); }
                set { Preferences.SetProperties("User", "SCMDefaultMode", value); }
            }

            /// <summary>
            /// 获取或设置处于编辑模式 (选中一个或多个对象且无命令在执行) 时绘图区域中右键单击的功能
            /// </summary>
            public static DrawingAreaSCMEdit SCMEditMode
            {
                get { return (DrawingAreaSCMEdit)Preferences.GetProperties("User", "SCMEditMode"); }
                set { Preferences.SetProperties("User", "SCMEditMode", value); }
            }

            /// <summary>
            /// 获取或设置是否开启计时右键动作
            /// </summary>
            public static bool SCMTimeMode
            {
                get { return (bool)Preferences.GetProperties("User", "SCMTimeMode"); }
                set { Preferences.SetProperties("User", "SCMTimeMode", value); }
            }

            /// <summary>
            /// 获取或设置设置按持续时间显示快捷菜单来控制计时右键动作
            /// </summary>
            public static int SCMTimeValue
            {
                get { return (int)Preferences.GetProperties("User", "SCMTimeValue"); }
                set { Preferences.SetProperties("User", "SCMTimeValue", value); }
            }

            /// <summary>
            /// 获取或设置在图形区域右键点击时显示快捷菜单还是执行回车
            /// </summary>
            public static bool ShortCutMenuDisplay
            {
                get { return (bool)Preferences.GetProperties("User", "ShortCutMenuDisplay"); }
                set { Preferences.SetProperties("User", "ShortCutMenuDisplay", value); }
            }

        }
    }
}








