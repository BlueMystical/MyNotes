using ControlTreeView;
using FontAwesome.Sharp;
using MyNotes.Classes;
using MyNotes.Forms;
using MyNotes.PropertyHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyNotes
{
	public partial class MainForm : Form
	{
		#region Declarations

		public MyNoteDocument Document { get; set; }
		public List<Traductor> Translator { get; set; }
		public Traductor CurrentLanguage { get; set; }

		private string Language { get; set; } = "en";
		private bool LanguageChanged = false;
		private string CurrentFile { get; set; }
		private string AppExePath = AppDomain.CurrentDomain.BaseDirectory;
		private List<MyTemplate> Templates = new List<MyTemplate>();
		private dynamic MyPageSettings = null;

		private SearchResults searchResults = null;
		private int PrintableWidth = 0;
		private bool AutoExpandSelected { get; set; } = true;

		#endregion

		#region Constructors

		public MainForm()
		{
			InitializeComponent();
			System.Threading.Thread.CurrentThread.SetApartmentState(System.Threading.ApartmentState.STA);
		}
		public MainForm(string pFilePath)
		{
			InitializeComponent();
			System.Threading.Thread.CurrentThread.SetApartmentState(System.Threading.ApartmentState.STA);

			if (!string.IsNullOrEmpty(pFilePath))
			{
				this.CurrentFile = pFilePath;
			}
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			//Lista de Idiomas a mostrar en las Propiedades del Documento:
			List<string> _languages = new List<string>(new string[] { "en", "es", "fr", "de", "ru" });
			StringListConverter.RegisterValuesForProperty(typeof(DocMetadata), nameof(DocMetadata.Language), _languages);

			LoadSettings();
		}
		private void MainForm_Shown(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(this.CurrentFile))
			{
				Document_Open(this.CurrentFile);
			}
		}
		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			Util.WinReg_WriteKey("Settings", "Lang", this.Language);
		}

		#endregion

		#region Methods

		public void LoadSettings(bool SkipMenus = false)
		{
			/* TRANSLATES THE UI'S MENUS AND BUTTONS TO THE SELECTED LANGUAGE
			 * Also loads their icons */
			try
			{
				this.Language = Util.WinReg_ReadKey("Settings", "Lang").NVL("en");
				this.AutoExpandSelected = Convert.ToBoolean(Util.WinReg_ReadKey("Settings", "AutoExpandSelected").NVL("true"));
				bool UseAwesomeIcons = false;

				//If we are in DEV environment, the Tranlations are build, otherwise they get loaded from a file.
#if DEBUG
				MakeTranslator();
#else
					if (File.Exists(Path.Combine(AppExePath, "Tranlations.json")))
					{
						this.Translator = Util.DeSerialize_FromJSON<List<Traductor>>(Path.Combine(AppExePath, "Tranlations.json"));
					}
					else
					{
						MakeTranslator();
					}
#endif

				if (File.Exists(Path.Combine(AppExePath, "Templates.json")))
				{
					Templates = Util.DeSerialize_FromJSON<List<MyTemplate>>(Path.Combine(AppExePath, "Templates.json"));
					if (Templates != null)
					{
						//Show the Templates in a sub-menu and links to the Click Event:
						//mnuFile_New.DropDownItems.Clear();
						foreach (var template in Templates)
						{
							var _menu = mnuFile_New.DropDownItems.Add(template.Name);
							_menu.Tag = template;
							_menu.Click += (object sender, EventArgs e) =>
							{
								LoadTemplate(template);
							};
						}
					}
				}

				#region Page Settings

				//Retrieve the PageSettings if the User ever set them, otherwise it uses Default values:
				if (File.Exists(Path.Combine(AppExePath, "PageSettings.json")))
				{
					MyPageSettings = Util.DeSerialize_FromJSON<dynamic>(Path.Combine(AppExePath, "PageSettings.json"));
				}
				else
				{
					this.MyPageSettings = new
					{
						PaperSize = printDocument1.DefaultPageSettings.PaperSize,
						Margins = printDocument1.DefaultPageSettings.Margins,
						Landscape = printDocument1.DefaultPageSettings.Landscape
					};
				}
				int[] Margins = ((string)MyPageSettings.Margins).Split(',').Select(n => Convert.ToInt32(n)).ToArray(); //<- in hundredths of an inch: left, right, top, bottom
				int PaperWidth = MyPageSettings.PaperSize.Width; //<- hin (hundreds of an inch)
				PrintableWidth = PaperWidth; // (int)(PaperWidth - Margins[0] - Margins[1]);

				printDocument1.DefaultPageSettings.Landscape = MyPageSettings.Landscape;
				printDocument1.DefaultPageSettings.PaperSize = MyPageSettings.PaperSize as System.Drawing.Printing.PaperSize;
				printDocument1.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(
					Margins[0], Margins[1], Margins[2], Margins[3]); //<- left, right, top, bottom

				/* Setting the RTB Ruler:   */
				textRuler1.PrintableWidth = PrintableWidth;
				textRuler1.Units = TextRulerControl.TextRuler.UnitType.Centimeters;
				textRuler1.ScrollingOffset = textRuler1.UnitsToPixels(1 / 4); // 'scroll past first quarter-inch
				textRuler1.FirstLineIndent = richTextBoxEx1.SelectionIndent;
				textRuler1.HangingIndent = richTextBoxEx1.SelectionHangingIndent;
				textRuler1.RightIndent = richTextBoxEx1.SelectionRightIndent;
				textRuler1.TabPositions = richTextBoxEx1.SelectionTabs;
				textRuler1.ZoomFactor = richTextBoxEx1.ZoomFactor;
				textRuler1.NoMargins = false;
				//textRuler1.RightMargin = 10;
				//textRuler1.LeftMargin = 10;

				using (var e = this.CreateGraphics())
				{
					textRuler1.LeftMargin = (int)(Margins[1] * e.DpiX / 100.0f);
					textRuler1.RightMargin = (int)(Margins[1] * e.DpiX / 100.0f);

				}

				richTextBoxEx1.SelectionIndent = textRuler1.FirstLineIndent;
				richTextBoxEx1.SelectionHangingIndent = textRuler1.HangingIndent;
				richTextBoxEx1.SelectionRightIndent = textRuler1.RightIndent;
				richTextBoxEx1.SelectionTabs = textRuler1.TabPositions;

				#endregion

				if (!SkipMenus)
				{
					//Show the Languages in a sub-menu and links to the Click Event:
					mnuLanguage.DropDownItems.Clear();
					foreach (var lang in Translator)
					{
						var _menu = mnuLanguage.DropDownItems.Add(lang.Language) as ToolStripMenuItem;
						_menu.Checked = lang.LangCode == this.Language;
						_menu.Click += mnuLanguages_Click;
						_menu.Tag = lang.LangCode;
					}
				}

				//Loads the translations for the Selected Language:
				CurrentLanguage = Translator.Find(lang => lang.LangCode == Language);

				/*-------  Set icons and Translations for Menus and Buttons  ------------*/
				var OpenIcon = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("open_16x16") : IconChar.FolderOpen.ToBitmap(16, 16, Color.Black);
				string translatedText = CurrentLanguage.GetTranslation("OpenFile");
				toolOpenFile.Image = OpenIcon; toolOpenFile.Text = translatedText; toolOpenFile.ToolTipText = translatedText;
				mnuFile_Open.Image = OpenIcon; mnuFile_Open.Text = "&" + translatedText;

				translatedText = CurrentLanguage.GetTranslation("SaveFile");
				var SaveIcon = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("save_16x16") : IconChar.FloppyDisk.ToBitmap(16, 16, Color.Black);
				toolSave.Image = SaveIcon; toolSave.Text = translatedText; toolSave.ToolTipText = translatedText;
				mnuFile_Save.Image = SaveIcon; mnuFile_Save.Text = "&" + translatedText;

				translatedText = CurrentLanguage.GetTranslation("SaveAs");
				mnuFile_SaveAs.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("saveall_16x16") : IconChar.FileExport.ToBitmap(16, 16, Color.Black);
				mnuFile_SaveAs.Text = translatedText;

				translatedText = CurrentLanguage.GetTranslation("NewFile");
				var NewIcon = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("additem_16x16") : IconChar.File.ToBitmap(16, 16, Color.Black);
				toolNew.Image = NewIcon; toolNew.Text = translatedText; toolNew.ToolTipText = translatedText;
				mnuFile_New.Image = NewIcon; mnuFile_New.Text = "&" + translatedText;
				mnuFile_New_Templates.Text = CurrentLanguage.GetTranslation("NewFileTemplates");

				translatedText = CurrentLanguage.GetTranslation("Print");
				var PrintIcon = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("print_16x16") : IconChar.Print.ToBitmap(16, 16, Color.Black);
				toolPrint.Image = PrintIcon; toolPrint.Text = translatedText; toolPrint.ToolTipText = translatedText;
				mnuFile_Print.Image = PrintIcon; mnuFile_Print.Text = "&" + translatedText;

				mnuFile_Preview.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("preview_16x16") : IconChar.Eye.ToBitmap(16, 16, Color.Black);
				mnuFile_Preview.Text = CurrentLanguage.GetTranslation("PrintPreview");

				mnuPageSetup.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("customizemergefield_16x16") : IconChar.FileLines.ToBitmap(16, 16, Color.Black);
				mnuPageSetup.Text = CurrentLanguage.GetTranslation("PageSetup");

				translatedText = CurrentLanguage.GetTranslation("Exit");
				mnuFile_Exit.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("forward_16x16") : IconChar.ArrowRightFromBracket.ToBitmap(16, 16, Color.Black);
				mnuFile_Exit.Text = translatedText;

				mnuFile.Text = CurrentLanguage.GetTranslation("File");
				mnuSettings.Text = CurrentLanguage.GetTranslation("Settings");
				mnuSettings_About.Text = CurrentLanguage.GetTranslation("About"); mnuSettings_About.Image = (Bitmap)Properties.Resources.ResourceManager.GetObject("show_16x16");
				mnuLanguage.Text = CurrentLanguage.GetTranslation("Language"); mnuLanguage.Image = (Bitmap)Properties.Resources.ResourceManager.GetObject("highlight_16x16");

				toolCopy.Text = CurrentLanguage.GetTranslation("Copy"); toolCopy.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("copy_16x16") : IconChar.Copy.ToBitmap(16, 16, Color.Black);                                                                                       //
				toolCut.Text = CurrentLanguage.GetTranslation("Cut"); toolCut.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("cut_16x16") : IconChar.Cut.ToBitmap(16, 16, Color.Black);
				toolPaste.Text = CurrentLanguage.GetTranslation("Paste");toolPaste.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("paste_16x16") : IconChar.Paste.ToBitmap(16, 16, Color.Black);
				toolUndo.Text = CurrentLanguage.GetTranslation("Undo"); toolUndo.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("undo_16x16") : IconChar.Undo.ToBitmap(16, 16, Color.Black);
				toolRedo.Text = CurrentLanguage.GetTranslation("Redo"); toolRedo.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("redo_16x16") : IconChar.Redo.ToBitmap(16, 16, Color.Black);

				toolFont.Text = CurrentLanguage.GetTranslation("Font"); toolFont.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("font_16x16") : IconChar.Font.ToBitmap(16, 16, Color.Black);
				toolFontcolor.Text = CurrentLanguage.GetTranslation("ForeColor"); toolFontcolor.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("fontcolor_16x16") : IconChar.Palette.ToBitmap(16, 16, Color.Black);
				toolBackColor.Text = CurrentLanguage.GetTranslation("BackColor"); toolBackColor.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("changefontstyle_16x16") : IconChar.Fill.ToBitmap(16, 16, Color.Black);

				toolBold.Text = CurrentLanguage.GetTranslation("Bold"); toolBold.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("bold_16x16") : IconChar.Bold.ToBitmap(24, 24, Color.Black);
				toolCursiva.Text = CurrentLanguage.GetTranslation("Italic"); toolCursiva.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("Italic_16x16") : IconChar.Italic.ToBitmap(24, 24, Color.Black);
				toolSubrayado.Text = CurrentLanguage.GetTranslation("Underline"); toolSubrayado.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("underline_16x16") : IconChar.Underline.ToBitmap(24, 24, Color.Black);
				toolTachado.Text = CurrentLanguage.GetTranslation("Strikeout"); toolTachado.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("strikeout_16x16") : IconChar.Strikethrough.ToBitmap(24, 24, Color.Black);

				toolTextAlign.Text = CurrentLanguage.GetTranslation("TextAlign"); toolTextAlign.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("alignleft_16x16") : IconChar.AlignLeft.ToBitmap(24, 24, Color.Black);
				izquierdaToolStripMenuItem.Text = CurrentLanguage.GetTranslation("Left"); izquierdaToolStripMenuItem.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("alignleft_16x16") : IconChar.AlignLeft.ToBitmap(24, 24, Color.Black);
				centroToolStripMenuItem.Text = CurrentLanguage.GetTranslation("Center"); centroToolStripMenuItem.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("aligncenter_16x16") : IconChar.AlignCenter.ToBitmap(24, 24, Color.Black);
				derechaToolStripMenuItem.Text = CurrentLanguage.GetTranslation("Right"); derechaToolStripMenuItem.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("alignright_16x16") : IconChar.AlignRight.ToBitmap(24, 24, Color.Black);
				justificadoToolStripMenuItem.Text = CurrentLanguage.GetTranslation("Justify"); justificadoToolStripMenuItem.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("alignjustify_16x16") : IconChar.AlignJustify.ToBitmap(24, 24, Color.Black);

				toolBullets.Text = CurrentLanguage.GetTranslation("Vignettes"); toolBullets.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("listbullets_16x16") : IconChar.ListUl.ToBitmap(16, 16, Color.Black);
				toolWordWrap.Text = CurrentLanguage.GetTranslation("WordWrap"); toolWordWrap.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("indentincrease_16x16") : IconChar.Indent.ToBitmap(16, 16, Color.Black);

				addRootItemToolStripMenuItem.Text = CurrentLanguage.GetTranslation("AddRootDoc");
				addSubItemToolStripMenuItem.Text = CurrentLanguage.GetTranslation("AddSubDoc");
				deleteItemToolStripMenuItem.Text = CurrentLanguage.GetTranslation("DeleteDoc");

				mnuFile_DocProps.Text = CurrentLanguage.GetTranslation("DocProps"); mnuFile_DocProps.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("edit_16x16") : IconChar.IdCard.ToBitmap(16, 16, Color.Black);
				toolInsertImage.Text = CurrentLanguage.GetTranslation("InsertImage"); toolInsertImage.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("insertimage_16x16") : IconChar.Image.ToBitmap(18, 18, Color.Black);
				toolInsertTable.Text = CurrentLanguage.GetTranslation("InsertTable"); toolInsertTable.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("all_borders_16x16") : IconChar.Table.ToBitmap(16, 16, Color.Black);

				toolInsertLink.Text = CurrentLanguage.GetTranslation("InsertLink"); toolInsertLink.Image = IconChar.Link.ToBitmap(16, 16, Color.Black);

				toolLabelFind.Text = CurrentLanguage.GetTranslation("Find");
				toolFind.Text = CurrentLanguage.GetTranslation("Find"); toolFind.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("find_16x16") : IconChar.Search.ToBitmap(16, 16, Color.Black);
				FindWholeWord.Text = CurrentLanguage.GetTranslation("WholeWord");
				FindMatchCase.Text = CurrentLanguage.GetTranslation("MatchCase");
				FindAlldocs.Text = CurrentLanguage.GetTranslation("AllDocuments");

				toolReplace.Text = CurrentLanguage.GetTranslation("FindReplace"); toolReplace.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("replace_16x16") : IconChar.SearchPlus.ToBitmap(16, 16, Color.Black);
				
				cmdTreeCommands.Text = CurrentLanguage.GetTranslation("Tree"); cmdTreeCommands.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("show_16x16") : IconChar.Eye.ToBitmap(16, 16, Color.Black);
				mnuAutoExpandSelected.Checked = this.AutoExpandSelected;

				mnuFile_OpenGdrive.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("logo_drive_2020q4_color_16x16") : IconChar.GoogleDrive.ToBitmap(16, 16, Color.Black);
				mnuFile_SaveGDrive.Image = !UseAwesomeIcons ? (Bitmap)Properties.Resources.ResourceManager.GetObject("logo_drive_2020q4_color_16x16") : IconChar.GoogleDrive.ToBitmap(16, 16, Color.Black);
				mnuFile_OpenGdrive.Text = CurrentLanguage.GetTranslation("GDrive_Open");
				mnuFile_SaveGDrive.Text = CurrentLanguage.GetTranslation("GDrive_Save");

				EnableContextMenu(this.richTextBoxEx1);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		private void EnableContextMenu(RichTextBoxEx rtb)
		{
			if (rtb.ContextMenuStrip == null)
			{
				// Create a ContextMenuStrip without icons
				ContextMenuStrip cms = new ContextMenuStrip();
				//cms.ShowImageMargin = false;

				// 1. Add the Undo option
				ToolStripMenuItem tsmiUndo = new ToolStripMenuItem(CurrentLanguage.GetTranslation("Undo"));
				tsmiUndo.Image = IconChar.Undo.ToBitmap(16, 16, Color.Black);
				tsmiUndo.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
				tsmiUndo.Click += (sender, e) => rtb.Undo();
				cms.Items.Add(tsmiUndo);

				// 2. Add the Redo option
				ToolStripMenuItem tsmiRedo = new ToolStripMenuItem(CurrentLanguage.GetTranslation("Redo"), IconChar.Redo.ToBitmap(16, 16, Color.Black));
				tsmiRedo.Click += (sender, e) => rtb.Redo();
				cms.Items.Add(tsmiRedo);

				// Add a Separator
				cms.Items.Add(new ToolStripSeparator());

				#region Clipboard Menus

				// 3. Add the Cut option (cuts the selected text inside the richtextbox)
				ToolStripMenuItem tsmiCut = new ToolStripMenuItem(CurrentLanguage.GetTranslation("Cut"), IconChar.Cut.ToBitmap(16, 16, Color.Black));
				tsmiCut.Click += (sender, e) => rtb.Cut();
				cms.Items.Add(tsmiCut);

				// 4. Add the Copy option (copies the selected text inside the richtextbox)
				ToolStripMenuItem tsmiCopy = new ToolStripMenuItem(CurrentLanguage.GetTranslation("Copy"), IconChar.Copy.ToBitmap(16, 16, Color.Black));
				tsmiCopy.Click += (sender, e) => rtb.Copy();
				cms.Items.Add(tsmiCopy);

				// 5. Add the Paste option (adds the text from the clipboard into the richtextbox)
				ToolStripMenuItem tsmiPaste = new ToolStripMenuItem(CurrentLanguage.GetTranslation("Paste"), IconChar.Paste.ToBitmap(16, 16, Color.Black));
				tsmiPaste.Click += (sender, e) => rtb.Paste();
				cms.Items.Add(tsmiPaste);

				// 6. Add the Delete Option (remove the selected text in the richtextbox)
				ToolStripMenuItem tsmiDelete = new ToolStripMenuItem(CurrentLanguage.GetTranslation("DeleteDoc"), IconChar.Remove.ToBitmap(16, 16, Color.Black));
				tsmiDelete.Click += (sender, e) => rtb.SelectedText = "";
				cms.Items.Add(tsmiDelete);

				#endregion

				// Add a Separator
				cms.Items.Add(new ToolStripSeparator());

				#region Paragraph Menu

				ToolStripMenuItem tsmiParrafo = new ToolStripMenuItem(CurrentLanguage.GetTranslation("Paragraph"), IconChar.AlignLeft.ToBitmap(16, 16, Color.Black));
				ToolStripMenuItem Interlineado = new ToolStripMenuItem(CurrentLanguage.GetTranslation("LineSpacing"), IconChar.Indent.ToBitmap(16, 16, Color.Black));
				ToolStripMenuItem Interlineado_10 = new ToolStripMenuItem(CurrentLanguage.GetTranslation("1.0"), IconChar.Indent.ToBitmap(16, 16, Color.Black));
				ToolStripMenuItem Interlineado_15 = new ToolStripMenuItem(CurrentLanguage.GetTranslation("1.5"), IconChar.Indent.ToBitmap(16, 16, Color.Black));
				ToolStripMenuItem Interlineado_20 = new ToolStripMenuItem(CurrentLanguage.GetTranslation("2.0"), IconChar.Indent.ToBitmap(16, 16, Color.Black));

				ToolStripMenuItem Alineacion = new ToolStripMenuItem(CurrentLanguage.GetTranslation("TextAlign"), IconChar.AlignLeft.ToBitmap(16, 16, Color.Black));
				ToolStripMenuItem ALeft = new ToolStripMenuItem(CurrentLanguage.GetTranslation("Left"), IconChar.AlignLeft.ToBitmap(16, 16, Color.Black));
				ToolStripMenuItem ACenter = new ToolStripMenuItem(CurrentLanguage.GetTranslation("Center"), IconChar.AlignCenter.ToBitmap(16, 16, Color.Black));
				ToolStripMenuItem ARight = new ToolStripMenuItem(CurrentLanguage.GetTranslation("Right"), IconChar.AlignRight.ToBitmap(16, 16, Color.Black));
				ToolStripMenuItem AJustify = new ToolStripMenuItem(CurrentLanguage.GetTranslation("Justify"), IconChar.AlignJustify.ToBitmap(16, 16, Color.Black));

				tsmiParrafo.DropDownItems.Add(Interlineado);
				tsmiParrafo.DropDownItems.Add(Alineacion);

				Interlineado.DropDownItems.Add(Interlineado_10);
				Interlineado.DropDownItems.Add(Interlineado_15);
				Interlineado.DropDownItems.Add(Interlineado_20);

				Alineacion.DropDownItems.Add(ALeft);
				Alineacion.DropDownItems.Add(ACenter);
				Alineacion.DropDownItems.Add(ARight);
				Alineacion.DropDownItems.Add(AJustify);

				cms.Items.Add(tsmiParrafo);

				Interlineado_10.Click += (sender, e) =>
				{
					rtb.SetLineFormat(0);
					Interlineado_10.Checked = true;
					Interlineado_15.Checked = false;
					Interlineado_20.Checked = false;
				};
				Interlineado_15.Click += (sender, e) =>
				{
					rtb.SetLineFormat(1);
					Interlineado_10.Checked = false;
					Interlineado_15.Checked = true;
					Interlineado_20.Checked = false;
				};
				Interlineado_20.Click += (sender, e) =>
				{
					rtb.SetLineFormat(2);
					Interlineado_10.Checked = false;
					Interlineado_15.Checked = false;
					Interlineado_20.Checked = true;
				};

				ALeft.Click += (sender, e) =>
				{
					rtb.SelectionAlignment = RichTextBoxEx.TextAlignment.Left;
				};
				ACenter.Click += (sender, e) =>
				{
					rtb.SelectionAlignment = RichTextBoxEx.TextAlignment.Center;
				};
				ARight.Click += (sender, e) =>
				{
					rtb.SelectionAlignment = RichTextBoxEx.TextAlignment.Right;
				};
				AJustify.Click += (sender, e) =>
				{
					rtb.SelectionAlignment = RichTextBoxEx.TextAlignment.Justify;
				};

				#endregion

				// Add a Separator
				cms.Items.Add(new ToolStripSeparator());

				// 7. Add the Select All Option (selects all the text inside the richtextbox)
				ToolStripMenuItem tsmiSelectAll = new ToolStripMenuItem(CurrentLanguage.GetTranslation("SelectAll"), IconChar.AlignJustify.ToBitmap(16, 16, Color.Black));
				tsmiSelectAll.Click += (sender, e) => rtb.SelectAll();
				cms.Items.Add(tsmiSelectAll);

				// Raw Text Editor:
				ToolStripMenuItem tsmiShowRawText = new ToolStripMenuItem(CurrentLanguage.GetTranslation("ShowRawText"), IconChar.FileText.ToBitmap(16, 16, Color.Black));
				tsmiShowRawText.Click += (sender, e) =>
				{
					using (RawTextEditorForm RawForm = new RawTextEditorForm())
					{
						RawForm.RawText = richTextBoxEx1.SelectedRtf;
						if (RawForm.ShowDialog() == DialogResult.OK)
						{
							richTextBoxEx1.SelectedRtf = RawForm.RawText;
						}
					}
				};
				cms.Items.Add(tsmiShowRawText);

				#region Before Opening the Menu

				// When opening the menu, check if the condition is fulfilled 
				// in order to enable the action
				cms.Opening += (sender, e) =>
				{
					tsmiUndo.Enabled = !rtb.ReadOnly && rtb.CanUndo; tsmiUndo.Text = CurrentLanguage.GetTranslation("Undo");
					tsmiRedo.Enabled = !rtb.ReadOnly && rtb.CanRedo;
					tsmiCut.Enabled = !rtb.ReadOnly && rtb.SelectionLength > 0;
					tsmiCopy.Enabled = rtb.SelectionLength > 0;
					tsmiPaste.Enabled = !rtb.ReadOnly && Clipboard.ContainsText();
					tsmiDelete.Enabled = !rtb.ReadOnly && rtb.SelectionLength > 0;
					tsmiSelectAll.Enabled = rtb.TextLength > 0 && rtb.SelectionLength < rtb.TextLength;

					if (this.LanguageChanged) //<- If Language was changed, apply the new Translations
					{
						tsmiUndo.Text = this.CurrentLanguage.GetTranslation("Undo");
						tsmiRedo.Text = CurrentLanguage.GetTranslation("Redo");
						tsmiCut.Text = CurrentLanguage.GetTranslation("Cut");
						tsmiCopy.Text = CurrentLanguage.GetTranslation("Copy");
						tsmiPaste.Text = CurrentLanguage.GetTranslation("Paste");
						tsmiDelete.Text = CurrentLanguage.GetTranslation("DeleteDoc");

						tsmiParrafo.Text = CurrentLanguage.GetTranslation("Paragraph");
						Interlineado.Text = CurrentLanguage.GetTranslation("LineSpacing");
						Alineacion.Text = CurrentLanguage.GetTranslation("TextAlign");
						ALeft.Text = CurrentLanguage.GetTranslation("Left");
						ACenter.Text = CurrentLanguage.GetTranslation("Center");
						ARight.Text = CurrentLanguage.GetTranslation("Right");
						AJustify.Text = CurrentLanguage.GetTranslation("Justify");

						tsmiSelectAll.Text = CurrentLanguage.GetTranslation("SelectAll");
						this.LanguageChanged = false;
					}

					//Checks the Context menu for LineSpacing:
					var Formato = rtb.GetSelectionFormat();
					if (Formato.dyLineSpacing == 0)
					{
						Interlineado_10.Checked = true;
						Interlineado_15.Checked = false;
						Interlineado_20.Checked = false;
					}
					if (Formato.dyLineSpacing == 1)
					{
						Interlineado_10.Checked = false;
						Interlineado_15.Checked = true;
						Interlineado_20.Checked = false;
					}
					if (Formato.dyLineSpacing == 2)
					{
						Interlineado_10.Checked = false;
						Interlineado_15.Checked = false;
						Interlineado_20.Checked = true;
					}

					//Checks the Context menu for Text Alignment:
					if (Formato.wAlignment == (short)RichTextBoxEx.TextAlignment.Center)
					{
						ACenter.Checked = true;
						ALeft.Checked = false;
						ARight.Checked = false;
						AJustify.Checked = false;
					}
					if (Formato.wAlignment == (short)RichTextBoxEx.TextAlignment.Left)
					{
						ACenter.Checked = false;
						ALeft.Checked = true;
						ARight.Checked = false;
						AJustify.Checked = false;
					}
					if (Formato.wAlignment == (short)RichTextBoxEx.TextAlignment.Right)
					{
						ACenter.Checked = false;
						ALeft.Checked = false;
						ARight.Checked = true;
						AJustify.Checked = false;
					}
					if (Formato.wAlignment == (short)RichTextBoxEx.TextAlignment.Justify)
					{
						ACenter.Checked = false;
						ALeft.Checked = false;
						ARight.Checked = false;
						AJustify.Checked = true;
					}
				};

				#endregion

				rtb.ContextMenuStrip = cms;
			}
		}

		public void LoadTemplate(MyTemplate CurrentTemplate)
		{
			/* CREATES A NEW DOCUMENT FROM A TEMPLATE  */
			try
			{
				if (CurrentTemplate.Data != null && CurrentTemplate.Data.Rows.Count > 0)
				{
					string Chapter = CurrentLanguage.GetTranslation("Chapter");
					string Section = CurrentLanguage.GetTranslation("Section");
					string Entry = CurrentLanguage.GetTranslation("Entry");
					string LorenIpsum = CurrentLanguage.GetTranslation("LorenIpsum");

					this.Document = new MyNoteDocument("[Untitled]", Util.GetUserName());
					this.Document.Metadata.Language = this.Language;
					this.CurrentFile = string.Empty;

					string F1 = string.Empty; DocContent C1 = new DocContent();
					string F2 = string.Empty; DocContent C2 = new DocContent();
					string F3 = string.Empty; DocContent C3 = new DocContent();
					string F4 = string.Empty; DocContent C4 = new DocContent();

					//Util.ReplaceAndTranslate("[Chapter] 1, [Section] B, [Entry] 4:", CurrentLanguage);

					foreach (DataRow DR in CurrentTemplate.Data.Rows)
					{
						if (DR["first_level"].ToString() != F1)
						{
							string val = DR["first_level"].ToString();
							if (!string.IsNullOrEmpty(val) && val != F1)
							{
								F1 = DR["first_level"].ToString();
								C1 = Document.Content.AddChild(Util.ReplaceAndTranslate(F1, CurrentLanguage));
								C1.Content = Util.CreateSimpleRTF(Util.ReplaceAndTranslate(DR["content"].ToString(), CurrentLanguage));
							}
						}
						if (DR["second_level"].ToString() != F2)
						{
							string val = DR["second_level"].ToString();
							if (!string.IsNullOrEmpty(val) && val != F2)
							{
								F2 = DR["second_level"].ToString();
								C2 = C1.AddChild(Util.ReplaceAndTranslate(F2, CurrentLanguage));
								C2.Content = Util.CreateSimpleRTF(Util.ReplaceAndTranslate(DR["content"].ToString(), CurrentLanguage));
							}
						}
						if (DR["third_level"].ToString() != F3)
						{
							string val = DR["third_level"].ToString();
							if (!string.IsNullOrEmpty(val) && val != F3)
							{
								F3 = DR["third_level"].ToString();
								C3 = C2.AddChild(Util.ReplaceAndTranslate(F3, CurrentLanguage));
								C3.Content = Util.CreateSimpleRTF(Util.ReplaceAndTranslate(DR["content"].ToString(), CurrentLanguage));
							}
						}
						if (DR["fourth_level"].ToString() != F4)
						{
							string val = DR["fourth_level"].ToString();
							if (!string.IsNullOrEmpty(val) && val != F4)
							{
								F4 = DR["fourth_level"].ToString();
								C4 = C3.AddChild(Util.ReplaceAndTranslate(F4, CurrentLanguage));
								C4.Content = Util.CreateSimpleRTF(Util.ReplaceAndTranslate(DR["content"].ToString(), CurrentLanguage));
							}
						}
					}

					Document_Show();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public void MakeTranslator()
		{
			/*  CREATES A DICTIONARY WITH WORDS TRANSLATES TO 5 LANGUAGES  */
			try
			{
				Translator = new List<Traductor>();
				var EN = Translator.AddLanguage("en", "English");
				var ES = Translator.AddLanguage("es", "Español");
				var FR = Translator.AddLanguage("fr", "Français");
				var DE = Translator.AddLanguage("de", "Deutsch");
				var RU = Translator.AddLanguage("ru", "русский");

				EN.Translations["OpenFile"] = "Open File..";
				ES.Translations["OpenFile"] = "Abrir Archivo..";
				FR.Translations["OpenFile"] = "Fichier ouvert..";
				DE.Translations["OpenFile"] = "Datei öffnen..";
				RU.Translations["OpenFile"] = "Открыть файл..";

				EN.Translations["SaveFile"] = "Save File..";
				ES.Translations["SaveFile"] = "Guardar..";
				FR.Translations["SaveFile"] = "Enregistrer le fichier..";
				DE.Translations["SaveFile"] = "Datei speichern..";
				RU.Translations["SaveFile"] = "Сохранить файл..";

				EN.Translations["SaveAs"] = "Save As..";
				ES.Translations["SaveAs"] = "Guardar Como..";
				FR.Translations["SaveAs"] = "Enregistrer sous..";
				DE.Translations["SaveAs"] = "Speichern als..";
				RU.Translations["SaveAs"] = "Сохранить как..";

				EN.Translations["Saved"] = "Saved.";
				ES.Translations["Saved"] = "Guardado.";
				FR.Translations["Saved"] = "Enregistré.";
				DE.Translations["Saved"] = "Gerettet.";
				RU.Translations["Saved"] = "Сохранено.";

				EN.Translations["NewFile"] = "New Document..";
				ES.Translations["NewFile"] = "Nuevo Documento..";
				FR.Translations["NewFile"] = "Nouveau document..";
				DE.Translations["NewFile"] = "Neues Dokument..";
				RU.Translations["NewFile"] = "Новый документ..";

				EN.Translations["NewFileEmpty"] = "Empty Document..";
				ES.Translations["NewFileEmpty"] = "Documento vacío..";
				FR.Translations["NewFileEmpty"] = "Document vide..";
				DE.Translations["NewFileEmpty"] = "Leeres Dokument.";
				RU.Translations["NewFileEmpty"] = "Пустой документ..";

				EN.Translations["NewFileBook"] = "Book Example..";
				ES.Translations["NewFileBook"] = "Ejemplo de libro..";
				FR.Translations["NewFileBook"] = "Exemple de livre..";
				DE.Translations["NewFileBook"] = "Buchbeispiel..";
				RU.Translations["NewFileBook"] = "Пример книги..";

				EN.Translations["NewFileDiary"] = "Diary Example..";
				ES.Translations["NewFileDiary"] = "Ejemplo de diario..";
				FR.Translations["NewFileDiary"] = "Exemple de journal..";
				DE.Translations["NewFileDiary"] = "Beispiel für ein Tagebuch.";
				RU.Translations["NewFileDiary"] = "Пример дневника..";

				EN.Translations["NewFileTemplates"] = "Manage Templates..";
				ES.Translations["NewFileTemplates"] = "Administrar plantillas..";
				FR.Translations["NewFileTemplates"] = "Gérer les modèles..";
				DE.Translations["NewFileTemplates"] = "Vorlagen verwalten..";
				RU.Translations["NewFileTemplates"] = "Управление шаблонами..";

				EN.Translations["Print"] = "Print..";
				ES.Translations["Print"] = "Imprimir..";
				FR.Translations["Print"] = "Imprimer..";
				DE.Translations["Print"] = "Drucken..";
				RU.Translations["Print"] = "Распечатать..";

				EN.Translations["PrintPreview"] = "Print Preview..";
				ES.Translations["PrintPreview"] = "Vista previa de impresión..";
				FR.Translations["PrintPreview"] = "Aperçu avant impression..";
				DE.Translations["PrintPreview"] = "Druckvorschau..";
				RU.Translations["PrintPreview"] = "Предварительный просмотр печати..";

				EN.Translations["Exit"] = "Exit..";
				ES.Translations["Exit"] = "Salir";
				FR.Translations["Exit"] = "Sortie..";
				DE.Translations["Exit"] = "Ausfahrt..";
				RU.Translations["Exit"] = "Выход..";

				EN.Translations["File"] = "File";
				ES.Translations["File"] = "Archivo";
				FR.Translations["File"] = "Déposer";
				DE.Translations["File"] = "Datei";
				RU.Translations["File"] = "Файл";

				EN.Translations["Settings"] = "Settings";
				ES.Translations["Settings"] = "Configuración";
				FR.Translations["Settings"] = "Paramètres";
				DE.Translations["Settings"] = "Einstellungen";
				RU.Translations["Settings"] = "Настройки";

				EN.Translations["Language"] = "Language";
				ES.Translations["Language"] = "Idioma";
				FR.Translations["Language"] = "Langue";
				DE.Translations["Language"] = "Sprache";
				RU.Translations["Language"] = "Язык";

				EN.Translations["About"] = "About..";
				ES.Translations["About"] = "Acerca de..";
				FR.Translations["About"] = "À propos..";
				DE.Translations["About"] = "Um..";
				RU.Translations["About"] = "О..";

				EN.Translations["AddRootDoc"] = "Add Item..";
				ES.Translations["AddRootDoc"] = "Añadir Elemento..";
				FR.Translations["AddRootDoc"] = "Ajouter un item..";
				DE.Translations["AddRootDoc"] = "Artikel hinzufügen..";
				RU.Translations["AddRootDoc"] = "Добавить элемент..";

				EN.Translations["AddSubDoc"] = "Add Sub Item..";
				ES.Translations["AddSubDoc"] = "Nueva Sub elemento..";
				FR.Translations["AddSubDoc"] = "Ajouter un sous-élément..";
				DE.Translations["AddSubDoc"] = "Unterelement hinzufügen.";
				RU.Translations["AddSubDoc"] = "Добавить подпункт..";

				EN.Translations["DeleteDoc"] = "Delete..";
				ES.Translations["DeleteDoc"] = "Eliminar..";
				FR.Translations["DeleteDoc"] = "Supprimer..";
				DE.Translations["DeleteDoc"] = "Löschen..";
				RU.Translations["DeleteDoc"] = "Удалить..";

				EN.Translations["Confirm"] = "Confirm?";
				ES.Translations["Confirm"] = "¿Confirmar?";
				FR.Translations["Confirm"] = "Confirmer?";
				DE.Translations["Confirm"] = "Bestätigen?";
				RU.Translations["Confirm"] = "Подтверждать?";

				EN.Translations["ConfirmDelete"] = "'{0}' will be deleted!";
				ES.Translations["ConfirmDelete"] = "'{0}' será eliminado!";
				FR.Translations["ConfirmDelete"] = "'{0}' sera supprimé !";
				DE.Translations["ConfirmDelete"] = "'{0}' wird gelöscht!";
				RU.Translations["ConfirmDelete"] = "'{0}' будет удален!";

				EN.Translations["Chapter"] = "Chapter";
				ES.Translations["Chapter"] = "Capítulo";
				FR.Translations["Chapter"] = "Chapitre";
				DE.Translations["Chapter"] = "Kapitel";
				RU.Translations["Chapter"] = "Глава";

				EN.Translations["Section"] = "Section";
				ES.Translations["Section"] = "Sección";
				FR.Translations["Section"] = "Section";
				DE.Translations["Section"] = "Abschnitt";
				RU.Translations["Section"] = "Раздел";

				EN.Translations["Entry"] = "Entry";
				ES.Translations["Entry"] = "Entrada";
				FR.Translations["Entry"] = "Entrée";
				DE.Translations["Entry"] = "Eintrag";
				RU.Translations["Entry"] = "Вход";

				EN.Translations["NewEntry"] = "New Entry";
				ES.Translations["NewEntry"] = "Nueva entrada";
				FR.Translations["NewEntry"] = "Nouvelle entrée";
				DE.Translations["NewEntry"] = "Neuer Eintrag";
				RU.Translations["NewEntry"] = "Новая запись";

				EN.Translations["LorenIpsum"] = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s.";
				ES.Translations["LorenIpsum"] = "Lorem Ipsum es simplemente un texto de relleno de la industria de la impresión y la tipografía. Lorem Ipsum ha sido el texto de relleno estándar de la industria desde el siglo XVI.";
				FR.Translations["LorenIpsum"] = "Lorem Ipsum est tout simplement un faux texte de l’industrie de l’imprimerie et de la composition. Le Lorem Ipsum est le texte factice standard de l'industrie depuis les années 1500.";
				DE.Translations["LorenIpsum"] = "Lorem Ipsum ist lediglich ein Blindtext der Druck- und Satzindustrie. Lorem Ipsum ist seit dem 16. Jahrhundert der Standard-Dummy-Text der Branche.";
				RU.Translations["LorenIpsum"] = "Lorem Ipsum — это просто текст-пустышка полиграфической и наборной индустрии. Lorem Ipsum является стандартным текстом-пустышкой в отрасли с 1500-х годов.";
				//-------------------------------------------------
				EN.Translations["Copy"] = "Copy";
				ES.Translations["Copy"] = "Copiar";
				FR.Translations["Copy"] = "Copie";
				DE.Translations["Copy"] = "Kopieren";
				RU.Translations["Copy"] = "Копировать";

				EN.Translations["Cut"] = "Cut";
				ES.Translations["Cut"] = "Cortar";
				FR.Translations["Cut"] = "Couper";
				DE.Translations["Cut"] = "Schneiden";
				RU.Translations["Cut"] = "Резать";

				EN.Translations["Paste"] = "Paste";
				ES.Translations["Paste"] = "Pegar";
				FR.Translations["Paste"] = "Pâte";
				DE.Translations["Paste"] = "Paste";
				RU.Translations["Paste"] = "Вставить";

				EN.Translations["Undo"] = "Undo";
				ES.Translations["Undo"] = "Deshacer";
				FR.Translations["Undo"] = "annuler";
				DE.Translations["Undo"] = "Rückgängig machen";
				RU.Translations["Undo"] = "Отменить";

				EN.Translations["Redo"] = "Redo";
				ES.Translations["Redo"] = "Rehacer";
				FR.Translations["Redo"] = "Refaire";
				DE.Translations["Redo"] = "Wiederholen";
				RU.Translations["Redo"] = "Повторить";

				EN.Translations["SelectAll"] = "Select All";
				ES.Translations["SelectAll"] = "Seleccionar todo";
				FR.Translations["SelectAll"] = "Tout sélectionner";
				DE.Translations["SelectAll"] = "Wählen Sie Alle";
				RU.Translations["SelectAll"] = "Выбрать все";
				//-------------------------------------------------
				EN.Translations["Templates"] = "Templates";
				ES.Translations["Templates"] = "Plantillas";
				FR.Translations["Templates"] = "Modèles";
				DE.Translations["Templates"] = "Vorlagen";
				RU.Translations["Templates"] = "Шаблоны";

				EN.Translations["Definition"] = "Definition";
				ES.Translations["Definition"] = "Definición";
				FR.Translations["Definition"] = "Définition";
				DE.Translations["Definition"] = "Definition";
				RU.Translations["Definition"] = "Определение";

				EN.Translations["Name"] = "Name";
				ES.Translations["Name"] = "Nombre";
				FR.Translations["Name"] = "Nom";
				DE.Translations["Name"] = "Name";
				RU.Translations["Name"] = "Имя";

				EN.Translations["Author"] = "Author";
				ES.Translations["Author"] = "Autor";
				FR.Translations["Author"] = "Auteur";
				DE.Translations["Author"] = "Autor";
				RU.Translations["Author"] = "Автор";

				EN.Translations["Description"] = "Description";
				ES.Translations["Description"] = "Descripción";
				FR.Translations["Description"] = "Description";
				DE.Translations["Description"] = "Beschreibung";
				RU.Translations["Description"] = "Описание";

				EN.Translations["Preview"] = "Preview";
				ES.Translations["Preview"] = "Vista Previa";
				FR.Translations["Preview"] = "Aperçu";
				DE.Translations["Preview"] = "Vorschau";
				RU.Translations["Preview"] = "превью";

				EN.Translations["Content"] = "Content";
				ES.Translations["Content"] = "Contenido";
				FR.Translations["Content"] = "Contenu";
				DE.Translations["Content"] = "Inhalt";
				RU.Translations["Content"] = "Содержание";


				EN.Translations["DeleteRow"] = "You want to delete the current row?";
				ES.Translations["DeleteRow"] = "¿Quieres eliminar la fila actual?";
				FR.Translations["DeleteRow"] = "Vous souhaitez supprimer la ligne actuelle ?";
				DE.Translations["DeleteRow"] = "Sie möchten die aktuelle Zeile löschen?";
				RU.Translations["DeleteRow"] = "Вы хотите удалить текущую строку?";
				//-------------------------------------------------
				EN.Translations["WeekDay"] = "Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,Sunday";
				ES.Translations["WeekDay"] = "Lunes,Martes,Miércoles,Jueves,Viernes,Sábado,Domingo";
				FR.Translations["WeekDay"] = "Lundi,Mardi,Mercredi,Jeudi,Vendredi,Samedi,Dimanche";
				DE.Translations["WeekDay"] = "Montag,Dienstag,Mittwoch,Donnerstag,Freitag,Samstag,Sonntag";
				RU.Translations["WeekDay"] = "Понедельник,вторник,среда,Четверг,Пятница,Суббота,воскресенье";

				EN.Translations["MonthName"] = "January,February,March,April,May,June,July,August,September,October,November,December.";
				ES.Translations["MonthName"] = "Enero,Febrero,Marzo,Abril,Mayo,Junio,Julio,Agosto,Septiembre,Octubre,Noviembre,Diciembre";
				FR.Translations["MonthName"] = "Janvier,Février,Mars,Avril,Mai,Juin,Juillet,Août,Septembre,Octobre,Novembre,Décembre.";
				DE.Translations["MonthName"] = "Januar,Februar,März,April,Mai,Juni,Juli,August,September,Oktober,November,Dezember";
				RU.Translations["MonthName"] = "Январь,февраль,март,апрель,май,июнь,июль,август,сентябрь,октябрь,ноябрь,декабрь";
				//-------------------------------------------------
				EN.Translations["Font"] = "Font";
				ES.Translations["Font"] = "Fuente";
				FR.Translations["Font"] = "Police de caractère";
				DE.Translations["Font"] = "Schriftart";
				RU.Translations["Font"] = "Шрифт";

				EN.Translations["ForeColor"] = "ForeColor";
				ES.Translations["ForeColor"] = "Color primario";
				FR.Translations["ForeColor"] = "Couleur de premier plan";
				DE.Translations["ForeColor"] = "Vordergrundfarbe";
				RU.Translations["ForeColor"] = "передний цвет";

				EN.Translations["BackColor"] = "BackColor";
				ES.Translations["BackColor"] = "Color de fondo";
				FR.Translations["BackColor"] = "Couleur de fond";
				DE.Translations["BackColor"] = "Hintergrundfarbe";
				RU.Translations["BackColor"] = "НазадЦвет";

				EN.Translations["Bold"] = "Bold";
				ES.Translations["Bold"] = "Negrita";
				FR.Translations["Bold"] = "Caractère gras";
				DE.Translations["Bold"] = "Fettgedruckte Schriftart";
				RU.Translations["Bold"] = "Жирный шрифт";

				EN.Translations["Italic"] = "Italics";
				ES.Translations["Italic"] = "Cursiva";
				FR.Translations["Italic"] = "Italique";
				DE.Translations["Italic"] = "Kursivschrift";
				RU.Translations["Italic"] = "курсив";

				EN.Translations["Underline"] = "Underline";
				ES.Translations["Underline"] = "Subrayar";
				FR.Translations["Underline"] = "Souligner";
				DE.Translations["Underline"] = "Unterstreichen";
				RU.Translations["Underline"] = "Подчеркнуть";

				EN.Translations["Strikeout"] = "Strikeout";
				ES.Translations["Strikeout"] = "Tachar";
				FR.Translations["Strikeout"] = "Barré";
				DE.Translations["Strikeout"] = "Durchgestrichen";
				RU.Translations["Strikeout"] = "Вычеркивание";

				EN.Translations["TextAlign"] = "Text Align";
				ES.Translations["TextAlign"] = "Alineación";
				FR.Translations["TextAlign"] = "Alignement";
				DE.Translations["TextAlign"] = "Textausrichtung";
				RU.Translations["TextAlign"] = "выравнивание";

				EN.Translations["Left"] = "Left";
				ES.Translations["Left"] = "Izquierda";
				FR.Translations["Left"] = "Gauche";
				DE.Translations["Left"] = "Links";
				RU.Translations["Left"] = "Левый";

				EN.Translations["Center"] = "Center";
				ES.Translations["Center"] = "Centro";
				FR.Translations["Center"] = "Centre";
				DE.Translations["Center"] = "Center";
				RU.Translations["Center"] = "Центр";

				EN.Translations["Right"] = "Right";
				ES.Translations["Right"] = "Derecha";
				FR.Translations["Right"] = "Droite";
				DE.Translations["Right"] = "Rechts";
				RU.Translations["Right"] = "Верно";

				EN.Translations["Justify"] = "Justify";
				ES.Translations["Justify"] = "Justificar";
				FR.Translations["Justify"] = "Justifier";
				DE.Translations["Justify"] = "Rechtfertigen";
				RU.Translations["Justify"] = "Оправдывать";

				EN.Translations["Vignettes"] = "Vignettes";
				ES.Translations["Vignettes"] = "viñetas";
				FR.Translations["Vignettes"] = "vignette";
				DE.Translations["Vignettes"] = "Vignetten";
				RU.Translations["Vignettes"] = "виньетки";

				EN.Translations["InsertImage"] = "Insert Image..";
				ES.Translations["InsertImage"] = "Insertar imagen..";
				FR.Translations["InsertImage"] = "Insérer une image..";
				DE.Translations["InsertImage"] = "Bild einfügen..";
				RU.Translations["InsertImage"] = "Вставить изображение..";

				EN.Translations["InsertTable"] = "Insert Table..";
				ES.Translations["InsertTable"] = "Insertar Tabla..";
				FR.Translations["InsertTable"] = "Insérer un tableau..";
				DE.Translations["InsertTable"] = "Tabelle einfügen..";
				RU.Translations["InsertTable"] = "Вставить таблицу..";

				EN.Translations["WordWrap"] = "Word Wrap";
				ES.Translations["WordWrap"] = "Ajuste de línea";
				FR.Translations["WordWrap"] = "Retour à la ligne";
				DE.Translations["WordWrap"] = "Zeilenumbruch";
				RU.Translations["WordWrap"] = "Перенос слова";

				EN.Translations["PageSetup"] = "Page Setup";
				ES.Translations["PageSetup"] = "Configuración de página";
				FR.Translations["PageSetup"] = "Mise en page";
				DE.Translations["PageSetup"] = "Seiteneinrichtung";
				RU.Translations["PageSetup"] = "Настройка страницы";

				EN.Translations["Cover"] = "Front page";
				ES.Translations["Cover"] = "Portada";
				FR.Translations["Cover"] = "Page de garde";
				DE.Translations["Cover"] = "Titelseite";
				RU.Translations["Cover"] = "Титульная страница";

				EN.Translations["DocProps"] = "Document Properties";
				ES.Translations["DocProps"] = "Propiedades del Documento";
				FR.Translations["DocProps"] = "Propriétés du document";
				DE.Translations["DocProps"] = "Dokumenteigenschaften";
				RU.Translations["DocProps"] = "Свойства документа";

				EN.Translations["Untitled"] = "Untitled";
				ES.Translations["Untitled"] = "Sin Título";
				FR.Translations["Untitled"] = "Sans titre";
				DE.Translations["Untitled"] = "Ohne Titel";
				RU.Translations["Untitled"] = "Без названия";

				EN.Translations["Find"] = "Find";
				ES.Translations["Find"] = "Buscar";
				FR.Translations["Find"] = "Trouver";
				DE.Translations["Find"] = "Finden";
				RU.Translations["Find"] = "Находить";

				EN.Translations["FindReplace"] = "Find and Replace";
				ES.Translations["FindReplace"] = "Buscar y Reemplazar";
				FR.Translations["FindReplace"] = "Trouver et remplacer";
				DE.Translations["FindReplace"] = "Suchen und Ersetzen";
				RU.Translations["FindReplace"] = "Найти и заменить";

				EN.Translations["MatchCase"] = "Match Uppercase";
				ES.Translations["MatchCase"] = "Coincidir Mayúsculas";
				FR.Translations["MatchCase"] = "Faire correspondre les majuscules";
				DE.Translations["MatchCase"] = "Passen Sie Großbuchstaben an";
				RU.Translations["MatchCase"] = "Соответствие верхнему регистру";

				EN.Translations["WholeWord"] = "Whole Word";
				ES.Translations["WholeWord"] = "Palabra Completa";
				FR.Translations["WholeWord"] = "Mot complet";
				DE.Translations["WholeWord"] = "Vollständiges Wort";
				RU.Translations["WholeWord"] = "Полное слово";

				EN.Translations["AllDocuments"] = "All Documents";
				ES.Translations["AllDocuments"] = "Todos los Documentos";
				FR.Translations["AllDocuments"] = "Tous les documents";
				DE.Translations["AllDocuments"] = "Alle Dokumente";
				RU.Translations["AllDocuments"] = "Все документы";

				EN.Translations["CurrentDocument"] = "Current Document";
				ES.Translations["CurrentDocument"] = "Docuemnto Actual";
				FR.Translations["CurrentDocument"] = "Document actuel";
				DE.Translations["CurrentDocument"] = "Aktuelles Dokument";
				RU.Translations["CurrentDocument"] = "Текущий документ";

				EN.Translations["Paragraph"] = "Paragraph";
				ES.Translations["Paragraph"] = "Párrafo";
				FR.Translations["Paragraph"] = "Paragraphe";
				DE.Translations["Paragraph"] = "Absatz";
				RU.Translations["Paragraph"] = "Параграф";

				EN.Translations["LineSpacing"] = "Line Spacing";
				ES.Translations["LineSpacing"] = "Interlineado";
				FR.Translations["LineSpacing"] = "Interligne";
				DE.Translations["LineSpacing"] = "Zeilenabstand";
				RU.Translations["LineSpacing"] = "Межстрочный интервал";

				EN.Translations["NotFound"] = "Not Found!";
				ES.Translations["NotFound"] = "No se Encontró!";
				FR.Translations["NotFound"] = "Pas trouvé!";
				DE.Translations["NotFound"] = "Nicht gefunden!";
				RU.Translations["NotFound"] = "Не найдено!";

				EN.Translations["SearchResults"] = "{0}/{1} Results, {2}/{3} on this document.";
				ES.Translations["SearchResults"] = "{0}/{1} Resultados, {2}/{3} en este documento.";
				FR.Translations["SearchResults"] = "{0}/{1} Résultats, {2}/{3} sur ce document.";
				DE.Translations["SearchResults"] = "{0}/{1} Ergebnisse, {2}/{3} zu diesem Dokument.";
				RU.Translations["SearchResults"] = "Результаты {0}/{1}, {2}/{3} в этом документе.";

				EN.Translations["GDrive_Open"] = "Open from Google Drive..";
				ES.Translations["GDrive_Open"] = "Abrir desde Google Drive..";
				FR.Translations["GDrive_Open"] = "Ouvrir depuis Google Drive.";
				DE.Translations["GDrive_Open"] = "Von Google Drive öffnen.";
				RU.Translations["GDrive_Open"] = "Открыть с Google Диска..";

				EN.Translations["GDrive_Save"] = "Save to Google Drive..";
				ES.Translations["GDrive_Save"] = "Guardar en Google Drive.";
				FR.Translations["GDrive_Save"] = "Enregistrer sur Google Drive..";
				DE.Translations["GDrive_Save"] = "Auf Google Drive speichern.";
				RU.Translations["GDrive_Save"] = "Сохранить на Google Диск..";

				EN.Translations["GDrive_TradeMark"] = "Google Drive is a trademark of Google Inc.Use of this trademark is subject to Google Permissions.";
				ES.Translations["GDrive_TradeMark"] = "Google Drive es una marca comercial de Google Inc. El uso de esta marca comercial está sujeto a los permisos de Google.";
				FR.Translations["GDrive_TradeMark"] = "Google Drive est une marque commerciale de Google Inc. L'utilisation de cette marque est soumise aux autorisations de Google.";
				DE.Translations["GDrive_TradeMark"] = "Google Drive ist eine Marke von Google Inc. Die Verwendung dieser Marke unterliegt den Google-Genehmigungen.";
				RU.Translations["GDrive_TradeMark"] = "Google Drive является товарным знаком Google Inc. Использование этого товарного знака осуществляется с разрешения Google.";

				EN.Translations["GDrive_Account"] = "Your Google Account:";
				ES.Translations["GDrive_Account"] = "Tu cuenta de Google:";
				FR.Translations["GDrive_Account"] = "Votre compte Google:";
				DE.Translations["GDrive_Account"] = "Ihr Google-Konto:";
				RU.Translations["GDrive_Account"] = "Ваш аккаунт Google:";

				EN.Translations["GDrive_SelectFile"] = "Select a File:";
				ES.Translations["GDrive_SelectFile"] = "Seleccione un archivo:";
				FR.Translations["GDrive_SelectFile"] = "Sélectionner un fichier:";
				DE.Translations["GDrive_SelectFile"] = "Wählen Sie eine Datei aus:";
				RU.Translations["GDrive_SelectFile"] = "Выберите файл:";

				EN.Translations["GDrive_SelectFolder"] = "Select a Folder:";
				ES.Translations["GDrive_SelectFolder"] = "Seleccione una Carpeta:";
				FR.Translations["GDrive_SelectFolder"] = "Sélectionnez un dossier :";
				DE.Translations["GDrive_SelectFolder"] = "Wählen Sie einen Ordner aus:";
				RU.Translations["GDrive_SelectFolder"] = "Выберите папку:";

				EN.Translations["CreateFolder"] = "New Folder..|Folder Details";
				ES.Translations["CreateFolder"] = "Nueva Carpeta..|Detalles de la Carpeta";
				FR.Translations["CreateFolder"] = "Nouveau dossier..|Détails du dossier";
				DE.Translations["CreateFolder"] = "Neuer Ordner..|Ordnerdetails";
				RU.Translations["CreateFolder"] = "Новая папка..|Детали папки";

				EN.Translations["FileLabels"] = "File|Folder|Size|Created|Modified";
				ES.Translations["FileLabels"] = "Archivo|Carpeta|Tamaño|Creado|Modificado";
				FR.Translations["FileLabels"] = "Fichier|Dossier|Taille|Créé|Modifié";
				DE.Translations["FileLabels"] = "Datei|Ordner|Größe|Erstellt|Geändert";
				RU.Translations["FileLabels"] = "Файл|Папка|Размер|Создано|Изменено";

				EN.Translations["NameValue"] = "Name|Description|Value";
				ES.Translations["NameValue"] = "Nombre|Descripción|Valor";
				FR.Translations["NameValue"] = "Nom|Description|Valeur";
				DE.Translations["NameValue"] = "Name|Beschreibung|Wert";
				RU.Translations["NameValue"] = "Имя|Описание|Значение";

				//EN.Translations[""] = "";
				//ES.Translations[""] = "";
				//FR.Translations[""] = "";
				//DE.Translations[""] = "";
				//RU.Translations[""] = "";

				Util.Serialize_ToJSON(Path.Combine(AppExePath, "Tranlations.json"), Translator);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void Document_Open(string pFilePath = "")
		{
			try
			{
				this.Cursor = Cursors.WaitCursor;
				bool Continuar = true;

				if (string.IsNullOrEmpty(pFilePath))
				{
					OpenFileDialog OFDialog = new OpenFileDialog()
					{
						Filter = "Supported Files|*.note;*.json;*.txt;*.rtf;*.epub",
						FilterIndex = 0,
						DefaultExt = "note",
						AddExtension = true,
						CheckPathExists = true,
						CheckFileExists = true,
						InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
					};
					if (OFDialog.ShowDialog() == DialogResult.OK)
					{
						this.CurrentFile = OFDialog.FileName;
					}
					else
					{
						Continuar = false;
					}
				}
				else
				{
					if (File.Exists(pFilePath))
					{
						this.CurrentFile = pFilePath;
					}
					else
					{
						Continuar = false;
					}
				}

				if (Continuar)
				{
					string Ext = System.IO.Path.GetExtension(CurrentFile); //<- Extension del archivo
					switch (Ext)
					{
						case ".note": this.Document = Util.DeSerialize_FromJSON<MyNoteDocument>(this.CurrentFile); break;
						case ".json": this.Document = Util.DeSerialize_FromJSON<MyNoteDocument>(this.CurrentFile); break;
						case ".epub": break; //TODO: Abrir EPUB
						default:
							this.Document = null;
							//TODO: Importar TXT o RTF en un nuevo nodo del documento actual o en un nuevo doc basado en 'EmptyTemplate'
							break;
					}
					Document_Show();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { this.Cursor = Cursors.Default; }
		}
		private void Document_Show()
		{
			/* Show the current Document in the Treeview  */
			try
			{
				this.Cursor = Cursors.WaitCursor;

				if (!string.IsNullOrEmpty(this.CurrentFile))
				{
					FileInfo file = new System.IO.FileInfo(CurrentFile);
					this.lbStatus.Text = string.Format("{0} [{1}]", CurrentFile, Util.GetFileSize(file.Length));
				}
				else
				{
					this.lbStatus.Text = "Un-saved Document"; //TODO: Traducir
				}
				if (this.Document != null)
				{
					cTreeView1.BeginUpdate();
					cTreeView1.Nodes.Clear();
					cTreeView1.Margin = new Padding(2);
					cTreeView1.AutoExpandSelected = this.AutoExpandSelected;

					if (Document.Metadata != null)
					{
						this.Text = string.Format("MyNotes 3 - {0}", Util.ReplaceAndTranslate(this.Document.Metadata.Title, this.CurrentLanguage));

						/* SETUP THE COVER */
						string _caption = CurrentLanguage.GetTranslation("Cover");
						CTreeNode _Cover = new CTreeNode(_caption, new Button() { Text = _caption, Tag = Document.Metadata });
						_Cover.Tag = Document.Metadata;
						_Cover.Control.Click += Control_Click;

						SizeF textSize = _Cover.GetControlFontSize(_caption);
						if (textSize.Width > 70) _Cover.Control.Width = (int)(textSize.Width + 10);

						cTreeView1.Nodes.Add(_Cover);						
					}

					int index = 0;
					foreach (var nodeInfo in Document.Content)
					{
						string Parent = string.Format("{0:D2}", index);
						var node = CreateTreeNodeEx(nodeInfo, Parent);

						cTreeView1.Nodes.Add(node);
						index++;
					}
					cTreeView1.CollapseAll();
					cTreeView1.EndUpdate();
					ShowNodeContent(Document.Metadata);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { this.Cursor = Cursors.Default; }
		}

		

		public ControlTreeView.CTreeNode CreateTreeNodeEx(DocContent nodeInfo, string Parent, int index = -1)
		{
			CTreeNode _root = new CTreeNode(nodeInfo.Title, new Button() { Text = nodeInfo.Title });
			_root.Control.Click += Control_Click;
			_root.Control.Tag = nodeInfo;
			_root.Tag = nodeInfo;
			_root.ImageKey += (index >= 0) ?
				string.Format("{0}.{1:D2}", Parent, index) :
				Parent;

			SizeF textSize = _root.GetControlFontSize(nodeInfo.Title);
			if (textSize.Width > 70) _root.Control.Width = (int)(textSize.Width + 10);

			if (nodeInfo.Childs != null)
			{
				int i = 0;
				foreach (var child in nodeInfo.Childs)
				{
					_root.Nodes.Add(CreateTreeNodeEx(child, _root.ImageKey, i));
					i++;
				}
			}

			return _root;
		}

		private void Document_Save(bool SaveAs = false, bool ReLoadDoc = true)
		{
			try
			{
				if (Document != null && Document.Content != null && Document.Content.Count > 0)
				{
					this.Cursor = Cursors.WaitCursor;

					//Set current changes in Editor:
					if (cTreeView1.CurrentNode != null && cTreeView1.CurrentNode.Tag is DocContent nodeInfo)
					{
						if (this.richTextBoxEx1.Modified)
						{
							nodeInfo.Content = this.richTextBoxEx1.Rtf;
						}
					}

					if (string.IsNullOrEmpty(this.CurrentFile) || SaveAs)
					{
						string IniDir = string.IsNullOrEmpty(this.CurrentFile) ?
							Environment.GetFolderPath(Environment.SpecialFolder.Desktop) :
							System.IO.Path.GetDirectoryName(this.CurrentFile);

						SaveFileDialog SFDialog = new SaveFileDialog()
						{
							Filter = "Note Document|*.note|Json Document|*.json|EPUB Document|*.epub|RichTextFormat|*.rtf|Plain Text|*.txt",
							FilterIndex = 0,
							DefaultExt = "note",
							AddExtension = true,
							CheckPathExists = true,
							OverwritePrompt = true,
							InitialDirectory = IniDir
						};
						if (SFDialog.ShowDialog() == DialogResult.OK)
						{
							this.CurrentFile = SFDialog.FileName;

							//TODO: Preguntar por las Propiedades del Cocumento
						}
					}

					if (!string.IsNullOrEmpty(this.CurrentFile))
					{
						SaveGoogleDrive();

						string Ext = System.IO.Path.GetExtension(CurrentFile); //<- Extension del archivo
						switch (Ext)
						{
							case ".note": Util.Serialize_ToJSON(this.CurrentFile, Document); break;
							case ".json": Util.Serialize_ToJSON(this.CurrentFile, Document); break;
							case ".rtf": richTextBoxEx1.SaveFile(this.CurrentFile, RichTextBoxStreamType.RichText); break;
							case ".txt": richTextBoxEx1.SaveFile(this.CurrentFile, RichTextBoxStreamType.PlainText); break;
							default: //TODO: Gragar en EPUB
								break;
						}

						if (File.Exists(CurrentFile))
						{
							Invoke((MethodInvoker)(() =>
							{
								lblStatus2.Text = CurrentLanguage.GetTranslation("Saved");
								if (ReLoadDoc) Document_Open(this.CurrentFile);
							}));
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { this.Cursor = Cursors.Default; }
		}

		protected async void SaveDocumentAsync(bool SaveAs = false, bool ReLoadDoc = true)
		{
			this.Cursor = Cursors.WaitCursor;

			// 'await' long-running method by wrapping inside Task.Run
			await Task.Run(new Action(() =>
			{
				if (string.IsNullOrEmpty(this.CurrentFile) || SaveAs)
				{
					string IniDir = string.IsNullOrEmpty(this.CurrentFile) ?
						Environment.GetFolderPath(Environment.SpecialFolder.Desktop) :
						System.IO.Path.GetDirectoryName(this.CurrentFile);

					SaveFileDialog SFDialog = new SaveFileDialog()
					{
						Filter = "Note Document|*.note|Json Document|*.json|EPUB Document|*.epub|RichTextFormat|*.rtf|Plain Text|*.txt",
						FilterIndex = 0,
						DefaultExt = "note",
						AddExtension = true,
						CheckPathExists = true,
						OverwritePrompt = true,
						InitialDirectory = IniDir
					};
					if (SFDialog.ShowDialog() == DialogResult.OK)
					{
						this.CurrentFile = SFDialog.FileName;

						//TODO: Preguntar por las Propiedades del Cocumento
					}
				}

				if (!string.IsNullOrEmpty(this.CurrentFile))
				{
					string Ext = System.IO.Path.GetExtension(CurrentFile); //<- Extension del archivo
					switch (Ext)
					{
						case ".note": Util.Serialize_ToJSON(this.CurrentFile, Document); break;
						case ".json": Util.Serialize_ToJSON(this.CurrentFile, Document); break;
						default: //TODO: Gragar en EPUB
							break;
					}

					if (File.Exists(CurrentFile))
					{
						this.BeginInvoke(new Action(() =>
						{

						}));
					}
				}
			})).ContinueWith(new Action<Task>(task =>
			{
				// No need to use BeginInvoke here
				//   because ContinueWith was called with TaskScheduler.FromCurrentSynchronizationContext()
				this.Cursor = Cursors.Default;
				if (ReLoadDoc) Document_Open(this.CurrentFile);
			}), TaskScheduler.FromCurrentSynchronizationContext());
		}

		public void ShowNodeContent(object nodeInfo)
		{
			try
			{
				if (nodeInfo != null)
				{
					this.richTextBoxEx1.Clear();

					if (nodeInfo is DocContent) //<- Is a regular Content
					{
						var _Content = nodeInfo as DocContent;
						if (_Content.Content.Contains(@"{\rtf1"))
						{
							this.richTextBoxEx1.Rtf = _Content.Content;
						}
						else
						{
							this.richTextBoxEx1.Text = _Content.Content;
						}
						richTextBoxEx1.ReadOnly = false;
					}

					if (nodeInfo is DocMetadata) //<- Is the Cover
					{
						richTextBoxEx1.ReadOnly = false;
						string rtf = BuildRTFCover(Document.Metadata);
						if (!string.IsNullOrEmpty(rtf))
						{
							richTextBoxEx1.Rtf = rtf;
						}

						richTextBoxEx1.ReadOnly = true;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public string BuildRTFCover(DocMetadata pCoverData)
		{
			string _ret = string.Empty;
			try
			{
				StringBuilder RTFlines = new StringBuilder();
				RTFlines.AppendLine(@"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033{\fonttbl{\f0\fnil\fcharset0 Calibri;}{\f1\fnil\fcharset0 Microsoft Sans Serif;}{\f2\fnil Microsoft Sans Serif;}}");
				RTFlines.AppendLine(@"{\*\generator Riched20 10.0.22621}\viewkind4\uc1 ");
				RTFlines.AppendLine(@"\pard\sa200\sl240\slmult1\qc\f0\fs22\lang10 ");

				if (!string.IsNullOrEmpty(pCoverData.Cover))
				{
					RTFlines.AppendLine(Util.GetEmbedImageString((Bitmap)Util.ImageFromBase64(pCoverData.Cover)));
					RTFlines.AppendLine(@"\par");
				}

				RTFlines.AppendLine();

				RTFlines.AppendLine(string.Format(@"\pard\qc\b\f1\fs28\lang1033  {0}\par", Util.ReplaceAndTranslate(pCoverData.Title, CurrentLanguage)));
				RTFlines.AppendLine(string.Format(@"\b0\fs17 {0}\par", pCoverData.Series));
				RTFlines.AppendLine(string.Format(@"\f1\fs17 By {0}\par", pCoverData.Author));
				RTFlines.AppendLine(@"\par");

				if (!string.IsNullOrEmpty(pCoverData.Summary))
				{
					RTFlines.AppendLine();
					RTFlines.AppendLine(string.Format(@"\pard\qj\b Summary:\b0\tab {0}\par", pCoverData.Summary.Replace(Environment.NewLine, @"\par")));
					RTFlines.AppendLine(@"\pard\qc\par");
					RTFlines.AppendLine();
				}
				RTFlines.AppendLine(@"\pard ");
				if (!string.IsNullOrEmpty(pCoverData.Language))
				{
					RTFlines.AppendLine(string.Format(@"Language:\tab {0}\par", pCoverData.Language));
				}
				if (pCoverData.Contributors != null)
				{
					string Contributors = string.Empty;
					foreach (string contributor in pCoverData.Contributors)
					{
						Contributors += contributor + ", ";
					}
					Contributors = Contributors.Remove(Contributors.Length - 2);
					RTFlines.AppendLine(string.Format(@"Contributors:\tab {0}\par", Contributors));
				}
				if (pCoverData.Translators != null)
				{
					string Translators = string.Empty;
					foreach (string item in pCoverData.Translators)
					{
						Translators += item + ", ";
					}
					Translators = Translators.Remove(Translators.Length - 2);
					RTFlines.AppendLine(string.Format(@"Translators:\tab {0}\par", Translators));
				}
				if (pCoverData.Identifiers != null)
				{
					string Identifiers = string.Empty;
					foreach (string item in pCoverData.Identifiers)
					{
						Identifiers += item + ", ";
					}
					Identifiers = Identifiers.Remove(Identifiers.Length - 2);
					RTFlines.AppendLine(string.Format(@"Identifiers:\tab {0}\par", Identifiers));
				}
				if (pCoverData.Publisher != null)
				{
					string Publisher = string.Empty;
					foreach (string item in pCoverData.Publisher)
					{
						Publisher += item + ", ";
					}
					Publisher = Publisher.Remove(Publisher.Length - 2);
					RTFlines.AppendLine(string.Format(@"Publisher:\tab\tab {0}\par", Publisher));
				}
				if (pCoverData.Keywords != null)
				{
					string Keywords = string.Empty;
					foreach (string item in pCoverData.Keywords)
					{
						Keywords += item + ", ";
					}
					Keywords = Keywords.Remove(Keywords.Length - 2);
					RTFlines.AppendLine(string.Format(@"Keywords:\tab {0}\par", Keywords));
				}
				if (pCoverData.Categories != null)
				{
					string Categories = string.Empty;
					foreach (string item in pCoverData.Categories)
					{
						Categories += item + ", ";
					}
					Categories = Categories.Remove(Categories.Length - 2);
					RTFlines.AppendLine(string.Format(@"Categories:\tab {0}\par", Categories));
				}

				if (pCoverData.GoogleDrive != null)
				{
					RTFlines.AppendLine(@"\par");
					RTFlines.AppendLine(Util.GetEmbedImageString( (Bitmap)Properties.Resources.ResourceManager.GetObject("logo_drive_2020q4_color_16x16")) + " Google Drive Sync.");
					RTFlines.AppendLine(string.Format("Last Update: {0}", Convert.ToDateTime(pCoverData.GoogleDrive.LastUpdate).ToString()));
					RTFlines.AppendLine(@"\par");
				}

				RTFlines.AppendLine();
				RTFlines.AppendLine(@"\pard\qc\par");
				RTFlines.AppendLine(@"\par");
				RTFlines.AppendLine(@"\pard\sa200\sl276\slmult1\f0\fs22\lang10\par");
				RTFlines.AppendLine(@"}"); //<- EOF

				_ret = RTFlines.ToString();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		/* Method to create a table format string which can directly be set to 
		   RichTextBoxControl. Rows, columns and cell width are passed as parameters 
		   rather than hard coding as in previous example. */
		private String InsertTableInRichTextBox(int rows, int cols, int width = 2000)
		{
			//Create StringBuilder Instance
			StringBuilder sringTableRtf = new StringBuilder();

			//beginning of rich text format
			sringTableRtf.AppendLine(@"{\rtf1 \trowd ");

			//Variable for cell width
			int cellWidth;

			//Loop to create table string
			for (int i = 0; i < rows; i++)
			{
				sringTableRtf.Append(@"\trowd"); //<- Start row

				for (int j = 0; j < cols; j++)
				{
					//Calculate cell end point for each cell
					cellWidth = (j + 1) * width;

					//A cell with width 1000 in each iteration.
					sringTableRtf.Append(@"\cellx" + cellWidth.ToString());
				}

				//Append the row:  \qj=Justified, \qc=Centered, \ql=Left-aligned (default), \qr=Right-aligned.
				sringTableRtf.Append(@"\intbl\qj \cell \row" + Environment.NewLine);
			}
			sringTableRtf.AppendLine(@"\pard");
			sringTableRtf.AppendLine(@"}");

			return sringTableRtf.ToString();
		}

		/// <summary>Attemps to save the Current Document on Google Drive.</summary>
		public async Task SaveGoogleDrive()
		{
			try
			{
				this.Cursor = Cursors.WaitCursor;
				if (this.Document != null && this.Document.Metadata.GoogleDrive != null)
				{
					using (GDriveNET API = new GDriveNET())
					{
						API.APP_NAME = Application.ProductName;
						API.ClientSecrets = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "client_secrets.json");
						string UserAccount = string.Empty;

						//Check If the user was already logged..
						if (Directory.Exists(API.LOCAL_STORAGE))
						{
							//Check the last logged user:
							FileInfo myCredentials = new DirectoryInfo(API.LOCAL_STORAGE)
								.GetFiles("*.*").OrderByDescending(f => f.LastWriteTime).First();

							if (myCredentials != null)
							{
								var UserData = myCredentials.Name.Split(new char[] { '-' }).ToList();
								UserAccount = UserData[1];
							}
						}
						if (await API.Authenticate(UserAccount))
						{
							var Returned = await API.UpdateFile(this.CurrentFile, this.Document.Metadata.GoogleDrive.FileID);
							this.Document.Metadata.GoogleDrive.FileID = Returned.Id;
							this.Document.Metadata.GoogleDrive.LastUpdate = (DateTime)Returned.ModifiedTime;
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { this.Cursor = Cursors.Default; }
		}

		#endregion

		#region TreeView

		//Events Firing Order: Control_Click -> BeforeNodeChanged -> AfterNodeChanged -> Click -> MouseClick

		private void Control_Click(object sender, EventArgs e)
		{
			if (sender is Button _control)
			{
				//TODO: Control_Click
			}
		}
		private void cTreeView1_BeforeNodeChanged(object sender, CTreeViewEventArgs e)
		{
			// Applies the Text on the Content of the Current Node before changing to the new one:	
			if (cTreeView1.CurrentNode != null)
			{
				var nodeInfo = cTreeView1.CurrentNode.Tag as DocContent;
				if (nodeInfo != null && this.richTextBoxEx1.Modified)
				{
					nodeInfo.Content = this.richTextBoxEx1.Rtf;
				}
			}
		}
		private void cTreeView1_AfterNodeChanged(object sender, CTreeViewEventArgs e)
		{
			if (cTreeView1.CurrentNode != null)
			{
				this.lblStatus2.Text = e.Node.FullPath;
			}
		}
		private void cTreeView1_MouseUp(object sender, MouseEventArgs e)
		{

		}
	
		private void CTreeView1_Click(object sender, EventArgs e)
		{
			//Here we show the Content of the selected node:
			if (sender is CTreeView TV)
			{
				if (TV.CurrentNode != null)
				{
					this.lblStatus2.Text = TV.CurrentNode.FullPath;
					ShowNodeContent(TV.CurrentNode.Tag);
				}
			}
		}
		private void cTreeView1_MouseClick(object sender, MouseEventArgs e)
		{

		}

		//TreeView Context Menus:
		private void contextMenuForTreeView_Opening(object sender, CancelEventArgs e)
		{
			// The popup menu doesnt show for the Cover element
			var SelectedNode = this.cTreeView1.CurrentNode;
			if (SelectedNode != null && SelectedNode.Tag is DocMetadata)
			{
				e.Cancel = true;
			}
		}
		
		private void addRootItemToolStripMenuItem_Click(object sender, EventArgs e)
		{
			/* ADDS A NEW ITEM AT SAME LEVEL THAN THE CLICKED ONE AND AT END OF IT'S LEVEL LIST  */
			var SelectedNode = this.cTreeView1.CurrentNode;
			if (SelectedNode != null && SelectedNode.Tag is DocContent) //<- Excludes the Cover
			{
				bool IsRoot = SelectedNode.ParentNode is null;
				int count = IsRoot ? this.cTreeView1.Nodes.Count : SelectedNode.ParentNode.Nodes.Count;
				var _caption = string.Format("{0} {1}", CurrentLanguage.GetTranslation("NewEntry"), count);
				var _contenido = new DocContent(0, _caption, Util.CreateSimpleRTF(_caption));

				var newNode = new CTreeNode(_caption,
							new Button()
							{
								Text = _caption,
								Tag = _contenido
							});
				newNode.Tag = _contenido;
				newNode.Control.Click += Control_Click;

				//Fix the length of the text
				SizeF textSize = newNode.GetControlFontSize(_caption);
				if (textSize.Width > 70) newNode.Control.Width = (int)(textSize.Width + 10);

				if (IsRoot)
				{
					var Path = SelectedNode.GetNodePath();
					int CurrIndex = Path[Path.Count - 1] -1; //<- The cover doesnt counts

					//Adds the new element as a Root above the Selected Node:						
					this.cTreeView1.Nodes.Insert(SelectedNode.Index, newNode);		//<- Adds the Node to the Tree
					this.Document.Content.Insert(CurrIndex, _contenido);			//<- Adds the node to the DataSource
				}
				else
				{
					//Locate the Parent Node in the DataSource:
					var Path = SelectedNode.GetNodePath();
					if (Path.Count > 0) Path.RemoveAt(Path.Count - 1); //<- last item is the SelectedNode, remove it to get the Parent
					Path[0] = Path[0] - 1; //<- The cover doesnt counts

					DocContent Parent = Util.GetItem(this.Document.Content, Path);
					if (Parent != null)
					{						
						SelectedNode.ParentNode.Nodes.Insert(SelectedNode.Index, newNode);      //<- Adds the Node to the Tree
						Parent.Childs.Insert(SelectedNode.Index-1, _contenido);					//<- Adds the node to the DataSource
					}
				}

				//TODO: -> TV.SelectedNode = Util.FindTreeNodeByFullPath(cTreeView1.Nodes, NodePath + CurrentLanguage.GetTranslation("NewEntry"));
			}
		}
		private void addSubItemToolStripMenuItem_Click(object sender, EventArgs e)
		{
			/* ADDS A NEW SUB-ITEM AS CHILD OF THE CLICKED ONE AND AT END OF THE LIST  */
			var SelectedNode = this.cTreeView1.CurrentNode;
			if (SelectedNode != null && SelectedNode.Tag is DocContent)
			{
				var _caption = CurrentLanguage.GetTranslation("NewEntry") + " " + SelectedNode.Nodes.Count;
				var _contenido = new DocContent(0, _caption, Util.CreateSimpleRTF(_caption));
				var newNode = new CTreeNode(_caption,
							new Button()
							{
								Text = _caption,
								Tag = _contenido
							});
				newNode.Tag = _contenido;
				newNode.Control.Click += Control_Click;

				//Fix the length of the text
				SizeF textSize = newNode.GetControlFontSize(_caption);
				if (textSize.Width > 70) newNode.Control.Width = (int)(textSize.Width + 10);

				//Locate the Parent Node in the DataSource:
				var Path = SelectedNode.GetNodePath();
				Path[0] = Path[0] - 1; //<- The cover doesnt counts

				DocContent Parent = Util.GetItem(this.Document.Content, Path);
				if (Parent != null)
				{
					SelectedNode.Nodes.Add(newNode);      //<- Adds the Node to the Tree
					Parent.Childs.Add(_contenido);        //<- Adds the node to the DataSource
				}
			}
		}
		private void deleteItemToolStripMenuItem_Click(object sender, EventArgs e)
		{
			/* DELETE THE SELECTED ITEM */
			var SelectedNode = this.cTreeView1.CurrentNode;
			if (SelectedNode != null && SelectedNode.Tag is DocContent Current)
			{
				if (MessageBox.Show(
					string.Format(CurrentLanguage.GetTranslation("ConfirmDelete"), Current.Title),
					CurrentLanguage.GetTranslation("Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					//Locate the Parent Node in the DataSource:
					var Path = SelectedNode.GetNodePath();
					Path[0] = Path[0] - 1; //<- The cover doesnt counts

					DeleteElement(Document.Content, Path.ToArray());//<- Delete the node from the DataSource
					this.cTreeView1.Nodes.Remove(SelectedNode);     //<- Delete the Node from the Tree
				}
			}
		}
		private void editItemToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var SelectedNode = this.cTreeView1.CurrentNode;
			if (SelectedNode != null && SelectedNode.Tag is DocContent) //<- Excludes the Cover
			{
				string DefaultName = SelectedNode.Name;
				if (Util.InputBox(CurrentLanguage.GetTranslation("Confirm"), 
					"Enter a new name for the item:", ref DefaultName) == DialogResult.OK)
				{
					//Locate the Parent Node in the DataSource:
					var Path = SelectedNode.GetNodePath();
					Path[0] = Path[0] - 1; //<- The cover doesnt counts

					RenameElement(Document.Content, Path.ToArray(), DefaultName);   //<- Rename the node from the DataSource
					SelectedNode.Control.Text = DefaultName;                        //<- Rename the node from the Tree
					SelectedNode.Name = DefaultName;
				}
			}			
		}
		private void mnuAutoExpandSelected_CheckStateChanged(object sender, EventArgs e)
		{
			this.AutoExpandSelected = (sender as ToolStripMenuItem).Checked;			
			cTreeView1.AutoExpandSelected = this.AutoExpandSelected;
			Util.WinReg_WriteKey("Settings", "AutoExpandSelected", this.AutoExpandSelected);
		}

		/// <summary>Remove the child element from the parent object, recursively.</summary>
		/// <param name="contents">The parent Array</param>
		/// <param name="path">the full path to the object to remove</param>
		/// <param name="index"></param>
		private void DeleteElement(List<DocContent> hData, int[] path)
		{
			if (path.Length == 0)
			{
				return;
			}

			int index = path[0];
			if (path.Length == 1)
			{
				// Remove the element at the specified index
				hData.RemoveAt(index);
			}
			else
			{
				// Recurse into the next level
				int[] subPath = path.Skip(1).ToArray();
				DeleteElement(hData[index].Childs, subPath);
			}
		}
		private void RenameElement(List<DocContent> hData, int[] path, string newName)
		{
			if (path.Length == 0)
			{
				return;
			}

			int index = path[0];
			if (path.Length == 1)
			{
				// Rename the element at the specified index
				hData[index].Title = newName;
			}
			else
			{
				// Recurse into the next level
				int[] subPath = path.Skip(1).ToArray();
				RenameElement(hData[index].Childs, subPath, newName);
			}
		}

		#endregion

		#region Find Text

		private void toolStripTextBox1_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Enter)
			{
				FindTextInDocs(toolSearchText.Text);
			}
		}
		private void toolFind_ButtonClick(object sender, EventArgs e)
		{
			FindTextInDocs(toolSearchText.Text);
		}
		private void toolReplace_ButtonClick(object sender, EventArgs e)
		{
			//Replace & Find Next:
			if (!string.IsNullOrEmpty(richTextBoxEx1.SelectedText))
			{
				richTextBoxEx1.SelectedText = txtReplaceText.Text;
				FindTextInDocs(toolSearchText.Text);
			}
		}

		/// <summary>Progressive search for text in a RichTextBox, automatically highlight and display results.</summary>
		/// <param name="pSearchText">Text to Find</param>
		public int FindTextInDocs(string pSearchText)
		{
			int _ret = 0; //<- Index of found text in current doc
			try
			{
				if (!string.IsNullOrEmpty(pSearchText) && this.cTreeView1.Nodes.Count > 0)
				{
					bool NewSearch = false;
					if (this.searchResults is null) //<- No Search had been made
					{
						NewSearch = true; // Start a New Search
					}
					else
					{
						//Check if any of the Search criteria had been changed:
						if (this.searchResults.SearchKey != toolSearchText.Text)
						{
							NewSearch = true;
						}
						else if (FindMatchCase.Checked != searchResults.FindMatchCase || FindWholeWord.Checked != searchResults.FindWholeWord)
						{
							NewSearch = true;
						}
						else
						{
							//Show Next Found Result:
							var LocalResults = searchResults.FoundNodes[searchResults.GlobalIndex].TextIndexes;
							if (LocalResults != null)
							{
								searchResults.CurrentIndex++;
								searchResults.LocalIndex++;
								if (searchResults.LocalIndex > LocalResults.Count - 1)
								{
									searchResults.GlobalIndex++;
									searchResults.LocalIndex = 0;
								}
							}
						}
					}

					if (NewSearch) //<- Starts a New Search
					{
						this.Cursor = Cursors.WaitCursor;
						var result = Task.Run(() =>
						{ /*  THIS TASK RUNS Synchronously, WILL WAIT FOR COMPLETION AND RETURN CODE  */
							this.searchResults = new SearchResults(toolSearchText.Text)
							{
								FindMatchCase = FindMatchCase.Checked,
								FindWholeWord = FindWholeWord.Checked
							};
							this.searchResults.SearchAllNodes(this.cTreeView1);
							this.searchResults.CurrentIndex++;
							return true;
						}).Result;
						this.Cursor = Cursors.Default;
					}

					//Show Results:
					if (this.searchResults.TotalResults > 0)
					{
						//If hit the end of the results, goes back to beginning:
						if (searchResults.GlobalIndex > searchResults.FoundNodes.Count - 1)
						{
							searchResults.CurrentIndex = 1;
							searchResults.GlobalIndex = 0;
							searchResults.LocalIndex = 0;
						}

						lblSearchResults.ForeColor = Color.Blue;
						lblSearchResults.Text = string.Format(CurrentLanguage.GetTranslation("SearchResults"),
							searchResults.CurrentIndex,
							searchResults.TotalResults,
							searchResults.LocalIndex + 1,
							searchResults.FoundNodes[searchResults.GlobalIndex].TextIndexes.Count);

						if (cTreeView1.CurrentNode != searchResults.FoundNodes[searchResults.GlobalIndex].Node)
						{
							cTreeView1.AutoExpandSelected = true;
							cTreeView1.CurrentNode = searchResults.FoundNodes[searchResults.GlobalIndex].Node;
							this.lblStatus2.Text = cTreeView1.CurrentNode.FullPath;

							cTreeView1.CurrentNode.Expand();
							var PP = cTreeView1.CurrentNode.GetNodePath();
							ShowNodeContent(cTreeView1.CurrentNode.Tag);
						}						

						_ret = searchResults.FoundNodes[searchResults.GlobalIndex].TextIndexes[searchResults.LocalIndex];
						richTextBoxEx1.Select(_ret, toolSearchText.Text.Length);
					}
					else //<- Nothing was Found
					{
						lblSearchResults.ForeColor = Color.Red;
						lblSearchResults.Text = CurrentLanguage.GetTranslation("NotFound");
					}
				}
				else
				{
					//Clear the Search:
					lblSearchResults.Text = string.Empty;
					this.searchResults = null;
				}
			}
			catch { _ret = 0; } //<- Starts from begining on error
			return _ret;
		}

		#endregion

		#region Printing

		private int firstCharOnPage = 0;
		private void printDocument1_BeginPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
		{
			// Start at the beginning of the text
			firstCharOnPage = 0;
		}
		private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
		{
			// To print the boundaries of the current page margins
			// uncomment the next line:
			// e.Graphics.DrawRectangle(System.Drawing.Pens.Blue, e.MarginBounds);

			// make the RichTextBoxEx calculate and render as much text as will
			// fit on the page and remember the last character printed for the
			// beginning of the next page
			firstCharOnPage = richTextBoxEx1.FormatRange(false, e, firstCharOnPage, richTextBoxEx1.TextLength);
			// check if there are more pages to print
			if (firstCharOnPage < richTextBoxEx1.TextLength)
				e.HasMorePages = true;
			else
				e.HasMorePages = false;
		}
		private void printDocument1_EndPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
		{
			// Clean up cached information
			richTextBoxEx1.FormatRangeDone();
		}

		#endregion

		#region Menus

		private void mnuFile_New_Templates_Click(object sender, EventArgs e)
		{
			TemplateManager _Form = new TemplateManager() { CurrentLanguage = this.CurrentLanguage };
			_Form.Show();
		}
		private void mnuFile_Open_Click(object sender, EventArgs e)
		{
			Document_Open();
		}
		private void mnuFile_Save_Click(object sender, EventArgs e)
		{
			Document_Save();
		}
		private void mnuFile_SaveAs_Click(object sender, EventArgs e)
		{
			Document_Save(true);
		}

		private void mnuFile_Print_Click(object sender, EventArgs e)
		{
			//Prints the Document asking for the Printer:
			if (printDialog1.ShowDialog() == DialogResult.OK)
			{
				printDocument1.PrinterSettings = printDialog1.PrinterSettings;
				printDocument1.Print();
			}
		}
		private void mnuPageSetup_Click(object sender, EventArgs e)
		{
			//Ask the User for the Page Settings:
			if (pageSetupDialog1.ShowDialog() == DialogResult.OK)
			{
				//Save the Page Settings for future use:
				printDocument1.DefaultPageSettings = pageSetupDialog1.PageSettings;
				this.MyPageSettings = new
				{
					PaperSize = printDocument1.DefaultPageSettings.PaperSize,
					Margins = printDocument1.DefaultPageSettings.Margins,
					Landscape = printDocument1.DefaultPageSettings.Landscape
				};
				Util.Serialize_ToJSON<dynamic>(Path.Combine(AppExePath, "PageSettings.json"), MyPageSettings);
			}
		}
		private void mnuFile_Preview_Click(object sender, EventArgs e)
		{
			//Shows a Print Preview Window (Maximized):  
			(printPreviewDialog1 as Form).WindowState = FormWindowState.Maximized;
			printPreviewDialog1.ShowDialog(this);
		}
		private void mnuFile_DocProps_Click(object sender, EventArgs e)
		{
			//Allows to edit Document Metadata:
			if (Document != null && Document.Metadata != null)
			{
				using (DocPropertiesForm _DPF = new DocPropertiesForm(Document.Metadata))
				{
					if (_DPF.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(CurrentFile))
					{
						Document_Save();
					}
				}
			}
		}
		private void mnuFile_Exit_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void mnuLanguages_Click(object sender, EventArgs e)
		{
			//Only 1 Lang menu can be checked:
			var mnu = sender as ToolStripMenuItem;
			foreach (var item in mnuLanguage.DropDownItems)
			{
				(item as ToolStripMenuItem).Checked = false;
			}
			mnu.Checked = true;

			//Set the Current Language:
			LanguageChanged = true;
			this.Language = mnu.Tag as string;
			Util.WinReg_WriteKey("Settings", "Lang", this.Language);

			//Apply the changes:
			LoadSettings(true);
		}

		#endregion

		#region ToolBar

		private void toolNew_Click(object sender, EventArgs e)
		{
			LoadTemplate(Templates[0]);
		}
		private void toolOpenFile_Click(object sender, EventArgs e)
		{
			Document_Open();
		}
		private void toolSave_Click(object sender, EventArgs e)
		{
			Document_Save(ReLoadDoc: false);
		}
		private void toolPrint_Click(object sender, EventArgs e)
		{
			//Prints the Document Directly to the Default Printer, no promts.
			printDocument1.Print();
		}

		private void toolCopy_Click(object sender, EventArgs e)
		{
			if (!this.richTextBoxEx1.ReadOnly && this.richTextBoxEx1.SelectionLength > 0)
			{
				richTextBoxEx1.Copy();
			}
		}
		private void toolCut_Click(object sender, EventArgs e)
		{
			if (!this.richTextBoxEx1.ReadOnly && this.richTextBoxEx1.SelectionLength > 0)
			{
				richTextBoxEx1.Cut();
			}
		}
		private void toolPaste_Click(object sender, EventArgs e)
		{
			if (!this.richTextBoxEx1.ReadOnly && Clipboard.ContainsText())
			{
				richTextBoxEx1.Paste();
			}
		}
		private void toolUndo_Click(object sender, EventArgs e)
		{
			if (!this.richTextBoxEx1.ReadOnly && this.richTextBoxEx1.CanUndo)
			{
				richTextBoxEx1.Undo();
			}
		}
		private void toolRedo_Click(object sender, EventArgs e)
		{
			if (!this.richTextBoxEx1.ReadOnly && this.richTextBoxEx1.CanRedo)
			{
				richTextBoxEx1.Redo();
			}
		}

		private void toolFont_Click(object sender, EventArgs e)
		{
			FontDialog Dialog = new FontDialog()
			{
				ShowColor = false,
				ShowHelp = false,
				ShowApply = false,
				FontMustExist = true,
				Font = richTextBoxEx1.SelectionFont
			};
			if (Dialog.ShowDialog() == DialogResult.OK)
			{
				richTextBoxEx1.SelectionFont = Dialog.Font;
			}

		}
		private void toolFontcolor_Click(object sender, EventArgs e)
		{
			ColorDialog Dialog = new ColorDialog()
			{
				AnyColor = true,
				FullOpen = true,
				AllowFullOpen = true,
				Color = richTextBoxEx1.SelectionColor
			};
			if (Dialog.ShowDialog() == DialogResult.OK)
			{
				richTextBoxEx1.SelectionColor = Dialog.Color;
			}
		}
		private void toolBackColor_Click(object sender, EventArgs e)
		{
			ColorDialog Dialog = new ColorDialog()
			{
				AnyColor = true,
				FullOpen = true,
				AllowFullOpen = true,
				Color = richTextBoxEx1.SelectionBackColor
			};
			if (Dialog.ShowDialog() == DialogResult.OK)
			{
				richTextBoxEx1.SelectionBackColor = Dialog.Color;
			}
		}

		private void toolBold_Click(object sender, EventArgs e)
		{
			//Toggles the 'Bold' of the selected text:
			richTextBoxEx1.SelectionFont = richTextBoxEx1.SelectionFont.Bold ?
				new Font(richTextBoxEx1.SelectionFont, FontStyle.Regular) :
				new Font(richTextBoxEx1.SelectionFont, FontStyle.Bold);
		}
		private void toolCursiva_Click(object sender, EventArgs e)
		{
			richTextBoxEx1.SelectionFont = richTextBoxEx1.SelectionFont.Style == FontStyle.Italic ?
				new Font(richTextBoxEx1.SelectionFont, FontStyle.Regular) :
				new Font(richTextBoxEx1.SelectionFont, FontStyle.Italic);
		}
		private void toolSubrayado_Click(object sender, EventArgs e)
		{
			richTextBoxEx1.SelectionFont = richTextBoxEx1.SelectionFont.Underline ?
				new Font(richTextBoxEx1.SelectionFont, FontStyle.Regular) :
				new Font(richTextBoxEx1.SelectionFont, FontStyle.Underline);
		}
		private void toolTachado_Click(object sender, EventArgs e)
		{
			richTextBoxEx1.SelectionFont = richTextBoxEx1.SelectionFont.Strikeout ?
				new Font(richTextBoxEx1.SelectionFont, FontStyle.Regular) :
				new Font(richTextBoxEx1.SelectionFont, FontStyle.Strikeout);
		}

		private void izquierdaToolStripMenuItem_Click(object sender, EventArgs e)
		{
			richTextBoxEx1.SelectionAlignment = RichTextBoxEx.TextAlignment.Left;
		}
		private void centroToolStripMenuItem_Click(object sender, EventArgs e)
		{
			richTextBoxEx1.SelectionAlignment = RichTextBoxEx.TextAlignment.Center;
		}
		private void derechaToolStripMenuItem_Click(object sender, EventArgs e)
		{
			richTextBoxEx1.SelectionAlignment = RichTextBoxEx.TextAlignment.Right;
		}
		private void justificadoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			richTextBoxEx1.SelectionAlignment = RichTextBoxEx.TextAlignment.Justify;
		}
		private void toolBullets_Click(object sender, EventArgs e)
		{
			richTextBoxEx1.SelectionBullet = !richTextBoxEx1.SelectionBullet;
		}
		private void toolWordWrap_Click(object sender, EventArgs e)
		{
			richTextBoxEx1.WordWrap = !richTextBoxEx1.WordWrap;
		}
		private void toolInsertImage_Click(object sender, EventArgs e)
		{
			try
			{
				using (OpenFileDialog OFDialog = new OpenFileDialog()
				{
					Filter = "Image Files|*.jpg;*.png;*.bmp;*.jpeg;*.gif;*.jpeg",
					FilterIndex = 0,
					DefaultExt = "jpg",
					AddExtension = true,
					CheckPathExists = true,
					CheckFileExists = true,
					InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
				})
				{
					if (OFDialog.ShowDialog() == DialogResult.OK)
					{
						Image myImage = Util.OpenImage(OFDialog.FileName); // new Bitmap(OFDialog.FileName);
						if (myImage.Width > 1024 || myImage.Height > 768)
						{
							//Image is too big, we are going to resize it:
							if (MessageBox.Show("Would you like to reduce it? Max size is 1.024 x 768px.", "WOW! that image is Huge!", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
							{
								myImage = Util.ResizeImage(myImage, 1024, 768);
							}
							else
							{
								//Nothing, we use the HUGE image.
							}
						}

						Clipboard.SetImage(myImage);
						richTextBoxEx1.Paste();
					}
				};
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		private void toolInsertTable_Click(object sender, EventArgs e)
		{
			using (TableForm TF = new TableForm(2, 2))
			{
				if (TF.ShowDialog() == DialogResult.OK)
				{
					int MaxVisibleSize = 11100; //<- Max Table Width for Visible A4 page is 11100:
					int ColumnWidth = MaxVisibleSize / TF.ColumnCount;

					richTextBoxEx1.SelectedRtf = InsertTableInRichTextBox(TF.RowCount, TF.ColumnCount, ColumnWidth);
				}
			}
		}
		private void toolInsertLink_Click(object sender, EventArgs e)
		{
			using (LinkForm LF = new LinkForm(richTextBoxEx1.SelectedText))
			{
				if (LF.ShowDialog() == DialogResult.OK)
				{
					if (!string.IsNullOrEmpty(richTextBoxEx1.SelectedRtf))
					{
						richTextBoxEx1.SelectedRtf = richTextBoxEx1.SelectedRtf.Replace(richTextBoxEx1.SelectedText, LF.LinkRTF);
					}
					else
					{
						richTextBoxEx1.SelectedRtf = LF.LinkRTF;
					}
					//richTextBoxEx1.InsertLink( LF.FriendlyName, LF.HyperLink);
				}
			}
		}

		#endregion

		private void richTextBoxEx1_MouseMove(object sender, MouseEventArgs e)
		{
			//TODO: Cursor en los bordes de las Tablas
		}
		private void richTextBoxEx1_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			try
			{
				// Enviar los Links al navegador:
				//MessageBox.Show("A link has been clicked.\nThe link text is '" + e.LinkText + "'");
				System.Diagnostics.Process.Start(e.LinkText);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		private void richTextBoxEx1_KeyDown(object sender, KeyEventArgs e)
		{
			// CTRL+F to Find selected Text
			if (e.KeyCode == Keys.F && (e.Control))
			{
				toolSearchText.Text = richTextBoxEx1.SelectedText;

				FindTextInDocs(toolSearchText.Text);
			}
		}
		private void richTextBoxEx1_SelectionChanged(object sender, EventArgs e)
		{
			var RTF = sender as RichTextBoxEx;
			if (RTF != null && !string.IsNullOrEmpty(RTF.SelectedRtf))
			{
				if (RTF.SelectedRtf.Contains(@"\row"))
				{
					//es una fila
				}
				if (RTF.SelectionType == RichTextBoxSelectionTypes.Object)
				{

				}
			}
		}

		private void pictureBox1_Paint(object sender, PaintEventArgs e)
		{
			/*
			//Obtiene los Pixeles del area imprimible (Ancho de Hoja - margenes)
			int Pixels = (int)(PrintableWidth * e.Graphics.DpiX / 100.0f);

			//Pixels = centimeters * DPI / 2.54
			int OneCm =  (int)((1.0f * e.Graphics.DpiX) / 2.54f); //<- Pixels in 1.0cm
			int HalfCm = (int)((0.5f * e.Graphics.DpiX) / 2.54f); //<- Pixels in 0.5cm

			//Dibuja un rectangulo azul indicando el area imprimible
			var brush = new SolidBrush(Color.FromArgb(0, 103, 184));
			e.Graphics.FillRectangle(brush, 10, 0, Pixels, 10);

			
			// Visible Bounds on the PictureBox: H and V rulers are always shown no matter the scrolling of the Picturebox.
			// TODO: graphic glitches when scrolling backwards.
			var visY = Math.Abs(this.pictureBox1.Bounds.Y);
			var visX = Math.Abs(this.pictureBox1.Bounds.X);

			
			// 2. Horizontal Ruler	
			using (Pen pen = new Pen(Color.White, 1.0F))
			{
				for (int i = 0; i < Pixels; i++)
				{
					//if (i % HalfCm == 0) // <- Minor line (3px width) each 0.5cm
					//{
					//	e.Graphics.DrawLine(pen, new Point(i + 10, visY), new Point(i + 10, visY + 2));
					//}
					if (i % OneCm == 0) //<- Mayor Line (6px width) each 10 pixels
					{
						e.Graphics.DrawLine(pen, new Point(i + 10, visY), new Point(i + 10, visY + 6));
					}

					//if (i % 5 == 0) // <- Minor line (3px width) each 5 pixels
					//{
					//	e.Graphics.DrawLine(pen, new Point(i, visY), new Point(i, visY + 2));
					//}
					//if (i % 10 == 0) //<- Mayor Line (6px width) each 10 pixels
					//{
					//	e.Graphics.DrawLine(pen, new Point(i, visY), new Point(i, visY + 6));
					//}
					//if (i % 50 == 0) //<- Number text each 50 pixels
					//{
					//	if (i > 0)
					//	{
					//		int spam = (i < 100) ? 8 : 10; //<- Horizontal span for the text, text size dependant
					//		spam = (i >= 1000) ? 14 : spam; //<- for 1.000px and more

					//		e.Graphics.DrawString(i.ToString(), this.Font, brush, new Point(i - spam, visY + 7));
					//	}
					//}
				}
			}					
			*/
		}
		private void pictureBox1_MouseHover(object sender, EventArgs e)
		{
			//ToolTip tt = new ToolTip();
			//tt.SetToolTip(this.pictureBox1, "Printable Area");
		}

		private void cmdTreeCommands_Collapse_Click(object sender, EventArgs e)
		{
			this.cTreeView1.CollapseAll();
		}
		private void cmdTreeCommands_Expand_Click(object sender, EventArgs e)
		{
			this.cTreeView1.ExpandAll();
		}
		private void cmdTreeCommands_Settings_Click(object sender, EventArgs e)
		{
			//TODO
		}
		private void cmdTreeCommands_Click(object sender, EventArgs e)
		{

		}

		private void mnuFile_OpenGdrive_Click(object sender, EventArgs e)
		{
			/* IMPORTS A FILE FROM GoogleDrive */
			GDriveForm _Form = new GDriveForm(true);
			_Form.CurrentLanguage = this.CurrentLanguage;

			if (_Form.ShowDialog() == DialogResult.OK)
			{
				Document_Open(_Form.FilePath);

				if (this.Document != null)
				{
					this.Document.Metadata.GoogleDrive = new GDriveInfo()
					{
						FileID = _Form.API.FileData.Id,
						Name = _Form.API.FileData.Name,
						LastUpdate = Convert.ToDateTime(_Form.API.FileData.ModifiedTimeRaw)
					};
					Document_Save(ReLoadDoc: false);
				}
			}
		}
		private void mnuFile_SaveGDrive_Click(object sender, EventArgs e)
		{
			if (this.Document != null)
			{
				GDriveForm _Form = new GDriveForm(false)
				{
					DriveInfo = this.Document.Metadata.GoogleDrive,
					CurrentLanguage = this.CurrentLanguage,					
					FilePath = this.CurrentFile
				};
				if (_Form.ShowDialog() == DialogResult.OK)
				{
					if (this.Document != null)
					{
						this.Document.Metadata.GoogleDrive = new GDriveInfo()
						{
							FileID = _Form.API.FileData.Id,
							Name = _Form.API.FileData.Name,
							LastUpdate = DateTime.Now
						};
						Document_Save(ReLoadDoc: false);
					}
				}
			}			
		}
	}
}
