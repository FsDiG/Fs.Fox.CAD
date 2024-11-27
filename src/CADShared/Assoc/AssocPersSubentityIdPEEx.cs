namespace IFoxCAD.Cad.Assoc;

/// <summary>
/// 子对象关系Id扩展
/// </summary>
public static class AssocPersSubentityIdPEEx
{
    private static readonly RXClass _acdbAssocPersSubentityIdPEClass = RXObject.GetClass(typeof(AssocPersSubentityIdPE));

    /// <summary>
    /// 获取实体的个性化子对象关系Id
    /// </summary>
    /// <param name="entity">要查询的实体</param>
    /// <returns>返回个性化子对象关系Id实例，如果不存在则返回null</returns>
    public static AssocPersSubentityIdPE? GetPersSubentityIdPE(this Entity entity)
    {
        var intPtr = entity.QueryX(_acdbAssocPersSubentityIdPEClass);
        return RXObject.Create(intPtr, false) as AssocPersSubentityIdPE;
    }

    /// <summary>
    /// 获取实体中所有指定类型的子对象Id
    /// </summary>
    /// <param name="entity">要查询的实体</param>
    /// <param name="subentityType">子对象的类型</param>
    /// <returns>返回所有子对象Id的数组</returns>
    public static SubentityId[] GetAllSubentityIds(this Entity entity, SubentityType subentityType)
    {
        var assocPersSubentityIdPE = entity.GetPersSubentityIdPE();
        return assocPersSubentityIdPE is null
            ? []
            : assocPersSubentityIdPE.GetAllSubentities(entity, subentityType);
    }

    /// <summary>
    /// 获取实体中所有指定类型的子对象
    /// </summary>
    /// <param name="entity">要查询的实体</param>
    /// <param name="subentityType">子对象的类型</param>
    /// <returns>返回所有子对象的列表</returns>
    public static List<Entity> GetAllSubentities(this Entity entity, SubentityType subentityType)
    {
        var subentityIds = entity.GetAllSubentityIds(subentityType);

        List<Entity> result = [];
        foreach (var subentityId in subentityIds)
        {
            var fullSubentityPath = new FullSubentityPath([entity.ObjectId], subentityId);
            // 这里会有get不到的情况
            // 比如一条line，获取edge能获取到有一个子边，但是实际是取不到的
            // 可能cad认为子边和自身一样没必要再返回
            if (entity.GetSubentity(fullSubentityPath) is not { } subentity)
                continue;
            result.Add(subentity);
        }

        return result;
    }
}