namespace Test;

public static class TestEntMod
{
    private const int EntityNameCode = -1;
    private const int ColorCode = 62;

    [CommandMethod(nameof(Test_EntMod))]
    public static void Test_EntMod()
    {
        var objectId = ObjectId.Null;
        Exception? testFailure = null;
        var successMessage = string.Empty;

        try
        {
            using (var tr = new DBTrans())
            {
                var line = new Line(new Point3d(0, 0, 0), new Point3d(10, 0, 0))
                {
                    ColorIndex = 1
                };
                objectId = tr.CurrentSpace.AddEntity(line);
            }

            var original = Env.EntGet(objectId);
            var modified = ReplaceColor(original, 2);

            if (!Env.EntMod(modified))
                throw new InvalidOperationException("EntMod rejected valid entity data.");

            AssertColor(objectId, 2);

            if (!Env.EntUpd(objectId))
                throw new InvalidOperationException("EntUpd rejected a valid ObjectId.");

            AssertColor(objectId, 2);
            AssertManagedArgumentChecks();

            var withoutEntityName = original
                .Where(value => value.TypeCode != EntityNameCode)
                .ToArray();
            if (withoutEntityName.Length == original.Length)
                throw new InvalidOperationException("EntGet did not return an entity-name value.");
            if (Env.EntMod(withoutEntityName))
                throw new InvalidOperationException("EntMod accepted data without an entity name.");

            successMessage = $"EntMod/EntUpd passed for {objectId.Handle}.";
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

    private static TypedValue[] ReplaceColor(TypedValue[] values, short colorIndex)
    {
        var result = new TypedValue[values.Length];
        var replaced = false;

        for (var i = 0; i < values.Length; i++)
        {
            if (values[i].TypeCode == ColorCode)
            {
                result[i] = new TypedValue(ColorCode, colorIndex);
                replaced = true;
            }
            else
            {
                result[i] = values[i];
            }
        }

        if (!replaced)
            throw new InvalidOperationException("EntGet did not return the line color.");

        return result;
    }

    private static void AssertColor(ObjectId objectId, short expected)
    {
        var color = Env.EntGet(objectId)
            .Where(value => value.TypeCode == ColorCode)
            .Select(value => Convert.ToInt16(value.Value))
            .FirstOrDefault();
        if (color != expected)
        {
            throw new InvalidOperationException(
                $"Unexpected color. Expected {expected}, got {color}.");
        }
    }

    private static void AssertManagedArgumentChecks()
    {
        try
        {
            _ = Env.EntMod(null!);
            throw new InvalidOperationException("EntMod accepted null data.");
        }
        catch (System.ArgumentNullException)
        {
            // Expected: null data is rejected before native interop.
        }

        try
        {
            _ = Env.EntMod(Array.Empty<TypedValue>());
            throw new InvalidOperationException("EntMod accepted empty data.");
        }
        catch (System.ArgumentException)
        {
            // Expected: empty data is rejected before native interop.
        }

        try
        {
            _ = Env.EntUpd(ObjectId.Null);
            throw new InvalidOperationException("EntUpd accepted ObjectId.Null.");
        }
        catch (System.ArgumentException)
        {
            // Expected: invalid identifiers are rejected before native interop.
        }
    }
}
