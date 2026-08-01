namespace Fs.Fox.Cad;

// 为了解决IExtensionApplication在一个dll内无法多次实现接口的关系
// 所以在这里反射加载所有的 IAutoGo ,以达到能分开写"启动运行"函数的目的
/// <summary>
/// 执行此方法
/// </summary>
/// <param name="method"></param>
/// <param name="sequence"></param>
/// <param name="instance">已经创建的对象</param>
internal class RunClass(MethodInfo method, Sequence sequence, object? instance = null)
{
    public Sequence Sequence { get; } = sequence;

    /// <summary>
    /// 运行方法
    /// </summary>
    public void Run()
    {
        method.Invoke(ref instance);
    }
}
