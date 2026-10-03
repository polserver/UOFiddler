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
using Ultima.Uop;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Plugin.MassImport.Imports;

namespace UoFiddler.Plugin.MassImport.Forms
{
    public partial class MassImportForm : Form
    {
        public MassImportForm()
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();
            _importList = new List<ImportEntry>();
            
            // 在窗体加载完成后应用汉化
            this.Shown += (s, e) =>
            {
                if (_localizationGetter != null)
                {
                    ApplyLocalization();
                }
            };
        }

        private Func<string, string> _localizationGetter;

        /// <summary>
        /// 设置汉化回调
        /// </summary>
        /// <param name="localizationGetter">返回汉化字符串的委托，参数为键值</param>
        public void SetLocalization(Func<string, string> localizationGetter)
        {
            _localizationGetter = localizationGetter;
            // 如果窗体已显示，立即应用汉化
            if (this.Visible)
            {
                ApplyLocalization();
            }
        }

        private void ApplyLocalization()
        {
            if (_localizationGetter == null)
            {
                return;
            }

            this.Text = _localizationGetter("Forms.MassImportForm.Title") ?? "批量导入";
            groupBox1.Text = _localizationGetter("Forms.MassImportForm.groupBox1") ?? "XML";
            button1.Text = _localizationGetter("Forms.MassImportForm.button1") ?? "创建默认 XML";
            button2.Text = _localizationGetter("Forms.MassImportForm.button2") ?? "加载 XML";
            groupBox2.Text = _localizationGetter("Forms.MassImportForm.groupBox2") ?? "输出";
            button3.Text = _localizationGetter("Forms.MassImportForm.button3") ?? "开始";
            checkBoxDirectSave.Text = _localizationGetter("Forms.MassImportForm.checkBoxDirectSave") ?? "直接保存";
        }

        private void DefaultXMLOnClick(object sender, EventArgs e)
        {
            string fileName = Path.Combine(Options.OutputPath, "MassImport.xml");

            XmlDocument dom = new XmlDocument();
            XmlDeclaration decl = dom.CreateXmlDeclaration("1.0", "utf-8", null);
            dom.AppendChild(decl);

            XmlComment comment = dom.CreateComment(
                Environment.NewLine + "MassImport Control XML" + Environment.NewLine +
                "Supported Nodes: item/landtile/texture/gump/tiledataitem/tiledataland/hue/multi" + Environment.NewLine +
                "file=relative or absolute" + Environment.NewLine +
                "remove=bool" + Environment.NewLine +
                "Examples:" + Environment.NewLine +
                "<item index='195' file='test.bmp' remove='False' /> -> Adds/Replace ItemArt from text.bmp to 195" +
                Environment.NewLine +
                @"<landtile index='195' file='C:\import\test.bmp' remove='False' />-> Adds/Replace LandTileArt from c:\import\text.bmp to 195" +
                Environment.NewLine +
                "<gump index='100' file='' remove='True' /> -> Removes gump with index 100" + Environment.NewLine +
                "<multi index='100' file='multi.txt' remove='False' /> -> Adds/Replace multi information from multi.txt for index 100" +
                Environment.NewLine +
                "Formats are inferred from file extensions." +
                Environment.NewLine +
                "TXT -> .txt, CSV (punt's multi tool) -> .csv, WSC -> .wsc, UOA (text) -> .uoa, UOA (binary) .uoab" +
                Environment.NewLine +
                "There is only minimal validation available so make sure your multi files are ok :-)" +
                Environment.NewLine +
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

            elem = dom.CreateElement("multi");
            elem.SetAttribute("index", "0");
            elem.SetAttribute("file", "");
            elem.SetAttribute("remove", "False");
            sr.AppendChild(elem);

            dom.AppendChild(sr);
            dom.Save(fileName);

            FileSavedDialog.Show(FindForm(), fileName, "Default xml saved successfully.");
        }

        private readonly List<ImportEntry> _importList;

        private void LoadXMLOnClick(object sender, EventArgs e)
        {
            string fileName;

            using (OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Choose xml file to open",
                CheckFileExists = true,
                Filter = "xml files (*.xml)|*.xml"
            })
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                if (!File.Exists(dialog.FileName))
                {
                    return;
                }

                fileName = dialog.FileName;
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            XmlDocument dom = new XmlDocument();
            dom.Load(fileName);

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
                    case "multi":
                        entry = new ImportEntryMulti();
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

                if (Utils.ConvertStringToInt(xNode.Attributes["index"].InnerText, out int temp))
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

            using (new WaitCursorScope(this))
            {
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

                    try
                    {
                        OutputBox.AppendText(".");
                        entry.Import(checkBoxDirectSave.Checked, ref changedUltimaClass);
                    }
                    catch (Exception ex)
                    {
                        OutputBox.AppendText(
                            $"{Environment.NewLine}Error importing {entry.Name} (index {entry.Index}): {ex.Message}{Environment.NewLine}");
                    }
                }

                OutputBox.AppendText($"Done{Environment.NewLine}");

                if (checkBoxDirectSave.Checked)
                {
                    if (changedUltimaClass["Art"])
                    {
                        OutputBox.AppendText($"Saving Items/LandTiles..{Environment.NewLine}");
                        SaveInChosenFormat(FileType.ArtLegacyMul, Ultima.Art.Save);
                    }

                    if (changedUltimaClass["Texture"])
                    {
                        OutputBox.AppendText($"Saving Textures..{Environment.NewLine}");
                        Ultima.Textures.Save(Options.OutputPath);
                    }

                    if (changedUltimaClass["Gumps"])
                    {
                        OutputBox.AppendText($"Saving Gumps..{Environment.NewLine}");
                        SaveInChosenFormat(FileType.GumpartLegacyMul, Ultima.Gumps.Save);
                    }

                    if (changedUltimaClass["TileData"])
                    {
                        OutputBox.AppendText($"Saving TileData..{Environment.NewLine}");
                        Ultima.TileData.SaveTileData(Path.Combine(Options.OutputPath, "tiledata.mul"));
                    }

                    if (changedUltimaClass["Hues"])
                    {
                        OutputBox.AppendText($"Saving Hues..{Environment.NewLine}");
                        Ultima.Hues.Save(Options.OutputPath);
                    }

                    if (changedUltimaClass["Multis"])
                    {
                        OutputBox.AppendText($"Saving Multis..{Environment.NewLine}");
                        SaveInChosenFormat(FileType.MultiCollection, Ultima.Multis.Save);
                    }

                    OutputBox.AppendText($"Done{Environment.NewLine}");
                }
            }
        }

        /// <summary>
        /// Writes one file type in whatever container the save format option asks for. A batch cannot
        /// stop on a dialog, so "ask every time" is taken here to mean the format the client already uses.
        /// </summary>
        private void SaveInChosenFormat(FileType type, Action<string> writeMul)
        {
            ContainerFormat format = SaveFormatResolver.Resolve(type);

            try
            {
                foreach (string concern in ClientFileSaver.Preflight(type, format))
                {
                    OutputBox.AppendText($"{concern}{Environment.NewLine}");
                }

                ClientFileSaveResult result = ClientFileSaver.Save(type, Options.OutputPath, format, writeMul);

                foreach (string warning in result.Warnings)
                {
                    OutputBox.AppendText($"{warning}{Environment.NewLine}");
                }
            }
            catch (Exception ex)
            {
                // Losing one type's output should not abandon the rest of the batch.
                OutputBox.AppendText($"Could not save {type}: {ex.Message}{Environment.NewLine}");
            }
        }
    }
}