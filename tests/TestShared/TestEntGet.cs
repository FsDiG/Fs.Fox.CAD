namespace Test;

public static class TestEntGet
{
    private const int ReadCount = 100;

    [CommandMethod(nameof(Test_EntGet))]
    public static void Test_EntGet()
    {
        var objectId = ObjectId.Null;
        Exception? testFailure = null;
        var successMessage = string.Empty;

        try
        {
            using (var tr = new DBTrans())
            {
                var line = new Line(new Point3d(0, 0, 0), new Point3d(10, 0, 0));
                objectId = tr.CurrentSpace.AddEntity(line);
            }

            var values = Array.Empty<TypedValue>();
            for (var i = 0; i < ReadCount; i++)
            {
                values = Env.EntGet(objectId);
                if (values.Length == 0)
                    throw new InvalidOperationException("EntGet returned an empty result.");
            }

            var dxfName = values.Where(value => value.TypeCode == 0)
                .Select(value => value.Value?.ToString())
                .FirstOrDefault();
            if (!string.Equals(dxfName, objectId.ObjectClass.DxfName,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Unexpected DXF name. Expected {objectId.ObjectClass.DxfName}, got {dxfName}.");
            }

            var handle = values.Where(value => value.TypeCode == 5)
                .Select(value => value.Value?.ToString())
                .FirstOrDefault();
            if (!string.Equals(handle, objectId.Handle.ToString(),
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Unexpected handle. Expected {objectId.Handle}, got {handle}.");
            }

            AssertNullObjectIdRejected();
            successMessage = $"EntGet passed for {dxfName} ({handle}).";
        }
        catch (Exception exception)
        {
            testFailure = exception;
            throw;
        }
        finally
        {
            try
            {
                if (objectId.IsOk())
                {
                    using var tr = new DBTrans();
                    var entity = tr.GetObject<Entity>(objectId, OpenMode.ForWrite);
                    entity?.Erase();
                }
            }
            catch (Exception cleanupException) when (testFailure is not null)
            {
                testFailure.Data["CleanupException"] = cleanupException;
            }
        }

        Env.Printl(successMessage);
    }

    private static void AssertNullObjectIdRejected()
    {
        try
        {
            _ = Env.EntGet(ObjectId.Null);
            throw new InvalidOperationException("EntGet accepted ObjectId.Null.");
        }
        catch (System.ArgumentException)
        {
            // Expected: invalid identifiers are rejected before native interop.
        }
    }
}
