using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyNotes.Forms
{
	public partial class LinkForm : Form
	{
		public string LinkRTF { get; set; } = string.Empty;
		public string FriendlyName = string.Empty;
		public string HyperLink { get; set; } = string.Empty;
		public KeyValue LinkType { get; set; }

		private List<KeyValue> HTypes = null;
		private bool NoSelection = false;

		private const string rtf_font = @"{\fonttbl{\f0\fswiss\fprq2\fcharset0 Arial;}{\f1\fnil\fcharset0 Arial;}}";
		private const string rtf_fieldHyper = @"{\*\generator Riched20 10.0.19041}\viewkind4\uc1\pard\widctlpar\sa200\sl276\slmult1 {\f0\fs24\lang11274{\field{\*\fldinst{HYPERLINK " + "\"";
		private const string rtf_fieldFrienName = @"}}{\fldrslt{\ul\cf1\cf1\ul ";
		private const string rtf_closeFields = "}}}";

		private const string RTF_EMPTY_TAG = @"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033{\fonttbl{\f0\fnil\fcharset0 Arial; }}\uc1\pard\sa200\sl276\slmult1\fs24\lang10 @RTFLINK }";
		
		private const string RTF_LINK_TEMPLATE = @"{{\field{\*\fldinst{HYPERLINK @URL_TEXT }}{\fldrslt{@FRIENDLY_NAME }}}}\f0\fs22";

		public LinkForm()
		{
			InitializeComponent();
		}
		public LinkForm(string SelectedText)
		{
			InitializeComponent();
			FriendlyName = SelectedText;
			NoSelection = string.IsNullOrEmpty(SelectedText);
		}

		private void LinkForm_Load(object sender, EventArgs e)
		{
			HTypes = new List<KeyValue>
			{
				new KeyValue("01", "URL"),
				new KeyValue("02", "Inner Content"),
				new KeyValue("03", "File System")
			};
			LinkType = HTypes[0];
		}
		private void LinkForm_Shown(object sender, EventArgs e)
		{
			this.cboHType.DataSource = HTypes;
			this.cboHType.SelectedItem = LinkType;

			this.txtFriendlyName.Text = FriendlyName;
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			FriendlyName = this.txtFriendlyName.Text;
			HyperLink = this.txtURL.Text;

			switch (LinkType.Key)
			{
				case "01": break;
				case "02": break;
			}

			LinkRTF = RTF_LINK_TEMPLATE.Replace("@URL_TEXT", HyperLink).Replace("@FRIENDLY_NAME", FriendlyName);

			if (NoSelection)
			{
				LinkRTF = RTF_EMPTY_TAG.Replace("@RTFLINK", LinkRTF);
			}

			this.DialogResult = DialogResult.OK;
		}

		private void cboHType_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.cboHType.SelectedItem != null)
			{
				LinkType = this.cboHType.SelectedItem as KeyValue;
			}			
		}
	}
}
