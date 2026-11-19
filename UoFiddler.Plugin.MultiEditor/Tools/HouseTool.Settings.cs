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
using System.Xml.Linq;
using UoFiddler.Plugin.MultiEditor.House;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    internal sealed partial class HouseTool
    {
        // ---------------------------------------------------------------- changes to the draft

        // What the last step of the history changed, when steps of that kind run together (a
        // number stepped up and up is one step to take back, not ten).
        private string _lastRun;

        /// <summary>
        /// Runs a change to the draft as one step of its history. Changes given the same
        /// <paramref name="run"/> name one after another are one step together.
        /// </summary>
        private void Edit(Action change, string run = null)
        {
            if (run == null || run != _lastRun)
            {
                Remember();
            }

            _lastRun = run;
            change();
            ++_version;
            RaiseChanged();
        }

        /// <summary>The styles were read again: the draft takes the new style objects in place of the ones it holds.</summary>
        public void StylesReloaded()
        {
            if (Context.HouseStyles == null)
            {
                return;
            }

            _draft.Remap(Context.HouseStyles.Successor);
            RemapCopies(copied => (copied as HouseDraft)?.Remap(Context.HouseStyles.Successor));
            ++_version;
            RaiseChanged();
        }

        /// <summary>Which gable ends of the house's roofs are closed, and with what.</summary>
        public void SetGables(RoofGables gables, RoofGableFill fill)
        {
            if (_draft.Gables != gables || _draft.GableFill != fill)
            {
                Edit(() => (_draft.Gables, _draft.GableFill) = (gables, fill));
            }
        }

        /// <summary>Sets the storeys of the selected wing, and of the wings drawn next.</summary>
        public void SetStoreys(int storeys)
        {
            storeys = Math.Clamp(storeys, 1, HouseDraft.MaxStoreys);
            _storeys = storeys;
            if (SelectedWing is { } wing && wing.Storeys != storeys)
            {
                Edit(() => wing.Storeys = storeys);
            }
            else
            {
                RaiseChanged();
            }
        }

        /// <summary>Sets the roof of the selected wing, and of the wings drawn next: a shape, or none.</summary>
        public void SetRoof(RoofShape shape, bool none)
        {
            (_shape, _noRoof) = (shape, none);
            if (SelectedWing is { } wing && (wing.Shape != shape || wing.NoRoof != none))
            {
                Edit(() => (wing.Shape, wing.NoRoof) = (shape, none));
            }
            else
            {
                RaiseChanged();
            }
        }

        public void SetRidge(RoofRidge ridge)
        {
            _ridge = ridge;
            if (SelectedWing is { } wing)
            {
                Edit(() => SetWingRidge(wing, ridge));
            }
            else
            {
                RaiseChanged();
            }
        }

        /// <summary>Moves the whole house up or down.</summary>
        public void SetBaseZ(int z)
        {
            z = Math.Clamp(z, -100, 100);
            if (Wings.Count == 0 || z == BaseZ)
            {
                return;
            }

            Edit(() =>
            {
                foreach (RoofWing wing in Wings)
                {
                    wing.EaveZ = z;
                }
            }, "baseZ");
        }

        /// <summary>
        /// Sets the rooms and their layout for a storey of the selected wing: for that storey
        /// alone when it has rooms of its own (<see cref="SetOwnRooms"/>), else for the ground
        /// floor and every storey that follows it. Without a selected wing it is for the wings drawn next.
        /// </summary>
        public void SetRooms(int rooms, RoomLayout layout, int storey)
        {
            rooms = Math.Clamp(rooms, 1, HouseDraft.MaxRooms);
            if (SelectedWing is not { } wing)
            {
                (_rooms, _layout) = (rooms, layout);
                RaiseChanged();
                return;
            }

            if (wing.RoomsOn(storey) == (rooms, layout))
            {
                return;
            }

            Edit(() =>
            {
                if (storey >= 1 && storey < HouseDraft.MaxStoreys && wing.RoomsAbove[storey] != null)
                {
                    wing.RoomsAbove[storey] = (rooms, layout);
                }
                else
                {
                    (wing.Rooms, wing.Layout) = (rooms, layout);
                    (_rooms, _layout) = (rooms, layout);
                }
            });
        }

        /// <summary>Gives a storey above the ground floor rooms of its own, or lets it follow the storey below again.</summary>
        public void SetOwnRooms(int storey, bool own)
        {
            if (SelectedWing is not { } wing || storey < 1 || storey >= HouseDraft.MaxStoreys || (wing.RoomsAbove[storey] != null) == own)
            {
                return;
            }

            Edit(() => wing.RoomsAbove[storey] = own ? wing.RoomsOn(storey) : null);
        }

        /// <summary>True when the selected wing, else the wings drawn next, keeps its walls against other wings.</summary>
        public bool Walled => SelectedWing?.Walled ?? _walled;

        /// <summary>Lets the selected wing, and the wings drawn next, keep its walls where it meets other wings.</summary>
        public void SetWalled(bool walled)
        {
            _walled = walled;
            if (SelectedWing is { } wing && wing.Walled != walled)
            {
                Edit(() => wing.Walled = walled);
            }
            else
            {
                RaiseChanged();
            }
        }

        /// <summary>The fortified houses a draft can be started from.</summary>
        public static readonly string[] Presets = { "Tower", "Keep", "Castle" };

        /// <summary>
        /// Makes a tower, keep or castle the draft, sized to the multi: flat roofs with
        /// battlements, corner towers that keep their walls, a front door and windows. It is an
        /// ordinary draft from there on, and one step of the history. Returns what is in the way,
        /// or null when the draft was made.
        /// </summary>
        public string LoadPreset(string preset)
        {
            int room = Math.Min(Context.CompList.Width, Context.CompList.Height) - 3;
            (int least, int most) = preset switch { "Tower" => (7, 11), "Keep" => (17, 23), _ => (24, 30) };
            if (room < least)
            {
                return $"A {preset.ToLowerInvariant()} needs a multi of at least {least + 3} x {least + 3} cells.";
            }

            int n = Math.Min(room, most);
            const int o = 1;
            string Wing(int x0, int y0, int x1, int y1, int storeys, string roof, bool walled, int rooms = 1) =>
                $"<Wing x0=\"{x0}\" y0=\"{y0}\" x1=\"{x1}\" y1=\"{y1}\" storeys=\"{storeys}\" rooms=\"{rooms}\" roof=\"{roof}\" ridge=\"Auto\" walled=\"{(walled ? "true" : "false")}\"/>";
            string Corners(int tower, int storeys) =>
                Wing(o, o, o + tower, o + tower, storeys, "Flat", true) + Wing(o + n - tower, o, o + n, o + tower, storeys, "Flat", true) +
                Wing(o, o + n - tower, o + tower, o + n, storeys, "Flat", true) + Wing(o + n - tower, o + n - tower, o + n, o + n, storeys, "Flat", true);

            string wings;
            switch (preset)
            {
                case "Tower":
                    wings = Wing(o, o, o + n, o + n, 3, "Flat", false);
                    break;
                case "Keep":
                {
                    // A hall of two storeys with a taller tower on each corner, standing two cells out of it.
                    const int tower = 6, inset = 2;
                    wings = Wing(o + inset, o + inset, o + n - inset, o + n - inset, 2, "Flat", false, 2) + Corners(tower, 3);
                    break;
                }

                default:
                {
                    // A walled yard with a tower on each corner and a hall across its far side.
                    const int tower = 7, inset = 2;
                    wings = Wing(o + inset, o + inset, o + n - inset, o + n - inset, 1, "None", false) + Corners(tower, 3) +
                            Wing(o + tower, o + inset, o + n - tower, o + inset + 7, 2, "Flat", true, 2);
                    break;
                }
            }

            EnsureMaterials();
            XElement house = XElement.Parse(
                $"<House version=\"1\" name=\"{preset}\" baseZ=\"{BaseZ}\" foundation=\"false\" battlements=\"true\" roomDoors=\"true\" frontDoor=\"South\" windowSpacing=\"3\">{wings}</House>");
            OpenXml(house);
            return null;
        }

        /// <summary>Battlements around the flat roofs, on or off.</summary>
        public void SetParapet(bool on)
        {
            if (_draft.Parapet != on)
            {
                Edit(() => _draft.Parapet = on);
            }
        }

        /// <summary>Doors in the doorways between rooms, on or off.</summary>
        public void SetAutoDoors(bool on)
        {
            if (_draft.AutoDoors != on)
            {
                Edit(() => _draft.AutoDoors = on);
            }
        }

        public void SetFrontDoor(FrontDoorSide side)
        {
            if (_draft.FrontDoor != side)
            {
                Edit(() => _draft.FrontDoor = side);
            }
        }

        /// <summary>Windows along the outer walls, this many cells apart; 0 for none.</summary>
        public void SetWindowSpacing(int cells)
        {
            cells = Math.Clamp(cells, 0, 12);
            if (_draft.WindowSpacing != cells)
            {
                Edit(() => _draft.WindowSpacing = cells, "windows");
            }
        }
    }
}
