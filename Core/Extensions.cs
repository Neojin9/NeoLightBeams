using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using NeoDraw.Core;


namespace NeoDraw.Core {

    public static class Extensions {

        private static Texture2D _pixel;

        public const string ALLOWED_NAMESPACE_CHARS = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890_";
        public const string ALLOWED_NAMESPACE_DIGIT_CHARS = "1234567890";

        #region Bool Extensions

        public static int ToInt(this bool value) => !value ? 0 : 1;

        public static int ToIntDirection(this bool value) => !value ? -1 : 1;

        #endregion

        #region Color Extensions

        public static Color Multiply(this Color color, Color color2) => new Color(color.ToVector4() * color2.ToVector4());

        public static Color Premultiply(this Color color) {
            
            float num = color.A / 255f;
            
            return new Color(color.R / 255f * num, color.G / 255f * num, color.B / 255f * num, num);

        }

        public static string ToHex(this Color color, bool includeHash = true, bool includeAlpha = false) {

            string[] value = includeAlpha ? new[] {
                color.A.ToString("X2"),
                color.R.ToString("X2"),
                color.G.ToString("X2"),
                color.B.ToString("X2")
            } : new[] {
                color.R.ToString("X2"),
                color.G.ToString("X2"),
                color.B.ToString("X2")
            };

            return (includeHash ? "#" : string.Empty) + string.Join(string.Empty, value);

        }

        #endregion

        #region Float Extensions

        public static float RotateLerp(this float current, float target, float lerpValue) {
            
            float angle;
            
            if (target < current) {
                float num = target + 6.28318548f;
                angle = ((num - current > current - target) ? MathHelper.Lerp(current, target, lerpValue) : MathHelper.Lerp(current, num, lerpValue));
            } else {
                if (target <= current)
                    return current;
                float num = target - 6.28318548f;
                angle = ((target - current > current - num) ? MathHelper.Lerp(current, num, lerpValue) : MathHelper.Lerp(current, target, lerpValue));
            }

            return MathHelper.WrapAngle(angle);

        }

        public static float RotateClamp(this float current, float target, float clampValue) {
            
            current = MathHelper.WrapAngle(current);
            target = MathHelper.WrapAngle(target);
            
            if (current < target) {
                if (target - current > 3.14159274f)
                    current += 6.28318548f;
            } else if (current - target > 3.14159274f)
                current -= 6.28318548f;
            
            current += MathHelper.Clamp(target - current, -clampValue, clampValue);
            
            return MathHelper.WrapAngle(current);

        }

        #endregion

        #region Int Extensions

        public static void Times(this int times, Action code) {
            for (int i = 0; i < times; i++)
                code();
        }

        public static void Times(this int times, Action<int> code) {
            for (int i = 0; i < times; i++)
                code(i);
        }

        #endregion

        #region Keys Extensions

        public static bool Down(this Keys key) => Main.keyState.IsKeyDown(key);

        public static bool Up(this Keys key) => Main.keyState.IsKeyUp(key);

        public static bool Pressed(this Keys key) => Main.keyState.GetPressedKeys().Contains(key);

        public static bool Released(this Keys key) => Main.oldKeyState.GetPressedKeys().Contains(key) && !Main.keyState.GetPressedKeys().Contains(key);

        public static bool WasDown(this Keys key) => Main.oldKeyState.IsKeyDown(key) && !Main.keyState.IsKeyDown(key);

        public static bool WasUp(this Keys key) => Main.oldKeyState.IsKeyUp(key) && !Main.keyState.IsKeyUp(key);

        #endregion

        #region KeyState Extensions

        public static bool PressingAlt(this KeyboardState keyboardState) => keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt);

        #endregion

        #region MouseState Extensions

        public static Vector2 Position(this MouseState mouse) => new Vector2(mouse.X, mouse.Y);

        #endregion

        #region Object Extensions

        public static bool IsNull(this object source) => source == null;

        public static bool IsNotNull(this object source) => source != null;

        #endregion

        #region Point Extensions

        public static Vector2 ToPositionCoordinates(this Point tileCoordinates) => new Vector2(tileCoordinates.X * 16, tileCoordinates.Y * 16);

        #endregion

        #region Player Extensions

        public static bool ShowItemIcon(this Player player) {
            
            return player.position.X / 16f - player.lastTileRangeX - player.inventory[player.selectedItem].tileBoost - player.blockRange <= Player.tileTargetX &&
                   (player.position.X + player.width) / 16f + player.lastTileRangeX + player.inventory[player.selectedItem].tileBoost - 1f + player.blockRange >= Player.tileTargetX &&
                   player.position.Y / 16f - player.lastTileRangeY - player.inventory[player.selectedItem].tileBoost - player.blockRange <= Player.tileTargetY &&
                   (player.position.Y + player.height) / 16f + player.lastTileRangeY + player.inventory[player.selectedItem].tileBoost - 2f + player.blockRange >= Player.tileTargetY;
            
        }

        #endregion

        #region Random Extensions

        public static float NextFloat(this Random random) => (float)random.NextDouble();

        public static Vector2 NextVector2(this Random random, float minimum, float maximum) => new Vector2((maximum - minimum) * random.NextFloat() + minimum, (maximum - minimum) * random.NextFloat() + minimum);

        public static Vector2 NextVector2Round(this Random random, float minimumRadius, float maximumRadius) => Vector2.UnitX.RotateRandom(3.1415927410125732) * ((maximumRadius - minimumRadius) * random.NextFloat() + minimumRadius);

        public static bool OneIn(this Random random, int sides) => random.Next(sides) == 0;

        #endregion

        #region Rectangle Extensions

        public static Vector2 Bottom(this Rectangle r) => new Vector2((r.X + r.Width / 2), (r.Y + r.Height));

        public static Vector2 BottomLeft(this Rectangle r) => new Vector2(r.X, (r.Y + r.Height));

        public static Vector2 BottomRight(this Rectangle r) => new Vector2((r.X + r.Width), (r.Y + r.Height));

        public static Vector2 Center(this Rectangle r) => new Vector2((r.X + r.Width / 2), (r.Y + r.Height / 2));

        public static bool Contains(this Rectangle rect, Vector2 v) => rect.Contains((int)v.X, (int)v.Y);

        public static Vector2 Left(this Rectangle r) => new Vector2(r.X, (r.Y + r.Height / 2));

        public static Vector2 Right(this Rectangle r) => new Vector2((r.X + r.Width), (r.Y + r.Height / 2));

        public static Vector2 Size(this Rectangle r) => new Vector2(r.Width, r.Height);

        public static Vector2 Top(this Rectangle r) => new Vector2((r.X + r.Width / 2), r.Y);

        public static Vector2 TopLeft(this Rectangle r) => new Vector2(r.X, r.Y);

        public static Vector2 TopRight(this Rectangle r) => new Vector2((r.X + r.Width), r.Y);

        #endregion

        #region SpriteBatch Extensions

        public static void DrawArc(this SpriteBatch spriteBatch, Vector2 center, float radius, int sides, float startingAngle, float radians, Color color, float thickness) {

            List<Vector2> points = Functions.CreateArc(radius, sides, startingAngle, radians);
            Drawing.DrawPoints(spriteBatch, center, points, color, thickness);

        }

        public static void DrawCircle(this SpriteBatch spriteBatch, Vector2 center, float radius, int sides, Color color, float thickness) {
            Drawing.DrawPoints(spriteBatch, center, Functions.CreateCircle(radius, sides), color, thickness);
        }

        public static void DrawLine(this SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, Color color, float thickness) {
            spriteBatch.DrawLine(new Vector2(x1, y1), new Vector2(x2, y2), color, thickness);
        }

        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness) {

            float length = Vector2.Distance(point1, point2);
            float angle = (float)Math.Atan2((point2.Y - point1.Y), (point2.X - point1.X));

            spriteBatch.DrawLine(point1, length, angle, color, thickness);

        }

        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness) {

            if (_pixel == null)
                CreateThePixel(spriteBatch);

            spriteBatch.Draw(_pixel, point, null, color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0f);

        }

        public static void DrawRectangle(this SpriteBatch spriteBatch, Vector2 location, Vector2 size, Color color, float thickness) {
            spriteBatch.DrawRectangle(new Rectangle((int)location.X, (int)location.Y, (int)size.X, (int)size.Y), color, thickness);
        }

        public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle rect, Color color, float thickness) {

            spriteBatch.DrawLine(new Vector2(rect.X, rect.Y), new Vector2(rect.Right, rect.Y), color, thickness);
            spriteBatch.DrawLine(new Vector2(rect.X + 1f, rect.Y), new Vector2(rect.X + 1f, rect.Bottom + thickness), color, thickness);
            spriteBatch.DrawLine(new Vector2(rect.X, rect.Bottom), new Vector2(rect.Right, rect.Bottom), color, thickness);
            spriteBatch.DrawLine(new Vector2(rect.Right + 1f, rect.Y), new Vector2(rect.Right + 1f, rect.Bottom + thickness), color, thickness);

        }

        public static void FillRectangle(this SpriteBatch spriteBatch, Vector2 location, Vector2 size, Color color, float angle) {

            if (_pixel == null)
                CreateThePixel(spriteBatch);

            spriteBatch.Draw(_pixel, location, null, color, angle, Vector2.Zero, size, SpriteEffects.None, 0f);

        }

        public static void PutPixel(this SpriteBatch spriteBatch, Vector2 position, Color color) {

            if (_pixel == null)
                CreateThePixel(spriteBatch);

            spriteBatch.Draw(_pixel, position, color);

        }

        #endregion

        #region String Extensions

        public static string AllowedNamespace(this string input) {

            for (int i = 0; i < input.Length; i++) {
                if (!"qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890_".Contains(input[i]))
                    input = input.Substring(0, i) + '_' + ((i == input.Length - 1) ? "" : input.Substring(i + 1));
            }

            while (input.IndexOf("__") != -1)
                input = input.Replace("__", "_");

            while (input.StartsWith("_"))
                input = input.Substring(1);

            while (input.EndsWith("_"))
                input = input.Substring(0, input.Length - 1);

            if ("1234567890".Contains(input[0]))
                input = "_" + input;

            return input;

        }

        public static string Delete(this string s, int count = 1, int at = -1) {
            
            if (count < 1)
                throw new ArgumentException();
            
            if (count > s.Length)
                count = s.Length;
            
            if (at < 0)
                at = s.Length - count;
            
            return s.Substring(0, at) + s.Substring(at + count);

        }

        public static string Format(this string s, params object[] args) => string.Format(s, args);

        public static bool Like(this string input, string pattern) => Regex.IsMatch(input, "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.IgnoreCase);

        #endregion

        #region T Extensions

        public static bool Between<T>(this T actual, T lower, T upper, Includes bounds = Includes.Lower) where T : IComparable<T> {

            switch (bounds) {

                case Includes.Neither:
                    return actual.CompareTo(lower) > 0 && actual.CompareTo(upper) < 0;

                case Includes.Upper:
                    return actual.CompareTo(lower) > 0 && actual.CompareTo(upper) <= 0;

                case Includes.Both:
                    return actual.CompareTo(lower) >= 0 && actual.CompareTo(upper) <= 0;

                default:
                    return actual.CompareTo(lower) >= 0 && actual.CompareTo(upper) < 0;

            }

        }

        public static bool Is<T>(this T source, params T[] list) {

            if (list == null)
                throw new ArgumentNullException("list");

            return list.Contains(source);

        }

        public static T[] Join<T>(this T[] array1, T[] array2) where T : class {

            if (array1 == null && array2 == null)
                return new T[0];

            if (array1 == null)
                return array2;

            if (array2 == null)
                return array1;

            if (array1.Length == 0 && array2.Length == 0)
                return new T[0];

            T[] newArray = new T[array1.Length + array2.Length];

            int index = 0;

            for (int i = 0; i < array1.Length; i++)
                newArray[index++] = array1[i];

            for (int i = 0; i < array2.Length; i++)
                newArray[index++] = array2[i];

            return newArray;

        }

        public static List<T> ToList<T>(this T[] array) where T : class {

            List<T> newList = new List<T>();

            if (array == null)
                return newList;

            for (int i = 0; i < array.Length; i++)
                newList.Add(array[i]);

            return newList;

        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {

            foreach (T sourceItem in source)
                action(sourceItem);

        }

        public static void AddRange<T, S>(this ICollection<T> list, params S[] values) where S : T {
            foreach (S value in values)
                list.Add(value);
        }

        #endregion

        #region Texture2D Extensions

        public static Texture2D ConvertToPreMultipliedAlpha(this Texture2D texture) {
            
            Color[] array = new Color[texture.Width * texture.Height];
            texture.GetData(array, 0, array.Length);
            
            for (int i = 0; i < array.Length; i++) {
                byte a = array[i].A;
                array[i] *= array[i].A / 255f;
                array[i].A = a;
            }
            
            texture.SetData(array, 0, array.Length);
            
            return texture;

        }

        public static Rectangle Frame(this Texture2D tex, int horizontalFrames = 1, int verticalFrames = 1) => new Rectangle(0, 0, tex.Width / horizontalFrames, tex.Height / verticalFrames);

        public static Vector2 Size(this Texture2D texture) => new Vector2(texture.Width, texture.Height);

        #endregion

        #region Tile Extensions

        public static bool IsNotActive(this Tile tile) => !tile.active();

        public static bool IsNotSolid(this Tile tile) => !Main.tileSolid[tile.type];

        public static bool IsPlatform(this Tile tile) => Main.tileSolidTop[tile.type];

        public static bool IsSolid(this Tile tile) => Main.tileSolid[tile.type];

        #endregion

        #region Vector2 Extensions

        public static Vector2 Floor(this Vector2 vec) => new Vector2((int)vec.X, (int)vec.Y);

        public static bool HasNaNs(this Vector2 vec) => float.IsNaN(vec.X) || float.IsNaN(vec.Y);

        public static Vector2 RotateRandom(this Vector2 spinninpoint, double maxRadians) => spinninpoint.RotateRandom(Main.rand.NextDouble() * maxRadians - Main.rand.NextDouble() * maxRadians);

        public static Point ToPoint(this Vector2 vec) => new Point((int)vec.X, (int)vec.Y);

        public static Point ToTileCoordinates(this Vector2 vec) => new Point((int)vec.X / 16, (int)vec.Y / 16);

        #endregion 

        public enum Includes {
            Neither,
            Lower,
            Upper,
            Both
        }

        private static void CreateThePixel(GraphicsResource spriteBatch) {

            _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);

            _pixel.SetData(new[] { Color.White });
            
        }

    }

}
