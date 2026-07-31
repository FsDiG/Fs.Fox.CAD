namespace Test;

public static class TestProgressMeter
{
    [CommandMethod(nameof(Test_ProgressMeter))]
    public static void Test_ProgressMeter()
    {
        RunProgressMeter();
        Env.Printl("Progress meter completed and the status bar was restored.");
    }

    [CommandMethod(nameof(Test_ProgressMeterFailure))]
    public static void Test_ProgressMeterFailure()
    {
        try
        {
            RunProgressMeter(10);
            throw new InvalidOperationException("The expected progress-meter failure was not raised.");
        }
        catch (ExpectedProgressMeterException)
        {
            Env.Printl("Progress meter exception path restored the status bar.");
        }
    }

    private static void RunProgressMeter(int? failurePosition = null)
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
                if (position == failurePosition)
                    throw new ExpectedProgressMeterException();
            }
        }
        finally
        {
            ProgressMeterUtils.RestoreApplicationStatusBar();
        }
    }

    private sealed class ExpectedProgressMeterException : Exception
    {
    }
}
