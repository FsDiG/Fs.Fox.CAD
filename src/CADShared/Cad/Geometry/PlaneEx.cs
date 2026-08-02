namespace Fs.Fox.Cad;

/// <summary>
/// 平面
/// </summary>
public static class PlaneEx
{
    /// <summary>
    /// X
    /// </summary>
    public static readonly Plane X = new(Point3d.Origin, Vector3d.XAxis);

    /// <summary>
    /// Y
    /// </summary>
    public static readonly Plane Y = new(Point3d.Origin, Vector3d.YAxis);

    /// <summary>
    /// Z
    /// </summary>
    public static readonly Plane Z = new(Point3d.Origin, Vector3d.ZAxis);
}
