using AcHelper.Wrappers;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcHelper
{
    public static class EntityExtensions
    {
        /// <summary>
        /// Resets the entity's properties.
        /// </summary>
        /// <param name="entity">Entity to reset.</param>
        /// <param name="newLayer">Name of the layer to set to the entity.</param>
        public static void ResetProperties(this Entity entity, string newLayer)
        {
            Color newColor = Color.FromColorIndex(ColorMethod.ByAci, 256);
            entity.Color = newColor;
            entity.Layer = newLayer;
            entity.Linetype = Constants.BYLAYER;
        }

        public static void ReadDBObject<T>(this ObjectId objectId, Action<T> action) where T : DBObject
        {
            Active.Document.StartNestedTransaction(tr =>
            {
                Transaction t = tr.Transaction;
                T entity = t.GetObject<T>(objectId, OpenMode.ForRead);
                action(entity);
            });
        }

        public static void EditDBObject<T>(this ObjectId objectId, Action<T> action) where T : DBObject
        {
            Active.Document.StartNestedTransaction(tr =>
            {
                Transaction t = tr.Transaction;
                T dbObject = t.GetObject<T>(objectId, OpenMode.ForRead);
                using (new WriteEnabler(dbObject))
                {
                    if (dbObject.IsWriteEnabled)
                    {
                        action(dbObject);
                    }
                }
            });
        }
    }
}
