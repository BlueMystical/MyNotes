using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace MyNotes.PropertyHelper
{
	/// <summary>Permite cargar un archivo de imagen en el PropertyGrid y devolver su contenido como una cadena Base64.</summary>
	public class BitmapLocationEditor : UITypeEditor
	{
		/* Uso:
		   [Editor(typeof(BitmapLocationEditor), typeof(UITypeEditor))]
		   public string Cover { get; set; }
		 */
		private Bitmap myImage;

		// Displays an ellipsis (...) button to start a modal dialog box
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}

		// Edits the value of the specified object using the editor style indicated by the GetEditStyle method.
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider,  object value)
		{
			// Show the dialog we use to open the file.
			// You could use a custom one at this point to provide the file path and the image.
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
					myImage = (Bitmap)Util.OpenImage(OFDialog.FileName); // new Bitmap(OFDialog.FileName);
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
					return Util.ImageToBase64(myImage);
				}
			};
			return value;
		}

		// Indicates whether the specified context supports painting
		// a representation of an object's value within the specified context.
		public override bool GetPaintValueSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		// Paints a representation of the value of an object using the specified PaintValueEventArgs.
		public override void PaintValue(PaintValueEventArgs e)
		{
			if (e.Value != null && !string.IsNullOrEmpty(e.Value.ToString()))
			{
				// Get image
				Bitmap bmp = (Bitmap)Util.ImageFromBase64(e.Value.ToString());// (Bitmap)e.Value;

				// This rectangle indicates the area of the Properties window to draw a representation of the value within.
				Rectangle destRect = e.Bounds;

				// Optional to set the default transparent color transparent for this image.
				bmp.MakeTransparent();

				// Draw image
				e.Graphics.DrawImage(bmp, destRect);
			}
		}

		public override string ToString()
		{
			return Util.ImageToBase64(this.myImage);
		}
	}

	/// <summary>Permite editar 'List<string>' en el PropertyGrid </summary>
	public class MyStringCollectionEditor : CollectionEditor
	{
		/* Uso:
			[Editor(typeof(MyStringCollectionEditor), typeof(UITypeEditor))]
			[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
			public List<string> Categories { get; set; }
		 */
		public MyStringCollectionEditor() : base(type: typeof(List<String>)) { }
		protected override object CreateInstance(Type itemType)
		{
			return string.Empty;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (value != null) // already initialized
			{
				return base.EditValue(context, provider, value);
			}
			else
			{
				return Activator.CreateInstance(typeof(List<String>));
			}
		}
	}

	/// <summary>Para usar con 'List<string>' Concatena los elementos en una sola cadena:
	/// <para>Ejem:  [1,2,3,4] -> '1,2,3,4' </para></summary>
	public class CsvConverter : TypeConverter
	{
		/*  USO:
		 *  [TypeConverter(typeof(CsvConverter))]
		 */
		// Overrides the ConvertTo method of TypeConverter.
		public override object ConvertTo(ITypeDescriptorContext context,
		   System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			List<String> v = value as List<String>;
			if (destinationType == typeof(string) && v != null)
			{
				return String.Join(",", v.ToArray());
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}

	/// <summary>Permiter elejir de una lista de valores predefinidos.</summary>
	public class StringListConverter : TypeConverter
	{
		/* USO:
		 * [TypeConverter(typeof(StringListConverter))]
			public string Language { get; set; } = "en";

		 * EN EL FORM_LOAD:
		    List<string> _languages = new List<string>(new string[] { "en", "es", "fr", "de", "ru", "it" });
			StringListConverter.RegisterValuesForProperty(typeof(MyClass), nameof(MyClass.Language), _languages);
			// puede haber varios.
		*/

		/// <summary>
		/// Dictionary that maps a combination of type and property name to a list of strings
		/// </summary>
		private static Dictionary<(Type type, string propertyName), IEnumerable<string>> _lists = new Dictionary<(Type type, string propertyName), IEnumerable<string>>();

		public static void RegisterValuesForProperty(Type type, string propertyName, IEnumerable<string> list)
		{
			_lists[(type, propertyName)] = list;
		}

		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			if (_lists.TryGetValue((context.PropertyDescriptor.ComponentType, context.PropertyDescriptor.Name), out var list))
			{
				return new StandardValuesCollection(list.ToList());
			}
			else
			{
				throw new Exception("Unknown property " + context.PropertyDescriptor.ComponentType + " " + context.PropertyDescriptor.Name);
			}
		}
	}
	//--------------------------------------------------------------------------;
	
	//Show a Custom Form as Editor:
	internal class GenericTypeEditor : UITypeEditor
	{
		/* USO:
			[EditorAttribute(typeof(GenericTypeEditor), typeof(UITypeEditor))]
			public string Summary { get; set; } = string.Empty;
		*/
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			IWindowsFormsEditorService winFormEditorSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

			//Show a Custom Form as Editor: [UNCOMMENT LINES BELOW]
			//using (MyForm editorForm = new MyForm())
			//{
			//	if (winFormEditorSvc.ShowDialog(editorForm) == System.Windows.Forms.DialogResult.OK)
			//		value = editorForm.ReturnObject;
			//}

			return value; //this can be null if you wish
		}

		public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}
	}

	public class MyMultiSelectionEditor : UITypeEditor
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			// adapt to your needs
			if (!IsPropertyGridInMultiView(context))
				return UITypeEditorEditStyle.None;

			return UITypeEditorEditStyle.Modal;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (IsPropertyGridInMultiView(context))
			{
				// multi view, show my custom stuff
				MessageBox.Show("hello from multi selection");
			}
			return base.EditValue(context, provider, value);
		}

		// gets a PropertyGrid instance from the context, if any
		private static PropertyGrid GetPropertyGrid(ITypeDescriptorContext context)
		{
			IServiceProvider sp = context as IServiceProvider;
			if (sp == null)
				return null;

			Control view = sp.GetService(typeof(IWindowsFormsEditorService)) as Control;
			if (view == null)
				return null;

			return view.Parent as PropertyGrid;
		}

		// determines if there is a PropertyGrid in the context, and if it's selection is multiple
		private static bool IsPropertyGridInMultiView(ITypeDescriptorContext context)
		{
			PropertyGrid pg = GetPropertyGrid(context);
			if (pg == null)
				return false;

			return pg.SelectedObjects != null && pg.SelectedObjects.Length > 1;
		}
	}

	public class StringArrayDescriptionProvider : TypeDescriptionProvider
	{
		private static TypeDescriptionProvider _baseProvider;

		static StringArrayDescriptionProvider()
		{
			// get default metadata
			_baseProvider = TypeDescriptor.GetProvider(typeof(string[]));
		}

		public override object CreateInstance(IServiceProvider provider, Type objectType, Type[] argTypes, object[] args)
		{
			// this is were we define create the instance
			// NB: .NET could do this IMHO...
			return Array.CreateInstance(typeof(string), 0);
		}

		public override IDictionary GetCache(object instance)
		{
			return _baseProvider.GetCache(instance);
		}

		public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
		{
			return _baseProvider.GetExtendedTypeDescriptor(instance);
		}

		public override string GetFullComponentName(object component)
		{
			return _baseProvider.GetFullComponentName(component);
		}

		public override Type GetReflectionType(Type objectType, object instance)
		{
			return _baseProvider.GetReflectionType(objectType, instance);
		}

		public override Type GetRuntimeType(Type reflectionType)
		{
			return _baseProvider.GetRuntimeType(reflectionType);
		}

		public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
		{
			return _baseProvider.GetTypeDescriptor(objectType, instance);
		}

		public override bool IsSupportedType(Type type)
		{
			return _baseProvider.IsSupportedType(type);
		}
	}

	public class SomeTypeEditor : CollectionEditor
	{
		public SomeTypeEditor(Type type) : base(type) { }

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			object result = base.EditValue(context, provider, value);

			// assign the temporary collection from the UI to the property
			//((ClassContainingStuffProperty)context.Instance).Stuff = (List<SomeType>)result;

			return result;
		}
	}

	
}
