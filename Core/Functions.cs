using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TOD = Terraria.ObjectData.TileObjectData;

namespace NeoDraw.Core {

    public class Functions {

        public static Point MousePoint => new Point(Main.mouseX, Main.mouseY);

        public static Tile TileTarget_Tile => Main.tile[Player.tileTargetX, Player.tileTargetY];

        public static Vector2 MouseVector => new Vector2(Main.mouseX, Main.mouseY);

        public static Vector2 TileTarget_Vector => new Vector2(Player.tileTargetX, Player.tileTargetY);

        private static readonly Dictionary<string, List<Vector2>> CircleCache = new Dictionary<string, List<Vector2>>();

        public static Color ColorBorder(int x, int y, int width, int height, int borderThickness, int borderRadius, int borderShadow, Color initialColor, List<Color> borderColors, float initialShadowIntensity, float finalShadowIntensity) {

            Rectangle internalRectangle = new Rectangle((borderThickness + borderRadius), (borderThickness + borderRadius), width - 2 * (borderThickness + borderRadius), height - 2 * (borderThickness + borderRadius));

            if (internalRectangle.Contains(x, y))
                return initialColor;

            Vector2 origin = Vector2.Zero;
            Vector2 point = new Vector2(x, y);

            if (x < borderThickness + borderRadius) {

                if (y < borderRadius + borderThickness) {
                    origin = new Vector2(borderRadius + borderThickness, borderRadius + borderThickness);
                }
                else if (y > height - (borderRadius + borderThickness)) {
                    origin = new Vector2(borderRadius + borderThickness, height - (borderRadius + borderThickness));
                }
                else {
                    origin = new Vector2(borderRadius + borderThickness, y);
                }

            }
            else if (x > width - (borderRadius + borderThickness)) {

                if (y < borderRadius + borderThickness) {
                    origin = new Vector2(width - (borderRadius + borderThickness), borderRadius + borderThickness);
                }
                else if (y > height - (borderRadius + borderThickness)) {
                    origin = new Vector2(width - (borderRadius + borderThickness), height - (borderRadius + borderThickness));
                }
                else {
                    origin = new Vector2(width - (borderRadius + borderThickness), y);
                }

            }
            else {

                if (y < borderRadius + borderThickness) {
                    origin = new Vector2(x, borderRadius + borderThickness);
                }
                else if (y > height - (borderRadius + borderThickness)) {
                    origin = new Vector2(x, height - (borderRadius + borderThickness));
                }

            }

            if (!origin.Equals(Vector2.Zero)) {

                float distance = Vector2.Distance(point, origin);

                if (distance > borderRadius + borderThickness + 1) {
                    return Color.Transparent;
                }

                if (distance > borderRadius + 1) {

                    if (borderColors.Count > 2) {

                        float modNum = distance - borderRadius;

                        if (modNum < borderThickness / 2)
                            return Color.Lerp(borderColors[2], borderColors[1], (float)((modNum) / (borderThickness / 2.0)));

                        return Color.Lerp(borderColors[1], borderColors[0], (float)((modNum - (borderThickness / 2.0)) / (borderThickness / 2.0)));

                    }


                    if (borderColors.Count > 0)
                        return borderColors[0];

                }
                else if (distance > borderRadius - borderShadow + 1) {

                    float mod = (distance - (borderRadius - borderShadow)) / borderShadow;
                    float shadowDiff = initialShadowIntensity - finalShadowIntensity;

                    return DarkenColor(initialColor, ((shadowDiff * mod) + finalShadowIntensity));

                }

            }

            return initialColor;

        }

        public static List<Vector2> CreateArc(float radius, int sides, float startingAngle, float radians) {

            List<Vector2> list = new List<Vector2>();

            list.AddRange(CreateCircle(radius, sides));
            list.RemoveAt(list.Count - 1);

            double num = 0;
            double num2 = Math.PI * 2 / sides;

            while (num + num2 / 2.0 < startingAngle) {
                num += num2;
                list.Add(list[0]);
                list.RemoveAt(0);
            }

            list.Add(list[0]);
            int num3 = (int)(radians / num2 + 0.5);
            list.RemoveRange(num3 + 1, list.Count - num3 - 1);

            return list;

        }

        public static List<Vector2> CreateCircle(double radius, int sides) {

            string key = radius + "x" + sides;

            if (CircleCache.ContainsKey(key))
                return CircleCache[key];

            List<Vector2> list = new List<Vector2>();
            double num = Math.PI * 2 / sides;

            for (double num2 = 0; num2 < Math.PI * 2; num2 += num)
                list.Add(new Vector2((float)(radius * Math.Cos(num2)), (float)(radius * Math.Sin(num2))));

            list.Add(new Vector2((float)(radius * Math.Cos(0.0)), (float)(radius * Math.Sin(0.0))));
            CircleCache.Add(key, list);

            return list;

        }

        public Texture2D CreateRoundedRectangleTexture(GraphicsDevice graphics, int width, int height, int borderThickness, int borderRadius, int borderShadow, List<Color> backgroundColors, List<Color> borderColors, float initialShadowIntensity, float finalShadowIntensity) {

            #region ArgumentExceptions

            if (backgroundColors == null || backgroundColors.Count == 0)
                throw new ArgumentException("Must define at least one background color (up to four).");
            if (borderColors == null || borderColors.Count == 0)
                throw new ArgumentException("Must define at least one border color (up to three).");
            if (borderRadius < 1)
                throw new ArgumentException("Must define a border radius (rounds off edges).");
            if (borderThickness < 1)
                throw new ArgumentException("Must define border thikness.");
            if (borderThickness + borderRadius > height / 2 || borderThickness + borderRadius > width / 2)
                throw new ArgumentException("Border will be too thick and/or rounded to fit on the texture.");
            if (borderShadow > borderRadius)
                throw new ArgumentException("Border shadow must be lesser in magnitude than the border radius (suggeted: shadow <= 0.25 * radius).");

            #endregion ArgumentExceptions

            Texture2D texture = new Texture2D(graphics, width, height, false, SurfaceFormat.Color);
            Color[] color = new Color[width * height];

            for (int x = 0; x < texture.Width; x++) {

                for (int y = 0; y < texture.Height; y++) {

                    switch (backgroundColors.Count) {

                        case 4:
                            Color leftColor0 = Color.Lerp(backgroundColors[0], backgroundColors[1], ((float)y / (width - 1)));
                            Color rightColor0 = Color.Lerp(backgroundColors[2], backgroundColors[3], ((float)y / (height - 1)));
                            color[x + width * y] = Color.Lerp(leftColor0, rightColor0, ((float)x / (width - 1)));
                            break;

                        case 3:
                            Color leftColor1 = Color.Lerp(backgroundColors[0], backgroundColors[1], ((float)y / (width - 1)));
                            Color rightColor1 = Color.Lerp(backgroundColors[1], backgroundColors[2], ((float)y / (height - 1)));
                            color[x + width * y] = Color.Lerp(leftColor1, rightColor1, ((float)x / (width - 1)));
                            break;

                        case 2:
                            color[x + width * y] = Color.Lerp(backgroundColors[0], backgroundColors[1], ((float)x / (width - 1)));
                            break;

                        default:
                            color[x + width * y] = backgroundColors[0];
                            break;

                    }

                    color[x + width * y] = ColorBorder(x, y, width, height, borderThickness, borderRadius, borderShadow, color[x + width * y], borderColors, initialShadowIntensity, finalShadowIntensity);

                }

            }

            texture.SetData(color);

            return texture;

        }

        public static Color DarkenColor(Color color, float shadowIntensity) => Color.Lerp(color, Color.Black, shadowIntensity);

        ///<summary>
        ///Will draw a border (hollow rectangle) of the given 'thicknessOfBorder' (in pixels)
        ///of the specified color.
        ///
        ///By Sean Colombo, from http://bluelinegamestudios.com/blog
        ///</summary>
        ///<param name="sb">The SpriteBatch currently being used to draw to screen.</param>
        ///<param name="borderTexture">The 1x1 Texture2D used to draw the border.</param>
        ///<param name="rectangleToDraw">The border position and size.</param>
        ///<param name="thicknessOfBorder">How thick in pixels the border should be.</param>
        ///<param name="borderColor">What color the border should be.</param>
        public void DrawBorder(SpriteBatch sb, Texture2D borderTexture, Rectangle rectangleToDraw, int thicknessOfBorder, Color borderColor) {

            if (borderTexture == null)
                return;

            // Draw top line
            sb.Draw(borderTexture, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y, rectangleToDraw.Width, thicknessOfBorder), borderColor);

            // Draw left line
            sb.Draw(borderTexture, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y, thicknessOfBorder, rectangleToDraw.Height), borderColor);

            // Draw right line
            sb.Draw(borderTexture, new Rectangle((rectangleToDraw.X + rectangleToDraw.Width - thicknessOfBorder),
                                            rectangleToDraw.Y,
                                            thicknessOfBorder,
                                            rectangleToDraw.Height), borderColor);

            // Draw bottom line
            sb.Draw(borderTexture, new Rectangle(rectangleToDraw.X,
                                            rectangleToDraw.Y + rectangleToDraw.Height - thicknessOfBorder,
                                            rectangleToDraw.Width,
                                            thicknessOfBorder), borderColor);

        }

        public static Point FindTopLeftPoint(int x, int y) {
            FindTopLeft(ref x, ref y);
            return new Point(x, y);
        }

        public static Tuple<int, int> FindTopLeft(int x, int y) {
            FindTopLeft(ref x, ref y);
            return new Tuple<int, int>(x, y);
        }

        public static void FindTopLeft(ref int x, ref int y) {

            int type = Main.tile[x, y].type;

            if (!Main.tileFrameImportant[type])
                return;

            if (type < TileID.Count)
                return;

            // TODO: Check to make sure frameWidth and frameHeight are being calculated correctly
            int frameWidth = TOD.GetTileData(Main.tile[x, y]).CoordinateWidth + TOD.GetTileData(Main.tile[x, y]).CoordinatePadding;
            int frameHeight = TOD.GetTileData(Main.tile[x, y]).CoordinateHeights[0] + TOD.GetTileData(Main.tile[x, y]).CoordinatePadding;

            int num = Main.tile[x, y].frameX / (frameWidth + 2);
            int num2 = Main.tile[x, y].frameY / (frameHeight + 2);

            num %= TOD.GetTileData(Main.tile[x, y]).Width;
            num2 %= TOD.GetTileData(Main.tile[x, y]).Height;

            x -= num;
            y -= num2;

        }

        /// <summary>
        /// Generates a single tile of liquid.
        /// </summary>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <param name="liquidType">Type of liquid to add. (0: Water, 1: Lava, 2: Honey)</param>
        /// <param name="updateFlow">True if you want the flow to update after placement. (Almost definitely yes)</param>
        /// <param name="liquidHeight">The height given to the liquid. (0 - 255)</param>
        /// <param name="sync">If true, will sync the client and server.</param>
        public static void GenerateLiquid(int x, int y, int liquidType, bool updateFlow = true, int liquidHeight = 255, bool sync = true) {

            liquidHeight = (int)MathHelper.Clamp(liquidHeight, 0, 255);
            Main.tile[x, y].liquid = (byte)liquidHeight;

            switch (liquidType) {

                case 0:
                    Main.tile[x, y].lava(false);
                    Main.tile[x, y].honey(false);
                    break;

                case 1:
                    Main.tile[x, y].lava(true);
                    Main.tile[x, y].honey(false);
                    break;

                case 2:
                    Main.tile[x, y].lava(false);
                    Main.tile[x, y].honey(true);
                    break;

            }

            if (updateFlow)
                Liquid.AddWater(x, y);

            if (sync && Main.netMode != 0)
                NetMessage.SendTileSquare(-1, x, y, 1);

        }

        /// <summary>
        /// Generates a width by height block of liquid with x, y being the top-left corner.
        /// </summary>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <param name="width">Width of area to add liquid.</param>
        /// <param name="height">Height of area to add liquid.</param>
        /// <param name="liquidType">Type of liquid to add. (0: Water, 1: Lava, 2: Honey)</param>
        /// <param name="updateFlow">True if you want the flow to update after placement. (Almost definitely yes)</param>
        /// <param name="liquidHeight">The height given to the liquid. (0 - 255)</param>
        /// <param name="sync">If true, will sync the client and server.</param>
        public static void GenerateLiquid(int x, int y, int width, int height, int liquidType, bool updateFlow = true, int liquidHeight = 255, bool sync = true) {

            for (int x1 = 0; x1 < width; x1++)
                for (int y1 = 0; y1 < height; y1++)
                    GenerateLiquid(x1 + x, y1 + y, liquidType, updateFlow, liquidHeight, false);

            int size = (width > height ? width : height);

            if (sync && Main.netMode != 0)
                NetMessage.SendTileSquare(-1, x + (int)(width * 0.5F) - 1, y + (int)(height * 0.5F) - 1, size + 4);

        }

        /// <summary>
        /// Generates a single tile and wall at the given coordinates. (if the tile is > 1 x 1 it assumes the passed in coordinate is the top left)
        /// </summary>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <param name="tile">Type of tile to place. -1 means don't do anything tile related, -2 is used in conjunction with active == false to make air.</param>
        /// <param name="wall">Type of wall to place. -1 means don't do anything wall related. -2 is used to remove the wall already there.</param>
        /// <param name="tileStyle">The style of the given tile.</param>
        /// <param name="active">If false, will make the tile 'air' and show the wall only.</param>
        /// <param name="removeLiquid">If true, it will remove liquids in the generating area.</param>
        /// <param name="slope">If -2, keep the current slope. If -1, make it a halfbrick, otherwise make it the slope given.</param>
        /// <param name="silent">If true, will not display dust nor sound.</param>
        /// <param name="sync">If true, will sync the client and server.</param>
        public static void GenerateTile(int x, int y, int tile, int wall, int tileStyle = 0, bool active = true, bool removeLiquid = true, int slope = -2, bool silent = false, bool sync = true) {

            if (Main.tile[x, y] == null)
                Main.tile[x, y] = new Tile();

            int width = TOD.GetTileData(Main.tile[x, y]).Width;
            int height = TOD.GetTileData(Main.tile[x, y]).Height;
            int tileWidth = (tile == -1 ? 1 : TOD.GetTileData(tile, tileStyle).Width);
            int tileHeight = (tile == -1 ? 1 : TOD.GetTileData(tile, tileStyle).Height);
            byte oldSlope = Main.tile[x, y].slope();
            bool oldHalfBrick = Main.tile[x, y].halfBrick();

            if (tile != -1) {

                WorldGen.destroyObject = true;

                if (width > 1 || height > 1) {

                    int xs = x;
                    int ys = y;

                    Tuple<int, int> top = FindTopLeft(xs, ys);
                    Vector2 newPos = new Vector2(top.Item1, top.Item2);

                    for (int x1 = 0; x1 < width; x1++) {

                        for (int y1 = 0; y1 < height; y1++) {

                            int x2 = (int)newPos.X + x1;
                            int y2 = (int)newPos.Y + y1;

                            if (x1 == 0 && y1 == 0 && Main.tile[x2, y2].type == 21) //is a chest, special case to prevent dupe glitch
                                KillChestAndItems(x2, y2);

                            Main.tile[x, y].type = 0;
                            Main.tile[x, y].active(false);

                            if (!silent)
                                WorldGen.KillTile(x, y, false, false, true);

                            if (removeLiquid)
                                GenerateLiquid(x2, y2, 0, true, 0, false);

                        }

                    }

                    for (int x1 = 0; x1 < width; x1++) {

                        for (int y1 = 0; y1 < height; y1++) {

                            int x2 = (int)newPos.X + x1;
                            int y2 = (int)newPos.Y + y1;

                            WorldGen.SquareTileFrame(x2, y2);
                            WorldGen.SquareWallFrame(x2, y2);

                        }

                    }

                }
                else if (!silent) {

                    WorldGen.KillTile(x, y, false, false, true);

                }

                WorldGen.destroyObject = false;

                if (active) {

                    if (tileWidth <= 1 && tileHeight <= 1) {

                        Main.tile[x, y].type = (ushort)tile;
                        Main.tile[x, y].active(true);

                        if (slope == -2 && oldHalfBrick) {
                            Main.tile[x, y].halfBrick(true);
                        }
                        else if (slope == -1) {
                            Main.tile[x, y].halfBrick(true);
                        }
                        else {
                            Main.tile[x, y].slope(slope == -2 ? oldSlope : (byte)slope);
                        }

                        WorldGen.SquareTileFrame(x, y);

                    }
                    else {

                        WorldGen.destroyObject = true;

                        if (!silent)
                            for (int x1 = 0; x1 < tileWidth; x1++)
                                for (int y1 = 0; y1 < tileHeight; y1++)
                                    WorldGen.KillTile(x + x1, y + y1, false, false, true);

                        WorldGen.destroyObject = false;

                        int genX = x;
                        int genY = (tile == TileID.ClosedDoor ? y : y + height);

                        WorldGen.PlaceTile(genX, genY, tile, true, true, -1, tileStyle);

                        for (int x1 = 0; x1 < tileWidth; x1++) {
                            for (int y1 = 0; y1 < tileHeight; y1++) {
                                WorldGen.SquareTileFrame(x + x1, y + y1);
                            }
                        }

                    }

                }
                else {

                    Main.tile[x, y].active(false);

                }

            }

            if (wall != -1) {

                if (wall == -2)
                    wall = 0;

                Main.tile[x, y].wall = 0;
                WorldGen.PlaceWall(x, y, wall, true);

            }

            if (!sync || Main.netMode == 0)
                return;

            int sizeWidth = tileWidth + Math.Max(0, (width - 1));
            int sizeHeight = tileHeight + Math.Max(0, (height - 1));
            int size = sizeWidth > sizeHeight ? sizeWidth : sizeHeight;

            NetMessage.SendTileSquare(-1, x + (int)(size * 0.5F), y + (int)(size * 0.5F), size + 1);

        }

        /// <summary>
        /// Gets the closest Projectile with the given type within the given distance from the center. If distance is -1, it gets the closest Projectile.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="projType">If -1, check for ANY projectiles in the area. If not, check for the projectiles who match the type given.</param>
        /// <param name="owner"></param>
        /// <param name="projsToExclude">An array of projectile whoAmIs to exclude from the search.</param>
        /// <param name="distance">The distance to check.</param>
        /// <param name="canAdd"></param>
        /// <returns></returns>
        public static int GetProjectile(Vector2 center, int projType = -1, int owner = -1, int[] projsToExclude = default(int[]), float distance = -1, Func<Projectile, bool> canAdd = null) {

            int currentProj = -1;

            for (int i = 0; i < Main.projectile.Length; i++) {

                Projectile proj = Main.projectile[i];

                if (proj != null && proj.active && (projType == -1 || proj.type == projType) && (owner == -1f || proj.owner == owner) && (distance == -1f || proj.Distance(center) < distance)) {

                    bool add = true;

                    if (projsToExclude != default(int[]))
                        if (projsToExclude.Any(m => m == proj.whoAmI))
                            add = false;

                    if (add && canAdd != null && !canAdd(proj))
                        continue;

                    if (add) {
                        distance = proj.Distance(center);
                        currentProj = i;
                    }

                }

            }

            return currentProj;

        }

        public static void GetSpawnCoords(ref int x, ref int y) {

            Tile tile = Main.tile[x, y];

            if (tile == null || !tile.nactive() || tile.type < 340 || !TileLoader.IsModBed(tile.type))
                return;

            FindTopLeft(ref x, ref y);

            //if (spawnAt[tile.type].HasValue) {
            //    x += spawnAt[tile.type].Value.X;
            //    y += spawnAt[tile.type].Value.Y;
            //}
            //else {
            x += (int)Math.Max(1f, TOD.GetTileData(tile).Width / 2f) - 1;
            y += TOD.GetTileData(tile).Height - 1;
            //}

            y++;

        }

        /// <summary>
        /// Check if the given value is in the array.
        /// </summary>
        /// <param name="array">Array to check.</param>
        /// <param name="value">Value to look for.</param>
        /// <returns></returns>
        public static bool InArray(int[] array, int value) {
            return array.Any(t => value == t);
        }

        /// <summary>
        /// Check if the given value is in the array, and returns the array index of the value if found.
        /// </summary>
        /// <param name="array">Array to check.</param>
        /// <param name="value">Value to look for.</param>
        /// <param name="index">Index location of the value</param>
        /// <returns></returns>
        public static bool InArray(int[] array, int value, ref int index) {

            for (int m = 0; m < array.Length; m++) {

                if (value != array[m])
                    continue;

                index = m;

                return true;

            }

            return false;

        }

        public static bool KeyboardInputFocused() {
            return !Main.chatRelease || Main.editSign || Main.editChest;
        }

        /// <summary>
        /// Completely kills a chest at X, Y and removes all items within it. (Note this does not remove the tile itself)
        /// </summary>
        /// <param name="X">X Coordinate</param>
        /// <param name="Y">Y Coordinate</param>
        /// <returns></returns>
        public static bool KillChestAndItems(int X, int Y) {

            for (int i = 0; i < 1000; i++) {

                if (Main.chest[i] == null || Main.chest[i].x != X || Main.chest[i].y != Y)
                    continue;

                Main.chest[i] = null;

                return true;

            }

            return false;

        }

        public static Player localPlayer => Main.player[Main.myPlayer];

        public static Vector2 mouse => new Vector2(Main.mouseX, Main.mouseY);

        /// <summary>
        /// Replaces tiles within a certain radius with the replacements. (Circular)
        /// </summary>
        /// <param name="position">The position of the center. (NOTE THIS IS NPC/PROJECTILE COORDS NOT TILE)</param>
        /// <param name="radius">The radis from the position you want to replace to.</param>
        /// <param name="tiles">The array of tiles you want to replace.</param>
        /// <param name="replacements">The array of replacement tiles. (It goes by using the same index as tiles. Ie, tiles[0] will be replaced with replacements[0].)</param>
        /// <param name="silent">The conditional over which of wether to sync or not.</param>
        /// <param name="sync">If true, prevents sounds and dusts.</param>
        public static void ReplaceTiles(Vector2 position, int radius, int[] tiles, int[] replacements, bool silent = false, bool sync = true) {

            int radiusLeft = Math.Max((int)(position.X / 16f - radius), 0);
            int radiusRight = Math.Min((int)(position.X / 16f + radius), Main.maxTilesX);
            int radiusUp = Math.Max((int)(position.Y / 16f - radius), 0);
            int radiusDown = Math.Min((int)(position.Y / 16f + radius), Main.maxTilesY);

            for (int x1 = radiusLeft; x1 <= radiusRight; x1++) {

                for (int y1 = radiusUp; y1 <= radiusDown; y1++) {

                    float distX = Math.Abs(x1 - position.X / 16f);
                    float distY = Math.Abs(y1 - position.Y / 16f);
                    double dist = Math.Sqrt(distX * distX + distY * distY);

                    if (dist < radius && Main.tile[x1, y1] != null && Main.tile[x1, y1].active()) {

                        int currentType = Main.tile[x1, y1].type;
                        int index = 0;

                        if (InArray(tiles, currentType, ref index))
                            GenerateTile(x1, y1, replacements[index], -1, 0, true, false, -2, silent, false);

                    }

                }

            }

            if (sync && Main.netMode != 0)
                NetMessage.SendTileSquare(-1, (int)(position.X / 16f), (int)(position.Y / 16f), (radius * 2) + 2);

        }

        public static bool ShowItemIcon(Player player) {
            // TODO: Make sure lastTileRangeX works the same as TAPIs tileRangeX
            if (player.position.X / 16f - player.lastTileRangeX - player.inventory[player.selectedItem].tileBoost - player.blockRange <= Player.tileTargetX &&
                (player.position.X + player.width) / 16f + player.lastTileRangeX + player.inventory[player.selectedItem].tileBoost - 1f + player.blockRange >= Player.tileTargetX &&
                player.position.Y / 16f - player.lastTileRangeY - player.inventory[player.selectedItem].tileBoost - player.blockRange <= Player.tileTargetY &&
                (player.position.Y + player.height) / 16f + player.lastTileRangeY + player.inventory[player.selectedItem].tileBoost - 2f + player.blockRange >= Player.tileTargetY) {

                return true;

            }

            return false;

        }

        public static Tile TileTarget => Main.tile[Player.tileTargetX, Player.tileTargetY];

    }

    public class ListItem {

        public int ID { get; private set; }
        public string Name { get; private set; }

        public ListItem(int id, string name) {

            ID = id;
            Name = name;

        }

    }

}
