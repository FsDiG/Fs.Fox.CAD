namespace Fs.Fox.Cad;

/// <summary>
/// 自动切换活动数据库
/// </summary>
public class SwitchDatabase : IDisposable
{
    private readonly Database _db;

    /// <summary>
    /// 切换活动数据库
    /// </summary>
    /// <param name="database">当前数据库</param>
    public SwitchDatabase(Database database)
    {
        _db = HostApplicationServices.WorkingDatabase;
        HostApplicationServices.WorkingDatabase = database;
    }

    /// <summary>
    /// 恢复活动数据库为默认
    /// </summary>
    public void Dispose()
    {
        HostApplicationServices.WorkingDatabase = _db;
        GC.SuppressFinalize(this);
    }
}
