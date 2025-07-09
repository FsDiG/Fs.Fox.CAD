using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsSystem;

//////////////////////////////////////////////////////////////////////////
// by Fenton Webb, DevTech, Autodesk. 23/05/2008

/// Revised by Tony Tanzillo, 10/2012, to correct issues that
/// prevented design-time instantiation of this control. Note
/// that there are no claims of ownership or warrantee of any
/// kind on this revised code, and the original owner retains 
/// all rights to same. Terms of use are subject to the terms
/// of the original publisher of the work.
/// 
/// See the OnSizeChanged() method for a brief discussion and
/// explaination of why design-time failures occur when AutoCAD
/// APIs and types are used in designable components, and how 
/// to avoid or correct that problem.
/// 
/// The basic refactoring pattern for avoiding design-time
/// failures resulting from use of AutoCAD types in methods
/// that execute in the designer is:
/// 
/// For any given method (represented as "MyMethod" in the
/// following example) that executes in the designer, the 
/// method is refactored thusly:
/// 
/// Before refactoring:
/// 
/// void MyMethod()
/// {
///    SomeOtherMethod();   // non-AutoCAD API call
///    
///    AutoCADMethod();     // A call to some AutoCAD API
/// }                       // that will likely cause a                      
///                         // a failure in the designer
///                         
/// The above after refactoring:
/// 
/// void MyMethod()
/// {
///    SomeOtherMethod(); // not an AutoCAD API call so it can stay here
///    
///	if( ! Utils.IsDesignMode )   // code that uses AutoCAD types
///	   InternalMyMethod();       // is refactored out to another
/// }                              // new method, and called from here
///                                // when not running in the designer
///                              
/// A new method is added, which encapsulates and isolates
/// the use of all AutoCAD types, and is called conditionally
/// from the method which the code was removed from, only when
/// not running in the designer:
/// 
/// void InternalMyMethod()
/// {
///    AutoCADMethod();
/// }
/// 
/// If you are wondering why it is necessary to refactor the
/// code into two methods, as opposed to simply guarding the
/// code that uses AutoCAD types with a relatively-simpler 
/// 'if( ! Utils.IsDesignMode ) {...}' test, the reason is 
/// because it is the jit'ing of the method itself that is 
/// causing the failure, which happens long before there is 
/// any use of AutoCAD types in the method.
///
/// Additional fixes and changes:
/// 
/// 1. The base type is Control rather than Panel. There is no
///    need to derive from a Panel for a control of this type.
///    
/// 2. GsPreviewCtrl.Designer.cs was removed as it serves no
///    purpose (the control does not need to be designable).
///
/// 3. In the included BlockViewDialog.cs and its .Designer.cs
///    file, the ToolStripMenuItems for selecting the RenderMode
///    and View Style were deleted, and are now created dynamically
///    at runtime (see EnumToolStripMenuItem.cs). This allowed the
///    elimination of vast amounts of code in BlockViewDialog.cs,
///    which did little other than handle each menu item's click
///    event, and check/uncheck related menu items.

namespace BlockView.NET
{
	public class GsPreviewCtrl : Control
	{
		public GsPreviewCtrl()
		{
		}



		// current dwg
		public Database mCurrentDwg = null;
		// Gs specific
		public Autodesk.AutoCAD.GraphicsSystem.Manager mpManager = null;
		public Autodesk.AutoCAD.GraphicsSystem.Device mpDevice = null;
		public Autodesk.AutoCAD.GraphicsSystem.Model mpModel = null;
		public Autodesk.AutoCAD.GraphicsSystem.View mpView = null;
		// flags
		public bool mZooming = false;
		public bool mMouseDown = false;
		public bool mMouseMoving = false;
		public bool mbPanning = false;
		public bool mbOrbiting = false;
		public System.Drawing.Point mStartPoint;
		public System.Drawing.Point mEndPoint;

		public static bool IsDesignMode
		{
			get
			{
				return Utils.IsDesignMode( null );
			}
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				// clear up the GS view gadgets
				if( !IsDesignMode )
					ClearAll();
			}
			base.Dispose( disposing );

		}

		// called from InitDrawingControl, does GsPreviewCtrl specific initialization
		public void Init( Document doc, Database db )
		{
			mCurrentDwg = db;
			// check to see if the manager is already initalised
			if( null == mpManager )
			{
				// if not let's set it up
				mpManager = doc.GraphicsManager;
				// here's some test code to show the GS Events working
				mpManager.ViewToBeDestroyed += new ViewToBeDestroyedEventHandler( GSUtil.mpManager_ViewToBeDestroyed );
				mpManager.ViewWasCreated += new ViewWasCreatedEventHandler( GSUtil.mpManager_ViewWasCreated );
				mpManager.GsToBeUnloaded += new GsToBeUnloadedEventHandler( GSUtil.mpManager_GsToBeUnloaded );
				mpManager.ConfigWasModified += new ConfigWasModifiedEventHandler( GSUtil.mpManager_ConfigWasModified );

                KernelDescriptor descriptor = new KernelDescriptor();
                descriptor.addRequirement(Autodesk.AutoCAD.UniqueString.Intern("3D Drawing"));
                GraphicsKernel kernal = Manager.AcquireGraphicsKernel(descriptor);
				// now create the Gs control, create the autocad device passing the handle to the Windows panel
				mpDevice = mpManager.CreateAutoCADDevice(kernal, this.Handle );
				// resize the device to the panel size
				mpDevice.OnSize( this.Size );
				// now create a new gs view
				mpView = new Autodesk.AutoCAD.GraphicsSystem.View();
				// and create the model
                mpModel = mpManager.CreateAutoCADModel(kernal);

				// add the view to the device
				mpDevice.Add( mpView );
			}
		}


		public void ClearAll()
		{
			if( !IsDesignMode )
			{
				if( mpDevice != null )
				{
					bool b = mpDevice.Erase( mpView );
				}
				if( mpView != null )
				{
					mpView.EraseAll();
					mpView.Dispose();
					mpView = null;
				}
				if( mpManager != null )
				{
					if( mpModel != null )
					{
						mpModel.Dispose();
						mpModel = null;
					}

					if( mpDevice != null )
					{
						mpDevice.Dispose();
						mpDevice = null;
					}
					mpManager = null;
				}
			}
		}

		// [TT]: Refactored to exclude AutoCAD types from methods that run in the designer:

		public void ErasePreview()
		{
			if( !IsDesignMode )
				InternalErasePreview();
		}

		public void InternalErasePreview()
		{
			if( mpView != null )
				mpView.EraseAll();
			if( mpManager != null && mpModel != null )
			{
				mpModel.Dispose();
				mpModel = null;
			}
		}


		/// [TT]: Depreciated (see http://support.microsoft.com/kb/314945) in
		/// favor of the following replacement (although not exactly the same,
		/// it seems to be more than adequate, and doesn't exhibit the same
		/// problems the original code has with erasure).

		//// draws the zoom window rectangle
		//public void RubberRectangle( System.Drawing.Point startPt, System.Drawing.Point endPt )
		//{
		//   // create a new instance of my GDI helper class, it seems too tricky to create simple XOR graphics
		//   // in .NET so let's resort to direct calls instead

		//   GSUtil.RubberbandRectangle rect = new GSUtil.RubberbandRectangle();
		//   using( System.Drawing.Graphics graphics = this.CreateGraphics() )
		//   {
		//      // draw the XOR rectangle 
		//      rect.DrawXORRectangle( graphics, startPt, endPt );
		//   }
		//}


		/// From http://support.microsoft.com/kb/314945
		private void RubberRectangle( Point p1, Point p2 )
		{
			Rectangle rc = new Rectangle();

			// Convert the points to screen coordinates.
			p1 = PointToScreen( p1 );
			p2 = PointToScreen( p2 );
			// Normalize the rectangle.
			if( p1.X < p2.X )
			{
				rc.X = p1.X;
				rc.Width = p2.X - p1.X;
			}
			else
			{
				rc.X = p2.X;
				rc.Width = p1.X - p2.X;
			}
			if( p1.Y < p2.Y )
			{
				rc.Y = p1.Y;
				rc.Height = p2.Y - p1.Y;
			}
			else
			{
				rc.Y = p2.Y;
				rc.Height = p1.Y - p2.Y;
			}
			// Draw the reversible frame.
			ControlPaint.DrawReversibleFrame( rc, Color.White, FrameStyle.Dashed );
		}

		// [TT]: Refactored to exclude AutoCAD types from methods that run in the designer:

		public void refreshView()
		{
			if( !IsDesignMode )
			{
				InternalRefreshView();
			}
		}

		// This method will never be jit'ed in the designer:

		void InternalRefreshView()
		{
			if( mpView != null )
			{
				mpView.Invalidate();
				mpView.Update();
			}
		}

		protected override void OnPaint( System.Windows.Forms.PaintEventArgs e )
		{
			base.OnPaint( e );
			refreshView();
		}

		// [TT]: Refactored to exclude AutoCAD types from methods that run in the designer:

		protected override void OnMouseMove( System.Windows.Forms.MouseEventArgs e )
		{
			base.OnMouseMove( e );
			if( !IsDesignMode )
			{
				InternalOnMouseMove( e );
			}
		}

		private void InternalOnMouseMove( System.Windows.Forms.MouseEventArgs e )
		{
			if( mpView != null )
			{
				if( mZooming )
				{
					// if left button is down
					if( mMouseDown )
					{
						if( mMouseMoving )
						{
							// erase the old rectangle
							RubberRectangle( mStartPoint, mEndPoint );
						}
						// draw the new rectangle
						RubberRectangle( mStartPoint, e.Location );
						mMouseMoving = true;
						// save the new point for next time
						mEndPoint = e.Location;
					}
				}
				else
				{
					if( mbPanning )
					{
						//transform the point from device coordinates to
						//world coordinates
						Vector3d pan_vec = new Vector3d( -( e.Location.X - mStartPoint.X ), e.Location.Y - mStartPoint.Y, 0 );
						pan_vec.TransformBy( mpView.ViewingMatrix * mpView.WorldToDeviceMatrix.Inverse() );
						mpView.Dolly( pan_vec );
						refreshView();
						mStartPoint = e.Location;
					}
					if( mbOrbiting )
					{
						double Half_Pi = 1.570796326795;

                        System.Drawing.Rectangle view_rect = new System.Drawing.Rectangle((int)mpView.ViewportExtents.MinPoint.X, (int)mpView.ViewportExtents.MinPoint.Y,
                                                                               (int)(mpView.ViewportExtents.MaxPoint.X - mpView.ViewportExtents.MinPoint.X),
                                                                               (int)(mpView.ViewportExtents.MaxPoint.Y - mpView.ViewportExtents.MinPoint.Y));

						int nViewportX = ( view_rect.Right - view_rect.Left ) + 1;
						int nViewportY = ( view_rect.Bottom - view_rect.Top ) + 1;

						int centerX = (int) ( nViewportX / 2.0f + view_rect.Left );
						int centerY = (int) ( nViewportY / 2.0f + view_rect.Top );

						double radius = System.Math.Min( nViewportX, nViewportY ) * 0.4f;

						// compute two vectors from last and new cursor positions:

						Vector3d last_vector = new Vector3d( ( mStartPoint.X - centerX ) / radius,
							 -( mStartPoint.Y - centerY ) / radius,
							 0.0 );
						if( last_vector.LengthSqrd > 1.0 )     // outside the radius
						{
							double x = last_vector.X / last_vector.Length;
							double y = last_vector.Y / last_vector.Length;
							double z = last_vector.Z / last_vector.Length;
							last_vector = new Vector3d( x, y, z );

						}
						else
						{
							double x = last_vector.X;
							double y = last_vector.Y;
							double z = System.Math.Sqrt( 1.0 - last_vector.X * last_vector.X - last_vector.Y * last_vector.Y );
							last_vector = new Vector3d( x, y, z );
						}

						Vector3d new_vector = new Vector3d( ( e.Location.X - centerX ) / radius, -( e.Location.Y - centerY ) / radius, 0.0 );

						if( new_vector.LengthSqrd > 1.0 )     // outside the radius
						{
							double x = new_vector.X / new_vector.Length;
							double y = new_vector.Y / new_vector.Length;
							double z = new_vector.Z / new_vector.Length;
							new_vector = new Vector3d( x, y, z );

						}
						else
						{
							double x = new_vector.X;
							double y = new_vector.Y;
							double z = System.Math.Sqrt( 1.0 - new_vector.X * new_vector.X - new_vector.Y * new_vector.Y );
							new_vector = new Vector3d( x, y, z );
						}

						// determine angles for proper sequence of camera manipulations:

						Vector3d rotation_vector = last_vector;
						rotation_vector = rotation_vector.CrossProduct( new_vector );  // rotation_vector = last_vector x new_vector

						Vector3d work_vector = rotation_vector;
						work_vector = new Vector3d( work_vector.X, work_vector.Y, 0.0f );                      // projection of rotation_vector onto xy plane

						double roll_angle = System.Math.Atan2( work_vector.X,
							 work_vector.Y );        // assuming that the camera's up vector is "up",
						// this computes the angle between the up vector 
						// and the work vector, which is the roll required
						// to make the up vector coincident with the rotation_vector
						double length = rotation_vector.Length;
						double orbit_y_angle = ( length != 0.0 ) ? System.Math.Acos( rotation_vector.Z / length ) + Half_Pi : Half_Pi;                   // represents inverse cosine of the dot product of the
						if( length > 1.0f )                                              // rotation_vector and the up_vector divided by the
							length = 1.0f;                                              // magnitude of both vectors.  We add pi/2 because we 
						// are making the up-vector parallel to the the rotation
						double rotation_angle = System.Math.Asin( length );                // vector ... up-vector is perpin. to the eye-vector.

						// perform view manipulations

						mpView.Roll( roll_angle );               // 1: roll camera to make up vector coincident with rotation vector
						mpView.Orbit( 0.0f, orbit_y_angle );     // 2: orbit along y to make up vector parallel with rotation vector
						mpView.Orbit( rotation_angle, 0.0f );     // 3: orbit along x by rotation angle
						mpView.Orbit( 0.0f, -orbit_y_angle );     // 4: orbit along y by the negation of 2
						mpView.Roll( -roll_angle );               // 5: roll camera by the negation of 1
						refreshView();
						mStartPoint = e.Location;
					}
				}
			}
		}

		// [TT]: Refactored to exclude AutoCAD types from methods that run in the designer:
		protected override void OnMouseDown( System.Windows.Forms.MouseEventArgs e )
		{
			base.OnMouseDown( e );
			if( !IsDesignMode )
			{
				InternalOnMouseDown( e );
			}
		}

		private void InternalOnMouseDown( System.Windows.Forms.MouseEventArgs e )
		{
			if( mpView != null )
			{
				if( e.Button == System.Windows.Forms.MouseButtons.Left )
				{
					// if zooming
					if( mZooming )
					{
						mMouseDown = true;
						mMouseMoving = false;
					}
					else
					{
						mbOrbiting = true;
						this.Focus();
					}

				}
				else if( e.Button == System.Windows.Forms.MouseButtons.Middle )
				{
					mbPanning = true;
				}
				mStartPoint = e.Location;
			}
		}

		// [TT]: Refactored to exclude AutoCAD types from methods that run in the designer:
		protected override void OnMouseUp( System.Windows.Forms.MouseEventArgs e )
		{
			base.OnMouseUp( e );
			if( !IsDesignMode )
			{
				InternalOnMouseUp( e );
			}
		}

		private void InternalOnMouseUp( System.Windows.Forms.MouseEventArgs e )
		{
			if( mpView != null )
			{
				if( e.Button == System.Windows.Forms.MouseButtons.Left )
				{
					if( mZooming && mMouseDown )
					{
						// end zoom
						mZooming = false;
						mMouseDown = false;
						mMouseMoving = false;


						mpView.ZoomWindow( new Point2d( mStartPoint.X, this.Bottom - mStartPoint.Y ), new Point2d( mEndPoint.X, this.Bottom - mEndPoint.Y ) );

						refreshView();
					}
					else
					{
						mbOrbiting = false;
					}
				}
				else if( e.Button == System.Windows.Forms.MouseButtons.Middle )
				{
					mbPanning = false;
				}
			}
		}

		/// [TT]: Refactoring comments:
		/// 
		/// The following is a typical example of the refactoring that was
		/// done in various methods of this class, that allow the control 
		/// to be instantiated in the designer. The OnSizeChanged() method 
		/// is called in the designer and had previously contained AutoCAD 
		/// types, which is what crashes the designer and/or visual studio.
		/// 
		/// Contrary to popular belief, the failure is not a result of an
		/// attempt to call an AutoCAD API from a method that runs in the
		/// designer. Even if a call to an AutoCAD API does not occur in
		/// the designer (perhaps because the programmer checked to see if
		/// they were running in the designer and skipped the call in that
		/// case), the failure still occurs, because AutoCAD types appear
		/// in the method, and is what triggers the loading of the AutoCAD 
		/// assemblies when the method is jit'ed in the designer. So, it is
		/// not enough to merely avoid the use of AutoCAD types using some
		/// conditional branching code, as the AutoCAD types are still in
		/// the method that runs in the designer, and must be removed from 
		/// same.
		/// 
		/// To way to fix that, is to just move all AutoCAD types out of 
		/// methods that are jit'ed and run in the designer, because it is
		/// the jit'ing of those methods in the designer that triggers the 
		/// loading of the AutoCAD assemblies, and the resulting failure.
		///
		/// The commented-out code below was moved to another, new method
		/// (OnInternalSizeChanged) below. That method will only be jit'ed if 
		/// it is actually called, which will never happen in the designer, 
		/// because the test involving IsDesignMode will skip the call to 
		/// InternalOnSizeChanged(), preventing it from being jit'ed in the 
		/// designer.
		/// 
		/// The same fix was applied to various other methods of this class
		/// to ensure that if they run in the designer, they will not cause 
		/// a failure. 
		/// 
		/// Tip: The fix is easy to make using the Refactor->Extract Method 
		/// command. Just select the parts of the method that you want to 
		/// move to another method, and then from the Refactor menu choose 
		/// "Extract Method...", give the new method a name and yer done.
		/// 

		// [TT]: Refactored to exclude AutoCAD types from methods that run in the designer:
		protected override void OnSizeChanged( EventArgs e )
		{
			base.OnSizeChanged( e );

			/// No AutoCAD types can be in this method, so we've
			/// moved the following code to another method which
			/// appears below, and only call that method when not 
			/// running in the designer:

			//if( mpDevice != null )
			//{
			//   mpDevice.OnSize( this.Size );
			//}

			if( !IsDesignMode )
			{
				InternalOnSizeChanged();
			}
		}

		// This method will never be jit'ed in the designer
		void InternalOnSizeChanged()
		{
			if( mpDevice != null )
			{
				mpDevice.OnSize( this.Size );
			}
		}
	}

	/// <summary>
	/// [TT]: Depreciated (using ControlPaint.DrawReversibleFrame() instead)
	/// </summary>

	public class GSUtil
	{
		public const String strActive = "*Active";
		public const String strActiveSettings = "ACAD_RENDER_ACTIVE_SETTINGS";
		public static void CustomUpdate( System.IntPtr parmeter, int left, int right, int bottom, int top )
		{
			MessageBox.Show( "Left:" + left + "Right" + right + "Bottom" + bottom + "Top" + top );
		}
		public static System.Drawing.Color[] MyAcadColorPs = 
    {
      System.Drawing.Color.FromArgb(255, 0, 0, 255),    //----- 0 - lets make it red for an example
      //{255, 255, 255, 255},//----- 0 - ByBlock - White
      System.Drawing.Color.FromArgb(255, 0, 0, 255),    //----- 1 - Red 
      System.Drawing.Color.FromArgb(255, 255, 0, 255),    //----- 2 - Yellow
      System.Drawing.Color.FromArgb(0, 255, 0, 255),    //----- 3 - Green
      System.Drawing.Color.FromArgb(0, 255, 255, 255),    //----- 4 - Cyan
      System.Drawing.Color.FromArgb(0, 0, 255, 255),    //----- 5 - Blue
      System.Drawing.Color.FromArgb(255, 0, 255, 255),    //----- 6 - Magenta
      System.Drawing.Color.FromArgb(255, 0, 0, 255),    //----- 7 - More red Red 
      System.Drawing.Color.FromArgb(255, 0, 0, 255),    //----- 8 - More red Red 
      System.Drawing.Color.FromArgb(255, 0, 0, 255),    //----- 9 - More red Red 
      /*System.Drawing.Color.FromArgb(255, 255, 255, 255),//----- 7 - White
      System.Drawing.Color.FromArgb(255, 255, 255, 255),//----- 8
      System.Drawing.Color.FromArgb(255, 255, 255, 255),//----- 9*/
      System.Drawing.Color.FromArgb(255, 0, 0, 255),    //----- 10
      System.Drawing.Color.FromArgb(255, 127, 127, 255),//----- 11
      System.Drawing.Color.FromArgb(165, 0, 0, 255),    //----- 12
      System.Drawing.Color.FromArgb(165, 82, 82, 255),    //----- 13
      System.Drawing.Color.FromArgb(127, 0, 0, 255),    //----- 14
      System.Drawing.Color.FromArgb(127, 63, 63, 255),    //----- 15
      System.Drawing.Color.FromArgb(76, 0, 0, 255),        //----- 16
      System.Drawing.Color.FromArgb(76, 38, 38, 255),    //----- 17
      System.Drawing.Color.FromArgb(38, 0, 0, 255),        //----- 18
      System.Drawing.Color.FromArgb(38, 19, 19, 255),    //----- 19
      System.Drawing.Color.FromArgb(255, 63, 0, 255),    //----- 20
      System.Drawing.Color.FromArgb(255, 159, 127, 255),//----- 21
      System.Drawing.Color.FromArgb(165, 41, 0, 255),    //----- 22
      System.Drawing.Color.FromArgb(165, 103, 82, 255),    //----- 23
      System.Drawing.Color.FromArgb(127, 31, 0, 255),    //----- 24
      System.Drawing.Color.FromArgb(127, 79, 63, 255),    //----- 25
      System.Drawing.Color.FromArgb(76, 19, 0, 255),    //----- 26
      System.Drawing.Color.FromArgb(76, 47, 38, 255),    //----- 27
      System.Drawing.Color.FromArgb(38, 9, 0, 255),        //----- 28
      System.Drawing.Color.FromArgb(38, 23, 19, 255),    //----- 29
      System.Drawing.Color.FromArgb(255, 127, 0, 255),    //----- 30
      System.Drawing.Color.FromArgb(255, 191, 127, 255),//----- 31
      System.Drawing.Color.FromArgb(165, 82, 0, 255),    //----- 32
      System.Drawing.Color.FromArgb(165, 124, 82, 255),    //----- 33
      System.Drawing.Color.FromArgb(127, 63, 0, 255),    //----- 34
      System.Drawing.Color.FromArgb(127, 95, 63, 255),    //----- 35
      System.Drawing.Color.FromArgb(76, 38, 0, 255),    //----- 36
      System.Drawing.Color.FromArgb(76, 57, 38, 255),    //----- 37
      System.Drawing.Color.FromArgb(38, 19, 0, 255),    //----- 38
      System.Drawing.Color.FromArgb(38, 28, 19, 255),    //----- 39
      System.Drawing.Color.FromArgb(255, 191, 0, 255),    //----- 40
      System.Drawing.Color.FromArgb(255, 223, 127, 255),//----- 41
      System.Drawing.Color.FromArgb(165, 124, 0, 255),    //----- 42
      System.Drawing.Color.FromArgb(165, 145, 82, 255),    //----- 43
      System.Drawing.Color.FromArgb(127, 95, 0, 255),    //----- 44
      System.Drawing.Color.FromArgb(127, 111, 63, 255),    //----- 45
      System.Drawing.Color.FromArgb(76, 57, 0, 255),    //----- 46
      System.Drawing.Color.FromArgb(76, 66, 38, 255),    //----- 47
      System.Drawing.Color.FromArgb(38, 28, 0, 255),    //----- 48
      System.Drawing.Color.FromArgb(38, 33, 19, 255),    //----- 49
      System.Drawing.Color.FromArgb(255, 255, 0, 255),    //----- 50
      System.Drawing.Color.FromArgb(255, 255, 127, 255),//----- 51
      System.Drawing.Color.FromArgb(165, 165, 0, 255),    //----- 52
      System.Drawing.Color.FromArgb(165, 165, 82, 255),    //----- 53
      System.Drawing.Color.FromArgb(127, 127, 0, 255),    //----- 54
      System.Drawing.Color.FromArgb(127, 127, 63, 255),    //----- 55
      System.Drawing.Color.FromArgb(76, 76, 0, 255),    //----- 56
      System.Drawing.Color.FromArgb(76, 76, 38, 255),    //----- 57
      System.Drawing.Color.FromArgb(38, 38, 0, 255),    //----- 58
      System.Drawing.Color.FromArgb(38, 38, 19, 255),    //----- 59
      System.Drawing.Color.FromArgb(191, 255, 0, 255),    //----- 60
      System.Drawing.Color.FromArgb(223, 255, 127, 255),//----- 61
      System.Drawing.Color.FromArgb(124, 165, 0, 255),    //----- 62
      System.Drawing.Color.FromArgb(145, 165, 82, 255),    //----- 63
      System.Drawing.Color.FromArgb(95, 127, 0, 255),    //----- 64
      System.Drawing.Color.FromArgb(111, 127, 63, 255),    //----- 65
      System.Drawing.Color.FromArgb(57, 76, 0, 255),    //----- 66
      System.Drawing.Color.FromArgb(66, 76, 38, 255),    //----- 67
      System.Drawing.Color.FromArgb(28, 38, 0, 255),    //----- 68
      System.Drawing.Color.FromArgb(33, 38, 19, 255),    //----- 69
      System.Drawing.Color.FromArgb(127, 255, 0, 255),    //----- 70
      System.Drawing.Color.FromArgb(191, 255, 127, 255),//----- 71
      System.Drawing.Color.FromArgb(82, 165, 0, 255),    //----- 72
      System.Drawing.Color.FromArgb(124, 165, 82, 255),    //----- 73
      System.Drawing.Color.FromArgb(63, 127, 0, 255),    //----- 74
      System.Drawing.Color.FromArgb(95, 127, 63, 255),    //----- 75
      System.Drawing.Color.FromArgb(38, 76, 0, 255),    //----- 76
      System.Drawing.Color.FromArgb(57, 76, 38, 255),    //----- 77
      System.Drawing.Color.FromArgb(19, 38, 0, 255),    //----- 78
      System.Drawing.Color.FromArgb(28, 38, 19, 255),    //----- 79
      System.Drawing.Color.FromArgb(63, 255, 0, 255),    //----- 80
      System.Drawing.Color.FromArgb(159, 255, 127, 255),//----- 81
      System.Drawing.Color.FromArgb(41, 165, 0, 255),    //----- 82
      System.Drawing.Color.FromArgb(103, 165, 82, 255),    //----- 83
      System.Drawing.Color.FromArgb(31, 127, 0, 255),    //----- 84
      System.Drawing.Color.FromArgb(79, 127, 63, 255),    //----- 85
      System.Drawing.Color.FromArgb(19, 76, 0, 255),    //----- 86
      System.Drawing.Color.FromArgb(47, 76, 38, 255),    //----- 87
      System.Drawing.Color.FromArgb(9, 38, 0, 255),        //----- 88
      System.Drawing.Color.FromArgb(23, 38, 19, 255),    //----- 89
      System.Drawing.Color.FromArgb(0, 255, 0, 255),    //----- 90
      System.Drawing.Color.FromArgb(127, 255, 127, 255),//----- 91
      System.Drawing.Color.FromArgb(0, 165, 0, 255),    //----- 92
      System.Drawing.Color.FromArgb(82, 165, 82, 255),    //----- 93
      System.Drawing.Color.FromArgb(0, 127, 0, 255),    //----- 94
      System.Drawing.Color.FromArgb(63, 127, 63, 255),    //----- 95
      System.Drawing.Color.FromArgb(0, 76, 0, 255),        //----- 96
      System.Drawing.Color.FromArgb(38, 76, 38, 255),    //----- 97
      System.Drawing.Color.FromArgb(0, 38, 0, 255),        //----- 98
      System.Drawing.Color.FromArgb(19, 38, 19, 255),    //----- 99
      System.Drawing.Color.FromArgb(0, 255, 63, 255),    //----- 100
      System.Drawing.Color.FromArgb(127, 255, 159, 255),//----- 101
      System.Drawing.Color.FromArgb(0, 165, 41, 255),    //----- 102
      System.Drawing.Color.FromArgb(82, 165, 103, 255),    //----- 103
      System.Drawing.Color.FromArgb(0, 127, 31, 255),    //----- 104
      System.Drawing.Color.FromArgb(63, 127, 79, 255),    //----- 105
      System.Drawing.Color.FromArgb(0, 76, 19, 255),    //----- 106
      System.Drawing.Color.FromArgb(38, 76, 47, 255),    //----- 107
      System.Drawing.Color.FromArgb(0, 38, 9, 255),        //----- 108
      System.Drawing.Color.FromArgb(19, 38, 23, 255),    //----- 109
      System.Drawing.Color.FromArgb(0, 255, 127, 255),    //----- 110
      System.Drawing.Color.FromArgb(127, 255, 191, 255),//----- 111
      System.Drawing.Color.FromArgb(0, 165, 82, 255),    //----- 112
      System.Drawing.Color.FromArgb(82, 165, 124, 255),    //----- 113
      System.Drawing.Color.FromArgb(0, 127, 63, 255),    //----- 114
      System.Drawing.Color.FromArgb(63, 127, 95, 255),    //----- 115
      System.Drawing.Color.FromArgb(0, 76, 38, 255),    //----- 116
      System.Drawing.Color.FromArgb(38, 76, 57, 255),    //----- 117
      System.Drawing.Color.FromArgb(0, 38, 19, 255),    //----- 118
      System.Drawing.Color.FromArgb(19, 38, 28, 255),    //----- 119
      System.Drawing.Color.FromArgb(0, 255, 191, 255),    //----- 120
      System.Drawing.Color.FromArgb(127, 255, 223, 255),//----- 121
      System.Drawing.Color.FromArgb(0, 165, 124, 255),    //----- 122
      System.Drawing.Color.FromArgb(82, 165, 145, 255),    //----- 123
      System.Drawing.Color.FromArgb(0, 127, 95, 255),    //----- 124
      System.Drawing.Color.FromArgb(63, 127, 111, 255),    //----- 125
      System.Drawing.Color.FromArgb(0, 76, 57, 255),    //----- 126
      System.Drawing.Color.FromArgb(38, 76, 66, 255),    //----- 127
      System.Drawing.Color.FromArgb(0, 38, 28, 255),    //----- 128
      System.Drawing.Color.FromArgb(19, 38, 33, 255),    //----- 129
      System.Drawing.Color.FromArgb(0, 255, 255, 255),    //----- 130
      System.Drawing.Color.FromArgb(127, 255, 255, 255),//----- 131
      System.Drawing.Color.FromArgb(0, 165, 165, 255),    //----- 132
      System.Drawing.Color.FromArgb(82, 165, 165, 255),    //----- 133
      System.Drawing.Color.FromArgb(0, 127, 127, 255),    //----- 134
      System.Drawing.Color.FromArgb(63, 127, 127, 255),    //----- 135
      System.Drawing.Color.FromArgb(0, 76, 76, 255),    //----- 136
      System.Drawing.Color.FromArgb(38, 76, 76, 255),    //----- 137
      System.Drawing.Color.FromArgb(0, 38, 38, 255),    //----- 138
      System.Drawing.Color.FromArgb(19, 38, 38, 255),    //----- 139
      System.Drawing.Color.FromArgb(0, 191, 255, 255),    //----- 140
      System.Drawing.Color.FromArgb(127, 223, 255, 255),//----- 141
      System.Drawing.Color.FromArgb(0, 124, 165, 255),    //----- 142
      System.Drawing.Color.FromArgb(82, 145, 165, 255),    //----- 143
      System.Drawing.Color.FromArgb(0, 95, 127, 255),    //----- 144
      System.Drawing.Color.FromArgb(63, 111, 127, 255),    //----- 145
      System.Drawing.Color.FromArgb(0, 57, 76, 255),    //----- 146
      System.Drawing.Color.FromArgb(38, 66, 76, 255),    //----- 147
      System.Drawing.Color.FromArgb(0, 28, 38, 255),    //----- 148
      System.Drawing.Color.FromArgb(19, 33, 38, 255),    //----- 149
      System.Drawing.Color.FromArgb(0, 127, 255, 255),    //----- 150
      System.Drawing.Color.FromArgb(127, 191, 255, 255),//----- 151
      System.Drawing.Color.FromArgb(0, 82, 165, 255),    //----- 152
      System.Drawing.Color.FromArgb(82, 124, 165, 255),    //----- 153
      System.Drawing.Color.FromArgb(0, 63, 127, 255),    //----- 154
      System.Drawing.Color.FromArgb(63, 95, 127, 255),    //----- 155
      System.Drawing.Color.FromArgb(0, 38, 76, 255),    //----- 156
      System.Drawing.Color.FromArgb(38, 57, 76, 255),    //----- 157
      System.Drawing.Color.FromArgb(0, 19, 38, 255),    //----- 158
      System.Drawing.Color.FromArgb(19, 28, 38, 255),    //----- 159
      System.Drawing.Color.FromArgb(0, 63, 255, 255),    //----- 160
      System.Drawing.Color.FromArgb(127, 159, 255, 255),//----- 161
      System.Drawing.Color.FromArgb(0, 41, 165, 255),    //----- 162
      System.Drawing.Color.FromArgb(82, 103, 165, 255),    //----- 163
      System.Drawing.Color.FromArgb(0, 31, 127, 255),    //----- 164
      System.Drawing.Color.FromArgb(63, 79, 127, 255),    //----- 165
      System.Drawing.Color.FromArgb(0, 19, 76, 255),    //----- 166
      System.Drawing.Color.FromArgb(38, 47, 76, 255),    //----- 167
      System.Drawing.Color.FromArgb(0, 9, 38, 255),        //----- 168
      System.Drawing.Color.FromArgb(19, 23, 38, 255),    //----- 169
      System.Drawing.Color.FromArgb(0, 0, 255, 255),    //----- 170
      System.Drawing.Color.FromArgb(127, 127, 255, 255),//----- 171
      System.Drawing.Color.FromArgb(0, 0, 165, 255),    //----- 172
      System.Drawing.Color.FromArgb(82, 82, 165, 255),    //----- 173
      System.Drawing.Color.FromArgb(0, 0, 127, 255),    //----- 174
      System.Drawing.Color.FromArgb(63, 63, 127, 255),    //----- 175
      System.Drawing.Color.FromArgb(0, 0, 76, 255),        //----- 176
      System.Drawing.Color.FromArgb(38, 38, 76, 255),    //----- 177
      System.Drawing.Color.FromArgb(0, 0, 38, 255),        //----- 178
      System.Drawing.Color.FromArgb(19, 19, 38, 255),    //----- 179
      System.Drawing.Color.FromArgb(63, 0, 255, 255),    //----- 180
      System.Drawing.Color.FromArgb(159, 127, 255, 255),//----- 181
      System.Drawing.Color.FromArgb(41, 0, 165, 255),    //----- 182
      System.Drawing.Color.FromArgb(103, 82, 165, 255),    //----- 183
      System.Drawing.Color.FromArgb(31, 0, 127, 255),    //----- 184
      System.Drawing.Color.FromArgb(79, 63, 127, 255),    //----- 185
      System.Drawing.Color.FromArgb(19, 0, 76, 255),    //----- 186
      System.Drawing.Color.FromArgb(47, 38, 76, 255),    //----- 187
      System.Drawing.Color.FromArgb(9, 0, 38, 255),        //----- 188
      System.Drawing.Color.FromArgb(23, 19, 38, 255),    //----- 189
      System.Drawing.Color.FromArgb(127, 0, 255, 255),    //----- 190
      System.Drawing.Color.FromArgb(191, 127, 255, 255),//----- 191
      System.Drawing.Color.FromArgb(82, 0, 165, 255),    //----- 192
      System.Drawing.Color.FromArgb(124, 82, 165, 255),    //----- 193
      System.Drawing.Color.FromArgb(63, 0, 127, 255),    //----- 194
      System.Drawing.Color.FromArgb(95, 63, 127, 255),    //----- 195
      System.Drawing.Color.FromArgb(38, 0, 76, 255),    //----- 196
      System.Drawing.Color.FromArgb(57, 38, 76, 255),    //----- 197
      System.Drawing.Color.FromArgb(19, 0, 38, 255),    //----- 198
      System.Drawing.Color.FromArgb(28, 19, 38, 255),    //----- 199
      System.Drawing.Color.FromArgb(191, 0, 255, 255),    //----- 200
      System.Drawing.Color.FromArgb(223, 127, 255, 255),//----- 201
      System.Drawing.Color.FromArgb(124, 0, 165, 255),    //----- 202
      System.Drawing.Color.FromArgb(145, 82, 165, 255),    //----- 203
      System.Drawing.Color.FromArgb(95, 0, 127, 255),    //----- 204
      System.Drawing.Color.FromArgb(111, 63, 127, 255),    //----- 205
      System.Drawing.Color.FromArgb(57, 0, 76, 255),    //----- 206
      System.Drawing.Color.FromArgb(66, 38, 76, 255),    //----- 207
      System.Drawing.Color.FromArgb(28, 0, 38, 255),    //----- 208
      System.Drawing.Color.FromArgb(33, 19, 38, 255),    //----- 209
      System.Drawing.Color.FromArgb(255, 0, 255, 255),    //----- 210
      System.Drawing.Color.FromArgb(255, 127, 255, 255),//----- 211
      System.Drawing.Color.FromArgb(165, 0, 165, 255),    //----- 212
      System.Drawing.Color.FromArgb(165, 82, 165, 255),    //----- 213
      System.Drawing.Color.FromArgb(127, 0, 127, 255),    //----- 214
      System.Drawing.Color.FromArgb(127, 63, 127, 255),    //----- 215
      System.Drawing.Color.FromArgb(76, 0, 76, 255),    //----- 216
      System.Drawing.Color.FromArgb(76, 38, 76, 255),    //----- 217
      System.Drawing.Color.FromArgb(38, 0, 38, 255),    //----- 218
      System.Drawing.Color.FromArgb(38, 19, 38, 255),    //----- 219
      System.Drawing.Color.FromArgb(255, 0, 191, 255),    //----- 220
      System.Drawing.Color.FromArgb(255, 127, 223, 255),//----- 221
      System.Drawing.Color.FromArgb(165, 0, 124, 255),    //----- 222
      System.Drawing.Color.FromArgb(165, 82, 145, 255),    //----- 223
      System.Drawing.Color.FromArgb(127, 0, 95, 255),    //----- 224
      System.Drawing.Color.FromArgb(127, 63, 111, 255),    //----- 225
      System.Drawing.Color.FromArgb(76, 0, 57, 255),    //----- 226
      System.Drawing.Color.FromArgb(76, 38, 66, 255),    //----- 227
      System.Drawing.Color.FromArgb(38, 0, 28, 255),    //----- 228
      System.Drawing.Color.FromArgb(38, 19, 33, 255),    //----- 229
      System.Drawing.Color.FromArgb(255, 0, 127, 255),    //----- 230
      System.Drawing.Color.FromArgb(255, 127, 191, 255),//----- 231
      System.Drawing.Color.FromArgb(165, 0, 82, 255),    //----- 232
      System.Drawing.Color.FromArgb(165, 82, 124, 255),    //----- 233
      System.Drawing.Color.FromArgb(127, 0, 63, 255),    //----- 234
      System.Drawing.Color.FromArgb(127, 63, 95, 255),    //----- 235
      System.Drawing.Color.FromArgb(76, 0, 38, 255),    //----- 236
      System.Drawing.Color.FromArgb(76, 38, 57, 255),    //----- 237
      System.Drawing.Color.FromArgb(38, 0, 19, 255),    //----- 238
      System.Drawing.Color.FromArgb(38, 19, 28, 255),    //----- 239
      System.Drawing.Color.FromArgb(255, 0, 63, 255),    //----- 240
      System.Drawing.Color.FromArgb(255, 127, 159, 255),//----- 241
      System.Drawing.Color.FromArgb(165, 0, 41, 255),    //----- 242
      System.Drawing.Color.FromArgb(165, 82, 103, 255),    //----- 243
      System.Drawing.Color.FromArgb(127, 0, 31, 255),    //----- 244
      System.Drawing.Color.FromArgb(127, 63, 79, 255),    //----- 245
      System.Drawing.Color.FromArgb(76, 0, 19, 255),    //----- 246
      System.Drawing.Color.FromArgb(76, 38, 47, 255),    //----- 247
      System.Drawing.Color.FromArgb(38, 0, 9, 255),        //----- 248
      System.Drawing.Color.FromArgb(38, 19, 23, 255),    //----- 249
      System.Drawing.Color.FromArgb(84, 84, 84, 255),    //----- 250
      System.Drawing.Color.FromArgb(118, 118, 118, 255),//----- 251
      System.Drawing.Color.FromArgb(152, 152, 152, 255),//----- 252
      System.Drawing.Color.FromArgb(186, 186, 186, 255),//----- 253
      System.Drawing.Color.FromArgb(220, 220, 220, 255),//----- 254
      System.Drawing.Color.FromArgb(255, 255, 255, 255),//----- 255
    };

		//////////////////////////////////////////////////////////////////////////////
		// standard autocad colours
		public static System.Drawing.Color[] MyAcadColorMs = 
    {
      System.Drawing.Color.FromArgb(255, 255, 255, 255),//----- 0 - ByBlock - White
      System.Drawing.Color.FromArgb(255, 0, 0, 255),    //----- 1 - Red 
      System.Drawing.Color.FromArgb(255, 255, 0, 255),    //----- 2 - Yellow
      System.Drawing.Color.FromArgb(0, 255, 0, 255),    //----- 3 - Green
      System.Drawing.Color.FromArgb(0, 255, 255, 255),    //----- 4 - Cyan
      System.Drawing.Color.FromArgb(0, 0, 255, 255),    //----- 5 - Blue
      System.Drawing.Color.FromArgb(255, 0, 255, 255),    //----- 6 - Magenta
      System.Drawing.Color.FromArgb(255, 255, 255, 255),//----- 7 - White
      System.Drawing.Color.FromArgb(255, 255, 255, 255),//----- 8
      System.Drawing.Color.FromArgb(255, 255, 255, 255),//----- 9
      System.Drawing.Color.FromArgb(255, 0, 0, 255),    //----- 10
      System.Drawing.Color.FromArgb(255, 127, 127, 255),//----- 11
      System.Drawing.Color.FromArgb(165, 0, 0, 255),    //----- 12
      System.Drawing.Color.FromArgb(165, 82, 82, 255),    //----- 13
      System.Drawing.Color.FromArgb(127, 0, 0, 255),    //----- 14
      System.Drawing.Color.FromArgb(127, 63, 63, 255),    //----- 15
      System.Drawing.Color.FromArgb(76, 0, 0, 255),        //----- 16
      System.Drawing.Color.FromArgb(76, 38, 38, 255),    //----- 17
      System.Drawing.Color.FromArgb(38, 0, 0, 255),        //----- 18
      System.Drawing.Color.FromArgb(38, 19, 19, 255),    //----- 19
      System.Drawing.Color.FromArgb(255, 63, 0, 255),    //----- 20
      System.Drawing.Color.FromArgb(255, 159, 127, 255),//----- 21
      System.Drawing.Color.FromArgb(165, 41, 0, 255),    //----- 22
      System.Drawing.Color.FromArgb(165, 103, 82, 255),    //----- 23
      System.Drawing.Color.FromArgb(127, 31, 0, 255),    //----- 24
      System.Drawing.Color.FromArgb(127, 79, 63, 255),    //----- 25
      System.Drawing.Color.FromArgb(76, 19, 0, 255),    //----- 26
      System.Drawing.Color.FromArgb(76, 47, 38, 255),    //----- 27
      System.Drawing.Color.FromArgb(38, 9, 0, 255),        //----- 28
      System.Drawing.Color.FromArgb(38, 23, 19, 255),    //----- 29
      System.Drawing.Color.FromArgb(255, 127, 0, 255),    //----- 30
      System.Drawing.Color.FromArgb(255, 191, 127, 255),//----- 31
      System.Drawing.Color.FromArgb(165, 82, 0, 255),    //----- 32
      System.Drawing.Color.FromArgb(165, 124, 82, 255),    //----- 33
      System.Drawing.Color.FromArgb(127, 63, 0, 255),    //----- 34
      System.Drawing.Color.FromArgb(127, 95, 63, 255),    //----- 35
      System.Drawing.Color.FromArgb(76, 38, 0, 255),    //----- 36
      System.Drawing.Color.FromArgb(76, 57, 38, 255),    //----- 37
      System.Drawing.Color.FromArgb(38, 19, 0, 255),    //----- 38
      System.Drawing.Color.FromArgb(38, 28, 19, 255),    //----- 39
      System.Drawing.Color.FromArgb(255, 191, 0, 255),    //----- 40
      System.Drawing.Color.FromArgb(255, 223, 127, 255),//----- 41
      System.Drawing.Color.FromArgb(165, 124, 0, 255),    //----- 42
      System.Drawing.Color.FromArgb(165, 145, 82, 255),    //----- 43
      System.Drawing.Color.FromArgb(127, 95, 0, 255),    //----- 44
      System.Drawing.Color.FromArgb(127, 111, 63, 255),    //----- 45
      System.Drawing.Color.FromArgb(76, 57, 0, 255),    //----- 46
      System.Drawing.Color.FromArgb(76, 66, 38, 255),    //----- 47
      System.Drawing.Color.FromArgb(38, 28, 0, 255),    //----- 48
      System.Drawing.Color.FromArgb(38, 33, 19, 255),    //----- 49
      System.Drawing.Color.FromArgb(255, 255, 0, 255),    //----- 50
      System.Drawing.Color.FromArgb(255, 255, 127, 255),//----- 51
      System.Drawing.Color.FromArgb(165, 165, 0, 255),    //----- 52
      System.Drawing.Color.FromArgb(165, 165, 82, 255),    //----- 53
      System.Drawing.Color.FromArgb(127, 127, 0, 255),    //----- 54
      System.Drawing.Color.FromArgb(127, 127, 63, 255),    //----- 55
      System.Drawing.Color.FromArgb(76, 76, 0, 255),    //----- 56
      System.Drawing.Color.FromArgb(76, 76, 38, 255),    //----- 57
      System.Drawing.Color.FromArgb(38, 38, 0, 255),    //----- 58
      System.Drawing.Color.FromArgb(38, 38, 19, 255),    //----- 59
      System.Drawing.Color.FromArgb(191, 255, 0, 255),    //----- 60
      System.Drawing.Color.FromArgb(223, 255, 127, 255),//----- 61
      System.Drawing.Color.FromArgb(124, 165, 0, 255),    //----- 62
      System.Drawing.Color.FromArgb(145, 165, 82, 255),    //----- 63
      System.Drawing.Color.FromArgb(95, 127, 0, 255),    //----- 64
      System.Drawing.Color.FromArgb(111, 127, 63, 255),    //----- 65
      System.Drawing.Color.FromArgb(57, 76, 0, 255),    //----- 66
      System.Drawing.Color.FromArgb(66, 76, 38, 255),    //----- 67
      System.Drawing.Color.FromArgb(28, 38, 0, 255),    //----- 68
      System.Drawing.Color.FromArgb(33, 38, 19, 255),    //----- 69
      System.Drawing.Color.FromArgb(127, 255, 0, 255),    //----- 70
      System.Drawing.Color.FromArgb(191, 255, 127, 255),//----- 71
      System.Drawing.Color.FromArgb(82, 165, 0, 255),    //----- 72
      System.Drawing.Color.FromArgb(124, 165, 82, 255),    //----- 73
      System.Drawing.Color.FromArgb(63, 127, 0, 255),    //----- 74
      System.Drawing.Color.FromArgb(95, 127, 63, 255),    //----- 75
      System.Drawing.Color.FromArgb(38, 76, 0, 255),    //----- 76
      System.Drawing.Color.FromArgb(57, 76, 38, 255),    //----- 77
      System.Drawing.Color.FromArgb(19, 38, 0, 255),    //----- 78
      System.Drawing.Color.FromArgb(28, 38, 19, 255),    //----- 79
      System.Drawing.Color.FromArgb(63, 255, 0, 255),    //----- 80
      System.Drawing.Color.FromArgb(159, 255, 127, 255),//----- 81
      System.Drawing.Color.FromArgb(41, 165, 0, 255),    //----- 82
      System.Drawing.Color.FromArgb(103, 165, 82, 255),    //----- 83
      System.Drawing.Color.FromArgb(31, 127, 0, 255),    //----- 84
      System.Drawing.Color.FromArgb(79, 127, 63, 255),    //----- 85
      System.Drawing.Color.FromArgb(19, 76, 0, 255),    //----- 86
      System.Drawing.Color.FromArgb(47, 76, 38, 255),    //----- 87
      System.Drawing.Color.FromArgb(9, 38, 0, 255),        //----- 88
      System.Drawing.Color.FromArgb(23, 38, 19, 255),    //----- 89
      System.Drawing.Color.FromArgb(0, 255, 0, 255),    //----- 90
      System.Drawing.Color.FromArgb(127, 255, 127, 255),//----- 91
      System.Drawing.Color.FromArgb(0, 165, 0, 255),    //----- 92
      System.Drawing.Color.FromArgb(82, 165, 82, 255),    //----- 93
      System.Drawing.Color.FromArgb(0, 127, 0, 255),    //----- 94
      System.Drawing.Color.FromArgb(63, 127, 63, 255),    //----- 95
      System.Drawing.Color.FromArgb(0, 76, 0, 255),        //----- 96
      System.Drawing.Color.FromArgb(38, 76, 38, 255),    //----- 97
      System.Drawing.Color.FromArgb(0, 38, 0, 255),        //----- 98
      System.Drawing.Color.FromArgb(19, 38, 19, 255),    //----- 99
      System.Drawing.Color.FromArgb(0, 255, 63, 255),    //----- 100
      System.Drawing.Color.FromArgb(127, 255, 159, 255),//----- 101
      System.Drawing.Color.FromArgb(0, 165, 41, 255),    //----- 102
      System.Drawing.Color.FromArgb(82, 165, 103, 255),    //----- 103
      System.Drawing.Color.FromArgb(0, 127, 31, 255),    //----- 104
      System.Drawing.Color.FromArgb(63, 127, 79, 255),    //----- 105
      System.Drawing.Color.FromArgb(0, 76, 19, 255),    //----- 106
      System.Drawing.Color.FromArgb(38, 76, 47, 255),    //----- 107
      System.Drawing.Color.FromArgb(0, 38, 9, 255),        //----- 108
      System.Drawing.Color.FromArgb(19, 38, 23, 255),    //----- 109
      System.Drawing.Color.FromArgb(0, 255, 127, 255),    //----- 110
      System.Drawing.Color.FromArgb(127, 255, 191, 255),//----- 111
      System.Drawing.Color.FromArgb(0, 165, 82, 255),    //----- 112
      System.Drawing.Color.FromArgb(82, 165, 124, 255),    //----- 113
      System.Drawing.Color.FromArgb(0, 127, 63, 255),    //----- 114
      System.Drawing.Color.FromArgb(63, 127, 95, 255),    //----- 115
      System.Drawing.Color.FromArgb(0, 76, 38, 255),    //----- 116
      System.Drawing.Color.FromArgb(38, 76, 57, 255),    //----- 117
      System.Drawing.Color.FromArgb(0, 38, 19, 255),    //----- 118
      System.Drawing.Color.FromArgb(19, 38, 28, 255),    //----- 119
      System.Drawing.Color.FromArgb(0, 255, 191, 255),    //----- 120
      System.Drawing.Color.FromArgb(127, 255, 223, 255),//----- 121
      System.Drawing.Color.FromArgb(0, 165, 124, 255),    //----- 122
      System.Drawing.Color.FromArgb(82, 165, 145, 255),    //----- 123
      System.Drawing.Color.FromArgb(0, 127, 95, 255),    //----- 124
      System.Drawing.Color.FromArgb(63, 127, 111, 255),    //----- 125
      System.Drawing.Color.FromArgb(0, 76, 57, 255),    //----- 126
      System.Drawing.Color.FromArgb(38, 76, 66, 255),    //----- 127
      System.Drawing.Color.FromArgb(0, 38, 28, 255),    //----- 128
      System.Drawing.Color.FromArgb(19, 38, 33, 255),    //----- 129
      System.Drawing.Color.FromArgb(0, 255, 255, 255),    //----- 130
      System.Drawing.Color.FromArgb(127, 255, 255, 255),//----- 131
      System.Drawing.Color.FromArgb(0, 165, 165, 255),    //----- 132
      System.Drawing.Color.FromArgb(82, 165, 165, 255),    //----- 133
      System.Drawing.Color.FromArgb(0, 127, 127, 255),    //----- 134
      System.Drawing.Color.FromArgb(63, 127, 127, 255),    //----- 135
      System.Drawing.Color.FromArgb(0, 76, 76, 255),    //----- 136
      System.Drawing.Color.FromArgb(38, 76, 76, 255),    //----- 137
      System.Drawing.Color.FromArgb(0, 38, 38, 255),    //----- 138
      System.Drawing.Color.FromArgb(19, 38, 38, 255),    //----- 139
      System.Drawing.Color.FromArgb(0, 191, 255, 255),    //----- 140
      System.Drawing.Color.FromArgb(127, 223, 255, 255),//----- 141
      System.Drawing.Color.FromArgb(0, 124, 165, 255),    //----- 142
      System.Drawing.Color.FromArgb(82, 145, 165, 255),    //----- 143
      System.Drawing.Color.FromArgb(0, 95, 127, 255),    //----- 144
      System.Drawing.Color.FromArgb(63, 111, 127, 255),    //----- 145
      System.Drawing.Color.FromArgb(0, 57, 76, 255),    //----- 146
      System.Drawing.Color.FromArgb(38, 66, 76, 255),    //----- 147
      System.Drawing.Color.FromArgb(0, 28, 38, 255),    //----- 148
      System.Drawing.Color.FromArgb(19, 33, 38, 255),    //----- 149
      System.Drawing.Color.FromArgb(0, 127, 255, 255),    //----- 150
      System.Drawing.Color.FromArgb(127, 191, 255, 255),//----- 151
      System.Drawing.Color.FromArgb(0, 82, 165, 255),    //----- 152
      System.Drawing.Color.FromArgb(82, 124, 165, 255),    //----- 153
      System.Drawing.Color.FromArgb(0, 63, 127, 255),    //----- 154
      System.Drawing.Color.FromArgb(63, 95, 127, 255),    //----- 155
      System.Drawing.Color.FromArgb(0, 38, 76, 255),    //----- 156
      System.Drawing.Color.FromArgb(38, 57, 76, 255),    //----- 157
      System.Drawing.Color.FromArgb(0, 19, 38, 255),    //----- 158
      System.Drawing.Color.FromArgb(19, 28, 38, 255),    //----- 159
      System.Drawing.Color.FromArgb(0, 63, 255, 255),    //----- 160
      System.Drawing.Color.FromArgb(127, 159, 255, 255),//----- 161
      System.Drawing.Color.FromArgb(0, 41, 165, 255),    //----- 162
      System.Drawing.Color.FromArgb(82, 103, 165, 255),    //----- 163
      System.Drawing.Color.FromArgb(0, 31, 127, 255),    //----- 164
      System.Drawing.Color.FromArgb(63, 79, 127, 255),    //----- 165
      System.Drawing.Color.FromArgb(0, 19, 76, 255),    //----- 166
      System.Drawing.Color.FromArgb(38, 47, 76, 255),    //----- 167
      System.Drawing.Color.FromArgb(0, 9, 38, 255),        //----- 168
      System.Drawing.Color.FromArgb(19, 23, 38, 255),    //----- 169
      System.Drawing.Color.FromArgb(0, 0, 255, 255),    //----- 170
      System.Drawing.Color.FromArgb(127, 127, 255, 255),//----- 171
      System.Drawing.Color.FromArgb(0, 0, 165, 255),    //----- 172
      System.Drawing.Color.FromArgb(82, 82, 165, 255),    //----- 173
      System.Drawing.Color.FromArgb(0, 0, 127, 255),    //----- 174
      System.Drawing.Color.FromArgb(63, 63, 127, 255),    //----- 175
      System.Drawing.Color.FromArgb(0, 0, 76, 255),        //----- 176
      System.Drawing.Color.FromArgb(38, 38, 76, 255),    //----- 177
      System.Drawing.Color.FromArgb(0, 0, 38, 255),        //----- 178
      System.Drawing.Color.FromArgb(19, 19, 38, 255),    //----- 179
      System.Drawing.Color.FromArgb(63, 0, 255, 255),    //----- 180
      System.Drawing.Color.FromArgb(159, 127, 255, 255),//----- 181
      System.Drawing.Color.FromArgb(41, 0, 165, 255),    //----- 182
      System.Drawing.Color.FromArgb(103, 82, 165, 255),    //----- 183
      System.Drawing.Color.FromArgb(31, 0, 127, 255),    //----- 184
      System.Drawing.Color.FromArgb(79, 63, 127, 255),    //----- 185
      System.Drawing.Color.FromArgb(19, 0, 76, 255),    //----- 186
      System.Drawing.Color.FromArgb(47, 38, 76, 255),    //----- 187
      System.Drawing.Color.FromArgb(9, 0, 38, 255),        //----- 188
      System.Drawing.Color.FromArgb(23, 19, 38, 255),    //----- 189
      System.Drawing.Color.FromArgb(127, 0, 255, 255),    //----- 190
      System.Drawing.Color.FromArgb(191, 127, 255, 255),//----- 191
      System.Drawing.Color.FromArgb(82, 0, 165, 255),    //----- 192
      System.Drawing.Color.FromArgb(124, 82, 165, 255),    //----- 193
      System.Drawing.Color.FromArgb(63, 0, 127, 255),    //----- 194
      System.Drawing.Color.FromArgb(95, 63, 127, 255),    //----- 195
      System.Drawing.Color.FromArgb(38, 0, 76, 255),    //----- 196
      System.Drawing.Color.FromArgb(57, 38, 76, 255),    //----- 197
      System.Drawing.Color.FromArgb(19, 0, 38, 255),    //----- 198
      System.Drawing.Color.FromArgb(28, 19, 38, 255),    //----- 199
      System.Drawing.Color.FromArgb(191, 0, 255, 255),    //----- 200
      System.Drawing.Color.FromArgb(223, 127, 255, 255),//----- 201
      System.Drawing.Color.FromArgb(124, 0, 165, 255),    //----- 202
      System.Drawing.Color.FromArgb(145, 82, 165, 255),    //----- 203
      System.Drawing.Color.FromArgb(95, 0, 127, 255),    //----- 204
      System.Drawing.Color.FromArgb(111, 63, 127, 255),    //----- 205
      System.Drawing.Color.FromArgb(57, 0, 76, 255),    //----- 206
      System.Drawing.Color.FromArgb(66, 38, 76, 255),    //----- 207
      System.Drawing.Color.FromArgb(28, 0, 38, 255),    //----- 208
      System.Drawing.Color.FromArgb(33, 19, 38, 255),    //----- 209
      System.Drawing.Color.FromArgb(255, 0, 255, 255),    //----- 210
      System.Drawing.Color.FromArgb(255, 127, 255, 255),//----- 211
      System.Drawing.Color.FromArgb(165, 0, 165, 255),    //----- 212
      System.Drawing.Color.FromArgb(165, 82, 165, 255),    //----- 213
      System.Drawing.Color.FromArgb(127, 0, 127, 255),    //----- 214
      System.Drawing.Color.FromArgb(127, 63, 127, 255),    //----- 215
      System.Drawing.Color.FromArgb(76, 0, 76, 255),    //----- 216
      System.Drawing.Color.FromArgb(76, 38, 76, 255),    //----- 217
      System.Drawing.Color.FromArgb(38, 0, 38, 255),    //----- 218
      System.Drawing.Color.FromArgb(38, 19, 38, 255),    //----- 219
      System.Drawing.Color.FromArgb(255, 0, 191, 255),    //----- 220
      System.Drawing.Color.FromArgb(255, 127, 223, 255),//----- 221
      System.Drawing.Color.FromArgb(165, 0, 124, 255),    //----- 222
      System.Drawing.Color.FromArgb(165, 82, 145, 255),    //----- 223
      System.Drawing.Color.FromArgb(127, 0, 95, 255),    //----- 224
      System.Drawing.Color.FromArgb(127, 63, 111, 255),    //----- 225
      System.Drawing.Color.FromArgb(76, 0, 57, 255),    //----- 226
      System.Drawing.Color.FromArgb(76, 38, 66, 255),    //----- 227
      System.Drawing.Color.FromArgb(38, 0, 28, 255),    //----- 228
      System.Drawing.Color.FromArgb(38, 19, 33, 255),    //----- 229
      System.Drawing.Color.FromArgb(255, 0, 127, 255),    //----- 230
      System.Drawing.Color.FromArgb(255, 127, 191, 255),//----- 231
      System.Drawing.Color.FromArgb(165, 0, 82, 255),    //----- 232
      System.Drawing.Color.FromArgb(165, 82, 124, 255),    //----- 233
      System.Drawing.Color.FromArgb(127, 0, 63, 255),    //----- 234
      System.Drawing.Color.FromArgb(127, 63, 95, 255),    //----- 235
      System.Drawing.Color.FromArgb(76, 0, 38, 255),    //----- 236
      System.Drawing.Color.FromArgb(76, 38, 57, 255),    //----- 237
      System.Drawing.Color.FromArgb(38, 0, 19, 255),    //----- 238
      System.Drawing.Color.FromArgb(38, 19, 28, 255),    //----- 239
      System.Drawing.Color.FromArgb(255, 0, 63, 255),    //----- 240
      System.Drawing.Color.FromArgb(255, 127, 159, 255),//----- 241
      System.Drawing.Color.FromArgb(165, 0, 41, 255),    //----- 242
      System.Drawing.Color.FromArgb(165, 82, 103, 255),    //----- 243
      System.Drawing.Color.FromArgb(127, 0, 31, 255),    //----- 244
      System.Drawing.Color.FromArgb(127, 63, 79, 255),    //----- 245
      System.Drawing.Color.FromArgb(76, 0, 19, 255),    //----- 246
      System.Drawing.Color.FromArgb(76, 38, 47, 255),    //----- 247
      System.Drawing.Color.FromArgb(38, 0, 9, 255),        //----- 248
      System.Drawing.Color.FromArgb(38, 19, 23, 255),    //----- 249
      System.Drawing.Color.FromArgb(84, 84, 84, 255),    //----- 250
      System.Drawing.Color.FromArgb(118, 118, 118, 255),//----- 251
      System.Drawing.Color.FromArgb(152, 152, 152, 255),//----- 252
      System.Drawing.Color.FromArgb(186, 186, 186, 255),//----- 253
      System.Drawing.Color.FromArgb(220, 220, 220, 255),//----- 254
      System.Drawing.Color.FromArgb(255, 255, 255, 255),//----- 255
    };

		public static bool GetActiveViewPortInfo( ref double height, ref double width, ref Point3d target, ref Vector3d viewDir, ref double viewTwist, bool getViewCenter )
		{
			// get the editor object
			Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
			ed.UpdateTiledViewportsInDatabase();
			Database db = HostApplicationServices.WorkingDatabase;
			using( Transaction t = db.TransactionManager.StartTransaction() )
			{
				ViewportTable vt = (ViewportTable) t.GetObject( db.ViewportTableId, OpenMode.ForRead );
				ViewportTableRecord btr = (ViewportTableRecord) t.GetObject( vt[GSUtil.strActive], OpenMode.ForRead );
				height = btr.Height;
				width = btr.Width;
				target = btr.Target;
				viewDir = btr.ViewDirection;
				viewTwist = btr.ViewTwist;
				t.Commit();
			}
			return true;
		}
		public static void mpManager_ViewToBeDestroyed( Object sender, ViewEventArgs e )
		{
			// get the editor object
			Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
			ed.WriteMessage( "BlockView: Event ViewToBeDestroyed fired" );
		}
		public static void mpManager_ViewWasCreated( Object sender, ViewEventArgs e )
		{
			// get the editor object
			Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
			ed.WriteMessage( "BlockView: Event ViewWasCreated fired" );
		}
		public static void mpManager_GsToBeUnloaded( Object sender, EventArgs e )
		{
			// get the editor object
			Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
			ed.WriteMessage( "BlockView: Event GsToBeUnloaded fired" );
		}
		public static void mpManager_ConfigWasModified( Object sender, EventArgs e )
		{
			// get the editor object
			Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
			ed.WriteMessage( "BlockView: Event ConfigWasModified fired" );
		}

		/// <summary>
		/// [TT]: Depreciated (using ControlPaint.DrawReversibleFrame() instead)
		/// </summary>
		
		public class RubberbandRectangle
		{
			public enum PenStyles
			{
				PS_SOLID = 0,
				PS_DASH = 1,
				PS_DOT = 2,
				PS_DASHDOT = 3,
				PS_DASHDOTDOT = 4
			}
			// These values come from the larger set of defines in WinGDI.h,
			// but are all that are needed for this application.  If this class
			// is expanded for more generic rectangle drawing, they should be
			// replaced by enums from those sets of defones.
			private int NULL_BRUSH = 5;
			private int R2_XORPEN = 7;
			private PenStyles penStyle;
			private int BLACK_PEN = 0;

			// Default contructor - sets member fields
			public RubberbandRectangle()
			{
				penStyle = PenStyles.PS_DOT;
			}

			// penStyles property get/set.
			public PenStyles PenStyle
			{
				get
				{
					return penStyle;
				}
				set
				{
					penStyle = value;
				}
			}

			public void DrawXORRectangle( Graphics grp, System.Drawing.Point startPt, System.Drawing.Point endPt )
			{
				int X1 = startPt.X;
				int Y1 = startPt.Y;
				int X2 = endPt.X;
				int Y2 = endPt.Y;
				// Extract the Win32 HDC from the Graphics object supplied.
				IntPtr hdc = grp.GetHdc();

				// Create a pen with a dotted style to draw the border of the
				// rectangle.
				IntPtr gdiPen = CreatePen( penStyle,
								  1, BLACK_PEN );

				// Set the ROP cdrawint mode to XOR.
				SetROP2( hdc, R2_XORPEN );

				// Select the pen into the device context.
				IntPtr oldPen = SelectObject( hdc, gdiPen );

				// Create a stock NULL_BRUSH brush and select it into the device
				// context so that the rectangle isn't filled.
				IntPtr oldBrush = SelectObject( hdc,
											GetStockObject( NULL_BRUSH ) );

				// Now XOR the hollow rectangle on the Graphics object with
				// a dotted outline.
				Rectangle( hdc, X1, Y1, X2, Y2 );

				// Put the old stuff back where it was.
				SelectObject( hdc, oldBrush ); // no need to delete a stock object
				SelectObject( hdc, oldPen );
				DeleteObject( gdiPen );		// but we do need to delete the pen

				// Return the device context to Windows.
				grp.ReleaseHdc( hdc );
			}

			// Use Interop to call the corresponding Win32 GDI functions
			[System.Runtime.InteropServices.DllImportAttribute( "gdi32.dll" )]
			private static extern int SetROP2(
					  IntPtr hdc,		// Handle to a Win32 device context
					  int enDrawMode	// Drawing mode
					  );
			[System.Runtime.InteropServices.DllImportAttribute( "gdi32.dll" )]
			private static extern IntPtr CreatePen(
					  PenStyles enPenStyle,	// Pen style from enum PenStyles
					  int nWidth,				// Width of pen
					  int crColor				// Color of pen
					  );
			[System.Runtime.InteropServices.DllImportAttribute( "gdi32.dll" )]
			private static extern bool DeleteObject(
					  IntPtr hObject	// Win32 GDI handle to object to delete
					  );
			[System.Runtime.InteropServices.DllImportAttribute( "gdi32.dll" )]
			private static extern IntPtr SelectObject(
					  IntPtr hdc,		// Win32 GDI device context
					  IntPtr hObject	// Win32 GDI handle to object to select
					  );
			[System.Runtime.InteropServices.DllImportAttribute( "gdi32.dll" )]
			private static extern void Rectangle(
					  IntPtr hdc,			// Handle to a Win32 device context
					  int X1,				// x-coordinate of top left corner
					  int Y1,				// y-cordinate of top left corner
					  int X2,				// x-coordinate of bottom right corner
					  int Y2				// y-coordinate of bottm right corner
					  );
			[System.Runtime.InteropServices.DllImportAttribute( "gdi32.dll" )]
			private static extern IntPtr GetStockObject(
					  int brStyle	// Selected from the WinGDI.h BrushStyles enum
					  );

			// C# version of Win32 RGB macro
			private static int RGB( int R, int G, int B )
			{
				return ( R | ( G << 8 ) | ( B << 16 ) );
			}
		}
	}


}
