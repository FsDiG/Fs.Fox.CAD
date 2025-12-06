using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ThCADExtension
{
    public static class ThStringTools
    {
        public static string NumberToChinese(this int number)
        {
            string res = string.Empty;
            string str = number.ToString();
            string schar = str.Substring(0, 1);
            switch (schar)
            {
                case "1":
                    res = "一";
                    break;
                case "2":
                    res = "二";
                    break;
                case "3":
                    res = "三";
                    break;
                case "4":
                    res = "四";
                    break;
                case "5":
                    res = "五";
                    break;
                case "6":
                    res = "六";
                    break;
                case "7":
                    res = "七";
                    break;
                case "8":
                    res = "八";
                    break;
                case "9":
                    res = "九";
                    break;
                default:
                    res = "零";
                    break;
            }
            if (str.Length > 1)
            {
                switch (str.Length)
                {
                    case 2:
                    case 6:
                        res += "十";
                        break;
                    case 3:
                    case 7:
                        res += "百";
                        break;
                    case 4:
                        res += "千";
                        break;
                    case 5:
                        res += "万";
                        break;
                    default:
                        res += "";
                        break;
                }
                res += NumberToChinese(int.Parse(str.Substring(1, str.Length - 1)));
            }
            return res;
        }

        /// <summary>
        /// 从汉字字符串中截取第一个数字。
        /// </summary>
        /// <param name="src">源文本</param>
        /// <returns></returns>
        /// ————————————————
        /// 版权声明：本文为CSDN博主「刘超峰」的原创文章，遵循 CC 4.0 BY-SA 版权协议，转载请附上原文出处链接及本声明。
        /// 原文链接：https://blog.csdn.net/qq_16427421/article/details/82598050
        public static string SubNumber(this string src)
        {
            // 把源文本转换为字符数组
            char[] charArr = src.ToCharArray();
            // 定义一个新的数组，用于接收判断结果
            char[] newCharArr = new char[charArr.Length];
            // 定义数字字符串
            string numberString = "零一二三四五六七八九十百千";

            // 遍历charArr，用于判断每个字符是否为数字
            for (int i = 0; i < charArr.Length; i++)
            {
                // 如果字符为数字，则结果为1，否则结果为0
                newCharArr[i] = numberString.IndexOf(charArr[i]) != -1 ? '1' : '0';
            }
            // 把结果数组转换为字符串
            string str = new string(newCharArr);
            // 定义正则表达式，用于匹配数字
            Regex reg = new Regex("1+");
            // 定义变量获取匹配结果的索引
            Match m = reg.Match(str);
            // 判断是否匹配成功
            if (m.Success)
            {
                // 如果匹配成功，则根据匹配结果截取源文本中的数字
                return src.Substring(m.Index, m.Value.Length);
            }
            // 如果匹配失败，则返回空字符串
            return string.Empty;
        }

        /// <summary>
        /// 中文转数字
        /// </summary>
        /// <param name="src">中文数字 如：九亿八千七百六十五万四千三百二十一</param>
        /// <returns></returns>
        /// ————————————————
        /// 版权声明：本文为CSDN博主「刘超峰」的原创文章，遵循 CC 4.0 BY-SA 版权协议，转载请附上原文出处链接及本声明。
        /// 原文链接：https://blog.csdn.net/qq_16427421/article/details/82598050
        public static int ChineseToNumber(this string src)
        {
            // 定义一个数组，用于接受分割字符串的结果
            string[] srcArr;
            // 定义计算结果
            int result = 0;
            // 如果字符串中包含'亿'则进行分割
            if (src.IndexOf("亿") != -1)
            {
                // 以字符'亿'分割源字符串
                srcArr = src.Split('亿');
                // 计算亿以上的数字
                result += Convert.ToInt32(Convert2Number(srcArr[0]) * Math.Pow(10, 8));
                // 如果剩余字符串中包括'万'，则再次进行分割
                if (src.IndexOf("万") != -1)
                {
                    // 以字符'万'分割源字符串
                    srcArr = srcArr[1].Split('万');
                    // 计算万以上亿以下的数字
                    result += Convert.ToInt32(Convert2Number(srcArr[0]) * Math.Pow(10, 4)) + Convert.ToInt32(Convert2Number(srcArr[1]));
                }
            }
            // 如果字符串中不包含字符'亿'
            else
            {
                // 如果源字符串中包括'万'，则以'万'字进行分割
                if (src.IndexOf("万") != -1)
                {
                    srcArr = src.Split('万');
                    result += Convert.ToInt32(Convert2Number(srcArr[0]) * Math.Pow(10, 4)) + Convert.ToInt32(Convert2Number(srcArr[1]));
                }
                else
                {
                    // 源文本为1万以下的数字
                    result += Convert.ToInt32(Convert2Number(src));
                }
            }
            return result;
        }
        /// <summary>
        /// 1万以内中文转数字
        /// </summary>
        /// <param name="src">源文本如：四千三百二十一</param>
        /// <returns></returns>
        /// ————————————————
        /// 版权声明：本文为CSDN博主「刘超峰」的原创文章，遵循 CC 4.0 BY-SA 版权协议，转载请附上原文出处链接及本声明。
        /// 原文链接：https://blog.csdn.net/qq_16427421/article/details/82598050
        public static int Convert2Number(string src)
        {
            // 定义包含所有数字的字符串，用以判断字符是否为数字。
            string numberString = "零一二三四五六七八九";
            // 定义单位字符串，用以判断字符是否为单位。
            string unitString = "零十百千";
            // 把数字字符串转换为char数组，方便截取。
            char[] charArr = src.Replace(" ", "").ToCharArray();
            // 返回结果
            int result = 0;
            // 如果源为空指针、空字符串、空白字符串 则返回0
            if (string.IsNullOrEmpty(src) || string.IsNullOrWhiteSpace(src))
            {
                return 0;
            }
            // 如果源的第一个字符不是数字 则返回0
            if (numberString.IndexOf(charArr[0]) == -1)
            {
                return 0;
            }
            // 遍历字符数组
            for (int i = 0; i < charArr.Length; i++)
            {
                // 遍历单位字符串
                for (int j = 0; j < unitString.Length; j++)
                {
                    // 如果字符为单位则进行计算
                    if (charArr[i] == unitString[j])
                    {
                        // 如果字符为非'零'字符，则计算出十位以上到万位以下数字的和
                        if (charArr[i] != '零')
                        {
                            result += Convert.ToInt32(int.Parse(numberString.IndexOf(charArr[i - 1]).ToString()) * Math.Pow(10, j));
                        }
                    }
                }
            }
            // 如果源文本末尾字符为'零'-'九'其中之一，则计算结果和个位数相加。
            if (numberString.IndexOf(charArr[charArr.Length - 1]) != -1)
            {
                result += numberString.IndexOf(charArr[charArr.Length - 1]);
            }
            // 返回计算结果。
            return result;
        }

        public static string NumBytesToUserReadableString(long numBytes)
        {
            if (numBytes > 1024)
            {
                double numBytesDecimal = numBytes;
                // Put in KB
                numBytesDecimal /= 1024;
                if (numBytesDecimal > 1024)
                {
                    // Put in MB
                    numBytesDecimal /= 1024;
                    if (numBytesDecimal > 1024)
                    {
                        // Put in GB
                        numBytesDecimal /= 1024;
                        return numBytesDecimal.ToString("F2") + " GB";
                    }
                    return numBytesDecimal.ToString("F2") + " MB";
                }
                return numBytesDecimal.ToString("F2") + " KB";
            }
            return numBytes.ToString();
        }

    }
}
