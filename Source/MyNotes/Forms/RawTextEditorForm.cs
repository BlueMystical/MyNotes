using System;
using System.Windows.Forms;

namespace MyNotes.Forms
{
	public partial class RawTextEditorForm : Form
	{
		public string RawText { get; set; } = string.Empty;
		public string PlainText { get; private set; } = string.Empty;

		public RawTextEditorForm()
		{
			InitializeComponent();
		}
		public RawTextEditorForm(string pRawText)
		{
			InitializeComponent();
			RawText = pRawText;
		}

		private void RawTextEditorForm_Load(object sender, EventArgs e)
		{

		}
		private void RawTextEditorForm_Shown(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(RawText))
			{
				textBox1.Text = RawText;
				richTextBox1.Rtf = RawText;
			}
		}
		private void RawTextEditorForm_FormClosing(object sender, FormClosingEventArgs e)
		{

		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			this.RawText = textBox1.Text;
			this.DialogResult = DialogResult.OK;
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			richTextBox1.Rtf = textBox1.Text;
		}

		private void chkWordWrap_CheckedChanged(object sender, EventArgs e)
		{
			textBox1.WordWrap = chkWordWrap.Checked;
			//richTextBox1.WordWrap = chkWordWrap.Checked;
		}
	}
}
