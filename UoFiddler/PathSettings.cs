/***************************************************************************
 *
 * $Author: Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Ultima;

namespace UoFiddler
{
    public partial class PathSettings : Form
    {
        public PathSettings()
        {
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
            propertyGrid1.SelectedObject = new DictionaryPropertyGridAdapter(Files.MulPath);
            TextBoxRoot.Text = Files.RootDir;
        }

        private void ReloadPath(object sender, EventArgs e)
        {
            Files.ReLoadDirectory();
            Files.LoadMulPath();
            Files.CheckForNewMapSize();
            propertyGrid1.SelectedObject = new DictionaryPropertyGridAdapter(Files.MulPath);
            propertyGrid1.Invalidate();
            propertyGrid1.Update();
            TextBoxRoot.Text = Files.RootDir;

        }

        private void OnClickManual(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory containing the client files";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Files.SetMulPath(dialog.SelectedPath);
                    propertyGrid1.SelectedObject = new DictionaryPropertyGridAdapter(Files.MulPath);
                    propertyGrid1.Invalidate();
                    propertyGrid1.Update();
                    TextBoxRoot.Text = Files.RootDir;
                    Files.CheckForNewMapSize();
                }
            }
        }

        private void onKeyDownDir(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Files.SetMulPath(TextBoxRoot.Text);
                propertyGrid1.SelectedObject = new DictionaryPropertyGridAdapter(Files.MulPath);
                propertyGrid1.Invalidate();
                propertyGrid1.Update();
                TextBoxRoot.Text = Files.RootDir;
                Files.CheckForNewMapSize();
            }
        }
    }
    class DictionaryPropertyGridAdapter : ICustomTypeDescriptor
    {
        IDictionary _dictionary;

        public DictionaryPropertyGridAdapter(IDictionary d)
        {
            _dictionary = d;
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        EventDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return _dictionary;
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        PropertyDescriptorCollection
            System.ComponentModel.ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(new Attribute[0]);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            List<PropertyDescriptor> properties = new List<PropertyDescriptor>();
            foreach (DictionaryEntry e in _dictionary)
            {
                properties.Add(new DictionaryPropertyDescriptor(_dictionary, e.Key));
            }

            PropertyDescriptor[] props = properties.ToArray();

            return new PropertyDescriptorCollection(props);
        }
    }

    class DictionaryPropertyDescriptor : PropertyDescriptor
    {
        IDictionary _dictionary;
        object _key;

        internal DictionaryPropertyDescriptor(IDictionary d, object key)
            : base(key.ToString(), null)
        {
            _dictionary = d;
            _key = key;
        }

        public override Type PropertyType
        {
            get { return typeof(string); }
        }

        public override void SetValue(object component, object value)
        {
            _dictionary[_key] = value;
        }

        public override object GetValue(object component)
        {
            return _dictionary[_key];
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override Type ComponentType
        {
            get { return null; }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override void ResetValue(object component)
        {
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

    }
}