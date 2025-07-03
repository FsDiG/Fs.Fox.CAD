namespace NFox.Cad
{
    /// <summary>
    /// 没用的类
    /// </summary>
    public static class DbUtility
    {
        #region MText

        //public static string GetMTextContents2(string str)
        //{
        //    string[] strs =
        //        str.Split(
        //            new string[] { "\\\\" },
        //            StringSplitOptions.None);
        //    for (int i = 0; i < strs.Length; i++)
        //    {
        //        strs[i] =
        //            Regex.Replace
        //            (
        //                strs[i],
        //                @"(?<!\\)[{}]|\\[OLP\~]|\\[CFHTQWA][^;]*;",
        //                "",
        //                RegexOptions.IgnoreCase
        //            );
        //        strs[i] =
        //            Regex.Replace
        //            (
        //                strs[i],
        //                @"\\S(.*?)[/#\^](.*?)(;|$)",
        //                "($1/$2)",
        //                RegexOptions.IgnoreCase
        //           );
        //        strs[i] =
        //            Regex.Replace
        //            (
        //                strs[i],
        //                @"\\([{}])",
        //                "$1"
        //            );
        //    }
        //    return string.Join("\\", strs);
        //}

        //public static string GetMTextContents3(string str)
        //{
        //    str = "{" + str + "}";
        //    Stack<string> stack = new Stack<string>();
        //    while (str.Length > 0)
        //    {
        //        int n = (str[0] == '\\') ? 2 : 1;
        //        if (n == 1 && str[0] == '}')
        //        {
        //            Stack<string> substack = new Stack<string>();
        //            while (stack.Peek() != "{")
        //            {
        //                substack.Push(stack.Pop());
        //            }
        //            stack.Pop();
        //            Queue<string> queue = GetRtfTextUnFormatString(substack);
        //            while (queue.Count > 0)
        //            {
        //                stack.Push(queue.Dequeue());
        //            }
        //        }
        //        else
        //        {
        //            stack.Push(str.Substring(0, n));
        //        }
        //        str = str.Substring(n);
        //    }
        //    string res = "";
        //    foreach (string s in stack)
        //    {
        //        if (s.Length == 1)
        //        {
        //            res = s + res;
        //        }
        //        else
        //        {
        //            res = s.Substring(1) + res;
        //        }
        //    }
        //    return res;
        //}

        //private static Queue<string>
        //    GetRtfTextUnFormatString(Stack<string> stack)
        //{
        //    Queue<string> queue = new Queue<string>();
        //    while (stack.Count > 0)
        //    {
        //        string str = stack.Pop();
        //        if (str.Length == 1)
        //        {
        //            queue.Enqueue(str);
        //        }
        //        else
        //        {
        //            switch (str.Substring(1).ToUpper()[0])
        //            {
        //                case '\\':
        //                case '{':
        //                case '}':
        //                case 'U':
        //                    queue.Enqueue(str);
        //                    break;
        //                case 'A':
        //                case 'C':
        //                case 'F':
        //                case 'H':
        //                case 'Q':
        //                case 'T':
        //                case 'W':
        //                    while (stack.Pop() != ";") ;
        //                    break;
        //                case 'S':
        //                    string s = "";
        //                    queue.Enqueue("(");
        //                    while ((s = stack.Peek()) != ";")
        //                    {
        //                        if (s == "^" || s == "#")
        //                        {
        //                            stack.Pop();
        //                            queue.Enqueue("/");
        //                        }
        //                        else
        //                        {
        //                            queue.Enqueue(stack.Pop());
        //                        }
        //                        if (stack.Count == 0)
        //                        {
        //                            break;
        //                        }
        //                    }
        //                    queue.Enqueue(")");
        //                    if (stack.Count > 0)
        //                    {
        //                        stack.Pop();
        //                    }
        //                    break;
        //            }
        //        }
        //    }
        //    return queue;
        //}

        #endregion MText

        #region XRecord

        //static bool SetXRecord(ObjectId dictid, ResultBuffer rb, string key, params string[] dictNames)
        //{
        //    using (DBDictionary dict = GetSubDict(dictid, true, dictNames))
        //    {
        //        Xrecord rec;
        //        if (dict.Contains(key))
        //        {
        //            rec = dict.GetAt(key).Open<Xrecord>(OpenMode.ForWrite);
        //        }
        //        else
        //        {
        //            dict.UpgradeOpen();
        //            rec = new Xrecord();
        //            dict.SetAt(key, rec);
        //        }
        //        using (rec)
        //        {
        //            rec.Data = rb;
        //        }
        //    }
        //    return true;
        //}

        //public static bool SetXRecord(DBObject obj, ResultBuffer rb, string key, params string[] dictNames)
        //{
        //    ObjectId dictid = obj.ExtensionDictionary;
        //    if (dictid == ObjectId.Null)
        //    {
        //        obj.CreateExtensionDictionary();
        //        dictid = obj.ExtensionDictionary;
        //    }
        //    return SetXRecordByDictId(dictid, rb, key, dictNames);
        //}

        //public static bool SetXRecord(ObjectId id, ResultBuffer rb, string key, params string[] dictNames)
        //{
        //    using (DBObject obj = id.Open<DBObject>(OpenMode.ForWrite))
        //        return SetXRecord(obj, rb, key, dictNames);
        //}

        //public static bool SetXRecord(Database db, ResultBuffer rb, string key, params string[] dictnames)
        //{
        //    return
        //        SetXRecordByDictId
        //        (
        //            db.NamedObjectsDictionaryId,
        //            rb,
        //            key,
        //            dictnames
        //        );
        //}

        //static ResultBuffer GetXRecordByDictId(ObjectId dictid, string key, params string[] dictNames)
        //{
        //    DBDictionary dict = GetSubDict(dictid, false, dictNames);
        //    if (dict != null)
        //    {
        //        using (dict)
        //        {
        //            if (dict.Contains(key))
        //            {
        //                using (Xrecord rec = dict.GetAt(key).Open<Xrecord>())
        //                {
        //                    return rec.Data;
        //                }
        //            }
        //        }
        //    }
        //    return null;
        //}

        //public static ResultBuffer GetXRecord(DBObject obj, string key, params string[] dictNames)
        //{
        //    ObjectId dictid = obj.ExtensionDictionary;
        //    if (dictid != null)
        //    {
        //        return GetXRecordByDictId(dictid, key, dictNames);
        //    }
        //    return null;
        //}

        //public static ResultBuffer GetXRecord(ObjectId id, string key, params string[] dictNames)
        //{
        //    using (DBObject obj = id.Open<DBObject>())
        //        return GetXRecord(obj, key, dictNames);
        //}

        //public static ResultBuffer GetXRecord(Database db, string key, params string[] dictnames)
        //{
        //    return
        //        GetXRecordByDictId
        //        (
        //            db.NamedObjectsDictionaryId,
        //            key,
        //            dictnames
        //        );
        //}

        #endregion XRecord
    }
}