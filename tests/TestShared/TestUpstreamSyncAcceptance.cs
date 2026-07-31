namespace Test;

public static class TestUpstreamSyncAcceptance
{
    [CommandMethod(nameof(Test_JigDisposeSafety))]
    public static void Test_JigDisposeSafety()
    {
        var queueField = typeof(JigEx).GetField("_drawEntities",
                             BindingFlags.Instance | BindingFlags.NonPublic)
                         ?? throw new InvalidOperationException(
                             "JigEx draw-entity queue field was not found.");
        var jig = new JigEx();
        var externallyDisposedLine = new Line(Point3d.Origin, new Point3d(1, 0, 0));
        var jigOwnedLine = new Line(Point3d.Origin, new Point3d(2, 0, 0));
        Exception? testFailure = null;

        try
        {
            externallyDisposedLine.Dispose();
            var queue = queueField.GetValue(jig) as Queue<Entity>
                        ?? throw new InvalidOperationException(
                            "JigEx draw-entity queue has an unexpected type.");
            queue.Enqueue(externallyDisposedLine);
            queue.Enqueue(jigOwnedLine);

            jig.Dispose();

            if (!jig.IsDisposed)
                throw new InvalidOperationException("JigEx did not enter the disposed state.");
            if (!jigOwnedLine.IsDisposed)
                throw new InvalidOperationException("JigEx did not dispose its queued transient entity.");
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
                if (!jig.IsDisposed)
                    jig.Dispose();
                if (!externallyDisposedLine.IsDisposed)
                    externallyDisposedLine.Dispose();
                if (!jigOwnedLine.IsDisposed)
                    jigOwnedLine.Dispose();
            }
            catch (Exception cleanupException) when (testFailure is not null)
            {
                testFailure.Data["CleanupException"] = cleanupException;
            }
        }

        Env.Printl("Jig dispose safety passed.");
    }

    [CommandMethod(nameof(Test_XDataRemovalIsolation))]
    public static void Test_XDataRemovalIsolation()
    {
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        var firstApp = $"FsFoxXDataA_{suffix}";
        var secondApp = $"FsFoxXDataB_{suffix}";
        var missingApp = $"FsFoxMissing_{suffix}";
        var lineId = ObjectId.Null;
        Exception? testFailure = null;

        try
        {
            using (var tr = new DBTrans())
            {
                tr.RegAppTable.Add(firstApp);
                tr.RegAppTable.Add(secondApp);

                var line = new Line(Point3d.Origin, new Point3d(10, 0, 0))
                {
                    XData = new XDataList
                    {
                        { DxfCode.ExtendedDataRegAppName, firstApp },
                        { DxfCode.ExtendedDataAsciiString, "first" },
                        { DxfCode.ExtendedDataRegAppName, secondApp },
                        { DxfCode.ExtendedDataAsciiString, "second" }
                    }
                };
                lineId = tr.CurrentSpace.AddEntity(line);
            }

            using (var tr = new DBTrans())
            {
                var line = tr.GetObject<Line>(lineId)
                           ?? throw new InvalidOperationException("The temporary XData line was not found.");
                var openedForModify = 0;

                void OnObjectOpenedForModify(object sender, ObjectEventArgs args)
                {
                    if (args.DBObject.ObjectId == lineId)
                        openedForModify++;
                }

                tr.Database.ObjectOpenedForModify += OnObjectOpenedForModify;
                try
                {
                    var firstDataBefore = GetXDataValues(line, firstApp);
                    var secondDataBefore = GetXDataValues(line, secondApp);

                    line.RemoveXData(missingApp);
                    if (openedForModify != 0)
                    {
                        throw new InvalidOperationException(
                            "Removing a missing RegApp opened the entity for modify.");
                    }

                    AssertXDataValue(firstDataBefore, firstApp, "first");
                    AssertXDataValue(secondDataBefore, secondApp, "second");
                    AssertXDataSequence(line, firstApp, firstDataBefore);
                    AssertXDataSequence(line, secondApp, secondDataBefore);

                    line.RemoveXData(firstApp);
                    if (openedForModify == 0)
                    {
                        throw new InvalidOperationException(
                            "Removing an existing RegApp did not open the entity for modify.");
                    }

                    using var removedData = line.GetXDataForApplication(firstApp);
                    if (removedData is not null)
                        throw new InvalidOperationException("The target RegApp XData was not removed.");
                    AssertXDataSequence(line, secondApp, secondDataBefore);
                }
                finally
                {
                    tr.Database.ObjectOpenedForModify -= OnObjectOpenedForModify;
                }
            }
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
                EraseEntity(lineId);
            }
            catch (Exception cleanupException) when (testFailure is not null)
            {
                testFailure.Data["CleanupException"] = cleanupException;
            }
        }

        Env.Printl("XData removal isolation passed.");
    }

    [CommandMethod(nameof(Test_BlockAttributeWriteScope))]
    public static void Test_BlockAttributeWriteScope()
    {
        const string targetTag = "TARGET";
        const string otherTag = "OTHER";
        const string updatedValue = "updated";
        const string otherValue = "untouched";
        var blockName = $"FsFoxAttr_{Guid.NewGuid():N}";
        var blockId = ObjectId.Null;
        var blockReferenceId = ObjectId.Null;
        Exception? testFailure = null;

        try
        {
            using (var tr = new DBTrans())
            {
                var definitions = new[]
                {
                    CreateAttributeDefinition(targetTag, "first", new Point3d(0, 0, 0)),
                    CreateAttributeDefinition(targetTag, "second", new Point3d(0, 2, 0)),
                    CreateAttributeDefinition(otherTag, otherValue, new Point3d(0, 4, 0))
                };
                blockId = tr.BlockTable.Add(blockName,
                    new Entity[] { new Line(Point3d.Origin, new Point3d(5, 0, 0)) }, definitions);
                blockReferenceId = tr.CurrentSpace.InsertBlock(Point3d.Origin, blockId,
                    new Scale3d(1), atts: new Dictionary<string, string>
                    {
                        { targetTag, "original" },
                        { otherTag, otherValue }
                    });
            }

            using (var tr = new DBTrans())
            {
                var blockReference = tr.GetObject<BlockReference>(blockReferenceId)
                                     ?? throw new InvalidOperationException(
                                         "The temporary block reference was not found.");
                var attributes = blockReference.GetAttributes().ToList();
                var targetAttributes = attributes.Where(attribute => attribute.Tag == targetTag).ToList();
                var otherAttributes = attributes.Where(attribute => attribute.Tag == otherTag).ToList();
                if (targetAttributes.Count != 2 || otherAttributes.Count != 1)
                {
                    throw new InvalidOperationException(
                        $"Unexpected attribute layout. Target={targetAttributes.Count}, Other={otherAttributes.Count}.");
                }

                var attributeIds = attributes.Select(attribute => attribute.ObjectId).ToHashSet();
                var targetIds = targetAttributes.Select(attribute => attribute.ObjectId).ToHashSet();
                var openedAttributeIds = new HashSet<ObjectId>();

                void OnObjectOpenedForModify(object sender, ObjectEventArgs args)
                {
                    if (attributeIds.Contains(args.DBObject.ObjectId))
                        openedAttributeIds.Add(args.DBObject.ObjectId);
                }

                tr.Database.ObjectOpenedForModify += OnObjectOpenedForModify;
                HashSet<ObjectId> targetUpdateIds;
                HashSet<ObjectId> missingUpdateIds;
                try
                {
                    blockReference.ChangeBlockAttribute(new Dictionary<string, string>
                    {
                        { targetTag, updatedValue }
                    });
                    targetUpdateIds = openedAttributeIds.ToHashSet();

                    openedAttributeIds.Clear();
                    blockReference.ChangeBlockAttribute(new Dictionary<string, string>
                    {
                        { "MISSING", "ignored" }
                    });
                    missingUpdateIds = openedAttributeIds.ToHashSet();
                }
                finally
                {
                    tr.Database.ObjectOpenedForModify -= OnObjectOpenedForModify;
                }

                if (!targetUpdateIds.SetEquals(targetIds))
                {
                    throw new InvalidOperationException(
                        $"Unexpected attributes opened for modify. Expected={targetIds.Count}, Actual={targetUpdateIds.Count}.");
                }
                if (missingUpdateIds.Count != 0)
                    throw new InvalidOperationException("A missing attribute Tag opened an attribute for modify.");

                if (targetAttributes.Any(attribute => attribute.TextString != updatedValue))
                    throw new InvalidOperationException("Not all duplicate target attributes were updated.");
                if (otherAttributes[0].TextString != otherValue)
                    throw new InvalidOperationException("The unrelated attribute value was changed.");
            }
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
                EraseBlock(blockReferenceId, blockId);
            }
            catch (Exception cleanupException) when (testFailure is not null)
            {
                testFailure.Data["CleanupException"] = cleanupException;
            }
        }

        Env.Printl("Block attribute write scope passed.");
    }

    private static TypedValue[] GetXDataValues(DBObject obj, string appName)
    {
        using var data = obj.GetXDataForApplication(appName);
        return data?.AsArray() ?? Array.Empty<TypedValue>();
    }

    private static void AssertXDataValue(
        IEnumerable<TypedValue> data, string appName, string expectedValue)
    {
        var value = data
            .FirstOrDefault(item => item.TypeCode == (int)DxfCode.ExtendedDataAsciiString)
            .Value?.ToString();
        if (!string.Equals(value, expectedValue, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Unexpected XData for {appName}. Expected {expectedValue}, got {value}.");
        }
    }

    private static void AssertXDataSequence(
        DBObject obj, string appName, IReadOnlyList<TypedValue> expected)
    {
        var actual = GetXDataValues(obj, appName);
        if (actual.Length != expected.Count)
        {
            throw new InvalidOperationException(
                $"XData length changed for {appName}. Expected {expected.Count}, got {actual.Length}.");
        }

        for (var i = 0; i < actual.Length; i++)
        {
            if (actual[i].TypeCode != expected[i].TypeCode ||
                !Equals(actual[i].Value, expected[i].Value))
            {
                throw new InvalidOperationException(
                    $"XData item {i} changed for {appName}.");
            }
        }
    }

    private static AttributeDefinition CreateAttributeDefinition(
        string tag, string value, Point3d position)
    {
        return new AttributeDefinition
        {
            Tag = tag,
            TextString = value,
            Position = position,
            Height = 1
        };
    }

    private static void EraseEntity(ObjectId objectId)
    {
        if (!objectId.IsOk())
            return;

        using var tr = new DBTrans();
        var entity = tr.GetObject<Entity>(objectId, OpenMode.ForWrite);
        entity?.Erase();
    }

    private static void EraseBlock(ObjectId blockReferenceId, ObjectId blockId)
    {
        using var tr = new DBTrans();
        if (blockReferenceId.IsOk())
        {
            var blockReference = tr.GetObject<BlockReference>(blockReferenceId, OpenMode.ForWrite);
            blockReference?.Erase();
        }

        if (blockId.IsOk())
        {
            var block = tr.GetObject<BlockTableRecord>(blockId, OpenMode.ForWrite);
            block?.Erase(true);
        }
    }
}
