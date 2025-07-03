using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;

namespace ThCADExtension
{
    public static class ThPickTool
    {
        // Taking mouse inputs from a modal dialog box
        //  https://adndevblog.typepad.com/autocad/2012/05/taking-mouse-inputs-from-a-modal-dialog-box.html
        public static ObjectId PickEntity(System.Windows.Forms.Control modalDialog, string selectMessage)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            ObjectId picked = ObjectId.Null;
            using (EditorUserInteraction inter = ed.StartUserInteraction(modalDialog))
            {
                PromptEntityOptions opt = new PromptEntityOptions("\n" + selectMessage);
                PromptEntityResult res = ed.GetEntity(opt);
                if (res.Status == PromptStatus.OK)
                {
                    picked = res.ObjectId;
                }
            }
            return picked;
        }

        public static ObjectId PickEntity(string selectMessage)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            ObjectId picked = ObjectId.Null;
            PromptEntityOptions opt = new PromptEntityOptions("\n" + selectMessage);
            PromptEntityResult res = ed.GetEntity(opt);
            if (res.Status == PromptStatus.OK)
            {
                picked = res.ObjectId;
            }

            return picked;
        }
    }
}
