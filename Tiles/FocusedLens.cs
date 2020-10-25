using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NeoLightBeams.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using TOD = Terraria.ObjectData.TileObjectData;

namespace NeoLightBeams.Tiles {

    public class FocusedLens : ModTile {

        #region Variables

        private Color _beamColor;
        private Color _beamColorModified;

        private int _beamLength;

        private const int _maxBeamLength = 396;
        private const float Alpha = 0.33f;

        #endregion Variables

        #region Style Structure

        private struct Style {

            public const int Up = 0;
            public const int Right = 18;
            public const int Left = 36;
            public const int Down = 54;
            public const int UpRight = 72;
            public const int UpLeft = 90;
            public const int DownRight = 108;
            public const int DownLeft = 126;

        }

        #endregion Style Structure

        #region Override Functions

        public override void SetDefaults() {

            Main.tileFrameImportant[Type] = true;

            TOD.newTile.CopyFrom(TOD.StyleTorch);
            TOD.newTile.CoordinateWidth = 16;
            TOD.newTile.CoordinateHeights = new[] { 16 };
            TOD.newTile.StyleHorizontal = false;
            TOD.newTile.StyleWrapLimit = 1;
            TOD.newTile.StyleMultiplier = 1;
            TOD.addTile(Type);

        }

        public override bool Drop(int x, int y) {

            Tile tile = Main.tile[x, y];

            int itemType = (tile.frameY % 36 == 0 ? mod.ItemType(Items.Placeable.FocusedLenses.FocusedLens) : mod.ItemType(Items.Placeable.FocusedLenses.FocusedBlackLens));

            Item.NewItem(x * 16, y * 16, 16, 16, itemType);

            return base.Drop(x, y);

        }

        public override void DrawEffects(int x, int y, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex) {

            if (!StyleIsValid(x, y))
                ChangeStyle(x, y);
            
            if (NeoLightBeams.HitWireList.Contains(new Vector2(x, y))) {

                NeoLightBeams.ResetLitUpGlassList = true;

                Tile tile = Main.tile[x, y];

                if (tile.frameY > 18) {
                    tile.frameY -= 36;
                } else {
                    tile.frameY += 36;
                }

                NeoLightBeams.HitWireList.Remove(new Vector2(x, y));

            }

        }

        public override void HitWire(int x, int y) => NeoLightBeams.HitWireList.Add(new Vector2(x, y));

        public override void KillTile(int x, int y, ref bool fail, ref bool effectOnly, ref bool noItem) {

            if (fail || effectOnly)
                return;

            if (NeoLightBeams.HitWireList.Contains(new Vector2(x, y)))
                NeoLightBeams.HitWireList.Remove(new Vector2(x, y));

            NeoLightBeams.ResetLitUpGlassList = true;

        }

        public override void PlaceInWorld(int x, int y, Item item) {

            // Determine which item is being placed and adjust frameY accordingly.
            int style = Main.LocalPlayer.HeldItem.placeStyle;
            Tile tile = Main.tile[x, y];
            tile.frameY = (short)(style * 18);

            int placementStyle = 0;

            // Determine which style lens to place by looking at the surrounding tiles.
            if (Main.tile[x, y + 1].active() && Main.tileSolid[Main.tile[x, y + 1].type]) {

                placementStyle = Style.Up;

            }
            else if (Main.tile[x - 1, y].active() && Main.tileSolid[Main.tile[x - 1, y].type]) {

                placementStyle = Style.Right;

            }
            else if (Main.tile[x + 1, y].active() && Main.tileSolid[Main.tile[x + 1, y].type]) {

                placementStyle = Style.Left;

            }
            else if (Main.tile[x, y - 1].active() && Main.tileSolid[Main.tile[x, y - 1].type]) {

                placementStyle = Style.Down;

            }

            // Adjust frameX based on style selected above.
            tile.frameX = (short)(placementStyle);

        }

        public override void PostDraw(int x, int y, SpriteBatch sb) {

            Tile tile = Main.tile[x, y];

            if (!ShouldFire(x, y) || Main.netMode == NetmodeID.Server || Main.dedServ) {

                if (tile.frameY == 0 || tile.frameY == 18)
                    tile.frameY += 36;

                return;
            }

            DrawLensOverlay(sb, x, y);

            _beamLength = 0;
            Vector2 screenPos = new Vector2(x * 16, y * 16) - Main.screenPosition;

            switch (tile.frameX) {

                case Style.Up: {

                        BeamUp(sb, screenPos, 0);

                        break;

                    }

                case Style.Right: {

                        BeamRight(sb, screenPos, 0);

                        break;

                    }

                case Style.Left: {

                        BeamLeft(sb, screenPos, 0);

                        break;

                    }

                case Style.Down: {

                        BeamDown(sb, screenPos, 0);

                        break;

                    }

                case Style.UpRight: {

                        BeamUpRight(sb, screenPos, 0);

                        break;

                    }

                case Style.UpLeft: {

                        BeamUpLeft(sb, screenPos, 0);

                        break;

                    }

                case Style.DownRight: {

                        BeamDownRight(sb, screenPos, 0);

                        break;

                    }

                case Style.DownLeft: {

                        BeamDownLeft(sb, screenPos, 0);

                        break;

                    }

            }

        }

        public override bool NewRightClick(int x, int y) {

            ChangeStyle(x, y);

            return false;

        }

        /*public override bool TileFrame(int x, int y, ref bool resetFrame, ref bool noBreak) {

            if (!StyleIsValid(x, y))
                ChangeStyle(x, y);
            
            return base.TileFrame(x, y, ref resetFrame, ref noBreak);

        }*/

        #endregion Override Functions

        #region Private Functions

        #region Beam Functions

        private void BeamUp(SpriteBatch sb, Vector2 screenPos, int count) {

            Point curPos = (screenPos + Main.screenPosition).ToTileCoordinates();
            
            Tile tileAbove = Main.tile[curPos.X, curPos.Y - count];

            int offset = 12 * 16 + 8;

            while ((curPos.Y - count) * 16 > Main.screenPosition.Y - (NeoLightBeams.OffscreenBlockRange * 16) && LaserPassesThrough(tileAbove, curPos.X, curPos.Y - count)) {

                if (TooManyBeams(count))
                    return;

                if (tileAbove.type == TileType<FocusedLens>()) {

                    if (tileAbove.frameX == Style.DownRight) {

                        sb.Draw(NeoLightBeams.BeamBounce90, new Vector2(screenPos.X + offset, screenPos.Y - count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipVertically, 0f);
                        BeamRight(sb, new Vector2(((curPos.X + 1) * 16) - Main.screenPosition.X, ((curPos.Y - count) * 16) - Main.screenPosition.Y), 0);

                    }
                    else if (tileAbove.frameX == Style.DownLeft) {

                        sb.Draw(NeoLightBeams.BeamBounce90, new Vector2(screenPos.X + offset, screenPos.Y - count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);
                        BeamLeft(sb, new Vector2(((curPos.X - 1) * 16) - Main.screenPosition.X, ((curPos.Y - count) * 16) - Main.screenPosition.Y), 0);

                    }
                    else if (tileAbove.frameX == Style.Left) {

                        sb.Draw(NeoLightBeams.BeamBounce45, new Vector2(screenPos.X + offset, screenPos.Y - count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally, 0f);
                        BeamUpLeft(sb, new Vector2(((curPos.X - 1) * 16) - Main.screenPosition.X, ((curPos.Y - count - 1) * 16) - Main.screenPosition.Y), 0, true);

                    }
                    else if (tileAbove.frameX == Style.Right) {

                        sb.Draw(NeoLightBeams.BeamBounce45, new Vector2(screenPos.X + offset, screenPos.Y - count * 16 + offset), _beamColorModified * Alpha);
                        BeamUpRight(sb, new Vector2(((curPos.X + 1) * 16) - Main.screenPosition.X, ((curPos.Y - count - 1) * 16) - Main.screenPosition.Y), 0, true);

                    }
                    else if (tileAbove.frameX == Style.UpLeft || tileAbove.frameX == Style.UpRight) {
                        break;
                    }

                    if (tileAbove.frameX != Style.Up && tileAbove.frameX != Style.Down)
                        return;

                }
                else if (tileAbove.type == TileID.Glass) {
                    LightUpGlass(sb, new Point(curPos.X, curPos.Y - count));
                    if (Main.tile[curPos.X, curPos.Y - count - 1].type == TileType<Tiles.FocusedLens>())
                        break;
                }

                Texture2D beamTexture;
                switch (Main.rand.Next(5)) {

                    case 0: beamTexture = NeoLightBeams.Beam0; break;
                    case 1: beamTexture = NeoLightBeams.Beam1; break;
                    case 2: beamTexture = NeoLightBeams.Beam2; break;
                    case 3: beamTexture = NeoLightBeams.Beam3; break;
                    case 4: beamTexture = NeoLightBeams.Beam4; break;
                    default: beamTexture = NeoLightBeams.Beam; break;

                }

                sb.Draw(beamTexture, new Vector2(screenPos.X + offset, screenPos.Y - count * 16 + offset), null, _beamColorModified * Alpha, MathHelper.ToRadians(90), new Vector2(8, 8), 1f, SpriteEffects.None, 0f);

                Lighting.AddLight(new Vector2(curPos.X, curPos.Y - count), _beamColorModified.R, _beamColorModified.G, _beamColorModified.B);

                IncrementCounts(ref count);

                tileAbove = Main.tile[curPos.X, curPos.Y - count];

            }

        }

        private void BeamRight(SpriteBatch sb, Vector2 screenPos, int count) {

            Point curPos = (screenPos + Main.screenPosition).ToTileCoordinates();
            Tile tileRight = Main.tile[curPos.X + count, curPos.Y];

            int offset = 12 * 16;

            while ((curPos.X + count) * 16 < Main.screenPosition.X + Main.screenWidth + (NeoLightBeams.OffscreenBlockRange * 16) && LaserPassesThrough(tileRight, curPos.X + count, curPos.Y)) {

                if (TooManyBeams(count))
                    return;

                if (tileRight.type == TileType<Tiles.FocusedLens>()) {

                    if (tileRight.frameX == Style.UpLeft) {

                        sb.Draw(NeoLightBeams.BeamBounce90, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally, 0f);
                        BeamUp(sb, new Vector2(((curPos.X + count) * 16) - Main.screenPosition.X, ((curPos.Y - 1) * 16) - Main.screenPosition.Y), 0);

                    }
                    else if (tileRight.frameX == Style.DownLeft) {

                        sb.Draw(NeoLightBeams.BeamBounce90, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);
                        BeamDown(sb, new Vector2(((curPos.X + count) * 16) - Main.screenPosition.X, ((curPos.Y + 1) * 16) - Main.screenPosition.Y), 0);

                    }
                    else if (tileRight.frameX == Style.Up) {

                        sb.Draw(NeoLightBeams.BeamBounce45, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y + offset), null, _beamColorModified * Alpha, (float)Math.PI / 2, new Vector2(0, 16), 1f, SpriteEffects.FlipHorizontally, 0f);
                        BeamUpRight(sb, new Vector2(((curPos.X + count + 1) * 16) - Main.screenPosition.X, ((curPos.Y - 1) * 16) - Main.screenPosition.Y), 0, true);

                    }
                    else if (tileRight.frameX == Style.Down) {

                        sb.Draw(NeoLightBeams.BeamBounce45, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y + offset), null, _beamColorModified * Alpha, (float)Math.PI / 2, new Vector2(0, 16), 1f, SpriteEffects.None, 0f);
                        BeamDownRight(sb, new Vector2(((curPos.X + count + 1) * 16) - Main.screenPosition.X, ((curPos.Y + 1) * 16) - Main.screenPosition.Y), 0, true);

                    }
                    else if (tileRight.frameX == Style.UpRight || tileRight.frameX == Style.DownRight) {
                        break;
                    }

                    if (tileRight.frameX != Style.Left && tileRight.frameX != Style.Right)
                        return;

                }
                else if (tileRight.type == TileID.Glass) {
                    LightUpGlass(sb, new Point(curPos.X + count, curPos.Y));
                    if (Main.tile[curPos.X + count + 1, curPos.Y].type == TileType<FocusedLens>())
                        break;
                }
                
                Texture2D beamTexture;
                switch (Main.rand.Next(5)) {

                    case 0: beamTexture = NeoLightBeams.Beam0; break;
                    case 1: beamTexture = NeoLightBeams.Beam1; break;
                    case 2: beamTexture = NeoLightBeams.Beam2; break;
                    case 3: beamTexture = NeoLightBeams.Beam3; break;
                    case 4: beamTexture = NeoLightBeams.Beam4; break;
                    default: beamTexture = NeoLightBeams.Beam; break;

                }

                sb.Draw(beamTexture, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y + offset), _beamColorModified * Alpha);

                //Lighting.AddLight(new Vector2((curPos.X + count) * 16, curPos.Y * 16), _beamColorModified.R * .005f * (1 / Math.Max(1, _beamLength - 25)), _beamColorModified.G * .005f * (1 / Math.Max(1, _beamLength - 25)), _beamColorModified.B * .005f * (1 / Math.Max(1, _beamLength - 25)));
                Lighting.AddLight(
                    new Vector2((curPos.X + count) * 16, curPos.Y * 16),
                    _beamColorModified.R * .003f * ((_maxBeamLength * 2 - _beamLength) / _maxBeamLength),
                    _beamColorModified.G * .003f * ((_maxBeamLength * 2 - _beamLength) / _maxBeamLength),
                    _beamColorModified.B * .003f * ((_maxBeamLength * 2 - _beamLength) / _maxBeamLength)
                );

                IncrementCounts(ref count);

                tileRight = Main.tile[curPos.X + count, curPos.Y];

            }

        }

        private void BeamLeft(SpriteBatch sb, Vector2 screenPos, int count) {

            Point curPos = (screenPos + Main.screenPosition).ToTileCoordinates();
            Tile tileLeft = Main.tile[curPos.X - count, curPos.Y];

            int offset = 12 * 16;

            while ((curPos.X - count) * 16 > Main.screenPosition.X - (NeoLightBeams.OffscreenBlockRange * 16) && LaserPassesThrough(tileLeft, curPos.X - count, curPos.Y)) {

                if (TooManyBeams(count))
                    return;

                if (tileLeft.type == TileType<FocusedLens>()) {

                    if (tileLeft.frameX == Style.UpRight) {

                        sb.Draw(NeoLightBeams.BeamBounce90, new Vector2(screenPos.X - count * 16 + offset, screenPos.Y + offset), _beamColorModified * Alpha);
                        BeamUp(sb, new Vector2(((curPos.X - count) * 16) - Main.screenPosition.X, ((curPos.Y - 1) * 16) - Main.screenPosition.Y), 0);

                    }
                    else if (tileLeft.frameX == Style.DownRight) {

                        sb.Draw(NeoLightBeams.BeamBounce90, new Vector2(screenPos.X - count * 16 + offset, screenPos.Y + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipVertically, 0f);
                        BeamDown(sb, new Vector2(((curPos.X - count) * 16) - Main.screenPosition.X, ((curPos.Y + 1) * 16) - Main.screenPosition.Y), 0);

                    }
                    else if (tileLeft.frameX == Style.Up) {

                        sb.Draw(NeoLightBeams.BeamBounce45, new Vector2(screenPos.X - count * 16 + offset, screenPos.Y + offset), null, _beamColorModified * Alpha, (float)Math.PI / 2, new Vector2(0, 16), 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);
                        BeamUpLeft(sb, new Vector2(((curPos.X - count - 1) * 16) - Main.screenPosition.X, ((curPos.Y - 1) * 16) - Main.screenPosition.Y), 0, true);

                    }
                    else if (tileLeft.frameX == Style.Down) {

                        sb.Draw(NeoLightBeams.BeamBounce45, new Vector2(screenPos.X - count * 16 + offset, screenPos.Y + offset), null, _beamColorModified * Alpha, (float)Math.PI / 2, new Vector2(0, 16), 1f, SpriteEffects.FlipVertically, 0f);
                        BeamDownLeft(sb, new Vector2(((curPos.X - count - 1) * 16) - Main.screenPosition.X, ((curPos.Y + 1) * 16) - Main.screenPosition.Y), 0, true);

                    }
                    else if (tileLeft.frameX == Style.UpLeft || tileLeft.frameX == Style.DownLeft) {
                        break;
                    }

                    if (tileLeft.frameX != Style.Left && tileLeft.frameX != Style.Right)
                        return;

                }
                else if (tileLeft.type == TileID.Glass) {
                    LightUpGlass(sb, new Point(curPos.X - count, curPos.Y));
                    if (Main.tile[curPos.X - count - 1, curPos.Y].type == TileType<FocusedLens>())
                        break;
                }

                Texture2D beamTexture;
                switch (Main.rand.Next(5)) {

                    case 0: beamTexture = NeoLightBeams.Beam0; break;
                    case 1: beamTexture = NeoLightBeams.Beam1; break;
                    case 2: beamTexture = NeoLightBeams.Beam2; break;
                    case 3: beamTexture = NeoLightBeams.Beam3; break;
                    case 4: beamTexture = NeoLightBeams.Beam4; break;
                    default: beamTexture = NeoLightBeams.Beam; break;

                }

                sb.Draw(beamTexture, new Vector2(screenPos.X - count * 16 + offset, screenPos.Y + offset), _beamColorModified * Alpha);

                Lighting.AddLight(new Vector2(curPos.X - count, curPos.Y), _beamColorModified.R, _beamColorModified.G, _beamColorModified.B);

                IncrementCounts(ref count);

                tileLeft = Main.tile[curPos.X - count, curPos.Y];

            }

        }

        private void BeamDown(SpriteBatch sb, Vector2 screenPos, int count) {

            Point curPos = (screenPos + Main.screenPosition).ToTileCoordinates();
            Tile tileBelow = Main.tile[curPos.X, curPos.Y + count];

            int offset = 12 * 16;

            while ((curPos.Y + count) * 16 < Main.screenPosition.Y + Main.screenHeight + (NeoLightBeams.OffscreenBlockRange * 16) && LaserPassesThrough(tileBelow, curPos.X, curPos.Y + count)) {

                if (TooManyBeams(count))
                    return;

                if (tileBelow.type == TileType<FocusedLens>()) {

                    if (tileBelow.frameX == Style.UpRight) {

                        sb.Draw(NeoLightBeams.BeamBounce90, new Vector2(screenPos.X + offset, screenPos.Y + count * 16 + offset), _beamColorModified * Alpha);
                        BeamRight(sb, new Vector2(((curPos.X + 1) * 16) - Main.screenPosition.X, ((curPos.Y + count) * 16) - Main.screenPosition.Y), 0);

                    }
                    else if (tileBelow.frameX == Style.UpLeft) {

                        sb.Draw(NeoLightBeams.BeamBounce90, new Vector2(screenPos.X + offset, screenPos.Y + count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally, 0f);
                        BeamLeft(sb, new Vector2(((curPos.X - 1) * 16) - Main.screenPosition.X, ((curPos.Y + count) * 16) - Main.screenPosition.Y), 0);

                    }
                    else if (tileBelow.frameX == Style.Left) {

                        sb.Draw(NeoLightBeams.BeamBounce45, new Vector2(screenPos.X + offset, screenPos.Y + count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);
                        BeamDownLeft(sb, new Vector2(((curPos.X - 1) * 16) - Main.screenPosition.X, ((curPos.Y + count + 1) * 16) - Main.screenPosition.Y), 0, true);

                    }
                    else if (tileBelow.frameX == Style.Right) {

                        sb.Draw(NeoLightBeams.BeamBounce45, new Vector2(screenPos.X + offset, screenPos.Y + count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipVertically, 0f);
                        BeamDownRight(sb, new Vector2(((curPos.X + 1) * 16) - Main.screenPosition.X, ((curPos.Y + count + 1) * 16) - Main.screenPosition.Y), 0, true);

                    }
                    else if (tileBelow.frameX == Style.DownRight || tileBelow.frameX == Style.DownLeft) {
                        break;
                    }

                    if (tileBelow.frameX != Style.Up && tileBelow.frameX != Style.Down)
                        return;

                }
                else if (tileBelow.type == TileID.Glass) {
                    LightUpGlass(sb, new Point(curPos.X, curPos.Y + count));
                    if (Main.tile[curPos.X, curPos.Y + count + 1].type == TileType<FocusedLens>())
                        break;
                }

                Texture2D beamTexture;
                switch (Main.rand.Next(5)) {

                    case 0: beamTexture = NeoLightBeams.Beam0; break;
                    case 1: beamTexture = NeoLightBeams.Beam1; break;
                    case 2: beamTexture = NeoLightBeams.Beam2; break;
                    case 3: beamTexture = NeoLightBeams.Beam3; break;
                    case 4: beamTexture = NeoLightBeams.Beam4; break;
                    default: beamTexture = NeoLightBeams.Beam; break;

                }

                sb.Draw(beamTexture, new Vector2(screenPos.X + 8 + offset, screenPos.Y + count * 16 + 8 + offset), null, _beamColorModified * Alpha, MathHelper.ToRadians(90), new Vector2(8, 8), 1f, SpriteEffects.None, 0f);

                Lighting.AddLight(new Vector2(curPos.X, curPos.Y + count), _beamColorModified.R, _beamColorModified.G, _beamColorModified.B);

                IncrementCounts(ref count);

                tileBelow = Main.tile[curPos.X, curPos.Y + count];

            }

        }

        private void BeamUpRight(SpriteBatch sb, Vector2 screenPos, int count, bool bouncing = false) {

            if (TooManyBeams(count))
                return;

            Point curPos = (screenPos + Main.screenPosition).ToTileCoordinates();

            int offset = 12 * 16;

            if (!bouncing) {

                sb.Draw(NeoLightBeams.BeamDiagonalStart, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y - count * 16 + offset), _beamColorModified * Alpha);

                IncrementCounts(ref count);

            }

            Tile tileAbove = Main.tile[curPos.X + count - 1, curPos.Y - count];
            Tile tileAboveRight = Main.tile[curPos.X + count, curPos.Y - count];

            while ((curPos.X + count) * 16 < Main.screenPosition.X + Main.screenWidth + (NeoLightBeams.OffscreenBlockRange * 16) &&
                   (curPos.Y - count) * 16 > Main.screenPosition.Y - (NeoLightBeams.OffscreenBlockRange * 16) &&
                   LaserPassesThrough(tileAboveRight, curPos.X + count, curPos.Y - count)
                  ) {

                if (TooManyBeams(count))
                    return;

                if ((tileAbove.type == TileType<FocusedLens>()) && tileAbove.frameX == Style.DownRight) {

                    sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X + (count - 1) * 16 + offset, screenPos.Y - count * 16 + offset), _beamColorModified * Alpha);
                    sb.Draw(NeoLightBeams.BeamBounce45, new Vector2(screenPos.X + count * 16 + 8 + offset, screenPos.Y - count * 16 + 8 + offset), null, _beamColorModified * Alpha, MathHelper.ToRadians(270), new Vector2(8, 8), 1f, SpriteEffects.FlipHorizontally, 0f);
                    sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y - (count - 1) * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);

                    BeamRight(sb, new Vector2((curPos.X + count + 1) * 16 - Main.screenPosition.X, (curPos.Y - count) * 16 - Main.screenPosition.Y), 0);

                    return;

                }
                else if (tileAboveRight.type == TileType<FocusedLens>()) {

                    if (tileAboveRight.frameX == Style.Left) {

                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X + (count - 1) * 16 + offset, screenPos.Y - count * 16 + offset), _beamColorModified * Alpha);
                        sb.Draw(NeoLightBeams.BeamBounce90Rotated, new Vector2(screenPos.X + count * 16 + 8 + offset, screenPos.Y - count * 16 + 8 + offset), null, _beamColorModified * Alpha, MathHelper.ToRadians(270), new Vector2(8, 8), 1f, SpriteEffects.None, 0f);
                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y - (count - 1) * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);

                        BeamUpLeft(sb, new Vector2(((curPos.X + count - 1) * 16) - Main.screenPosition.X, ((curPos.Y - count - 1) * 16) - Main.screenPosition.Y), 0, true);

                        return;

                    }
                    else if (tileAboveRight.frameX == Style.Down) {

                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X + (count - 1) * 16 + offset, screenPos.Y - count * 16 + offset), _beamColorModified * Alpha);
                        sb.Draw(NeoLightBeams.BeamBounce90Rotated, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y - count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipVertically, 0f);
                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y - (count - 1) * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);

                        BeamDownRight(sb, new Vector2(((curPos.X + count + 1) * 16) - Main.screenPosition.X, ((curPos.Y - count + 1) * 16) - Main.screenPosition.Y), 0, true);

                        return;

                    }
                    else if (tileAboveRight.frameX == Style.DownLeft) {

                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X + (count - 1) * 16 + offset, screenPos.Y - count * 16 + offset), _beamColorModified * Alpha);
                        sb.Draw(NeoLightBeams.BeamDiagonalStart, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y - count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);
                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y - (count - 1) * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);

                        return;

                    }
                    else if (tileAboveRight.frameX == Style.UpRight || tileAboveRight.frameX == Style.UpLeft || tileAboveRight.frameX == Style.DownRight) {

                        break;

                    }

                    //                    if (tileAboveRight.frameX != Style.Right && tileAboveRight.frameX != Style.Up)
                    //                        return;

                }
                else if (tileAboveRight.type == TileID.Glass) {
                    LightUpGlass(sb, new Point(curPos.X + count, curPos.Y - count));
                }

                sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X + (count - 1) * 16 + offset, screenPos.Y - count * 16 + offset), _beamColorModified * Alpha);

                if (tileAboveRight.type != TileID.Glass) {

                    Texture2D beamTexture;
                    switch (Main.rand.Next(5)) {

                        case 0: beamTexture = NeoLightBeams.BeamDiagonal0; break;
                        case 1: beamTexture = NeoLightBeams.BeamDiagonal1; break;
                        case 2: beamTexture = NeoLightBeams.BeamDiagonal2; break;
                        case 3: beamTexture = NeoLightBeams.BeamDiagonal3; break;
                        case 4: beamTexture = NeoLightBeams.BeamDiagonal4; break;
                        default: beamTexture = NeoLightBeams.BeamDiagonal; break;

                    }

                    sb.Draw(beamTexture, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y - count * 16 + offset), _beamColorModified * Alpha);

                    Lighting.AddLight(new Vector2(curPos.X + count, curPos.Y - count), _beamColorModified.R, _beamColorModified.G, _beamColorModified.B);

                }

                sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y - (count - 1) * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);

                IncrementCounts(ref count);

                tileAbove = Main.tile[curPos.X + count - 1, curPos.Y - count];
                tileAboveRight = Main.tile[curPos.X + count, curPos.Y - count];

            }

            tileAboveRight = Main.tile[curPos.X + count - 1, curPos.Y - count];

            if ((curPos.X + count - 1) * 16 < Main.screenPosition.X + Main.screenWidth + (NeoLightBeams.OffscreenBlockRange * 16) &&
                (curPos.Y - count) * 16 > Main.screenPosition.Y - (NeoLightBeams.OffscreenBlockRange * 16) &&
                LaserPassesThrough(tileAboveRight)
               ) {

                sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X + (count - 1) * 16 + offset, screenPos.Y - count * 16 + offset), _beamColorModified * Alpha);

            }

            tileAboveRight = Main.tile[curPos.X + count, curPos.Y - count + 1];

            if ((curPos.X + count) * 16 < Main.screenPosition.X + Main.screenWidth + (NeoLightBeams.OffscreenBlockRange * 16) &&
                (curPos.Y - count + 1) * 16 > Main.screenPosition.Y - (NeoLightBeams.OffscreenBlockRange * 16) &&
                LaserPassesThrough(tileAboveRight)
               ) {

                sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y - (count - 1) * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);

            }

        }

        private void BeamUpLeft(SpriteBatch sb, Vector2 screenPos, int count, bool bouncing = false) {

            if (TooManyBeams(count))
                return;

            Point curPos = (screenPos + Main.screenPosition).ToTileCoordinates();

            int offset = 12 * 16;

            if (!bouncing) {

                sb.Draw(NeoLightBeams.BeamDiagonalStart, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y - count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally, 0f);

                IncrementCounts(ref count);

            }

            Tile tileAboveLeft = Main.tile[curPos.X - count, curPos.Y - count];

            while ((curPos.X - count) * 16 > Main.screenPosition.X - (NeoLightBeams.OffscreenBlockRange * 16) &&
                   (curPos.Y - count) * 16 > Main.screenPosition.Y - (NeoLightBeams.OffscreenBlockRange * 16) &&
                   LaserPassesThrough(tileAboveLeft, curPos.X - count, curPos.Y - count)
                  ) {

                if (TooManyBeams(count))
                    return;

                if (tileAboveLeft.type == TileType<FocusedLens>()) {

                    if (tileAboveLeft.frameX == Style.Right) {

                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X - count * 16 + offset, screenPos.Y - (count - 1) * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipVertically, 0f);
                        sb.Draw(NeoLightBeams.BeamBounce90Rotated, new Vector2(screenPos.X - count * 16 + 8 + offset, screenPos.Y - count * 16 + 8 + offset), null, _beamColorModified * Alpha, MathHelper.ToRadians(90), new Vector2(8, 8), 1f, SpriteEffects.None, 0f);
                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X - (count - 1) * 16 + offset, screenPos.Y - count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally, 0f);

                        BeamUpRight(sb, new Vector2(((curPos.X - count + 1) * 16) - Main.screenPosition.X, ((curPos.Y - count - 1) * 16) - Main.screenPosition.Y), 0, true);

                    }
                    else if (tileAboveLeft.frameX == Style.Down) {

                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X - count * 16 + offset, screenPos.Y - (count - 1) * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipVertically, 0f);
                        sb.Draw(NeoLightBeams.BeamBounce90Rotated, new Vector2(screenPos.X - count * 16 + offset, screenPos.Y - count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipVertically, 0f);
                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X - (count - 1) * 16 + offset, screenPos.Y - count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally, 0f);

                        BeamDownLeft(sb, new Vector2(((curPos.X - count - 1) * 16) - Main.screenPosition.X, ((curPos.Y - count + 1) * 16) - Main.screenPosition.Y), 0, true);

                    }
                    else if (tileAboveLeft.frameX == Style.DownRight) {

                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X - count * 16 + offset, screenPos.Y - (count - 1) * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipVertically, 0f);
                        sb.Draw(NeoLightBeams.BeamDiagonalStart, new Vector2(screenPos.X - count * 16 + offset, screenPos.Y - count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipVertically, 0f);
                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X - (count - 1) * 16 + offset, screenPos.Y - count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally, 0f);

                    }
                    else if (tileAboveLeft.frameX == Style.UpRight || tileAboveLeft.frameX == Style.UpLeft || tileAboveLeft.frameX == Style.DownLeft) {

                        break;

                    }

                    if (tileAboveLeft.frameX != Style.Left && tileAboveLeft.frameX != Style.Up)
                        return;

                }
                else if (tileAboveLeft.type == TileID.Glass) {
                    LightUpGlass(sb, new Point(curPos.X - count, curPos.Y - count));
                }

                sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X - count * 16 + offset, screenPos.Y - (count - 1) * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipVertically, 0f);

                if (tileAboveLeft.type != TileID.Glass) {

                    Texture2D beamTexture;
                    switch (Main.rand.Next(5)) {

                        case 0: beamTexture = NeoLightBeams.BeamDiagonal0; break;
                        case 1: beamTexture = NeoLightBeams.BeamDiagonal1; break;
                        case 2: beamTexture = NeoLightBeams.BeamDiagonal2; break;
                        case 3: beamTexture = NeoLightBeams.BeamDiagonal3; break;
                        case 4: beamTexture = NeoLightBeams.BeamDiagonal4; break;
                        default: beamTexture = NeoLightBeams.BeamDiagonal; break;

                    }

                    sb.Draw(beamTexture, new Vector2(screenPos.X - count * 16 + offset, screenPos.Y - count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally, 0f);

                    Lighting.AddLight(new Vector2(curPos.X - count, curPos.Y - count), _beamColorModified.R, _beamColorModified.G, _beamColorModified.B);

                }

                sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X - (count - 1) * 16 + offset, screenPos.Y - count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally, 0f);

                IncrementCounts(ref count);

                tileAboveLeft = Main.tile[curPos.X - count, curPos.Y - count];

            }

            tileAboveLeft = Main.tile[curPos.X - count, curPos.Y - count + 1];

            if ((curPos.X - count) * 16 > Main.screenPosition.X - (NeoLightBeams.OffscreenBlockRange * 16) &&
                (curPos.Y - count + 1) * 16 > Main.screenPosition.Y - (NeoLightBeams.OffscreenBlockRange * 16) &&
                LaserPassesThrough(tileAboveLeft)
               ) {

                sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X - count * 16 + offset, screenPos.Y - (count - 1) * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipVertically, 0f);

            }

            tileAboveLeft = Main.tile[curPos.X - count + 1, curPos.Y - count];

            if ((curPos.X - count + 1) * 16 > Main.screenPosition.X - (NeoLightBeams.OffscreenBlockRange * 16) &&
                (curPos.Y - count) * 16 > Main.screenPosition.Y - (NeoLightBeams.OffscreenBlockRange * 16) &&
                LaserPassesThrough(tileAboveLeft)
               ) {

                sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X - (count - 1) * 16 + offset, screenPos.Y - count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally, 0f);

            }

        }

        private void BeamDownRight(SpriteBatch sb, Vector2 screenPos, int count, bool bouncing = false) {

            if (TooManyBeams(count))
                return;

            Point curPos = (screenPos + Main.screenPosition).ToTileCoordinates();

            int offset = 12 * 16;

            if (!bouncing) {

                sb.Draw(NeoLightBeams.BeamDiagonalStart, new Vector2(screenPos.X + offset, screenPos.Y + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipVertically, 0f);

                IncrementCounts(ref count);

            }

            Tile tileBelowRight = Main.tile[curPos.X + count, curPos.Y + count];

            while ((curPos.X + count) * 16 < Main.screenPosition.X + Main.screenWidth + (NeoLightBeams.OffscreenBlockRange * 16) &&
                   (curPos.Y + count) * 16 < Main.screenPosition.Y + Main.screenHeight + (NeoLightBeams.OffscreenBlockRange * 16) &&
                   LaserPassesThrough(tileBelowRight, curPos.X + count, curPos.Y + count)
                  ) {

                if (TooManyBeams(count))
                    return;

                if (tileBelowRight.type == TileType<FocusedLens>()) {

                    if (tileBelowRight.frameX == Style.Left) {

                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y + (count - 1) * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally, 0f);
                        sb.Draw(NeoLightBeams.BeamBounce90Rotated, new Vector2(screenPos.X + count * 16 + 8 + offset, screenPos.Y + count * 16 + 8 + offset), null, _beamColorModified * Alpha, MathHelper.ToRadians(270), new Vector2(8, 8), 1f, SpriteEffects.None, 0f);
                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X + (count - 1) * 16 + offset, screenPos.Y + count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipVertically, 0f);

                        BeamDownLeft(sb, new Vector2(((curPos.X + count - 1) * 16) - Main.screenPosition.X, ((curPos.Y + count + 1) * 16) - Main.screenPosition.Y), 0, true);

                    }
                    else if (tileBelowRight.frameX == Style.Up) {

                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y + (count - 1) * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally, 0f);
                        sb.Draw(NeoLightBeams.BeamBounce90Rotated, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y + count * 16 + offset), _beamColorModified * Alpha);
                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X + (count - 1) * 16 + offset, screenPos.Y + count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipVertically, 0f);

                        BeamUpRight(sb, new Vector2(((curPos.X + count + 1) * 16) - Main.screenPosition.X, ((curPos.Y + count - 1) * 16) - Main.screenPosition.Y), 0, true);

                    }
                    else if (tileBelowRight.frameX == Style.UpLeft) {

                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y + (count - 1) * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally, 0f);
                        sb.Draw(NeoLightBeams.BeamDiagonalStart, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y + count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally, 0f);
                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X + (count - 1) * 16 + offset, screenPos.Y + count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipVertically, 0f);

                    }
                    else if (tileBelowRight.frameX == Style.DownRight || tileBelowRight.frameX == Style.DownLeft || tileBelowRight.frameX == Style.UpRight) {

                        break;

                    }

                    if (tileBelowRight.frameX != Style.Right && tileBelowRight.frameX != Style.Down)
                        return;

                }
                else if (tileBelowRight.type == TileID.Glass) {
                    LightUpGlass(sb, new Point(curPos.X + count, curPos.Y + count));
                }

                sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y + (count - 1) * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally, 0f);

                if (tileBelowRight.type != TileID.Glass) {

                    Texture2D beamTexture;
                    switch (Main.rand.Next(5)) {

                        case 0: beamTexture = NeoLightBeams.BeamDiagonal0; break;
                        case 1: beamTexture = NeoLightBeams.BeamDiagonal1; break;
                        case 2: beamTexture = NeoLightBeams.BeamDiagonal2; break;
                        case 3: beamTexture = NeoLightBeams.BeamDiagonal3; break;
                        case 4: beamTexture = NeoLightBeams.BeamDiagonal4; break;
                        default: beamTexture = NeoLightBeams.BeamDiagonal; break;

                    }

                    sb.Draw(beamTexture, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y + count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally, 0f);

                    Lighting.AddLight(new Vector2(curPos.X + count, curPos.Y + count), _beamColorModified.R, _beamColorModified.G, _beamColorModified.B);

                }

                sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X + (count - 1) * 16 + offset, screenPos.Y + count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipVertically, 0f);

                IncrementCounts(ref count);

                tileBelowRight = Main.tile[curPos.X + count, curPos.Y + count];

            }

            tileBelowRight = Main.tile[curPos.X + count, curPos.Y + count - 1];

            if ((curPos.X + count) * 16 < Main.screenPosition.X + Main.screenWidth + (NeoLightBeams.OffscreenBlockRange * 16) &&
                (curPos.Y + count - 1) * 16 < Main.screenPosition.Y + Main.screenHeight + (NeoLightBeams.OffscreenBlockRange * 16) &&
                LaserPassesThrough(tileBelowRight)
               ) {

                sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X + count * 16 + offset, screenPos.Y + (count - 1) * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally, 0f);

            }

            tileBelowRight = Main.tile[curPos.X + count - 1, curPos.Y + count];

            if ((curPos.X + count - 1) * 16 < Main.screenPosition.X + Main.screenWidth + (NeoLightBeams.OffscreenBlockRange * 16) &&
                (curPos.Y + count) * 16 < Main.screenPosition.Y + Main.screenHeight + (NeoLightBeams.OffscreenBlockRange * 16) &&
                LaserPassesThrough(tileBelowRight)
               ) {

                sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X + (count - 1) * 16 + offset, screenPos.Y + count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipVertically, 0f);

            }

        }

        private void BeamDownLeft(SpriteBatch sb, Vector2 screenPos, int count, bool bouncing = false) {

            if (TooManyBeams(count))
                return;

            Point curPos = (screenPos + Main.screenPosition).ToTileCoordinates();

            int offset = 12 * 16;

            if (!bouncing) {

                sb.Draw(NeoLightBeams.BeamDiagonalStart, new Vector2(screenPos.X + offset, screenPos.Y + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);

                IncrementCounts(ref count);

            }

            Tile tileBelowLeft = Main.tile[curPos.X - count, curPos.Y + count];

            while ((curPos.X - count) * 16 > Main.screenPosition.X - (NeoLightBeams.OffscreenBlockRange * 16) &&
                   (curPos.Y + count) * 16 < Main.screenPosition.Y + Main.screenHeight + (NeoLightBeams.OffscreenBlockRange * 16) &&
                   LaserPassesThrough(tileBelowLeft, curPos.X - count, curPos.Y + count)
                  ) {

                if (TooManyBeams(count))
                    return;

                if (tileBelowLeft.type == TileType<FocusedLens>()) {

                    if (tileBelowLeft.frameX == Style.Right) {

                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X - count * 16 + offset, screenPos.Y + (count - 1) * 16 + offset), _beamColorModified * Alpha);
                        sb.Draw(NeoLightBeams.BeamBounce90Rotated, new Vector2(screenPos.X - count * 16 + 8 + offset, screenPos.Y + count * 16 + 8 + offset), null, _beamColorModified * Alpha, MathHelper.ToRadians(90), new Vector2(8, 8), 1f, SpriteEffects.None, 0f);
                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X - (count - 1) * 16 + offset, screenPos.Y + count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);

                        BeamDownRight(sb, new Vector2(((curPos.X - count + 1) * 16) - Main.screenPosition.X, ((curPos.Y + count + 1) * 16) - Main.screenPosition.Y), 0, true);

                    }
                    else if (tileBelowLeft.frameX == Style.Up) {

                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X - count * 16 + offset, screenPos.Y + (count - 1) * 16 + offset), _beamColorModified * Alpha);
                        sb.Draw(NeoLightBeams.BeamBounce90Rotated, new Vector2(screenPos.X - count * 16 + offset, screenPos.Y + count * 16 + offset), _beamColorModified * Alpha);
                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X - (count - 1) * 16 + offset, screenPos.Y + count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);

                        BeamUpLeft(sb, new Vector2(((curPos.X - count - 1) * 16) - Main.screenPosition.X, ((curPos.Y + count - 1) * 16) - Main.screenPosition.Y), 0, true);

                    }
                    else if (tileBelowLeft.frameX == Style.UpRight) {

                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X - count * 16 + offset, screenPos.Y + (count - 1) * 16 + offset), _beamColorModified * Alpha);
                        sb.Draw(NeoLightBeams.BeamDiagonalStart, new Vector2(screenPos.X - count * 16 + offset, screenPos.Y + count * 16 + offset), _beamColorModified * Alpha);
                        sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X - (count - 1) * 16 + offset, screenPos.Y + count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);

                    }
                    else if (tileBelowLeft.frameX == Style.DownLeft || tileBelowLeft.frameX == Style.DownRight || tileBelowLeft.frameX == Style.UpLeft) {

                        break;

                    }

                    if (tileBelowLeft.frameX != Style.Left && tileBelowLeft.frameX != Style.Down)
                        return;

                }
                else if (tileBelowLeft.type == TileID.Glass) {
                    LightUpGlass(sb, new Point(curPos.X - count, curPos.Y + count));
                }

                sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X - count * 16 + offset, screenPos.Y + (count - 1) * 16 + offset), _beamColorModified * Alpha);

                if (tileBelowLeft.type != TileID.Glass) {

                    Texture2D beamTexture;
                    switch (Main.rand.Next(5)) {

                        case 0: beamTexture = NeoLightBeams.BeamDiagonal0; break;
                        case 1: beamTexture = NeoLightBeams.BeamDiagonal1; break;
                        case 2: beamTexture = NeoLightBeams.BeamDiagonal2; break;
                        case 3: beamTexture = NeoLightBeams.BeamDiagonal3; break;
                        case 4: beamTexture = NeoLightBeams.BeamDiagonal4; break;
                        default: beamTexture = NeoLightBeams.BeamDiagonal; break;

                    }

                    sb.Draw(beamTexture, new Vector2(screenPos.X - count * 16 + offset, screenPos.Y + count * 16 + offset), _beamColorModified * Alpha);
                    
                    Lighting.AddLight(new Vector2(curPos.X - count, curPos.Y + count), _beamColorModified.R, _beamColorModified.G, _beamColorModified.B);

                }

                sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X - (count - 1) * 16 + offset, screenPos.Y + count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);

                IncrementCounts(ref count);

                tileBelowLeft = Main.tile[curPos.X - count, curPos.Y + count];

            }

            tileBelowLeft = Main.tile[curPos.X - count, curPos.Y + count - 1];

            if ((curPos.X - count) * 16 > Main.screenPosition.X - (NeoLightBeams.OffscreenBlockRange * 16) &&
                (curPos.Y + count - 1) * 16 < Main.screenPosition.Y + Main.screenHeight + (NeoLightBeams.OffscreenBlockRange * 16) &&
                LaserPassesThrough(tileBelowLeft)
               ) {

                sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X - count * 16 + offset, screenPos.Y + (count - 1) * 16 + offset), _beamColorModified * Alpha);

            }

            tileBelowLeft = Main.tile[curPos.X - count + 1, curPos.Y + count];

            if ((curPos.X - count + 1) * 16 > Main.screenPosition.X - (NeoLightBeams.OffscreenBlockRange * 16) &&
                (curPos.Y + count) * 16 < Main.screenPosition.Y + Main.screenHeight + (NeoLightBeams.OffscreenBlockRange * 16) &&
                LaserPassesThrough(tileBelowLeft)
               ) {

                sb.Draw(NeoLightBeams.BeamCorner, new Vector2(screenPos.X - (count - 1) * 16 + offset, screenPos.Y + count * 16 + offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);

            }

        }

        #endregion Beam Functions

        private bool ChangeStyle(int x, int y) {

            NeoLightBeams.ResetLitUpGlassList = true;

            int currentStyle = Main.tile[x, y].frameX;
            
            do {

                Main.tile[x, y].frameX += 18;

                if (Main.tile[x, y].frameX > 7 * 18)
                    Main.tile[x, y].frameX = 0;

                if (Main.tile[x, y].frameX == currentStyle) {
                    WorldGen.KillTile(x, y);
                    return false;
                }

            } while (!StyleIsValid(x, y));

            NetMessage.SendTileSquare(-1, x, y, 1);

            return true;

        }

        private void DrawLensOverlay(SpriteBatch sb, int x, int y) {

            Tile tile = Main.tile[x, y];

            if (tile.frameX < 72)
                return;

            int offset = 12 * 16;

            sb.Draw(NeoLightBeams.AngledLensOverlay, new Vector2(x * 16 - Main.screenPosition.X + offset, y * 16 - Main.screenPosition.Y + offset), new Rectangle(tile.frameX, 0, 16, 16), _beamColor * (_beamColor == Color.White ? 0.25f : 0.33f));

        }

        private void IncrementCounts(ref int count) {

            count++;
            _beamLength++;
            NeoLightBeams.TotalBeamLength++;

        }

        private static Color InvertColor(Color originalColor) => new Color(255 - originalColor.R, 255 - originalColor.G, 255 - originalColor.B, originalColor.A);

        private bool LaserPassesThrough(Tile tile, int tileX = -1, int tileY = -1) {

            if (tile.type == TileID.Torches) {

                switch (tile.frameY / 22) {

                    case 0: _beamColor = Core.Colors.GemAmber; break;
                    case 1: _beamColor = Core.Colors.GemSapphire; break;
                    case 2: _beamColor = Core.Colors.GemRuby; break;
                    case 3: _beamColor = Core.Colors.GemEmerald; break;
                    case 4: _beamColor = Core.Colors.GemAmethyst; break;
                    case 5: _beamColor = Core.Colors.GemDiamond; break;
                    case 6: _beamColor = Core.Colors.GemTopaz; break;
                    case 7: _beamColor = Color.DarkGray; break;
                    case 8: _beamColor = Color.Black; break;
                    case 9: _beamColor = Color.LightSkyBlue; break;
                    case 10: _beamColor = Core.Colors.GemAmber; break;
                    case 11: _beamColor = Color.DarkGoldenrod; break;
                    case 12: _beamColor = Core.Colors.GemDiamond; break;
                    case 13: _beamColor = Color.LightGray; break;
                    case 14: _beamColor = Color.Transparent; break; // TODO: See what this does.
                    case 15: _beamColor = Color.Pink; break;
                    default: _beamColor = Core.Colors.GemAmber; break;

                }

            }
            else if (tile.type == 129 && tileX != -1 && tileY != -1 && Main.rand.Next(10) == 0) {

                Projectile.NewProjectile(
                    tileX * 16 - 8,
                    tileY * 16 + 16,
                    (float)Main.rand.NextDouble() * (Main.rand.Next(8) + 1) * ((Main.rand.Next(2) == 1) ? -1 : 1),
                    -2,
                    ProjectileType<Projectiles.RainbowBeamProj>(),
                    0,
                    0,
                    Main.myPlayer,
                    0f,
                    0f
                );

            }

            return !tile.active() || !Main.tileSolid[tile.type] || TileID.Sets.Platforms[tile.type] || tile.type == TileID.Glass;

        }

        private void LightUpGlass(SpriteBatch sb, Point tilePos) {

            int offset = 12 * 16;

            Vector2 screenPos = new Vector2(tilePos.X * 16 - Main.screenPosition.X, tilePos.Y * 16 - Main.screenPosition.Y);
            sb.Draw(NeoLightBeams.BeamFull, screenPos + new Vector2(offset, offset), null, _beamColorModified * Alpha, 0f, default, 1f, SpriteEffects.None, 0f);

            Tuple<Point, Color> beamTuple = new Tuple<Point, Color>(tilePos, _beamColor);

            if (!NeoLightBeams.LitUpGlassList2.Contains(beamTuple))
                NeoLightBeams.LitUpGlassList2.Add(beamTuple);

        }

        private bool ShouldFire(int x, int y) {

            Tile tile = Main.tile[x, y];

            if (tile.frameY > 18)
                return false;

            Tile tileToCheck = null;
            Point tileToCheckPosition = default;

            Tile otherTileToCheck = null;
            Point otherTileToCheckPosition = default;

            switch (tile.frameX) {

                case Style.Up:

                    tileToCheck = Main.tile[x, y + 1];
                    tileToCheckPosition = new Point(x, y + 1);

                    break;

                case Style.Right:

                    tileToCheck = Main.tile[x - 1, y];
                    tileToCheckPosition = new Point(x - 1, y);

                    break;

                case Style.Left:

                    tileToCheck = Main.tile[x + 1, y];
                    tileToCheckPosition = new Point(x + 1, y);

                    break;

                case Style.Down:

                    tileToCheck = Main.tile[x, y - 1];
                    tileToCheckPosition = new Point(x, y - 1);

                    break;

                case Style.UpRight:

                    tileToCheck = Main.tile[x, y + 1];
                    tileToCheckPosition = new Point(x, y + 1);

                    otherTileToCheck = Main.tile[x - 1, y];
                    otherTileToCheckPosition = new Point(x - 1, y);

                    break;

                case Style.UpLeft:

                    tileToCheck = Main.tile[x, y + 1];
                    tileToCheckPosition = new Point(x, y + 1);

                    otherTileToCheck = Main.tile[x + 1, y];
                    otherTileToCheckPosition = new Point(x + 1, y);

                    break;

                case Style.DownRight:

                    tileToCheck = Main.tile[x, y - 1];
                    tileToCheckPosition = new Point(x, y - 1);

                    otherTileToCheck = Main.tile[x - 1, y];
                    otherTileToCheckPosition = new Point(x - 1, y);

                    break;

                case Style.DownLeft:

                    tileToCheck = Main.tile[x, y - 1];
                    tileToCheckPosition = new Point(x, y - 1);

                    otherTileToCheck = Main.tile[x + 1, y];
                    otherTileToCheckPosition = new Point(x + 1, y);

                    break;

            }

            _beamColor = Color.White;

            if (tileToCheck != null && tileToCheck.active()) {

                if (tileToCheck.type > 261 && tileToCheck.type < 269) {

                    switch (tileToCheck.type) {

                        case TileID.AmethystGemspark:
                        case TileID.AmethystGemsparkOff:
                            _beamColor = Core.Colors.GemAmethyst;
                            break;

                        case TileID.TopazGemspark:
                        case TileID.TopazGemsparkOff:
                            _beamColor = Core.Colors.GemTopaz;
                            break;

                        case TileID.SapphireGemspark:
                        case TileID.SapphireGemsparkOff:
                            _beamColor = Core.Colors.GemSapphire;
                            break;

                        case TileID.EmeraldGemspark:
                        case TileID.EmeraldGemsparkOff:
                            _beamColor = Core.Colors.GemEmerald;
                            break;

                        case TileID.RubyGemspark:
                        case TileID.RubyGemsparkOff:
                            _beamColor = Core.Colors.GemRuby;
                            break;

                        case TileID.AmberGemspark:
                        case TileID.AmberGemsparkOff:
                            _beamColor = Core.Colors.GemAmber;
                            break;

                        default:
                            _beamColor = Core.Colors.GemDiamond;
                            break;

                    }

                    if (tile.frameY == 18)
                        _beamColor = InvertColor(_beamColor);

                    return true;

                }

                if (tileToCheck.type == TileID.Glass) {

                    bool tupleFound = false;
                    Color tempColor = Color.White;

                    foreach (Tuple<Point, Color> tuple in NeoLightBeams.LitUpGlassList2) {

                        if (tuple.Item1 != tileToCheckPosition)
                            continue;

                        Color tempColor2 = tuple.Item2;

                        tempColor = tupleFound ? AlphaBlend(tempColor, tempColor2) : tempColor2;

                        tupleFound = true;

                    }

                    if (tupleFound) {

                        _beamColor = tempColor;

                        if (Main.tile[x, y].frameY != 0)
                            _beamColor = InvertColor(_beamColor);

                        return true;

                    }

                }

            }

            if (otherTileToCheck != null && otherTileToCheck.active()) {

                if (otherTileToCheck.type > 261 && otherTileToCheck.type < 269) {

                    switch (otherTileToCheck.type) {

                        case TileID.AmethystGemspark:
                        case TileID.AmethystGemsparkOff:
                            _beamColor = Core.Colors.GemAmethyst;
                            break;

                        case TileID.TopazGemspark:
                        case TileID.TopazGemsparkOff:
                            _beamColor = Core.Colors.GemTopaz;
                            break;

                        case TileID.SapphireGemspark:
                        case TileID.SapphireGemsparkOff:
                            _beamColor = Core.Colors.GemSapphire;
                            break;

                        case TileID.EmeraldGemspark:
                        case TileID.EmeraldGemsparkOff:
                            _beamColor = Core.Colors.GemEmerald;
                            break;

                        case TileID.RubyGemspark:
                        case TileID.RubyGemsparkOff:
                            _beamColor = Core.Colors.GemRuby;
                            break;

                        case TileID.AmberGemspark:
                        case TileID.AmberGemsparkOff:
                            _beamColor = Core.Colors.GemAmber;
                            break;

                        default:
                            _beamColor = Core.Colors.GemDiamond;
                            break;

                    }

                    if (Main.tile[x, y].frameY != 0)
                        _beamColor = InvertColor(_beamColor);

                    return true;

                }

                if (otherTileToCheck.type == TileID.Glass) {

                    bool tupleFound = false;
                    Color tempColor = Color.White;

                    foreach (Tuple<Point, Color> tuple in NeoLightBeams.LitUpGlassList2) {

                        if (tuple.Item1 != otherTileToCheckPosition)
                            continue;


                        Color tempColor2 = tuple.Item2;

                        tempColor = tupleFound ? AlphaBlend(tempColor, tempColor2) : tempColor2;

                        tupleFound = true;

                    }

                    if (tupleFound) {

                        _beamColor = tempColor;

                        return true;

                    }

                }

            }
                
            if (tile.frameY <= 18)
                NeoLightBeams.ResetLitUpGlassList = true;

            return false;

        }

        private static bool StyleIsValid(int x, int y) {

            Tile tileToCheck = null;
            Tile otherTileToCheck = null;

            switch (Main.tile[x, y].frameX) {

                case Style.Up:
                    tileToCheck = Main.tile[x, y + 1];
                    break;

                case Style.Right:
                    tileToCheck = Main.tile[x - 1, y];
                    break;

                case Style.Left:
                    tileToCheck = Main.tile[x + 1, y];
                    break;

                case Style.Down:
                    tileToCheck = Main.tile[x, y - 1];
                    break;

                case Style.UpRight:
                    tileToCheck = Main.tile[x, y + 1];
                    otherTileToCheck = Main.tile[x - 1, y];
                    break;

                case Style.UpLeft:
                    tileToCheck = Main.tile[x, y + 1];
                    otherTileToCheck = Main.tile[x + 1, y];
                    break;

                case Style.DownRight:
                    tileToCheck = Main.tile[x, y - 1];
                    otherTileToCheck = Main.tile[x - 1, y];
                    break;

                case Style.DownLeft:
                    tileToCheck = Main.tile[x, y - 1];
                    otherTileToCheck = Main.tile[x + 1, y];
                    break;

            }

            if (tileToCheck != null && tileToCheck.active() && Main.tileSolid[tileToCheck.type])
                return true;

            if (otherTileToCheck != null && otherTileToCheck.active() && Main.tileSolid[otherTileToCheck.type])
                return true;

            return false;

        }

        private bool TooManyBeams(int count) {

            float beamAlpha = (100 - count) / 100f;

            _beamColorModified = _beamColor * beamAlpha;

            return count > 99 || _beamLength > _maxBeamLength || NeoLightBeams.TotalBeamLength > NeoLightBeams.MaxBeamLength;

        }

        #region Color Functions

        #region Color Blending Functions

        public static Color AlphaBlend(Color foreGround, Color backGround) {

            CIELab foreGroundLab = RGBtoLab(foreGround);
            CIELab backGroundLab = RGBtoLab(backGround);

            float l = (float)(foreGroundLab.L + backGroundLab.L) / 2f;
            float a = (float)(foreGroundLab.A + backGroundLab.A) / 2f;
            float b = (float)(foreGroundLab.B + backGroundLab.B) / 2f;

            Color newRGB = LabtoRGB(new CIELab(l, a, b));
            newRGB.A = 255;

            return newRGB;

        }

        #endregion

        #region Color Convert Functions

        public static CIELab RGBtoLab(Color color) => RGBtoLab(color.R, color.G, color.B);

        public static CIELab RGBtoLab(int red, int green, int blue) => XYZtoLab(RGBtoXYZ(red, green, blue));

        public static CIEXYZ RGBtoXYZ(int red, int green, int blue) {

            // normalize red, green, blue values
            double rLinear = red / 255.0;
            double gLinear = green / 255.0;
            double bLinear = blue / 255.0;

            // convert to a sRGB form
            double r = (rLinear > 0.04045) ?
                Math.Pow((rLinear + 0.055) / (1 + 0.055), 2.2) :
                (rLinear / 12.92);
            double g = (gLinear > 0.04045) ?
                Math.Pow((gLinear + 0.055) / (1 + 0.055), 2.2) :
                (gLinear / 12.92);
            double b = (bLinear > 0.04045) ?
                Math.Pow((bLinear + 0.055) / (1 + 0.055), 2.2) :
                (bLinear / 12.92);

            // converts
            return new CIEXYZ(
                (r * 0.4124 + g * 0.3576 + b * 0.1805),
                (r * 0.2126 + g * 0.7152 + b * 0.0722),
                (r * 0.0193 + g * 0.1192 + b * 0.9505)
            );

        }

        public static Color XYZtoRGB(CIEXYZ xyzColor) => XYZtoRGB(xyzColor.X, xyzColor.Y, xyzColor.Z);

        public static Color XYZtoRGB(double x, double y, double z) {
            double[] Clinear = new double[3];
            Clinear[0] = x * 3.2410 - y * 1.5374 - z * 0.4986; // red
            Clinear[1] = -x * 0.9692 + y * 1.8760 - z * 0.0416; // green
            Clinear[2] = x * 0.0556 - y * 0.2040 + z * 1.0570; // blue

            for (int i = 0; i < 3; i++) {
                Clinear[i] = (Clinear[i] <= 0.0031308) ? 12.92 * Clinear[i] : (
                    1 + 0.055) * Math.Pow(Clinear[i], (1.0 / 2.4)) - 0.055;
            }

            return new Color(
                Convert.ToInt32(double.Parse($"{Clinear[0] * 255.0:0.00}")),
                Convert.ToInt32(double.Parse($"{Clinear[1] * 255.0:0.00}")),
                Convert.ToInt32(double.Parse($"{Clinear[2] * 255.0:0.00}"))
                );
        }

        private static double Fxyz(double t) => ((t > 0.008856) ? Math.Pow(t, (1.0 / 3.0)) : (7.787 * t + 16.0 / 116.0));

        public static CIELab XYZtoLab(CIEXYZ xyzColor) => XYZtoLab(xyzColor.X, xyzColor.Y, xyzColor.Z);

        public static CIELab XYZtoLab(double x, double y, double z) {

            CIELab lab = CIELab.Empty;

            lab.L = 116.0 * Fxyz(y / CIEXYZ.D65.Y) - 16;
            lab.A = 500.0 * (Fxyz(x / CIEXYZ.D65.X) - Fxyz(y / CIEXYZ.D65.Y));
            lab.B = 200.0 * (Fxyz(y / CIEXYZ.D65.Y) - Fxyz(z / CIEXYZ.D65.Z));

            return lab;

        }

        public static CIEXYZ LabtoXYZ(double l, double a, double b) {

            const double delta = 6.0 / 29.0;

            double fy = (l + 16) / 116.0;
            double fx = fy + (a / 500.0);
            double fz = fy - (b / 200.0);

            return new CIEXYZ(
                (fx > delta) ? CIEXYZ.D65.X * (fx * fx * fx) : (fx - 16.0 / 116.0) * 3 * (
                    delta * delta) * CIEXYZ.D65.X,
                (fy > delta) ? CIEXYZ.D65.Y * (fy * fy * fy) : (fy - 16.0 / 116.0) * 3 * (
                    delta * delta) * CIEXYZ.D65.Y,
                (fz > delta) ? CIEXYZ.D65.Z * (fz * fz * fz) : (fz - 16.0 / 116.0) * 3 * (
                    delta * delta) * CIEXYZ.D65.Z
                );
        }

        public static Color LabtoRGB(CIELab labColor) => LabtoRGB(labColor.L, labColor.A, labColor.B);

        public static Color LabtoRGB(double l, double a, double b) => XYZtoRGB(LabtoXYZ(l, a, b));

        #endregion Color Blending Functions

        #endregion

        #endregion Private Functions

    }

    #region Color Color Structures

    public struct CIELab {

        public static readonly CIELab Empty = new CIELab();

        public static bool operator ==(CIELab item1, CIELab item2) {
            return (
                item1.L == item2.L
                && item1.A == item2.A
                && item1.B == item2.B
                );
        }

        public static bool operator !=(CIELab item1, CIELab item2) {
            return (
                item1.L != item2.L
                || item1.A != item2.A
                || item1.B != item2.B
                );
        }

        public double L { get; set; }

        public double A { get; set; }

        public double B { get; set; }

        public CIELab(double l, double a, double b) {
            L = l;
            A = a;
            B = b;
        }

        public override bool Equals(object obj) {

            if (obj == null || GetType() != obj.GetType())
                return false;

            return (this == (CIELab)obj);

        }

        public override int GetHashCode() => L.GetHashCode() ^ A.GetHashCode() ^ B.GetHashCode();

    }

    public struct CIELCH {

        public static readonly CIELCH Empty = new CIELCH();

        public static bool operator ==(CIELCH item1, CIELCH item2) {
            return (
                item1.L == item2.L
                && item1.C == item2.C
                && item1.H == item2.H
                );
        }

        public static bool operator !=(CIELCH item1, CIELCH item2) {
            return (
                item1.L != item2.L
                || item1.C != item2.C
                || item1.H != item2.H
                );
        }

        public double L { get; set; }

        public double C { get; set; }

        public double H { get; set; }

        public CIELCH(double l, double c, double h) {
            L = l;
            C = c;
            H = h;
        }

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return (this == (CIELCH)obj);
        }

        public override int GetHashCode() => L.GetHashCode() ^ C.GetHashCode() ^ H.GetHashCode();

    }

    public struct CIEXYZ {

        public static readonly CIEXYZ Empty = new CIEXYZ();

        public static readonly CIEXYZ D65 = new CIEXYZ(0.9505, 1.0, 1.0890);

        private double x;
        private double y;
        private double z;

        public static bool operator ==(CIEXYZ item1, CIEXYZ item2) {
            return (
                item1.X == item2.X
                && item1.Y == item2.Y
                && item1.Z == item2.Z
                );
        }

        public static bool operator !=(CIEXYZ item1, CIEXYZ item2) {
            return (
                item1.X != item2.X
                || item1.Y != item2.Y
                || item1.Z != item2.Z
                );
        }

        public double X {
            get {
                return x;
            }
            set {
                x = (value > 0.9505) ? 0.9505 : ((value < 0) ? 0 : value);
            }
        }

        public double Y {
            get {
                return y;
            }
            set {
                y = (value > 1.0) ? 1.0 : ((value < 0) ? 0 : value);
            }
        }

        public double Z {
            get {
                return z;
            }
            set {
                z = (value > 1.089) ? 1.089 : ((value < 0) ? 0 : value);
            }
        }

        public CIEXYZ(double x, double y, double z) {
            this.x = (x > 0.9505) ? 0.9505 : ((x < 0) ? 0 : x);
            this.y = (y > 1.0) ? 1.0 : ((y < 0) ? 0 : y);
            this.z = (z > 1.089) ? 1.089 : ((z < 0) ? 0 : z);
        }

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return (this == (CIEXYZ)obj);
        }

        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();

    }

    public struct CMYK {

        public readonly static CMYK Empty = new CMYK();

        private double c;
        private double m;
        private double y;
        private double k;

        public static bool operator ==(CMYK item1, CMYK item2) {
            return (
                item1.Cyan == item2.Cyan
                && item1.Magenta == item2.Magenta
                && item1.Yellow == item2.Yellow
                && item1.Black == item2.Black
                );
        }

        public static bool operator !=(CMYK item1, CMYK item2) {
            return (
                item1.Cyan != item2.Cyan
                || item1.Magenta != item2.Magenta
                || item1.Yellow != item2.Yellow
                || item1.Black != item2.Black
                );
        }

        public double Cyan {
            get {
                return c;
            }
            set {
                c = value;
                c = (c > 1) ? 1 : ((c < 0) ? 0 : c);
            }
        }

        public double Magenta {
            get {
                return m;
            }
            set {
                m = value;
                m = (m > 1) ? 1 : ((m < 0) ? 0 : m);
            }
        }

        public double Yellow {
            get {
                return y;
            }
            set {
                y = value;
                y = (y > 1) ? 1 : ((y < 0) ? 0 : y);
            }
        }

        public double Black {
            get {
                return k;
            }
            set {
                k = value;
                k = (k > 1) ? 1 : ((k < 0) ? 0 : k);
            }
        }

        public CMYK(double c, double m, double y, double k) {
            this.c = c;
            this.m = m;
            this.y = y;
            this.k = k;
        }

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return (this == (CMYK)obj);
        }

        public override int GetHashCode() => Cyan.GetHashCode() ^ Magenta.GetHashCode() ^ Yellow.GetHashCode() ^ Black.GetHashCode();

    }

    #endregion Color Color Structures

}
