using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using System;

namespace Autodesk.AutoCAD.Runtime
{
    /// <summary>
    ///
    /// </summary>
    public abstract class ExtensionApplication : IExtensionApplication
    {
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Terminates this instance.
        /// </summary>
        public abstract void Terminate();

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        void IExtensionApplication.Initialize()
        {
            try
            {
                this.Initialize();
            }
            catch (System.Exception ex)
            {
                Console.Beep();
                Application.DocumentManager.MdiActiveDocument.Editor.WriteLine("\nAn error occured while loading {0}:\n\n{1}",
                  this.GetType().Assembly.Location,
                  ex.ToString()
                );
                throw;
            }
        }

        /// <summary>
        /// Terminates this instance.
        /// </summary>
        void IExtensionApplication.Terminate()
        {
            this.Terminate();
        }
    }
}