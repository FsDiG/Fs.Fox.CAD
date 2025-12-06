using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

// autocad stuff
using Autodesk.AutoCAD.ApplicationServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsSystem;

//using GsRenderMode = Autodesk.AutoCAD.GraphicsSystem.RenderMode;

//////////////////////////////////////////////////////////////////////////
// by Fenton Webb, DevTech, Autodesk. 23/05/2008

/// Revised 10/2012 Tony Tanzillo:
/// 
/// Revisions to correct issues that prevented the included custom
/// control and this form from being opened in the designer, and
/// and other miscellaneous refactoring to eliminate significant 
/// amounts of code to manage menu items.

namespace BlockView.NET
{
	public partial class BlockViewDialog : System.Windows.Forms.Form
	{
		public BlockViewDialog()
		{
			InitializeComponent();
			if( !this.IsDesignMode() )
				RuntimeConstruct();
		}

		void RuntimeConstruct()
		{
			InitializeRenderModeMenuItems();
			InitializeViewStyleMenuItems();

			Document document = AcadApp.DocumentManager.MdiActiveDocument;
			if( document != null )
			{
				InitDrawingControl( document, document.Database );
			}
			base.Disposed += new EventHandler( OnDisposed );
		}

		void OnDisposed( object sender, EventArgs e )
		{
			if( ! this.IsDesignMode() )
			{
				// clean up
				if( mPreviewCtrl != null )
				{
					mPreviewCtrl.ClearAll();
					mPreviewCtrl.Dispose();
					mPreviewCtrl = null;
				}

				if( spheres != null )
				{
					spheres.DisposeItems();
					spheres = null;
				}
				if( dwgs != null )
				{
					dwgs.DisposeItems();
					dwgs = null;
				}
			}
		}

		// list of opened drawing files to be disposed of
		private List<Database> dwgs = new List<Database>();

		#region FileMenu
		private void FileOpenMenuItem_Click( object sender, EventArgs e )
		{
			// pick the place where o output the image type
			Autodesk.AutoCAD.Windows.OpenFileDialog dialog = new Autodesk.AutoCAD.Windows.OpenFileDialog( "Open DWG File", null, "dwg", "OpenDWGFileDialog", Autodesk.AutoCAD.Windows.OpenFileDialog.OpenFileDialogFlags.NoUrls );
			// if all is ok?
			if( DialogResult.OK == dialog.ShowDialog() )
			{
				// clear the preview control
				mPreviewCtrl.mpView.EraseAll();
				// create a new database
				dwgs.Add( new Database( false, true ) );
				// now read it in
				dwgs[dwgs.Count - 1].ReadDwgFile( dialog.Filename, FileOpenMode.OpenForReadAndReadShare, true, "" );
				// initialising the drawing control, pass the existing document still as the gs view still refers to it
				InitDrawingControl( AcadApp.DocumentManager.MdiActiveDocument, dwgs[dwgs.Count - 1] );
			}
		}

		private void OutputImageMenuItem_Click( object sender, EventArgs e )
		{
			// pick the place where o output the image type
			Autodesk.AutoCAD.Windows.SaveFileDialog dialog = new Autodesk.AutoCAD.Windows.SaveFileDialog( "RenderToImage", null, "jpg;png;tif;bmp", "BlockViewSnapshotBrowseDialog", Autodesk.AutoCAD.Windows.SaveFileDialog.SaveFileDialogFlags.AllowAnyExtension );
			// if all is ok?
			if( DialogResult.OK == dialog.ShowDialog() )
			{
				// create an offscreen device
				using( Device device = new Device() )
				{
					// get the size of the GS view
					Size size = mPreviewCtrl.ClientRectangle.Size;
					// resize the device to this
					device.OnSize( size );
					device.BackgroundColor = Color.Black;
					device.SetLogicalPalette( GSUtil.MyAcadColorMs );
					device.OnRealizeBackgroundPalette();
					device.OnRealizeForegroundPalette();

					// make a copy of the gs view
					using( Autodesk.AutoCAD.GraphicsSystem.View view = mPreviewCtrl.mpView.Clone( true, true ) )
					{
						try
						{
							// add it to the device
							device.Add( view );
							// now render the image to a bitmap
                            
							//using( System.Drawing.Bitmap bitmap = view.RenderToImage() )
							//{
								// now save it out!!
								//bitmap.Save( dialog.Filename );
							//}
                            using (Bitmap bitmap = view.GetSnapshot(mPreviewCtrl.ClientRectangle))
                            {
                                bitmap.Save(dialog.Filename);
                            }
						}
						finally
						{
							// do a clear up
							device.EraseAll();
						}
					}
				}
			}
		}

		

		private void ExitMenuItem_Click( object sender, EventArgs e )
		{
			this.Close();
		}
		#endregion

		#region ViewMenu

		#region Zooming
		private void ZoomWindowMenuItem_Click( object sender, EventArgs e )
		{
			// set that we now want zoom window mode
			mPreviewCtrl.mZooming = true;
		}

		private void ZoomExtentsMenuItem_Click( object sender, EventArgs e )
		{
			// check to make sure it's valid
			if( mPreviewCtrl.mpView != null )
			{
				mPreviewCtrl.mpView.ZoomExtents( mPreviewCtrl.mCurrentDwg.Extmin, mPreviewCtrl.mCurrentDwg.Extmax );
				refreshView();
			}
		}

		private void ZoomInMenuItem_Click( object sender, EventArgs e )
		{
			// check to make sure it's valid
			if( mPreviewCtrl.mpView != null )
			{
				// zoom in a bit
				mPreviewCtrl.mpView.Zoom( 1.5 );
				// refresh the view
				refreshView();
			}

		}

		private void ZoomOutMenuItem_Click( object sender, EventArgs e )
		{
			// check to make sure it's valid
			if( mPreviewCtrl.mpView != null )
			{
				// zoom out a bit
				mPreviewCtrl.mpView.Zoom( 0.5 );
				// refresh the view
				refreshView();
			}
		}

		#endregion Zooming

		#region RandomShowSettings
		private void showLinetypesToolStripMenuItem_Click( object sender, EventArgs e )
		{
			// find out what the setting was before
			bool isChecked = this.showLinetypesToolStripMenuItem.Checked;
			// toggle it
			isChecked = !isChecked;
			this.showLinetypesToolStripMenuItem.Checked = isChecked;

			// now set the apt values in the gsview
			mPreviewCtrl.mpModel.LinetypesEnabled = isChecked;
			// refresh
			refreshView();
		}

		private void showSectioningToolStripMenuItem_Click( object sender, EventArgs e )
		{
			// find out what the setting was before
			bool isChecked = this.showSectioningToolStripMenuItem.Checked;
			// toggle it
			isChecked = !isChecked;
			this.showSectioningToolStripMenuItem.Checked = isChecked;

			// now set the apt values in the gsview
			mPreviewCtrl.mpModel.EnableSectioning = isChecked;
			// refresh
			refreshView();
		}
        #endregion

        #region RenderMode



        void InitializeRenderModeMenuItems()
		{
			
		}

	

		#endregion RenderMode

		#region VisualStyle

		void InitializeViewStyleMenuItems()
		{
			viewStyleToolStripMenuItem.DropDown.Items.AddRange(
				EnumToolStripMenuItem.CreateItems<VisualStyleType>( ViewStyleMenuItemClick ) );
		}

		// pass a view style you want, the gs view will update to it
		public void ChangeViewStyleTo( VisualStyleType vs )
		{
			VisualStyle oldVs = mPreviewCtrl.mpView.VisualStyle;
			mPreviewCtrl.mpView.VisualStyle = new VisualStyle( vs );
			if( oldVs != null )
				oldVs.Dispose();
			EnumToolStripMenuItem.UpdateCheckedState( viewStyleToolStripMenuItem.DropDown.Items, vs );
			refreshView();
		}
		
		void ViewStyleMenuItemClick( object sender, EventArgs e )
		{
			EnumToolStripMenuItem item = sender as EnumToolStripMenuItem;
			if( item != null && item.Value is VisualStyleType )
			{
				ChangeViewStyleTo( (VisualStyleType) item.Value );
			}
		}

		#endregion VisualStyle

		#endregion ViewMenu

		#region Toolsmenu
		private void RemapColoursNormalMenuItem_Click( object sender, EventArgs e )
		{
			// get the current background color
			Color color = mPreviewCtrl.mpDevice.BackgroundColor;
			// now change it to black
			color = Color.Black;
			// and update it
			mPreviewCtrl.mpDevice.BackgroundColor = color;
			// finally set the logical palette
			mPreviewCtrl.mpDevice.SetLogicalPalette( GSUtil.MyAcadColorMs );

			// now update the gs view
			refreshView();
		}

		private void RemapColoursCustomMenuItem_Click( object sender, EventArgs e )
		{
			// get the current background color
			Color color = mPreviewCtrl.mpDevice.BackgroundColor;

			// now change it to black
			color = Color.White;
			// and update it
			mPreviewCtrl.mpDevice.BackgroundColor = color;
			// finally set the logical palette
			mPreviewCtrl.mpDevice.SetLogicalPalette( GSUtil.MyAcadColorPs );

			// now update the gs view
			refreshView();
		}

		private Solid3d CreateSphere()
		{
			// create the sphere
			Solid3d sphere = new Solid3d();
			sphere.SetDatabaseDefaults();
			// let's create a random number generator for the sphere
			Random randomGenerator = new Random();
			double radius = randomGenerator.NextDouble() * 50;
			// create the sphere 
			sphere.CreateSphere( radius );
			// randomize the position using the randomizer!
			Matrix3d randomMover;
			double xVec = randomGenerator.NextDouble() * 500;
			double yVec = randomGenerator.NextDouble() * 400;
			double zVec = randomGenerator.NextDouble() * 300;
			randomMover = Matrix3d.Displacement( new Vector3d( xVec, yVec, zVec ) );
			// now apply the the transform to the sphere
			sphere.TransformBy( randomMover );

			// set the material name
			const string matname = "Sitework.Paving - Surfacing.Riverstone.Mortared";
			using( DBDictionary matdict = mPreviewCtrl.mCurrentDwg.MaterialDictionaryId.Open( OpenMode.ForRead ) as DBDictionary )
			{
				// if we have found the material, set it to the sphere
				if( matdict.Contains( matname ) )
					sphere.Material = matname;
				else
				{
					// get the editor object
					Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
					ed.WriteMessage( "\nMaterial (" + matname + ") not found" + " - sphere will be rendered without it.", matname );
				}
			}

			return sphere;
		}

		// list of temporary spheres added to the gsView, these are not handled by autocad so must me disposed ourselves
		private List<Solid3d> spheres = new List<Solid3d>();
		private void AddTempEntityMenuItem_Click( object sender, EventArgs e )
		{
			// add a random sphere
			spheres.Add( CreateSphere() );
			mPreviewCtrl.mpView.Add( spheres[spheres.Count - 1], mPreviewCtrl.mpModel );
		}

		private void AddEntityToDwgMenuItem_Click( object sender, EventArgs e )
		{
			// this time add a new object to the current drawing
			using( Solid3d sphere = CreateSphere() )
			{
				// next add it to the current space
				using( BlockTableRecord curSpace = mPreviewCtrl.mCurrentDwg.CurrentSpaceId.Open( OpenMode.ForWrite ) as BlockTableRecord )
					curSpace.AppendEntity( sphere );

				// then add to the gs view
				mPreviewCtrl.mpView.Add( sphere, mPreviewCtrl.mpModel );
			}
		}

		#endregion Toolsmenu

		#region BlockViewSpecific

		// initializes the GsPreViewCtrl
		public void InitDrawingControl( Document doc, Database db )
		{
			// initialize the control
			mPreviewCtrl.Init( doc, db );
			// now find out what the current view is and set the GsPreviewCtrl to the same
			SetViewTo( mPreviewCtrl.mpView, db );
			// now add the current space to the GsView
			using( BlockTableRecord curSpace = db.CurrentSpaceId.Open( OpenMode.ForRead, true, true ) as BlockTableRecord )
				mPreviewCtrl.mpView.Add( curSpace, mPreviewCtrl.mpModel );

			// set the view style to basic
			ChangeViewStyleTo( VisualStyleType.Basic );

			
		}

		// sets a GsView to the active viewport data held by the database
		public void SetViewTo( Autodesk.AutoCAD.GraphicsSystem.View view, Database db )
		{
			// just check we have valid extents
			if( db.Extmax.X < db.Extmin.X || db.Extmax.Y < db.Extmin.Y || db.Extmax.Z < db.Extmax.Z )
			{
				db.Extmin = new Point3d( 0, 0, 0 );
				db.Extmax = new Point3d( 400, 400, 400 );
			}
			// get the dwg extents
			Point3d extMax = db.Extmax;
			Point3d extMin = db.Extmin;
			// now the active viewport info
			double height = 0.0, width = 0.0, viewTwist = 0.0;
			Point3d targetView = new Point3d();
			Vector3d viewDir = new Vector3d();
			GSUtil.GetActiveViewPortInfo( ref height, ref width, ref targetView, ref viewDir, ref viewTwist, true );
			// from the data returned let's work out the viewmatrix
			viewDir = viewDir.GetNormal();
			Vector3d viewXDir = viewDir.GetPerpendicularVector().GetNormal();
			viewXDir = viewXDir.RotateBy( viewTwist, -viewDir );
			Vector3d viewYDir = viewDir.CrossProduct( viewXDir );
			Point3d boxCenter = extMin + 0.5 * ( extMax - extMin );
			Matrix3d viewMat;
			viewMat = Matrix3d.AlignCoordinateSystem( boxCenter, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis,
			  boxCenter, viewXDir, viewYDir, viewDir ).Inverse();
			Extents3d wcsExtents = new Extents3d( extMin, extMax );
			Extents3d viewExtents = wcsExtents;
			viewExtents.TransformBy( viewMat );
			double xMax = System.Math.Abs( viewExtents.MaxPoint.X - viewExtents.MinPoint.X );
			double yMax = System.Math.Abs( viewExtents.MaxPoint.Y - viewExtents.MinPoint.Y );
			Point3d eye = boxCenter + viewDir;
			// finally set the Gs view to the dwg view
			view.SetView( eye, boxCenter, viewYDir, xMax, yMax );

			// now update
			refreshView();
		}

		public void refreshView()
		{
			if( !IsDesignMode )
			{
				mPreviewCtrl.mpView.Invalidate();
				mPreviewCtrl.mpView.Update();
			}
		}

		#endregion BlockViewSpecific

		public static bool IsDesignMode
		{
			get
			{
				return GsPreviewCtrl.IsDesignMode;
			}
		}

		protected override void OnMouseWheel( MouseEventArgs e )
		{
			base.OnMouseWheel( e );
			if( !IsDesignMode )
			{
				// if we are wheeling down
				if( e.Delta < 0 )
					mPreviewCtrl.mpView.Zoom( 0.5 );
				else
					// wheel up!
					mPreviewCtrl.mpView.Zoom( 1.5 );

				refreshView();
			}
		}
	}



}