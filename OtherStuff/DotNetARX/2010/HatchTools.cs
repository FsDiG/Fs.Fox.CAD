using System;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using System.Security;

namespace DotNetARX
{
    /// <summary>
    /// 渐变色填充的图案名称
    /// </summary>
    public enum HatchGradientName
    {
        /// <summary>
        /// 线状渐变
        /// </summary>
        Linear,
        /// <summary>
        /// 圆柱状渐变
        /// </summary>
        Cylinder,
        /// <summary>
        /// 反圆柱状渐变
        /// </summary>
        Invcylinder,
        /// <summary>
        /// 球状渐变
        /// </summary>
        Spherical,
        /// <summary>
        /// 反球状渐变
        /// </summary>
        Invspherical,
        /// <summary>
        /// 半球状渐变
        /// </summary>
        Hemisperical,
        /// <summary>
        /// 反半球状渐变
        /// </summary>
        InvHemisperical,
        /// <summary>
        /// 抛物面状渐变
        /// </summary>
        Curved,
        /// <summary>
        /// 反抛物面状渐变
        /// </summary>
        Incurved
    }

    /// <summary>
    /// 填充图案选项板类
    /// </summary>
    public class HatchPalletteDialog
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport("acad.exe", EntryPoint = "?acedHatchPalletteDialog@@YA_NPB_W_NAAPA_W@Z", CharSet = CharSet.Auto)]
        static extern bool acedHatchPalletteDialog(string currentPattern, bool showCustom, out IntPtr newPattern);
        private string pattern;//用于存储用户选择的填充图案名称
        /// <summary>
        /// 是否显示自定义标签
        /// </summary>
        public bool ShowCustom { get; set; }
        /// <summary>
        /// 获取用户选择的填充图案名称
        /// </summary>
        /// <returns>返回填充图案名称</returns>
        public string GetPattern()
        {
            return pattern;
        }
        /// <summary>
        /// 构造函数，在填充图案选项板中显示自定义标签
        /// </summary>
        public HatchPalletteDialog()
        {
            ShowCustom = true;//显示自定义标签
        }
        /// <summary>
        /// 显示填充图案选项板
        /// </summary>
        /// <returns>如果用户选择了填充图案，则返回true，否则返回false</returns>
        public bool ShowDialog()
        {
            IntPtr ptr;//用户选择的
            //显示填充图案选项板
            bool isOK=acedHatchPalletteDialog(HatchTools.CurrentPattern, ShowCustom, out ptr);
            if (!isOK) return false;//如果用户未选择填充图案，返回false
            //用户选择了填充图案，通过指针获得图案名称并将其置为当前名称
            pattern = HatchTools.CurrentPattern = Marshal.PtrToStringAuto(ptr);
            return true;
        }
    }
    /// <summary>
    /// 填充操作类
    /// </summary>
    public static class HatchTools
    {
        /// <summary>
        /// 设置或读取默认填充图案，其名称最多可包含 34 个字符，其中不能有空格。
        /// </summary>
        public static string CurrentPattern
        {
            //获取HPNAME系统变量值，它表示默认的填充图案名
            get { return Application.GetSystemVariable("HPNAME").ToString(); }
            set
            {
                //如果要设置的值符合填充图案名，则设置HPNAME系统变量值
                if (value.Length <= 34 && !value.Contains(" ") && !value.IsNullOrWhiteSpace() && value != CurrentPattern)
                    Application.SetSystemVariable("HPNAME", value);
            }
        }

        /// <summary>
        /// 创建图案填充
        /// </summary>
        /// <param name="hatch">填充对象</param>
        /// <param name="patternType">填充图案类型</param>
        /// <param name="patternName">填充图案名</param>
        /// <param name="associative">填充是否与边界关联</param>
        public static void CreateHatch(this Hatch hatch, HatchPatternType patternType, string patternName, bool associative)
        {
            Database db=HostApplicationServices.WorkingDatabase;
            hatch.SetDatabaseDefaults();//设置填充的默认值
            //设置填充的类型和填充图案名
            hatch.SetHatchPattern(patternType, patternName);
            db.AddToModelSpace(hatch);//将填充添加到模型空间
            //设置填充与边界是否关联
            hatch.Associative = associative ? true : false;
        }

        /// <summary>
        /// 创建渐变色填充
        /// </summary>
        /// <param name="hatch">填充对象</param>
        /// <param name="gradientName">渐变色填充的图案名称</param>
        /// <param name="color1">渐变色填充的起始颜色</param>
        /// <param name="color2">渐变色填充的结束颜色</param>
        /// <param name="associative">填充是否与边界关联</param>
        public static void CreateGradientHatch(this Hatch hatch, HatchGradientName gradientName, Color color1, Color color2, bool associative)
        {
            //设置渐变色填充的类型所代表的字符串，用于将HatchGradientName枚举转换为对应的字符串
            string[] gradientNames=new string[] { "Linear","Cylinder","Invcylinder","Spherical",
                                            "Invspherical","Hemisperical","InvHemisperical","Curved","Incurved"};
            Database db=HostApplicationServices.WorkingDatabase;
            hatch.SetDatabaseDefaults();//设置填充的默认值
            //设置填充的类型为渐变色填充
            hatch.HatchObjectType = HatchObjectType.GradientObject;
            //设置渐变色填充的类型和图案名称
            hatch.SetGradient(GradientPatternType.PreDefinedGradient, gradientNames[(int)gradientName]);
            //新建两个Color类对象，分别表示渐变色填充的起始和结束颜色
            GradientColor gColor1=new GradientColor(color1, 0);
            GradientColor gColor2=new GradientColor(color2, 1);
            //设置渐变色填充的起始和结束颜色
            hatch.SetGradientColors(new GradientColor[] { gColor1, gColor2 });
            db.AddToModelSpace(hatch);//将填充添加到模型空间
            //设置填充与边界是否关联
            hatch.Associative = associative ? true : false;
        }
    }
}
