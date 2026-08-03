// ReSharper disable ForCanBeConvertedToForeach

#if AC_NET48 || ZWCAD || GC_2022 || GC_2023
using ArgumentNullException = Fs.Fox.Basal.ArgumentNullEx;
#endif

namespace Fs.Fox.Cad;

/// <summary>
/// 获取外部参照的路径
/// </summary>
public class XrefPath
{
    #region 属性

    /// <summary>
    /// 基础路径
    /// </summary>
    public readonly string? CurrentDatabasePath;

    /// <summary>
    /// 是否外部参照
    /// </summary>
    public bool IsFromExternalReference { get; private set; }

    /// <summary>
    /// 外部参照保存的路径
    /// <para>
    /// 它们会是以下任一路径:<br/>
    /// 0x01 相对路径<br/>
    /// 0x02 绝对路径<br/>
    /// 0x03 共目录优先找到的路径(文件夹整体移动会发生此类情况)
    /// </para>
    /// </summary>
    public string? PathSave { get; private set; }

    /// <summary>
    /// 找到的路径(参照面板的名称)
    /// <para><see cref="PathSave"/>路径不存在时,返回是外部参照dwg文件路径</para>
    /// </summary>
    public string? PathDescribe { get; private set; }

    private string? _pathComplete;

    /// <summary>
    /// 绝对路径
    /// </summary>
    public string? PathComplete =>
        _pathComplete ??= PathConverter(CurrentDatabasePath, PathDescribe, PathConverterModes.Complete);

    private string? _pathRelative;

    /// <summary>
    /// 相对路径
    /// </summary>
    public string? PathRelative =>
        _pathRelative ??= PathConverter(CurrentDatabasePath, PathComplete, PathConverterModes.Relative);

    #endregion

    #region 构造

    /// <summary>
    /// 获取外部参照的路径
    /// </summary>
    /// <param name="brf">外部参照图元</param>
    /// <param name="tr">事务</param>
    /// <returns>是否外部参照</returns>
    public XrefPath(BlockReference brf, DBTrans tr)
    {
        //if (brf == null)
        //    throw new ArgumentNullException(nameof(brf));
        ArgumentNullException.ThrowIfNull(brf);
        CurrentDatabasePath = Path.GetDirectoryName(tr.Database.Filename);

        var btRec = tr.GetObject<BlockTableRecord>(brf.BlockTableRecord); // 块表记录
        if (btRec == null)
            return;

        IsFromExternalReference = btRec.IsFromExternalReference;
        if (!IsFromExternalReference)
            return;

        // 相对路径==".\\AA.dwg"
        // 无路径=="AA.dwg"
        PathSave = btRec.PathName;

        if ((!string.IsNullOrEmpty(PathSave) && PathSave[0] == '.') || File.Exists(PathSave))
        {
            // 相对路径||绝对路径
            PathDescribe = PathSave;
        }
        else
        {
            // 无路径
            var db = btRec.GetXrefDatabase(true);
            PathDescribe = db.Filename;
        }
    }

    #endregion

    #region 静态函数

    /// <summary>
    /// 获取相对路径或者绝对路径
    /// <see href="https://www.cnblogs.com/hont/p/5412340.html">参考链接</see>
    /// </summary>
    /// <param name="directory">基础目录(末尾无斜杠)</param>
    /// <param name="fileRelations">相对路径或者绝对路径</param>
    /// <param name="converterModes">依照枚举返回对应的字符串</param>
    /// <returns></returns>
    public static string? PathConverter(string? directory, string? fileRelations,
        PathConverterModes converterModes)
    {
        //if (directory == null)
        //    throw new ArgumentNullException(nameof(directory));
        //if (fileRelations == null)
        //    throw new ArgumentNullException(nameof(fileRelations));

        ArgumentNullException.ThrowIfNull(directory);
        ArgumentNullException.ThrowIfNull(fileRelations);

        string? result = null;
        switch (converterModes)
        {
            case PathConverterModes.Relative:
                result = GetRelativePath(directory, fileRelations);
                break;
            case PathConverterModes.Complete:
                result = GetCompletePath(directory, fileRelations);
                break;
        }

        return result;
    }

#if error_demo
    /// <summary>
    /// 绝对路径->相对路径
    /// </summary>
    /// <param name="strDbPath">绝对路径</param>
    /// <param name="strXrefPath">相对关系</param>
    /// <returns></returns>
    /// StringHelper.GetRelativePath("G:\\A1.项目\\20190920金山谷黄宅\\01.饰施图\\03.平面图",
    /// "G:\\A1.项目\\20190920金山谷黄宅\\01.饰施图\\01.辅助文件\\图框\\A3图框.dwg");
    public static string GetRelativePath(string strDbPath, string strXrefPath)
    {
        Uri uri1 = new(strXrefPath);
        Uri uri2 = new(strDbPath);
        Uri relativeUri = uri2.MakeRelativeUri(uri1);
        // 测试例子变成 01.%E8%BE%85%E5%8A%A9%E6%96%87%E4%BB%B6/%E5%9B%BE%E6%A1%86/A3%E5%9B%BE%E6%A1%86.dwg
        string str = relativeUri.ToString();

        // 因为这里不会实现".\A.dwg"而是"A.dwg",所以加入这个操作,满足同目录文件
        var strs = str.Split('\\');
        if (strs.Length == 1)
            str = ".\\" + str;
        return str;
    }
#else
    /// <summary>
    /// 绝对路径->相对路径
    /// </summary>
    /// <param name="directory">相对关系:文件夹路径</param>
    /// <param name="file">完整路径:文件路径</param>
    /// <returns>相对路径</returns>
    /// <![CDATA[
    /// GetRelativePath("G:\\A1.项目\\20190920金山谷黄宅\\01.饰施图\\03.平面图",
    /// "G:\\A1.项目\\20190920金山谷黄宅\\01.饰施图\\01.辅助文件\\图框\\A3图框.dwg")
    /// =>  "..\\01.辅助文件\\图框\\A3图框.dwg"
    /// ]]>
    private static string GetRelativePath(string directory, string file)
    {
        string[] directories = directory.Split('\\');
        string[] files = file.Split('\\');
        // 获取两条路径中的最短路径
        var getMinLength = directories.Length < files.Length ? directories.Length : files.Length;

        // 用于确定我们退出的循环中的位置。
        var lastCommonRoot = -1;
        int index;
        // 找到共根
        for (index = 0; index < getMinLength; index++)
        {
            if (directories[index] != files[index])
                break;
            lastCommonRoot = index;
        }

        // 如果我们没有找到一个共同的前缀,那么抛出
        if (lastCommonRoot == -1)
            throw new ArgumentException("路径没有公共相同路径部分");

        // 建立相对路径
        var result = new StringBuilder();
        for (index = lastCommonRoot + 1; index < directories.Length; index++)
            if (directories[index].Length > 0)
                result.Append("..\\"); // 上级目录加入

        // 添加文件夹
        for (index = lastCommonRoot + 1; index < files.Length - 1; index++)
            result.Append(files[index] + "\\");

        // 本级目录
        if (result.Length == 0)
            result.Append(".\\");
        // result.Append(strXrefPaths[^1]);// 下级目录加入
        result.Append(files[^1]); // 下级目录加入
        return result.ToString();
    }
#endif

    /// <summary>
    /// 相对路径->绝对路径
    /// </summary>
    /// <param name="directory">文件夹路径</param>
    /// <param name="relativePath">相对关系:有..的</param>
    /// <returns>完整路径</returns>
    /// <![CDATA[
    /// GetCompletePath("G:\\A1.项目\\20190920金山谷黄宅\\01.饰施图\\03.平面图" ,
    /// "..\\01.辅助文件\\图框\\A3图框.dwg")
    /// =>   "G:\\A1.项目\\20190920金山谷黄宅\\01.饰施图\\01.辅助文件\\图框\\A3图框.dwg"
    /// ]]>
    private static string? GetCompletePath(string directory, string relativePath)
    {
        if (relativePath.Trim() == string.Empty)
            return null;

        var relativeName = Path.GetDirectoryName(relativePath);
        if (relativeName is null)
            return null;

        if (relativePath[0] != '.')
            return relativePath;

        const char slash = '\\';

        // 判断向上删除几个
        var slashes = relativeName.Split(slash);
        var index = 0;
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < slashes.Length; i++)
        {
            if (slashes[i] != "..")
                break;
            index++;
        }

        var result = new StringBuilder();
        // 前段
        var pathDwgs = directory.Split(slash);
        pathDwgs = pathDwgs.Where(s => !string.IsNullOrEmpty(s)).ToArray(); // 清理空数组
        for (var i = 0; i < pathDwgs.Length - index; i++)
        {
            result.Append(pathDwgs[i]);
            result.Append(slash);
        }

        // 后段
        for (var i = 0; i < slashes.Length; i++)
        {
            var item = slashes[i];
            if (item != "." && item != "..")
            {
                result.Append(item);
                result.Append(slash);
            }
        }

        result.Append(Path.GetFileName(relativePath));
        return result.ToString();
    }

    #endregion
}
