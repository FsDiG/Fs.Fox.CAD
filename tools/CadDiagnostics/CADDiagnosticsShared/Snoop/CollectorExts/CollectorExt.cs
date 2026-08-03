
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
using System.Diagnostics;

namespace Fs.Fox.CAD.Diagnostics.Snoop.CollectorExts
{
	/// <summary>
	/// Base class for CollectorExt objects.
	/// </summary>
	internal abstract class CollectorExt : IDisposable
	{
		private readonly Snoop.Collectors.Collector.CollectorExt m_handler;
		private bool m_isRegistered;

		public
		CollectorExt()
		{
		        // add ourselves to the event list of all SnoopCollectors
		    m_handler = new Snoop.Collectors.Collector.CollectorExt(CollectEvent);
		    Snoop.Collectors.Collector.OnCollectorExt += m_handler;
		    m_isRegistered = true;
		}

		/// <summary>
		/// Removes this extension from the static collector event. This makes
		/// application termination repeatable and prevents handlers from surviving
		/// a failed or partial plug-in initialization.
		/// </summary>
		public void
		Dispose()
		{
		    if (!m_isRegistered)
		        return;

		    Snoop.Collectors.Collector.OnCollectorExt -= m_handler;
		    m_isRegistered = false;
		    GC.SuppressFinalize(this);
		}

        protected abstract void
        CollectEvent(object sender, Snoop.Collectors.CollectorEventArgs e);

    }
}
