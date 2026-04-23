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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class VerdataControl : UserControl
    {
        public VerdataControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        private static bool _loaded;
        private int _currentAnimBody = -1;
        private bool _suppressAnimEvents;
        private Entry5D[] _customPatches;

        private Entry5D[] ActivePatches => _customPatches ?? Verdata.Patches;

        // Action names by body type — mirrors AnimationListControl.GetActionNames
        // [0] Monster (body 0–199):   22 actions
        // [1] Animal  (body 200–399): 13 actions
        // [2] Human   (body 400+):    35 actions
        private static readonly string[][] ActionNames =
        {
            // Monster — 22 actions
            new[]
            {
                "Walk", "Idle", "Die1", "Die2", "Attack1", "Attack2", "Attack3",
                "AttackBow", "AttackCrossBow", "AttackThrow", "GetHit", "Pillage",
                "Stomp", "Cast2", "Cast3", "BlockRight", "BlockLeft", "Idle",
                "Fidget", "Fly", "TakeOff", "GetHitInAir"
            },
            // Animal — 13 actions
            new[]
            {
                "Walk", "Run", "Idle", "Eat", "Alert", "Attack1", "Attack2",
                "GetHit", "Die1", "Idle", "Fidget", "LieDown", "Die2"
            },
            // Human — 35 actions
            new[]
            {
                "Walk_01", "WalkStaff_01", "Run_01", "RunStaff_01", "Idle_01", "Idle_01",
                "Fidget_Yawn_Stretch_01", "CombatIdle1H_01", "CombatIdle1H_01",
                "AttackSlash1H_01", "AttackPierce1H_01", "AttackBash1H_01",
                "AttackBash2H_01", "AttackSlash2H_01", "AttackPierce2H_01",
                "CombatAdvance_1H_01", "Spell1", "Spell2", "AttackBow_01",
                "AttackCrossbow_01", "GetHit_Fr_Hi_01", "Die_Hard_Fwd_01",
                "Die_Hard_Back_01", "Horse_Walk_01", "Horse_Run_01", "Horse_Idle_01",
                "Horse_Attack1H_SlashRight_01", "Horse_AttackBow_01",
                "Horse_AttackCrossbow_01", "Horse_Attack2H_SlashRight_01",
                "Block_Shield_Hard_01", "Punch_Punch_Jab_01", "Bow_Lesser_01",
                "Salute_Armed1h_01", "Ingest_Eat_01"
            }
        };

        private static readonly (string Name, int[] FileIds)[] FileTypeGroups =
        {
            ("Art (art.mul)", new[] { 3, 4 }),
            ("Gumps (gumpart.mul)", new[] { 11, 12 }),
            ("Textures (texmaps.mul)", new[] { 9, 10 }),
            ("TileData (tiledata.mul)", new[] { 30 }),
            ("AnimData (animdata.mul)", new[] { 31 }),
            ("Map (map0.mul)", new[] { 0 }),
            ("Statics (statics0.mul)", new[] { 1, 2 }),
            ("Animations (anim.mul)", new[] { 5, 6 }),
            ("Sounds (sound.mul)", new[] { 7, 8 }),
            ("Multi (multi.mul)", new[] { 13, 14 }),
            ("Skills (skills.mul)", new[] { 15, 16 }),
        };

        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Populate();

            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                ControlEvents.PreviewBackgroundColorChangeEvent += OnPreviewBackgroundColorChanged;

                pictureBoxPreview.BackColor = Options.PreviewBackgroundColor;
                panelMultiScroll.BackColor = Options.PreviewBackgroundColor;
            }

            _loaded = true;
        }

        private void OnFilePathChangeEvent()
        {
            // Client path changed — drop any custom file and go back to the default
            _customPatches = null;
            labelCurrentFile.Text = string.Empty;
            Populate();
        }

        private void ChangeBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            Options.PreviewBackgroundColor = colorDialog.Color;
            ControlEvents.FirePreviewBackgroundColorChangeEvent();
        }

        private void OnPreviewBackgroundColorChanged()
        {
            pictureBoxPreview.BackColor = Options.PreviewBackgroundColor;
            panelMultiScroll.BackColor = Options.PreviewBackgroundColor;
        }

        private void Populate()
        {
            listBoxType.BeginUpdate();
            listBoxType.Items.Clear();
            listBoxPatches.Items.Clear();
            ClearPreview();

            Entry5D[] patches = ActivePatches;

            if (patches.Length == 0)
            {
                labelCount.Text = "No patches found";
                listBoxType.EndUpdate();
                return;
            }

            var knownFileIds = new HashSet<int>(FileTypeGroups.SelectMany(g => g.FileIds));

            foreach (var (name, fileIds) in FileTypeGroups)
            {
                var groupPatches = patches.Where(p => fileIds.Contains(p.File)).ToList();
                if (groupPatches.Count > 0)
                {
                    listBoxType.Items.Add(new TypeGroupItem(name, fileIds, groupPatches));
                }
            }

            var unknownFileIdList = patches
                .Where(p => !knownFileIds.Contains(p.File))
                .Select(p => p.File)
                .Distinct()
                .OrderBy(id => id);

            foreach (int fileId in unknownFileIdList)
            {
                var groupPatches = patches.Where(p => p.File == fileId).ToList();
                listBoxType.Items.Add(new TypeGroupItem($"File {fileId}", new[] { fileId }, groupPatches));
            }

            listBoxType.EndUpdate();

            if (listBoxType.Items.Count > 0)
            {
                listBoxType.SelectedIndex = 0;
            }
        }

        private void OnTypeSelected(object sender, EventArgs e)
        {
            if (listBoxType.SelectedItem is not TypeGroupItem group)
            {
                return;
            }

            listBoxPatches.BeginUpdate();
            listBoxPatches.Items.Clear();

            foreach (var patch in group.Patches.OrderBy(p => p.Index))
            {
                listBoxPatches.Items.Add(new PatchItem(patch));
            }

            labelCount.Text = $"Patches: {group.Patches.Count}";
            listBoxPatches.EndUpdate();

            ClearPreview();
        }

        private void OnPatchSelected(object sender, EventArgs e)
        {
            if (listBoxPatches.SelectedItem is not PatchItem item)
            {
                return;
            }

            ShowPatch(item.Patch);
        }

        private void ShowPatch(Entry5D patch)
        {
            labelFile.Text = $"File:   {patch.File}  ({GetFileName(patch.File)})";
            labelIndex.Text = $"Index:  0x{patch.Index:X4}  ({patch.Index})";
            labelLookup.Text = $"Lookup: 0x{patch.Lookup:X}";
            labelLength.Text = $"Length: {patch.Length}";
            labelExtra.Text = $"Extra:  0x{patch.Extra:X}";

            int fileId = patch.File;
            int index = patch.Index;

            if (fileId is 3 or 4)
            {
                ShowArtPreview(index);
            }
            else if (fileId is 11 or 12)
            {
                ShowGumpPreview(index);
            }
            else if (fileId is 9 or 10)
            {
                ShowTexturePreview(index);
            }
            else if (fileId is 5 or 6)
            {
                ShowAnimationPreview(index);
            }
            else if (fileId is 13 or 14)
            {
                ShowMultiPreview(index);
            }
            else if (fileId is 15 or 16)
            {
                ShowSkillDetails(index);
            }
            else if (fileId == 30)
            {
                ShowTileDataDetails(index);
            }
            else if (fileId == 31)
            {
                ShowAnimDataDetails(index);
            }
            else
            {
                ShowNoPreview(fileId);
            }
        }

        private void ShowAnimationPreview(int index)
        {
            Options.LoadedUltimaClass["Animations"] = true;

            var (body, action, direction) = DecodeAnimIndex(index);
            labelDecoded.Text = $"Body: {body}  Action: {action}  Dir: {direction}";

            _currentAnimBody = body;

            // Select action name array based on body range
            string[] names;
            if (body < 200)
            {
                names = ActionNames[0]; // Monster — 22 actions
            }
            else if (body < 400)
            {
                names = ActionNames[1]; // Animal — 13 actions
            }
            else
            {
                names = ActionNames[2]; // Human — 35 actions
            }

            // Populate action list without triggering LoadAnimationFrames
            _suppressAnimEvents = true;
            listBoxActions.BeginUpdate();
            listBoxActions.Items.Clear();
            for (int i = 0; i < names.Length; i++)
            {
                listBoxActions.Items.Add($"{i} {names[i]}");
            }
            listBoxActions.EndUpdate();

            trackBarDirection.Value = Math.Max(0, Math.Min(direction, trackBarDirection.Maximum));
            labelDirValue.Text = trackBarDirection.Value.ToString();
            listBoxActions.SelectedIndex = Math.Max(0, Math.Min(action, listBoxActions.Items.Count - 1));
            _suppressAnimEvents = false;

            // Show panel and stop any previous animation
            animatedPictureBox.Animate = false;
            buttonPlayStop.Text = "Play";

            richTextBoxDetails.Visible = false;
            pictureBoxPreview.Visible = false;
            panelAnimation.Visible = true;

            LoadAnimationFrames();
        }

        private void LoadAnimationFrames()
        {
            if (_currentAnimBody < 0)
            {
                return;
            }

            int action = listBoxActions.SelectedIndex >= 0 ? listBoxActions.SelectedIndex : 0;
            int dir = trackBarDirection.Value;

            int hue = 0;
            var frames = Animations.GetAnimation(_currentAnimBody, action, dir, ref hue, false, false)
                ?.Select(f => new AnimatedFrame(f.Bitmap, f.Center)).ToList();

            bool wasAnimating = animatedPictureBox.Animate;
            animatedPictureBox.Animate = false;

            animatedPictureBox.Frames = frames == null || frames.Count == 0 ? null : frames;

            if (wasAnimating && animatedPictureBox.Frames?.Count > 0)
            {
                animatedPictureBox.Animate = true;
            }
        }

        private void OnClickPlayStop(object sender, EventArgs e)
        {
            if (animatedPictureBox.Frames == null || animatedPictureBox.Frames.Count == 0)
            {
                return;
            }

            animatedPictureBox.Animate = !animatedPictureBox.Animate;
            buttonPlayStop.Text = animatedPictureBox.Animate ? "Stop" : "Play";
        }

        private void OnActionSelected(object sender, EventArgs e)
        {
            if (_suppressAnimEvents)
            {
                return;
            }

            LoadAnimationFrames();
        }

        private void OnScrollDirection(object sender, EventArgs e)
        {
            if (_suppressAnimEvents)
            {
                return;
            }

            labelDirValue.Text = trackBarDirection.Value.ToString();
            LoadAnimationFrames();
        }

        private static (int body, int action, int direction) DecodeAnimIndex(int index)
        {
            // Reverse of GetFileIndex for fileType 1 (anim.mul, body ranges from Animations.cs)
            int body, inner;
            if (index < 22000)
            {
                body = index / 110;
                inner = index % 110;
            }
            else if (index < 35000)
            {
                body = 200 + (index - 22000) / 65;
                inner = (index - 22000) % 65;
            }
            else
            {
                body = 400 + (index - 35000) / 175;
                inner = (index - 35000) % 175;
            }

            int action = inner / 5;
            int direction = inner % 5;
            return (body, action, direction);
        }

        private void ShowArtPreview(int index)
        {
            Options.LoadedUltimaClass["Art"] = true;

            Bitmap bmp;
            string info;

            if (index < 0x4000)
            {
                bmp = Art.GetLand(index);
                info = $"Land Tile  ID: 0x{index:X4}";
            }
            else
            {
                int staticIndex = index - 0x4000;
                bmp = Art.GetStatic(staticIndex);
                info = $"Static Item  ID: 0x{staticIndex:X4}";
            }

            ShowImage(bmp, info);
        }

        private void ShowGumpPreview(int index)
        {
            Options.LoadedUltimaClass["Gumps"] = true;
            var bmp = Gumps.GetGump(index);
            ShowImage(bmp, $"Gump  ID: 0x{index:X4}");
        }

        private void ShowTexturePreview(int index)
        {
            Options.LoadedUltimaClass["Texture"] = true;
            var bmp = Textures.GetTexture(index);
            ShowImage(bmp, $"Texture  ID: 0x{index:X4}");
        }

        private void ShowMultiPreview(int index)
        {
            Options.LoadedUltimaClass["Multis"] = true;

            var multi = Multis.GetComponents(index);
            if (multi == MultiComponentList.Empty || multi.Width == 0)
            {
                ShowNoPreview(-1, $"Multi  ID: 0x{index:X4}  (no data)");
                return;
            }

            // Preview tab — original-size image in scrollable panel
            var bmp = multi.GetImage();
            pictureBoxMulti.Image = bmp;

            // Components tab — same format as MultisControl.ChangeComponentList
            richTextBoxMultiComponents.Clear();
            bool isUohsa = Art.IsUOAHS();
            for (int x = 0; x < multi.Width; ++x)
            {
                for (int y = 0; y < multi.Height; ++y)
                {
                    foreach (var mTile in multi.Tiles[x][y])
                    {
                        richTextBoxMultiComponents.AppendText(
                            isUohsa
                                ? $"0x{mTile.Id:X4} {x,3} {y,3} {mTile.Z,2} {mTile.Flag,2} {mTile.Unk1,2}\n"
                                : $"0x{mTile.Id:X4} {x,3} {y,3} {mTile.Z,2} {mTile.Flag,2}\n");
                    }
                }
            }

            richTextBoxDetails.Visible = false;
            pictureBoxPreview.Visible = false;
            panelAnimation.Visible = false;
            animatedPictureBox.Animate = false;
            panelMulti.Visible = true;
        }

        private void ShowSkillDetails(int index)
        {
            Options.LoadedUltimaClass["Skills"] = true;

            labelDecoded.Text = string.Empty;
            panelAnimation.Visible = false;
            animatedPictureBox.Animate = false;
            panelMulti.Visible = false;
            pictureBoxPreview.Visible = false;
            richTextBoxDetails.Visible = true;

            var sb = new StringBuilder();
            var skill = Skills.GetSkill(index);

            if (skill != null)
            {
                sb.AppendLine($"=== Skill  Index: {index} (0x{index:X4}) ===");
                sb.AppendLine();
                sb.AppendLine($"Name:     {skill.Name}");
                sb.AppendLine($"IsAction: {skill.IsAction}");
                sb.AppendLine($"Extra:    0x{skill.Extra:X} ({skill.Extra})");
            }
            else
            {
                sb.AppendLine($"No skill entry found for index {index} (0x{index:X4}).");
            }

            richTextBoxDetails.Text = sb.ToString();
        }

        private void ShowTileDataDetails(int index)
        {
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;

            labelDecoded.Text = string.Empty;
            panelAnimation.Visible = false;
            animatedPictureBox.Animate = false;
            panelMulti.Visible = false;
            pictureBoxPreview.Visible = false;
            richTextBoxDetails.Visible = true;

            var sb = new StringBuilder();

            if (index < 0x4000 && TileData.LandTable != null && index < TileData.LandTable.Length)
            {
                var land = TileData.LandTable[index];
                sb.AppendLine($"=== Land Tile  ID: 0x{index:X4} ({index}) ===");
                sb.AppendLine();
                sb.AppendLine($"Name:      {land.Name}");
                sb.AppendLine($"Flags:     0x{(ulong)land.Flags:X16}");
                sb.AppendLine($"           {land.Flags}");
                sb.AppendLine($"TextureID: 0x{land.TextureId:X4} ({land.TextureId})");
            }
            else
            {
                int itemIndex = index - 0x4000;
                if (TileData.ItemTable != null && itemIndex >= 0 && itemIndex < TileData.ItemTable.Length)
                {
                    var item = TileData.ItemTable[itemIndex];
                    sb.AppendLine($"=== Item Tile  ID: 0x{index:X4}  Item Index: 0x{itemIndex:X4} ({itemIndex}) ===");
                    sb.AppendLine();
                    sb.AppendLine($"Name:      {item.Name}");
                    sb.AppendLine($"Flags:     0x{(ulong)item.Flags:X16}");
                    sb.AppendLine($"           {item.Flags}");
                    sb.AppendLine($"Animation: 0x{item.Animation:X4} ({item.Animation})");
                    sb.AppendLine($"Weight:    {item.Weight}");
                    sb.AppendLine($"Quality:   {item.Quality}");
                    sb.AppendLine($"Height:    {item.Height}");
                    sb.AppendLine($"Hue:       {item.Hue}");
                }
                else
                {
                    sb.AppendLine($"Index 0x{index:X4} is out of range.");
                    sb.AppendLine($"Land table: {TileData.LandTable?.Length ?? 0} entries");
                    sb.AppendLine($"Item table: {TileData.ItemTable?.Length ?? 0} entries");
                }
            }

            richTextBoxDetails.Text = sb.ToString();
        }

        private void ShowAnimDataDetails(int index)
        {
            Options.LoadedUltimaClass["Animdata"] = true;

            labelDecoded.Text = string.Empty;
            panelAnimation.Visible = false;
            animatedPictureBox.Animate = false;
            panelMulti.Visible = false;
            pictureBoxPreview.Visible = false;
            richTextBoxDetails.Visible = true;

            var sb = new StringBuilder();

            if (Animdata.AnimData != null && Animdata.AnimData.TryGetValue(index, out var entry))
            {
                sb.AppendLine($"=== AnimData Entry  ID: 0x{index:X4} ({index}) ===");
                sb.AppendLine();
                sb.AppendLine($"Frame Count:    {entry.FrameCount}");
                sb.AppendLine($"Frame Interval: {entry.FrameInterval}");
                sb.AppendLine($"Frame Start:    {entry.FrameStart}");
                sb.AppendLine();
                sb.AppendLine("Frames:");
                for (int i = 0; i < entry.FrameCount && i < entry.FrameData.Length; i++)
                {
                    int frameGraphic = index + entry.FrameData[i];
                    sb.AppendLine($"  [{i}]  offset={entry.FrameData[i],4:+0;-0;+0}  graphic=0x{frameGraphic:X4} ({frameGraphic})");
                }
            }
            else
            {
                sb.AppendLine($"No AnimData entry found for index 0x{index:X4} ({index}).");
                sb.AppendLine("Note: AnimData entries are only present for animated items.");
            }

            richTextBoxDetails.Text = sb.ToString();
        }

        private void ShowImage(Bitmap bmp, string info)
        {
            panelAnimation.Visible = false;
            animatedPictureBox.Animate = false;
            panelMulti.Visible = false;

            if (bmp == null)
            {
                pictureBoxPreview.Visible = false;
                richTextBoxDetails.Visible = true;
                richTextBoxDetails.Text = $"{info}\n\nNo image available.";
            }
            else
            {
                richTextBoxDetails.Visible = false;
                pictureBoxPreview.Visible = true;
                pictureBoxPreview.Image = bmp;
                pictureBoxPreview.BackColor = Options.PreviewBackgroundColor;
            }
        }

        private void ShowNoPreview(int fileId)
        {
            panelAnimation.Visible = false;
            animatedPictureBox.Animate = false;
            panelMulti.Visible = false;
            pictureBoxPreview.Visible = false;
            richTextBoxDetails.Visible = true;
            richTextBoxDetails.Text = $"No type-specific preview available for File ID {fileId} ({GetFileName(fileId)}).";
        }

        private void ShowNoPreview(int fileId, string message)
        {
            panelAnimation.Visible = false;
            animatedPictureBox.Animate = false;
            panelMulti.Visible = false;
            pictureBoxPreview.Visible = false;
            richTextBoxDetails.Visible = true;
            richTextBoxDetails.Text = fileId >= 0
                ? $"File ID {fileId} ({GetFileName(fileId)}): {message}"
                : message;
        }

        private void ClearPreview()
        {
            labelFile.Text = "File:";
            labelIndex.Text = "Index:";
            labelLookup.Text = "Lookup:";
            labelLength.Text = "Length:";
            labelExtra.Text = "Extra:";
            labelDecoded.Text = string.Empty;
            _currentAnimBody = -1;
            animatedPictureBox.Animate = false;
            animatedPictureBox.Frames = null;
            panelAnimation.Visible = false;
            buttonPlayStop.Text = "Play";
            panelMulti.Visible = false;
            pictureBoxMulti.Image = null;
            richTextBoxMultiComponents.Text = string.Empty;
            pictureBoxPreview.Image = null;
            pictureBoxPreview.Visible = false;
            richTextBoxDetails.Text = string.Empty;
            richTextBoxDetails.Visible = true;
        }

        private void OnClickLoadFile(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Open verdata.mul",
                Filter = "verdata.mul|verdata.mul|MUL files (*.mul)|*.mul|All files (*.*)|*.*",
                FilterIndex = 1
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            _customPatches = LoadVerdataFromFile(dialog.FileName);
            labelCurrentFile.Text = dialog.FileName;
            Populate();
        }

        private static Entry5D[] LoadVerdataFromFile(string path)
        {
            using var fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
            using var bin = new System.IO.BinaryReader(fs);

            int count = bin.ReadInt32();
            var patches = new Entry5D[count];
            for (int i = 0; i < count; i++)
            {
                patches[i].File = bin.ReadInt32();
                patches[i].Index = bin.ReadInt32();
                patches[i].Lookup = bin.ReadInt32();
                patches[i].Length = bin.ReadInt32();
                patches[i].Extra = bin.ReadInt32();
            }
            return patches;
        }

        private void OnClickReload(object sender, EventArgs e)
        {
            Populate();
        }

        private static string GetFileName(int fileId) => fileId switch
        {
            0 => "map0.mul",
            1 => "staidx0.mul",
            2 => "statics0.mul",
            3 => "artidx.mul",
            4 => "art.mul",
            5 => "anim.idx",
            6 => "anim.mul",
            7 => "soundidx.mul",
            8 => "sound.mul",
            9 => "texidx.mul",
            10 => "texmaps.mul",
            11 => "gumpidx.mul",
            12 => "gumpart.mul",
            13 => "multi.idx",
            14 => "multi.mul",
            15 => "skills.idx",
            16 => "skills.mul",
            30 => "tiledata.mul",
            31 => "animdata.mul",
            _ => $"unknown"
        };

        private sealed class TypeGroupItem
        {
            public string Name { get; }
            public int[] FileIds { get; }
            public List<Entry5D> Patches { get; }

            public TypeGroupItem(string name, int[] fileIds, List<Entry5D> patches)
            {
                Name = name;
                FileIds = fileIds;
                Patches = patches;
            }

            public override string ToString() => $"{Name} ({Patches.Count})";
        }

        private sealed class PatchItem
        {
            public Entry5D Patch { get; }

            public PatchItem(Entry5D patch)
            {
                Patch = patch;
            }

            public override string ToString() => $"0x{Patch.Index:X4}  (file {Patch.File})";
        }
    }
}
