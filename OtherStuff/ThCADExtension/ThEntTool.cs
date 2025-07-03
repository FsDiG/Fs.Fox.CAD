using AcHelper;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADExtension
{
    public static class ThEntTool
    {
        // Making a copy of an entity
        //  https://adndevblog.typepad.com/autocad/2012/07/making-a-copy-of-an-entity.html
        public static ObjectId DeepClone(ObjectId objectId)
        {
            ObjectIdCollection collection = new ObjectIdCollection
            {
                objectId
            };

            //make model space as owner for new entity
            ObjectId modelSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(Active.Database);

            IdMapping mapping = new IdMapping();
            Active.Database.DeepCloneObjects(collection, modelSpaceId, mapping, false);

            // return the cloned object
            return mapping[objectId].Value;
        }
    }
}
