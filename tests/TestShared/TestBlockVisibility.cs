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
}
