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

using System.Text;
using System.Windows.Forms;

namespace UoFiddler.Controls.Forms
{
    public partial class TileDataHelpForm : Form
    {
        public TileDataHelpForm()
        {
            InitializeComponent();
            PopulateFields();
        }

        private void PopulateFields()
        {
            _listView.Groups.Add(new ListViewGroup("items", "Items"));
            _listView.Groups.Add(new ListViewGroup("land", "Land Tiles"));
            _listView.Groups.Add(new ListViewGroup("flags", "Flags"));

            Add("Name", "This field is for the name of the item, which can be a maximum of 20 characters.", "items");
            Add("Animation", "This field is for the animation ID associated with the item.", "items");
            Add("Weight", "This field is for the weight of the item.", "items");
            Add("Layer", new StringBuilder()
                .AppendLine("This field is for the layer of the item:")
                .AppendLine("")
                .AppendLine("1 One handed weapon")
                .AppendLine("2 Two handed weapon, shield, or misc.")
                .AppendLine("3 Shoes")
                .AppendLine("4 Pants")
                .AppendLine("5 Shirt")
                .AppendLine("6 Helm / Line")
                .AppendLine("7 Gloves")
                .AppendLine("8 Ring")
                .AppendLine("9 Talisman")
                .AppendLine("10 Neck")
                .AppendLine("11 Hair")
                .AppendLine("12 Waist (half apron)")
                .AppendLine("13 Torso (inner) (chest armor)")
                .AppendLine("14 Bracelet")
                .AppendLine("15 Unused (but backpackers for backpackers go to 21)")
                .AppendLine("16 Facial Hair")
                .AppendLine("17 Torso (middle) (surcoat, tunic, full apron, sash)")
                .AppendLine("18 Earrings")
                .AppendLine("19 Arms")
                .AppendLine("20 Back (cloak)")
                .AppendLine("21 Backpack")
                .AppendLine("22 Torso (outer) (robe)")
                .AppendLine("23 Legs (outer) (skirt / kilt)")
                .AppendLine("24 Legs (inner) (leg armor)")
                .AppendLine("25 Mount (horse, ostard, etc)")
                .AppendLine("26 NPC Buy Restock container")
                .AppendLine("27 NPC Buy no restock container")
                .Append("28 NPC Sell container")
                .ToString(), "items");
            Add("Quantity", "This field is for the quantity of the item.", "items");
            Add("Value", "This field is for the value of the item.", "items");
            Add("Stacking Offset", new StringBuilder()
                .AppendLine("StackOff refers to the stacking offset in pixels when multiple items are stacked.")
                .Append("A higher StackOff value means the items will appear further apart from each other within the stack.")
                .ToString(), "items");
            Add("Hue", "This field is for the hue (color) of the item.", "items");
            Add("Unknown 2", "This field is for the second unknown value.", "items");
            Add("Misc Data", "Old UO Demo weapon template definition", "items");
            Add("Height", "This field is for the height of the item.", "items");
            Add("Unknown 3", "This field is for the third unknown value.", "items");

            Add("Name", "This field is for the name of the land tile, which can be a maximum of 20 characters.", "land");
            Add("Texture ID", "This field is for the texture ID associated with the land tile.", "land");

            Add("Background",    "Not yet documented.",                                                               "flags");
            Add("Weapon",        "Not yet documented.",                                                               "flags");
            Add("Transparent",   "Not yet documented.",                                                               "flags");
            Add("Translucent",   "The tile is rendered with partial alpha-transparency.",                             "flags");
            Add("Wall",          "The tile is a wall.",                                                               "flags");
            Add("Damaging",      "The tile can cause damage when moved over.",                                        "flags");
            Add("Impassable",    "The tile may not be moved over or through.",                                        "flags");
            Add("Wet",           "Not yet documented.",                                                               "flags");
            Add("Unknown1",      "Unknown.",                                                                          "flags");
            Add("Surface",       "The tile is a surface. It may be moved over, but not through.",                     "flags");
            Add("Bridge",        "The tile is a stair, ramp, or ladder.",                                             "flags");
            Add("Generic",       "The tile is stackable.",                                                            "flags");
            Add("Window",        "The tile is a window. Like NoShoot, tiles with this flag block line of sight.",     "flags");
            Add("NoShoot",       "The tile blocks line of sight.",                                                    "flags");
            Add("ArticleA",      "For single-amount tiles, the string \"a \" should be prepended to the tile name.",  "flags");
            Add("ArticleAn",     "For single-amount tiles, the string \"an \" should be prepended to the tile name.", "flags");
            Add("ArticleThe",    "Probably article \"The\" prepended to the tile name.",                              "flags");
            Add("Foliage",       "The tile becomes translucent when walked behind. Boat masts also have this flag.",  "flags");
            Add("PartialHue",    "Only gray pixels will be hued.",                                                    "flags");
            Add("NoHouse",       "NoHouse or Unknown. Needs further research.",                                       "flags");
            Add("Map",           "The tile is a map in the cartography sense. Unknown usage.",                        "flags");
            Add("Container",     "The tile is a container.",                                                          "flags");
            Add("Wearable",      "The tile may be equipped.",                                                         "flags");
            Add("LightSource",   "The tile gives off light.",                                                         "flags");
            Add("Animation",     "The tile is animated.",                                                             "flags");
            Add("HoverOver",     "Gargoyles can fly over, or NoDiagonal.",                                            "flags");
            Add("NoDiagonal",    "NoDiagonal (Unknown3).",                                                            "flags");
            Add("Armor",         "Not yet documented.",                                                               "flags");
            Add("Roof",          "The tile is a slanted roof.",                                                       "flags");
            Add("Door",          "The tile is a door. Tiles with this flag can be moved through by ghosts and GMs.",  "flags");
            Add("StairBack",     "Not yet documented.",                                                               "flags");
            Add("StairRight",    "Not yet documented.",                                                               "flags");
            Add("AlphaBlend",    "Blend Alphas, tile blending.",                                                      "flags");
            Add("UseNewArt",     "Uses new art style? Something related to the nodraw tile?",                         "flags");
            Add("ArtUsed",       "Has art being used?",                                                               "flags");
            Add("Unused8",       "Unused or unknown yet.",                                                            "flags");
            Add("NoShadow",      "Disallow shadow on this tile, light source? lava?",                                 "flags");
            Add("PixelBleed",    "Let pixels bleed in to other tiles? Is this Disabling Texture Clamp?",              "flags");
            Add("PlayAnimOnce",  "Play tile animation once.",                                                         "flags");
            Add("MultiMovable",  "Movable multi? Cool ships and vehicles etc? Something related to the masts.",       "flags");
            Add("Unused10",      "Unused or unknown yet.",                                                            "flags");
            Add("Unused11",      "Unused or unknown yet.",                                                            "flags");
            Add("Unused12",      "Unused or unknown yet.",                                                            "flags");
            Add("Unused13",      "Unused or unknown yet.",                                                            "flags");
            Add("Unused14",      "Unused or unknown yet.",                                                            "flags");
            Add("Unused15",      "Unused or unknown yet.",                                                            "flags");
            Add("Unused16",      "Unused or unknown yet.",                                                            "flags");
            Add("Unused17",      "Unused or unknown yet.",                                                            "flags");
            Add("Unused18",      "Unused or unknown yet.",                                                            "flags");
            Add("Unused19",      "Unused or unknown yet.",                                                            "flags");
            Add("Unused20",      "Unused or unknown yet.",                                                            "flags");
            Add("Unused21",      "Unused or unknown yet.",                                                            "flags");
            Add("Unused22",      "Unused or unknown yet.",                                                            "flags");
            Add("Unused23",      "Unused or unknown yet.",                                                            "flags");
            Add("Unused24",      "Unused or unknown yet.",                                                            "flags");
            Add("Unused25",      "Unused or unknown yet.",                                                            "flags");
            Add("Unused26",      "Unused or unknown yet.",                                                            "flags");
            Add("Unused27",      "Unused or unknown yet.",                                                            "flags");
            Add("Unused28",      "Unused or unknown yet.",                                                            "flags");
            Add("Unused29",      "Unused or unknown yet.",                                                            "flags");
            Add("Unused30",      "Unused or unknown yet.",                                                            "flags");
            Add("Unused31",      "Unused or unknown yet.",                                                            "flags");
            Add("Unused32",      "Unused or unknown yet.",                                                            "flags");

            if (_listView.Items.Count > 0)
                _listView.Items[0].Selected = true;
        }

        private void Add(string field, string description, string groupKey)
        {
            var group = _listView.Groups[groupKey];
            var item = new ListViewItem(field, group) { Tag = description };
            _listView.Items.Add(item);
        }

        private void OnSelectionChanged(object sender, System.EventArgs e)
        {
            if (_listView.SelectedItems.Count == 0)
                return;

            _descriptionBox.Text = _listView.SelectedItems[0].Tag as string ?? string.Empty;
        }
    }
}
