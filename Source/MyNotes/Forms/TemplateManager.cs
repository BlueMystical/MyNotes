using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyNotes
{
	public partial class TemplateManager : Form
	{
		public Traductor CurrentLanguage { get; set; }
		private string AppExePath = AppDomain.CurrentDomain.BaseDirectory;

		private List<MyTemplate> Templates { get; set; }
		private MyTemplate CurrentTemplate { get; set; }

		#region Contructors

		public TemplateManager()
		{
			InitializeComponent();
		}
		private void TemplateManager_Load(object sender, EventArgs e)
		{
			LoadTemplates();
			LoadSettings();
		}
		private void TemplateManager_Shown(object sender, EventArgs e)
		{
			ShowCurrentTemplate();
		}
		private void TemplateManager_FormClosing(object sender, FormClosingEventArgs e)
		{
			
		}

		#endregion

		#region Methods

		public void LoadSettings()
		{
			try
			{
				if (CurrentLanguage != null)
				{
					string translatedText = CurrentLanguage.Translations["SaveFile"];
					cmdSaveTemplate.Image = IconChar.FloppyDisk.ToBitmap(16, 16, Color.Black);
					cmdSaveTemplate.Text = translatedText; cmdSaveTemplate.ToolTipText = translatedText;

					menuTemplates.Text = CurrentLanguage.Translations["Templates"];
					grpTemplateDetail.Text = CurrentLanguage.Translations["Definition"];
					lblTemplate_Name.Text = CurrentLanguage.Translations["Name"];
					lblTemplate_Author.Text = CurrentLanguage.Translations["Author"];
					lblTemplate_Description.Text = CurrentLanguage.Translations["Description"];
					lblTemplate_Preview.Text = CurrentLanguage.Translations["Preview"];
					lblTemplate_Content.Text = CurrentLanguage.Translations["Content"];
					//.Text = CurrentLanguage.Translations["Name"];
					//.Text = CurrentLanguage.Translations["Name"];
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		public void LoadTemplates()
		{
			try
			{
				if (File.Exists(Path.Combine(AppExePath, "Templates.json")))
				{
					Templates = Util.DeSerialize_FromJSON<List<MyTemplate>>(Path.Combine(AppExePath, "Templates.json"));
					if (Templates != null && Templates.Count > 0)
					{
						menuTemplates.DropDownItems.Clear();

						int Index = 0;
						foreach (var item in Templates)
						{							
							var _menu = menuTemplates.DropDownItems.Add(item.Name) as ToolStripMenuItem;
							_menu.Click += Template_Click;
							item.Index = Index;
							_menu.Tag = item;
							Index++;
						}
						this.CurrentTemplate = Templates[0];
					}
				}
				else
				{
					CreateDT();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		public void ShowCurrentTemplate()
		{
			try
			{
				if (CurrentTemplate != null)
				{
					dataGridView1.DataSource = CurrentTemplate.Data;

					txtTemplate_Name.Text = CurrentTemplate.Name;
					txtTemplate_Author.Text = CurrentTemplate.Author;
					txtTemplate_Description.Text = CurrentTemplate.Description;

					richTextBox1.Rtf = Util.CreateSimpleRTF( 
						Util.ReplaceAndTranslate(
							CurrentTemplate.Data.Rows[0]["content"].ToString(), CurrentLanguage
							));
					BuildTree();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public void CreateDT()
		{
			try
			{
				Templates = new List<MyTemplate>();

				CurrentTemplate.Data = new DataTable("book_template");

				CurrentTemplate.Data.Columns.Add("content", typeof(String));
				CurrentTemplate.Data.Columns.Add("first_level", typeof(String));
				CurrentTemplate.Data.Columns.Add("second_level", typeof(String));
				CurrentTemplate.Data.Columns.Add("third_level", typeof(String));
				CurrentTemplate.Data.Columns.Add("fourth_level", typeof(String));
				//CurrentTemplate.Data.Columns.Add("fifth_level", typeof(String));
				//CurrentTemplate.Data.Columns.Add("sixth_level", typeof(String));

				CurrentTemplate.Data.Rows.Add("[LorenIpsum]", "[Chapter] 1", string.Empty, string.Empty, string.Empty);
				CurrentTemplate.Data.Rows.Add("[LorenIpsum]", string.Empty, "[Section] A",  string.Empty, string.Empty);
				CurrentTemplate.Data.Rows.Add("[LorenIpsum]", string.Empty, "[Section] B", string.Empty, string.Empty);
				CurrentTemplate.Data.Rows.Add("[LorenIpsum]", string.Empty, "[Section] C", string.Empty, string.Empty);
				CurrentTemplate.Data.Rows.Add("[LorenIpsum]", string.Empty, string.Empty, "[Entry] 1",  string.Empty);
				CurrentTemplate.Data.Rows.Add("[LorenIpsum]", "[Chapter] 2", string.Empty, string.Empty, string.Empty);
				CurrentTemplate.Data.Rows.Add("[LorenIpsum]", "[Chapter] 3", string.Empty, string.Empty, string.Empty);

				dataGridView1.DataSource = CurrentTemplate.Data;
				richTextBox1.Text = Util.ReplaceAndTranslate("[LorenIpsum]", CurrentLanguage);

				//Templates.Add

				BuildTree();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		public void BuildTree()
		{
			try
			{
				if (this.CurrentTemplate.Data != null && this.CurrentTemplate.Data.Rows.Count > 0)
				{
					string Chapter = CurrentLanguage.Translations["Chapter"];
					string Section = CurrentLanguage.Translations["Section"];
					string Entry = CurrentLanguage.Translations["Entry"];
					string LorenIpsum = CurrentLanguage.Translations["LorenIpsum"];

					var Document = new List<DocContent>();

					string F1 = string.Empty; DocContent C1 = new DocContent();
					string F2 = string.Empty; DocContent C2 = new DocContent();
					string F3 = string.Empty; DocContent C3 = new DocContent();
					string F4 = string.Empty; DocContent C4 = new DocContent();

					Util.ReplaceAndTranslate("[Chapter] 1, [Section] B, [Entry] 4:", CurrentLanguage);

					foreach (DataRow DR in CurrentTemplate.Data.Rows)
					{
						if (DR["first_level"].ToString() != F1)
						{
							string val = DR["first_level"].ToString();
							if (!string.IsNullOrEmpty(val) && val != F1)
							{
								F1 = DR["first_level"].ToString();
								C1 = Document.AddChild(Util.ReplaceAndTranslate(F1, CurrentLanguage));
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


					if (Document != null)
					{
						treeView1.Nodes.Clear();
						int i = 0;
						foreach (var _root in Document)
						{
							treeView1.Nodes.Add(Util.CreateTreeNode(_root, string.Format("{0:D2}", i)));
							i++;
						}
						treeView1.ExpandAll();
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		#endregion

		#region Control Events

		private void Template_Click(object sender, EventArgs e)
		{
			var Menu = sender as ToolStripMenuItem;
			this.CurrentTemplate = Menu.Tag as MyTemplate;
			ShowCurrentTemplate();
		}

		private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				DataRow DR = ((DataRowView)dataGridView1.Rows[e.RowIndex].DataBoundItem).Row;
				if (DR != null)
				{
					string Content = DR["content"].ToString(); 
					richTextBox1.Rtf = Util.CreateSimpleRTF(
						Util.ReplaceAndTranslate(
							Content, CurrentLanguage ));
				}
			}
			catch { }
			BuildTree();
		}

		private void cmdSaveTemplate_Click(object sender, EventArgs e)
		{
			CurrentTemplate.Name = txtTemplate_Name.Text;
			CurrentTemplate.Author = txtTemplate_Author.Text;
			CurrentTemplate.Description = txtTemplate_Description.Text;

			Templates[CurrentTemplate.Index] = CurrentTemplate;

			Util.Serialize_ToJSON(Path.Combine(AppExePath, "Templates.json"), Templates);
			lblStatus.Text = "Template Saved!";
		}


		private void insertRowToolStripMenuItem_Click(object sender, EventArgs e)
		{
			/* INSERTS A NEW ROW JUST ABOVE THE CURRENT CELL  */
			int idx = dataGridView1.CurrentCell.RowIndex;
			var table = (DataTable)dataGridView1.DataSource;
			var row = table.NewRow();
			table.Rows.InsertAt(row, idx);
		}
		private void deleteRowToolStripMenuItem_Click(object sender, EventArgs e)
		{
			int idx = dataGridView1.CurrentCell.RowIndex;
			var table = (DataTable)dataGridView1.DataSource;
			if (MessageBox.Show(CurrentLanguage.Translations["DeleteRow"], 
								CurrentLanguage.Translations["Confirm"], 
								MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				table.Rows.RemoveAt(idx);
			}
		}

		#endregion
	}
}
