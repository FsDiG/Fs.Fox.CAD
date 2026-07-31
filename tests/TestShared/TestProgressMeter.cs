namespace Test;

public static class TestProgressMeter
{
    [CommandMethod(nameof(Test_ProgressMeter))]
    public static void Test_ProgressMeter()
    {
        try
        {
            ProgressMeterUtils.SetApplicationStatusBarProgressMeter(
                "Fs.Fox.CAD 进度测试", 0, 20);
            for (var position = 0; position <= 20; position++)
            {
                ProgressMeterUtils.SetApplicationStatusBarProgressMeter(position);
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(50);
            }
        }
        finally
        {
            ProgressMeterUtils.RestoreApplicationStatusBar();
        }

        Env.Printl("Progress meter completed and the status bar was restored.");
    }
}
