namespace Fs.Fox.Cad;

/// <summary>
/// 向量扩展类
/// </summary>
public static class VectorEx
{
    /// <summary>
    /// 转换为2d向量
    /// </summary>
    /// <param name="vector3d">3d向量</param>
    /// <returns>2d向量</returns>
    public static Vector2d Convert2d(this Vector3d vector3d)
    {
        return new Vector2d(vector3d.X, vector3d.Y);
    }

    /// <summary>
    /// 转换为3d向量
    /// </summary>
    /// <param name="vector2d">2d向量</param>
    /// <param name="z">z值</param>
    /// <returns>3d向量</returns>
    public static Vector3d Convert3d(this Vector2d vector2d, double z = 0)
    {
        return new Vector3d(vector2d.X, vector2d.Y, z);
    }

    /// <summary>
    /// 2d叉乘
    /// </summary>
    /// <param name="a">向量a</param>
    /// <param name="b">向量b</param>
    /// <returns>叉乘值</returns>
    public static double Cross2d(this Vector3d a, Vector3d b)
    {
        return a.X * b.Y - b.X * a.Y;
    }

    /// <summary>
    /// 2d叉乘
    /// </summary>
    /// <param name="a">向量a</param>
    /// <param name="b">向量b</param>
    /// <returns>叉乘值</returns>
    public static double Cross2d(this Vector2d a, Vector2d b)
    {
        return a.X * b.Y - b.X * a.Y;
    }

    /// <summary>
    /// 向量Z值归零
    /// </summary>
    /// <param name="vector3d">向量</param>
    /// <returns></returns>
    public static Vector3d Z20(this Vector3d vector3d)
    {
        return new Vector3d(vector3d.X, vector3d.Y, 0);
    }

    /// <summary>
    /// 向量在平面上的弧度
    /// </summary>
    /// <param name="vector">向量</param>
    /// <param name="plane">平面</param>
    /// <returns>弧度</returns>
    public static double AngleOnPlane(this Vector3d vector, Plane? plane = null)
    {
        return vector.AngleOnPlane(plane ?? PlaneEx.Z);
    }
}
