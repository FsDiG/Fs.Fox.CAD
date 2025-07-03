using System;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.DatabaseServices;
namespace DotNetARX
{
    /// <summary>
    /// 多行文字堆叠方式
    /// </summary>
    public enum StackType
    {
        /// <summary>
        /// 水平分数(/)
        /// </summary>
        HorizontalFraction,
        /// <summary>
        /// 斜分数(#)
        /// </summary>
        ItalicFraction,
        /// <summary>
        /// 公差(^)
        /// </summary>
        Tolerance
    }
    /// <summary>
    /// 特殊字符
    /// </summary>
    public struct TextSpecialSymbol
    {
        #region 特殊字符
        /// <summary>
        /// 度符号(°)
        /// </summary>
        public static readonly string Degree=@"\U+00B0";
        /// <summary>
        /// 公差符号(±)
        /// </summary>
        public static readonly string Tolerance=@"\U+00B1";
        /// <summary>
        /// 直径符号(φ)
        /// </summary>
        public static readonly string Diameter=@"\U+2205";
        /// <summary>
        /// 几乎相等(≈)
        /// </summary>
        public static readonly string AlmostEqual=@"\U+2248";
        /// <summary>
        /// 角度(∠)
        /// </summary>
        public static readonly string Angle=@"\U+2220";
        /// <summary>
        /// 边界线
        /// </summary>
        public static readonly string LineBoundary=@"\U+E100";
        /// <summary>
        /// 中心线
        /// </summary>
        public static readonly string LineCenter=@"\U+2104";
        /// <summary>
        /// 增量(Δ)
        /// </summary>
        public static readonly string Delta=@"\U+0394";
        /// <summary>
        /// 电相位(φ)
        /// </summary>
        public static readonly string ElectricalPhase=@"\U+0278";
        /// <summary>
        /// 流线
        /// </summary>
        public static readonly string LineFlow=@"\U+E101";
        /// <summary>
        /// 标识
        /// </summary>
        public static readonly string Identity=@"\U+2261";
        /// <summary>
        /// 初始长度
        /// </summary>
        public static readonly string InitialLength=@"\U+E200";
        /// <summary>
        /// 界碑线
        /// </summary>
        public static readonly string LineMonument=@"\U+E102";
        /// <summary>
        /// 不相等(≠)
        /// </summary>
        public static readonly string Notequal=@"\U+2260";
        /// <summary>
        /// 欧姆
        /// </summary>
        public static readonly string Ohm=@"\U+2126";
        /// <summary>
        /// 欧米加(Ω)
        /// </summary>
        public static readonly string Omega=@"\U+03A9";
        /// <summary>
        /// 地界线
        /// </summary>
        public static readonly string LinePlate=@"\U+214A";
        /// <summary>
        /// 下标2
        /// </summary>
        public static readonly string Subscript2=@"\U+2082";
        /// <summary>
        /// 平方
        /// </summary>
        public static readonly string Square=@"\U+00B2";
        /// <summary>
        /// 立方
        /// </summary>
        public static readonly string Cube=@"\U+00B3";
        #endregion
        #region 单行文字上下划线
        /// <summary>
        /// 单行文字上划线
        /// </summary>
        public static readonly string Overline=@"%%o";
        /// <summary>
        /// 单行文字下划线
        /// </summary>
        public static readonly string Underline=@"%%u";
        #endregion
        #region 希腊字母
        /// <summary>
        /// α
        /// </summary>
        public static readonly string Alpha=@"\U+03B1";
        /// <summary>
        /// β
        /// </summary>
        public static readonly string Belta=@"\U+03B2";
        /// <summary>
        /// γ 
        /// </summary>
        public static readonly string Gamma=@"\U+03B3";
        #endregion
        #region 钢筋符号
        /// <summary>
        /// 一级钢筋符号
        /// </summary>
        public static readonly string SteelBar1=@"\U+0082";
        /// <summary>
        /// 二级钢筋符号
        /// </summary>
        public static readonly string SteelBar2=@"\U+0083";
        /// <summary>
        /// 三级钢筋符号
        /// </summary>
        public static readonly string SteelBar3=@"\U+0084";
        /// <summary>
        /// 四级钢筋符号
        /// </summary>
        public static readonly string SteelBar4=@"\U+0085";
        #endregion
    }
    /// <summary>
    /// 文字操作类
    /// </summary>
    public static class TextTools
    {
        /// <summary>
        /// 获取多行文字的真实内容
        /// </summary>
        /// <param name="mtext">多行文字对象</param>
        /// <returns>返回多行文字的真实内容</returns>
        public static string GetText(this MText mtext)
        {
            string content=mtext.Contents;//多行文本内容
            //将多行文本按“\\”进行分割
            string[] strs=content.Split(new string[] { @"\\" }, StringSplitOptions.None);
            //指定不区分大小写
            RegexOptions ignoreCase=RegexOptions.IgnoreCase;
            for (int i = 0; i < strs.Length; i++)
            {
                //删除段落缩进格式
                strs[i] = Regex.Replace(strs[i], @"\\pi(.[^;]*);", "", ignoreCase);
                //删除制表符格式
                strs[i] = Regex.Replace(strs[i], @"\\pt(.[^;]*);", "", ignoreCase);
                //删除堆迭格式
                strs[i] = Regex.Replace(strs[i], @"\\S(.[^;]*)(\^|#|\\)(.[^;]*);", @"$1$3", ignoreCase);
                strs[i] = Regex.Replace(strs[i], @"\\S(.[^;]*)(\^|#|\\);", "$1", ignoreCase);
                //删除字体、颜色、字高、字距、倾斜、字宽、对齐格式
                strs[i] = Regex.Replace(strs[i], @"(\\F|\\C|\\H|\\T|\\Q|\\W|\\A)(.[^;]*);", "", ignoreCase);
                //删除下划线、删除线格式
                strs[i] = Regex.Replace(strs[i], @"(\\L|\\O|\\l|\\o)", "", ignoreCase);
                //删除不间断空格格式
                strs[i] = Regex.Replace(strs[i], @"\\~", "", ignoreCase);
                //删除换行符格式
                strs[i] = Regex.Replace(strs[i], @"\\P", "\n", ignoreCase);
                //删除换行符格式(针对Shift+Enter格式)
                //strs[i] = Regex.Replace(strs[i], "\n", "", ignoreCase);
                //删除{}
                strs[i] = Regex.Replace(strs[i], @"({|})", "", ignoreCase);
                //替换回\\,\{,\}字符
                //strs[i] = Regex.Replace(strs[i], @"\x01", @"\", ignoreCase);
                //strs[i] = Regex.Replace(strs[i], @"\x02", @"{", ignoreCase);
                //strs[i] = Regex.Replace(strs[i], @"\x03", @"}", ignoreCase);
            }
            return string.Join("\\", strs);//将文本中的特殊字符去掉后重新连接成一个字符串
        }
        /// <summary>
        /// 堆叠多行文字
        /// </summary>
        /// <param name="text">堆叠分数前的文字</param>
        /// <param name="supText">堆叠分数的分子</param>
        /// <param name="subText">堆叠分数的分母</param>
        /// <param name="stackType">堆叠类型</param>
        /// <param name="scale">堆叠文字的缩放比例</param>
        /// <returns>返回堆叠好的文字</returns>
        public static string StackText(string text, string supText, string subText, StackType stackType, double scale)
        {
            //设置堆叠方式所代表的字符，用于将StackType枚举转换为对应的字符
            string[] strs=new string[] { "/", "#", "^" };
            //设置堆叠文字
            return string.Format(
                    "\\A1;{0}{1}\\H{2:0.#}x;\\S{3}{4}{5};{6}",
                    text, "{", scale, supText, strs[(int)stackType],
                    subText, "}");
        }

        /// <summary>
        /// 设置单行文本的属性为当前文字样式的属性
        /// </summary>
        /// <param name="txt">单行文本对象</param>
        public static void SetFromTextStyle(this DBText txt)
        {
            //打开文字样式表记录
            TextStyleTableRecord str=(TextStyleTableRecord)txt.TextStyleId.GetObject(OpenMode.ForRead);
            //必须保证文字为写的状态
            if (!txt.IsWriteEnabled)
                txt.UpgradeOpen();
            txt.Oblique = str.ObliquingAngle;//设置倾斜角(弧度)
            txt.Annotative = str.Annotative;//设置文字的注释性
            //文字方向与布局是否匹配
            txt.SetPaperOrientation(Convert.ToBoolean(str.PaperOrientation));
            txt.WidthFactor = str.XScale;//设置宽度比例
            txt.Height = str.TextSize;//设置高度
            if (str.FlagBits == 2)
            {
                txt.IsMirroredInX = true;//颠倒
            }
            else if (str.FlagBits == 4)
            {
                txt.IsMirroredInY = true;//反向
            }
            else if (str.FlagBits == 6)//颠倒并反向
            {
                txt.IsMirroredInX = txt.IsMirroredInY = true;
            }
            txt.DowngradeOpen();//为了安全切换为读的状态
        }

        /// <summary>
        /// 设置多行文本的属性为当前文字样式的属性
        /// </summary>
        /// <param name="mtxt">多行文本对象</param>
        public static void SetFromTextStyle(this MText mtxt)
        {
            Database db=mtxt.Database;
            var trans=db.TransactionManager;
            TextStyleTableRecord str=(TextStyleTableRecord)trans.GetObject(mtxt.TextStyleId, OpenMode.ForRead);
            if (!mtxt.IsWriteEnabled)
                mtxt.UpgradeOpen();
            mtxt.Rotation = str.ObliquingAngle;
            mtxt.Annotative = str.Annotative;
            mtxt.SetPaperOrientation(Convert.ToBoolean(str.PaperOrientation));
            mtxt.LineSpacingFactor = str.XScale;
            mtxt.TextHeight = str.TextSize;
        }
    }
}
