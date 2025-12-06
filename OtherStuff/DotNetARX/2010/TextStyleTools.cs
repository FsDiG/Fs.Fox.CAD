using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;
namespace DotNetARX
{
    /// <summary>
    /// 字体字符集名称
    /// </summary>
    public enum FontCharSet
    {
        /// <summary>
        /// 英文
        /// </summary>
        Ansi = 0,
        /// <summary>
        /// 与当前操作系统语言有关，如为简体中文，则被设置为GB2312
        /// </summary>
        Default = 1,
        /// <summary>
        /// 简体中文
        /// </summary>
        GB2312 = 134,
        /// <summary>
        /// 繁体中文
        /// </summary>
        Big5 = 136,
        /// <summary>
        /// 与操作系统有关
        /// </summary>
        OEM = 255
    }

    /// <summary>
    /// 字体的字宽
    /// </summary>
    public enum FontPitch
    {
        /// <summary>
        /// 默认字宽
        /// </summary>
        Default = 0,
        /// <summary>
        /// 固定字宽
        /// </summary>
        Fixed = 1,
        /// <summary>
        /// 可变字宽
        /// </summary>
        Variable = 2
    }

    /// <summary>
    /// 字体的语系定义
    /// </summary>
    public enum FontFamily
    {
        /// <summary>
        /// 使用默认字体
        /// </summary>
        Dontcare = 0,
        /// <summary>
        /// 可变的笔划宽度，有衬线，如MS Serif字体
        /// </summary>
        Roman = 16,
        /// <summary>
        /// 可变的笔划宽度，无衬线，如MS Sans Serif字体 
        /// </summary>
        Swiss = 32,
        /// <summary>
        /// 固定笔划宽度，衬线可以有也可以没有,如Courier New字体
        /// </summary>
        Modern = 48,
        /// <summary>
        /// 手写体，如Cursive字体
        /// </summary>
        Script = 64,
        /// <summary>
        /// 小说字体，如旧式英语
        /// </summary>
        Decorative = 80
    }

    /// <summary>
    /// 文字样式操作类
    /// </summary>
    public static class TextStyleTools
    {
        /// <summary>
        /// 创建一个新的文字样式
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="styleName">文字样式名</param>
        /// <param name="fontFilename">字体文件名</param>
        /// <param name="bigFontFilename">大字体文件名</param>
        /// <returns>返回添加的文字样式的Id</returns>
        public static ObjectId AddTextStyle(this Database db, string styleName, string fontFilename, string bigFontFilename)
        {
            //打开文字样式表
            TextStyleTable st=(TextStyleTable)db.TextStyleTableId.GetObject(OpenMode.ForRead);
            if (!st.Has(styleName))//如果不存在名为styleName的文字样式，则新建一个文字样式
            {
                //定义一个新的文字样式表记录
                TextStyleTableRecord str=new TextStyleTableRecord();
                str.Name = styleName;//设置文字样式名
                str.FileName = fontFilename;//设置字体文件名
                str.BigFontFileName = bigFontFilename;//设置大字体文件名
                st.UpgradeOpen();//切换文字样式表的状态为写以添加新的文字样式
                st.Add(str);//将文字样式表记录的信息添加到文字样式表中
                //把文字样式表记录添加到事务处理中
                db.TransactionManager.AddNewlyCreatedDBObject(str, true);
                st.DowngradeOpen();//为了安全，将文字样式表的状态切换为读
            }
            return st[styleName];//返回新添加的文字样式表记录的ObjectId
        }

        /// <summary>
        /// 创建一个新的文字样式
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="styleName">文字样式名</param>
        /// <param name="fontFilename">字体文件名</param>
        /// <returns>返回添加的文字样式的Id</returns>
        public static ObjectId AddTextStyle(this Database db, string styleName, string fontFilename)
        {
            return db.AddTextStyle(styleName, fontFilename, "");
        }

        /// <summary>
        /// 创建一个新的文字样式
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="styleName">文字样式名</param>
        /// <param name="fontName">字体文件名</param>
        /// <param name="bold">是否加粗</param>
        /// <param name="italic">是否倾斜</param>
        /// <param name="charset">字体的字符集</param>
        /// <param name="pitchAndFamily">字体的字宽和语系定义</param>
        /// <returns>返回添加的文字样式的Id</returns>
        public static ObjectId AddTextStyle(this Database db, string styleName, string fontName, bool bold, bool italic, int charset, int pitchAndFamily)
        {
            //打开文字样式表
            TextStyleTable st=(TextStyleTable)db.TextStyleTableId.GetObject(OpenMode.ForRead);
            if (!st.Has(styleName))//如果不存在名为styleName的文字样式，则新建一个文字样式
            {
                //定义一个新的的文字样式表记录
                TextStyleTableRecord str=new TextStyleTableRecord();
                str.Name = styleName;//设置的文字样式名
                //设置文字样式的字体
                str.Font = new FontDescriptor(fontName, bold, italic, charset, pitchAndFamily);
                st.UpgradeOpen();//切换的文字样式表的状态为写以添加新的的文字样式
                st.Add(str);//将新建的文字样式表记录的信息添加到文字样式表中
                //把的文字样式表记录添加到事务处理中
                db.TransactionManager.AddNewlyCreatedDBObject(str, true);
                st.DowngradeOpen();//为了安全，将文字样式表的状态切换为读
            }
            return st[styleName];//返回新添加的的文字样式表记录的ObjectId
        }

        /// <summary>
        /// 添加包含图案的形定义文件
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="styleName">样式名</param>
        /// <param name="shapeFilename">形定义文件名</param>
        /// <returns>返回新添加的文字样式表记录的ObjectId</returns>
        public static ObjectId AddShapeTextStyle(this Database db, string styleName, string shapeFilename)
        {
            //打开文字样式表
            TextStyleTable st=(TextStyleTable)db.TextStyleTableId.GetObject(OpenMode.ForRead);
            if (!st.Has(styleName))//如果不存在名为styleName的文字样式，则新建一个文字样式
            {
                //定义一个新的的文字样式表记录
                TextStyleTableRecord tstr=new TextStyleTableRecord();
                tstr.Name = styleName;//设置的文字样式名
                tstr.FileName = shapeFilename;//设置字体文件名
                tstr.IsShapeFile = true;//文件为形定义文件
                st.UpgradeOpen();//切换的文字样式表的状态为写以添加新的的文字样式
                st.Add(tstr);//将新建的文字样式表记录的信息添加到文字样式表中
                //把的文字样式表记录添加到事务处理中
                db.TransactionManager.AddNewlyCreatedDBObject(tstr, true);
                st.DowngradeOpen();
            }
            return st[styleName];//返回新添加的文字样式表记录的ObjectId
        }

        /// <summary>
        /// 设置当前文字样式
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="styleName">文字样式名</param>
        /// <returns>如果设置成功返回true，否则返回false</returns>
        public static bool SetCurrentTextStyle(this Database db, string styleName)
        {
            var trans=db.TransactionManager;
            //打开文字样式表
            TextStyleTable st=(TextStyleTable)trans.GetObject(db.TextStyleTableId, OpenMode.ForRead);
            //如果不存在名为styleName的文字样式，则返回
            if (!st.Has(styleName)) return false;
            //获取名为styleName的的文字样式表记录的Id
            ObjectId styleId=st[styleName];
            //如果指定的文字样式为当前文字样式，则返回
            if (db.Textstyle == styleId) return false;
            //指定当前文字样式
            db.Textstyle = styleId;
            return true;//指定当前文字样式成功
        }

        /// <summary>
        /// 删除文字样式
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="styleName">文字样式名</param>
        /// <returns>如果删除成功返回true，否则返回false</returns>
        public static bool DeleteTextStyle(this Database db, string styleName)
        {
            var trans=db.TransactionManager;
            //打开文字样式表
            TextStyleTable st=(TextStyleTable)trans.GetObject(db.TextStyleTableId, OpenMode.ForRead);
            //如果不存在名为styleName的文字样式，则返回
            if (!st.Has(styleName)) return false;
            //获取名为styleName的文字样式表记录的Id
            ObjectId styleId=st[styleName];
            //如果要删除的文字样式为当前文字样式，则返回（不能删除当前文字样式）
            if (styleId == db.Textstyle) return false;
            //以写的方式打开名为styleName的文字样式表记录
            TextStyleTableRecord str=(TextStyleTableRecord)trans.GetObject(styleId, OpenMode.ForWrite);
            //删除名为styleName的文字样式
            str.Erase(true);
            return true;//删除文字样式成功
        }

        /// <summary>
        /// 设置文字样式的有关属性
        /// </summary>
        /// <param name="styleId">文字样式的Id</param>
        /// <param name="textSize">高度</param>
        /// <param name="xscale">宽度因子</param>
        /// <param name="obliquingAngle">倾斜角度</param>
        /// <param name="isVertical">是否垂直</param>
        /// <param name="upsideDown">是否上下颠倒</param>
        /// <param name="backwards">是否反向</param>
        /// <param name="annotative">是否具有注释性</param>
        /// <param name="paperOrientation">文字方向与布局是否匹配</param>
        public static void SetTextStyleProp(this ObjectId styleId, double textSize, double xscale, double obliquingAngle, bool isVertical, bool upsideDown, bool backwards, AnnotativeStates annotative, bool paperOrientation)
        {
            //打开文字样式表记录
            TextStyleTableRecord str=styleId.GetObject(OpenMode.ForWrite) as TextStyleTableRecord;
            if (str == null) return;////如果styleId表示是不是文字样式表记录，则返回            
            str.TextSize = textSize;//高度
            str.XScale = xscale;//宽度因子
            str.ObliquingAngle = obliquingAngle;//倾斜角度
            str.IsVertical = isVertical;//是否垂直
            str.FlagBits = (byte)0;
            str.FlagBits += upsideDown ? (byte)2 : (byte)0;//是否上下颠倒
            str.FlagBits += backwards ? (byte)4 : (byte)0;//是否反向
            str.Annotative = annotative;//是否具有注释性
            str.SetPaperOrientation(paperOrientation);//文字方向与布局是否匹配           
            str.DowngradeOpen();//为了安全切换为读的状态            
        }
    }
}
