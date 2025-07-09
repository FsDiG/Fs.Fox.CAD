using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace BlockView.NET
{
	public static class BlockViewExtensionMethods
	{
		/// <summary>
		/// 
		/// This extension method can be called on any IEnumerable<T> 
		/// where the element type T implements the IDisposable interface.
		/// It's purpose is to guarantee the entire sequence is disposed
		/// even if an exception is thrown by the call to Dispose() on any
		/// element in the sequence.
		/// 
		/// </summary>
	
		public static void DisposeItems<T>( this IEnumerable<T> items ) where T : IDisposable
		{
			DisposeItems( items.GetEnumerator() );
		}

		static void DisposeItems( IEnumerator e ) 
		{
			while( e.MoveNext() )
			{
				try
				{
					IDisposable disposable = e.Current as IDisposable;
					if( disposable != null )
						disposable.Dispose();
				}
				catch
				{
					DisposeItems( e );
					throw;
				}
			}

		}
	}

	/// <summary>
	///
	/// [TT]: Detecting design-time execution:
	/// 
	/// The IsDesignMode method of this class should be used in preference
	/// to the DesignMode property of Control. The latter is only true when
	/// called on a control that is being designed. It is NOT true when it
	/// is called on an instance of a control that is a child of a control 
	/// that's being designed (e.g., the GsPreviewCtrl), or when called on 
	/// an instance of a base type of a control that is being designed.
	/// 
	/// For example, within the GsPreviewCtrl class, if the GsPreviewCtrl's
	/// DesignMode property is read when the BlockViewDialog is open in the 
	/// designer, it will return false because the GsPreviewCtrl is not what
	/// is being designed (the BlockViewDialog is what's being designed).
	/// 
	/// </summary>

	public static class Utils
	{
		static bool designMode = string.Equals(
			Path.GetFileNameWithoutExtension( Process.GetCurrentProcess().MainModule.FileName ),
			"devenv",
			StringComparison.OrdinalIgnoreCase );

		public static bool IsDesignMode( this Control control )
		{
			return designMode;
		}

		public static bool DesignMode
		{
			get
			{
				return designMode;
			}
		}

	}



}
