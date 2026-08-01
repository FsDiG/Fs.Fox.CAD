namespace Fs.Fox.Cad;

/// <summary>
/// 提供Document类型的扩展方法来方便创建DocumentLockManager实例。
/// </summary>
public static class DocumentLockManagerExtension
{
    /// <summary>
    /// 安全锁定文档，返回一个新的DocumentLockManager实例。
    /// </summary>
    /// <param name="doc">需要进行锁定的文档。</param>
    /// <returns>DocumentLockManager实例，用于管理文档锁。</returns>
    public static DocumentLockManager SecurelyLock(this Document doc)
    {
        // 创建并返回DocumentLockManager实例。
        return new DocumentLockManager(doc);
    }
}
