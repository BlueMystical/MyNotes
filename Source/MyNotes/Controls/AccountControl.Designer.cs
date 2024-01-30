
namespace MyNotes.Controls
{
	partial class AccountControl
	{
		/// <summary> 
		/// Variable del diseñador necesaria.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Limpiar los recursos que se estén usando.
		/// </summary>
		/// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Código generado por el Diseñador de componentes

		/// <summary> 
		/// Método necesario para admitir el Diseñador. No se puede modificar
		/// el contenido de este método con el editor de código.
		/// </summary>
		private void InitializeComponent()
		{
			this.txUserName = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.txPassword = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.txEmail = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.txName = new System.Windows.Forms.TextBox();
			this.txExtraInfo = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.picLockSafe = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.picLockSafe)).BeginInit();
			this.SuspendLayout();
			// 
			// txUserName
			// 
			this.txUserName.BackColor = System.Drawing.SystemColors.Control;
			this.txUserName.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txUserName.ForeColor = System.Drawing.Color.MediumBlue;
			this.txUserName.Location = new System.Drawing.Point(56, 89);
			this.txUserName.Name = "txUserName";
			this.txUserName.Size = new System.Drawing.Size(219, 13);
			this.txUserName.TabIndex = 15;
			this.txUserName.Text = "user name";
			this.txUserName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.txUserName.TextChanged += new System.EventHandler(this.txField_TextChanged);
			this.txUserName.MouseEnter += new System.EventHandler(this.txField_Enter);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
			this.label4.Location = new System.Drawing.Point(7, 89);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(32, 13);
			this.label4.TabIndex = 14;
			this.label4.Text = "User:";
			// 
			// txPassword
			// 
			this.txPassword.BackColor = System.Drawing.SystemColors.Control;
			this.txPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txPassword.ForeColor = System.Drawing.Color.MediumBlue;
			this.txPassword.Location = new System.Drawing.Point(56, 63);
			this.txPassword.Name = "txPassword";
			this.txPassword.Size = new System.Drawing.Size(219, 13);
			this.txPassword.TabIndex = 13;
			this.txPassword.Text = "password";
			this.txPassword.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.txPassword.TextChanged += new System.EventHandler(this.txField_TextChanged);
			this.txPassword.MouseEnter += new System.EventHandler(this.txField_Enter);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
			this.label3.Location = new System.Drawing.Point(6, 63);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(56, 13);
			this.label3.TabIndex = 12;
			this.label3.Text = "Password:";
			// 
			// txEmail
			// 
			this.txEmail.BackColor = System.Drawing.SystemColors.Control;
			this.txEmail.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txEmail.ForeColor = System.Drawing.Color.MediumBlue;
			this.txEmail.Location = new System.Drawing.Point(56, 37);
			this.txEmail.Name = "txEmail";
			this.txEmail.Size = new System.Drawing.Size(219, 13);
			this.txEmail.TabIndex = 11;
			this.txEmail.Text = "user.name@gmail.com";
			this.txEmail.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.txEmail.TextChanged += new System.EventHandler(this.txField_TextChanged);
			this.txEmail.MouseEnter += new System.EventHandler(this.txField_Enter);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
			this.label2.Location = new System.Drawing.Point(7, 37);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(35, 13);
			this.label2.TabIndex = 10;
			this.label2.Text = "Email:";
			// 
			// txName
			// 
			this.txName.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txName.ForeColor = System.Drawing.Color.Maroon;
			this.txName.Location = new System.Drawing.Point(5, 5);
			this.txName.Name = "txName";
			this.txName.Size = new System.Drawing.Size(272, 25);
			this.txName.TabIndex = 9;
			this.txName.Text = "[Titulo Largo de Este Item]";
			this.txName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.txName.TextChanged += new System.EventHandler(this.txField_TextChanged);
			this.txName.MouseEnter += new System.EventHandler(this.txField_Enter);
			// 
			// txExtraInfo
			// 
			this.txExtraInfo.BackColor = System.Drawing.SystemColors.Control;
			this.txExtraInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txExtraInfo.ForeColor = System.Drawing.Color.MediumBlue;
			this.txExtraInfo.Location = new System.Drawing.Point(56, 115);
			this.txExtraInfo.Name = "txExtraInfo";
			this.txExtraInfo.Size = new System.Drawing.Size(219, 13);
			this.txExtraInfo.TabIndex = 17;
			this.txExtraInfo.Text = "Something";
			this.txExtraInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.txExtraInfo.TextChanged += new System.EventHandler(this.txField_TextChanged);
			this.txExtraInfo.MouseEnter += new System.EventHandler(this.txField_Enter);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
			this.label5.Location = new System.Drawing.Point(7, 115);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(34, 13);
			this.label5.TabIndex = 16;
			this.label5.Text = "Extra:";
			// 
			// picLockSafe
			// 
			this.picLockSafe.Image = global::MyNotes.Properties.Resources.Lock;
			this.picLockSafe.Location = new System.Drawing.Point(68, 37);
			this.picLockSafe.Name = "picLockSafe";
			this.picLockSafe.Size = new System.Drawing.Size(207, 101);
			this.picLockSafe.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.picLockSafe.TabIndex = 18;
			this.picLockSafe.TabStop = false;
			this.picLockSafe.Visible = false;
			// 
			// AccountControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.picLockSafe);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.txName);
			this.Controls.Add(this.txPassword);
			this.Controls.Add(this.txExtraInfo);
			this.Controls.Add(this.txUserName);
			this.Controls.Add(this.txEmail);
			this.Name = "AccountControl";
			this.Size = new System.Drawing.Size(283, 144);
			this.Load += new System.EventHandler(this.AccountControl_Load);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AccountControl_MouseDown);
			((System.ComponentModel.ISupportInitialize)(this.picLockSafe)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txUserName;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox txPassword;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txEmail;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txName;
		private System.Windows.Forms.TextBox txExtraInfo;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.PictureBox picLockSafe;
	}
}
