
namespace MyNotes.Forms
{
	partial class TableForm
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
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.txColumnCount = new System.Windows.Forms.NumericUpDown();
			this.txRowCount = new System.Windows.Forms.NumericUpDown();
			this.cmdOK = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.txColumnCount)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.txRowCount)).BeginInit();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(50, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Columns:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(26, 42);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(37, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Rows:";
			// 
			// txColumnCount
			// 
			this.txColumnCount.Location = new System.Drawing.Point(75, 13);
			this.txColumnCount.Name = "txColumnCount";
			this.txColumnCount.Size = new System.Drawing.Size(49, 20);
			this.txColumnCount.TabIndex = 2;
			this.txColumnCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// txRowCount
			// 
			this.txRowCount.Location = new System.Drawing.Point(75, 40);
			this.txRowCount.Name = "txRowCount";
			this.txRowCount.Size = new System.Drawing.Size(49, 20);
			this.txRowCount.TabIndex = 3;
			this.txRowCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// cmdOK
			// 
			this.cmdOK.Location = new System.Drawing.Point(39, 9);
			this.cmdOK.Name = "cmdOK";
			this.cmdOK.Size = new System.Drawing.Size(75, 23);
			this.cmdOK.TabIndex = 4;
			this.cmdOK.Text = "OK";
			this.cmdOK.UseVisualStyleBackColor = true;
			this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.SystemColors.ControlLight;
			this.panel1.Controls.Add(this.cmdOK);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 69);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(153, 40);
			this.panel1.TabIndex = 5;
			// 
			// TableForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(153, 109);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.txRowCount);
			this.Controls.Add(this.txColumnCount);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "TableForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "New Table";
			this.Load += new System.EventHandler(this.TableForm_Load);
			this.Shown += new System.EventHandler(this.TableForm_Shown);
			((System.ComponentModel.ISupportInitialize)(this.txColumnCount)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.txRowCount)).EndInit();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown txColumnCount;
		private System.Windows.Forms.NumericUpDown txRowCount;
		private System.Windows.Forms.Button cmdOK;
		private System.Windows.Forms.Panel panel1;
	}
}