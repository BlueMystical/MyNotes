using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MyNotes.Controls
{
	/// <summary>Muestra informacion de una Cuenta.</summary>
	public partial class AccountControl : UserControl
	{
		#region Private Members

		private Color borderColor = Color.MediumSlateBlue;
		private Color borderFocusColor = Color.HotPink;
		
		private bool underlinedStyle = false;
		private bool isFocused = false;
		private int borderRadius = 8;
		private int borderSize = 2;

		private bool IsLoading = false;
		private ContextMenu _contextMenu;

		#endregion

		#region Win32 API Declarations

		private const int EM_SETCUEBANNER = 5377;
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private extern static IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

		[DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
		private static extern IntPtr CreateRoundRectRgn
		(
			int nLeftRect,     // x-coordinate of upper-left corner
			int nTopRect,      // y-coordinate of upper-left corner
			int nRightRect,    // x-coordinate of lower-right corner
			int nBottomRect,   // y-coordinate of lower-right corner
			int nWidthEllipse, // height of ellipse
			int nHeightEllipse // width of ellipse
		);

		#endregion

		#region Public Properties

		public Account Data { get; set; }

		/// <summary>For Translations.</summary>
		public Traductor CurrentLanguage { get; set; }

		/// <summary>If 'true' this content is Password Protected and has not passed the verification. The Shield will hide the sensitive data.
		/// <para>If 'false', the content can be seen freely.</para>
		/// </summary>
		public bool SecuredContent { get; set; } = true;

		public Color BorderColor
		{
			get { return borderColor; }
			set
			{
				borderColor = value;
				this.Invalidate();
			}
		}
		public Color BorderFocusColor
		{
			get { return borderFocusColor; }
			set { borderFocusColor = value; }
		}
		public override Color ForeColor
		{
			get { return base.ForeColor; }
			set
			{
				base.ForeColor = value;
				this.ForeColor = value;
			}
		}
		
		public int BorderSize
		{
			get { return borderSize; }
			set
			{
				if (value >= 1)
				{
					borderSize = value;
					this.Invalidate();
				}
			}
		}
		public int BorderRadius
		{
			get { return borderRadius; }
			set
			{
				if (value >= 0)
				{
					borderRadius = value;
					this.Invalidate();//Redraw control
				}
			}
		}

		public event EventHandler OnRequestNewAccount = delegate { };
		public event EventHandler OnRequestDeleteAccount = delegate { };
		public event EventHandler OnSelection = delegate { };

		#endregion

		#region Constructors

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pData">Data to be Loaded in the Control</param>
		/// <param name="pSecuredContent">'true': Hide Content, 'false': Show Content</param>
		public AccountControl(Account pData, bool pSecuredContent, Traductor pLanguage)
		{
			InitializeComponent();

			// For Rounded Corners:
			this.BorderStyle = BorderStyle.None;
			Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 10, 10));

			Data = pData;
			CurrentLanguage = pLanguage;
			SecuredContent = pSecuredContent;

			_contextMenu = new ContextMenu();

			// Add menu items using the `Items` property
			_contextMenu.MenuItems.Add(new MenuItem(CurrentLanguage.GetTranslation("NewEntry"), ContextMenu_Click) { Name = "mnuNewEntry" });
			_contextMenu.MenuItems.Add(new MenuItem(CurrentLanguage.GetTranslation("DeleteDoc"), ContextMenu_Click) { Name = "mnuDeleteDoc" });
			_contextMenu.MenuItems.Add(new MenuItem("-")); // Separator
			_contextMenu.MenuItems.Add(new MenuItem(CurrentLanguage.GetTranslation("Copy"), ContextMenu_Click) { Name = "mnuCopy" });
			_contextMenu.MenuItems.Add(new MenuItem(CurrentLanguage.GetTranslation("Cut"), ContextMenu_Click) { Name = "mnuCut" });
			_contextMenu.MenuItems.Add(new MenuItem(CurrentLanguage.GetTranslation("Paste"), ContextMenu_Click) { Name = "mnuPaste" });

			// Oculta o Muestra Menus dependiendo del tipo de control que fue clickeado:
			_contextMenu.Popup += (object sender, EventArgs e) =>
			{
				var _Sender = _contextMenu.SourceControl;
				if (_Sender is TextBox)
				{
					_contextMenu.MenuItems[0].Visible = false; // mnuNewEntry
					_contextMenu.MenuItems[1].Visible = false; // mnuDeleteDoc
				}
				else
				{
					_contextMenu.MenuItems[0].Visible = true; // mnuNewEntry
					_contextMenu.MenuItems[1].Visible = true; // mnuDeleteDoc
				}
			};

			this.ContextMenu = _contextMenu;
			this.txEmail.ContextMenu = _contextMenu;
			this.txPassword.ContextMenu = _contextMenu;
			this.txUserName.ContextMenu = _contextMenu;
			this.txExtraInfo.ContextMenu = _contextMenu;

			// Add place-holders to the Text Boxes:
			SendMessage(this.txName.Handle, EM_SETCUEBANNER, new IntPtr(0), "Account Name");
			SendMessage(this.txEmail.Handle, EM_SETCUEBANNER, new IntPtr(0), "e-mail");
			SendMessage(this.txPassword.Handle, EM_SETCUEBANNER, new IntPtr(0), "password");
			//SendMessage(this.txUserName.Handle, EM_SETCUEBANNER, new IntPtr(0), "user name");
			//SendMessage(this.txExtraInfo.Handle, EM_SETCUEBANNER, new IntPtr(0), "Aditional Info");
		}

		private void AccountControl_Load(object sender, EventArgs e)
		{
			IsLoading = true;
			if (Data != null)
			{
				txName.Text = Data.Name;
				txEmail.Text = Data.Email;
				txPassword.Text = Data.Password;
				txUserName.Text = Data.User;
				txExtraInfo.Text = Data.Extra;
			}

			picLockSafe.Visible = SecuredContent;
			IsLoading = false;
		}

		// For Rounded Corners:
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			Graphics graph = e.Graphics;

			if (borderRadius > 1)//Rounded TextBox
			{
				//-Fields
				var rectBorderSmooth = this.ClientRectangle;
				var rectBorder = Rectangle.Inflate(rectBorderSmooth, -borderSize, -borderSize);
				int smoothSize = borderSize > 0 ? borderSize : 1;

				using (GraphicsPath pathBorderSmooth = GetFigurePath(rectBorderSmooth, borderRadius))
				using (GraphicsPath pathBorder = GetFigurePath(rectBorder, borderRadius - borderSize))
				using (Pen penBorderSmooth = new Pen(this.Parent.BackColor, smoothSize))
				using (Pen penBorder = new Pen(borderColor, borderSize))
				{
					//-Drawing
					this.Region = new Region(pathBorderSmooth);//Set the rounded region of UserControl
					if (borderRadius > 15) SetTextBoxRoundedRegion();//Set the rounded region of TextBox component
					graph.SmoothingMode = SmoothingMode.AntiAlias;
					penBorder.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;
					if (isFocused) penBorder.Color = borderFocusColor;

					if (underlinedStyle) //Line Style
					{
						//Draw border smoothing
						graph.DrawPath(penBorderSmooth, pathBorderSmooth);
						//Draw border
						graph.SmoothingMode = SmoothingMode.None;
						graph.DrawLine(penBorder, 0, this.Height - 1, this.Width, this.Height - 1);
					}
					else //Normal Style
					{
						//Draw border smoothing
						graph.DrawPath(penBorderSmooth, pathBorderSmooth);
						//Draw border
						graph.DrawPath(penBorder, pathBorder);
					}
				}
			}
		}

		#endregion

		#region Private Methods

		// For Rounded Corners:
		private void SetTextBoxRoundedRegion()
		{
			GraphicsPath pathTxt;

			pathTxt = GetFigurePath(this.ClientRectangle, borderSize * 2);
			this.Region = new Region(pathTxt);

			pathTxt.Dispose();
		}

		// For Rounded Corners:
		private GraphicsPath GetFigurePath(Rectangle rect, int radius)
		{
			GraphicsPath path = new GraphicsPath();
			float curveSize = radius * 2F;

			path.StartFigure();
			path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
			path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
			path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
			path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
			path.CloseFigure();
			return path;
		}

		#endregion

		#region Public Methods

		/// <summary>Retrieves the Data of the Control in a JSON string.
		/// <para>Use 'Data' property to get the actual Data.</para></summary>
		public string GetJsonData()
		{
			string _ret = string.Empty;
			if (Data != null)
			{
				_ret = Util.Serialize_ToJSON(Data);
			}
			return _ret;
		}

		/// <summary>Set the Data to be Shown in the Control.</summary>
		/// <param name="pData">The data</param>
		public void SetData(Account pData)
		{
			this.Data = pData;
			if (pData != null)
			{
				txName.Text = Data.Name;
				txEmail.Text = Data.Email;
				txPassword.Text = Data.Password;
				txUserName.Text = Data.User;
				txExtraInfo.Text = Data.Extra;
			}
			else
			{
				txName.Text = string.Empty;
				txEmail.Text = string.Empty;
				txPassword.Text = string.Empty;
				txUserName.Text = string.Empty;
				txExtraInfo.Text = string.Empty;
			}
		}

		#endregion

		#region Control Events

		// Clicked on any of the Context Menus
		private void ContextMenu_Click(object sender, EventArgs e)
		{
			if (sender is MenuItem)
			{
				var _Menu = sender as MenuItem;
				var _Sender = (_Menu.Parent as ContextMenu).SourceControl;

				switch (_Menu.Name)
				{
					case "mnuNewEntry":
						OnRequestNewAccount?.Invoke(this, EventArgs.Empty);
						break;

					case "mnuDeleteDoc":
						OnRequestDeleteAccount?.Invoke(this, EventArgs.Empty);
						break;

					case "mnuCopy":
						if (_Sender is TextBox)
						{
							Clipboard.SetText((_Sender as TextBox).Text);
						}
						if (_Sender is AccountControl)
						{
							Clipboard.SetText((_Sender as AccountControl).GetJsonData());
						}
						break;
					case "mnuCut":
						if (_Sender is TextBox)
						{
							Clipboard.SetText((_Sender as TextBox).Text);
							(_Sender as TextBox).Text = string.Empty;
						}
						if (_Sender is AccountControl)
						{
							Clipboard.SetText((_Sender as AccountControl).GetJsonData(), TextDataFormat.Text);
						}
						break;
					case "mnuPaste":
						if (_Sender is TextBox)
						{
							if (Clipboard.ContainsText()) (_Sender as TextBox).Text = Clipboard.GetText();
						}
						if (_Sender is AccountControl)
						{
							if (Clipboard.ContainsText())
							{
								string Json = Clipboard.GetText();
								if (!string.IsNullOrEmpty(Json))
								{
									try
									{
										(_Sender as AccountControl).SetData(
										Util.DeSerialize_FromJSON_String<Account>(Json));
									}
									catch { }
								}
							}														
						}
						break;
					default: break;
				}
			}
		}

		//Mouse Entered a TextField
		private void txField_Enter(object sender, EventArgs e)
		{
			(sender as TextBox).SelectAll();
			this.isFocused = true;
			OnSelection?.Invoke(this, EventArgs.Empty);
		}

		private void txField_TextChanged(object sender, EventArgs e)
		{
			if (!IsLoading)
			{
				var _CTRL = sender as TextBox;
				switch (_CTRL.Name)
				{
					case "txName":		Data.Name =		_CTRL.Text; break;
					case "txEmail":		Data.Email =	_CTRL.Text; break;
					case "txPassword":	Data.Password = _CTRL.Text; break;
					case "txUserName":	Data.User =		_CTRL.Text; break;
					case "txExtraInfo": Data.Extra =	_CTRL.Text; break;
					default: break;
				}
			}
		}

		//For Drag & Drop Events
		private void AccountControl_MouseDown(object sender, MouseEventArgs e)
		{
			Control control = sender as Control;
			if (e.Button == MouseButtons.Left)
			{
				DoDragDrop(control, DragDropEffects.Move);
			}
		}

		#endregion


	}
}