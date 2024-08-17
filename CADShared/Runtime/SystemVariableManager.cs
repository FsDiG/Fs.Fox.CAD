namespace IFoxCAD.Cad;

/// <summary>
/// 系统变量管理器
/// </summary>
public class SystemVariableManager
{
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