using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsSystem;



namespace NFox.Cad
{
    /// <summary>
    /// 工具函数类
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// 字符串重复，支持中文
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="n">重复的次数</param>
        /// <returns></returns>
        public static string Repeat(this string str, int n)
        {
            char[] arr = str.ToCharArray();
            char[] arrDest = new char[arr.Length * n];

            for (int i = 0; i < n; i++)
            {
                Buffer.BlockCopy(arr, 0, arrDest, i * arr.Length * 2, arr.Length * 2);
            }

            return new string(arrDest);
        }

        [DllImport("kernel32.dll", EntryPoint = "_lopen")]
        private static extern IntPtr Lopen(string lpPathName, int iReadWrite);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        /// <summary>
        /// 文件是否打开
        /// </summary>
        /// <param name="filename">文件路径</param>
        /// <returns></returns>
        public static bool IsOpened(string filename)
        {
            const int OF_READWRITE = 2;
            const int OF_SHARE_DENY_NONE = 0x40;
            IntPtr HFILE_ERROR = new IntPtr(-1);

            IntPtr handle = Lopen(filename, OF_READWRITE | OF_SHARE_DENY_NONE);
            if (handle == HFILE_ERROR)
            {
                return true;
            }
            CloseHandle(handle);
            return false;
        }

        /// <summary>
        /// 打开注册表项
        /// </summary>
        /// <param name="root">根键</param>
        /// <param name="keys">子项逐级列表</param>
        /// <example>GetRegistryKey(root, sub1, sub2, sub3) === root/sub1/sub2/sub3</example>
        /// <returns></returns>
        public static RegistryKey GetRegistryKey(RegistryKey root, params string[] keys)
        {
            RegistryKey rk = root;
            foreach (string key in keys)
            {
                try
                {
                    rk = rk.OpenSubKey(key);
                }
                catch
                {
                    return null;
                }
            }
            return rk;
        }

        #region Resource

        /// <summary>
        /// 从资源里读取字符串
        /// </summary>
        /// <param name="path">要在其中搜索资源的目录的名称。 path 可以是绝对路径或应用程序目录中的相对路径。</param>
        /// <param name="filename">资源的根名称。 例如，名为“MyResource.en-US.resources”的资源文件的根名称为“MyResource”</param>
        /// <param name="key">字符串的名字</param>
        /// <returns></returns>
        public static string GetStringFormResource(string path, string filename, string key)
        {
            ResourceManager rm = ResourceManager.CreateFileBasedResourceManager(filename, path, null);
            return rm.GetString(key);
        }

        /// <summary>
        /// 获取资源中的图片
        /// </summary>
        /// <param name="path">要在其中搜索资源的目录的名称。 path 可以是绝对路径或应用程序目录中的相对路径。</param>
        /// <param name="filename">资源的根名称。 例如，名为“MyResource.en-US.resources”的资源文件的根名称为“MyResource”</param>
        /// <param name="key">图片的名字</param>
        /// <returns></returns>
        public static Bitmap GetImageFormResource(string path, string filename, string key)
        {
            ResourceManager rm = ResourceManager.CreateFileBasedResourceManager(filename, path, null);
            return (Bitmap)rm.GetObject(key);
        }

        /// <summary>
        /// xml转表的函数
        /// </summary>
        /// <param name="xml">xml字符串</param>
        /// <returns></returns>
        public static DataTableCollection GetTablesFormXmlString(string xml)
        {
            DataSet ds = new DataSet();
            StringReader sr = new StringReader(xml);
            ds.ReadXml(sr);
            return ds.Tables;
        }

        /// <summary>
        /// xml 转表函数
        /// </summary>
        /// <param name="filename">xml文件路径</param>
        /// <returns></returns>
        public static DataTableCollection GetTablesFormXml(string filename)
        {
            DataSet ds = new DataSet();
            ds.ReadXml(filename);
            return ds.Tables;
        }

        /// <summary>
        /// 创建资源文件
        /// </summary>
        /// <param name="sourcepath">存放资源(如图片，xml之类)的路径</param>
        /// <param name="destpath">生成资源文件的路径</param>
        /// <returns></returns>
        public static bool MakeResources(string sourcepath, string destpath)
        {
            if (sourcepath[sourcepath.Length - 1] != '\\')
            {
                sourcepath = sourcepath + "\\";
            }

            DirectoryInfo di = new DirectoryInfo(sourcepath);
            if (di == di.Root)
            {
                return false;
            }
            else
            {
                try
                {
                    foreach (FileInfo dllfile in di.GetFiles("*.dll"))
                    {
                        File.Copy(dllfile.FullName, Directory.GetParent(destpath).FullName + "\\" + dllfile.Name, true);
                    }
                }
                catch
                { }

                if (destpath[destpath.Length - 1] != '\\')
                {
                    destpath = destpath + "\\";
                }
                string filename = destpath + di.Name + ".resources";
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }

                ResourceWriter rw = new ResourceWriter(filename);
                foreach (FileInfo jpgfile in di.GetFiles("*.jpg"))
                {
                    System.Drawing.Image img = System.Drawing.Image.FromFile(jpgfile.FullName);
                    rw.AddResource(jpgfile.Name, img);
                }

                foreach (FileInfo xmlfile in di.GetFiles("*.xml"))
                {
                    string s = File.ReadAllText(xmlfile.FullName);
                    rw.AddResource(xmlfile.Name, s);
                }

                rw.Generate();
                rw.Close();

                return true;
            }
        }

        #endregion Resource

        private static void Test()
        {
            Polyline2d pl2d = new Polyline2d();
            int n = pl2d.Cast<ObjectId>().Count();
        }

        //public static void Export(List<Entity> entitys, string fileName, ImageFormat format)
        //{
        //    Document doc = Application.DocumentManager.MdiActiveDocument;
        //    Manager gsm = doc.GraphicsManager;

        //    using (View view = new View())
        //    {
        //        //获取当前视口属性
        //        gsm.SetViewFromViewport(
        //            view,
        //            Convert.ToInt32(Application.GetSystemVariable("CVPORT")));

        //        using (Device dev = gsm.CreateAutoCADOffScreenDevice())
        //        {
        //            using (Model model = gsm.CreateAutoCADModel())
        //            {
        //                //获取实体集合的范围
        //                Extents3d ext = new Extents3d();
        //                foreach (Entity ent in entitys)
        //                {
        //                    Entity entity =(Entity)ent.Clone();
        //                    entity.SetDatabaseDefaults();
        //                    view.Add(entity, model);
        //                    ext.AddExtents(ent.GeometricExtents);
        //                }

        //                //设置视口中心、范围、方向
        //                Point3d maxpoint = ext.MaxPoint;
        //                Point3d minpoint = ext.MinPoint;
        //                Point3d center = minpoint + 0.5 * (maxpoint - minpoint);
        //                view.SetView(
        //                    center + Vector3d.ZAxis,
        //                    center,
        //                    Vector3d.YAxis,
        //                    maxpoint.X - minpoint.X,
        //                    maxpoint.Y - minpoint.Y);
        //                view.Invalidate();
        //                view.Update();

        //                Point pnt = new Point(0, 0);
        //                Size size = GetSize(ext);
        //                dev.OnSize(size);
        //                dev.DeviceRenderType = RendererType.Default;
        //                dev.BackgroundColor = Color.White;
        //                dev.Add(view);
        //                dev.Update();

        //                using (Bitmap bitmap = view.GetSnapshot(new Rectangle(pnt, size)))
        //                {
        //                    bitmap.Save(fileName, format);
        //                    // Clean up
        //                    view.EraseAll();
        //                    dev.Erase(view);
        //                }
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// 缩放视图到指定的坐标范围
        /// </summary>
        /// <param name="view">视图对象</param>
        /// <param name="ext">范围坐标</param>
        public static void SetViewTo(View view, Extents3d ext)
        {
            double height = 0.0, width = 0.0, viewTwist = 0.0;
            Point3d targetView = new Point3d();
            Vector3d viewDir = new Vector3d();

            GsUtility.GetActiveViewPortInfo(ref height, ref width, ref targetView, ref viewDir, ref viewTwist, true);
            // from the data returned let's work out the viewmatrix
            viewDir = viewDir.GetNormal();
            Vector3d viewXDir = viewDir.GetPerpendicularVector().GetNormal();
            viewXDir = viewXDir.RotateBy(viewTwist, -viewDir);
            Vector3d viewYDir = viewDir.CrossProduct(viewXDir);
            Point3d boxCenter = ext.MinPoint + 0.5 * (ext.MaxPoint - ext.MinPoint);
            Matrix3d viewMat =
                Matrix3d.AlignCoordinateSystem(
                    boxCenter,
                    Vector3d.XAxis,
                    Vector3d.YAxis,
                    Vector3d.ZAxis,
                    boxCenter,
                    viewXDir,
                    viewYDir,
                    viewDir).Inverse();
            Extents3d viewExtents = ext;
            viewExtents.TransformBy(viewMat);
            double xMax = System.Math.Abs(viewExtents.MaxPoint.X - viewExtents.MinPoint.X);
            double yMax = System.Math.Abs(viewExtents.MaxPoint.Y - viewExtents.MinPoint.Y);
            Point3d eye = boxCenter + viewDir;
            // finally set the Gs view to the dwg view
            view.SetView(eye, boxCenter, viewYDir, xMax, yMax);
        }
    }
}