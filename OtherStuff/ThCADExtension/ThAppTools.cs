using System;
using Autodesk.AutoCAD.ApplicationServices;

namespace ThCADExtension
{
    public static class ThAppTools
    {
        /// 
        /// Automates saving/changing/restoring system variables
        /// 
        /// 
        public class ManagedSystemVariable : IDisposable
        {
            string name = null;
            object oldval = null;

            public ManagedSystemVariable(string name, object value)
               : this(name)
            {
                Application.SetSystemVariable(name, value);
            }

            public ManagedSystemVariable(string name)
            {
                this.name = name;
                this.oldval = Application.GetSystemVariable(name);
            }

            public void Dispose()
            {
                if (oldval != null)
                {
                    object temp = oldval;
                    oldval = null;
                    Application.SetSystemVariable(name, temp);
                }
            }
        }
    }
}
