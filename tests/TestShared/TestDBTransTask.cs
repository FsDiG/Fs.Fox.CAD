namespace Test;

/// <summary>
/// CAD-host commands; build coverage does not execute these methods.
/// </summary>
public static class TestDBTransTask
{
    [CommandMethod(nameof(Test_DBTransTaskRestoresWorkingDatabase))]
    public static void Test_DBTransTaskRestoresWorkingDatabase()
    {
        var originalWorkingDatabase = HostApplicationServices.WorkingDatabase;
        using var tr = CreateBackgroundTransaction();
        var actionUsedBackgroundDatabase = false;
        var restoredOriginalDatabase = false;

        try
        {
            tr.Task(() => {
                actionUsedBackgroundDatabase = IsCurrentWorkingDatabase(tr.Database);
            });
            restoredOriginalDatabase = IsCurrentWorkingDatabase(originalWorkingDatabase);
        }
        finally
        {
            HostApplicationServices.WorkingDatabase = originalWorkingDatabase;
        }

        if (!actionUsedBackgroundDatabase)
            throw new InvalidOperationException("DBTrans.Task did not activate the background database.");
        if (!restoredOriginalDatabase)
            throw new InvalidOperationException("DBTrans.Task did not restore WorkingDatabase.");

        Env.Printl("DBTrans Task normal path restored WorkingDatabase.");
    }

    [CommandMethod(nameof(Test_DBTransTaskRestoresWorkingDatabaseAfterException))]
    public static void Test_DBTransTaskRestoresWorkingDatabaseAfterException()
    {
        var originalWorkingDatabase = HostApplicationServices.WorkingDatabase;
        using var tr = CreateBackgroundTransaction();
        var expectedException = new ExpectedTaskException();
        var actionUsedBackgroundDatabase = false;
        var expectedExceptionPropagated = false;
        var restoredOriginalDatabase = false;

        try
        {
            try
            {
                tr.Task(() => {
                    actionUsedBackgroundDatabase = IsCurrentWorkingDatabase(tr.Database);
                    throw expectedException;
                });
            }
            catch (ExpectedTaskException exception) when (ReferenceEquals(exception, expectedException))
            {
                expectedExceptionPropagated = true;
            }

            restoredOriginalDatabase = IsCurrentWorkingDatabase(originalWorkingDatabase);
        }
        finally
        {
            HostApplicationServices.WorkingDatabase = originalWorkingDatabase;
        }

        if (!actionUsedBackgroundDatabase)
            throw new InvalidOperationException("DBTrans.Task did not activate the background database.");
        if (!expectedExceptionPropagated)
            throw new InvalidOperationException("DBTrans.Task did not propagate the original exception.");
        if (!restoredOriginalDatabase)
            throw new InvalidOperationException("DBTrans.Task did not restore WorkingDatabase after an exception.");

        Env.Printl("DBTrans Task exception path restored WorkingDatabase.");
    }

    private static DBTrans CreateBackgroundTransaction()
    {
        var fileName = Path.Combine(
            Path.GetTempPath(),
            $"FsFoxCad-DBTransTask-{Guid.NewGuid():N}.dwg");
        return new DBTrans(fileName, commit: false);
    }

    private static bool IsCurrentWorkingDatabase(Database expected)
    {
        var current = HostApplicationServices.WorkingDatabase;
        return ReferenceEquals(current, expected) ||
               current.UnmanagedObject == expected.UnmanagedObject;
    }

    private sealed class ExpectedTaskException : Exception
    {
    }
}
