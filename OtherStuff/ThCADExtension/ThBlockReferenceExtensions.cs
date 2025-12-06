using System;
using Linq2Acad;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Internal;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThCADExtension
{
    public static class ThBlockReferenceExtensions
    {
        /// <summary>
        ///Created thanks to the help of many @the Swamp.org.
        ///Intended to change the displayed insertion point of simple 2D blocks in ModelSpace(not sure how it affects dynamic or more complex blocks).
        ///Moves entities contained in block reference to center on the insertion/position point.
        ///but can be used to set the insertion point to any location desired.
        ///*Caution*, this will change the appearance of drawings where the insertion point is away from the displayed entities.
        /// </summary>
        // http://www.theswamp.org/index.php?topic=31859.msg525789#msg525789
        public static void ChangeInsertPoint()
        {
            Document document = AcadApp.DocumentManager.MdiActiveDocument;
            Editor editor = document.Editor;
            Database database = document.Database;

            using (document.LockDocument())
            {
                PromptEntityOptions options = new PromptEntityOptions("\nPick a block: ");
                options.SetRejectMessage("\nMust be a block reference: ");
                options.AddAllowedClass(typeof(BlockReference), true);

                Utils.PostCommandPrompt();
                PromptEntityResult result = editor.GetEntity(options);  //select a block reference in the drawing.

                if (result.Status == PromptStatus.OK)
                {
                    using (Transaction transaction = database.TransactionManager.StartTransaction())
                    {
                        BlockReference reference = (BlockReference)transaction.GetObject(result.ObjectId, OpenMode.ForRead);
                        BlockTableRecord record = (BlockTableRecord)transaction.GetObject(reference.BlockTableRecord, OpenMode.ForRead);

                        Point3d refPos = reference.Position;                           //current position of inserted block ref.                        
                        Point3d pmin = reference.Bounds.Value.MinPoint;                //bounding box of entities.
                        Point3d pmax = reference.Bounds.Value.MaxPoint;
                        Point3d newPos = (Point3d)(pmin + (pmax - pmin) / 2);          //center point of displayed graphics.

                        Vector3d vec = newPos.GetVectorTo(refPos);                     //apply your own desired points here.                  
                        vec = vec.TransformBy(reference.BlockTransform.Transpose());   //
                        Matrix3d mat = Matrix3d.Displacement(vec);                     //     


                        foreach (ObjectId eid in record)                               //update entities in the table record.
                        {
                            Entity entity = (Entity)transaction.GetObject(eid, OpenMode.ForRead) as Entity;

                            if (entity != null)
                            {
                                entity.UpgradeOpen();
                                entity.TransformBy(mat);
                                entity.DowngradeOpen();
                            }
                        }

                        ObjectIdCollection blockReferenceIds = record.GetBlockReferenceIds(false, false); //get all instances of same block ref.

                        foreach (ObjectId eid in blockReferenceIds)              //update all block references of the block modified.
                        {
                            BlockReference BlkRef = (BlockReference)transaction.GetObject(eid, OpenMode.ForWrite);

                            // BlkRef.TransformBy(mat.Inverse());  // include this line if you want block ref to stay in original location in dwg.

                            BlkRef.RecordGraphicsModified(true);
                        }

                        transaction.TransactionManager.QueueForGraphicsFlush();  //

                        editor.WriteMessage("\nInsertion points modified.");     //                               

                        transaction.Commit();                                    //                        
                    }
                }
                else
                {
                    editor.WriteMessage("Nothing picked: *" + result.Status + "*");
                }
            }
            AcadApp.UpdateScreen();
            Utils.PostCommandPrompt();
        }

        // MatchProperties
        //  https://forums.autodesk.com/t5/net/burst-blocks/td-p/8402180
        private static DBText MatchPropertiesFrom(DBText source)
        {
            var text = new DBText
            {
                Normal = source.Normal,
                Thickness = source.Thickness,
                Oblique = source.Oblique,
                Rotation = source.Rotation,
                Height = source.Height,
                WidthFactor = source.WidthFactor,
                TextString = source.TextString,
                TextStyleId = source.TextStyleId,
                IsMirroredInX = source.IsMirroredInX,
                IsMirroredInY = source.IsMirroredInY,
                HorizontalMode = source.HorizontalMode,
                VerticalMode = source.VerticalMode,
                Position = source.Position
            };
            if (source.Justify != AttachmentPoint.BaseLeft)
            {
                text.Justify = source.Justify;
                text.AlignmentPoint = source.AlignmentPoint;
            }
            text.SetPropertiesFrom(source);
            return text;
        }

        public static Entity ConvertAttributeReferenceToText(this AttributeReference attributeReference)
        {
            if (attributeReference.IsMTextAttribute)
            {
                return (MText)attributeReference.MTextAttribute.Clone();
            }
            else
            {
                return MatchPropertiesFrom(attributeReference);
            }
        }

        public static Entity ConvertAttributeDefinitionToText(this AttributeDefinition attributeDefinition)
        {
            if (attributeDefinition.IsMTextAttributeDefinition)
            {
                return (MText)attributeDefinition.MTextAttributeDefinition.Clone();
            }
            else
            {
                return MatchPropertiesFrom(attributeDefinition);
            }
        }

        // mimic the Burst command
        //  https://adndevblog.typepad.com/autocad/2015/06/programmatically-mimic-the-burst-command.html
        public static void Burst(this BlockReference blockReference, DBObjectCollection blockEntities)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                // 对于动态块，BlockReference.Name返回的可能是一个匿名块的名字（*Uxxx）
                // 对于这样的动态块，我们并不需要访问到它的“原始”动态块定义，我们只关心它“真实”的块定义
                var blockTableRecord = acadDatabase.Blocks.Element(blockReference.Name);

                // 如果没有属性定义，执行正常的Explode()操作
                if (!blockTableRecord.HasAttributeDefinitions)
                {
                    blockReference.Explode(blockEntities);
                    return;
                }

                // 先检查常量（可见）属性
                foreach (var attDef in blockTableRecord.GetAttributeDefinitions())
                {
                    if (attDef.Constant && !attDef.Invisible)
                    {
                        blockEntities.Add(attDef.ConvertAttributeDefinitionToText());
                    }
                }

                // 再检查非常量（可见）属性
                foreach (ObjectId attRefId in blockReference.AttributeCollection)
                {
                    var attRef = acadDatabase.Element<AttributeReference>(attRefId);
                    if (!attRef.Invisible)
                    {
                        blockEntities.Add(attRef.ConvertAttributeReferenceToText());
                    }
                }

                // Explode块引用，忽略属性定义
                using (DBObjectCollection dbObjs = new DBObjectCollection())
                {
                    blockReference.Explode(dbObjs);
                    foreach (Entity dbObj in dbObjs)
                    {
                        if (dbObj is AttributeDefinition)
                        {
                            continue;
                        }

                        blockEntities.Add(dbObj);
                    }
                }
            }
        }
    }
}
