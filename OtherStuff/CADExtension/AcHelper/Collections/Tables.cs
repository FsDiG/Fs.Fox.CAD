using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;
using System;

namespace AcHelper.Collections
{
    public static class Tables
    {
        #region TextStyle
        public static TextStyleTableRecord GetTextStyle(string textStyleName)
        {
            Document document = Active.Document;
            Database database = document.Database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {
                    TextStyleTable textStyleTable = GetTextStyleTable();
                    foreach (ObjectId textStyleTableRecordId in textStyleTable)
                    {
                        TextStyleTableRecord textStyleTableRecord = transaction.GetObject<TextStyleTableRecord>(textStyleTableRecordId, OpenMode.ForRead);
                        if (textStyleTableRecord.Name == textStyleName)
                        {
                            return textStyleTableRecord;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Active.WriteMessage(ex.Message);
                    throw;
                }
            }//using
            return null;
        }

        public static void AddTextStyle(TextStyle textStyle)
        {
            Document document = Active.Document;
            Database database = document.Database;

            try
            {
                TextStyleTable textStyleTable = GetTextStyleTable();
                using (Transaction transaction = database.TransactionManager.StartTransaction())
                {
                    SymbolUtilityServices.ValidateSymbolName(textStyle.Name, false);

                    if (!textStyleTable.Has(textStyle.Name))
                    {
                        TextStyleTableRecord textStyleTableRecord = new TextStyleTableRecord();
                        textStyleTableRecord.Name = textStyle.Name;
                        if (textStyle.FileName.EndsWith(".shx"))
                            textStyleTableRecord.FileName = textStyle.FileName;
                        else
                            textStyleTableRecord.Font = new FontDescriptor(textStyle.Name, false, false, 0, 0);
                        textStyleTableRecord.TextSize = textStyle.TextSize;
                        textStyleTableRecord.ObliquingAngle = textStyle.ObliquingAngle;

                        textStyleTable.Add(textStyleTableRecord);
                        transaction.AddNewlyCreatedDBObject(textStyleTableRecord, true);
                    }

                    transaction.Commit();
                }//using
            }
            catch (Exception ex)
            {
                Active.WriteMessage(ex.Message);
                throw;
            }
        }

        public static void RemoveTextStyle(string styleName)
        {
            Document document = Active.Document;
            Database database = document.Database;

            try
            {
                TextStyleTable textStyleTable = GetTextStyleTable();
                using (Transaction transaction = database.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId textStyleTableRecordId in textStyleTable)
                    {
                        TextStyleTableRecord textStyleTableRecord = transaction.GetObject<TextStyleTableRecord>(textStyleTableRecordId, OpenMode.ForRead);
                        if (textStyleTableRecord.Name == styleName)
                        {
                            textStyleTableRecord.UpgradeOpen();
                            textStyleTableRecord.Erase();
                        }
                    }

                    transaction.Commit();
                }//using
            }
            catch (Exception ex)
            {
                Active.WriteMessage(ex.Message);
                throw;
            }
        }

        public static TextStyleTable GetTextStyleTable()
        {
            Document document = Active.Document;
            Database database = document.Database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {
                    TextStyleTable textStyleTable = transaction.GetObject<TextStyleTable>(database.TextStyleTableId, OpenMode.ForRead);
                    return textStyleTable;
                }
                catch (Exception ex)
                {
                    Active.WriteMessage(ex.Message);
                    return null;
                }
            }// using Transaction
        }
        #endregion

        #region DimStyle
        public static DimStyleTableRecord GetDimStyle(string styleName)
        {
            Document document = Active.Document;
            Database database = document.Database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {
                    DimStyleTable dimStyleTable = GetDimStyleTable();
                    foreach (ObjectId dimStyleTableRecordId in dimStyleTable)
                    {
                        DimStyleTableRecord dimStyleTableRecord = transaction.GetObject<DimStyleTableRecord>(dimStyleTableRecordId, OpenMode.ForRead);
                        if (dimStyleTableRecord.Name == styleName)
                        {
                            return dimStyleTableRecord;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Active.WriteMessage(ex.Message);
                    throw;
                }
            }// using
            return null;
        }

        public static void AddDimStyle(DimStyle dimStyle)
        {
            Document document = Active.Document;
            Database database = document.Database;

            try
            {
                DimStyleTable dimStyleTable = GetDimStyleTable();
                TextStyleTableRecord textStyleTableRecord = GetTextStyle(dimStyle.TextStyle);

                using (Transaction transaction = database.TransactionManager.StartTransaction())
                {
                    LinetypeTableRecord lineTypeTableRecord = GetLineType(transaction, "ByLayer");
                    if (textStyleTableRecord != null & lineTypeTableRecord != null)
                    {
                        DimStyleTableRecord dimStyleTableRecord = new DimStyleTableRecord();
                        dimStyleTableRecord.Name = dimStyle.Name;

                        // Lines & Arrows
                        dimStyleTableRecord.Dimasz = dimStyle.TextHeight;
                        dimStyleTableRecord.Dimlwd = LineWeight.ByLayer;
                        dimStyleTableRecord.Dimlwe = LineWeight.ByLayer;
                        dimStyleTableRecord.Dimsd1 = false;
                        dimStyleTableRecord.Dimsd2 = false;
                        dimStyleTableRecord.Dimclrd = Color.FromColorIndex(ColorMethod.ByAci, Constants.COLOR_INDEX_BYLAYER);
                        dimStyleTableRecord.Dimltype = lineTypeTableRecord.ObjectId;
                        dimStyleTableRecord.Dimdle = 0;
                        dimStyleTableRecord.Dimltex1 = lineTypeTableRecord.ObjectId;
                        dimStyleTableRecord.Dimltex2 = lineTypeTableRecord.ObjectId;
                        dimStyleTableRecord.Dimse1 = false;
                        dimStyleTableRecord.Dimse2 = false;
                        dimStyleTableRecord.DimfxlenOn = false;
                        dimStyleTableRecord.Dimfxlen = 1;
                        dimStyleTableRecord.Dimclre = Color.FromColorIndex(ColorMethod.ByAci, Constants.COLOR_INDEX_BYLAYER);
                        dimStyleTableRecord.Dimexe = 0.18;
                        dimStyleTableRecord.Dimexo = 0;

                        // Text
                        dimStyleTableRecord.Dimtfill = 0;
                        dimStyleTableRecord.Dimfrac = 0;
                        dimStyleTableRecord.Dimclrt = Color.FromColorIndex(ColorMethod.ByAci, Constants.COLOR_INDEX_BYLAYER);
                        dimStyleTableRecord.Dimtxt = dimStyle.TextHeight;
                        dimStyleTableRecord.Dimgap = 0.5;
                        dimStyleTableRecord.Dimtih = false;
                        dimStyleTableRecord.Dimtoh = false;
                        dimStyleTableRecord.Dimjust = 0;
                        dimStyleTableRecord.Dimtad = 1;
                        dimStyleTableRecord.Dimtxsty = textStyleTableRecord.ObjectId;

                        // Fit
                        dimStyleTableRecord.Dimtofl = true;
                        dimStyleTableRecord.Dimsoxd = false;
                        dimStyleTableRecord.Dimtix = false;
                        dimStyleTableRecord.Dimscale = dimStyle.DimScale;
                        dimStyleTableRecord.Dimatfit = 3;
                        dimStyleTableRecord.Dimtmove = 0;

                        // Primary Units
                        dimStyleTableRecord.Dimdsep = Char.Parse(".");
                        dimStyleTableRecord.Dimpost = "";
                        dimStyleTableRecord.Dimrnd = 0;
                        dimStyleTableRecord.Dimlfac = 1;
                        dimStyleTableRecord.Dimlunit = 2;
                        dimStyleTableRecord.Dimazin = 2;
                        dimStyleTableRecord.Dimzin = 8;
                        dimStyleTableRecord.Dimdec = 1;

                        // Alternate Units
                        dimStyleTableRecord.Dimalt = false;

                        // Tolerances
                        dimStyleTableRecord.Dimtol = false;

                        dimStyleTable.Add(dimStyleTableRecord);
                        transaction.AddNewlyCreatedDBObject(dimStyleTableRecord, true);
                    }//if

                    transaction.Commit();
                }//using
            }
            catch (Exception ex)
            {
                Active.WriteMessage(ex.Message);
                throw;
            }
        }

        public static void RemoveDimStyle(string dimStyleName)
        {
            Database database = Active.Database;

            try
            {
                DimStyleTable dimStyleTable = GetDimStyleTable();
                using (Transaction transaction = database.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId dimStyleTableRecordId in dimStyleTable)
                    {
                        DimStyleTableRecord dimStyleTableRecord = transaction.GetObject<DimStyleTableRecord>(dimStyleTableRecordId, OpenMode.ForRead);
                        if (dimStyleTableRecord.Name == dimStyleName)
                        {
                            dimStyleTableRecord.UpgradeOpen();
                            dimStyleTableRecord.Erase();
                        }
                    }

                    transaction.Commit();
                }//using
            }
            catch (Exception ex)
            {
                Active.WriteMessage(ex.Message);
                throw;
            }
        }

        public static DimStyleTable GetDimStyleTable()
        {
            Document document = Active.Document;
            Database database = document.Database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {
                    DimStyleTable dimStyleTable = transaction.GetObject<DimStyleTable>(database.DimStyleTableId, OpenMode.ForRead);
                    return dimStyleTable;

                }
                catch (Exception ex)
                {
                    Active.WriteMessage(ex.Message);
                    return null;
                }
            }
        }
        #endregion

        #region LineType
        public static LinetypeTableRecord GetLineType(Transaction transaction, string lineTypeName)
        {
            Database db = Active.Database;

            LinetypeTable lineTypeTable = transaction.GetObject<LinetypeTable>(db.LinetypeTableId, OpenMode.ForRead);
            if (lineTypeTable.Has(lineTypeName))
            {
                ObjectId oid = lineTypeTable[lineTypeName];
                return transaction.GetObject<LinetypeTableRecord>(oid, OpenMode.ForRead);
            }
            return null;
        }
        public static void AddLineType(Transaction transaction, LineType lineType)
        {
            Document document = Active.Document;
            Database db = document.Database;

            LinetypeTable lineTypeTable = transaction.GetObject<LinetypeTable>(db.LinetypeTableId, OpenMode.ForRead);
            // if the layer is not loaded in the current dwg
            if (!lineTypeTable.Has(lineType.Name))
            {
                db.LoadLineTypeFile(lineType.Name, lineType.FileName);
                // Todo: Test if commit doesn't close the transaction.
                transaction.Commit();
            }
        }
        #endregion

        #region Layer
        public static LayerTableRecord GetLayer(Transaction transaction, string layerName)
        {
            Database db = Active.Database;
            LayerTable layertable = transaction.GetObject<LayerTable>(db.LayerTableId, OpenMode.ForRead);
            if (layertable.Has(layerName))
            {
                ObjectId oid = layertable[layerName];
                return transaction.GetObject<LayerTableRecord>(oid, OpenMode.ForRead);
            }
            return null;
        }
        public static bool CheckLayer(Transaction transaction, string layerName)
        {
            Database db = Active.Database;
            LayerTable layertable = transaction.GetObject<LayerTable>(db.LayerTableId, OpenMode.ForRead);

            return layertable.Has(layerName);
        }
        public static void AddLayer(Transaction transaction, Layer layer)
        {
            Database db = Active.Database;

            LinetypeTable lineTypeTabel = transaction.GetObject<LinetypeTable>(db.LinetypeTableId, OpenMode.ForRead);
            LayerTable layerTable = transaction.GetObject<LayerTable>(db.LayerTableId, OpenMode.ForRead);

            if (GetLineType(transaction, layer.LineType) is LinetypeTableRecord lineType
                && GetLayer(transaction, layer.Name) is LayerTableRecord)
            {
                LayerTableRecord newLayer = new LayerTableRecord()
                {
                    Name = layer.Name,

                    LinetypeObjectId = lineType.ObjectId,
                    Color = Color.FromColorIndex(ColorMethod.ByAci, layer.ColorIndex)
                };
                using (new Wrappers.WriteEnabler(layerTable))
                {
                    if (layerTable.IsWriteEnabled)
                    {
                        layerTable.Add(newLayer);
                        transaction.AddNewlyCreatedDBObject(newLayer, true);
                    }
                }
            }
        }
        public static void RemoveLayer(Transaction transaction, string layerName)
        {
            Database db = Active.Database;
            LayerTable layerTable = transaction.GetObject<LayerTable>(db.LayerTableId, OpenMode.ForRead);

            if (GetLayer(transaction, layerName) is LayerTableRecord layer)
            {
                using (new Wrappers.WriteEnabler(layer))
                {
                    if (layer.IsWriteEnabled)
                    {
                        layer.Erase();
                    }
                }
                transaction.Commit();
            }
        }

        [Obsolete("Layertable can be retrieved with Transaction.GetObject(LayerTableId, OpenMode.ForRead)", true)]
        public static LayerTable GetLayerTable()
        {
            Database database = Active.Database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {
                    LayerTable layerTable = transaction.GetObject<LayerTable>(database.LayerTableId, OpenMode.ForRead);
                    return layerTable;
                }
                catch (Exception ex)
                {
                    Active.WriteMessage(ex.Message);
                    return null;
                }
            }
        }
        #endregion

        #region Block
        public static BlockTableRecord GetBlock(string blockName)
        {
            Document document = Active.Document;
            Database database = document.Database;
            BlockTable blTable = GetBlockTable();

            try
            {
                using (Transaction transaction = database.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId blRecId in blTable)
                    {
                        BlockTableRecord blRec = transaction.GetObject<BlockTableRecord>(blRecId, OpenMode.ForRead);
                        if (blRec.Name == blockName)
                        {
                            return blRec;
                        }
                    }
                }//using
            }
            catch (Exception ex)
            {
                Active.WriteMessage(ex.Message);
                throw;
            }
            return null;
        }

        public static void AddBlock(AcBlock block)
        {
            Document document = Active.Document;
            Database database = document.Database;
            BlockTable blTable = GetBlockTable();
        }

        public static void RemoveBlock(string blockName)
        {
            Document document = Active.Document;
            Database database = document.Database;
            BlockTable blTable = GetBlockTable();
        }

        public static BlockTable GetBlockTable()
        {
            Document document = Active.Document;
            Database database = document.Database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {
                    BlockTable blTable = transaction.GetObject<BlockTable>(database.BlockTableId, OpenMode.ForRead);
                    return blTable;
                }
                catch (Exception ex)
                {
                    Active.WriteMessage(ex.Message);
                    return null;
                }
            }
        }
        #endregion

        #region Hatch

        public static BlockTableRecord GetHatch(string hatchName)
        {
            Document document = Active.Document;
            Database database = document.Database;
            BlockTable blTable = GetBlockTable();

            try
            {
                using (Transaction transaction = database.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId blRecId in blTable)
                    {
                        BlockTableRecord blRec = transaction.GetObject<BlockTableRecord>(blRecId, OpenMode.ForRead);
                        if (blRec.Name == hatchName)
                        {
                            return blRec;
                        }
                    }
                }//using
            }
            catch (Exception ex)
            {
                Active.WriteMessage(ex.Message);
                throw;
            }
            return null;
        }

        public static void AddHatch(AcHatch hatch)
        {

        }

        #endregion
    }
}
