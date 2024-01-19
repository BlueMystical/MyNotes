using FontAwesome.Sharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MyNotes.PropertyHelper;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using ControlTreeView;

namespace MyNotes
{
	/// <summary>This stores the whole Document.</summary>
	public class MyNoteDocument
	{
		public MyNoteDocument() 
		{
			Metadata = new DocMetadata();
			Content = new List<DocContent>();
		}
		public MyNoteDocument(string pTitle, string pAuthor)
		{
			Metadata = new DocMetadata(pTitle, pAuthor);
			Content = new List<DocContent>();
		}

		/// <summary>Metadata for the Document.</summary>
		public DocMetadata Metadata { get; set; }

		/// <summary>The actual Data of the document.</summary>
		public List<DocContent> Content { get; set; }
	}
	/// <summary>Metadata for the Document.</summary>
	public class DocMetadata
	{
		public DocMetadata() { }
		public DocMetadata(string pTitle, string pAuthor) 
		{
			Title = pTitle;
			Author = pAuthor;
		}

		[TypeConverter(typeof(StringListConverter))]
		public string Language { get; set; } = "en";

		public string Title { get; set; } = "[Untitled]";

		[Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
		public string Series { get; set; } = string.Empty;

		public string Author { get; set; } = "[Unknown]";

		[TypeConverter(typeof(CsvConverter))]
		[Editor(typeof(MyStringCollectionEditor), typeof(UITypeEditor))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public List<string> Contributors { get; set; }

		[TypeConverter(typeof(CsvConverter))]
		[Editor(typeof(MyStringCollectionEditor), typeof(UITypeEditor))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public List<string> Translators { get; set; }

		[TypeConverter(typeof(CsvConverter))]
		[Editor(typeof(MyStringCollectionEditor), typeof(UITypeEditor))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public List<string> Identifiers { get; set; }

		[TypeConverter(typeof(CsvConverter))]
		[Editor(typeof(MyStringCollectionEditor), typeof(UITypeEditor))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public List<string> Publisher { get; set; }

		[TypeConverter(typeof(CsvConverter))]
		[Editor(typeof(MyStringCollectionEditor), typeof(UITypeEditor))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public List<string> Keywords { get; set; }

		[TypeConverter(typeof(CsvConverter))]
		[Editor(typeof(MyStringCollectionEditor), typeof(UITypeEditor))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public List<string> Categories { get; set; }

		[Editor(typeof(BitmapLocationEditor), typeof(UITypeEditor))]
		public string Cover { get; set; }

		[Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
		public string Summary { get; set; } = string.Empty;

		public GDriveInfo GoogleDrive { get; set; } = null;

		public override string ToString()
		{
			return String.Format("'{0}' by {1}", Title, Author);
		}
	}
	
	/// <summary>The actual Data of the document.</summary>
	public class DocContent
	{
		public DocContent() { }

		public DocContent(int pID, string pName = "", string pContent = "")
		{
			Title = pName;
			Content = pContent;
		}

		/// <summary>[Internal Use Only] ID for Node elements.</summary>
		[JsonIgnore]
		public string ID { get; set; }

		/// <summary>Visible Name (Label) for the Element.</summary>
		public string Title { get; set; } = string.Empty;

		/// <summary>RTF Text displayed on the RichTextBox control.</summary>
		public string Content { get; set; } = string.Empty;

		/// <summary>Child elements of this one.</summary>
		public List<DocContent> Childs { get; set; } = new List<DocContent>();

		public override string ToString()
		{
			return String.Format("'{0}'", Title);
		}
	}

	/// <summary>Integration of the Document with Google Drive.</summary>
	public class GDriveInfo
	{
		public GDriveInfo() { }

		public string FileID { get; set; }
		public string Name { get; set; }
		public DateTime LastUpdate { get; set; }
	}

	/// <summary>Generic Class to Translate UI elements into several languages.</summary>
	public class Traductor
	{
		public Traductor() { }
		public Traductor(string pLangCode, string pLanguage)
		{
			Language = pLanguage;
			LangCode = pLangCode;
			Translations = new Dictionary<string, string>();
		}

		/// <summary>Name of the Language (in native lang.)</summary>
		public string Language { get; set; } = "English";

		/// <summary>ISO 639-1 Language Code</summary>
		public string LangCode { get; set; } = "en";

		/// <summary>Key of the item you want to translate and the Value is the translated text.</summary>
		public Dictionary<string, string> Translations { get; set; }

		/// <summary>Returns the translated Key or the same if not found.</summary>
		/// <param name="pKey">Key of the Word/Phrase to translate.</param>
		public string GetTranslation(string pKey)
		{
			string _ret = pKey;
			try
			{
				_ret = Translations[pKey];
			}
			catch { }
			return _ret;
		}

		public override string ToString()
		{
			return String.Format("{0} ({1})", Language, LangCode);
		}
	}

	/// <summary>Template for Documents.</summary>
	public class MyTemplate
	{
		public MyTemplate() { }

		public string Name { get; set; }
		public string Description { get; set; }
		public string Author { get; set; }

		public System.Data.DataTable Data { get; set; }

		[JsonIgnore]
		public int Index { get; set; }
	}

	/// <summary>Store Results from a Search.</summary>
	public class SearchResults
	{
		public SearchResults(string pSearchKey) 
		{
			SearchKey = pSearchKey;
		}

		public string SearchKey { get; set; } = string.Empty;

		public List<FindResult> FoundNodes { get; set; }

		public int TotalResults { get; set; } = 0;
		public int GlobalIndex { get; set; } = 0;
		public int LocalIndex { get; set; } = 0;
		public int CurrentIndex { get; set; } = 0;

		public bool FindMatchCase { get; set; }
		public bool FindWholeWord { get; set; }

		/// <summary>Starts a new Search in all Nodes and Sub-Nodes of the TreeView Control.</summary>
		/// <param name="treeView">The Control holding the data</param>
		public List<FindResult> SearchAllNodes(CTreeView treeView)
		{
			FoundNodes = new List<FindResult>();

			// Recursively search the TreeView control for matching nodes.
			void SearchNodes(CTreeNode node)
			{
				foreach (CTreeNode childNode in node.Nodes)
				{
					List<int> Finds = SearchInNodeEx(childNode);
					if (Finds != null)
					{
						FoundNodes.Add(new FindResult(childNode, Finds));
					}
					SearchNodes(childNode);
				}
			}

			// Start the searching all root nodes:		
			foreach (CTreeNode node in treeView.Nodes)
			{
				SearchNodes(node);
			}

			foreach (var item in FoundNodes)
			{
				TotalResults += item.TextIndexes.Count;
			}
			return FoundNodes;
		}

		/// <summary>Search in RTF data stored in the 'tag' property of the Node.</summary>
		/// <param name="pNode">TreeView Node</param>
		/// <returns>Return all the Indexes found in the RTF.</returns>
		private List<int> SearchInNodeEx(CTreeNode pNode)
		{
			List<int> _ret = null; //<- Return the indexes of all finds.
			try
			{
				if (pNode.Tag != null && pNode.Tag is DocContent)
				{
					string RTFContent = (pNode.Tag as DocContent).Content;
					if (!string.IsNullOrEmpty(RTFContent))
					{
						// using a hidden RTB to temporarily set the RTF and seach on it
						using (RichTextBoxEx RTB = new RichTextBoxEx())
						{
							RTB.Rtf = RTFContent;

							if (RTFContent.Contains(@"{\rtf1")) { RTB.Rtf = RTFContent; }
							else { RTB.Text = RTFContent; }

							var Options = RichTextBoxFinds.None;
							if (FindMatchCase) Options |= RichTextBoxFinds.MatchCase;
							if (FindWholeWord) Options |= RichTextBoxFinds.WholeWord;

							bool KeepSearching = true;
							int FIndex = 0;
							do
							{
								FIndex = RTB.Find(SearchKey, FIndex, Options);
								if (FIndex > 0)
								{
									if (_ret is null) _ret = new List<int>();
									_ret.Add(FIndex);
									FIndex += SearchKey.Length;
								}
								else
								{
									KeepSearching = false;
								}
							} while (KeepSearching);
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}
	}
	public class FindResult
	{
		public FindResult() { }
		public FindResult(CTreeNode pNode, List<int> Finds)
		{
			Node = pNode;
			TextIndexes = Finds;
		}
		public CTreeNode Node { get; set; }
		public List<int> TextIndexes { get; set; }
	}

	public class KeyValue
	{
		public KeyValue() { }
		public KeyValue(string pKey, string pValue, ValueTypes pType = 0)
		{
			Key = pKey;
			Value = pValue;
			ValueType = pType;
		}

		public string Key { get; set; }
		public string Value { get; set; }

		public ValueTypes ValueType { get; set; } = ValueTypes.String;

		public override string ToString()
		{
			return string.Format("{0} - {1}", Key, Value);
		}

		public List<KeyValue> DataSet { get; set; }

		public enum ValueTypes
		{
			String = 0,
			Integer = 1,
			Decimal = 2,
			Date = 3,
			Boolean = 4,
			Dynamic
		}
	}

	public static class Util
	{
		#region Extension Methods

		public static DocContent AddChild(this List<DocContent> pParent, string pName, string pContent = "")
		{
			DocContent _ret = new DocContent(pParent.Count, pName) { Content = pContent };  //<- Null Parent is a Root Element
			pParent.Add(_ret);
			return _ret;
		}
		public static DocContent AddChild(this DocContent pParent, string pName, string pContent = "")
		{
			DocContent _ret = new DocContent(pParent.Childs.Count, pName) { Content = pContent };
			pParent.Childs.Add(_ret);
			return _ret;
		}

		/// <summary>Agrega un nuevo Lenguaje a la lista de idiomas.</summary>
		/// <param name="Idiomas">Lista de Idiomas</param>
		/// <param name="LangCode">ISO 639-1 Language Code</param>
		/// <param name="Language">Name of the Language (in native lang.)</param>
		/// <returns>The added Language</returns>
		public static Traductor AddLanguage(this List<Traductor> Idiomas, string LangCode, string Language)
		{
			Traductor _ret = new Traductor(LangCode, Language);
			Idiomas.Add(_ret);
			return _ret;
		}

		public static string NVL(this object Data, string pDefaultVal = "")
		{
			return (Data != null && Data.ToString() != string.Empty) ?
				Data.ToString() :
				pDefaultVal;
		}

		/// <summary>Convierte una Lista de Numeros en una Cadena Separada x Comas (CSV).</summary>
		/// <param name="Data">Lista de Numeros Enteros</param>
		/// <param name="Separator">[OPCIONAL] Caracter usado para separar los elementos de la Lista. Defecto es Coma.</param>
		/// <returns>int[1,2,3,4] -> '1,2,3,4'</returns>
		public static string ToCSVstring(this List<int> Data, char Separator = ',')
		{
			string _ret = string.Empty;
			try
			{
				if (Data != null && Data.Count > 0)
				{
					return String.Join(Separator.ToString(), Data);
				}
			}
			catch { throw; }
			return _ret;
		}
		
		/// <summary>Convierte una Cadena Separada x Comas (CSV) en su Lista de Numeros Equivalente.</summary>
		/// <param name="CsvData">Texto a Convertir.</param>
		/// <param name="Separator">[OPCIONAL] Caracter usado para separar los elementos de la Lista. Defecto es Coma.</param>
		/// <returns>'1,2,3,4' -> int[1,2,3,4]</returns>
		public static List<int> FromCSVstring(this String CsvData, char Separator = ',')
		{
			List<int> _ret = null;
			try
			{
				if (!string.IsNullOrEmpty(CsvData))
				{
					var TempList = CsvData.Split(new char[] { Separator }).ToList();
					foreach (string item in TempList)
					{
						if (_ret is null) _ret = new List<int>();
						_ret.Add(Convert.ToInt32(item));
					}
				}
			}
			catch { throw; }
			return _ret;
		}

		/// <summary>Devuelve la Sumatoria de todos los numeros en la Lista.</summary>
		/// <param name="Data">Lista de Numeros Enteros</param>
		public static int Sumatoria(this List<int> Data)
		{
			int _ret = 0;
			try
			{
				if (Data != null && Data.Count > 0)
				{
					foreach (var number in Data)
					{
						_ret += number;
					}
				}
			}
			catch { throw; }
			return _ret;
		}

		#endregion

		#region Serializacion

		#region JSON ToFrom a file

		/// <summary>Serializa y escribe el objeto indicado en un archivo JSON.
		/// <para>La Clase a Serializar DEBE tener un Constructor sin parametros.</para>
		/// <para>Only Public properties and variables will be written to the file. These can be any type, even other classes.</para>
		/// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [JsonIgnore] attribute.</para>
		/// </summary>
		/// <typeparam name="T">El tipo de Objeto a guardar en el Archivo.</typeparam>
		/// <param name="filePath">Ruta completa al archivo donde se guardan.</param>
		/// <param name="objectToWrite">Instancia del Objeto a Serializar</param>
		/// <param name="append">'false'=Sobre-Escribe el Archivo, 'true'=Añade datos al final del archivo.</param>
		public static void Serialize_ToJSON<T>(string filePath, T objectToWrite, bool Beautify = true, bool append = false) where T : new()
		{
			TextWriter writer = null;
			try
			{
				var contentsToWriteToFile = Newtonsoft.Json.JsonConvert.SerializeObject(objectToWrite,
					(Beautify ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None)
					);
				//new Newtonsoft.Json.JsonSerializerSettings
				//{
				//	PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects
				//});

				writer = new StreamWriter(filePath, append);
				writer.Write(contentsToWriteToFile);
			}
			finally
			{
				if (writer != null)
					writer.Close();
			}
		}

		/// <summary>Crea una instancia de un Objeto leyendo sus datos desde un archivo JSON.
		/// <para>Object type must have a parameterless constructor.</para></summary>
		/// <typeparam name="T">The type of object to read from the file.</typeparam>
		/// <param name="filePath">The file path to read the object instance from.</param>
		/// <returns>Returns a new instance of the object read from the Json file.</returns>
		public static T DeSerialize_FromJSON<T>(string filePath) where T : new()
		{
			TextReader reader = null;
			try
			{
				reader = new StreamReader(filePath);
				var fileContents = reader.ReadToEnd();
				return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(fileContents);
			}
			finally
			{
				if (reader != null)
					reader.Close();
			}
		}

		#endregion

		#region JSON To/From a String

		/// <summary>Serializa y escribe el objeto indicado en una cadena JSON.
		/// <para>Object type must have a parameterless constructor.</para>
		/// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>
		/// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [JsonIgnore] attribute.</para>
		/// </summary>
		/// <typeparam name="T">The type of object being written to the file.</typeparam>
		/// <param name="objectToWrite">The object instance to write to the file.</param>
		public static string Serialize_ToJSON<T>(T objectToWrite) where T : new()
		{
			string _ret = string.Empty;
			try
			{
				_ret = Newtonsoft.Json.JsonConvert.SerializeObject(objectToWrite, Newtonsoft.Json.Formatting.Indented);
			}
			catch { }
			return _ret;
		}

		/// <summary>Crea una instancia de un Objeto leyendo sus datos desde una cadena JSON.
		/// <para>Object type must have a parameterless constructor.</para></summary>
		/// <typeparam name="T">The type of object to read from the file.</typeparam>
		/// <param name="JSONstring">Texto con formato JSON</param>
		/// <returns>Returns a new instance of the object</returns>
		public static T DeSerialize_FromJSON_String<T>(string JSONstring) where T : new()
		{
			if (JSONstring != null && JSONstring != string.Empty)
			{
				return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(JSONstring);
			}
			else
			{
				return default(T);
			}
		}

		#endregion

		#region XML

		/// <summary>Serializa un Obje.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="serializableObject"></param>
		/// <param name="fileName"></param>
		public static void Serialize_ToXML<T>(T serializableObject, string fileName)
		{
			if (serializableObject == null) { return; }

			try
			{
				System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument();
				System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(serializableObject.GetType());
				using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
				{
					serializer.Serialize(stream, serializableObject);
					stream.Position = 0;
					xmlDocument.Load(stream);
					xmlDocument.Save(fileName);
				}
			}
			catch (Exception ex)
			{
				//Log exception here
			}
		}

		/// <summary>
		/// Deserializes an xml file into an object list
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static T DeSerialize_FromXML<T>(string fileName)
		{
			if (string.IsNullOrEmpty(fileName)) { return default(T); }

			T objectOut = default(T);

			try
			{
				System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument();
				xmlDocument.Load(fileName);
				string xmlString = xmlDocument.OuterXml;

				using (System.IO.StringReader read = new System.IO.StringReader(xmlString))
				{
					Type outType = typeof(T);

					System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(outType);
					using (System.Xml.XmlReader reader = new System.Xml.XmlTextReader(read))
					{
						objectOut = (T)serializer.Deserialize(reader);
					}
				}
			}
			catch (Exception ex)
			{
				//Log exception here
			}

			return objectOut;
		}

		#endregion

		#endregion

		#region Files & Directories

		/// <summary>Convierte el tamaño de un archivo a la unidad más adecuada.</summary>
		/// <param name="pFileBytes">Tamaño del Archivo en Bytes</param>
		/// <returns>"0.### XB", ejem. "4.2 KB" or "1.434 GB"</returns>
		public static string GetFileSize(long pFileBytes)
		{
			// Get absolute value
			long absolute_i = (pFileBytes < 0 ? -pFileBytes : pFileBytes);
			// Determine the suffix and readable value
			string suffix;
			double readable;
			if (absolute_i >= 0x1000000000000000) // Exabyte
			{
				suffix = "EB";
				readable = (pFileBytes >> 50);
			}
			else if (absolute_i >= 0x4000000000000) // Petabyte
			{
				suffix = "PB";
				readable = (pFileBytes >> 40);
			}
			else if (absolute_i >= 0x10000000000) // Terabyte
			{
				suffix = "TB";
				readable = (pFileBytes >> 30);
			}
			else if (absolute_i >= 0x40000000) // Gigabyte
			{
				suffix = "GB";
				readable = (pFileBytes >> 20);
			}
			else if (absolute_i >= 0x100000) // Megabyte
			{
				suffix = "MB";
				readable = (pFileBytes >> 10);
			}
			else if (absolute_i >= 0x400) // Kilobyte
			{
				suffix = "KB";
				readable = pFileBytes;
			}
			else
			{
				return pFileBytes.ToString("0 B"); // Byte
			}

			readable = System.Math.Round((readable / 1024), 2);
			return string.Format("{0:n1} {1}", readable, suffix);
		}

		/// <summary>Muestra un mensaje en un cuadro de diálogo, espera a que el usuario escriba un texto 
		/// o haga clic en un botón y devuelve una cadena con el contenido del cuadro de texto.</summary>
		/// <param name="title">Expresión de tipo String que se muestra en la barra de título del cuadro de diálogo.</param>
		/// <param name="promptText">Expresión de tipo String que se muestra como mensaje en el cuadro de diálogo.</param>
		/// <param name="value">Valor Suministrado por el usuario.</param>
		public static DialogResult InputBox(string title, string promptText, ref string value)
		{
			Form form = new Form();
			Label label = new Label();
			TextBox textBox = new TextBox();
			Button buttonOk = new Button();
			Button buttonCancel = new Button();

			form.Text = title;
			label.Text = promptText;
			textBox.Text = value;

			buttonOk.Text = "Aceptar";
			buttonCancel.Text = "Cancelar";
			buttonOk.DialogResult = DialogResult.OK;
			buttonCancel.DialogResult = DialogResult.Cancel;

			label.SetBounds(9, 20, 372, 13);
			textBox.SetBounds(12, 36, 372, 20);
			buttonOk.SetBounds(228, 72, 75, 23);
			buttonCancel.SetBounds(309, 72, 75, 23);

			label.AutoSize = true;
			textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
			buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

			form.ClientSize = new Size(396, 107);
			form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
			form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
			form.FormBorderStyle = FormBorderStyle.FixedDialog;
			form.StartPosition = FormStartPosition.CenterScreen;
			form.MinimizeBox = false;
			form.MaximizeBox = false;
			form.AcceptButton = buttonOk;
			form.CancelButton = buttonCancel;

			DialogResult dialogResult = form.ShowDialog();
			value = textBox.Text;
			return dialogResult;
		}

		/// <summary>Muestra un mensaje en un cuadro de diálogo, solicitando al usuario el ingreso de datos varios.</summary>
		/// <param name="title">Expresión de tipo String que se muestra en la barra de título del cuadro de diálogo.</param>
		/// <param name="promptText">Expresión de tipo String que se muestra como mensaje en el cuadro de diálogo.</param>
		/// <param name="Fields">[REFERENCIA] Campos de tipo 'KeyValue' que el usuario puede cambiar.</param>
		/// <param name="ButtonsTexts">[OPTIONAL] Texts for both OK and Cancel Buttons.</param>
		/// <returns>OK si el usuario acepta.</returns>
		public static DialogResult InputBox(string title, string promptText, ref List<KeyValue> Fields, string ButtonsTexts = "Aceptar|Cancelar")
		{
			Form form = new Form
			{
				FormBorderStyle = FormBorderStyle.FixedDialog,
				StartPosition = FormStartPosition.CenterScreen,
				MaximizeBox = false,
				MinimizeBox = false,
				Text = title,
				Width = 340,
				Height = 170
			};

			#region Bottom Panel & Buttons

			Panel bottomPanel = new Panel();
			form.Controls.Add(bottomPanel);
			bottomPanel.Dock = DockStyle.Bottom;
			bottomPanel.Height = 38;
			bottomPanel.BackColor = SystemColors.ControlLightLight;

			var palabras = ButtonsTexts.Split('|');

			Button buttonOk = new Button() { Text = palabras[0], Anchor = AnchorStyles.Top | AnchorStyles.Right, DialogResult = DialogResult.OK };
			Button buttonCancel = new Button() { Text = palabras[1], Anchor = AnchorStyles.Top | AnchorStyles.Right, DialogResult = DialogResult.Cancel };

			bottomPanel.Controls.Add(buttonOk);
			bottomPanel.Controls.Add(buttonCancel);

			buttonOk.SetBounds(form.ClientSize.Width - 77, 7, 75,23);
			buttonCancel.SetBounds(buttonOk.Left - 77, 7, 75,23);

			form.AcceptButton = buttonOk;
			form.CancelButton = buttonCancel;

			#endregion

			#region Prompt Text

			Label lblPrompt = new Label() 
			{
				Dock = DockStyle.Top,
				Text = promptText, 
				Font = new Font(form.Font, FontStyle.Bold),
				AutoSize = false, 				
				Height = 24 ,
				TextAlign = ContentAlignment.MiddleCenter
			};
			form.Controls.Add(lblPrompt);

			#endregion

			#region Controls for KeyValues

			TableLayoutPanel Contenedor = new TableLayoutPanel()
			{
				Size = new Size(form.ClientSize.Width - 20, 50),
				AutoSizeMode = AutoSizeMode.GrowAndShrink,
				AutoSize = true,
				ColumnCount = 2,				
				Location = new Point(10, lblPrompt.Location.Y + lblPrompt.Height + 4)
			};
			form.Controls.Add(Contenedor);
			Contenedor.ColumnStyles.Clear();
			Contenedor.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
			Contenedor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute));
			Contenedor.ColumnStyles[1].Width = form.ClientSize.Width - 120;
			Contenedor.RowStyles.Clear();
			
			int currentRow = 0;
			foreach (KeyValue field in Fields)
			{
				// Create Label and TextBox controls
				Label field_label = new Label
				{
					Text = field.Key,
					AutoSize = false,
					Dock = DockStyle.Fill,
					TextAlign = ContentAlignment.MiddleCenter 
				};
				Control field_value = null;
				if (field.ValueType == KeyValue.ValueTypes.String)
				{
					field_value = new TextBox
					{
						Text = field.Value,
						Dock = DockStyle.Fill
					};
					((TextBox)field_value).TextChanged += (sender, args) =>
					{
						field.Value = ((TextBox)sender).Text;
					};
				}
				if (field.ValueType == KeyValue.ValueTypes.Integer)
				{
					field_value = new NumericUpDown
					{
						Minimum = int.MinValue,
						Maximum = int.MaxValue,
						Value = Convert.ToInt32(field.Value),
						Dock = DockStyle.Fill,
						DecimalPlaces = 0
					};
					((NumericUpDown)field_value).ValueChanged += (sender, args) =>
					{
						field.Value = ((NumericUpDown)sender).Value.ToString();
					};
				}
				if (field.ValueType == KeyValue.ValueTypes.Decimal)
				{
					field_value = new NumericUpDown
					{
						Minimum = Decimal.MinValue,
						Maximum = Decimal.MaxValue,
						Value = Convert.ToDecimal(field.Value),
						Dock = DockStyle.Fill,
						DecimalPlaces = 2
					};
					((NumericUpDown)field_value).ValueChanged += (sender, args) =>
					{
						field.Value = ((NumericUpDown)sender).Value.ToString();
					};
				}
				if (field.ValueType == KeyValue.ValueTypes.Date)
				{
					field_value = new DateTimePicker
					{
						Value = Convert.ToDateTime(field.Value),
						Dock = DockStyle.Fill,
						Format = DateTimePickerFormat.Short
					};
					((DateTimePicker)field_value).ValueChanged += (sender, args) =>
					{
						field.Value = ((DateTimePicker)sender).Value.ToString();
					};
				}
				if (field.ValueType == KeyValue.ValueTypes.Boolean)
				{
					field_value = new CheckBox
					{
						Checked = Convert.ToBoolean(field.Value),
						Dock = DockStyle.Fill,
						Text = field.Key
					};
					((CheckBox)field_value).CheckedChanged += (sender, args) =>
					{
						field.Value = ((CheckBox)sender).Checked.ToString();
					};
				}
				if (field.ValueType == KeyValue.ValueTypes.Dynamic)
				{
					field_value = new ComboBox
					{
						DataSource = field.DataSet,
						ValueMember = "Value",
						DisplayMember = "Key",
						DropDownStyle = ComboBoxStyle.DropDownList,
						SelectedValue = field.Value,
						Dock = DockStyle.Fill
					};
					((ComboBox)field_value).SelectedValueChanged += (sender, args) =>
					{
						field.Value = ((ComboBox)sender).SelectedValue.ToString();
					};
				}

				// Add controls to appropriate cells:
				Contenedor.Controls.Add(field_label, 0, currentRow); // Column 0 for labels
				Contenedor.Controls.Add(field_value, 1, currentRow); // Column 1 for text boxes

				// Increment row index for the next pair
				currentRow++;
			}

			Contenedor.Width = form.ClientSize.Width - 20;

			#endregion

			form.ClientSize = new Size(340, 
				bottomPanel.Height +
				lblPrompt.Height +
				Contenedor.Height +
				20
			);		
			return form.ShowDialog();
		}

		#endregion

		#region Windows Registry

		/// <summary>Lee una Clave del Registro de Windows para el Usuario Actual.
		/// Las Claves en este caso siempre se Leen desde 'HKEY_CURRENT_USER\Software\Cutcsa\DXComercial'.</summary>
		/// <param name="Sistema">Nombre del Sistema que guarda las Claves, ejem: RRHH, Contaduria, CutcsaPagos, etc.</param>
		/// <param name="KeyName">Nombre de la Clave a Leer</param>
		/// <returns>Devuelve NULL si la clave no existe</returns>
		public static object WinReg_ReadKey(string Sistema, string KeyName)
		{
			Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.CurrentUser;
			Microsoft.Win32.RegistryKey sk1 = rk.OpenSubKey(@"Software\BlueMystic\MyNotes\" + Sistema);

			// Si la Clave no existe u ocurre un error al leerla, devuelve NULL
			if (sk1 == null)
			{
				return null;
			}
			else
			{
				try { return sk1.GetValue(KeyName); }
				catch { return null; }
			}
		}

		/// <summary>Escribe un Valor en una Clave del Registro de Windows para el Usuario Actual.
		/// Las Claves en este caso se Guardan siempre en 'HKEY_CURRENT_USER\Software\Cutcsa\DXComercial'.</summary>
		/// <param name="Sistema">Nombre del Sistema que guarda las Claves, ejem: RRHH, Contaduria, CutcsaPagos, etc.</param>
		/// <param name="KeyName">Nombre de la Clave a guardar, Si no existe se crea.</param>
		/// <param name="Value">Valor a Guardar</param>
		/// <returns>Devuelve TRUE si se guardo el valor Correctamente</returns>
		public static bool WinReg_WriteKey(string Sistema, string KeyName, object Value)
		{
			try
			{
				Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.CurrentUser;
				Microsoft.Win32.RegistryKey sk1 = rk.CreateSubKey(@"Software\BlueMystic\MyNotes\" + Sistema);
				sk1.SetValue(KeyName, Value);

				return true; //<-La Clave se Guardo Exitosamente!
			}
			catch { return false; }
		}

		/// <summary>Obtiene el Nombre de Usuario de Windows o en su defecto en Nombre de Red del Usuario.</summary>
		/// <returns>'Jhollman' o 'INFO38/Jhollman'</returns>
		public static string GetUserName()
		{
			string _ret = null;
			try
			{
				string userName = Environment.UserName;
				if (string.IsNullOrEmpty(userName))
				{
					//Get the Network Name:
					userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
				}
				_ret = userName;
			}
			catch { }
			return _ret;
		}

		#endregion

		#region Imagen

		// ImageConverter object used to convert byte arrays containing JPEG or PNG file images into 
		//  Bitmap objects. This is static and only gets instantiated once.
		private static readonly ImageConverter _imageConverter = new ImageConverter();

		/// <summary>
		/// Method to "convert" an Image object into a byte array, formatted in PNG file format, which 
		/// provides lossless compression. This can be used together with the GetImageFromByteArray() 
		/// method to provide a kind of serialization / deserialization. 
		/// </summary>
		/// <param name="theImage">Image object, must be convertable to PNG format</param>
		/// <returns>byte array image of a PNG file containing the image</returns>
		public static byte[] CopyImageToByteArray(Image theImage)
		{
			if (theImage is null) return null;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				theImage.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
				return memoryStream.ToArray();
			}
		}

		/// <summary>
		/// Method that uses the ImageConverter object in .Net Framework to convert a byte array, 
		/// presumably containing a JPEG or PNG file image, into a Bitmap object, which can also be 
		/// used as an Image object.
		/// </summary>
		/// <param name="byteArray">byte array containing JPEG or PNG file image or similar</param>
		/// <returns>Bitmap object if it works, else exception is thrown</returns>
		public static Bitmap GetImageFromByteArray(byte[] byteArray)
		{
			if (byteArray is null) return null;

			Bitmap bm = (Bitmap)_imageConverter.ConvertFrom(byteArray);
			if (bm != null && (bm.HorizontalResolution != (int)bm.HorizontalResolution ||
							   bm.VerticalResolution != (int)bm.VerticalResolution))
			{
				// Correct a strange glitch that has been observed in the test program when converting 
				//  from a PNG file image created by CopyImageToByteArray() - the dpi value "drifts" 
				//  slightly away from the nominal integer value
				bm.SetResolution((int)(bm.HorizontalResolution + 0.5f),
								 (int)(bm.VerticalResolution + 0.5f));
			}
			return bm;
		}

		/// <summary>Convierte Imagen en Bytes</summary>
		/// <param name="img">Datos de la Imagen</param>
		public static byte[] BytesFromImage(this Image img) => CopyImageToByteArray(img);

		/// <summary>Convierte Bytes en Imagen.</summary>
		/// <param name="pBytes">Datos de la Imagen</param>
		public static Image Bytes2Image(this byte[] pBytes) => GetImageFromByteArray(pBytes);

		//-----------------------------------------------------------------------------------------

		/// <summary>cretes a bitmap from a Base64-encoded string.</summary>
		/// <param name="base64data">Base64 data string.</param>
		public static Image ImageFromBase64(string base64data)
		{
			if (string.IsNullOrEmpty(base64data)) return null;
			return Image.FromStream(new MemoryStream(Convert.FromBase64String(base64data)));
		}

		/// <summary>Convierte una imagen en una cadena Base64</summary>
		/// <param name="image">Imagen a convertir</param>
		public static string ImageToBase64(Bitmap image)
		{
			if (image is null) return null;
			return Convert.ToBase64String(BytesFromImage(image));
		}
		public static string ImageToBase64(Image image)
			=> ImageToBase64((Bitmap)image);

		//-----------------------------------------------------------------------------------------

		/// <summary>Abre la Imagen indicada (si existe) sin dejarla 'en uso'.</summary>
		/// <param name="_ImagePath">Ruta Completa al Archivo</param>
		public static Image OpenImage(string _ImagePath)
		{
			if (string.IsNullOrEmpty(_ImagePath)) return null;
			if (!File.Exists(_ImagePath)) return null;

			return GetImageFromByteArray(File.ReadAllBytes(_ImagePath)); ;
		}

		/// <summary>Abre un archivo de Imagen devolviendolo como Array de Bytes.</summary>
		/// <param name="_ImagePath">Ruta al archivo que contiene la imagen.</param>
		public static byte[] OpenImageBytes(string _ImagePath)
		{
			if (string.IsNullOrEmpty(_ImagePath)) return null;
			if (!File.Exists(_ImagePath)) return null;
			return System.IO.File.ReadAllBytes(_ImagePath);
		}

		/// <summary>Abre un archivo de imagen devolviendola como cadena de Base64.</summary>
		/// <param name="_ImagePath">Ruta completa al archivo de la imagen</param>
		public static string OpenImageBase64(string _ImagePath)
		{
			if (string.IsNullOrEmpty(_ImagePath)) return null;
			if (!File.Exists(_ImagePath)) return null;
			return Convert.ToBase64String(
				System.IO.File.ReadAllBytes(_ImagePath));
		}

		/// <summary>Abre un archivo de imagen devolviendola como Bytes de Base64.</summary>
		/// <param name="_ImagePath">Ruta completa al archivo de la imagen</param>
		public static byte[] OpenImageBase64bytes(string _ImagePath)
		{
			if (string.IsNullOrEmpty(_ImagePath)) return null;
			if (!File.Exists(_ImagePath)) return null;
			return Encoding.ASCII.GetBytes(
				Convert.ToBase64String(
					File.ReadAllBytes(_ImagePath)));
		}
		//-------------------------------------------------------------------------------------

		/// <summary>Guarda una Imagen en diferentes Formatos con alta Calidad y Compresion: JPG, PNG, BMP, GIF.</para></summary>
		/// <param name="image">Imagen a guardar.</param>
		/// <param name="fileName">Ruta completa al archivo donde se guarda la imagen.</param>
		/// <param name="pMIMEType">'image/jpeg', 'image/bmp', 'image/png', 'image/gif' [Default='image/jpeg']</param>
		/// <param name="compression">% de Compresion aplicado a la imagen, de 0 - 100. [Default=100] válido sólo para JPGs.</param>
		public static void SaveImage(Image image, string fileName, string pMIMEType = "image/jpeg", long compression = 100)
			=> SaveImage(BytesFromImage(image), fileName, 1);

		/// <summary>Guarda una Imagen en diferentes Formatos con alta Calidad y Compresion: JPG, PNG, BMP, GIF.</para></summary>
		/// <param name="image">Imagen a guardar.</param>
		/// <param name="fileName">Ruta completa al archivo donde se guarda la imagen.</param>
		/// <param name="pMIMEType">'image/jpeg', 'image/bmp', 'image/png', 'image/gif' [Default='image/jpeg']</param>
		/// <param name="compression">% de Compresion aplicado a la imagen, de 0 - 100. [Default=100] válido sólo para JPGs.</param>
		public static void SaveImage(byte[] pSrcImage, string filePath, string pMIMEType = "image/jpeg", long compression = 100)
		{
			try
			{
				if (pSrcImage != null && pSrcImage.Length > 0)
				{
					var _Encoder = ImageCodecInfo.GetImageEncoders().FirstOrDefault(encoder => encoder.MimeType == pMIMEType);
					var _QualityParams = new EncoderParameters(1);
					_QualityParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, compression);

					using (var ms = new MemoryStream(pSrcImage))
					{
						using (var fs = new FileStream(filePath, FileMode.Create, System.IO.FileAccess.ReadWrite))
						{
							var image = Image.FromStream(ms);
							image.Save(fs, _Encoder, _QualityParams);
						}
					}
				}
			}
			catch { throw; }
		}

		/// <summary>Guarda la Imagen en diferentes Formatos, PNG x defecto.</summary>
		/// <param name="image">Imagen a Guardar</param>
		/// <param name="pImagePath">[Opcional] Ruta del Archivo donde se guarda la imagen. 
		/// <para>Si esta vacia se muestra un Dialogo preguntando la ruta de guardado. El MIME type se infiere de la extension del archivo.</para></param>
		/// <param name="ImageIndex">[Opcional] Nº de Imagen a Guardar, se usa como parte del Nombre de archivo.</param>
		public static void SaveImage(byte[] image, string pImagePath = "", int ImageIndex = 1)
		{
			try
			{
				string FilePath = string.Empty;

				if (string.IsNullOrEmpty(pImagePath))
				{
					SaveFileDialog SFDialog = new SaveFileDialog()
					{
						Filter = "Imagen PNG|*.png|Imagen JPG|*.jpg|Imagen BMP|*.bmp|Image GIF|*.gif",
						FilterIndex = 0,
						DefaultExt = "png",
						AddExtension = true,
						CheckPathExists = true,
						OverwritePrompt = true,
						FileName = string.Format("ImagenEscaneada_{0:00}", ImageIndex),
						InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
					};
					if (SFDialog.ShowDialog() == DialogResult.OK)
					{
						FilePath = SFDialog.FileName;
					}
				}
				string Ext = System.IO.Path.GetExtension(FilePath); //<- Extension del archivo
				string MimeType = "image/jpeg";

				switch (Ext.ToUpper())
				{
					case ".JPG": MimeType = "image/jpeg"; break;
					case ".JPEG": MimeType = "image/jpeg"; break;
					case ".PNG": MimeType = "image/png"; break;
					case ".BMP": MimeType = "image/bmp"; break;
					case ".GIF": MimeType = "image/gif"; break;
					default: break;
				}
				SaveImage(image, FilePath, MimeType);

				if (File.Exists(FilePath))
				{
					//Mensajero.ShowMessage("Exito!", "Imagen Guardada!", pIcon: MessageBoxIcon.Information);
				}
			}
			catch (Exception ex)
			{
				throw ex;
				//Mensajero.ShowMessage("ERROR", ex.Message + ex.StackTrace, pIcon: MessageBoxIcon.Error);
			}
		}

		//-------------------------------------------------------------------------------------

		public static Bitmap ResizeImage(this Bitmap originalBitmap, int newWidth, int maxHeight, bool onlyResizeIfWider = true)
		{
			if (onlyResizeIfWider)
			{
				if (originalBitmap.Width <= newWidth)
				{
					newWidth = originalBitmap.Width;
				}
			}

			int newHeight = originalBitmap.Height * newWidth / originalBitmap.Width;
			if (newHeight > maxHeight)
			{
				// Resize with height instead
				newWidth = originalBitmap.Width * maxHeight / originalBitmap.Height;
				newHeight = maxHeight;
			}

			var alteredImage = new Bitmap(originalBitmap, new Size(newWidth, newHeight));
			alteredImage.SetResolution(72, 72);
			return alteredImage;
		}
		public static Image ResizeImage(this Image originalBitmap, int newWidth, int maxHeight, bool onlyResizeIfWider = true) =>
			(Image)ResizeImage((Bitmap)originalBitmap, newWidth, maxHeight, onlyResizeIfWider);

		//-------------------------------------------------------------------------------------

		#endregion

		#region Common Project Methods

		/// <summary>Extrae palabras dentro de [ ] y busca su traduccion en el Lenguaje indicado.</summary>
		/// <param name="pText"></param>
		/// <param name="CurrentLanguage"></param>
		public static string ReplaceAndTranslate(string pText, Traductor CurrentLanguage)
		{
			string _ret = string.Empty;
			try
			{
				if (!string.IsNullOrEmpty(pText))
				{
					var Palabras = pText.Split(new char[] { '[' }).ToList();
					if (Palabras.Count > 0)
					{
						foreach (string word in Palabras)
						{
							if (!string.IsNullOrEmpty(word))
							{
								var P = word.Split(new char[] { ']' }).ToList();
								if (P.Count > 0)
								{
									if (P[0] != string.Empty)
									{
										pText = pText.Replace(string.Format("[{0}]", P[0]), CurrentLanguage.GetTranslation(P[0]));
									}
								}
							}
						}
						_ret = pText;
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return _ret;
		}

		public static string CreateSimpleRTF(string Content)
		{
			string _ret = string.Empty;
			try
			{
				if (!string.IsNullOrEmpty(Content))
				{
					StringBuilder RTF = new StringBuilder();
					RTF.AppendLine(@"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033{\fonttbl{\f0\fnil\fcharset0 Arial;}{\f1\fnil\fcharset0 Calibri;}}");
					RTF.AppendLine(@"{\*\generator Riched20 10.0.22621}\viewkind4\uc1 ");
					RTF.AppendLine(string.Format(@"\pard\sa200\sl276\slmult1\fs24\lang10 {0}\f1\fs22\par", Content));
					RTF.AppendLine(@"}");

					_ret = RTF.ToString();
				}
			}
			catch (Exception ex)
			{
				
			}
			return _ret;
		}
		
		public static System.Windows.Forms.TreeNode CreateTreeNode(DocContent nodeInfo, string Parent, int index = -1)
		{
			TreeNode _root = new TreeNode(nodeInfo.Title);
			_root.Tag = nodeInfo;
			_root.ImageKey += (index >= 0) ?
				string.Format("{0}.{1:D2}", Parent, index) :
				Parent;

			if (nodeInfo.Childs != null)
			{
				int i = 0;
				foreach (var child in nodeInfo.Childs)
				{
					_root.Nodes.Add(CreateTreeNode(child, _root.ImageKey, i));
					i++;
				}
			}
			
			return _root;
		}
		

		public static string GetParentPath(string Path)
		{
			string _ret = string.Empty;
			try
			{
				if (!string.IsNullOrEmpty(Path))
				{
					var items = Path.Split(new char[] { '\\' }).ToList();
					for (int i = 0; i < items.Count -1; i++)
					{
						_ret += string.Format(@"{0}\", items[i]);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		public static SizeF GetFontSize(string pText, Font pFont, Control Parent)
		{
			SizeF _ret = SizeF.Empty;
			try
			{
				using (Graphics g = Parent.CreateGraphics())
				{
					_ret = g.MeasureString(pText, pFont);
				}
			}
			catch { }
			return _ret;
		}

		public static TreeNode FindTreeNodeByFullPath(this TreeNodeCollection collection, string fullPath, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
		{
			var foundNode = collection.Cast<TreeNode>().FirstOrDefault(tn => string.Equals(tn.FullPath, fullPath, comparison));
			if (null == foundNode)
			{
				foreach (var childNode in collection.Cast<TreeNode>())
				{
					var foundChildNode = FindTreeNodeByFullPath(childNode.Nodes, fullPath, comparison);
					if (null != foundChildNode)
					{
						return foundChildNode;
					}
				}
			}

			return foundNode;
		}
		public static TreeNode FindTreeNodeByIPadress(this TreeNodeCollection collection, string ipAddress, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
		{
			var foundNode = collection.Cast<TreeNode>().FirstOrDefault(tn => string.Equals((tn.Tag as DocContent).ID, ipAddress, comparison));
			if (null == foundNode)
			{
				foreach (var childNode in collection.Cast<TreeNode>())
				{
					var foundChildNode = FindTreeNodeByFullPath(childNode.Nodes, ipAddress, comparison);
					if (null != foundChildNode)
					{
						return foundChildNode;
					}
				}
			}

			return foundNode;
		}

		/// <summary>Gets the desired item from a list by its index path.</summary>
		/// <param name="list"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public static object GetItem(List<object> list, List<int> path)
		{
			object current = list;

			foreach (int index in path)
			{
				if (current is List<object> currentList && index < currentList.Count)
				{
					current = currentList[index];
				}
				else
				{
					throw new IndexOutOfRangeException("The path is not valid for the given list.");
				}
			}

			return current;
		}
		public static DocContent GetItem(List<DocContent> content, List<int> path)
		{
			DocContent _ret = null;
			if (content != null && content.Count > 0)
			{
				if (path != null && path.Count > 0)
				{
					List<DocContent> childs = content;
					foreach (int index in path)
					{
						_ret = childs[index];
						childs = _ret.Childs;
					}
				}
			}
			return _ret;
		}

		public static void HighlightText(this RichTextBox myRtb, string word, Color color)
		{

			if (word == string.Empty)
				return;

			int s_start = myRtb.SelectionStart, startIndex = 0, index;

			while ((index = myRtb.Text.IndexOf(word, startIndex)) != -1)
			{
				myRtb.Select(index, word.Length);
				myRtb.SelectionColor = color;

				startIndex = index + word.Length;
			}

			myRtb.SelectionStart = s_start;
			myRtb.SelectionLength = 0;
			myRtb.SelectionColor = Color.Black;
		}

		#endregion

		#region RTF


		[Flags]
		enum EmfToWmfBitsFlags
		{
			EmfToWmfBitsFlagsDefault = 0x00000000,
			EmfToWmfBitsFlagsEmbedEmf = 0x00000001,
			EmfToWmfBitsFlagsIncludePlaceable = 0x00000002,
			EmfToWmfBitsFlagsNoXORClip = 0x00000004
		}
		const int MM_ISOTROPIC = 7;
		const int MM_ANISOTROPIC = 8;

		[DllImport("gdiplus.dll")]
		private static extern uint GdipEmfToWmfBits(IntPtr _hEmf, uint _bufferSize,			byte[] _buffer, int _mappingMode, EmfToWmfBitsFlags _flags);
		[DllImport("gdi32.dll")]
		private static extern IntPtr SetMetaFileBitsEx(uint _bufferSize,			byte[] _buffer);
		[DllImport("gdi32.dll")]
		private static extern IntPtr CopyMetaFile(IntPtr hWmf,			string filename);
		[DllImport("gdi32.dll")]
		private static extern bool DeleteMetaFile(IntPtr hWmf);
		[DllImport("gdi32.dll")]
		private static extern bool DeleteEnhMetaFile(IntPtr hEmf);

		/// <summary>Obtiene codigo RTF para insertar una Imagen.</summary>
		/// <param name="image">La imagen a insertar</param>
		public static string GetEmbedImageString(Bitmap image)
		{
			// RTF Image Format
			// {\pict\wmetafile8\picw[A]\pich[B]\picwgoal[C]\pichgoal[D]
			//  
			// A    = (Image Width in Pixels / Graphics.DpiX) * 2540 
			//  
			// B    = (Image Height in Pixels / Graphics.DpiX) * 2540 
			//  
			// C    = (Image Width in Pixels / Graphics.DpiX) * 1440 
			//  
			// D    = (Image Height in Pixels / Graphics.DpiX) * 1440 

			Metafile metafile = null;
			float dpiX; float dpiY;

			using (Graphics g = Graphics.FromImage(image))
			{
				IntPtr hDC = g.GetHdc();
				metafile = new Metafile(hDC, EmfType.EmfOnly);
				g.ReleaseHdc(hDC);
			}

			using (Graphics g = Graphics.FromImage(metafile))
			{
				g.DrawImage(image, 0, 0);
				dpiX = g.DpiX;
				dpiY = g.DpiY;
			}

			IntPtr _hEmf = metafile.GetHenhmetafile();
			uint _bufferSize = GdipEmfToWmfBits(_hEmf, 0, null, MM_ANISOTROPIC,
			EmfToWmfBitsFlags.EmfToWmfBitsFlagsDefault);
			byte[] _buffer = new byte[_bufferSize];
			GdipEmfToWmfBits(_hEmf, _bufferSize, _buffer, MM_ANISOTROPIC,
										EmfToWmfBitsFlags.EmfToWmfBitsFlagsDefault);
			IntPtr hmf = SetMetaFileBitsEx(_bufferSize, _buffer);
			string tempfile = Path.GetTempFileName();
			CopyMetaFile(hmf, tempfile);
			DeleteMetaFile(hmf);
			DeleteEnhMetaFile(_hEmf);

			var stream = new MemoryStream();
			byte[] data = File.ReadAllBytes(tempfile);
			//File.Delete (tempfile);
			int count = data.Length;
			stream.Write(data, 0, count);

			string proto = @"{\pict\wmetafile8\picw" + (int)(((float)image.Width / dpiX) * 2540)
							  + @"\pich" + (int)(((float)image.Height / dpiY) * 2540)
							  + @"\picwgoal" + (int)(((float)image.Width / dpiX) * 1440)
							  + @"\pichgoal" + (int)(((float)image.Height / dpiY) * 1440)
							  + " "
				  + BitConverter.ToString(stream.ToArray()).Replace("-", "")
							  + "}";
			return proto;
		}

		

		#endregion
	}


}
