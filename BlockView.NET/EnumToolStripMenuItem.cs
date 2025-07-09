using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
//using GsRenderMode = Autodesk.AutoCAD.GraphicsSystem.RenderMode;
using System.ComponentModel;
using System.Reflection;

namespace BlockView.NET
{

	// A specialization of ToolStripMenuItem that can be used
	// to represent memberss of an enum type

	public class EnumToolStripMenuItem : ToolStripMenuItem
	{
		bool isFlag = false;
		long intval = 0L;

		EnumToolStripMenuItem( Enum value, EventHandler onClick )
			: base( GetItemText( value ) )
		{
			base.Tag = value;
			if( onClick != null )
				base.Click += onClick;
			isFlag = IsFlags( value.GetType() );
			intval = Convert.ToInt64( value );
		}

		/// <summary>
		/// Generates an EnumToolStripMenuItem for each member of an enum type,
		/// and sets the item's text to the the value of the DescriptionAttribute 
		/// applied to the member, or the member's name if there is no description
		/// attribute.
		/// 
		/// If OnClick is provided and not null, each menu item's Click event will 
		/// be handled by OnClick.
		/// 
		/// The following example enum type shows how the DescriptionAttribute can be 
		/// used to specify each member's menu item text (sorry, no localization support):
		/// 
		///	public enum MyFlags
		///	{
		///		[Description( "First item menu text" )]
		///		First = 0,
		///		
		///		[Description( "Second item menu text" )]
		///		Second = 1,
		///		
		///		[Description( "Thrid item menu text" )]
		///		Third = 2
		///	}
		///}
		/// 
		/// </summary>
		
		public static EnumToolStripMenuItem[] CreateItems<T>() where T : struct 
		{
			return CreateItems( typeof(T), null );
		}

		public static EnumToolStripMenuItem[] CreateItems<T>( EventHandler onClick ) where T : struct
		{
			return CreateItems( typeof( T ), onClick );
		}

		public static EnumToolStripMenuItem[] CreateItems<T>( IEnumerable<T> items, EventHandler onClick ) where T : struct
		{
			return CreateItems( items.Cast<Enum>(), onClick );
		}

		public static EnumToolStripMenuItem[] CreateItems( Type enumType )
		{
			return CreateItems( enumType, null );
		}

		public static EnumToolStripMenuItem[] CreateItems( Type enumType, EventHandler onClick )
		{
			if( enumType == null )
				throw new ArgumentNullException( "enumType" );
			if( !enumType.IsEnum )
				throw new ArgumentException( "Requires an Enum type" );
			return CreateItems( Enum.GetValues( enumType ).Cast<Enum>().Distinct(), onClick );
		}

		// This override can be used to selectively add a specific set of members:
		public static EnumToolStripMenuItem[] CreateItems( IEnumerable<Enum> values, EventHandler onClick )
		{
			return values.Select( v => new EnumToolStripMenuItem( v, onClick ) ).ToArray();
		}

		public static void UpdateCheckedState( ToolStripItemCollection items, Enum value )
		{
			foreach( EnumToolStripMenuItem item in items.OfType<EnumToolStripMenuItem>() )
			{
				item.UpdateCheckedState( value );
			}
		}

		public void UpdateCheckedState( Enum value )
		{
			if( base.Tag != null && base.Tag.GetType() == value.GetType() )
			{
				Int64 flags = Convert.ToInt64( value );
				base.Checked = isFlag ? ( ( intval & flags ) == intval ) : intval == flags;
			}
		}

		static string GetItemText( Enum value )
		{
			FieldInfo fi = value.GetType().GetField( value.ToString() );
			if( fi != null )
			{
				DescriptionAttribute[] att = (DescriptionAttribute[])
					fi.GetCustomAttributes( typeof( DescriptionAttribute ), false );
				if( att != null && att.Length > 0 )
				{
					string desc = att[0].Description;
					if( !string.IsNullOrEmpty( desc ) )
						return desc;
				}
			}
			return value.ToString();
		}

		public Enum Value
		{
			get
			{
				return (Enum) base.Tag;
			}
		}

		public Int64 IntVal
		{
			get
			{
				return intval;
			}
		}

		static bool IsFlags( Type enumType )
		{
			var a = enumType.GetCustomAttributes( typeof( FlagsAttribute ), false );
			return a != null && a.Length > 0;
		}
	}

}
