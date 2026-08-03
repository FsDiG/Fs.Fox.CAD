
//
// (C) Copyright 2004 by Autodesk, Inc.
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
//


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;

[assembly:ExtensionApplication(typeof(Fs.Fox.CAD.Diagnostics.App))]
[assembly:CommandClass(typeof(Fs.Fox.CAD.Diagnostics.Test.TestCmds))]

namespace Fs.Fox.CAD.Diagnostics
{

	public class App : IExtensionApplication
	{
        private readonly List<Test.MgdDbgTestFuncs> m_tests = new List<Test.MgdDbgTestFuncs>();
        private readonly List<Snoop.CollectorExts.CollectorExt> m_collectorExts =
            new List<Snoop.CollectorExts.CollectorExt>();
        private readonly List<string> m_snoopAssemblyNames = new List<string>();
        private AppDocReactor m_appDocReactor = null;
        private bool m_initialized;

		public void
		Initialize()
		{
            if (m_initialized)
                return;

            Utils.AcadUi.PrintToCmdLine("\nLoading Fs.Fox.CAD.Diagnostics...");

            try {
                    // Register collector extensions before exposing commands in the UI.
                RegisterCollectorExtensions();
                AppContextMenu.AddContextMenu();
                CreateAndAddTestFuncs();

                m_appDocReactor = new AppDocReactor();
                m_appDocReactor.EnableEvents();

                AddFilterForSnoopClasses();
                m_initialized = true;
            }
            catch {
                    // A partially initialized NETLOAD must not leave static handlers behind.
                TerminateCore();
                throw;
            }
		}

		public void
		Terminate()
		{
            TerminateCore();
            m_initialized = false;
		}

        private void
        RegisterCollectorExtensions()
        {
            m_collectorExts.Add(new Snoop.CollectorExts.Object());
            m_collectorExts.Add(new Snoop.CollectorExts.RxObject());
            m_collectorExts.Add(new Snoop.CollectorExts.DbObject());
            m_collectorExts.Add(new Snoop.CollectorExts.SymbolTable());
            m_collectorExts.Add(new Snoop.CollectorExts.Entity());
            m_collectorExts.Add(new Snoop.CollectorExts.Color());
            m_collectorExts.Add(new Snoop.CollectorExts.Geometry());
            m_collectorExts.Add(new Snoop.CollectorExts.GraphNodes());
            m_collectorExts.Add(new Snoop.CollectorExts.DbMisc());
            m_collectorExts.Add(new Snoop.CollectorExts.GraphicsInterface());
            m_collectorExts.Add(new Snoop.CollectorExts.LayerManager());
            m_collectorExts.Add(new Snoop.CollectorExts.GraphicsSystem());
            m_collectorExts.Add(new Snoop.CollectorExts.Publish());
            m_collectorExts.Add(new Snoop.CollectorExts.Plotting());
            m_collectorExts.Add(new Snoop.CollectorExts.EditorInput());
        }

        /// <summary>
        /// Reverses initialization order and tolerates repeated calls. AutoCAD
        /// can call Terminate after a failed or incomplete initialization.
        /// </summary>
        private void
        TerminateCore()
        {
            TryCleanup(RemoveFilterForSnoopClasses);

            if (m_appDocReactor != null) {
                AppDocReactor reactor = m_appDocReactor;
                m_appDocReactor = null;
                TryCleanup(reactor.DisableEvents);
            }

            TryCleanup(RemoveAndFreeTestFuncs);
            TryCleanup(AppContextMenu.RemoveContextMenu);

            for (int i = m_collectorExts.Count - 1; i >= 0; i--) {
                TryCleanup(m_collectorExts[i].Dispose);
            }
            m_collectorExts.Clear();
        }

        private static void
        TryCleanup(Action cleanup)
        {
            try {
                cleanup();
            }
            catch (System.Exception exception) {
                    // Termination should continue so later registrations are not leaked.
                Debug.WriteLine("Fs.Fox.CAD.Diagnostics cleanup failed: " + exception);
            }
        }

        /// <summary>
        /// The TestFramework allows us to plug tests and sample functions into an existing
        /// UI Framework.  For each TestFuncs object we've created to house our individual
        /// tests, we need to add them during App start up, and remove them during App shut down.
        /// </summary>

        private void
        CreateAndAddTestFuncs()
        {
            m_tests.Add(new Fs.Fox.CAD.Diagnostics.Test.DbTests());
            m_tests.Add(new Fs.Fox.CAD.Diagnostics.Test.MakeEntTests());
            m_tests.Add(new Fs.Fox.CAD.Diagnostics.Test.MakeSymTblRecTests());
            m_tests.Add(new Fs.Fox.CAD.Diagnostics.Test.ModifyEntTests());
            m_tests.Add(new Fs.Fox.CAD.Diagnostics.Test.QueryCurveTests());
            m_tests.Add(new Fs.Fox.CAD.Diagnostics.Test.QueryEntTests());
            m_tests.Add(new Fs.Fox.CAD.Diagnostics.Test.CategoryTests());

            foreach (Fs.Fox.CAD.Diagnostics.Test.MgdDbgTestFuncs testFunc in m_tests) {
                Fs.Fox.CAD.Diagnostics.Test.MgdDbgTestFuncs.AddTestFuncsToFramework(testFunc);
            }
        }

        /// <summary>
        /// Reverse of above.  Nothing to do for each TestFunc object though
        /// because we already know which ones were registered for this app.
        /// </summary>

        private void
        RemoveAndFreeTestFuncs()
        {
            for (int i = m_tests.Count - 1; i >= 0; i--) {
                Fs.Fox.CAD.Diagnostics.Test.MgdDbgTestFuncs testFunc = m_tests[i];
                Fs.Fox.CAD.Diagnostics.Test.MgdDbgTestFuncs.RemoveTestFuncsFromFramework(testFunc);
            }
            m_tests.Clear();
        }

        /// <summary>
        /// This function adds the assemblies we are interested in having the Snoop.Editor
        /// dialog get class information from.  We don't want to display class info for every
        /// assembly in .NET, just the ones we are responsible for.  So, it acts as a filter.
        /// </summary>

        private void
        AddFilterForSnoopClasses()
        {
            foreach (Assembly assembly in HostAssemblyLoader.GetClassBrowserAssemblies()) {
                string assemblyName = assembly.FullName;
                if (!Fs.Fox.CAD.Diagnostics.Snoop.Forms.Editor.assemblyNamesToLoad.Contains(assemblyName)) {
                    Fs.Fox.CAD.Diagnostics.Snoop.Forms.Editor.assemblyNamesToLoad.Add(assemblyName);
                    m_snoopAssemblyNames.Add(assemblyName);
                }
            }
        }

        private void
        RemoveFilterForSnoopClasses()
        {
            for (int i = m_snoopAssemblyNames.Count - 1; i >= 0; i--) {
                Fs.Fox.CAD.Diagnostics.Snoop.Forms.Editor.assemblyNamesToLoad.Remove(
                    m_snoopAssemblyNames[i]);
            }
            m_snoopAssemblyNames.Clear();
        }
	}
}
