namespace IFoxCAD.Event;
internal static class DocumentLockModeChangedEvent
{
    private static readonly Type returnType = typeof(void);
    private static readonly Type firstType = typeof(object);
    private static readonly Type secondType = typeof(DocumentLockModeChangedEventArgs);
    private static readonly Dictionary<string, HashSet<EventMethodInfo>> dic = new();
    internal static void Initlize(Assembly assembly)
    {
        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            if (!type.IsClass)
                continue;
            foreach (var methodInfo in type.GetMethods())
            {
                foreach (Attribute att in methodInfo.GetCustomAttributes(typeof(DocumentLockModeChangedAttribute), false))
                {
                    if (att is not DocumentLockModeChangedAttribute targetAtt)
                        continue;
                    if (!methodInfo.IsStatic)
                        throw new ArgumentException($"���{nameof(DocumentLockModeChangedAttribute)}���Եķ���{type.Name}.{methodInfo.Name},ӦΪ��̬����");
                    if (methodInfo.ReturnType != returnType)
                        throw new ArgumentException($"���{nameof(DocumentLockModeChangedAttribute)}���Եķ���{type.Name}.{methodInfo.Name}������ֵӦΪvoid");
                    var args = methodInfo.GetParameters();
                    var key = targetAtt.CommandName.ToUpper();
                    if (!dic.ContainsKey(key))
                    {
                        dic.Add(key, new());
                    }
                    if (args.Length > 2)
                        throw new ArgumentException($"���{nameof(DocumentLockModeChangedAttribute)}���Եķ���{type.Name}.{methodInfo.Name}���������ʹ���");


                    EventParameterType? ept = null;
                    if (args.Length == 0)
                        ept = EventParameterType.None;
                    else if (args.Length == 1)
                    {
                        if (args[0].ParameterType == firstType)
                            ept = EventParameterType.Object;
                        else if (args[0].ParameterType == secondType)
                            ept = EventParameterType.EventArgs;
                    }
                    else if (args.Length == 2 && args[0].ParameterType == firstType && args[1].ParameterType == secondType)
                    {
                        ept = EventParameterType.Complete;
                    }
                    if (ept is null)
                        throw new ArgumentException($"���{nameof(DocumentLockModeChangedAttribute)}���Եķ���{type.Name}.{methodInfo.Name}���������ʹ���");
                    dic[key].Add(new(methodInfo, ept.Value, targetAtt.Level));
                }
            }
        }
        AddEvent();
    }
    internal static void AddEvent()
    {
        Acap.DocumentManager.DocumentLockModeChanged -= DocumentManager_DocumentLockModeChanged;
        Acap.DocumentManager.DocumentLockModeChanged += DocumentManager_DocumentLockModeChanged;
    }

    internal static void RemoveEvent()
    {
        Acap.DocumentManager.DocumentLockModeChanged -= DocumentManager_DocumentLockModeChanged;
    }
    private static void DocumentManager_DocumentLockModeChanged(object sender, DocumentLockModeChangedEventArgs e)
    {
        var key = e.GlobalCommandName.ToUpper();
        if (!dic.ContainsKey(key))
            return;

#if Debug
        if (!EventFactory.closeCheck.Invoke())
        {
            EventFactory.RemoveEvent(CadEvent.All);
            return;
        }
#endif
        foreach (var eventMethodInfo in dic[key].OrderByDescending(a => a.Level))
        {
            switch (eventMethodInfo.ParameterType)
            {
                case EventParameterType.None:
                    eventMethodInfo.Method.Invoke(null, new object[0]);
                    break;
                case EventParameterType.Object:
                    eventMethodInfo.Method.Invoke(null, new[] { sender });
                    break;
                case EventParameterType.EventArgs:
                    eventMethodInfo.Method.Invoke(null, new[] { e });
                    break;
                case EventParameterType.Complete:
                    eventMethodInfo.Method.Invoke(null, new[] { sender, e });
                    break;
            }
        }
    }

}
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class DocumentLockModeChangedAttribute : Attribute
{
    /// <summary>
    /// ϵͳ�����޸�ʱ��������ǵĺ���
    /// ����ֵӦΪvoid
    /// ����������2����ֻ��Ϊobject��DocumentLockModeChangedEventArgs
    /// </summary>
    /// <param name="commandName">������</param>
    /// <param name="level">����(Խ��Խ�ȴ���)</param>
    public DocumentLockModeChangedAttribute(string commandName, int level = -1)
    {
        CommandName = commandName;
        Level = level;
    }

    /// <summary>
    /// ϵͳ������
    /// </summary>
    public string CommandName { get; }
    public int Level { get; }
}