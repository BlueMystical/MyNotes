
namespace MyNotes.Forms
{
	partial class GDriveForm
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GDriveForm));
			this.label1 = new System.Windows.Forms.Label();
			this.lblTrademark = new System.Windows.Forms.Label();
			this.lblAccount = new System.Windows.Forms.Label();
			this.txtUserAccount = new System.Windows.Forms.TextBox();
			this.cmdLogin = new System.Windows.Forms.Button();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabLoadFile = new System.Windows.Forms.TabPage();
			this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
			this.listCloudFiles = new System.Windows.Forms.ListView();
			this.colLoadName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colLoadSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colLoadModified = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.Load_lblSelectFile = new System.Windows.Forms.ToolStripLabel();
			this.Load_txtSearch = new System.Windows.Forms.ToolStripTextBox();
			this.Load_cmdSearch = new System.Windows.Forms.ToolStripButton();
			this.tabSaveFile = new System.Windows.Forms.TabPage();
			this.toolStripContainer2 = new System.Windows.Forms.ToolStripContainer();
			this.listCloudFolders = new System.Windows.Forms.ListView();
			this.colSaveFolder = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colSaveDetails = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colSaveModified = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.toolStrip2 = new System.Windows.Forms.ToolStrip();
			this.Save_lblSelectFile = new System.Windows.Forms.ToolStripLabel();
			this.Save_txtSearch = new System.Windows.Forms.ToolStripTextBox();
			this.Save_cmdSearch = new System.Windows.Forms.ToolStripButton();
			this.Save_cmdCreateFolder = new System.Windows.Forms.ToolStripButton();
			this.panel1 = new System.Windows.Forms.Panel();
			this.cmdCancel = new System.Windows.Forms.Button();
			this.cmdOK = new System.Windows.Forms.Button();
			this.panel2 = new System.Windows.Forms.Panel();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.tabControl1.SuspendLayout();
			this.tabLoadFile.SuspendLayout();
			this.toolStripContainer1.ContentPanel.SuspendLayout();
			this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
			this.toolStripContainer1.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			this.tabSaveFile.SuspendLayout();
			this.toolStripContainer2.ContentPanel.SuspendLayout();
			this.toolStripContainer2.TopToolStripPanel.SuspendLayout();
			this.toolStripContainer2.SuspendLayout();
			this.toolStrip2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(95, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(155, 29);
			this.label1.TabIndex = 1;
			this.label1.Text = "Google Drive";
			// 
			// lblTrademark
			// 
			this.lblTrademark.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblTrademark.Location = new System.Drawing.Point(14, 75);
			this.lblTrademark.Name = "lblTrademark";
			this.lblTrademark.Size = new System.Drawing.Size(371, 32);
			this.lblTrademark.TabIndex = 2;
			this.lblTrademark.Text = "Google Drive is a trademark of Google Inc. Use of this trademark is subject to Go" +
    "ogle Permissions.";
			// 
			// lblAccount
			// 
			this.lblAccount.AutoSize = true;
			this.lblAccount.Location = new System.Drawing.Point(15, 114);
			this.lblAccount.Name = "lblAccount";
			this.lblAccount.Size = new System.Drawing.Size(87, 13);
			this.lblAccount.TabIndex = 3;
			this.lblAccount.Text = "Google Account:";
			// 
			// txtUserAccount
			// 
			this.txtUserAccount.Location = new System.Drawing.Point(128, 111);
			this.txtUserAccount.Name = "txtUserAccount";
			this.txtUserAccount.Size = new System.Drawing.Size(176, 20);
			this.txtUserAccount.TabIndex = 4;
			this.txtUserAccount.Text = "user@gmail.com";
			this.txtUserAccount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// cmdLogin
			// 
			this.cmdLogin.Location = new System.Drawing.Point(310, 109);
			this.cmdLogin.Name = "cmdLogin";
			this.cmdLogin.Size = new System.Drawing.Size(75, 23);
			this.cmdLogin.TabIndex = 5;
			this.cmdLogin.Text = "Log In";
			this.cmdLogin.UseVisualStyleBackColor = true;
			this.cmdLogin.Click += new System.EventHandler(this.cmdLogin_Click);
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabLoadFile);
			this.tabControl1.Controls.Add(this.tabSaveFile);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 142);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(397, 331);
			this.tabControl1.TabIndex = 6;
			// 
			// tabLoadFile
			// 
			this.tabLoadFile.Controls.Add(this.toolStripContainer1);
			this.tabLoadFile.Location = new System.Drawing.Point(4, 22);
			this.tabLoadFile.Name = "tabLoadFile";
			this.tabLoadFile.Padding = new System.Windows.Forms.Padding(3);
			this.tabLoadFile.Size = new System.Drawing.Size(389, 305);
			this.tabLoadFile.TabIndex = 0;
			this.tabLoadFile.Text = "Load from Google Drive:";
			this.tabLoadFile.UseVisualStyleBackColor = true;
			// 
			// toolStripContainer1
			// 
			// 
			// toolStripContainer1.ContentPanel
			// 
			this.toolStripContainer1.ContentPanel.Controls.Add(this.listCloudFiles);
			this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(383, 274);
			this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripContainer1.Location = new System.Drawing.Point(3, 3);
			this.toolStripContainer1.Name = "toolStripContainer1";
			this.toolStripContainer1.Size = new System.Drawing.Size(383, 299);
			this.toolStripContainer1.TabIndex = 2;
			this.toolStripContainer1.Text = "toolStripContainer1";
			// 
			// toolStripContainer1.TopToolStripPanel
			// 
			this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
			// 
			// listCloudFiles
			// 
			this.listCloudFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colLoadName,
            this.colLoadSize,
            this.colLoadModified});
			this.listCloudFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listCloudFiles.HideSelection = false;
			this.listCloudFiles.Location = new System.Drawing.Point(0, 0);
			this.listCloudFiles.Name = "listCloudFiles";
			this.listCloudFiles.Size = new System.Drawing.Size(383, 274);
			this.listCloudFiles.SmallImageList = this.imageList1;
			this.listCloudFiles.TabIndex = 1;
			this.listCloudFiles.UseCompatibleStateImageBehavior = false;
			this.listCloudFiles.View = System.Windows.Forms.View.Details;
			// 
			// colLoadName
			// 
			this.colLoadName.Text = "Name";
			this.colLoadName.Width = 196;
			// 
			// colLoadSize
			// 
			this.colLoadSize.Text = "Size";
			this.colLoadSize.Width = 75;
			// 
			// colLoadModified
			// 
			this.colLoadModified.Text = "Modified";
			this.colLoadModified.Width = 93;
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "logo_drive_2020q4_color_16x16.png");
			this.imageList1.Images.SetKeyName(1, "additem_16x16.png");
			this.imageList1.Images.SetKeyName(2, "open_16x16.png");
			this.imageList1.Images.SetKeyName(3, "book_16.png");
			// 
			// toolStrip1
			// 
			this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Load_lblSelectFile,
            this.Load_txtSearch,
            this.Load_cmdSearch});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(383, 25);
			this.toolStrip1.Stretch = true;
			this.toolStrip1.TabIndex = 0;
			// 
			// Load_lblSelectFile
			// 
			this.Load_lblSelectFile.Name = "Load_lblSelectFile";
			this.Load_lblSelectFile.Size = new System.Drawing.Size(133, 22);
			this.Load_lblSelectFile.Text = "Seleccione una Carpeta:";
			// 
			// Load_txtSearch
			// 
			this.Load_txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Load_txtSearch.Name = "Load_txtSearch";
			this.Load_txtSearch.Size = new System.Drawing.Size(100, 25);
			// 
			// Load_cmdSearch
			// 
			this.Load_cmdSearch.Image = ((System.Drawing.Image)(resources.GetObject("Load_cmdSearch.Image")));
			this.Load_cmdSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.Load_cmdSearch.Name = "Load_cmdSearch";
			this.Load_cmdSearch.Size = new System.Drawing.Size(62, 22);
			this.Load_cmdSearch.Text = "Search";
			this.Load_cmdSearch.Click += new System.EventHandler(this.Load_cmdSearch_Click);
			// 
			// tabSaveFile
			// 
			this.tabSaveFile.Controls.Add(this.toolStripContainer2);
			this.tabSaveFile.Location = new System.Drawing.Point(4, 22);
			this.tabSaveFile.Name = "tabSaveFile";
			this.tabSaveFile.Padding = new System.Windows.Forms.Padding(3);
			this.tabSaveFile.Size = new System.Drawing.Size(389, 305);
			this.tabSaveFile.TabIndex = 1;
			this.tabSaveFile.Text = "Save to Google Drive:";
			this.tabSaveFile.UseVisualStyleBackColor = true;
			// 
			// toolStripContainer2
			// 
			// 
			// toolStripContainer2.ContentPanel
			// 
			this.toolStripContainer2.ContentPanel.Controls.Add(this.listCloudFolders);
			this.toolStripContainer2.ContentPanel.Size = new System.Drawing.Size(383, 274);
			this.toolStripContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripContainer2.Location = new System.Drawing.Point(3, 3);
			this.toolStripContainer2.Name = "toolStripContainer2";
			this.toolStripContainer2.Size = new System.Drawing.Size(383, 299);
			this.toolStripContainer2.TabIndex = 4;
			this.toolStripContainer2.Text = "toolStripContainer2";
			// 
			// toolStripContainer2.TopToolStripPanel
			// 
			this.toolStripContainer2.TopToolStripPanel.Controls.Add(this.toolStrip2);
			// 
			// listCloudFolders
			// 
			this.listCloudFolders.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colSaveFolder,
            this.colSaveDetails,
            this.colSaveModified});
			this.listCloudFolders.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listCloudFolders.HideSelection = false;
			this.listCloudFolders.Location = new System.Drawing.Point(0, 0);
			this.listCloudFolders.Name = "listCloudFolders";
			this.listCloudFolders.Size = new System.Drawing.Size(383, 274);
			this.listCloudFolders.SmallImageList = this.imageList1;
			this.listCloudFolders.TabIndex = 3;
			this.listCloudFolders.UseCompatibleStateImageBehavior = false;
			this.listCloudFolders.View = System.Windows.Forms.View.Details;
			// 
			// colSaveFolder
			// 
			this.colSaveFolder.Text = "Folder";
			this.colSaveFolder.Width = 225;
			// 
			// colSaveDetails
			// 
			this.colSaveDetails.Text = "Details";
			// 
			// colSaveModified
			// 
			this.colSaveModified.Text = "Modified";
			this.colSaveModified.Width = 82;
			// 
			// toolStrip2
			// 
			this.toolStrip2.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Save_lblSelectFile,
            this.Save_txtSearch,
            this.Save_cmdSearch,
            this.Save_cmdCreateFolder});
			this.toolStrip2.Location = new System.Drawing.Point(3, 0);
			this.toolStrip2.Name = "toolStrip2";
			this.toolStrip2.Size = new System.Drawing.Size(276, 25);
			this.toolStrip2.TabIndex = 0;
			// 
			// Save_lblSelectFile
			// 
			this.Save_lblSelectFile.Name = "Save_lblSelectFile";
			this.Save_lblSelectFile.Size = new System.Drawing.Size(86, 22);
			this.Save_lblSelectFile.Text = "Select a Folder:";
			// 
			// Save_txtSearch
			// 
			this.Save_txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Save_txtSearch.Name = "Save_txtSearch";
			this.Save_txtSearch.Size = new System.Drawing.Size(100, 25);
			// 
			// Save_cmdSearch
			// 
			this.Save_cmdSearch.Image = ((System.Drawing.Image)(resources.GetObject("Save_cmdSearch.Image")));
			this.Save_cmdSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.Save_cmdSearch.Name = "Save_cmdSearch";
			this.Save_cmdSearch.Size = new System.Drawing.Size(62, 22);
			this.Save_cmdSearch.Text = "Search";
			this.Save_cmdSearch.Click += new System.EventHandler(this.Save_cmdSearch_Click_1);
			// 
			// Save_cmdCreateFolder
			// 
			this.Save_cmdCreateFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.Save_cmdCreateFolder.Image = ((System.Drawing.Image)(resources.GetObject("Save_cmdCreateFolder.Image")));
			this.Save_cmdCreateFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.Save_cmdCreateFolder.Name = "Save_cmdCreateFolder";
			this.Save_cmdCreateFolder.Size = new System.Drawing.Size(23, 22);
			this.Save_cmdCreateFolder.Text = "toolStripButton2";
			this.Save_cmdCreateFolder.Click += new System.EventHandler(this.Save_cmdCreateFolder_ClickAsync);
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.SystemColors.ControlLight;
			this.panel1.Controls.Add(this.progressBar1);
			this.panel1.Controls.Add(this.cmdCancel);
			this.panel1.Controls.Add(this.cmdOK);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 473);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(397, 36);
			this.panel1.TabIndex = 7;
			// 
			// cmdCancel
			// 
			this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.Location = new System.Drawing.Point(232, 7);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(75, 23);
			this.cmdCancel.TabIndex = 1;
			this.cmdCancel.Text = "Cancel";
			this.cmdCancel.UseVisualStyleBackColor = true;
			this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
			// 
			// cmdOK
			// 
			this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdOK.Location = new System.Drawing.Point(313, 7);
			this.cmdOK.Name = "cmdOK";
			this.cmdOK.Size = new System.Drawing.Size(75, 23);
			this.cmdOK.TabIndex = 0;
			this.cmdOK.Text = "OK";
			this.cmdOK.UseVisualStyleBackColor = true;
			this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.pictureBox1);
			this.panel2.Controls.Add(this.label1);
			this.panel2.Controls.Add(this.lblTrademark);
			this.panel2.Controls.Add(this.cmdLogin);
			this.panel2.Controls.Add(this.lblAccount);
			this.panel2.Controls.Add(this.txtUserAccount);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel2.Location = new System.Drawing.Point(0, 0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(397, 142);
			this.panel2.TabIndex = 8;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = global::MyNotes.Properties.Resources.logo_drive_2020q4_color_2x_web_64dp;
			this.pictureBox1.Location = new System.Drawing.Point(3, 3);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(72, 69);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(7, 6);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(219, 23);
			this.progressBar1.TabIndex = 2;
			// 
			// GDriveForm
			// 
			this.AcceptButton = this.cmdOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cmdCancel;
			this.ClientSize = new System.Drawing.Size(397, 509);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "GDriveForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Google Drive Integration:";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GDriveForm_FormClosing);
			this.Load += new System.EventHandler(this.GDriveForm_Load);
			this.Shown += new System.EventHandler(this.GDriveForm_Shown);
			this.tabControl1.ResumeLayout(false);
			this.tabLoadFile.ResumeLayout(false);
			this.toolStripContainer1.ContentPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.PerformLayout();
			this.toolStripContainer1.ResumeLayout(false);
			this.toolStripContainer1.PerformLayout();
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.tabSaveFile.ResumeLayout(false);
			this.toolStripContainer2.ContentPanel.ResumeLayout(false);
			this.toolStripContainer2.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainer2.TopToolStripPanel.PerformLayout();
			this.toolStripContainer2.ResumeLayout(false);
			this.toolStripContainer2.PerformLayout();
			this.toolStrip2.ResumeLayout(false);
			this.toolStrip2.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label lblTrademark;
		private System.Windows.Forms.Label lblAccount;
		private System.Windows.Forms.TextBox txtUserAccount;
		private System.Windows.Forms.Button cmdLogin;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabLoadFile;
		private System.Windows.Forms.TabPage tabSaveFile;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button cmdOK;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.ListView listCloudFiles;
		private System.Windows.Forms.ColumnHeader colLoadName;
		private System.Windows.Forms.ColumnHeader colLoadSize;
		private System.Windows.Forms.ColumnHeader colLoadModified;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.ListView listCloudFolders;
		private System.Windows.Forms.ColumnHeader colSaveFolder;
		private System.Windows.Forms.ColumnHeader colSaveModified;
		private System.Windows.Forms.ToolStripContainer toolStripContainer1;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripLabel Load_lblSelectFile;
		private System.Windows.Forms.ToolStripTextBox Load_txtSearch;
		private System.Windows.Forms.ToolStripButton Load_cmdSearch;
		private System.Windows.Forms.ToolStripContainer toolStripContainer2;
		private System.Windows.Forms.ToolStrip toolStrip2;
		private System.Windows.Forms.ToolStripLabel Save_lblSelectFile;
		private System.Windows.Forms.ToolStripTextBox Save_txtSearch;
		private System.Windows.Forms.ToolStripButton Save_cmdSearch;
		private System.Windows.Forms.ToolStripButton Save_cmdCreateFolder;
		private System.Windows.Forms.ColumnHeader colSaveDetails;
		private System.Windows.Forms.ProgressBar progressBar1;
	}
}