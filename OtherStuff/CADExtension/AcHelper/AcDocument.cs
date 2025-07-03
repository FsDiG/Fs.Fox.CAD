using Autodesk.AutoCAD.ApplicationServices;
using System;

namespace AcHelper
{
    [Obsolete("Functions can be found in Active. This class wil be removed next version.", true)]
    public static class AcDocument
    {

#region Commandline
        /// <summary>
        /// Send command on commandline
        /// </summary>
        /// <param name="command"></param>
        public static void SendCommand(string command)
        {
            if (!string.IsNullOrEmpty(command))
            {
                Document document = Active.Document;
                if (document != null)
                {
                    document.SendStringToExecute(string.Format("{0} ", command), true, false, false);
                }//if
            }//if
        }
       
        /// <summary>
        /// Set a AcadCAD variable to a new value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetVariable(string name, object value)
        {
            try
            {
                Application.SetSystemVariable(name, value);
            }
            catch(Exception ex)
            {
                Active.WriteMessage(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retreive the value of an AutoCAD Variable
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetVariable(string name)
        {
            try
            {
                object value= Application.GetSystemVariable(name);
                if (value != null)
                {
                    return value.ToString();
                }
            }
            catch(Exception ex)
            {
                Active.WriteMessage(ex.Message);
                throw;
            }
            return string.Empty;
        }

#endregion

    }
}
