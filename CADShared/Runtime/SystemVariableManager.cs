using Autodesk.AutoCAD.Internal.DatabaseServices;

namespace IFoxCAD.Cad;

/// <summary>
/// 系统变量管理器
/// </summary>
public class SystemVariableManager
{
    #region A

    /// <summary>
    /// 打开或关闭自动捕捉靶框的显示
    /// </summary>
    public bool ApBox
    {
        get => Convert.ToBoolean(Acap.GetSystemVariable(nameof(ApBox)));
        set => Acap.SetSystemVariable(nameof(ApBox), Convert.ToInt32(value));
    }

    /// <summary>
    /// 对象捕捉靶框的大小，范围[1,50]
    /// </summary>
    public int Aperture
    {
        get => Convert.ToInt32(Acap.GetSystemVariable(nameof(Aperture)));
        set => Acap.SetSystemVariable(nameof(Aperture), value);
    }

    /// <summary>
    /// 图形单位-角度-类型，范围[0-十进制度数,1-度/分/秒,2-百分度,3-弧度,4-勘测单位]
    /// </summary>
    public int Aunits
    {
        get => Convert.ToInt32(Acap.GetSystemVariable(nameof(Aunits)));
        set => Acap.SetSystemVariable(nameof(Aunits), value);
    }

    /// <summary>
    /// 图形单位-角度-精度，范围<c>[0,8]</c>
    /// </summary>
    public int Auprec
    {
        get => Convert.ToInt32(Acap.GetSystemVariable(nameof(Auprec)));
        set => Acap.SetSystemVariable(nameof(Auprec), value);
    }

    #endregion

    #region C

    /// <summary>
    /// 存在活动命令
    /// </summary>
    public static bool CmdActive => Convert.ToBoolean(Acap.GetSystemVariable(nameof(CmdActive)));

    /// <summary>
    /// 当前的命令
    /// </summary>
    public static string CmdNames => Acap.GetSystemVariable(nameof(CmdNames)).ToString();


    public static short CVPort
    {
        get => Convert.ToInt16(Acap.GetSystemVariable(nameof(CVPort)));
        set => Acap.SetSystemVariable(nameof(CVPort), value);
    }

    #endregion

    #region D

    /// <summary>
    /// 是否开启双击
    /// </summary>
    public static bool DblClick
    {
        get => Convert.ToBoolean(Acap.GetSystemVariable(nameof(DblClick)));
        set => Acap.SetSystemVariable(nameof(DblClick), Convert.ToInt32(value));
    }

    public static int DynMode
    {
        get => Convert.ToInt32(Acap.GetSystemVariable(nameof(DynMode)));
        set => Acap.SetSystemVariable(nameof(DynMode), value);
    }

    public static bool DynPrompt
    {
        get => Convert.ToBoolean(Acap.GetSystemVariable(nameof(DynPrompt)));
        set => Acap.SetSystemVariable(nameof(DynPrompt), Convert.ToInt32(value));
    }

    #endregion

    #region G

    /// <summary>
    /// 显示图形栅格
    /// </summary>
    public bool GridMode
    {
        get => Acap.GetSystemVariable(nameof(GridMode)) is 1;
        set => Acap.SetSystemVariable(nameof(GridMode), Convert.ToInt32(value));
    }

    #endregion

    #region H

    /// <summary>
    /// 填充比例
    /// </summary>
    public static double HPScale
    {
        get => Convert.ToDouble(Acap.GetSystemVariable(nameof(HPScale)));
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(HPScale), "HPScale必须大于0");
            Acap.SetSystemVariable(nameof(HPScale), value);
        }
    }

    #endregion

    #region I

    /// <summary>
    /// 图形单位-插入时的缩放单位
    /// </summary>
    public UnitsValue Insunits
    {
        get => (UnitsValue)Acap.GetSystemVariable(nameof(Insunits));
        set => Acap.SetSystemVariable(nameof(Insunits), (int)value);
    }

    #endregion

    #region L

    /// <summary>
    /// 储存所输入相对于当前用户坐标系统(UCS)的最后点的值
    /// </summary>
    public Point3d LastPoint
    {
        get => (Point3d)Acap.GetSystemVariable(nameof(LastPoint));
        set => Acap.SetSystemVariable(nameof(LastPoint), value);
    }

    /// <summary>
    /// 图形单位-长度-类型，范围[1-科学,2-小数,3-工程,4-建筑,5-分数]
    /// </summary>
    public int Lunits
    {
        get => Convert.ToInt32(Acap.GetSystemVariable(nameof(Lunits)));
        set => Acap.SetSystemVariable(nameof(Lunits), value);
    }

    /// <summary>
    /// 图形单位-长度-精度，范围<c>[0,8]</c>
    /// </summary>
    public int Luprec
    {
        get => Convert.ToInt32(Acap.GetSystemVariable(nameof(Luprec)));
        set => Acap.SetSystemVariable(nameof(Luprec), value);
    }

    #endregion

    #region M

    /// <summary>
    /// 图形单位
    /// </summary>
    public MeasurementValue Measurement
    {
        get => (MeasurementValue)Acap.GetSystemVariable(nameof(Measurement));
        set => Acap.SetSystemVariable(nameof(Measurement), Convert.ToInt32(value));
    }

    #endregion

    #region P

    /// <summary>
    /// 允许先选择后执行
    /// </summary>
    public static bool PickFirst
    {
        get => Convert.ToBoolean(Acap.GetSystemVariable(nameof(PickFirst)));
        set => Acap.SetSystemVariable(nameof(PickFirst), Convert.ToInt32(value));
    }

    #endregion

    #region T

    public static Point3d Target => (Point3d)Acap.GetSystemVariable(nameof(Target));

    #endregion
}