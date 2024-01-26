namespace IFoxCAD.Cad;


using System.Diagnostics;

/// <summary>
/// 加载时优先级
/// </summary>
[Flags]
public enum Sequence : byte
{
    /// <summary>
    /// 最先
    /// </summary>
    First,
    /// <summary>
    /// 最后
    /// </summary>
    Last, 
}

/// <summary>
/// 加载时自动执行接口
/// </summary>
public interface IFoxAutoGo
{
    /// <summary>
    /// 控制加载顺序
    /// </summary>
    /// <returns></returns>
    Sequence SequenceId();

    /// <summary>
    /// 关闭cad的时候会自动执行
    /// </summary>
    void Terminate();

    /// <summary>
    /// 打开cad的时候会自动执行
    /// </summary>
    void Initialize();
}

/// <summary>
/// 加载时自动执行特性
/// </summary>
/// <remarks>
/// 用于初始化和结束回收
/// </remarks>
/// <param name="sequence">优先级</param>
/// <param name="isInitialize"><see langword="true"/>用于初始化;<see langword="false"/>用于结束回收</param>
[AttributeUsage(AttributeTargets.Method)]
public class IFoxInitialize(Sequence sequence = Sequence.Last, bool isInitialize = true) : Attribute
{
    /// <summary>
    /// 优先级
    /// </summary>
    internal Sequence SequenceId = sequence;
    /// <summary>
    /// <see langword="true"/>用于初始化;<see langword="false"/>用于结束回收
    /// </summary>
    internal bool IsInitialize = isInitialize;
}

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

/// <summary>
/// 此类作为加载后cad自动运行接口的一部分,用于反射特性和接口
/// <para>
/// 启动cad后的执行顺序为:<br/>
/// 1:<see cref="IFoxInitialize"/>特性..(多个)<br/>
/// 2:<see cref="IFoxAutoGo"/>接口..(多个)
/// </para>
/// </summary>
/// <remarks>
/// 反射执行
/// <para>
/// 1.特性:<see cref="IFoxInitialize"/><br/>
/// 2.接口:<see cref="IFoxAutoGo"/>
/// </para>
/// </remarks>
/// <param name="dllName">约束在此dll进行加速</param>
/// <param name="configInfo"></param>
public class AutoReflection(string dllName, AutoRegConfig configInfo)
{
    private static List<RunClass> _InitializeList = []; // 储存方法用于初始化
    private static List<RunClass> _TerminateList = [];  // 储存方法用于结束释放

    readonly string _dllName = dllName;
    readonly AutoRegConfig _autoRegConfig = configInfo;

    /// <summary>
    /// 启动cad的时候会自动执行
    /// </summary>
    public void Initialize()
    {
        try
        {
            // 收集特性,包括启动时和关闭时
            if ((_autoRegConfig & AutoRegConfig.ReflectionAttribute) == AutoRegConfig.ReflectionAttribute)
                GetAttributeFunctions(_InitializeList, _TerminateList);

            if ((_autoRegConfig & AutoRegConfig.ReflectionInterface) == AutoRegConfig.ReflectionInterface)
            {
                GetInterfaceFunctions(_InitializeList, nameof(Initialize), _TerminateList, nameof(Terminate));
            }

            if (_InitializeList.Count <= 0) return;
            // 按照 SequenceId 排序_升序
            _InitializeList = _InitializeList.OrderBy(runClass => runClass.Sequence).ToList();
            RunFunctions(_InitializeList);
        }
        catch 
        {
            Debugger.Break();
        }
    }

    /// <summary>
    /// 关闭cad的时候会自动执行
    /// </summary>
    public void Terminate()
    {
        try
        {
            //if ((_autoRegConfig & AutoRegConfig.ReflectionInterface) == AutoRegConfig.ReflectionInterface)
            //    GetInterfaceFunctions(_TerminateList, nameof(Terminate));

            if (_TerminateList.Count <= 0) return;
            // 按照 SequenceId 排序_降序
            _TerminateList = [.. _TerminateList.OrderByDescending(runClass => runClass.Sequence)];
            RunFunctions(_TerminateList);
        }
        catch (Exception e)
        {
            Env.Printl(e.Message);
            Debugger.Break();
        }
    }

    /// <summary>
    /// 遍历程序域下所有类型
    /// </summary>
    /// <param name="action">输出每个成员执行</param>
    /// <param name="dllNameWithoutExtension">过滤此dll,不含后缀</param>
    public static void AppDomainGetTypes(Action<Type> action, string? dllNameWithoutExtension = null)
    {
#if DEBUG
        var error = 0;
#endif
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // cad2021出现如下报错
            // System.NotSupportedException:动态程序集中不支持已调用的成员
            // assemblies = assemblies.Where(p => !p.IsDynamic).ToArray();// 这个要容器类型转换
            assemblies = Array.FindAll(assemblies, p => !p.IsDynamic);

            // 主程序域
            for (var ii = 0; ii < assemblies.Length; ii++)
            {
                var assembly = assemblies[ii];

                // 获取类型集合,反射时候还依赖其他的dll就会这个错误
                // 此通讯库要跳过,否则会报错.
                var location = Path.GetFileNameWithoutExtension(assembly.Location);
                if (dllNameWithoutExtension != null && location != dllNameWithoutExtension)
                    continue;
                if (location == "AcInfoCenterConn")// 通讯库
                    continue;

                Type[]? types = null;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException) { continue; }

                if (types is null)
                    continue;

                for (var jj = 0; jj < types.Length; jj++)
                {
                    var type = types[jj];
                    if (type is not null)
                    {
#if DEBUG
                        ++error;
#endif
                        action.Invoke(type);
                    }
                }
            }
        }
#if DEBUG
        catch (Exception e)
        {
            DebugEx.Printl($"出错:{nameof(AppDomainGetTypes)};计数{error};错误信息:{e.Message}");
            Debugger.Break();
#else
        catch
        {        
#endif
        }
}

    /// <summary>
    /// 收集接口下的函数
    /// </summary>
    /// <param name="initializes"></param>
    /// <param name="initializeName"></param>
    /// <param name="terminates"></param>
    /// <param name="terminateName"></param>
    void GetInterfaceFunctions(List<RunClass> initializes, string initializeName,
                               List<RunClass> terminates, string terminateName)
    {
        AppDomainGetTypes(type => {
            // 接口的静态类屏蔽,继承接口无法使用静态类,因此跳过
            if (type.IsAbstract)
                return;

            var ints = type.GetInterfaces();
            for (int sss = 0; sss < ints.Length; sss++)
            {
                if (ints[sss].Name != nameof(IFoxAutoGo))
                    continue;

                Sequence? sequence = null;
                MethodInfo? initialize = null;
                MethodInfo? terminate = null;
                object? instance = null;

                var mets = type.GetMethods();
                for (int jj = 0; jj < mets.Length; jj++)
                {
                    var method = mets[jj];

                    // 接口的静态方法屏蔽,继承的方法也不可能是静态的,因此跳过
                    if (method.IsAbstract)
                        continue;

                    if (method.Name == nameof(IFoxAutoGo.SequenceId))
                    {
                        // 避免触发两次构造函数,所以这里需要ref构造的对象出来
                        var obj = method.Invoke(ref instance);
                        if (obj is not null)
                            sequence = (Sequence)obj;
                        continue;
                    }
                    if (method.Name == initializeName)
                    {
                        initialize = method;
                        continue;
                    }
                    if (method.Name == terminateName)
                    {
                        terminate = method;
                        continue;
                    }
                    if (sequence is not null && initialize is not null && terminate is not null)
                        break;
                }

                // 避免在terminate释放的时候去再次构造,所以需要一次性收集
                // 若是释放的时候去再次构造:
                // 0x01 initialize构造的字段拥有的资源就处于系统释放了,这是不合理的
                // 0x02 同时也发生了,为了释放而去构造这个操作
                var seq = sequence is null ? Sequence.Last : sequence.Value;
                if (initialize is not null)
                    initializes.Add(new(initialize, seq, instance));
                if (terminate is not null)
                    terminates.Add(new(terminate, seq, instance));
                break;
            }
        }, _dllName);
    }

    /// <summary>
    /// 收集特性下的函数
    /// </summary>
    void GetAttributeFunctions(List<RunClass> initializes,
                               List<RunClass> terminates)
    {
        AppDomainGetTypes(type => {
            if (!type.IsClass)
                return;

            // 特性的静态类不屏蔽
            //if (type.IsAbstract)
            //    return;

            var mets = type.GetMethods();
            for (int ii = 0; ii < mets.Length; ii++)
            {
                var method = mets[ii];

                // 特性的静态方法不屏蔽
                //if (method.IsAbstract)
                //    continue;

                var attr = method.GetCustomAttributes(true);
                for (int jj = 0; jj < attr.Length; jj++)
                {
                    if (attr[jj] is IFoxInitialize jjAtt)
                    {
                        var runc = new RunClass(method, jjAtt.SequenceId);
                        if (jjAtt.IsInitialize)
                            initializes.Add(runc);
                        else
                            terminates.Add(runc);
                        break;
                    }
                }
            }
        }, _dllName);
    }

    /// <summary>
    /// 执行收集到的函数
    /// </summary>
    static void RunFunctions(List<RunClass> runClassList)
    {
        for (int i = 0; i < runClassList.Count; i++)
            runClassList[i].Run();
        runClassList.Clear();
    }

#if Debug
    /// <summary>
    /// 检查当前程序域重复出现命令,
    /// 当出现重复时候将引起断点
    /// </summary>
    public static void DebugCheckCmdRecurrence()
    {
        HashSet<string> keys = new();

        // 本dll中存在冲突命令,此时cad自动接口可以运行,但是加载命令之后会报错,因此利用断点告诉程序员
        AutoReflection.AppDomainGetTypes(type => {
            var mets = type.GetMethods();
            for (int ii = 0; ii < mets.Length; ii++)
            {
                var method = mets[ii];
                var attr = method.GetCustomAttributes(true);
                for (int jj = 0; jj < attr.Length; jj++)
                    if (attr[jj] is CommandMethodAttribute att)
                    {
                        if (keys.Contains(att.GlobalName))
                            Debugger.Break();
                        keys.Add(att.GlobalName);
                    }
            }
        }, Assembly.GetCallingAssembly().GetName().Name);

        // 其他dll中存在冲突命令,此时会覆盖命令,友好的提示程序员
        keys.Clear();
        HashSet<string> msgMod = new();
        AutoReflection.AppDomainGetTypes(type => {
            var mets = type.GetMethods();
            for (int ii = 0; ii < mets.Length; ii++)
            {
                var method = mets[ii];
                var attr = method.GetCustomAttributes(true);
                for (int jj = 0; jj < attr.Length; jj++)
                    if (attr[jj] is CommandMethodAttribute att)
                    {
                        if (keys.Contains(att.GlobalName))
                            msgMod.Add(att.GlobalName);
                        keys.Add(att.GlobalName);
                    }
            }
        });
        var sb = new StringBuilder();
        foreach (string key in msgMod)
            sb.AppendLine(key);
        if (sb.Length != 0)
        {
            Env.Printl("当前cad环境加载的多个DLL中存在重复命令将被覆盖:");
            Env.Printl("{");
            Env.Printl(sb.ToString());
            Env.Printl("}");
        }
    }
#endif
}