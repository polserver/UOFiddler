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
using UoFiddler.Plugin.MultiEditor.House;
using UoFiddler.Plugin.MultiEditor.Model;
using UoFiddler.Plugin.MultiEditor.Styles;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Roofs, shaped before they are placed (see <see cref="WingDraftTool"/> for the shaping).
    /// The first click on (or inside) a walled room fits a roof to its walls, part by part on an
    /// L or T shaped house; a drag over the walls, the floor inside them or the area the roof
    /// will cover fits it too, and any other drag is a roof of that size on the work plane.
    /// Further wings join the others with valleys and T pieces. The Shape and Ridge options
    /// change the selected wing; Shift+click turns a wing's ridge. Enter, or
    /// switching to another tool, places the roof on the layer named <see cref="LayerName"/>.
    /// A fitted roof sits on the wall tops whatever floor is chosen, overhangs the south and east
    /// walls by a cell and grows the multi when that cell lies outside it. Slopes rise 3 per cell
    /// like the official house roofs. A roof already there is replaced when it lies wholly under
    /// the new one and joined otherwise.
    /// </summary>
    internal sealed partial class RoofTool : WingDraftTool
    {
        /// <summary>The layer a placed roof goes on; made on first use.</summary>
        public const string LayerName = "Roof";

        private const int _rise = RoofPlanner.Rise;
        private const int _storey = 20;
        private const string _capStyles = "Miscellaneous Roof Pieces";

        private HouseStyle _lastStyle;
        private HouseStyle _planStyle;
        private RoofShape _shape = RoofShape.Gable;
        private RoofRidge _ridge = RoofRidge.Auto;
        private RoofGables _gables = RoofGables.Front;
        private RoofGableFill _gableFill = RoofGableFill.Caps;

        // Walls a house draft is about to build, which the multi does not hold yet.
        private Dictionary<(int X, int Y), List<(ushort Id, int Z)>> _draftWalls;

        public RoofTool(IEditorContext context) : base(context)
        {
        }

        /// <summary>The shape of new wings; also sets the selected wing's.</summary>
        public RoofShape Shape
        {
            get => _shape;
            set
            {
                if (SelectedWing is { } wing && wing.Shape != value)
                {
                    Remember();
                    wing.Shape = value;
                }

                _shape = value;
            }
        }

        /// <summary>The ridge direction of new wings; also sets the selected wing's.</summary>
        public RoofRidge Ridge
        {
            get => _ridge;
            set
            {
                if (SelectedWing is { } wing)
                {
                    Remember();
                    wing.RidgeAuto = value == RoofRidge.Auto;
                    wing.RidgeAlongY = value == RoofRidge.Auto ? RoofPlanner.RidgeAlongY(wing.Box) : value == RoofRidge.AlongY;
                }

                _ridge = value;
            }
        }

        public RoofGables Gables
        {
            get => _gables;
            set => _gables = value;
        }

        public RoofGableFill GableFill
        {
            get => _gableFill;
            set => _gableFill = value;
        }

        protected override bool Ready => Style != null;

        protected override string ActionName => "Roof";

        protected override string DefaultLayer => LayerName;

        /// <summary>The roof style chosen in the House tab; a pending roof keeps the last one.</summary>
        private HouseStyle Style
        {
            get
            {
                if (Context.HouseStyle is { Kind: HousePartKind.Roof } style)
                {
                    _lastStyle = style;
                    return style;
                }

                return HasPending ? _lastStyle : null;
            }
        }

        /// <summary>
        /// The eaves go on the work plane, or on top of what the mouse points at (a wall top).
        /// </summary>
        protected override void StartCell(out int x, out int y, out int z)
        {
            Context.CellAt(Context.MouseLoc, out x, out y, out z);
            if (Context.WorkZ is { } baseZ)
            {
                z = baseZ;
                Context.PlaneCell(Context.MouseLoc, z, out x, out y);
            }
        }

        protected override void WingResized(RoofWing wing)
        {
            if (wing.RidgeAuto)
            {
                wing.RidgeAlongY = RoofPlanner.RidgeAlongY(wing.Box);
            }
        }

        /// <summary>Turns a wing's ridge the other way.</summary>
        protected override void Turn(RoofWing wing)
        {
            wing.RidgeAlongY = !wing.RidgeAlongY;
            wing.RidgeAuto = false;
        }

        /// <summary>
        /// The wings a stroke makes. Walls sit on the +X/+Y edges of their cells, so the roof over
        /// them is their box one cell further along X and Y (as in the official houses, which
        /// overhang their east and south walls by a cell). A click takes the walls of the room it
        /// points at, each part of an L or T shaped house as a wing of its own; a drag that comes
        /// within a cell of such a roof on every side means that roof. Any other stroke is the
        /// roof itself, at <paramref name="z"/>. Shift turns a gable's ridge.
        /// </summary>
        protected override List<RoofWing> WingsFor(int startX, int startY, int endX, int endY, int z, out bool fitted)
        {
            int x0 = Math.Min(startX, endX), x1 = Math.Max(startX, endX);
            int y0 = Math.Min(startY, endY), y1 = Math.Max(startY, endY);
            bool click = x0 == x1 && y0 == y1;
            var wings = new List<RoofWing>();
            fitted = FitToWalls(x0, y0, x1, y1, z, out WallBox walls);
            if (fitted)
            {
                (x0, y0, x1, y1) = (walls.X0 + 1, walls.Y0 + 1, walls.X1 + 1, walls.Y1 + 1);
                z = walls.Top;
                if (click && Shape == RoofShape.Gable)
                {
                    var parts = RoofPlanner.Wings(RoofRegion(walls));
                    if (parts.Count > 1)
                    {
                        foreach (((int X0, int Y0, int X1, int Y1) box, bool alongY) in parts)
                        {
                            wings.Add(new RoofWing { X0 = box.X0, Y0 = box.Y0, X1 = box.X1, Y1 = box.Y1, EaveZ = z, RidgeAlongY = alongY });
                        }

                        return wings;
                    }
                }
            }

            bool auto = Ridge == RoofRidge.Auto;
            bool ridgeAlongY = auto ? RoofPlanner.RidgeAlongY((x0, y0, x1, y1)) : Ridge == RoofRidge.AlongY;
            if (ShiftHeld && Shape == RoofShape.Gable)
            {
                (ridgeAlongY, auto) = (!ridgeAlongY, false);
            }

            wings.Add(new RoofWing
            {
                X0 = x0, Y0 = y0, X1 = x1, Y1 = y1, EaveZ = z, Shape = Shape,
                RidgeAlongY = ridgeAlongY, RidgeAuto = auto
            });
            return wings;
        }

        protected override (List<MultiTile> Remove, List<(ushort Id, int X, int Y, int Z, string Layer)> Add) Plan(IReadOnlyList<RoofWing> wings)
        {
            return PlanRoof(wings, Style, null, null);
        }
    }
}
