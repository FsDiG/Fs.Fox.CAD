namespace Test;

public static class TestEntGet
{
    [CommandMethod(nameof(Test_EntGet))]
    public static void Test_EntGet()
    {
        var objectId = ObjectId.Null;

        try
        {
            using (var tr = new DBTrans())
            {
                var line = new Line(new Point3d(0, 0, 0), new Point3d(10, 0, 0));
                objectId = tr.CurrentSpace.AddEntity(line);
            }

            var values = Env.EntGet(objectId);

            for (var i = 1; i < 100; i++)
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
            Env.Printl($"EntGet passed for {dxfName} ({handle}).");
        }
        finally
        {
            if (objectId.IsOk())
            {
                using var tr = new DBTrans();
                var entity = tr.GetObject<Entity>(objectId, OpenMode.ForWrite);
                entity?.Erase();
            }
        }
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
