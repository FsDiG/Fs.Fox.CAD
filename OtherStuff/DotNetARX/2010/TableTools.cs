using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace DotNetARX
{
    /// <summary>
    /// 表格操作类
    /// </summary>
    public static class TableTools
    {
        /// <summary>
        /// 创建表格
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="position">表格位置</param>
        /// <param name="numRows">表格行数</param>
        /// <param name="numCols">表格列数</param>
        /// <returns>返回创建的表格的Id</returns>
        public static ObjectId CreateTable(this Database db, Point3d position, int numRows, int numCols)
        {
            ObjectId tableId;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                Table table = new Table();
                //设置表格的行数和列数
                table.SetSize(numRows, numCols);
                //设置表格放置的位置
                table.Position = position;
                //非常重要，根据当前样式更新表格，不加此句，会导致AutoCAD崩溃
                table.GenerateLayout();
                //表格添加到模型空间
                tableId = db.AddToModelSpace(table);
                trans.Commit();
            }
            return tableId;
        }

        /// <summary>
        /// 设置单元格中文本的高度
        /// </summary>
        /// <param name="table">表格对象</param>
        /// <param name="height">文本高度</param>
        /// <param name="rowType">行的标志位</param>
        //public static void SetTextHeight(this Table table, double height, RowType rowType)
        //{
        //    table.SetTextHeight(height, rowType);
        //}

        /// <summary>
        /// 设置表格中所有单元格中文本为同一高度
        /// </summary>
        /// <param name="table">表格对象</param>
        /// <param name="height">文本高度</param>
        public static void SetTextHeight(this Table table, double height)
        {
            for (int row = 0; row < table.Rows.Count; row++)
            {
                for (int column = 0; column < table.Columns.Count; column++)
                {
                    table.Cells[row, column].TextHeight = height;
                }
            }
        }

        /// <summary>
        /// 设置单元格中文本的对齐方式
        /// </summary>
        /// <param name="table">表格对象</param>
        /// <param name="align">单元格对齐方式</param>
        /// <param name="rowType">行的标志位</param>
        //public static void SetAlignment(this Table table, CellAlignment align, RowType rowType)
        //{
        //    table.SetAlignment(align, (int)rowType);
        //}

        /// <summary>
        /// 设置表格中所有单元格中文本为同一对齐方式
        /// </summary>
        /// <param name="table">表格对象</param>
        /// <param name="align">单元格对齐方式</param>
        public static void SetAlignment(this Table table, CellAlignment align)
        {
            for (int row = 0; row < table.Rows.Count; row++)
            {
                for (int column = 0; column < table.Columns.Count; column++)
                {
                    table.Cells[row, column].Alignment = align;
                }
            }
        }

        /// <summary>
        /// 一次性按行设置单元格文本
        /// </summary>
        /// <param name="table">表格对象</param>
        /// <param name="rowIndex">行号</param>
        /// <param name="data">文本内容</param>
        /// <returns>如果设置成功，则返回true，否则返回false</returns>
        public static bool SetRowTextString(this Table table, int rowIndex, params string[] data)
        {
            if (data.Length > table.Columns.Count) return false;
            for (int j = 0; j < data.Length; j++)
            {
                table.Cells[rowIndex, j].TextString = data[j];
            }
            return true;
        }

        /// <summary>
        /// 为图形添加一个新的表格样式
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="styleName">表格样式的名称</param>
        /// <returns>返回表格样式的Id</returns>
        public static ObjectId AddTableStyle(this Database db, string styleName)
        {
            ObjectId styleId;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //打开表格样式字典
                DBDictionary dict = (DBDictionary)trans.GetObject(db.TableStyleDictionaryId, OpenMode.ForRead);
                //判断是否存在指定的表格样式
                if (dict.Contains(styleName))
                    styleId = dict.GetAt(styleName);//如果存在则返回表格样式的Id
                else
                {
                    //新建一个表格样式
                    TableStyle style = new TableStyle();
                    dict.UpgradeOpen();//切换表格样式字典为写的状态
                    //将新的表格样式添加到样式字典并获取其 Id
                    styleId = dict.SetAt(styleName, style);
                    //将新建的表格样式添加到事务处理中
                    trans.AddNewlyCreatedDBObject(style, true);
                    trans.Commit();
                }
            }
            return styleId;//返回表格样式的Id
        }
    }
}
