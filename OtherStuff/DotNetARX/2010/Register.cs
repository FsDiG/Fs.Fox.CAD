using System.Linq;
using Microsoft.Win32;

namespace DotNetARX
{
    /// <summary>
    /// AutoCAD注册表操作类
    /// </summary>
    public class Register
    {
        /// <summary>
        /// 获取当前AutoCAD的主版本号
        /// </summary>
        /// <returns>返回当前AutoCAD的主版本号</returns>
        public static string GetACDBDLL_NAME()
        {
            // 获取AutoCAD的版本号
            string acadVer = GetAutoCADVersion();
            if (acadVer.Contains("R16"))
            {
                return "R16";
            }
            else if (acadVer.Contains("R17"))
            {
                return "R17";
            }
            else if (acadVer.Contains("R18"))
            {
                return "R18";
            }
            else if (acadVer.Contains("R19"))
            {
                return "R19";
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// 获取AutoCAD的注册表键值
        /// </summary>
        /// <returns>返回AutoCAD的注册表键值</returns>
        public static string GetAutoCADKey()
        {
            // 获取当前用户的注册表键值
            RegistryKey hive = Registry.CurrentUser;
            //打开AutoCAD的注册表键值
            RegistryKey ack = hive.OpenSubKey("Software\\Autodesk\\AutoCAD");
            using (ack)
            {
                // 获取AutoCAD主版本的注册表键值
                string ver = ack.GetValue("CurVer") as string;
                if (ver == null)
                {
                    return "";
                }
                else
                {
                    RegistryKey verk = ack.OpenSubKey(ver);
                    using (verk)
                    {
                        // 获取对应语言版本的注册表键值
                        string lng = verk.GetValue("CurVer") as string;
                        if (lng == null)
                        {
                            return "";
                        }
                        else
                        {
                            RegistryKey lngk = verk.OpenSubKey(lng);
                            using (lngk)
                            {
                                // 返回无前缀的注册表键值
                                return lngk.Name.Substring(hive.Name.Length + 1);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取AutoCAD的版本号
        /// </summary>
        /// <returns></returns>
        public static string GetAutoCADVersion()
        {
            return GetAutoCADKey().Split('\\')[3];
        }

        /// <summary>
        /// 获取AutoCAD所属的注册表键名
        /// </summary>
        /// <returns>返回AutoCAD所属的注册表键名</returns>
        public static string GetAutoCADKeyName()
        {
            // 获取HKEY_CURRENT_USER键
            RegistryKey keyCurrentUser = Registry.CurrentUser;
            // 打开AutoCAD所属的注册表键:HKEY_CURRENT_USER\Software\Autodesk\AutoCAD
            RegistryKey keyAutoCAD = keyCurrentUser.OpenSubKey("Software\\Autodesk\\AutoCAD");
            //获得表示当前的AutoCAD版本的注册表键值:R18.2
            string valueCurAutoCAD = keyAutoCAD.GetValue("CurVer").ToString();
            if (valueCurAutoCAD == null) return "";//如果未安装AutoCAD，则返回
            //获取当前的AutoCAD版本的注册表键:HKEY_LOCAL_MACHINE\Software\Autodesk\AutoCAD\R18.2
            RegistryKey keyCurAutoCAD = keyAutoCAD.OpenSubKey(valueCurAutoCAD);
            //获取表示AutoCAD当前语言的注册表键值:ACAD-a001:804
            string language = keyCurAutoCAD.GetValue("CurVer").ToString();
            //获取AutoCAD当前语言的注册表键:HKEY_LOCAL_MACHINE\Software\Autodesk\AutoCAD\R18.2\ACAD-a001:804
            RegistryKey keyLanguage = keyCurAutoCAD.OpenSubKey(language);
            //返回去除HKEY_LOCAL_MACHINE前缀的当前AutoCAD注册表项的键名:Software\Autodesk\AutoCAD\R18.2\ACAD-a001:804
            return keyLanguage.Name.Substring(keyCurrentUser.Name.Length + 1);
        }

        /// <summary>
        /// 创建自动加载.NET程序所需要的注册表项
        /// </summary>
        /// <param name="appName">.NET程序名</param>
        /// <param name="appDesc">描述程序用途的字符串</param>
        /// <param name="appPath">.NET程序的路径</param>
        /// <param name="currentUser">注册表项是创建在HKEY_CURRENT_USER键下还是HKEY_LOCAL_MACHINE健下</param>
        /// <param name="overwrite">是否覆盖同名的程序</param>
        /// <param name="flagLOADCTRLS">LOADCTRLS键的值，用来描述装载程序的原因</param>
        /// <returns>如果创建注册表项成功则返回true,否则返回false</returns>
        public static bool CreateDemandLoadingEntries(string appName, string appDesc, string appPath, bool currentUser, bool overwrite, int flagLOADCTRLS)
        {
            //获取AutoCAD所属的注册表键名
            var autoCADKeyName = GetAutoCADKeyName();
            //确定是HKEY_CURRENT_USER还是HKEY_LOCAL_MACHINE
            RegistryKey keyRoot = currentUser ? Registry.CurrentUser : Registry.LocalMachine;
            // 由于某些AutoCAD版本的HKEY_CURRENT_USER可能不包括Applications键值，因此要创建该键值
            // 如果已经存在该鍵，无须担心可能的覆盖操作问题，因为CreateSubKey函数会以写的方式打开它而不会执行覆盖操作
            RegistryKey keyApp = keyRoot.CreateSubKey(autoCADKeyName + "\\" + "Applications");
            //若存在同名的程序且选择不覆盖则返回
            if (!overwrite && keyApp.GetSubKeyNames().Contains(appName))
                return false;
            //创建相应的键并设置自动加载应用程序的选项
            RegistryKey keyUserApp = keyApp.CreateSubKey(appName);
            keyUserApp.SetValue("DESCRIPTION", appName, RegistryValueKind.String);
            keyUserApp.SetValue("LOADCTRLS", flagLOADCTRLS, RegistryValueKind.DWord);
            keyUserApp.SetValue("LOADER", appPath, RegistryValueKind.String);
            keyUserApp.SetValue("MANAGED", 1, RegistryValueKind.DWord);
            return true;//创建键成功则返回
        }

        /// <summary>
        /// 删除自动加载.NET程序所需要的注册表项
        /// </summary>
        /// <param name="appName">.NET程序名</param>
        /// <param name="currentUser">注册表项是创建在HKEY_CURRENT_USER键下还是HKEY_LOCAL_MACHINE健下</param>
        /// <returns>如果删除注册表项成功则返回true，否则返回false</returns>
        public static bool RemoveDemandLoadingEntries(string appName, bool currentUser)
        {
            try
            {
                // 获取AutoCAD所属的注册表键名
                string cadName = GetAutoCADKeyName();
                // 确定是HKEY_CURRENT_USER还是HKEY_LOCAL_MACHINE
                RegistryKey keyRoot = currentUser ? Registry.CurrentUser : Registry.LocalMachine;
                // 以写的方式打开Applications注册表键
                RegistryKey keyApp = keyRoot.OpenSubKey(cadName + "\\" + "Applications", true);
                //删除指定名称的注册表键
                keyApp.DeleteSubKeyTree(appName);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
