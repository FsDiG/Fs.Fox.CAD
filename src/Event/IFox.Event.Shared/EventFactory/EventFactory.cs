namespace IFoxCAD.Event;
public static class EventFactory
{
    /// <summary>
    /// ʹ��Cad�¼�
    /// </summary>
    /// <param name="cadEvent">�¼�ö��</param>
    /// <param name="assembly">����</param>
    public static void UseCadEvent(CadEvent cadEvent, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        IdleAction.Add(() =>
        {
            if ((cadEvent & CadEvent.SystemVariableChanged) != 0)
            {
                SystemVariableChangedEvent.Initlize(assembly);
            }
            if ((cadEvent & CadEvent.DocumentLockModeChanged) != 0)
            {
                DocumentLockModeChangedEvent.Initlize(assembly);
            }
            if ((cadEvent & CadEvent.BeginDoubleClick) != 0)
            {
                BeginDoubleClickEvent.Initlize(assembly);
            }
        });
    }
    /// <summary>
    /// ��ʱ�ر��¼�(��Ҫ���ö��)
    /// </summary>
    /// <param name="cadEvent"></param>
    /// <returns></returns>
    public static EventTemporaryShutdownManager TemporaryShutdown(CadEvent cadEvent = CadEvent.All)
    {
        return new EventTemporaryShutdownManager(cadEvent);
    }
    /// <summary>
    /// ����¼�
    /// </summary>
    /// <param name="cadEvent">�¼�ö��</param>
    public static void AddEvent(CadEvent cadEvent)
    {
        if ((cadEvent & CadEvent.SystemVariableChanged) != 0)
        {
            SystemVariableChangedEvent.AddEvent();
        }
        if ((cadEvent & CadEvent.DocumentLockModeChanged) != 0)
        {
            DocumentLockModeChangedEvent.AddEvent();
        }
        if ((cadEvent & CadEvent.BeginDoubleClick) != 0)
        {
            BeginDoubleClickEvent.AddEvent();
        }
    }
    /// <summary>
    /// �Ƴ��¼�
    /// </summary>
    /// <param name="cadEvent">�¼�ö��</param>
    public static void RemoveEvent(CadEvent cadEvent)
    {
        if ((cadEvent & CadEvent.SystemVariableChanged) != 0)
        {
            SystemVariableChangedEvent.RemoveEvent();
        }
        if ((cadEvent & CadEvent.DocumentLockModeChanged) != 0)
        {
            DocumentLockModeChangedEvent.RemoveEvent();
        }
        if ((cadEvent & CadEvent.BeginDoubleClick) != 0)
        {
            BeginDoubleClickEvent.RemoveEvent();
        }
    }

    internal static Func<bool> closeCheck = () => true;
#if Debug
    /// <summary>
    /// ����ж��ȫ���¼�������������Debug����ʹ�ã�
    /// ���㶯̬���ض�θ���ʱж�ص�֮ǰ��dll������ж��������Ҫ�û�����
    /// </summary>
    /// <param name="condition">����</param>
    public static void SetCloseCondition(this Func<bool> condition)
    {
        closeCheck = condition;
    }
#endif
}