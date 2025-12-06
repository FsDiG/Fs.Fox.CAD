using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace DotNetARX
{
    /// <summary>
    /// 选择操作类
    /// </summary>
    public static class SelectionTools
    {
        /// <summary>
        /// 选择过一点的所有实体
        /// </summary>
        /// <param name="ed">命令行对象</param>
        /// <param name="point">点</param>
        /// <param name="filter">选择过滤器</param>
        /// <returns>返回过指定点的所有实体</returns>
        public static PromptSelectionResult SelectAtPoint(this Editor ed, Point3d point, SelectionFilter filter)
        {
            return ed.SelectCrossingWindow(point, point, filter);
        }

        /// <summary>
        /// 选择过一点的所有实体
        /// </summary>
        /// <param name="ed">命令行对象</param>
        /// <param name="point">点</param>
        /// <returns>返回过指定点的所有实体</returns>
        public static PromptSelectionResult SelectAtPoint(this Editor ed, Point3d point)
        {
            return ed.SelectCrossingWindow(point, point);
        }

    }
}
