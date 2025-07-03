using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace DotNetARX
{
    /// <summary>
    /// 外部参照操作类
    /// </summary>
    public static class XrefTools
    {
        /// <summary>
        /// 将外部参照插入到图形
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="fileName">包含完整路径的外部参照文件名</param>
        /// <param name="blockName">参照名</param>
        /// <param name="insertionPoint">外部参照块的插入点</param>
        /// <param name="scaleFactors">外部参照块的缩放因子</param>
        /// <param name="rotation">外部参照块的旋转角度，以弧度表示</param>
        /// <param name="isOverlay">外部参照块的类型，为true时表示覆盖，为false时表示附着</param>
        /// <returns></returns>
        public static ObjectId AttachXref(this Database db, string fileName, string blockName, Point3d insertionPoint, Scale3d scaleFactors, double rotation, bool isOverlay)
        {
            ObjectId xrefId=ObjectId.Null;//外部参照的Id
            //选择以覆盖的方式插入外部参照
            if (isOverlay) xrefId = db.OverlayXref(fileName, blockName);
            //选择以附着的方式插入外部参照
            else xrefId = db.AttachXref(fileName, blockName);
            //根据外部参照创建一个块参照，并指定其插入点
            BlockReference bref=new BlockReference(insertionPoint, xrefId);
            bref.ScaleFactors = scaleFactors;//外部参照块的缩放因子
            bref.Rotation = rotation;//外部参照块的旋转角度
            db.AddToModelSpace(bref);//将外部参照块添加到模型空间
            return xrefId;//返回外部参照的Id
        }
    }
}
