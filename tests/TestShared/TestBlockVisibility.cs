namespace Test;

public static class TestBlockVisibility
{
    [CommandMethod(nameof(Test_BlockVisibilityInfo))]
    public static void Test_BlockVisibilityInfo()
    {
        var options = new PromptEntityOptions("\nSelect a block reference: ");
        options.SetRejectMessage("\nThe selected object is not a block reference.");
        options.AddAllowedClass(typeof(BlockReference), true);

        var prompt = Env.Editor.GetEntity(options);
        if (prompt.Status != PromptStatus.OK)
            return;

        using var tr = new DBTrans();
        var blockReference = tr.GetObject<BlockReference>(prompt.ObjectId)
                             ?? throw new InvalidOperationException(
                                 "The selected block reference could not be opened.");
        var info = blockReference.GetVisibilityInfo();

        if (!blockReference.IsDynamicBlock && info.Has)
        {
            throw new InvalidOperationException(
                "A non-dynamic block reported a visibility parameter.");
        }

        if (info.Has && (string.IsNullOrWhiteSpace(info.PropertyName) ||
                         info.AllowedValues.Count == 0))
        {
            throw new InvalidOperationException(
                "The visibility parameter metadata is incomplete.");
        }

        var allowedValues = info.Has ? string.Join(", ", info.AllowedValues) : "none";
        Env.Printl(
            $"Visibility: Has={info.Has}, Property={info.PropertyName}, Values={allowedValues}");
    }

    [CommandMethod(nameof(Test_BlockVisibilityInfoAll))]
    public static void Test_BlockVisibilityInfoAll()
    {
        using var tr = new DBTrans();
        var blockReferences = tr.CurrentSpace.GetEntities<BlockReference>().ToList();
        if (blockReferences.Count == 0)
        {
            Env.Printl("[SKIP] Block visibility scan requires a drawing with block references.");
            return;
        }

        var mutationEvents = new List<string>();
        var dynamicCount = 0;
        var visibilityCount = 0;

        void OnObjectOpenedForModify(object sender, ObjectEventArgs args)
        {
            mutationEvents.Add($"OpenedForModify:{args.DBObject.ObjectId}");
        }

        void OnObjectAppended(object sender, ObjectEventArgs args)
        {
            mutationEvents.Add($"Appended:{args.DBObject.ObjectId}");
        }

        tr.Database.ObjectOpenedForModify += OnObjectOpenedForModify;
        tr.Database.ObjectAppended += OnObjectAppended;
        try
        {
            foreach (var blockReference in blockReferences)
            {
                var info = blockReference.GetVisibilityInfo();
                if (blockReference.IsDynamicBlock)
                    dynamicCount++;
                else if (info.Has)
                    throw new InvalidOperationException("A non-dynamic block reported visibility metadata.");

                if (!info.Has)
                    continue;

                visibilityCount++;
                if (string.IsNullOrWhiteSpace(info.PropertyName) || info.AllowedValues.Count == 0)
                {
                    throw new InvalidOperationException(
                        $"Block {blockReference.Handle} returned incomplete visibility metadata.");
                }

                Env.Printl(
                    $"Visibility block {blockReference.Handle}: {info.PropertyName} = {string.Join(", ", info.AllowedValues)}");
            }
        }
        finally
        {
            tr.Database.ObjectOpenedForModify -= OnObjectOpenedForModify;
            tr.Database.ObjectAppended -= OnObjectAppended;
        }

        if (mutationEvents.Count != 0)
        {
            throw new InvalidOperationException(
                $"Visibility query mutated the database: {string.Join("; ", mutationEvents)}");
        }

        if (dynamicCount == 0)
        {
            Env.Printl("[SKIP] Block visibility scan requires at least one dynamic block reference.");
            return;
        }

        if (visibilityCount == 0)
        {
            Env.Printl("[SKIP] Block visibility scan requires at least one visibility parameter.");
            return;
        }

        Env.Printl(
            $"Block visibility scan passed. Total={blockReferences.Count}, Dynamic={dynamicCount}, WithVisibility={visibilityCount}.");
    }

    [CommandMethod(nameof(Test_BlockVisibilityInfoOrdinary))]
    public static void Test_BlockVisibilityInfoOrdinary()
    {
        var blockName = $"FsFoxOrdinary_{Guid.NewGuid():N}";
        var blockId = ObjectId.Null;
        var blockReferenceId = ObjectId.Null;

        try
        {
            using (var tr = new DBTrans())
            {
                blockId = tr.BlockTable.Add(blockName,
                    new Entity[] { new Line(Point3d.Origin, new Point3d(5, 0, 0)) });
                blockReferenceId = tr.CurrentSpace.InsertBlock(Point3d.Origin, blockId,
                    new Scale3d(1));
            }

            using (var tr = new DBTrans())
            {
                var blockReference = tr.GetObject<BlockReference>(blockReferenceId)
                                     ?? throw new InvalidOperationException(
                                         "The temporary ordinary block reference was not found.");
                var blockDefinition = tr.GetObject<BlockTableRecord>(blockId)
                                      ?? throw new InvalidOperationException(
                                          "The temporary ordinary block definition was not found.");
                var extensionDictionaryBefore = blockDefinition.ExtensionDictionary;
                var mutationEvents = new List<string>();

                void OnObjectOpenedForModify(object sender, ObjectEventArgs args)
                {
                    mutationEvents.Add($"OpenedForModify:{args.DBObject.ObjectId}");
                }

                void OnObjectAppended(object sender, ObjectEventArgs args)
                {
                    mutationEvents.Add($"Appended:{args.DBObject.ObjectId}");
                }

                tr.Database.ObjectOpenedForModify += OnObjectOpenedForModify;
                tr.Database.ObjectAppended += OnObjectAppended;
                BlockVisibilityInfo info;
                try
                {
                    info = blockReference.GetVisibilityInfo();
                }
                finally
                {
                    tr.Database.ObjectOpenedForModify -= OnObjectOpenedForModify;
                    tr.Database.ObjectAppended -= OnObjectAppended;
                }

                if (info.Has)
                    throw new InvalidOperationException("An ordinary block reported visibility metadata.");
                if (blockDefinition.ExtensionDictionary != extensionDictionaryBefore)
                    throw new InvalidOperationException("The visibility query created an extension dictionary.");
                if (mutationEvents.Count != 0)
                {
                    throw new InvalidOperationException(
                        $"Ordinary block visibility query mutated the database: {string.Join("; ", mutationEvents)}");
                }
            }
        }
        finally
        {
            using var tr = new DBTrans();
            if (blockReferenceId.IsOk())
                tr.GetObject<BlockReference>(blockReferenceId, OpenMode.ForWrite)?.Erase();
            if (blockId.IsOk())
                tr.GetObject<BlockTableRecord>(blockId, OpenMode.ForWrite)?.Erase(true);
        }

        Env.Printl("Ordinary block visibility query passed.");
    }
}
