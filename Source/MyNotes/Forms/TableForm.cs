using System;
using System.Windows.Forms;

namespace MyNotes.Forms
{
	public partial class TableForm : Form
	{
		public int ColumnCount { get; set; } = 2;
		public int RowCount { get; set; } = 2;

		public TableForm()
		{
			InitializeComponent();
		}
		public TableForm(int pColumnCount, int pRowCount)
		{
			InitializeComponent();
			ColumnCount = pColumnCount;
			RowCount = pRowCount;
		}

		private void TableForm_Load(object sender, EventArgs e)
		{
			
		}
		private void TableForm_Shown(object sender, EventArgs e)
		{
			txColumnCount.Value = ColumnCount;
			txRowCount.Value = RowCount;
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			this.ColumnCount =	Convert.ToInt32(txColumnCount.Value);
			this.RowCount =		Convert.ToInt32(txRowCount.Value);

			this.DialogResult = DialogResult.OK;
		}

		
	}
}
