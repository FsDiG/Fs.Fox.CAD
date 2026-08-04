
//
// (C) Copyright 2006 by Autodesk, Inc.
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
using System.Collections;
using System.Collections.Generic;
using System.Text;

using AcApp = Autodesk.AutoCAD.ApplicationServices;
using AcRx = Autodesk.AutoCAD.Runtime;

namespace Fs.Fox.CAD.Diagnostics {

    class AppDocReactor {

        private bool m_documentCreatedEnabled;
        private bool m_documentToBeDestroyedEnabled;

        public
        AppDocReactor()
        {
        }

        public void
        EnableEvents()
        {
            if (m_documentCreatedEnabled && m_documentToBeDestroyedEnabled)
                return;

            AcApp.DocumentCollection docs = AcApp.Application.DocumentManager;

            if (!m_documentCreatedEnabled) {
                docs.DocumentCreated += new AcApp.DocumentCollectionEventHandler(event_DocumentCreated);
                m_documentCreatedEnabled = true;
            }
            if (!m_documentToBeDestroyedEnabled) {
                docs.DocumentToBeDestroyed += new AcApp.DocumentCollectionEventHandler(event_DocumentToBeDestroyed);
                m_documentToBeDestroyedEnabled = true;
            }
        }

        public void
        DisableEvents()
        {
            if (!m_documentCreatedEnabled && !m_documentToBeDestroyedEnabled)
                return;

            AcApp.DocumentCollection docs = AcApp.Application.DocumentManager;

                // Detach in reverse registration order. The legacy snapshot left
                // DocumentCreated attached because of an old SnoopEd assertion;
                // retaining it would leak this application instance after unload.
            if (m_documentToBeDestroyedEnabled) {
                docs.DocumentToBeDestroyed -= new AcApp.DocumentCollectionEventHandler(event_DocumentToBeDestroyed);
                m_documentToBeDestroyedEnabled = false;
            }
            if (m_documentCreatedEnabled) {
                docs.DocumentCreated -= new AcApp.DocumentCollectionEventHandler(event_DocumentCreated);
                m_documentCreatedEnabled = false;
            }
        }

        private void
        event_DocumentCreated(object sender, AcApp.DocumentCollectionEventArgs e)
        {
                // if the reactor instance exists and if the relevant checkbox is ticked in the Reactors UI
            if (Fs.Fox.CAD.Diagnostics.Reactors.Forms.EventsForm.m_dbEvents.AreEventsEnabled) {
                Fs.Fox.CAD.Diagnostics.Reactors.Forms.EventsForm.m_dbEvents.EnableEvents(e.Document.Database);  // will turn on just for this new document
            }

            if (Fs.Fox.CAD.Diagnostics.Reactors.Forms.EventsForm.m_dbObjEvents.AreEventsEnabled) {
                Fs.Fox.CAD.Diagnostics.Reactors.Forms.EventsForm.m_dbObjEvents.EnableEvents(e.Document.Database);  // will turn on just for this new document
            }

            if (Fs.Fox.CAD.Diagnostics.Reactors.Forms.EventsForm.m_docEvents.AreEventsEnabled) {
                Fs.Fox.CAD.Diagnostics.Reactors.Forms.EventsForm.m_docEvents.EnableEvents(e.Document);          // will turn on just for this new document
            }

            if (Fs.Fox.CAD.Diagnostics.Reactors.Forms.EventsForm.m_edEvents.AreEventsEnabled) {
                Fs.Fox.CAD.Diagnostics.Reactors.Forms.EventsForm.m_edEvents.EnableEvents(e.Document.Editor);    // will turn on just for this new document
            }

            if (Fs.Fox.CAD.Diagnostics.Reactors.Forms.EventsForm.m_gsEvents.AreEventsEnabled) {
                Fs.Fox.CAD.Diagnostics.Reactors.Forms.EventsForm.m_gsEvents.EnableEvents(e.Document.GraphicsManager);    // will turn on just for this new document
            }
        }

        private void
        event_DocumentToBeDestroyed(object sender, AcApp.DocumentCollectionEventArgs e)
        {
                // if the reactor instance exists and if the relevant checkbox is ticked in the Reactors UI
            if (Fs.Fox.CAD.Diagnostics.Reactors.Forms.EventsForm.m_dbEvents.AreEventsEnabled) {
                Fs.Fox.CAD.Diagnostics.Reactors.Forms.EventsForm.m_dbEvents.DisableEvents(e.Document.Database); // will turn off just for this new document
            }

            if (Fs.Fox.CAD.Diagnostics.Reactors.Forms.EventsForm.m_dbObjEvents.AreEventsEnabled) {
                Fs.Fox.CAD.Diagnostics.Reactors.Forms.EventsForm.m_dbObjEvents.DisableEvents(e.Document.Database); // will turn off just for this new document
            }

            if (Fs.Fox.CAD.Diagnostics.Reactors.Forms.EventsForm.m_docEvents.AreEventsEnabled) {
                Fs.Fox.CAD.Diagnostics.Reactors.Forms.EventsForm.m_docEvents.DisableEvents(e.Document);         // will turn off just for this new document
            }

            if (Fs.Fox.CAD.Diagnostics.Reactors.Forms.EventsForm.m_edEvents.AreEventsEnabled) {
                Fs.Fox.CAD.Diagnostics.Reactors.Forms.EventsForm.m_edEvents.DisableEvents(e.Document.Editor);   // will turn off just for this new document
            }

            if (Fs.Fox.CAD.Diagnostics.Reactors.Forms.EventsForm.m_gsEvents.AreEventsEnabled) {
                Fs.Fox.CAD.Diagnostics.Reactors.Forms.EventsForm.m_gsEvents.DisableEvents(e.Document.GraphicsManager);   // will turn off just for this new document
            }

        }

    }
}
