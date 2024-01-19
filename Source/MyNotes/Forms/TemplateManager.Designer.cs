
namespace MyNotes
{
	partial class TemplateManager
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TemplateManager));
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.cmdSaveTemplate = new System.Windows.Forms.ToolStripButton();
			this.menuTemplates = new System.Windows.Forms.ToolStripDropDownButton();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.FirstLevel = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.SecondLevel = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ThirdLevel = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.FourthLevel = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.treeView1 = new System.Windows.Forms.TreeView();
			this.lblTemplate_Preview = new System.Windows.Forms.Label();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.lblTemplate_Content = new System.Windows.Forms.Label();
			this.grpTemplateDetail = new System.Windows.Forms.GroupBox();
			this.lblTemplate_Name = new System.Windows.Forms.Label();
			this.lblTemplate_Description = new System.Windows.Forms.Label();
			this.lblTemplate_Author = new System.Windows.Forms.Label();
			this.txtTemplate_Name = new System.Windows.Forms.TextBox();
			this.txtTemplate_Author = new System.Windows.Forms.TextBox();
			this.txtTemplate_Description = new System.Windows.Forms.TextBox();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.insertRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStrip1.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.grpTemplateDetail.SuspendLayout();
			this.contextMenuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuTemplates,
            this.cmdSaveTemplate});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(946, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// cmdSaveTemplate
			// 
			this.cmdSaveTemplate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.cmdSaveTemplate.Image = ((System.Drawing.Image)(resources.GetObject("cmdSaveTemplate.Image")));
			this.cmdSaveTemplate.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.cmdSaveTemplate.Name = "cmdSaveTemplate";
			this.cmdSaveTemplate.Size = new System.Drawing.Size(23, 22);
			this.cmdSaveTemplate.Text = "Save";
			this.cmdSaveTemplate.Click += new System.EventHandler(this.cmdSaveTemplate_Click);
			// 
			// menuTemplates
			// 
			this.menuTemplates.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.menuTemplates.Image = ((System.Drawing.Image)(resources.GetObject("menuTemplates.Image")));
			this.menuTemplates.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.menuTemplates.Name = "menuTemplates";
			this.menuTemplates.Size = new System.Drawing.Size(73, 22);
			this.menuTemplates.Text = "Templates";
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
			this.statusStrip1.Location = new System.Drawing.Point(0, 428);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(946, 22);
			this.statusStrip1.TabIndex = 1;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// lblStatus
			// 
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(42, 17);
			this.lblStatus.Text = "Ready.";
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 25);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.dataGridView1);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			this.splitContainer1.Size = new System.Drawing.Size(946, 403);
			this.splitContainer1.SplitterDistance = 562;
			this.splitContainer1.TabIndex = 2;
			// 
			// dataGridView1
			// 
			this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.FirstLevel,
            this.SecondLevel,
            this.ThirdLevel,
            this.FourthLevel});
			this.dataGridView1.ContextMenuStrip = this.contextMenuStrip1;
			this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGridView1.Location = new System.Drawing.Point(0, 0);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.Size = new System.Drawing.Size(562, 403);
			this.dataGridView1.TabIndex = 0;
			this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellClick);
			// 
			// FirstLevel
			// 
			this.FirstLevel.DataPropertyName = "first_level";
			this.FirstLevel.HeaderText = "First Level";
			this.FirstLevel.Name = "FirstLevel";
			// 
			// SecondLevel
			// 
			this.SecondLevel.DataPropertyName = "second_level";
			this.SecondLevel.HeaderText = "Second Level";
			this.SecondLevel.Name = "SecondLevel";
			// 
			// ThirdLevel
			// 
			this.ThirdLevel.DataPropertyName = "third_level";
			this.ThirdLevel.HeaderText = "Third Level";
			this.ThirdLevel.Name = "ThirdLevel";
			// 
			// FourthLevel
			// 
			this.FourthLevel.DataPropertyName = "fourth_level";
			this.FourthLevel.HeaderText = "Fourth Level";
			this.FourthLevel.Name = "FourthLevel";
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.treeView1);
			this.splitContainer2.Panel1.Controls.Add(this.lblTemplate_Preview);
			this.splitContainer2.Panel1.Controls.Add(this.grpTemplateDetail);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.richTextBox1);
			this.splitContainer2.Panel2.Controls.Add(this.lblTemplate_Content);
			this.splitContainer2.Size = new System.Drawing.Size(380, 403);
			this.splitContainer2.SplitterDistance = 259;
			this.splitContainer2.TabIndex = 0;
			// 
			// treeView1
			// 
			this.treeView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView1.Location = new System.Drawing.Point(0, 123);
			this.treeView1.Name = "treeView1";
			this.treeView1.Size = new System.Drawing.Size(380, 136);
			this.treeView1.TabIndex = 0;
			// 
			// lblTemplate_Preview
			// 
			this.lblTemplate_Preview.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblTemplate_Preview.Location = new System.Drawing.Point(0, 100);
			this.lblTemplate_Preview.Name = "lblTemplate_Preview";
			this.lblTemplate_Preview.Size = new System.Drawing.Size(380, 23);
			this.lblTemplate_Preview.TabIndex = 1;
			this.lblTemplate_Preview.Text = "Preview:";
			this.lblTemplate_Preview.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// richTextBox1
			// 
			this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBox1.Location = new System.Drawing.Point(0, 23);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.Size = new System.Drawing.Size(380, 117);
			this.richTextBox1.TabIndex = 0;
			this.richTextBox1.Text = "";
			// 
			// lblTemplate_Content
			// 
			this.lblTemplate_Content.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblTemplate_Content.Location = new System.Drawing.Point(0, 0);
			this.lblTemplate_Content.Name = "lblTemplate_Content";
			this.lblTemplate_Content.Size = new System.Drawing.Size(380, 23);
			this.lblTemplate_Content.TabIndex = 1;
			this.lblTemplate_Content.Text = "Content:";
			this.lblTemplate_Content.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// grpTemplateDetail
			// 
			this.grpTemplateDetail.Controls.Add(this.txtTemplate_Description);
			this.grpTemplateDetail.Controls.Add(this.txtTemplate_Author);
			this.grpTemplateDetail.Controls.Add(this.txtTemplate_Name);
			this.grpTemplateDetail.Controls.Add(this.lblTemplate_Author);
			this.grpTemplateDetail.Controls.Add(this.lblTemplate_Description);
			this.grpTemplateDetail.Controls.Add(this.lblTemplate_Name);
			this.grpTemplateDetail.Dock = System.Windows.Forms.DockStyle.Top;
			this.grpTemplateDetail.Location = new System.Drawing.Point(0, 0);
			this.grpTemplateDetail.Name = "grpTemplateDetail";
			this.grpTemplateDetail.Size = new System.Drawing.Size(380, 100);
			this.grpTemplateDetail.TabIndex = 2;
			this.grpTemplateDetail.TabStop = false;
			this.grpTemplateDetail.Text = "Template Detail:";
			// 
			// lblTemplate_Name
			// 
			this.lblTemplate_Name.AutoSize = true;
			this.lblTemplate_Name.Location = new System.Drawing.Point(7, 23);
			this.lblTemplate_Name.Name = "lblTemplate_Name";
			this.lblTemplate_Name.Size = new System.Drawing.Size(38, 13);
			this.lblTemplate_Name.TabIndex = 0;
			this.lblTemplate_Name.Text = "Name:";
			// 
			// lblTemplate_Description
			// 
			this.lblTemplate_Description.AutoSize = true;
			this.lblTemplate_Description.Location = new System.Drawing.Point(7, 75);
			this.lblTemplate_Description.Name = "lblTemplate_Description";
			this.lblTemplate_Description.Size = new System.Drawing.Size(63, 13);
			this.lblTemplate_Description.TabIndex = 1;
			this.lblTemplate_Description.Text = "Description:";
			// 
			// lblTemplate_Author
			// 
			this.lblTemplate_Author.AutoSize = true;
			this.lblTemplate_Author.Location = new System.Drawing.Point(7, 49);
			this.lblTemplate_Author.Name = "lblTemplate_Author";
			this.lblTemplate_Author.Size = new System.Drawing.Size(41, 13);
			this.lblTemplate_Author.TabIndex = 2;
			this.lblTemplate_Author.Text = "Author:";
			// 
			// txtTemplate_Name
			// 
			this.txtTemplate_Name.Location = new System.Drawing.Point(73, 20);
			this.txtTemplate_Name.Name = "txtTemplate_Name";
			this.txtTemplate_Name.Size = new System.Drawing.Size(295, 20);
			this.txtTemplate_Name.TabIndex = 3;
			// 
			// txtTemplate_Author
			// 
			this.txtTemplate_Author.Location = new System.Drawing.Point(73, 46);
			this.txtTemplate_Author.Name = "txtTemplate_Author";
			this.txtTemplate_Author.Size = new System.Drawing.Size(295, 20);
			this.txtTemplate_Author.TabIndex = 4;
			// 
			// txtTemplate_Description
			// 
			this.txtTemplate_Description.Location = new System.Drawing.Point(73, 73);
			this.txtTemplate_Description.Name = "txtTemplate_Description";
			this.txtTemplate_Description.Size = new System.Drawing.Size(295, 20);
			this.txtTemplate_Description.TabIndex = 5;
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insertRowToolStripMenuItem,
            this.deleteRowToolStripMenuItem});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(140, 48);
			// 
			// insertRowToolStripMenuItem
			// 
			this.insertRowToolStripMenuItem.Name = "insertRowToolStripMenuItem";
			this.insertRowToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			this.insertRowToolStripMenuItem.Text = "&Insert Row..";
			this.insertRowToolStripMenuItem.Click += new System.EventHandler(this.insertRowToolStripMenuItem_Click);
			// 
			// deleteRowToolStripMenuItem
			// 
			this.deleteRowToolStripMenuItem.Name = "deleteRowToolStripMenuItem";
			this.deleteRowToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			this.deleteRowToolStripMenuItem.Text = "Delete Row..";
			this.deleteRowToolStripMenuItem.Click += new System.EventHandler(this.deleteRowToolStripMenuItem_Click);
			// 
			// TemplateManager
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(946, 450);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.toolStrip1);
			this.Name = "TemplateManager";
			this.Text = "TemplateManager";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TemplateManager_FormClosing);
			this.Load += new System.EventHandler(this.TemplateManager_Load);
			this.Shown += new System.EventHandler(this.TemplateManager_Shown);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.grpTemplateDetail.ResumeLayout(false);
			this.grpTemplateDetail.PerformLayout();
			this.contextMenuStrip1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton cmdSaveTemplate;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel lblStatus;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.RichTextBox richTextBox1;
		private System.Windows.Forms.Label lblTemplate_Content;
		private System.Windows.Forms.TreeView treeView1;
		private System.Windows.Forms.Label lblTemplate_Preview;
		private System.Windows.Forms.ToolStripDropDownButton menuTemplates;
		private System.Windows.Forms.DataGridViewTextBoxColumn FirstLevel;
		private System.Windows.Forms.DataGridViewTextBoxColumn SecondLevel;
		private System.Windows.Forms.DataGridViewTextBoxColumn ThirdLevel;
		private System.Windows.Forms.DataGridViewTextBoxColumn FourthLevel;
		private System.Windows.Forms.GroupBox grpTemplateDetail;
		private System.Windows.Forms.TextBox txtTemplate_Description;
		private System.Windows.Forms.TextBox txtTemplate_Author;
		private System.Windows.Forms.TextBox txtTemplate_Name;
		private System.Windows.Forms.Label lblTemplate_Author;
		private System.Windows.Forms.Label lblTemplate_Description;
		private System.Windows.Forms.Label lblTemplate_Name;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem insertRowToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteRowToolStripMenuItem;
	}
}