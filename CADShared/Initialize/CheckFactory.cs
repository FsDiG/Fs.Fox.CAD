﻿#if DEBUG
namespace IFoxCAD.Cad;

/// <summary>
/// 命令检查类
/// </summary>
public static class CheckFactory
{
    /*
     * 平时command命令的globalName如果重复，加载时会报错
     * 但是并不会告诉你是哪里错了，通常需要花大量时间来查找
     * 将此函数添加在IExtensionApplication.Initialize()函数开头
     * 虽然还会报错，但是至少能知道哪个类下哪个方法导致的报错
     * 聊胜于无吧
     * 2023-05-16 by DYH
     */

    /// <summary>
    /// 检查Command命令重复
    /// </summary>
    public static void CheckDuplicateCommand(Assembly? assembly = null)
    {
        var dic = new Dictionary<string, List<string>>();
        assembly ??= Assembly.GetCallingAssembly();
        // 反射所有的公共类型
        var typeArray = assembly.GetExportedTypes();
        foreach (var type in typeArray)
        {
            if (!type.IsPublic)
                continue;
            foreach (var method in type.GetMethods())
            {
                if (!method.IsPublic)
                    continue;
                if (method.GetCustomAttribute<CommandMethodAttribute>() is not { } att)
                    continue;
                if (!dic.ContainsKey(att.GlobalName))
                {
                    dic.Add(att.GlobalName, []);
                }

                dic[att.GlobalName].Add(type.Name + "." + method.Name);
            }
        }

        var strings = dic
            .Where(o => o.Value.Count() > 1)
            .Select(o => o.Key + "命令重复，在类" + string.Join("和", o.Value) + "中");
        var str = string.Join(Environment.NewLine, strings);
        if (!string.IsNullOrEmpty(str))
            System.Windows.Forms.MessageBox.Show(str, @"错误：重复命令！");
    }
}
#endif