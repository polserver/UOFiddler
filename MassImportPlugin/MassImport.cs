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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace MassImport
{
    public partial class MassImport : Form
    {
        public MassImport()
        {
            InitializeComponent();
            Icon = FiddlerControls.Options.GetFiddlerIcon();
            _importList = new List<ImportEntry>();
        }

        private void DefaultXMLOnClick(object sender, EventArgs e)
        {
            string fileName = Path.Combine(FiddlerControls.Options.OutputPath, "plugins/MassImport.xml");

            XmlDocument dom = new XmlDocument();
            XmlDeclaration decl = dom.CreateXmlDeclaration("1.0", "utf-8", null);
            dom.AppendChild(decl);

            XmlComment comment = dom.CreateComment(
                Environment.NewLine + "MassImport Control XML" + Environment.NewLine +
                "Supported Nodes: item/landtile/texture/gump/tiledataitem/tiledataland/hue" + Environment.NewLine +
                "file=relativ or absolute" + Environment.NewLine +
                "remove=bool" + Environment.NewLine +
                "Examples:" + Environment.NewLine +
                "<item index='195' file='test.bmp' remove='False' /> -> Adds/Replace ItemArt from text.bmp to 195" +
                Environment.NewLine +
                @"<landtile index='195' file='C:\import\test.bmp' remove='False' />-> Adds/Replace LandTileArt from c:\import\text.bmp to 195" +
                Environment.NewLine +
                "<gump index='100' file='' remove='True' /> -> Removes gump with index 100" + Environment.NewLine +
                "<tiledataitem index='100' file='test.csv' remove='False' /> -> Reads TileData information from test.csv for index 100" +
                Environment.NewLine +
                "Note: TileData.csv can be one for all (it searches for the right entry)"
            );
            dom.AppendChild(comment);

            XmlElement sr = dom.CreateElement("MassImport");

            XmlElement elem = dom.CreateElement("item");
            elem.SetAttribute("index", "195");
            elem.SetAttribute("file", "test.bmp");
            elem.SetAttribute("remove", "False");
            sr.AppendChild(elem);

            elem = dom.CreateElement("item");
            elem.SetAttribute("index", "1");
            elem.SetAttribute("file", "test.bmp");
            elem.SetAttribute("remove", "True");
            sr.AppendChild(elem);

            elem = dom.CreateElement("landtile");
            elem.SetAttribute("index", "0");
            elem.SetAttribute("file", "");
            elem.SetAttribute("remove", "False");
            sr.AppendChild(elem);

            elem = dom.CreateElement("texture");
            elem.SetAttribute("index", "0");
            elem.SetAttribute("file", "");
            elem.SetAttribute("remove", "False");
            sr.AppendChild(elem);

            elem = dom.CreateElement("gump");
            elem.SetAttribute("index", "0");
            elem.SetAttribute("file", "");
            elem.SetAttribute("remove", "False");
            sr.AppendChild(elem);

            elem = dom.CreateElement("tiledataland");
            elem.SetAttribute("index", "0");
            elem.SetAttribute("file", "");
            elem.SetAttribute("remove", "False");
            sr.AppendChild(elem);

            elem = dom.CreateElement("tiledataitem");
            elem.SetAttribute("index", "0");
            elem.SetAttribute("file", "");
            elem.SetAttribute("remove", "False");
            sr.AppendChild(elem);

            elem = dom.CreateElement("hue");
            elem.SetAttribute("index", "0");
            elem.SetAttribute("file", "");
            elem.SetAttribute("remove", "False");
            sr.AppendChild(elem);

            dom.AppendChild(sr);
            dom.Save(fileName);
            MessageBox.Show($"Default xml saved to {fileName}", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private readonly List<ImportEntry> _importList;

        private void LoadXMLOnClick(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Choose xml file to open",
                CheckFileExists = true,
                Filter = "xml files (*.xml)|*.xml"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (!File.Exists(dialog.FileName))
                {
                    return;
                }

                XmlDocument dom = new XmlDocument();
                dom.Load(dialog.FileName);

                _importList.Clear();

                OutputBox.Clear();

                if (dom.SelectSingleNode("MassImport") == null)
                {
                    OutputBox.AppendText("Invalid XML" + Environment.NewLine);
                    return;
                }

                int currBoxLength = 0;

                foreach (XmlNode xNode in dom.SelectSingleNode("MassImport"))
                {
                    if (xNode.NodeType == XmlNodeType.Comment)
                    {
                        continue;
                    }

                    ImportEntry entry;
                    switch (xNode.Name.ToLower())
                    {
                        case "item":
                            entry = new ImportEntryItem();
                            break;
                        case "landtile":
                            entry = new ImportEntryLandTile();
                            break;
                        case "texture":
                            entry = new ImportEntryTexture();
                            break;
                        case "gump":
                            entry = new ImportEntryGump();
                            break;
                        case "tiledataland":
                            entry = new ImportEntryTileDataLand();
                            break;
                        case "tiledataitem":
                            entry = new ImportEntryTileDataItem();
                            break;
                        case "hue":
                            entry = new ImportEntryHue();
                            break;
                        default:
                            entry = new ImportEntryInvalid();
                            break;
                    }

                    entry.File = xNode.Attributes["file"].InnerText;
                    if (!string.IsNullOrEmpty(entry.File))
                    {
                        entry.File = Path.GetFullPath(entry.File);
                    }

                    if (FiddlerControls.Utils.ConvertStringToInt(xNode.Attributes["index"].InnerText, out int temp))
                    {
                        entry.Index = temp;
                    }
                    else
                    {
                        entry.Index = -1;
                    }

                    entry.Remove = bool.Parse(xNode.Attributes["remove"].InnerText);
                    entry.Valid = true;

                    string message = entry.Test();

                    if (entry.Valid)
                    {
                        _importList.Add(entry);
                        OutputBox.AppendText(message + Environment.NewLine);
                    }
                    else
                    {
                        OutputBox.AppendText(message + Environment.NewLine);
                        OutputBox.Select(currBoxLength, message.Length);
                        OutputBox.SelectionColor = Color.Red;
                    }

                    currBoxLength += message.Length;
                    Application.DoEvents();
                }

                OutputBox.AppendText("------------------------------------------------" + Environment.NewLine);
                OutputBox.AppendText($"{_importList.Count} valid entries found{Environment.NewLine}");

                if (_importList.Count > 0)
                {
                    button3.Enabled = true;
                }
            }

            dialog.Dispose();
        }

        private void StartOnClick(object sender, EventArgs e)
        {
            if (_importList == null)
            {
                return;
            }

            if (_importList.Count == 0)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            OutputBox.Clear();

            Dictionary<string, bool> changedUltimaClass = new Dictionary<string, bool>
            {
                {"Animations", false},
                {"Animdata", false},
                {"Art", false},
                {"ASCIIFont", false},
                {"UnicodeFont", false},
                {"Gumps", false},
                {"Hues", false},
                {"Light", false},
                {"Map", false},
                {"Multis", false},
                {"Skills", false},
                {"Sound", false},
                {"Speech", false},
                {"CliLoc", false},
                {"Texture", false},
                {"TileData", false},
                {"RadarColor", false}
            };

            OutputBox.AppendText("Importing");

            foreach (ImportEntry entry in _importList)
            {
                if (!entry.Valid)
                {
                    continue;
                }

                OutputBox.AppendText(".");
                entry.Import(checkBoxDirectSave.Checked, ref changedUltimaClass);
            }

            OutputBox.AppendText("Done" + Environment.NewLine);

            if (checkBoxDirectSave.Checked)
            {
                if (changedUltimaClass["Art"])
                {
                    OutputBox.AppendText("Saving Items/LandTiles.." + Environment.NewLine);
                    Ultima.Art.Save(FiddlerControls.Options.OutputPath);
                }

                if (changedUltimaClass["Texture"])
                {
                    OutputBox.AppendText("Saving Textures.." + Environment.NewLine);
                    Ultima.Textures.Save(FiddlerControls.Options.OutputPath);
                }

                if (changedUltimaClass["Gumps"])
                {
                    OutputBox.AppendText("Saving Gumps.." + Environment.NewLine);
                    Ultima.Gumps.Save(FiddlerControls.Options.OutputPath);
                }

                if (changedUltimaClass["TileData"])
                {
                    OutputBox.AppendText("Saving TileData.." + Environment.NewLine);
                    Ultima.TileData.SaveTileData(Path.Combine(FiddlerControls.Options.OutputPath, "tiledata.mul"));
                }

                if (changedUltimaClass["Hues"])
                {
                    OutputBox.AppendText("Saving Hues.." + Environment.NewLine);
                    Ultima.Hues.Save(FiddlerControls.Options.OutputPath);
                }
            }

            Cursor.Current = Cursors.Default;
        }
    }

    public class ImportEntryItem : ImportEntry
    {
        public override int MaxIndex => Ultima.Art.GetMaxItemID();

        public override string Name => "Item";

        public override void Import(bool direct, ref Dictionary<string, bool> changedClasses)
        {
            if (!Remove)
            {
                Bitmap import = new Bitmap(File);
                if (File.Contains(".bmp"))
                {
                    import = FiddlerControls.Utils.ConvertBmp(import);
                }

                Ultima.Art.ReplaceStatic(Index, import);
            }
            else
            {
                Ultima.Art.RemoveStatic(Index);
            }

            if (!direct)
            {
                FiddlerControls.Events.FireItemChangeEvent(this, Index);
                FiddlerControls.Options.ChangedUltimaClass["Art"] = true;
            }

            changedClasses["Art"] = true;
        }
    }

    public class ImportEntryLandTile : ImportEntry
    {
        public override string Name => "LandTile";

        public override void Import(bool direct, ref Dictionary<string, bool> changedClasses)
        {
            if (!Remove)
            {
                Bitmap import = new Bitmap(File);
                if (File.Contains(".bmp"))
                    import = FiddlerControls.Utils.ConvertBmp(import);
                Ultima.Art.ReplaceLand(Index, import);
            }
            else
            {
                Ultima.Art.RemoveLand(Index);
            }

            if (!direct)
            {
                FiddlerControls.Events.FireLandTileChangeEvent(this, Index);
                FiddlerControls.Options.ChangedUltimaClass["Art"] = true;
            }

            changedClasses["Art"] = true;
        }
    }

    public class ImportEntryGump : ImportEntry
    {
        public override int MaxIndex => 0xFFFF;

        public override string Name => "Gump";

        public override void Import(bool direct, ref Dictionary<string, bool> changedClasses)
        {
            if (!Remove)
            {
                Bitmap import = new Bitmap(File);
                if (File.Contains(".bmp"))
                    import = FiddlerControls.Utils.ConvertBmp(import);
                Ultima.Gumps.ReplaceGump(Index, import);
            }
            else
            {
                Ultima.Gumps.RemoveGump(Index);
            }

            if (!direct)
            {
                FiddlerControls.Events.FireGumpChangeEvent(this, Index);
                FiddlerControls.Options.ChangedUltimaClass["Gumps"] = true;
            }

            changedClasses["Gumps"] = true;
        }
    }

    public class ImportEntryTexture : ImportEntry
    {
        public override int MaxIndex => Ultima.Textures.GetIdxLength();

        public override string Name => "Texture";

        public override void Import(bool direct, ref Dictionary<string, bool> changedClasses)
        {
            if (!Remove)
            {
                Bitmap import = new Bitmap(File);
                if (File.Contains(".bmp"))
                    import = FiddlerControls.Utils.ConvertBmp(import);
                Ultima.Textures.Replace(Index, import);
            }
            else
            {
                Ultima.Textures.Remove(Index);
            }

            if (!direct)
            {
                FiddlerControls.Events.FireTextureChangeEvent(this, Index);
                FiddlerControls.Options.ChangedUltimaClass["Texture"] = true;
            }

            changedClasses["Texture"] = true;
        }
    }

    public class ImportEntryTileDataItem : ImportEntry
    {
        private string[] _tiledata;

        public override int MaxIndex => Ultima.Art.GetMaxItemID();

        public override string Name => "TileDataItem";

        protected override void TestFile(ref string message)
        {
            if (!File.Contains(".csv"))
            {
                message += " Invalid File format";
                Valid = false;
            }
            else
            {
                Valid = GetTileDataInfo(File, ref message, ref _tiledata);
            }
        }

        public override void Import(bool direct, ref Dictionary<string, bool> changedClasses)
        {
            Ultima.TileData.ItemTable[Index].ReadData(_tiledata);
            if (!direct)
            {
                FiddlerControls.Options.ChangedUltimaClass["TileData"] = true;
                FiddlerControls.Events.FireTileDataChangeEvent(this, Index + 0x4000);
            }

            changedClasses["TileData"] = true;
        }
    }

    public class ImportEntryTileDataLand : ImportEntry
    {
        private string[] _tiledata;

        public override string Name => "TileDataLand";

        protected override void TestFile(ref string message)
        {
            if (!File.Contains(".csv"))
            {
                message += " Invalid Fileformat";
                Valid = false;
            }
            else
            {
                Valid = GetTileDataInfo(File, ref message, ref _tiledata);
            }
        }

        public override void Import(bool direct, ref Dictionary<string, bool> changedClasses)
        {
            Ultima.TileData.LandTable[Index].ReadData(_tiledata);
            if (!direct)
            {
                FiddlerControls.Options.ChangedUltimaClass["TileData"] = true;
                FiddlerControls.Events.FireTileDataChangeEvent(this, Index);
            }

            changedClasses["TileData"] = true;
        }
    }

    public class ImportEntryHue : ImportEntry
    {
        public override int MaxIndex => 3000;

        public override string Name => "Hue";

        protected override void TestFile(ref string message)
        {
            if (!File.Contains(".txt"))
            {
                message += " Invalid Fileformat";
                Valid = false;
            }
            else
            {
                Valid = true;
            }
        }

        public override void Import(bool direct, ref Dictionary<string, bool> changedClasses)
        {
            if (!Remove)
            {
                Ultima.Hues.List[Index].Import(File);
            }
            else
            {
                Ultima.Hues.List[Index].Name = "";
                for (int i = 0; i < Ultima.Hues.List[Index].Colors.Length; ++i)
                {
                    Ultima.Hues.List[Index].Colors[i] = -32767;
                }

                Ultima.Hues.List[Index].TableStart = -32768;
                Ultima.Hues.List[Index].TableEnd = -32768;
            }

            if (!direct)
            {
                FiddlerControls.Events.FireHueChangeEvent();
                FiddlerControls.Options.ChangedUltimaClass["Hues"] = true;
            }

            changedClasses["Hues"] = true;
        }
    }

    public class ImportEntryInvalid : ImportEntry
    {
        public override string Name => "Invalid";

        protected override void TestFile(ref string message)
        {
            Valid = false;
        }

        public override void Import(bool direct, ref Dictionary<string, bool> changedClasses)
        {
        }
    }

    public abstract class ImportEntry
    {
        public virtual int MaxIndex => 0x3FFF;

        public abstract void Import(bool direct, ref Dictionary<string, bool> changedClasses);

        public abstract string Name { get; }

        public string File { get; set; }

        public int Index { get; set; }

        public bool Remove { get; set; }

        public bool Valid { get; set; }

        protected virtual void TestFile(ref string message)
        {
            if (File.Contains(".bmp") || File.Contains(".tiff"))
            {
                return;
            }

            message += " Invalid Imageformat";
            Valid = false;
        }

        public string Test()
        {
            string message = $"{Name}: ({Index})";
            Valid = true;

            if (Index < 0)
            {
                message += " Invalid Index ";
                Valid = false;
            }

            if (Index > MaxIndex)
            {
                message += " Invalid Index ";
                Valid = false;
            }

            if (!Remove)
            {
                if (!System.IO.File.Exists(File))
                {
                    message += $" File not found ({File}) ";
                    Valid = false;
                }
                else
                {
                    TestFile(ref message);
                }
            }

            if (!Valid)
            {
                return message;
            }

            message += !Remove ? $" Add/Replace ({File})" : " Remove";

            return message;
        }

        protected bool GetTileDataInfo(string fileName, ref string message, ref string[] tiledata)
        {
            try
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if ((line = line.Trim()).Length == 0 || line.StartsWith("#"))
                        {
                            continue;
                        }

                        if (line.StartsWith("ID;"))
                        {
                            continue;
                        }

                        string[] split = line.Split(';');
                        if (split.Length < 36)
                        {
                            continue;
                        }

                        if (!FiddlerControls.Utils.ConvertStringToInt(split[0], out int id))
                        {
                            continue;
                        }

                        if (Index != id)
                        {
                            continue;
                        }

                        tiledata = split;
                        return true;
                    }
                }
            }
            catch
            {
                // ignored
            }

            message += " No Tiledata information found";
            return false;
        }
    }
}