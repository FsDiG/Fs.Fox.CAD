using Autodesk.AutoCAD.DatabaseServices;
// by Fenton Webb, DevTech, Autodesk. 23/05/2008
namespace BlockView.NET
{
  partial class BlockViewDialog
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
		//// clean up
		//mPreviewCtrl.ClearAll();
		//// dispose all drawings that were opened
		//foreach (Database dwg in dwgs)
		//  dwg.Dispose();
		//// dispose all temporary entities
		//foreach (Solid3d sphere in spheres)
		//  sphere.Dispose();

      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileOpenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OutputImageMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ZoomWindowMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ZoomExtentsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ZoomInMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ZoomOutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.viewStyleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.showLinetypesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showSectioningToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reMapColorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RemapColoursNormalMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RemapColoursCustomMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.addAnEntityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddTempEntityMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddEntityToDwgMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createdByToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fentonWebbFentonWebbautodeskcomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.devTechToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autodeskToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.may2008ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mPreviewCtrl = new BlockView.NET.GsPreviewCtrl();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(952, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileOpenMenuItem,
            this.OutputImageMenuItem,
            this.ExitMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // FileOpenMenuItem
            // 
            this.FileOpenMenuItem.Name = "FileOpenMenuItem";
            this.FileOpenMenuItem.Size = new System.Drawing.Size(220, 24);
            this.FileOpenMenuItem.Text = "Open";
            this.FileOpenMenuItem.Click += new System.EventHandler(this.FileOpenMenuItem_Click);
            // 
            // OutputImageMenuItem
            // 
            this.OutputImageMenuItem.Name = "OutputImageMenuItem";
            this.OutputImageMenuItem.Size = new System.Drawing.Size(220, 24);
            this.OutputImageMenuItem.Text = "Output Image to Disk";
            this.OutputImageMenuItem.Click += new System.EventHandler(this.OutputImageMenuItem_Click);
            // 
            // ExitMenuItem
            // 
            this.ExitMenuItem.Name = "ExitMenuItem";
            this.ExitMenuItem.Size = new System.Drawing.Size(220, 24);
            this.ExitMenuItem.Text = "Exit";
            this.ExitMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zoomToolStripMenuItem,
            this.toolStripSeparator3,
            this.viewStyleToolStripMenuItem,
            this.toolStripSeparator7,
            this.settingsToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(53, 24);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // zoomToolStripMenuItem
            // 
            this.zoomToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ZoomWindowMenuItem,
            this.ZoomExtentsMenuItem,
            this.ZoomInMenuItem,
            this.ZoomOutMenuItem});
            this.zoomToolStripMenuItem.Name = "zoomToolStripMenuItem";
            this.zoomToolStripMenuItem.Size = new System.Drawing.Size(152, 24);
            this.zoomToolStripMenuItem.Text = "Zoom";
            // 
            // ZoomWindowMenuItem
            // 
            this.ZoomWindowMenuItem.Name = "ZoomWindowMenuItem";
            this.ZoomWindowMenuItem.Size = new System.Drawing.Size(133, 24);
            this.ZoomWindowMenuItem.Text = "Window";
            this.ZoomWindowMenuItem.Click += new System.EventHandler(this.ZoomWindowMenuItem_Click);
            // 
            // ZoomExtentsMenuItem
            // 
            this.ZoomExtentsMenuItem.Name = "ZoomExtentsMenuItem";
            this.ZoomExtentsMenuItem.Size = new System.Drawing.Size(133, 24);
            this.ZoomExtentsMenuItem.Text = "Extents";
            this.ZoomExtentsMenuItem.Click += new System.EventHandler(this.ZoomExtentsMenuItem_Click);
            // 
            // ZoomInMenuItem
            // 
            this.ZoomInMenuItem.Name = "ZoomInMenuItem";
            this.ZoomInMenuItem.Size = new System.Drawing.Size(133, 24);
            this.ZoomInMenuItem.Text = "In 0.5";
            this.ZoomInMenuItem.Click += new System.EventHandler(this.ZoomInMenuItem_Click);
            // 
            // ZoomOutMenuItem
            // 
            this.ZoomOutMenuItem.Name = "ZoomOutMenuItem";
            this.ZoomOutMenuItem.Size = new System.Drawing.Size(133, 24);
            this.ZoomOutMenuItem.Text = "Out 1.5";
            this.ZoomOutMenuItem.Click += new System.EventHandler(this.ZoomOutMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(149, 6);
            // 
            // viewStyleToolStripMenuItem
            // 
            this.viewStyleToolStripMenuItem.Name = "viewStyleToolStripMenuItem";
            this.viewStyleToolStripMenuItem.Size = new System.Drawing.Size(152, 24);
            this.viewStyleToolStripMenuItem.Text = "View Style";
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(149, 6);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showToolStripMenuItem1});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(152, 24);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // showToolStripMenuItem1
            // 
            this.showToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showLinetypesToolStripMenuItem,
            this.showSectioningToolStripMenuItem});
            this.showToolStripMenuItem1.Name = "showToolStripMenuItem1";
            this.showToolStripMenuItem1.Size = new System.Drawing.Size(114, 24);
            this.showToolStripMenuItem1.Text = "Show";
            // 
            // showLinetypesToolStripMenuItem
            // 
            this.showLinetypesToolStripMenuItem.Checked = true;
            this.showLinetypesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showLinetypesToolStripMenuItem.Name = "showLinetypesToolStripMenuItem";
            this.showLinetypesToolStripMenuItem.Size = new System.Drawing.Size(188, 24);
            this.showLinetypesToolStripMenuItem.Text = "Show Linetypes";
            this.showLinetypesToolStripMenuItem.Click += new System.EventHandler(this.showLinetypesToolStripMenuItem_Click);
            // 
            // showSectioningToolStripMenuItem
            // 
            this.showSectioningToolStripMenuItem.Checked = true;
            this.showSectioningToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showSectioningToolStripMenuItem.Name = "showSectioningToolStripMenuItem";
            this.showSectioningToolStripMenuItem.Size = new System.Drawing.Size(188, 24);
            this.showSectioningToolStripMenuItem.Text = "Show Sectioning";
            this.showSectioningToolStripMenuItem.Click += new System.EventHandler(this.showSectioningToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.reMapColorsToolStripMenuItem,
            this.toolStripSeparator2,
            this.addAnEntityToolStripMenuItem,
            this.toolStripSeparator1});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(57, 24);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // reMapColorsToolStripMenuItem
            // 
            this.reMapColorsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RemapColoursNormalMenuItem,
            this.RemapColoursCustomMenuItem});
            this.reMapColorsToolStripMenuItem.Name = "reMapColorsToolStripMenuItem";
            this.reMapColorsToolStripMenuItem.Size = new System.Drawing.Size(171, 24);
            this.reMapColorsToolStripMenuItem.Text = "ReMap Colors";
            // 
            // RemapColoursNormalMenuItem
            // 
            this.RemapColoursNormalMenuItem.Name = "RemapColoursNormalMenuItem";
            this.RemapColoursNormalMenuItem.Size = new System.Drawing.Size(128, 24);
            this.RemapColoursNormalMenuItem.Text = "Normal";
            this.RemapColoursNormalMenuItem.Click += new System.EventHandler(this.RemapColoursNormalMenuItem_Click);
            // 
            // RemapColoursCustomMenuItem
            // 
            this.RemapColoursCustomMenuItem.Name = "RemapColoursCustomMenuItem";
            this.RemapColoursCustomMenuItem.Size = new System.Drawing.Size(128, 24);
            this.RemapColoursCustomMenuItem.Text = "Custom";
            this.RemapColoursCustomMenuItem.Click += new System.EventHandler(this.RemapColoursCustomMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(168, 6);
            // 
            // addAnEntityToolStripMenuItem
            // 
            this.addAnEntityToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddTempEntityMenuItem,
            this.AddEntityToDwgMenuItem});
            this.addAnEntityToolStripMenuItem.Name = "addAnEntityToolStripMenuItem";
            this.addAnEntityToolStripMenuItem.Size = new System.Drawing.Size(171, 24);
            this.addAnEntityToolStripMenuItem.Text = "Add an Entity";
            // 
            // AddTempEntityMenuItem
            // 
            this.AddTempEntityMenuItem.Name = "AddTempEntityMenuItem";
            this.AddTempEntityMenuItem.Size = new System.Drawing.Size(192, 24);
            this.AddTempEntityMenuItem.Text = "Temporary";
            this.AddTempEntityMenuItem.Click += new System.EventHandler(this.AddTempEntityMenuItem_Click);
            // 
            // AddEntityToDwgMenuItem
            // 
            this.AddEntityToDwgMenuItem.Name = "AddEntityToDwgMenuItem";
            this.AddEntityToDwgMenuItem.Size = new System.Drawing.Size(192, 24);
            this.AddEntityToDwgMenuItem.Text = "To This Database";
            this.AddEntityToDwgMenuItem.Click += new System.EventHandler(this.AddEntityToDwgMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(168, 6);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createdByToolStripMenuItem,
            this.fentonWebbFentonWebbautodeskcomToolStripMenuItem,
            this.devTechToolStripMenuItem,
            this.autodeskToolStripMenuItem,
            this.may2008ToolStripMenuItem});
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(62, 24);
            this.aboutToolStripMenuItem.Text = "About";
            // 
            // createdByToolStripMenuItem
            // 
            this.createdByToolStripMenuItem.Name = "createdByToolStripMenuItem";
            this.createdByToolStripMenuItem.Size = new System.Drawing.Size(451, 24);
            this.createdByToolStripMenuItem.Text = "               Autodesk Developer Network (ADN) sample";
            // 
            // fentonWebbFentonWebbautodeskcomToolStripMenuItem
            // 
            this.fentonWebbFentonWebbautodeskcomToolStripMenuItem.Name = "fentonWebbFentonWebbautodeskcomToolStripMenuItem";
            this.fentonWebbFentonWebbautodeskcomToolStripMenuItem.Size = new System.Drawing.Size(451, 24);
            this.fentonWebbFentonWebbautodeskcomToolStripMenuItem.Text = "Created by Fenton Webb (Fenton.Webb@autodesk.com)";
            // 
            // devTechToolStripMenuItem
            // 
            this.devTechToolStripMenuItem.Name = "devTechToolStripMenuItem";
            this.devTechToolStripMenuItem.Size = new System.Drawing.Size(451, 24);
            this.devTechToolStripMenuItem.Text = "                                  DevTech";
            // 
            // autodeskToolStripMenuItem
            // 
            this.autodeskToolStripMenuItem.Name = "autodeskToolStripMenuItem";
            this.autodeskToolStripMenuItem.Size = new System.Drawing.Size(451, 24);
            this.autodeskToolStripMenuItem.Text = "                                 Autodesk";
            // 
            // may2008ToolStripMenuItem
            // 
            this.may2008ToolStripMenuItem.Name = "may2008ToolStripMenuItem";
            this.may2008ToolStripMenuItem.Size = new System.Drawing.Size(451, 24);
            this.may2008ToolStripMenuItem.Text = "                             21/May/2008";
            // 
            // mPreviewCtrl
            // 
            this.mPreviewCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mPreviewCtrl.Location = new System.Drawing.Point(0, 28);
            this.mPreviewCtrl.Margin = new System.Windows.Forms.Padding(4);
            this.mPreviewCtrl.Name = "mPreviewCtrl";
            this.mPreviewCtrl.Size = new System.Drawing.Size(952, 688);
            this.mPreviewCtrl.TabIndex = 1;
            // 
            // BlockViewDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(952, 716);
            this.Controls.Add(this.mPreviewCtrl);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "BlockViewDialog";
            this.Text = "BlockViewDialog";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.MenuStrip menuStrip1;
    
    // uncomment the panel declaration so that you can load the Form in the designer
    private GsPreviewCtrl mPreviewCtrl;
    //private System.Windows.Forms.Panel mPreviewCtrl;

    private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem FileOpenMenuItem;
    private System.Windows.Forms.ToolStripMenuItem OutputImageMenuItem;
    private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem ExitMenuItem;
    private System.Windows.Forms.ToolStripMenuItem zoomToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem ZoomWindowMenuItem;
    private System.Windows.Forms.ToolStripMenuItem ZoomExtentsMenuItem;
    private System.Windows.Forms.ToolStripMenuItem ZoomInMenuItem;
    private System.Windows.Forms.ToolStripMenuItem ZoomOutMenuItem;
    private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem showToolStripMenuItem1;
    private System.Windows.Forms.ToolStripMenuItem showLinetypesToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem showSectioningToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem reMapColorsToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem RemapColoursNormalMenuItem;
    private System.Windows.Forms.ToolStripMenuItem RemapColoursCustomMenuItem;
    private System.Windows.Forms.ToolStripMenuItem addAnEntityToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem AddTempEntityMenuItem;
    private System.Windows.Forms.ToolStripMenuItem AddEntityToDwgMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem createdByToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem fentonWebbFentonWebbautodeskcomToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem devTechToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem autodeskToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem may2008ToolStripMenuItem;
	 private System.Windows.Forms.ToolStripMenuItem viewStyleToolStripMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
  }
}