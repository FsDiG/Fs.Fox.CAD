using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
namespace DotNetARX
{

    /// <summary>
    /// P/Invoke操作类
    /// </summary>
    public static class PInvoke
    {
        #region 动态P/Invoke
        private const uint LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008;

        //允许托管代码不进行安全检查即调入非托管代码,从而提高程序的运行效率。只须在类内声明一次。
        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool FreeLibrary(IntPtr dllPointer);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetProcAddress(IntPtr dllPointer, string functionName);

        /// <summary>
        /// 判断系统是32位还是64位
        /// </summary>
        public static bool Is32Bit { get { return 4 == IntPtr.Size; } }

        /// <summary>
        /// 将非托管函数指针转换为委托。
        /// </summary>
        /// <param name="acdb">true表示非托管函数为acdb**.dll中定义的，false表示函数为acad.exe中定义的</param>
        /// <param name="funcName32">32位函数的重整名称</param>
        /// <param name="funcName64">64位函数的重整名称</param>
        /// <param name="t">要返回的委托的类型</param>
        /// <returns>委托实例，可强制转换为适当的委托类型。</returns>
        public static Delegate GetDelegateForFunction(bool acdb, string funcName32, string funcName64, Type t)
        {
            Delegate delegateFunc = null; // 用来作为返回值的代理对象
            string lpFileName = "acad.exe"; // 缺省认为函数位于acad.exe中
            // 根据系统的位数（32或64位）设定要调用的函数名
            string functionName = Is32Bit ? funcName32 : funcName64;
            if (acdb) // 如果非托管函数为acdb**.dll中定义的
            {
                // 获取acdb**.dll的主版本号
                string dllName = Register.GetACDBDLL_NAME();
                switch (dllName) //根据主版本号确定acdb**.dll的具体名称
                {
                    case "R17":
                        lpFileName = "acdb17.dll";
                        break;
                    case "R18":
                        lpFileName = "acdb18.dll";
                        break;
                    case "R19":
                        lpFileName = "acdb19.dll";
                        break;
                }
            }
            // 装载指定的动态链接库（acad.exe或acdb**.dll）
            IntPtr moduleHandle = LoadLibraryEx(lpFileName, IntPtr.Zero, LOAD_WITH_ALTERED_SEARCH_PATH);
            if (moduleHandle != IntPtr.Zero) // 如果装载成功
            {
                // 检索指定的动态链接库中的输出库函数地址
                IntPtr pProc = GetProcAddress(moduleHandle, functionName);
                if (pProc != IntPtr.Zero) // 如果找到
                {
                    // 将非托管函数指针转换为委托
                    delegateFunc = Marshal.GetDelegateForFunctionPointer(pProc, t);
                }
                FreeLibrary(moduleHandle); // 释放动态链接库
            }
            return delegateFunc; // 返回委托
        }
        #endregion

        [DllImport("acad.exe", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern int acedGetEnv(string envName, StringBuilder result);
        /// <summary>
        /// 获取环境变量值
        /// </summary>
        /// <param name="ed">命令行对象，无意义，只是为了创建扩展函数</param>
        /// <param name="symbol">环境变量名</param>
        /// <returns>返回环境变量值</returns>
        public static string GetEnvVariable(this Editor ed, string symbol)
        {
            StringBuilder ret = new StringBuilder(1024);
            acedGetEnv(symbol, ret);
            return ret.ToString();
        }

        [DllImport("acad.exe", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern int acedSetEnv(string envName, StringBuilder value);
        /// <summary>
        /// 设置环境变量值
        /// </summary>
        /// <param name="ed">命令行对象，无意义，只是为了创建扩展函数</param>
        /// <param name="symbol">环境变量名</param>
        /// <param name="value">要设置的值</param>
        /// <returns>如果设置成功，则返回true,否则返回false</returns>
        public static bool SetEnvVariable(this Editor ed, string symbol, string value)
        {
            StringBuilder newValue = new StringBuilder(value);
            if (acedSetEnv(symbol, newValue) == 5100)
                return true;
            else return false;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool acdbGetPreviewBitmapFromDwg([MarshalAs(UnmanagedType.LPWStr)] string pszDwgfilename, ref IntPtr pPreviewBmp, ref IntPtr pRetPal);
        /// <summary>
        /// 获取dwg文件的预览图案
        /// </summary>
        /// <param name="db">数据库对象，无意义，只是为了创建扩展函数</param>
        /// <param name="fileName">dwg文件名</param>
        /// <returns>返回dwg文件的预览图案</returns>
        public static Bitmap GetPreviewBitmapFromDwg(this Database db, string fileName)
        {
            // 初始化IntPtr
            IntPtr hBmp = default(IntPtr);
            IntPtr hPal = default(IntPtr);
            // 32位和64位acdbGetPreviewBitmapFromDwg函数的重整名称
            string func32 = "?acdbGetPreviewBitmapFromDwg@@YA_NPB_WPAPAUHBITMAP__@@PAPAUHPALETTE__@@@Z";
            string func64 = "?acdbGetPreviewBitmapFromDwg@@YA_NPEB_WPEAPEAUHBITMAP__@@PEAPEAUHPALETTE__@@@Z";
            // 获取表示acdbGetPreviewBitmapFromDwg函数的委托
            acdbGetPreviewBitmapFromDwg delegateFunc = (acdbGetPreviewBitmapFromDwg)GetDelegateForFunction(true, func32, func64, typeof(acdbGetPreviewBitmapFromDwg));
            // 调用委托，执行acdbGetPreviewBitmapFromDwg函数
            delegateFunc(fileName, ref hBmp, ref hPal);
            if (hBmp.Equals(IntPtr.Zero)) // 如果没有预览图则返回空
            {
                return null;
            }
            else
            {
                return Bitmap.FromHbitmap(hBmp); // 返回预览图
            }
        }
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void addPersistentReactor(IntPtr dbObject, ObjectId objId);
        /// <summary>
        /// 为对象添加永久反应器
        /// </summary>
        /// <param name="dbObject">对象</param>
        /// <param name="objId">永久反应器的Id</param>
        public static void AddPersistentReactor(this DBObject dbObject, ObjectId objId)
        {
            string func = "?addPersistentReactor@AcDbObject@@UAEXVAcDbObjectId@@@Z";
            addPersistentReactor delegateFunc = (addPersistentReactor)GetDelegateForFunction(true, func, func, typeof(addPersistentReactor));
            delegateFunc(dbObject.UnmanagedObject, objId);
        }

        [DllImport("acad.exe", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acedGenerateThumbnailBitmap@@YAPAUtagABITMAPINFO@@XZ")]
        private static extern IntPtr acedGenerateThumbnailBitmap();
        /// <summary>
        /// 为当前图形生成缩略图
        /// </summary>
        /// <param name="ed">命令行对象，无意义，只是为了创建扩展函数</param>
        public static void GenerateThumbnailBitmap(this Editor ed)
        {
            acedGenerateThumbnailBitmap();
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool annotativeDwg(IntPtr pdb);
        /// <summary>
        /// 当图形数据库插入到另外一个图形时，是否将其看作为可缩放块
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <returns>若为true,则表示将数据库看为可缩放块</returns>
        public static bool AnnotativeDwg(this Database db)
        {
            // 32位和64位annotativeDwg函数的重整名称
            string func32 = "?annotativeDwg@AcDbDatabase@@QBE_NXZ";
            string func64 = "?annotativeDwg@AcDbDatabase@@QEBA_NXZ";
            // 获取表示annotativeDwg函数的委托
            annotativeDwg delegateFunc = (annotativeDwg)GetDelegateForFunction(true, func32, func64, typeof(annotativeDwg));
            // 调用委托，执行annotativeDwg函数
            return delegateFunc(db.UnmanagedObject);            
        }
    }
}
