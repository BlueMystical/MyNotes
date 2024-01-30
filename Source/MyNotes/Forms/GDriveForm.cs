using MyNotes.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyNotes.Forms
{
	public partial class GDriveForm : Form
	{
		/// <summary>'true': Load File from GDrive, 'false': Save File to GDrive.</summary>
		public bool IsLoadMode { get; set; } = true;
		public string FilePath { get; set; } = string.Empty;

		public Traductor CurrentLanguage { get; set; }

		public GDriveNET API = null;
		public GDriveInfo DriveInfo { get; set; }

		private bool FolderCreated = false;

		public GDriveForm(bool pLoadMode = true)
		{
			InitializeComponent();
			IsLoadMode = pLoadMode;
		}

		private void GDriveForm_Load(object sender, EventArgs e)
		{
			tabControl1.TabPages.Remove(tabLoadFile);
			tabControl1.TabPages.Remove(tabSaveFile);

			tabLoadFile.Text = CurrentLanguage.GetTranslation("GDrive_Open");
			tabSaveFile.Text = CurrentLanguage.GetTranslation("GDrive_Save");

			lblTrademark.Text = CurrentLanguage.GetTranslation("GDrive_TradeMark");
			lblAccount.Text = CurrentLanguage.GetTranslation("GDrive_Account");

			Load_lblSelectFile.Text = CurrentLanguage.GetTranslation("GDrive_SelectFile");
			Load_cmdSearch.Text = CurrentLanguage.GetTranslation("Find");
			Load_cmdSearch.Image = (Bitmap)Properties.Resources.ResourceManager.GetObject("find_16x16");

			Save_lblSelectFile.Text = CurrentLanguage.GetTranslation("GDrive_SelectFolder");
			Save_cmdSearch.Text = CurrentLanguage.GetTranslation("Find");
			Save_cmdSearch.Image = (Bitmap)Properties.Resources.ResourceManager.GetObject("find_16x16");
			Save_cmdCreateFolder.Text = CurrentLanguage.GetTranslation("CreateFolder").Split('|')[0];
			Save_cmdCreateFolder.Image = (Bitmap)Properties.Resources.ResourceManager.GetObject("open_16x16");

			cmdOK.Enabled = false;

			string FileLabels = CurrentLanguage.GetTranslation("FileLabels");
			if (!string.IsNullOrEmpty(FileLabels))
			{				
				string[] Palabras = FileLabels.Split(new char[] { '|' }); //<-	File|Folder|Size|Created|Modified

				colLoadName.Text = Palabras[0];
				colSaveFolder.Text = Palabras[1];
				colLoadSize.Text = Palabras[2];

				colLoadModified.Text = Palabras[4];
				colSaveModified.Text = Palabras[4];
			}
		}
		private void GDriveForm_Shown(object sender, EventArgs e)
		{
			try
			{
				this.Cursor = Cursors.WaitCursor;
				API = new GDriveNET();
				API.APP_NAME = Application.ProductName;
				API.ClientSecrets = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "client_secrets.json");

				if (Directory.Exists(API.LOCAL_STORAGE))
				{
					//Check the last logged user:
					try
					{
						FileInfo myCredentials = new DirectoryInfo(API.LOCAL_STORAGE)
							.GetFiles("*.*").OrderByDescending(f => f.LastWriteTime).First();

						if (myCredentials != null)
						{
							var partes = myCredentials.Name.Split(new char[] { '-' }).ToList();
							txtUserAccount.Text = partes[1];

							cmdLogin_Click(sender, e);
						}
					}
					catch { }					
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { this.Cursor = Cursors.Default; }					
		}
		private void GDriveForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			API.Dispose();
		}

		public async void LoadFiles(string Filter = "*.note")
		{
			try
			{
				this.Cursor = Cursors.WaitCursor;

				System.Collections.Generic.List<Google.Apis.Drive.v3.Data.File> myFiles = await API.FindFiles(Filter);

				if (myFiles != null)
				{
					listCloudFiles.BeginUpdate();
					listCloudFiles.Items.Clear();

					foreach (var file in myFiles)
					{
						var newItem = listCloudFiles.Items.Add(file.Name);  //<- First Column
							newItem.SubItems.Add(Util.GetFileSize(file.Size != null ? file.Size.Value : 0));    //<- Second Column
							newItem.SubItems.Add(Convert.ToDateTime(file.ModifiedTimeRaw).ToShortDateString()); //<- Third Column
							newItem.Tag = file; //<- The Data
							newItem.ImageIndex = 1; //<- The icon

						if (file.MimeType == "application/vnd.google-apps.folder")
						{
							newItem.ImageIndex = 2;
						}
						else if (file.MimeType == "application/octet-stream")
						{
							newItem.ImageIndex = file.Name.EndsWith(".note") ? 3 : 1;
						}
					}

					listCloudFiles.EndUpdate();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { this.Cursor = Cursors.Default; }
		}
		public async void LoadFolders(string Filter = "Document")
		{
			try
			{
				this.Cursor = Cursors.WaitCursor;

				System.Collections.Generic.List<Google.Apis.Drive.v3.Data.File> myFiles = await API.FindFolders(Filter);

				if (myFiles != null)
				{
					listCloudFolders.Cursor = Cursors.WaitCursor;
					listCloudFolders.BeginUpdate();
					listCloudFolders.Items.Clear();

					foreach (var file in myFiles)
					{
						var newItem = listCloudFolders.Items.Add(file.Name);  //<- First Column
							newItem.SubItems.Add(file.Description);    //<- Second Column
							newItem.SubItems.Add(Convert.ToDateTime(file.ModifiedTimeRaw).ToShortDateString()); //<- Third Column
							newItem.Tag = file; //<- The Data
							newItem.ImageIndex = 1; //<- The icon

						colSaveDetails.Text = CurrentLanguage.GetTranslation("NameValue").Split('|')[1]; //<- Name|Description|Value
						colSaveModified.Text = CurrentLanguage.GetTranslation("FileLabels").Split('|')[4]; //<- File|Folder|Size|Created|Modified

						if (file.MimeType == "application/vnd.google-apps.folder")
						{
							newItem.ImageIndex = 2;
						}
						else if (file.MimeType == "application/octet-stream")
						{
							newItem.ImageIndex = file.Name.EndsWith(".note") ? 3 : 1;
						}
					}

					listCloudFolders.EndUpdate();
					listCloudFolders.Cursor = Cursors.Default;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { this.Cursor = Cursors.Default; }
		}

		private async void cmdLogin_Click(object sender, EventArgs e)
		{
			try
			{
				this.Cursor = Cursors.WaitCursor;

				if (await API.Authenticate(txtUserAccount.Text))
				{
					switch (IsLoadMode)
					{
						case true: tabControl1.TabPages.Add(tabLoadFile); break;
						case false: tabControl1.TabPages.Add(tabSaveFile); break;
					}
					cmdLogin.Enabled = false;

					if (IsLoadMode) //<- 'true': Load File from GDrive
					{
						LoadFiles();
					}
					else //<- 'false': Save File to GDrive.
					{
						// If the File to save already has GDrive Information..
						if (DriveInfo != null)
						{
							// Retrieve the info of the file on the Cloud
							var _file = API.GetFile(DriveInfo.FileID);
							if (_file != null)
							{
								Save_txtSearch.Text = System.IO.Path.GetDirectoryName(API.FileData.Name);

								// And show the file on the list:
								listCloudFolders.BeginUpdate();
								listCloudFolders.Items.Clear();

								var newItem = listCloudFolders.Items.Add(API.FileData.Name);  //<- First Column
									newItem.SubItems.Add(Util.GetFileSize(API.FileData.Size != null ? API.FileData.Size.Value : 0));    //<- Second Column
									newItem.SubItems.Add(Convert.ToDateTime(API.FileData.ModifiedTimeRaw).ToShortDateString()); //<- Third Column
									newItem.Tag = API.FileData; //<- The Data
									newItem.ImageIndex = 1; //<- The icon

								if (API.FileData.MimeType == "application /vnd.google-apps.folder")
								{
									newItem.ImageIndex = 2;
								}
								else if (API.FileData.MimeType == "application/octet-stream")
								{
									newItem.ImageIndex = API.FileData.Name.EndsWith(".note") ? 3 : 1;
								}
								listCloudFolders.EndUpdate();
							}
						}
						else // If the file doesnt have GDrive information..
						{
							LoadFolders();
						}						
					}					
				}
			}
			catch (Exception ex)
			{
				cmdLogin.Enabled = true;
				MessageBox.Show(ex.Message, "ERROR:", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { this.Cursor = Cursors.Default; cmdOK.Enabled = true; }
		}

		private void Load_cmdSearch_Click(object sender, EventArgs e)
		{
			LoadFiles(this.Load_txtSearch.Text);
		}
		private void Save_cmdSearch_Click_1(object sender, EventArgs e)
		{
			LoadFolders(Save_txtSearch.Text);
		}
		private async void Save_cmdCreateFolder_ClickAsync(object sender, EventArgs e)
		{
			this.Cursor = Cursors.WaitCursor;
			try
			{
				var Titulos = CurrentLanguage.GetTranslation("CreateFolder").Split('|'); //<-  New Folder..|Folder Details
				var Etiquetas = CurrentLanguage.GetTranslation("NameValue").Split('|');  //<-  Name|Description|Value

				List<KeyValue> Input = new List<KeyValue>
				{
					new KeyValue(Etiquetas[0], "Documents"),
					new KeyValue(Etiquetas[1], string.Empty)
				};
				if (Util.InputBox(Titulos[0], Titulos[1], ref Input) == DialogResult.OK)
				{
					var _NewFolder = await API.CreateFolder(Input[0].Value, Input[1].Value);
					if (_NewFolder != null)
					{
						FolderCreated = true;
						listCloudFolders.BeginUpdate();
						listCloudFolders.Items.Clear();

						var newItem = listCloudFolders.Items.Add(_NewFolder.Name);  //<- First Column
							newItem.SubItems.Add(Util.GetFileSize(_NewFolder.Size != null ? _NewFolder.Size.Value : 0));    //<- Second Column
							newItem.SubItems.Add(Convert.ToDateTime(_NewFolder.ModifiedTimeRaw).ToShortDateString()); //<- Third Column
							newItem.Tag = _NewFolder; //<- The Data
							newItem.ImageIndex = 1; //<- The icon

						if (_NewFolder.MimeType == "application /vnd.google-apps.folder")
						{
							newItem.ImageIndex = 2;
						}
						else if (_NewFolder.MimeType == "application/octet-stream")
						{
							newItem.ImageIndex = _NewFolder.Name.EndsWith(".note") ? 3 : 1;
						}
						listCloudFolders.EndUpdate();
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "ERROR:", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { this.Cursor = Cursors.Default; }
		}

		private async void cmdOK_Click(object sender, EventArgs e)
		{
			try
			{
				this.Cursor = Cursors.WaitCursor;

				ListViewItem SelectedFile = null;

				API.OnProgressChange += (object ProgPercentage, EventArgs _e) =>
				{
					progressBar1.Value = Convert.ToInt32(ProgPercentage);
				};

				if (IsLoadMode) //<- 'true': Load File from GDrive
				{
					SelectedFile = (listCloudFiles.SelectedItems.Count > 0) ? listCloudFiles.SelectedItems[0] : listCloudFiles.Items[0];

					if (SelectedFile != null && SelectedFile.Tag != null)
					{
						if (SelectedFile.Tag is Google.Apis.Drive.v3.Data.File _File)
						{
							//var _file = API.GetFile(DriveInfo.FileID);
							this.FilePath = await API.DownloadFile(_File.Id);
							if (!string.IsNullOrEmpty(FilePath))
							{
								//File Donwloaded!!!
								this.DialogResult = DialogResult.OK;
							}
						}
					}
				}
				else //<- 'false': Save File to GDrive.
				{
					SelectedFile = (listCloudFolders.SelectedItems.Count > 0) ? listCloudFolders.SelectedItems[0] : listCloudFolders.Items[0];
					if (SelectedFile != null && SelectedFile.Tag != null)
					{
						if (SelectedFile.Tag is Google.Apis.Drive.v3.Data.File GDfile)
						{
							if (GDfile.MimeType == "application/vnd.google-apps.folder")
							{
								await API.CreateFile(this.FilePath, GDfile.Id);
							}
							else
							{
								if (DriveInfo != null)
								{
									await API.UpdateFile(this.FilePath, GDfile.Id);
								}
							}
							this.DialogResult = DialogResult.OK;
						}
					}
					else
					{
						return;
					}
				}
				
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { this.Cursor = Cursors.Default; }
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		
	}
}
