using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
namespace DotNetARX
{
    /// <summary>
    /// 摘要信息操作类
    /// </summary>
    public static class SummaryInfoTools
    {
        /// <summary>
        /// 获取摘要信息中的自定义属性个数
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <returns>返回自定义属性个数</returns>
        public static int NumCustomProperties(this Database db)
        {
            DatabaseSummaryInfo info = db.SummaryInfo; //获取数据库摘要信息
            //获取摘要信息中的自定义属性集合
            System.Collections.IDictionaryEnumerator props = info.CustomProperties;
            int count = 0;//计数器，用于统计自定义属性个数
            while (props.MoveNext())//遍历自定义属性
            {
                count++;//计数器累加
            }
            return count;//返回自定义属性个数
        }

        /// <summary>
        /// 判断图形中是否存在指定的自定义属性
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="key">自定义属性的名称</param>
        /// <returns>如果存在指定的自定义属性，返回true，否则返回false</returns>
        public static bool HasCustomProperty(this Database db, string key)
        {
            DatabaseSummaryInfo info = db.SummaryInfo; //获取数据库摘要信息
            //获取摘要信息中的自定义属性集合
            System.Collections.IDictionaryEnumerator props = info.CustomProperties;
            while (props.MoveNext())//遍历自定义属性
            {
                //如果存在指定的自定义属性，返回true
                if (props.Key.ToString().ToUpper() == key.ToUpper()) return true;
            }
            return false;//不存在指定的自定义属性，返回false
        }

        /// <summary>
        /// 判断图形中是否存在摘要信息
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <returns>如果存在摘要信息，返回true，否则返回false</returns>
        public static bool HasSummaryInfo(this Database db)
        {
            //如果存在自定义属性，则说明肯定有摘要信息
            if (db.NumCustomProperties() > 0) return true;
            DatabaseSummaryInfo info = db.SummaryInfo;//数据库的摘要信息
            //如果存在摘要信息，则返回true
            if (!info.Author.IsNullOrWhiteSpace() && !info.Comments.IsNullOrWhiteSpace()
                && !info.HyperlinkBase.IsNullOrWhiteSpace() && !info.Keywords.IsNullOrWhiteSpace()
                && !info.RevisionNumber.IsNullOrWhiteSpace()
                && !info.Subject.IsNullOrWhiteSpace() && !info.Title.IsNullOrWhiteSpace())
                return true;
            return false;//不存在摘要信息，返回false
        }

        /// <summary>
        /// 获取图形创建的时间
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <returns>返回图形创建的时间</returns>
        public static DateTime CreationTime(this Database db)
        {
            //获取TDCREATE系统变量，该变量存储图形的创建时间（儒略日格式）
            double creationTime = (double)Application.GetSystemVariable("TDCREATE");
            //将儒略日时间格式转换为DateTime格式，并返回
            return ConvertAcadJulianToDateTime(creationTime);
        }

        /// <summary>
        /// 获取图形的上次修改时间
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <returns>返回图形的上次修改时间</returns>
        public static DateTime ModifyTime(this Database db)
        {
            //获取TDUPDATE系统变量，该变量存储图形上次修改的时间（儒略日格式）
            double creationTime = (double)Application.GetSystemVariable("TDUPDATE");
            //将儒略日时间格式转换为DateTime格式，并返回
            return ConvertAcadJulianToDateTime(creationTime);
        }

        /// <summary>
        /// 获取编辑图形的总时间
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <returns>返回编辑图形的总时间</returns>
        public static TimeSpan TotalEditTime(this Database db)
        {
            //获取TDINDWG系统变量，该变量存储编辑图形的总时间，该时间格式为<天数>.<天的小数部分> 
            double day = (double)Application.GetSystemVariable("TDINDWG");
            return TimeSpan.FromDays(day);//返回一个时间间隔对象
        }

        /// <summary>
        /// 将儒略日时间格式转换为DateTime格式
        /// </summary>
        /// <param name="julianDate">儒略日时间</param>
        /// <returns>返回DateTime格式的时间</returns>
        public static DateTime ConvertAcadJulianToDateTime(double julianDate)
        {
            DateTime date;
            try
            {
                double z = Math.Floor(julianDate);
                double w = Math.Floor((z - 1867216.25) / 36524.25);
                double x = Math.Floor(w / 4);

                double a = (z + 1 + w - x);
                double b = a + 1524;
                double c = Math.Floor((b - 122.1) / 365.25);

                double d = Math.Floor(365.25 * c);
                double e = Math.Floor((b - d) / 30.6001);
                double f = Math.Floor(30.6001 * e);

                int day = Convert.ToInt32(b - d - f);

                int m = Convert.ToInt32((e < 14) ? (e - 2) : (e - 14));
                int month = (m + 1);

                int y = Convert.ToInt32((m > 1) ? (c - 4716) : (c - 4715));
                int year = (y == 0) ? (y - 1) : y;
                //
                // strip the integer
                double t = (julianDate - z);
                // hours since midnight
                double hours = Math.Floor(t * 24.0);
                // temp value
                double mins = t - (hours / 24.0);
                // 1440 minutes per day
                double minutes = Math.Floor(mins * 1440.0);
                //
                double seconds = Math.Floor((mins - (minutes / 1440.0)) * 86400.0);

                date = new DateTime(year, month, day, Convert.ToInt32(hours), Convert.ToInt32(minutes), Convert.ToInt32(seconds));
                return date;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Application.ShowAlertDialog(ex.Message);
                date = new DateTime(0);
            }
            catch (Exception ex)
            {
                Application.ShowAlertDialog(ex.Message);
                date = new DateTime(0);
            }
            return date;
        }
    }
}
