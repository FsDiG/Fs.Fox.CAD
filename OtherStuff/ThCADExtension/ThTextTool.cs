using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Text.RegularExpressions;

namespace ThCADExtension
{
    public static class ThTextTool
    {
        // 文字包围框
        //  https://adndevblog.typepad.com/autocad/2013/10/mirroring-a-dbtext-entity.html
        public static void GetTextBoxCorners(
            this DBText dbText,
            out Point3d pt1,
            out Point3d pt2,
            out Point3d pt3,
            out Point3d pt4)
        {
            InvokeTool.ads_name name = new InvokeTool.ads_name();
            int result = InvokeTool.acdbGetAdsName(ref name, dbText.ObjectId);

            ResultBuffer rb = new ResultBuffer();
            Interop.AttachUnmanagedObject(rb,InvokeTool.acdbEntGet(ref name), true);

            // Call imported arx function
            double[] point1 = new double[3];
            double[] point2 = new double[3];
            InvokeTool.acedTextBox(rb.UnmanagedObject, point1, point2);
            pt1 = new Point3d(point1);
            pt2 = new Point3d(point2);

            // Create rotation matrix
            Matrix3d rotMat = Matrix3d.Rotation(dbText.Rotation, dbText.Normal, pt1);

            // The returned points from acedTextBox need
            // to be transformed as follow
            pt1 = pt1.TransformBy(rotMat).Add(dbText.Position.GetAsVector());
            pt2 = pt2.TransformBy(rotMat).Add(dbText.Position.GetAsVector());

            Vector3d rotDir = new Vector3d(
                -Math.Sin(dbText.Rotation), 
                Math.Cos(dbText.Rotation), 
                0);
            Vector3d linDir = rotDir.CrossProduct(dbText.Normal);
            double actualWidth = Math.Abs((pt2.GetAsVector() - pt1.GetAsVector()).DotProduct(linDir));
            pt3 = pt1.Add(linDir * actualWidth);
            pt4 = pt2.Subtract(linDir * actualWidth);
        }
    }
}
