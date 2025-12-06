using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ThCADExtension
{
    public class COMTool
    {
        public static void ZoomWindow(Point3d pt1, Point3d pt2)
        {
            //获取Application的COM对象
            Type comType = Type.GetTypeFromHandle(Type.GetTypeHandle(Application.AcadApplication));

            //通过后期绑定的方式调用ZoomWindow函数缩放窗口
            comType.InvokeMember("ZoomWindow", BindingFlags.InvokeMethod,
                null, Application.AcadApplication, new object[] { pt1.ToArray(), pt2.ToArray() });
        }
    }
}
