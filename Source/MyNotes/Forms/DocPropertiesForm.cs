using System;
using System.Windows.Forms;

namespace MyNotes.Forms
{
	public partial class DocPropertiesForm : Form
	{
		public DocMetadata MetaData { get; set; }

		public DocPropertiesForm(DocMetadata pMetaData)
		{
			InitializeComponent();
			MetaData = pMetaData;
		}

		private void DocPropertiesForm_Load(object sender, EventArgs e)
		{
			if (MetaData != null)
			{
				propertyGrid1.SelectedObject = MetaData;
			}
		}
		private void DocPropertiesForm_Shown(object sender, EventArgs e)
		{

		}
		private void DocPropertiesForm_FormClosing(object sender, FormClosingEventArgs e)
		{

		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}
		private void cmdOK_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
		}
	}
}
