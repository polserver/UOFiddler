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
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class TileDataHelpForm : Form
    {
        private Func<string, string?>? _localizationGetter;

        public TileDataHelpForm()
        {
            InitializeComponent();
            PopulateFields();
        }

        public void SetLocalization(Func<string, string?> getLocalized)
        {
            _localizationGetter = getLocalized;
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            if (_localizationGetter == null) return;

            this.Text = _localizationGetter("Forms.TileDataHelpForm.Title") ?? "TileData Help";
            
            // Localize column header
            var fieldHeader = _localizationGetter("Forms.TileDataHelpForm.FieldColumnHeader") ?? "Field";
            _columnField.Text = fieldHeader;
            
            // Localize Close button
            var closeButton = _localizationGetter("Forms.TileDataHelpForm.CloseButton") ?? "Close";
            _btnClose.Text = closeButton;
            
            // Clear and re-populate with localized content
            _listView.Items.Clear();
            PopulateFields();
        }

        private void PopulateFields()
        {
            // Helper function to get localized text
            Func<string, string> GetLocalized = (string key) =>
            {
                return _localizationGetter?.Invoke(key) ?? null;
            };

            // Editing many entries section
            AddHeader(GetLocalized("Forms.TileDataHelpForm.EditingManyEntries.Title") ?? "Editing many entries");
            Add(GetLocalized("Forms.TileDataHelpForm.EditingManyEntries.MultiSelect.Title") ?? "Multi-select",
                GetLocalized("Forms.TileDataHelpForm.EditingManyEntries.MultiSelect.Description")
                ?? "Ctrl+click and Shift+click select several entries at once. The right hand pane keeps showing the entry you picked last, and 'Save Changes' writes to every selected entry.",
                "multi");
            Add(GetLocalized("Forms.TileDataHelpForm.EditingManyEntries.EmptyBoxes.Title") ?? "Empty boxes",
                GetLocalized("Forms.TileDataHelpForm.EditingManyEntries.EmptyBoxes.Description")
                ?? "With more than one entry selected, a box the entries disagree on comes up empty, and an empty box is left alone when you save - the entries keep their own values. Fill it in to give them all the same value. Because empty means 'leave alone', a name cannot be cleared across a multi-selection; select the entry on its own to do that.",
                "multi");
            Add(GetLocalized("Forms.TileDataHelpForm.EditingManyEntries.GreyedFlags.Title") ?? "Greyed flags",
                GetLocalized("Forms.TileDataHelpForm.EditingManyEntries.GreyedFlags.Description")
                ?? "A flag that is set on some of the selected entries but not all shows greyed, and a greyed flag is left alone when you save. Clicking it cycles leave alone -> set on all -> clear on all. A flag they all already agree on just toggles.",
                "multi");
            Add(GetLocalized("Forms.TileDataHelpForm.EditingManyEntries.CopyPasteSpecial.Title") ?? "Copy / Paste special",
                GetLocalized("Forms.TileDataHelpForm.EditingManyEntries.CopyPasteSpecial.Description")
                ?? "Right-click an entry and choose 'Copy tile data' to remember it, then select any number of entries and choose 'Paste special...' to pick which of the copied fields and flags to write onto them.",
                "multi");
            Add(GetLocalized("Forms.TileDataHelpForm.EditingManyEntries.Undo.Title") ?? "Undo",
                GetLocalized("Forms.TileDataHelpForm.EditingManyEntries.Undo.Description")
                ?? "Misc -> 'Undo last bulk apply' puts back the values the entries had before the last apply. Only the most recent apply is kept, and it is forgotten when tiledata is reloaded.",
                "multi");

            // Items section
            AddHeader(GetLocalized("Forms.TileDataHelpForm.Items.Title") ?? "Items");
            Add(GetLocalized("Forms.TileDataHelpForm.Items.Name.Title") ?? "Name",
                GetLocalized("Forms.TileDataHelpForm.Items.Name.Description")
                ?? "This field is for the name of the item, which can be a maximum of 20 characters.", "items");
            Add(GetLocalized("Forms.TileDataHelpForm.Items.Animation.Title") ?? "Animation",
                GetLocalized("Forms.TileDataHelpForm.Items.Animation.Description")
                ?? "This field is for the animation ID associated with the item.", "items");
            Add(GetLocalized("Forms.TileDataHelpForm.Items.Weight.Title") ?? "Weight",
                GetLocalized("Forms.TileDataHelpForm.Items.Weight.Description")
                ?? "This field is for the weight of the item.", "items");
            Add(GetLocalized("Forms.TileDataHelpForm.Items.Layer.Title") ?? "Layer",
                GetLocalized("Forms.TileDataHelpForm.Items.Layer.Description")
                ?? "This field is for the layer of the item:\n\n1 One handed weapon\n2 Two handed weapon, shield, or misc.\n3 Shoes\n4 Pants\n5 Shirt\n6 Helm / Line\n7 Gloves\n8 Ring\n9 Talisman\n10 Neck\n11 Hair\n12 Waist (half apron)\n13 Torso (inner) (chest armor)\n14 Bracelet\n15 Unused (but backpackers for backpackers go to 21)\n16 Facial Hair\n17 Torso (middle) (surcoat, tunic, full apron, sash)\n18 Earrings\n19 Arms\n20 Back (cloak)\n21 Backpack\n22 Torso (outer) (robe)\n23 Legs (outer) (skirt / kilt)\n24 Legs (inner) (leg armor)\n25 Mount (horse, ostard, etc)\n26 NPC Buy Restock container\n27 NPC Buy no restock container\n28 NPC Sell container", "items");
            Add(GetLocalized("Forms.TileDataHelpForm.Items.Quantity.Title") ?? "Quantity",
                GetLocalized("Forms.TileDataHelpForm.Items.Quantity.Description")
                ?? "This field is for the quantity of the item.", "items");
            Add(GetLocalized("Forms.TileDataHelpForm.Items.Value.Title") ?? "Value",
                GetLocalized("Forms.TileDataHelpForm.Items.Value.Description")
                ?? "This field is for the value of the item.", "items");
            Add(GetLocalized("Forms.TileDataHelpForm.Items.StackingOffset.Title") ?? "Stacking Offset",
                GetLocalized("Forms.TileDataHelpForm.Items.StackingOffset.Description")
                ?? "StackOff refers to the stacking offset in pixels when multiple items are stacked.\nA higher StackOff value means the items will appear further apart from each other within the stack.", "items");
            Add(GetLocalized("Forms.TileDataHelpForm.Items.Hue.Title") ?? "Hue",
                GetLocalized("Forms.TileDataHelpForm.Items.Hue.Description")
                ?? "This field is for the hue (color) of the item.", "items");
            Add(GetLocalized("Forms.TileDataHelpForm.Items.Unknown2.Title") ?? "Unknown 2",
                GetLocalized("Forms.TileDataHelpForm.Items.Unknown2.Description")
                ?? "This field is for the second unknown value.", "items");
            Add(GetLocalized("Forms.TileDataHelpForm.Items.MiscData.Title") ?? "Misc Data",
                GetLocalized("Forms.TileDataHelpForm.Items.MiscData.Description")
                ?? "Old UO Demo weapon template definition", "items");
            Add(GetLocalized("Forms.TileDataHelpForm.Items.Height.Title") ?? "Height",
                GetLocalized("Forms.TileDataHelpForm.Items.Height.Description")
                ?? "This field is for the height of the item.", "items");
            Add(GetLocalized("Forms.TileDataHelpForm.Items.Unknown3.Title") ?? "Unknown 3",
                GetLocalized("Forms.TileDataHelpForm.Items.Unknown3.Description")
                ?? "This field is for the third unknown value.", "items");

            // Land Tiles section
            AddHeader(GetLocalized("Forms.TileDataHelpForm.LandTiles.Title") ?? "Land Tiles");
            Add(GetLocalized("Forms.TileDataHelpForm.LandTiles.Name.Title") ?? "Name",
                GetLocalized("Forms.TileDataHelpForm.LandTiles.Name.Description")
                ?? "This field is for the name of the land tile, which can be a maximum of 20 characters.", "land");
            Add(GetLocalized("Forms.TileDataHelpForm.LandTiles.TextureID.Title") ?? "Texture ID",
                GetLocalized("Forms.TileDataHelpForm.LandTiles.TextureID.Description")
                ?? "This field is for the texture ID associated with the land tile.", "land");

            // Flags section
            AddHeader(GetLocalized("Forms.TileDataHelpForm.Flags.Title") ?? "Flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Background.Title") ?? "Background",    GetLocalized("Forms.TileDataHelpForm.Flags.Background.Description") ?? "Not yet documented.",                                                               "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Weapon.Title") ?? "Weapon",        GetLocalized("Forms.TileDataHelpForm.Flags.Weapon.Description") ?? "Not yet documented.",                                                               "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Transparent.Title") ?? "Transparent",   GetLocalized("Forms.TileDataHelpForm.Flags.Transparent.Description") ?? "Not yet documented.",                                                               "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Translucent.Title") ?? "Translucent",   GetLocalized("Forms.TileDataHelpForm.Flags.Translucent.Description") ?? "The tile is rendered with partial alpha-transparency.",                             "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Wall.Title") ?? "Wall",          GetLocalized("Forms.TileDataHelpForm.Flags.Wall.Description") ?? "The tile is a wall.",                                                               "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Damaging.Title") ?? "Damaging",      GetLocalized("Forms.TileDataHelpForm.Flags.Damaging.Description") ?? "The tile can cause damage when moved over.",                                        "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Impassable.Title") ?? "Impassable",    GetLocalized("Forms.TileDataHelpForm.Flags.Impassable.Description") ?? "The tile may not be moved over or through.",                                        "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Wet.Title") ?? "Wet",           GetLocalized("Forms.TileDataHelpForm.Flags.Wet.Description") ?? "Not yet documented.",                                                               "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unknown1.Title") ?? "Unknown1",      GetLocalized("Forms.TileDataHelpForm.Flags.Unknown1.Description") ?? "Unknown.",                                                                          "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Surface.Title") ?? "Surface",       GetLocalized("Forms.TileDataHelpForm.Flags.Surface.Description") ?? "The tile is a surface. It may be moved over, but not through.",                     "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Bridge.Title") ?? "Bridge",        GetLocalized("Forms.TileDataHelpForm.Flags.Bridge.Description") ?? "The tile is a stair, ramp, or ladder.",                                             "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Generic.Title") ?? "Generic",       GetLocalized("Forms.TileDataHelpForm.Flags.Generic.Description") ?? "The tile is stackable.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Window.Title") ?? "Window",        GetLocalized("Forms.TileDataHelpForm.Flags.Window.Description") ?? "The tile is a window. Like NoShoot, tiles with this flag block line of sight.",     "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.NoShoot.Title") ?? "NoShoot",       GetLocalized("Forms.TileDataHelpForm.Flags.NoShoot.Description") ?? "The tile blocks line of sight.",                                                    "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.ArticleA.Title") ?? "ArticleA",      GetLocalized("Forms.TileDataHelpForm.Flags.ArticleA.Description") ?? "For single-amount tiles, the string \"a \" should be prepended to the tile name.",  "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.ArticleAn.Title") ?? "ArticleAn",     GetLocalized("Forms.TileDataHelpForm.Flags.ArticleAn.Description") ?? "For single-amount tiles, the string \"an \" should be prepended to the tile name.", "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.ArticleThe.Title") ?? "ArticleThe",    GetLocalized("Forms.TileDataHelpForm.Flags.ArticleThe.Description") ?? "Probably article \"The\" prepended to the tile name.",                              "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Foliage.Title") ?? "Foliage",       GetLocalized("Forms.TileDataHelpForm.Flags.Foliage.Description") ?? "The tile becomes translucent when walked behind. Boat masts also have this flag.",  "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.PartialHue.Title") ?? "PartialHue",    GetLocalized("Forms.TileDataHelpForm.Flags.PartialHue.Description") ?? "Only gray pixels will be hued.",                                                    "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.NoHouse.Title") ?? "NoHouse",       GetLocalized("Forms.TileDataHelpForm.Flags.NoHouse.Description") ?? "NoHouse or Unknown. Needs further research.",                                       "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Map.Title") ?? "Map",           GetLocalized("Forms.TileDataHelpForm.Flags.Map.Description") ?? "The tile is a map in the cartography sense. Unknown usage.",                        "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Container.Title") ?? "Container",     GetLocalized("Forms.TileDataHelpForm.Flags.Container.Description") ?? "The tile is a container.",                                                          "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Wearable.Title") ?? "Wearable",      GetLocalized("Forms.TileDataHelpForm.Flags.Wearable.Description") ?? "The tile may be equipped.",                                                         "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.LightSource.Title") ?? "LightSource",   GetLocalized("Forms.TileDataHelpForm.Flags.LightSource.Description") ?? "The tile gives off light.",                                                         "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Animation.Title") ?? "Animation",     GetLocalized("Forms.TileDataHelpForm.Flags.Animation.Description") ?? "The tile is animated.",                                                             "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.HoverOver.Title") ?? "HoverOver",     GetLocalized("Forms.TileDataHelpForm.Flags.HoverOver.Description") ?? "Gargoyles can fly over, or NoDiagonal.",                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.NoDiagonal.Title") ?? "NoDiagonal",    GetLocalized("Forms.TileDataHelpForm.Flags.NoDiagonal.Description") ?? "NoDiagonal (Unknown3).",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Armor.Title") ?? "Armor",         GetLocalized("Forms.TileDataHelpForm.Flags.Armor.Description") ?? "Not yet documented.",                                                               "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Roof.Title") ?? "Roof",          GetLocalized("Forms.TileDataHelpForm.Flags.Roof.Description") ?? "The tile is a slanted roof.",                                                       "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Door.Title") ?? "Door",          GetLocalized("Forms.TileDataHelpForm.Flags.Door.Description") ?? "The tile is a door. Tiles with this flag can be moved through by ghosts and GMs.",  "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.StairBack.Title") ?? "StairBack",     GetLocalized("Forms.TileDataHelpForm.Flags.StairBack.Description") ?? "Not yet documented.",                                                               "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.StairRight.Title") ?? "StairRight",    GetLocalized("Forms.TileDataHelpForm.Flags.StairRight.Description") ?? "Not yet documented.",                                                               "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.AlphaBlend.Title") ?? "AlphaBlend",    GetLocalized("Forms.TileDataHelpForm.Flags.AlphaBlend.Description") ?? "Blend Alphas, tile blending.",                                                      "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.UseNewArt.Title") ?? "UseNewArt",     GetLocalized("Forms.TileDataHelpForm.Flags.UseNewArt.Description") ?? "Uses new art style? Something related to the nodraw tile?",                         "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.ArtUsed.Title") ?? "ArtUsed",       GetLocalized("Forms.TileDataHelpForm.Flags.ArtUsed.Description") ?? "Has art being used?",                                                               "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused8.Title") ?? "Unused8",       GetLocalized("Forms.TileDataHelpForm.Flags.Unused8.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.NoShadow.Title") ?? "NoShadow",      GetLocalized("Forms.TileDataHelpForm.Flags.NoShadow.Description") ?? "Disallow shadow on this tile, light source? lava?",                                 "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.PixelBleed.Title") ?? "PixelBleed",    GetLocalized("Forms.TileDataHelpForm.Flags.PixelBleed.Description") ?? "Let pixels bleed in to other tiles? Is this Disabling Texture Clamp?",              "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.PlayAnimOnce.Title") ?? "PlayAnimOnce",  GetLocalized("Forms.TileDataHelpForm.Flags.PlayAnimOnce.Description") ?? "Play tile animation once.",                                                         "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.MultiMovable.Title") ?? "MultiMovable",  GetLocalized("Forms.TileDataHelpForm.Flags.MultiMovable.Description") ?? "Movable multi? Cool ships and vehicles etc? Something related to the masts.",       "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused10.Title") ?? "Unused10",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused10.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused11.Title") ?? "Unused11",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused11.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused12.Title") ?? "Unused12",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused12.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused13.Title") ?? "Unused13",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused13.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused14.Title") ?? "Unused14",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused14.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused15.Title") ?? "Unused15",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused15.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused16.Title") ?? "Unused16",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused16.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused17.Title") ?? "Unused17",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused17.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused18.Title") ?? "Unused18",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused18.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused19.Title") ?? "Unused19",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused19.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused20.Title") ?? "Unused20",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused20.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused21.Title") ?? "Unused21",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused21.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused22.Title") ?? "Unused22",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused22.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused23.Title") ?? "Unused23",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused23.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused24.Title") ?? "Unused24",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused24.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused25.Title") ?? "Unused25",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused25.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused26.Title") ?? "Unused26",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused26.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused27.Title") ?? "Unused27",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused27.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused28.Title") ?? "Unused28",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused28.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused29.Title") ?? "Unused29",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused29.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused30.Title") ?? "Unused30",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused30.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused31.Title") ?? "Unused31",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused31.Description") ?? "Unused or unknown yet.",                                                            "flags");
            Add(GetLocalized("Forms.TileDataHelpForm.Flags.Unused32.Title") ?? "Unused32",      GetLocalized("Forms.TileDataHelpForm.Flags.Unused32.Description") ?? "Unused or unknown yet.",                                                            "flags");

            if (_listView.Items.Count > 0)
            {
                foreach (ListViewItem li in _listView.Items)
                {
                    if (li.Tag is string)
                    {
                        li.Selected = true;
                        break;
                    }
                }
            }
        }

        private void AddHeader(string text)
        {
            var item = new ListViewItem(text)
            {
                Font = new Font(_listView.Font, FontStyle.Bold),
                ForeColor = Options.DarkMode ? Color.OrangeRed : Color.MediumBlue,
            };
            _listView.Items.Add(item);
        }

        private void Add(string field, string description, string groupKey)
        {
            var item = new ListViewItem(field) { Tag = description };
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
