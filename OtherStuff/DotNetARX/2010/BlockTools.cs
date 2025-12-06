using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Internal;

namespace DotNetARX
{
    /// <summary>
    /// 动态块的动态属性类型
    /// </summary>
    public enum DynBlockPropTypeCode
    {
        /// <summary>
        /// 字符串
        /// </summary>
        String = 1,
        /// <summary>
        /// 实数
        /// </summary>
        Real = 40,
        /// <summary>
        /// 短整型
        /// </summary>
        Short = 70,
        /// <summary>
        /// 长整型
        /// </summary>
        Long = 90
    }

    /// <summary>
    /// 块操作类
    /// </summary>
    public static partial class BlockTools
    {
        /// <summary>
        /// 创建一个块表记录并添加到数据库中
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="blockName">块名</param>
        /// <param name="ents">加入块中的实体列表</param>
        /// <returns>返回块表记录的Id</returns>
        public static ObjectId AddBlockTableRecord(this Database db, string blockName, List<Entity> ents)
        {
            //打开块表
            BlockTable bt = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
            if (!bt.Has(blockName)) //判断是否存在名为blockName的块
            {
                //创建一个BlockTableRecord类的对象，表示所要创建的块
                BlockTableRecord btr = new BlockTableRecord();
                btr.Name = blockName;//设置块名                
                //将列表中的实体加入到新建的BlockTableRecord对象
                ents.ForEach(ent => btr.AppendEntity(ent));
                bt.UpgradeOpen();//切换块表为写的状态
                bt.Add(btr);//在块表中加入blockName块
                db.TransactionManager.AddNewlyCreatedDBObject(btr, true);//通知事务处理
                bt.DowngradeOpen();//为了安全，将块表状态改为读
            }
            return bt[blockName];//返回块表记录的Id
        }

        /// <summary>
        /// 创建一个块表记录并添加到数据库中
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="blockName">块名</param>
        /// <param name="ents">加入块中的实体列表</param>
        /// <returns>返回块表记录的Id</returns>
        public static ObjectId AddBlockTableRecord(this Database db, string blockName, params Entity[] ents)
        {
            return AddBlockTableRecord(db, blockName, ents.ToList());
        }

        /// <summary>
        /// 在AutoCAD图形中插入块参照
        /// </summary>
        /// <param name="spaceId">块参照要加入的模型空间或图纸空间的Id</param>
        /// <param name="layer">块参照要加入的图层名</param>
        /// <param name="blockName">块参照所属的块名</param>
        /// <param name="position">插入点</param>
        /// <param name="scale">缩放比例</param>
        /// <param name="rotateAngle">旋转角度</param>
        /// <returns>返回块参照的Id</returns>
        public static ObjectId InsertBlockReference(this ObjectId spaceId, string layer, string blockName, Point3d position, Scale3d scale, double rotateAngle)
        {
            ObjectId blockRefId;//存储要插入的块参照的Id
            Database db = spaceId.Database;//获取数据库对象
            //以读的方式打开块表
            BlockTable bt = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
            //如果没有blockName表示的块，则程序返回
            if (!bt.Has(blockName)) return ObjectId.Null;
            //以写的方式打开空间（模型空间或图纸空间）
            BlockTableRecord space = (BlockTableRecord)spaceId.GetObject(OpenMode.ForWrite);
            //创建一个块参照并设置插入点
            BlockReference br = new BlockReference(position, bt[blockName]);
            br.ScaleFactors = scale;//设置块参照的缩放比例
            br.Layer = layer;//设置块参照的层名
            br.Rotation = rotateAngle;//设置块参照的旋转角度
            ObjectId btrId = bt[blockName];//获取块表记录的Id
            //打开块表记录
            BlockTableRecord record = (BlockTableRecord)btrId.GetObject(OpenMode.ForRead);
            //添加可缩放性支持
            if (record.Annotative == AnnotativeStates.True)
            {
                ObjectContextCollection contextCollection = db.ObjectContextManager.GetContextCollection("ACDB_ANNOTATIONSCALES");
                ObjectContexts.AddContext(br, contextCollection.GetContext("1:1"));
            }
            blockRefId = space.AppendEntity(br);//在空间中加入创建的块参照
            db.TransactionManager.AddNewlyCreatedDBObject(br, true);//通知事务处理加入创建的块参照
            space.DowngradeOpen();//为了安全，将块表状态改为读
            return blockRefId;//返回添加的块参照的Id
        }

        /// <summary>
        /// 在AutoCAD图形中插入块参照
        /// </summary>
        /// <param name="spaceId">块参照要加入的模型空间或图纸空间的Id</param>
        /// <param name="layer">块参照要加入的图层名</param>
        /// <param name="blockName">块参照所属的块名</param>
        /// <param name="position">插入点</param>
        /// <param name="scale">缩放比例</param>
        /// <param name="rotateAngle">旋转角度</param>
        /// <param name="attNameValues">属性的名称与取值</param>
        /// <returns>返回块参照的Id</returns>
        public static ObjectId InsertBlockReference(this ObjectId spaceId, string layer, string blockName, Point3d position, Scale3d scale, double rotateAngle, Dictionary<string, string> attNameValues)
        {
            Database db = spaceId.Database;//获取数据库对象
            //以读的方式打开块表
            BlockTable bt = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
            //如果没有blockName表示的块，则程序返回
            if (!bt.Has(blockName)) return ObjectId.Null;
            //以写的方式打开空间（模型空间或图纸空间）
            BlockTableRecord space = (BlockTableRecord)spaceId.GetObject(OpenMode.ForWrite);
            ObjectId btrId = bt[blockName];//获取块表记录的Id
            //打开块表记录
            BlockTableRecord record = (BlockTableRecord)btrId.GetObject(OpenMode.ForRead);
            //创建一个块参照并设置插入点
            BlockReference br = new BlockReference(position, bt[blockName]);
            br.ScaleFactors = scale;//设置块参照的缩放比例
            br.Layer = layer;//设置块参照的层名
            br.Rotation = rotateAngle;//设置块参照的旋转角度
            space.AppendEntity(br);//为了安全，将块表状态改为读 
            //判断块表记录是否包含属性定义
            if (record.HasAttributeDefinitions)
            {
                //若包含属性定义，则遍历属性定义
                foreach (ObjectId id in record)
                {
                    //检查是否是属性定义
                    AttributeDefinition attDef = id.GetObject(OpenMode.ForRead) as AttributeDefinition;
                    if (attDef != null)
                    {
                        //创建一个新的属性对象
                        AttributeReference attribute = new AttributeReference();
                        //从属性定义获得属性对象的对象特性
                        attribute.SetAttributeFromBlock(attDef, br.BlockTransform);
                        //设置属性对象的其它特性
                        attribute.Position = attDef.Position.TransformBy(br.BlockTransform);
                        attribute.Rotation = attDef.Rotation;
                        attribute.AdjustAlignment(db);
                        //判断是否包含指定的属性名称
                        if (attNameValues.ContainsKey(attDef.Tag.ToUpper()))
                        {
                            //设置属性值
                            attribute.TextString = attNameValues[attDef.Tag.ToUpper()].ToString();
                        }
                        //向块参照添加属性对象
                        br.AttributeCollection.AppendAttribute(attribute);
                        db.TransactionManager.AddNewlyCreatedDBObject(attribute, true);
                    }
                }
            }
            db.TransactionManager.AddNewlyCreatedDBObject(br, true);
            return br.ObjectId;//返回添加的块参照的Id
        }

        /// <summary>
        /// 更新块参照中的属性值
        /// </summary>
        /// <param name="blockRefId">块参照的Id</param>
        /// <param name="attNameValues">需要更新的属性名称与取值</param>
        public static void UpdateAttributesInBlock(this ObjectId blockRefId, Dictionary<string, string> attNameValues)
        {
            //获取块参照对象
            BlockReference blockRef = blockRefId.GetObject(OpenMode.ForRead) as BlockReference;
            if (blockRef != null)
            {
                //遍历块参照中的属性
                foreach (ObjectId id in blockRef.AttributeCollection)
                {
                    //获取属性
                    AttributeReference attref = id.GetObject(OpenMode.ForRead) as AttributeReference;
                    //判断是否包含指定的属性名称
                    if (attNameValues.ContainsKey(attref.Tag.ToUpper()))
                    {
                        attref.UpgradeOpen();//切换属性对象为写的状态
                        //设置属性值
                        attref.TextString = attNameValues[attref.Tag.ToUpper()].ToString();
                        attref.DowngradeOpen();//为了安全，将属性对象的状态改为读
                    }
                }
            }
        }

        /// <summary>
        /// 获取指定名称的块参照的属性名和属性值
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="blockName">块名</param>
        /// <returns>返回块参照的属性名和属性值</returns>
        public static SortedDictionary<string, string> GetAttributesInBlock(this Database db, string blockName)
        {
            SortedDictionary<string, string> attributes = new SortedDictionary<string, string>();
            // 筛选指定名称的块参照
            TypedValue[] values = { new TypedValue((int)DxfCode.Start, "INSERT"),
                                    new TypedValue((int)DxfCode.BlockName, blockName),
                                    };
            var filter = new SelectionFilter(values);
            Editor ed = Application.DocumentManager.GetDocument(db).Editor;
            var entSelected = ed.SelectAll(filter);
            // 如果数据库不存在指定名称的块参照，则返回
            if (entSelected.Status != PromptStatus.OK) return null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 遍历块参照
                foreach (var id in entSelected.Value.GetObjectIds())
                {
                    BlockReference bref = (BlockReference)trans.GetObject(id, OpenMode.ForRead);
                    // 遍历块参照中的属性
                    foreach (ObjectId attId in bref.AttributeCollection)
                    {
                        AttributeReference attRef = (AttributeReference)trans.GetObject(attId, OpenMode.ForRead);
                        // 将块参照的属性名和属性值添加到字典中
                        attributes.Add(attRef.Tag, attRef.TextString);
                    }
                }
                trans.Commit();
            }
            return attributes; // 返回指定名称的块参照的属性名和属性值
        }

        /// <summary>
        /// 获取块参照的属性名和属性值
        /// </summary>
        /// <param name="blockReferenceId">块参照的Id</param>
        /// <returns>返回块参照的属性名和属性值</returns>
        public static SortedDictionary<string, string> GetAttributesInBlockReference(this ObjectId blockReferenceId)
        {
            SortedDictionary<string, string> attributes = new SortedDictionary<string, string>();
            Database db = blockReferenceId.Database;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 获取块参照
                BlockReference bref = (BlockReference)trans.GetObject(blockReferenceId, OpenMode.ForRead);
                // 遍历块参照的属性，并将其属性名和属性值添加到字典中
                foreach (ObjectId attId in bref.AttributeCollection)
                {
                    AttributeReference attRef = (AttributeReference)trans.GetObject(attId, OpenMode.ForRead);
                    attributes.Add(attRef.Tag, attRef.TextString);
                }
                trans.Commit();
            }
            return attributes; // 返回块参照的属性名和属性值
        }

        /// <summary>
        /// 获取指定名称的块属性值
        /// </summary>
        /// <param name="blockReferenceId">块参照的Id</param>
        /// <param name="attributeName">属性名</param>
        /// <returns>返回指定名称的块属性值</returns>
        public static string GetAttributeInBlockReference(this ObjectId blockReferenceId, string attributeName)
        {
            string attributeValue = string.Empty; // 属性值
            Database db = blockReferenceId.Database;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 获取块参照
                BlockReference bref = (BlockReference)trans.GetObject(blockReferenceId, OpenMode.ForRead);
                // 遍历块参照的属性
                foreach (ObjectId attId in bref.AttributeCollection)
                {
                    // 获取块参照属性对象
                    AttributeReference attRef = (AttributeReference)trans.GetObject(attId, OpenMode.ForRead);
                    //判断属性名是否为指定的属性名
                    if (attRef.Tag.ToUpper() == attributeName.ToUpper())
                    {
                        attributeValue = attRef.TextString;//获取属性值
                        break;
                    }
                }
                trans.Commit();
            }
            return attributeValue; //返回块属性值
        }

        /// <summary>
        /// 获取指定块名的块参照
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="blockName">块名</param>
        /// <returns>返回指定块名的块参照</returns>
        public static List<BlockReference> GetAllBlockReferences(this Database db, string blockName)
        {
            List<BlockReference> blocks = new List<BlockReference>();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //打开块表
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                //打开指定块名的块表记录
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[blockName], OpenMode.ForRead);
                //获取指定块名的块参照集合的Id
                ObjectIdCollection blockIds = btr.GetBlockReferenceIds(true, true);
                foreach (ObjectId id in blockIds) // 遍历块参照的Id
                {
                    //获取块参照
                    BlockReference block = (BlockReference)trans.GetObject(id, OpenMode.ForRead);
                    blocks.Add(block); // 将块参照添加到返回列表 
                }
                trans.Commit();
            }
            return blocks; //返回块参照列表
        }

        /// <summary>
        /// 返回指定块名的动态块参照
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="blockName">动态块的块名</param>
        /// <returns>返回指定块名的动态块参照</returns>
        public static List<BlockReference> GetAllDynBlockReferences(this Database db, string blockName)
        {
            List<BlockReference> blocks = new List<BlockReference>();
            var trans = db.TransactionManager;
            BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
            BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[blockName], OpenMode.ForRead);
            blocks = (from b in db.GetEntsInDatabase<BlockReference>()
                      where b.GetBlockName() == blockName
                      select b).ToList();
            return blocks;
        }

        /// <summary>
        /// 导入外部文件中的块
        /// </summary>
        /// <param name="destDb">目标数据库</param>
        /// <param name="sourceFileName">包含完整路径的外部文件名</param>
        public static void ImportBlocksFromDwg(this Database destDb, string sourceFileName)
        {
            //创建一个新的数据库对象，作为源数据库，以读入外部文件中的对象
            Database sourceDb = new Database(false, true);
            try
            {
                //把DWG文件读入到一个临时的数据库中
                sourceDb.ReadDwgFile(sourceFileName, System.IO.FileShare.Read, true, null);
                //创建一个变量用来存储块的ObjectId列表
                ObjectIdCollection blockIds = new ObjectIdCollection();
                //获取源数据库的事务处理管理器
                Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = sourceDb.TransactionManager;
                //在源数据库中开始事务处理
                using (Transaction myT = tm.StartTransaction())
                {
                    //打开源数据库中的块表
                    BlockTable bt = (BlockTable)tm.GetObject(sourceDb.BlockTableId, OpenMode.ForRead, false);
                    //遍历每个块
                    foreach (ObjectId btrId in bt)
                    {
                        BlockTableRecord btr = (BlockTableRecord)tm.GetObject(btrId, OpenMode.ForRead, false);
                        //只加入命名块和非布局块到复制列表中
                        if (!btr.IsAnonymous && !btr.IsLayout)
                        {
                            blockIds.Add(btrId);
                        }
                        btr.Dispose();
                    }
                    bt.Dispose();
                }
                //定义一个IdMapping对象
                IdMapping mapping = new IdMapping();
                //从源数据库向目标数据库复制块表记录
                sourceDb.WblockCloneObjects(blockIds, destDb.BlockTableId, mapping, DuplicateRecordCloning.Replace, false);
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Application.ShowAlertDialog("复制错误: " + ex.Message);
            }
            //操作完成，销毁源数据库
            sourceDb.Dispose();
        }

        /// <summary>
        /// 获取块参照的块名（包括动态块）
        /// </summary>
        /// <param name="id">块参照的Id</param>
        /// <returns>返回块名</returns>
        public static string GetBlockName(this ObjectId id)
        {
            //获取块参照
            BlockReference bref = id.GetObject(OpenMode.ForRead) as BlockReference;
            if (bref != null)//如果是块参照
                return GetBlockName(bref);
            else
                return null;
        }

        /// <summary>
        /// 获取块参照的块名（包括动态块）
        /// </summary>
        /// <param name="bref">块参照</param>
        /// <returns>返回块名</returns>
        public static string GetBlockName(this BlockReference bref)
        {
            string blockName;//存储块名
            if (bref == null) return null;//如果块参照不存在，则返回
            if (bref.IsDynamicBlock) //如果是动态块
            {
                //获取动态块所属的动态块表记录
                ObjectId idDyn = bref.DynamicBlockTableRecord;
                //打开动态块表记录
                BlockTableRecord btr = (BlockTableRecord)idDyn.GetObject(OpenMode.ForRead);
                blockName = btr.Name;//获取块名
            }
            else //非动态块
                blockName = bref.Name; //获取块名
            return blockName;//返回块名
        }

        /// <summary>
        /// 为块表记录添加属性
        /// </summary>
        /// <param name="blockId">块表记录的Id</param>
        /// <param name="atts">要加入的块属性列表</param>
        public static void AddAttsToBlock(this ObjectId blockId, List<AttributeDefinition> atts)
        {
            Database db = blockId.Database;//获取数据库对象
            //打开块表记录为写的状态
            BlockTableRecord btr = (BlockTableRecord)blockId.GetObject(OpenMode.ForWrite);
            //遍历属性定义对象列表
            foreach (AttributeDefinition att in atts)
            {
                btr.AppendEntity(att);//为块表记录添加属性
                db.TransactionManager.AddNewlyCreatedDBObject(att, true);//通知事务处理
            }
            btr.DowngradeOpen();//为了安全，将块表记录的状态改为读
        }

        /// <summary>
        /// 为块表记录添加属性
        /// </summary>
        /// <param name="blockId">块表记录的Id</param>
        /// <param name="atts">要加入的块属性列表</param>
        public static void AddAttsToBlock(this ObjectId blockId, params AttributeDefinition[] atts)
        {
            blockId.AddAttsToBlock(atts.ToList());
        }

        #region 动态块
        /// <summary>
        /// 获取动态块的动态属性值
        /// </summary>
        /// <param name="blockId">动态块的Id</param>
        /// <param name="propName">需要查找的动态属性名</param>
        /// <returns>返回指定动态属性的值</returns>
        public static string GetDynBlockValue(this ObjectId blockId, string propName)
        {
            string propValue = null;//用于返回动态属性值的变量
            var props = blockId.GetDynProperties();//获得动态块的所有动态属性
            //遍历动态属性
            foreach (DynamicBlockReferenceProperty prop in props)
            {
                //如果动态属性的名称与输入的名称相同
                if (prop.PropertyName == propName)
                {
                    //获取动态属性值并结束遍历
                    propValue = prop.Value.ToString();
                    break;
                }
            }
            return propValue;//返回动态属性值
        }

        /// <summary>
        /// 获得动态块的所有动态属性
        /// </summary>
        /// <param name="blockId">动态块的Id</param>
        /// <returns>返回动态块的所有属性</returns>
        public static DynamicBlockReferencePropertyCollection GetDynProperties(this ObjectId blockId)
        {
            //获取块参照
            BlockReference br = blockId.GetObject(OpenMode.ForRead) as BlockReference;
            //如果不是动态块，则返回
            if (br == null && !br.IsDynamicBlock) return null;
            //返回动态块的动态属性
            return br.DynamicBlockReferencePropertyCollection;
        }

        /// <summary>
        /// 设置动态块的动态属性
        /// </summary>
        /// <param name="blockId">动态块的ObjectId</param>
        /// <param name="propName">动态属性的名称</param>
        /// <param name="value">动态属性的值</param>
        public static void SetDynBlockValue(this ObjectId blockId, string propName, object value)
        {
            var props = blockId.GetDynProperties();//获得动态块的所有动态属性
            //遍历动态属性
            foreach (DynamicBlockReferenceProperty prop in props)
            {
                //如果动态属性的名称与输入的名称相同且为可读
                if (prop.ReadOnly == false && prop.PropertyName == propName)
                {
                    //判断动态属性的类型并通过类型转化设置正确的动态属性值
                    switch (prop.PropertyTypeCode)
                    {
                        case (short)DynBlockPropTypeCode.Short://短整型
                            prop.Value = Convert.ToInt16(value);
                            break;
                        case (short)DynBlockPropTypeCode.Long://长整型
                            prop.Value = Convert.ToInt64(value);
                            break;
                        case (short)DynBlockPropTypeCode.Real://实型
                            prop.Value = Convert.ToDouble(value);
                            break;
                        default://其它
                            prop.Value = value;
                            break;
                    }
                    break;
                }
            }
        }

        #endregion
    }
}
