namespace Test;

public static class TestProgressMeter
{
    private const int ProgressMinimum = 0;
    private const int ProgressMaximum = 20;
    private const int FailurePosition = 10;

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
            RunProgressMeter(FailurePosition);
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
                "Fs.Fox.CAD 进度测试", ProgressMinimum, ProgressMaximum);
            for (var position = ProgressMinimum; position <= ProgressMaximum; position++)
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
