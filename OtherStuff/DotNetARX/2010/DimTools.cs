using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace DotNetARX
{
    /// <summary>
    /// 与公差标注有关的特殊格式代码
    /// </summary>
    public struct DimFormatCode
    {
        /// <summary>
        /// 倾斜度（∠）
        /// </summary>
        public static readonly string Angular=@"{\Fgdt;" + "a}";
        /// <summary>
        /// 垂直度（⊥）
        /// </summary>
        public static readonly string VerticalLine=@"{\Fgdt;" + "b}";
        /// <summary>
        /// 平面度（◇）
        /// </summary>
        public static readonly string Flatness=@"{\Fgdt;" + "c}";
        /// <summary>
        /// 面轮廓度（封闭的半圆）
        /// </summary>
        public static readonly string HalfCircleClosed=@"{\Fgdt;" + "d}";
        /// <summary>
        /// 圆度（○）
        /// </summary>
        public static readonly string SingleCircle=@"{\Fgdt;" + "e}";
        /// <summary>
        /// 平行度（∥）
        /// </summary>
        public static readonly string Parallel=@"{\Fgdt;" + "f}";
        /// <summary>
        /// 圆柱度
        /// </summary>
        public static readonly string Cylindricity=@"{\Fgdt;" + "g}";
        /// <summary>
        /// 圆跳动度（↗）
        /// </summary>
        public static readonly string CircularRunout=@"{\Fgdt;" + "h}";
        /// <summary>
        /// 对称度
        /// </summary>
        public static readonly string SymmetrySymbol=@"{\Fgdt;" + "i}";
        /// <summary>
        /// 位置度（⊕）
        /// </summary>
        public static readonly string Position=@"{\Fgdt;" + "j}";
        /// <summary>
        /// 线轮廓度（⌒）
        /// </summary>
        public static readonly string HalfCircle=@"{\Fgdt;" + "k}";
        /// <summary>
        /// 最小实体要求（带圆圈的L）
        /// </summary>
        public static readonly string CircleL=@"{\Fgdt;" + "l}";
        /// <summary>
        /// 最大实体要求（带圆圈的M）
        /// </summary>
        public static readonly string CircleM=@"{\Fgdt;" + "m}";
        /// <summary>
        /// 公差直径（φ）
        /// </summary>
        public static readonly string Diameter=@"{\Fgdt;" + "n}";
        /// <summary>
        /// 正方形（□）
        /// </summary>
        public static readonly string Square=@"{\Fgdt;" + "o}";
        /// <summary>
        /// 延伸公差带（带圆圈的P）
        /// </summary>
        public static readonly string CircleP=@"{\Fgdt;" + "p}";
        /// <summary>
        /// 中心线（C和L重叠）
        /// </summary>
        public static readonly string CenterLine=@"{\Fgdt;" + "q}";
        /// <summary>
        /// 同轴度（◎）
        /// </summary>
        public static readonly string TwoCircles=@"{\Fgdt;" + "r}";
        /// <summary>
        /// 不考虑特征尺寸（带圆圈的S）
        /// </summary>
        public static readonly string CircleS=@"{\Fgdt;" + "s}";
        /// <summary>
        /// 全跳动度（符号为两带箭头的斜线）
        /// </summary>
        public static readonly string TotalRunout=@"{\Fgdt;" + "t}";
        /// <summary>
        /// 直线度（―）
        /// </summary>
        public static readonly string Line=@"{\Fgdt;" + "u}";
        /// <summary>
        /// 机械制图中的柱形沉孔和锪平面孔
        /// </summary>
        public static readonly string CounterBore=@"{\Fgdt;" + "v}";
        /// <summary>
        /// 机械制图中的埋头孔（∨）
        /// </summary>
        public static readonly string CounterSink=@"{\Fgdt;" + "w}";
        /// <summary>
        /// 机械制图中的沉孔深度
        /// </summary>
        public static readonly string Depth=@"{\Fgdt;" + "x}";
        /// <summary>
        /// 锥形接续器符号
        /// </summary>
        public static readonly string ConicalTaper=@"{\Fgdt;" + "y}";
        /// <summary>
        /// 机械制图中的锥度（⊿）
        /// </summary>
        public static readonly string Slope=@"{\Fgdt;" + "z}";
    }

    /// <summary>
    /// 标注中的箭头符号
    /// </summary>
    public struct DimArrowBlock
    {
        /// <summary>
        /// 实心闭合
        /// </summary>
        public static readonly string ClosedFilled="";
        /// <summary>
        /// 点
        /// </summary>
        public static readonly string Dot="_DOT";
        /// <summary>
        /// 小点
        /// </summary>
        public static readonly string DotSmall="_DOTSMALL";
        /// <summary>
        /// 空心点
        /// </summary>
        public static readonly string DotBlank="_DOTBLANK";
        /// <summary>
        /// 原点标记
        /// </summary>
        public static readonly string Origin="_ORIGIN";
        /// <summary>
        /// 原点标记2
        /// </summary>
        public static readonly string Origin2="_ORIGIN2";
        /// <summary>
        /// 打开
        /// </summary>
        public static readonly string Open="_OPEN";
        /// <summary>
        /// 直角
        /// </summary>
        public static readonly string RightAngle="_OPEN90";
        /// <summary>
        /// 30度角
        /// </summary>
        public static readonly string Angle30="_OPEN30";
        /// <summary>
        /// 闭合
        /// </summary>
        public static readonly string Closed="_CLOSED";
        /// <summary>
        /// 空心小点
        /// </summary>
        public static readonly string DotSmallBlank="_SMALL";
        /// <summary>
        /// 无
        /// </summary>
        public static readonly string None="_NONE";
        /// <summary>
        /// 倾斜
        /// </summary>
        public static readonly string Oblique="_OBLIQUE";
        /// <summary>
        /// 实心框
        /// </summary>
        public static readonly string BoxFilled="_BOXFILLED";
        /// <summary>
        /// 框
        /// </summary>
        public static readonly string Box="_BOXBLANK";
        /// <summary>
        /// 空心闭合
        /// </summary>
        public static readonly string ClosedBlank="_CLOSEDBLANK";
        /// <summary>
        /// 实心基准三角形
        /// </summary>
        public static readonly string TriangleFilled="_DATUMFILLED";
        /// <summary>
        /// 基准三角形
        /// </summary>
        public static readonly string Triangle="_DATUMBLANK";
        /// <summary>
        /// 积分
        /// </summary>
        public static readonly string Integral="_INTEGRAL";
        /// <summary>
        /// 建筑标记
        /// </summary>
        public static readonly string ArchitecturalTick="_ARCHTICK";
    }
    /// <summary>
    /// AutoCAD自带的用于多重引线注释块的名称
    /// </summary>
    public struct LeaderBlockContent
    {
        /// <summary>
        /// 详细信息标注
        /// </summary>
        public static readonly string Detail="_DetailCallout";
        /// <summary>
        /// 方框（□）
        /// </summary>
        public static readonly string Box="_TagBox";
        /// <summary>
        /// 圆（○）
        /// </summary>
        public static readonly string Circle="_TagCircle";
        /// <summary>
        /// 正六边形
        /// </summary>
        public static readonly string Hexagon="_TagHexagon";
        /// <summary>
        /// 槽
        /// </summary>
        public static readonly string Slot="_TagSlot";
        /// <summary>
        /// 三角形（△）
        /// </summary>
        public static readonly string Triangle="_TagTriangle";
    }
    /// <summary>
    /// 标注操作类（含引线、形位公差）
    /// </summary>
    public static class DimTools
    {
        #region 标注系统变量
        /// <summary>
        /// 当前尺寸线箭头符号
        /// </summary>
        public static string ArrowBlock
        {
            //获取DIMBLK系统变量值，它表示尺寸线末端显示的箭头块
            get { return Application.GetSystemVariable("DIMBLK").ToString(); }
            set
            {
                //设置DIMBLK系统变量值
                Application.SetSystemVariable("DIMBLK", value);
            }
        }
        #endregion

        /// <summary>
        /// 获取与名称对应的箭头块的ObjectId
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="arrowName">箭头名</param>
        /// <returns>返回指定箭头块的ObjectId</returns>
        public static ObjectId GetArrowObjectId(this Database db, string arrowName)
        {
            ObjectId arrId=ObjectId.Null;//存储箭头符号的ObjectId
            using (Transaction trans=db.TransactionManager.StartTransaction())
            {
                BlockTable bt=(BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                //如果图形中存在指定名称的箭头块，则获取其ObjectId
                if (bt.Has(arrowName)) arrId = bt[arrowName];
                trans.Commit();
            }
            return arrId;//返回箭头块的ObjectId
        }
        /// <summary>
        /// 设置形位公差值
        /// </summary>
        /// <param name="frame">形位公差特征控制框对象</param>
        /// <param name="geometricSym">几何特征符号，表示公差的几何特征，例如位置、轮廓、形状、方向或跳动</param>
        /// <param name="torlerance">公差值</param>
        /// <param name="firstDatum">第一级基准要素</param>
        /// <param name="secondDatum">第二级基准要素</param>
        /// <param name="thirdDatum">第三级基准要素</param>
        public static void CreateTolerance(this FeatureControlFrame frame, string geometricSym, string torlerance, string firstDatum, string secondDatum, string thirdDatum)
        {
            if (frame == null) return;//特征控制框对象必须已定义，否则返回
            //设置形位公差值，各组成部分用竖线（%%v）分隔
            frame.Text = geometricSym + "%%v" + torlerance + "%%v" + firstDatum + "%%v" + secondDatum + "%%v" + thirdDatum;
        }
    }
}
