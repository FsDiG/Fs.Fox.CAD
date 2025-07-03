using System.Collections.Generic;
using System.IO;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Colors;

namespace NFox.Cad
{
    /// <summary>
    /// 符号表扩展类
    /// </summary>
    public static class SymbolTableEx
    {
        #region BlockTable

        /// <summary>
        /// 添加块定义
        /// </summary>
        /// <param name="table">块表</param>
        /// <param name="blockName">块定义名</param>
        /// <param name="ents">图元实体集合</param>
        /// <returns>块定义Id</returns>
        public static ObjectId Add(this SymbolTableCollection<BlockTable, BlockTableRecord> table, string blockName, IEnumerable<Entity> ents)
        {
            return table.Add(blockName, btr => table.Trans.AddEntity(btr, ents));
        }
        /// <summary>
        /// 添加属性块
        /// </summary>
        /// <param name="table">块表</param>
        /// <param name="blockName">块名</param>
        /// <param name="ents">图元集合</param>
        /// <param name="attdef">属性定义集合</param>
        /// <returns>块定义Id</returns>
        public static ObjectId Add(this SymbolTableCollection<BlockTable,BlockTableRecord> table, string blockName, IEnumerable<Entity> ents, IEnumerable<AttributeDefinition> attdef)
        {
            return table.Add(blockName, btr =>
             {
                 table.Trans.AddEntity(btr, ents);
                 table.Trans.AddEntity(btr, attdef);
                 
             });
        }


        /// <summary>
        /// 从文件中获取块定义
        /// </summary>
        /// <param name="table">块表</param>
        /// <param name="fileName">文件名</param>
        /// <param name="blockName">块定义名</param>
        /// <param name="over">是否覆盖</param>
        /// <returns>块定义Id</returns>
        public static ObjectId GetBlockFrom(this SymbolTableCollection<BlockTable, BlockTableRecord> table, string fileName, string blockName, bool over)
        {
            return
               table.GetRecordFrom(
                    t => t.BlockTable,
                    fileName,
                    blockName,
                    over);
        }

        /// <summary>
        /// 从文件中获取块定义
        /// </summary>
        /// <param name="table">块表</param>
        /// <param name="fileName">文件名</param>
        /// <param name="over">是否覆盖</param>
        /// <returns>块定义Id</returns>
        public static ObjectId GetBlockFrom(this SymbolTableCollection<BlockTable, BlockTableRecord> table, string fileName, bool over)
        {
            FileInfo fi = new FileInfo(fileName);
            string blkdefname = fi.Name;
            if (blkdefname.Contains("."))
            {
                blkdefname = blkdefname.Substring(0, blkdefname.LastIndexOf('.'));
            }

            ObjectId id = table[blkdefname];
            bool has = id != ObjectId.Null;
            if ((has && over) || !has)
            {
                Database db = new Database();
                db.ReadDwgFile(fileName, FileShare.Read, true, null);
                id = table.Database.Insert(BlockTableRecord.ModelSpace, blkdefname, db, false);
            }

            return id;
        }

        #endregion BlockTable

        #region LayerTable

        /// <summary>
        /// 添加图层
        /// </summary>
        /// <param name="table">图层表</param>
        /// <param name="layerName">图层名</param>
        /// <param name="color">颜色</param>
        /// <param name="LinetypeId">线型id</param>
        /// <param name="lineweight">线宽</param>
        /// <returns>图层id</returns>
        public static ObjectId Add(this SymbolTableCollection<LayerTable, LayerTableRecord> table, string layerName, Color color, ObjectId LinetypeId, LineWeight lineweight)
        {
            return
                table.Add(
                    layerName,
                    ltr =>
                    {
                        ltr.Name = layerName;
                        ltr.Color = color;
                        ltr.LinetypeObjectId = LinetypeId;
                        ltr.LineWeight = lineweight;
                    });
        }

        /// <summary>
        /// 从文件中获取图层记录
        /// </summary>
        /// <param name="table">层表</param>
        /// <param name="fileName">文件名</param>
        /// <param name="layerName">图层名</param>
        /// <param name="over">是否覆盖</param>
        /// <returns>图层Id</returns>
        public static ObjectId GetLayerFrom(this SymbolTableCollection<LayerTable, LayerTableRecord> table, string fileName, string layerName, bool over)
        {
            return
                table.GetRecordFrom(
                    t => t.LayerTable,
                    fileName,
                    layerName,
                    over);
        }

        #endregion LayerTable

        #region TextStyleTable

        /// <summary>
        /// 添加文字样式记录
        /// </summary>
        /// <param name="table">字体样式表</param>
        /// <param name="textStyleName">文字样式名</param>
        /// <param name="smallfont">小字体名</param>
        /// <param name="bigfont">大字体名</param>
        /// <param name="xscale">宽度比例</param>
        /// <returns>文字样式Id</returns>
        public static ObjectId Add(this SymbolTableCollection<TextStyleTable, TextStyleTableRecord> table, string textStyleName, string smallfont, string bigfont, double xscale)
        {
            return
                table.Add(
                    textStyleName,
                    tstr =>
                    {
                        tstr.Name = textStyleName;
                        tstr.FileName = smallfont;
                        tstr.BigFontFileName = bigfont;
                        tstr.XScale = xscale;
                    });
        }

        /// <summary>
        /// 添加文字样式记录
        /// </summary>
        /// <param name="table">文字样式表</param>
        /// <param name="textStyleName">文字样式名</param>
        /// <param name="font">字体名</param>
        /// <param name="xscale">宽度比例</param>
        /// <returns>文字样式Id</returns>
        public static ObjectId Add(this SymbolTableCollection<TextStyleTable, TextStyleTableRecord> table, string textStyleName, string font, double xscale)
        {
            return
                table.Add(
                    textStyleName,
                    tstr =>
                    {
                        tstr.Name = textStyleName;
                        tstr.FileName = font;
                        tstr.XScale = xscale;
                    });
        }

        /// <summary>
        /// 从文件中获取文字样式记录
        /// </summary>
        /// <param name="table">文字样式表</param>
        /// <param name="fileName">文件名</param>
        /// <param name="textStyleName">字体样式名</param>
        /// <param name="over">是否覆盖</param>
        /// <returns>文字样式Id</returns>
        public static ObjectId GetTextStyleFrom(this SymbolTableCollection<TextStyleTable, TextStyleTableRecord> table, string fileName, string textStyleName, bool over)
        {
            return
                table.GetRecordFrom(
                    t => t.TextStyleTable,
                    fileName,
                    textStyleName,
                    over);
        }

        #endregion TextStyleTable

        #region DimStyleTable

        private static void SetDimstyleData(this SymbolTableCollection<DimStyleTable, DimStyleTableRecord> table, ObjectId id)
        {
            var dstr = table.GetRecord(id);
            table.Database.SetDimstyleData(dstr);
        }

        /// <summary>
        /// 从文件中获取标注样式记录
        /// </summary>
        /// <param name="target">目标</param>
        /// <param name="source">源</param>
        /// <param name="dimStyleName">标注样式名</param>
        /// <param name="over">是否覆盖</param>
        /// <returns>注样式记录id</returns>
        public static ObjectId GetDimStyleFrom(this SymbolTableCollection<DimStyleTable, DimStyleTableRecord> target, SymbolTableCollection<DimStyleTable, DimStyleTableRecord> source, string dimStyleName, bool over)
        {
            ObjectId id =
                target.GetRecordFrom(source, dimStyleName, over);
            target.SetDimstyleData(id);
            target.SetDimstyleData(target.Database.Dimstyle);
            return id;
        }

        /// <summary>
        /// 从文件中获取标注样式记录
        /// </summary>
        /// <param name="table">目标</param>
        /// <param name="fileName">文件名</param>
        /// <param name="dimStyleName">标注样式名</param>
        /// <param name="over">是否覆盖</param>
        /// <returns>注样式记录id</returns>
        public static ObjectId GetDimStyleFrom(this SymbolTableCollection<DimStyleTable, DimStyleTableRecord> table, string fileName, string dimStyleName, bool over)
        {
            ObjectId id =
                table.GetRecordFrom(
                    t => t.DimStyleTable,
                    fileName,
                    dimStyleName,
                    over);
            table.SetDimstyleData(id);
            table.SetDimstyleData(table.Database.Dimstyle);
            return id;
        }

        #endregion DimStyleTable

        #region LinetypeTable
        /// <summary>
        /// 添加线型
        /// </summary>
        /// <param name="table">线型表</param>
        /// <param name="name">线型名</param>
        /// <param name="description">线型说明</param>
        /// <param name="length">线型长度</param>
        /// <param name="dash">笔画长度数组</param>
        /// <returns>线型id</returns>
        public static ObjectId Add(this SymbolTableCollection<LinetypeTable,LinetypeTableRecord> table, string name, string description, double length, double[] dash)
        {
            return table.Add(
                name,
                ltt =>
                {
                    ltt.AsciiDescription = description;
                    ltt.PatternLength = length; //线型的总长度
                    ltt.NumDashes = dash.Length; //组成线型的笔画数目
                    for (int i = 0; i < dash.Length; i++)
                    {
                        ltt.SetDashLengthAt(i, dash[i]);
                    }
                    //ltt.SetDashLengthAt(0, 0.5); //0.5个单位的划线
                    //ltt.SetDashLengthAt(1, -0.25); //0.25个单位的空格
                    //ltt.SetDashLengthAt(2, 0); // 一个点
                    //ltt.SetDashLengthAt(3, -0.25); //0.25个单位的空格
                }
            );
        }
        /// <summary>
        /// 从文件中获取线型记录
        /// </summary>
        /// <param name="table">线型表</param>
        /// <param name="fileName">文件名</param>
        /// <param name="linetypeName">线型名</param>
        /// <param name="over">是否覆盖</param>
        /// <returns>线型Id</returns>
        public static ObjectId GetLinetypeFrom(this SymbolTableCollection<LinetypeTable, LinetypeTableRecord> table, string fileName, string linetypeName, bool over)
        {
            return
                table.GetRecordFrom(
                    t => t.LinetypeTable,
                    fileName,
                    linetypeName,
                    over);
        }

        #endregion LinetypeTable
    }

    //public static class SymbolTableEx
    //{
    //    /// <summary>
    //    /// 添加记录到符号表
    //    /// </summary>
    //    /// <param name="table">符号表</param>
    //    /// <param name="tr">事务</param>
    //    /// <param name="record">记录</param>
    //    /// <returns></returns>
    //    public static ObjectId Add(this SymbolTable table, Transaction tr, SymbolTableRecord record)
    //    {
    //        ObjectId id = table.Add(record);
    //        tr.AddNewlyCreatedDBObject(record, true);
    //        return id;
    //    }
    //    /// <summary>
    //    /// 在符号表中获取对应键值的记录的迭代器
    //    /// </summary>
    //    /// <param name="table">符号表</param>
    //    /// <param name="tr">事务</param>
    //    /// <param name="mode">读写模式</param>
    //    /// <param name="includingErased">是否包括已删除对象</param>
    //    /// <returns></returns>
    //    public static IEnumerable<SymbolTableRecord> GetRecords(this SymbolTable table, Transaction tr, OpenMode mode, bool includingErased)
    //    {
    //        foreach (ObjectId id in table)
    //        {
    //            SymbolTableRecord record = tr.GetObject(id, mode, includingErased) as SymbolTableRecord;
    //            if (record != null && (includingErased || !id.IsErased))
    //                yield return record;
    //        }
    //    }
    //    /// <summary>
    //    /// 在符号表中获取对应键值的记录的迭代器
    //    /// </summary>
    //    /// <param name="table">符号表</param>
    //    /// <param name="tr">事务</param>
    //    /// <param name="mode">读写模式</param>
    //    /// <returns></returns>
    //    public static IEnumerable<SymbolTableRecord> GetRecords(this SymbolTable table, Transaction tr, OpenMode mode)
    //    {
    //        foreach (ObjectId id in table)
    //        {
    //            if (!id.IsErased)
    //            {
    //                SymbolTableRecord record = tr.GetObject(id, mode, false) as SymbolTableRecord;
    //                if (record != null)
    //                    yield return record;
    //            }
    //        }
    //    }
    //    /// <summary>
    //    /// 在符号表中获取对应键值的记录的迭代器
    //    /// </summary>
    //    /// <param name="table">符号表</param>
    //    /// <param name="tr">事务</param>
    //    /// <returns></returns>
    //    public static IEnumerable<SymbolTableRecord> GetRecords(this SymbolTable table, Transaction tr)
    //    {
    //        return GetRecords(table, tr, OpenMode.ForRead);
    //    }

    //    /// <summary>
    //    /// 在符号表中获取对应键值的记录Id
    //    /// </summary>
    //    /// <param name="table">符号表</param>
    //    /// <param name="tr">事务</param>
    //    /// <param name="key">记录键值</param>
    //    /// <returns>对应键值的记录Id</returns>
    //    public static ObjectId GetRecorId(this SymbolTable table, Transaction tr, string key)
    //    {
    //        if (table.Has(key))
    //        {
    //            if (Application.Version.Major < 18)
    //            {
    //                ObjectId idres = table[key];
    //                if (!idres.IsErased)
    //                    return idres;
    //                return
    //                    table.Cast<ObjectId>()
    //                    .Where(id => !id.IsErased)
    //                    .Select(id => tr.GetObject(id, OpenMode.ForRead) as SymbolTableRecord)
    //                    .FirstOrDefault(str => str.Name == key)
    //                    .ObjectId;
    //            }
    //            else
    //            {
    //                return table[key];
    //            }
    //        }
    //        return ObjectId.Null;
    //    }

    //}
}