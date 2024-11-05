namespace IFoxCAD.Basal;

/// <summary>
/// 系统扩展
/// </summary>
public class SystemEx
{
    /// <summary>
    /// 关闭进程
    /// </summary>
    /// <param name="procName">进程名</param>
    /// <returns>成功返回<c>true</c></returns>
    public static bool CloseProc(string procName)
    {
        var result = false;

        foreach (var thisProc in Process.GetProcesses())
        {
            var tempName = thisProc.ProcessName;
            if (tempName != procName) 
                continue;
            thisProc.Kill(); //当发送关闭窗口命令无效时强行结束进程                    
            result = true;
        }

        return result;
    }
}