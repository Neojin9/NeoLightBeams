using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;


namespace NeoDraw.Core {

    public class Drawing {

        #region Constants

        public const string REGEX_RESET = "#;";
        public const string REGEX_COLOR1 = "#([0-9a-f]);";
        public const string REGEX_COLOR3 = "#([0-9a-f]{3});";
        public const string REGEX_COLOR6 = "#([0-9a-f]{6});";
        public const string REGEX_COLOR_BRIGHTER = "#>([0-9]{1,3});";
        public const string REGEX_COLOR_DARKER = "#<([0-9]{1,3});";
        public const string REGEX_COLOR_ADD = "#\\+([0-9a-f]{1,2});";
        public const string REGEX_COLOR_SUB = "#\\-([0-9a-f]{1,2});";
        public const string REGEX_COLOR_INVERT = "#\\^;";
        public const string REGEX_COLOR_SWITCH = "#([RGB]{3});";
        public const string REGEX_SCALE = "#%([0-9]{1,3});";

        #endregion

        #region Readonly

        public static readonly string BUILT_REGEX = "(#[^#]*?;)";
        public static readonly int[][] shadowOffset;

        #endregion

        #region Variables

        public static Texture2D box;

        #endregion

        static Drawing() {
            // Note: this type is marked as 'beforefieldinit'.
            
            shadowOffset = new[] {
                new[] { -1, 0 },
                new[] { 1, 0 },
                new[] { 0, -1 },
                new[] { 0, 1 }
            };

            box = Main.inventoryBackTexture;

        }

        #region Public Functions

        public static void StringShadowed(SpriteBatch sb, DynamicSpriteFont font, string text, Vector2 pos) {
            StringShadowed(sb, font, text, pos, Color.White);
        }

        public static void StringShadowed(SpriteBatch sb, DynamicSpriteFont font, string text, Vector2 pos, Color c, float scale = 1f, Vector2 origin = default(Vector2)) {
            DrawStringShadow(sb, font, text, pos, new Color(0, 0, 0, c.A), 0f, origin, scale);
            sb.DrawString(font, text, pos, c, 0f, origin, scale, SpriteEffects.None, 0f);
        }

        public static string ToColorCode(Color c) => "#" + c.ToHex(false).ToLower() + ";";

        public static Vector2 DrawColorCodedString(SpriteBatch sb, DynamicSpriteFont font, string text, Vector2 pos, float maxW = -1f) {
            return DrawColorCodedString(sb, font, text, pos, Color.White, 0f, default(Vector2), 1f, maxW);
        }

        public static Vector2 DrawColorCodedString(SpriteBatch sb, DynamicSpriteFont font, string text, Vector2 pos, Color baseColor, float rotation = 0f, Vector2 origin = default(Vector2), float scale = 1f, float maxW = -1f) {
            return DrawColorCodedString(sb, font, text, pos, baseColor, rotation, origin, new Vector2(scale, scale), maxW);
        }

        public static Vector2 DrawColorCodedString(SpriteBatch sb, DynamicSpriteFont font, string text, Vector2 pos, Color baseColor, float rotation, Vector2 origin, Vector2 baseScale, float maxW = -1f, bool ignoreColors = false) {
            
            Vector2 vector = new Vector2(pos.X, pos.Y);
            Vector2 result = vector;
            string[] array = text.Split(new[] {'\n'});
            float x = font.MeasureString(" ").X;
            Color color = Color.White;
            float num = 1f;
            float num2 = 0f;
            string[] array2 = array;
            
            foreach (string[] array4 in array2.Select(input => Regex.Split(input, BUILT_REGEX)).Select(array3 => array3)) {
                
                foreach (string text2 in array4) {
                    
                    if (Regex.Match(text2, BUILT_REGEX).Success) {
                        
                        Match match = Regex.Match(text2, "#;");
                        
                        if (match.Success) {
                            
                            num = 1f;
                            
                            if (!ignoreColors)
                                color = Color.White;

                        } else {
                            
                            match = Regex.Match(text2, "#([0-9a-f]);");
                            
                            if (match.Success) {
                                
                                if (!ignoreColors) {
                                    float num3 = int.Parse(match.Groups[1].Value, NumberStyles.HexNumber) / 15f;
                                    color = new Color(num3, num3, num3, color.A / 255f);
                                }

                            } else {
                                
                                match = Regex.Match(text2, "#([0-9a-f]{3});");
                                
                                if (match.Success) {
                                    
                                    if (!ignoreColors) {
                                        string value = match.Groups[1].Value;
                                        float r = int.Parse(string.Concat(value.ElementAt(0)), NumberStyles.HexNumber) / 15f;
                                        float g = int.Parse(string.Concat(value.ElementAt(1)), NumberStyles.HexNumber) / 15f;
                                        float b = int.Parse(string.Concat(value.ElementAt(2)), NumberStyles.HexNumber) / 15f;
                                        color = new Color(r, g, b, color.A / 255f);
                                    }

                                } else {
                                    
                                    match = Regex.Match(text2, "#([0-9a-f]{6});");
                                    
                                    if (match.Success) {
                                        
                                        if (!ignoreColors) {
                                            string value2 = match.Groups[1].Value;
                                            float r2 = int.Parse(value2.Substring(0, 2), NumberStyles.HexNumber) / 255f;
                                            float g2 = int.Parse(value2.Substring(2, 2), NumberStyles.HexNumber) / 255f;
                                            float b2 = int.Parse(value2.Substring(4, 2), NumberStyles.HexNumber) / 255f;
                                            color = new Color(r2, g2, b2, color.A / 255f);
                                        }

                                    } else {
                                        
                                        match = Regex.Match(text2, "#>([0-9]{1,3});");
                                        
                                        if (match.Success) {
                                            
                                            if (!ignoreColors) {
                                                int num4 = int.Parse(match.Groups[1].Value);
                                                color = new Color(color.R / 255f * (1f + num4 * 0.01f), color.G / 255f * (1f + num4 * 0.01f), color.B / 255f * (1f + num4 * 0.01f), color.A / 255f);
                                            }

                                        } else {
                                            
                                            match = Regex.Match(text2, "#<([0-9]{1,3});");
                                            
                                            if (match.Success) {
                                                
                                                if (!ignoreColors) {
                                                    int num5 = int.Parse(match.Groups[1].Value);
                                                    color = new Color(color.R / 255f * (1f - num5 * 0.01f), color.G / 255f * (1f - num5 * 0.01f), color.B / 255f * (1f - num5 * 0.01f), color.A / 255f);
                                                }

                                            } else {
                                                
                                                match = Regex.Match(text2, "#\\+([0-9a-f]{1,2});");
                                                
                                                if (match.Success) {
                                                    
                                                    if (!ignoreColors) {
                                                        int num6 = int.Parse(match.Groups[1].Value, NumberStyles.HexNumber);
                                                        color = new Color(((byte)Math.Min(color.R + num6, 255)), ((byte)Math.Min(color.G + num6, 255)), ((byte)Math.Min(color.B + num6, 255)), color.A);
                                                    }

                                                } else {
                                                    
                                                    match = Regex.Match(text2, "#\\-([0-9a-f]{1,2});");
                                                    
                                                    if (match.Success) {
                                                        
                                                        if (!ignoreColors) {
                                                            int num7 = int.Parse(match.Groups[1].Value, NumberStyles.HexNumber);
                                                            color = new Color(((byte)Math.Max(color.R - num7, 0)), ((byte)Math.Max(color.G - num7, 0)), ((byte)Math.Max(color.B - num7, 0)), color.A);
                                                        }

                                                    } else {
                                                        
                                                        match = Regex.Match(text2, "#\\^;");
                                                        
                                                        if (match.Success) {
                                                            
                                                            if (!ignoreColors)
                                                                color = new Color((255 - color.R), (255 - color.G), (255 - color.B), color.A);

                                                        } else {
                                                            
                                                            match = Regex.Match(text2, "#([RGB]{3});");
                                                            
                                                            if (match.Success) {
                                                                
                                                                if (!ignoreColors) {
                                                                    string value3 = match.Groups[1].Value;
                                                                    int r3 = ((value3.ElementAt(0) == 'R') ? color.R : ((value3.ElementAt(0) == 'G') ? color.G : color.B));
                                                                    int g3 = ((value3.ElementAt(1) == 'R') ? color.R : ((value3.ElementAt(1) == 'G') ? color.G : color.B));
                                                                    int b3 = ((value3.ElementAt(2) == 'R') ? color.R : ((value3.ElementAt(2) == 'G') ? color.G : color.B));
                                                                    color = new Color(r3, g3, b3, color.A);
                                                                }

                                                            } else {
                                                                
                                                                match = Regex.Match(text2, "#%([0-9]{1,3});");
                                                                
                                                                if (match.Success)
                                                                    num = int.Parse(match.Groups[1].Value) * 0.01f;

                                                            }

                                                        }

                                                    }

                                                }

                                            }

                                        }

                                    }

                                }

                            }

                        }

                    } else {
                        
                        string[] array5 = text2.Split(new[] {' '});

                        for (int k = 0; k < array5.Length; k++) {
                            
                            if (k != 0)
                                vector.X += x * baseScale.X * num;
                            
                            if (maxW > 0f) {
                                
                                float num8 = font.MeasureString(array5[k]).X * baseScale.X * num;
                                
                                if (vector.X - pos.X + num8 > maxW) {
                                    vector.X = pos.X;
                                    vector.Y += font.LineSpacing * num2 * baseScale.Y;
                                    result.Y = Math.Max(result.Y, vector.Y);
                                    num2 = 0f;
                                }

                            }

                            if (num2 < num)
                                num2 = num;
                            
                            sb.DrawString(font, array5[k], vector, color.Multiply(baseColor), rotation, origin, baseScale * num, SpriteEffects.None, 0f);
                            vector.X += font.MeasureString(array5[k]).X * baseScale.X * num;
                            result.X = Math.Max(result.X, vector.X);

                        }

                    }

                }

                vector.X = pos.X;
                vector.Y += font.LineSpacing * num2 * baseScale.Y;
                result.Y = Math.Max(result.Y, vector.Y);
                num2 = 0f;

            }

            return result;

        }

        public static void DrawColorCodedStringShadow(SpriteBatch sb, DynamicSpriteFont font, string text, Vector2 pos, float maxW = -1f, int offset = 1) {
            DrawColorCodedStringShadow(sb, font, text, pos, new Color(0f, 0f, 0f, 0.5f), 0f, default(Vector2), 1f, maxW, offset);
        }

        public static void DrawColorCodedStringShadow(SpriteBatch sb, DynamicSpriteFont font, string text, Vector2 pos, Color color, float rotation = 0f, Vector2 origin = default(Vector2), float scale = 1f, float maxW = -1f, int offset = 1) {
            DrawColorCodedStringShadow(sb, font, text, pos, color, rotation, origin, new Vector2(scale, scale), maxW, offset);
        }

        public static void DrawColorCodedStringShadow(SpriteBatch sb, DynamicSpriteFont font, string text, Vector2 pos, Color color, float rotation, Vector2 origin, Vector2 scale, float maxW = -1f, int offset = 1) {
            
            color = new Color(color.R, color.G, color.B, ((byte)(Math.Pow((color.A / 255f), 2.0) * 255.0)));
            
            foreach (int[] t in shadowOffset)
                DrawColorCodedString(sb, font, text, new Vector2(pos.X + (t[0] * offset), pos.Y + (t[1] * offset)), color, rotation, origin, scale, maxW, true);

        }

        public static string DropColorCodes(string text) {
            
            string[] array = Regex.Split(text, BUILT_REGEX);
            StringBuilder stringBuilder = new StringBuilder();
            string[] array2 = array;
            
            foreach (string text2 in array2.Where(text2 => !Regex.Match(text2, BUILT_REGEX).Success))
                stringBuilder.Append(text2);
            
            return stringBuilder.ToString();

        }

        public static Vector2 MeasureColorCodedString(DynamicSpriteFont font, string text) => font.MeasureString(DropColorCodes(text));

        public static void DrawBox(SpriteBatch SP, float x, float y, float w, float h, Color c) {
            DrawBox(SP, (int)x, (int)y, (int)w, (int)h, c);
        }

        public static void DrawBox(SpriteBatch SP, float x, float y, float w, float h, float a = 0.785f) {
            DrawBox(SP, (int)x, (int)y, (int)w, (int)h, new Color(63, 65, 151) * a);
        }

        public static void DrawBox(SpriteBatch SP, int x, int y, int w, int h, Color c) {
            
            Texture2D texture2D = box;
            
            if (w < 20)
                w = 20;
            
            if (h < 20)
                h = 20;
            
            SP.Draw(texture2D, new Rectangle(x, y, 10, 10), new Rectangle(0, 0, 10, 10), c);
            SP.Draw(texture2D, new Rectangle(x + 10, y, w - 20, 10), new Rectangle(10, 0, 10, 10), c);
            SP.Draw(texture2D, new Rectangle(x + w - 10, y, 10, 10), new Rectangle(texture2D.Width - 10, 0, 10, 10), c);
            SP.Draw(texture2D, new Rectangle(x, y + 10, 10, h - 20), new Rectangle(0, 10, 10, 10), c);
            SP.Draw(texture2D, new Rectangle(x + 10, y + 10, w - 20, h - 20), new Rectangle(10, 10, 10, 10), c);
            SP.Draw(texture2D, new Rectangle(x + w - 10, y + 10, 10, h - 20), new Rectangle(texture2D.Width - 10, 10, 10, 10), c);
            SP.Draw(texture2D, new Rectangle(x, y + h - 10, 10, 10), new Rectangle(0, texture2D.Height - 10, 10, 10), c);
            SP.Draw(texture2D, new Rectangle(x + 10, y + h - 10, w - 20, 10), new Rectangle(10, texture2D.Height - 10, 10, 10), c);
            SP.Draw(texture2D, new Rectangle(x + w - 10, y + h - 10, 10, 10), new Rectangle(texture2D.Width - 10, texture2D.Height - 10, 10, 10), c);

        }

        public static Vector2 DString(SpriteBatch SP, string se, Vector2 ve, Color ce, float fe = 1f, float rex = 0f, float rey = 0f, int font = 0) {
            
            DynamicSpriteFont spriteFont = Main.fontMouseText;
            
            if (font == 1)
                spriteFont = Main.fontDeathText;
            
            for (int i = -1; i < 2; i++) {
                for (int j = -1; j < 2; j++)
                    SP.DrawString(spriteFont, se, ve + new Vector2(i, j) * 1f, Color.Black, 0f, new Vector2(rex, rey) * spriteFont.MeasureString(se), fe, SpriteEffects.None, 0f);
            }
            
            SP.DrawString(spriteFont, se, ve, ce, 0f, new Vector2(rex, rey) * spriteFont.MeasureString(se), fe, SpriteEffects.None, 0f);
            
            return spriteFont.MeasureString(se) * fe;

        }

        public static void DrawStringShadow(SpriteBatch sb, DynamicSpriteFont font, string text, Vector2 pos, int offset = 1) {
            DrawStringShadow(sb, font, text, pos, new Color(0f, 0f, 0f, 0.5f), 0f, default(Vector2), 1f, offset);
        }

        public static void DrawStringShadow(SpriteBatch sb, DynamicSpriteFont font, string text, Vector2 pos, Color color, float rotation = 0f, Vector2 origin = default(Vector2), float scale = 1f, int offset = 1) {
            DrawStringShadow(sb, font, text, pos, color, rotation, origin, new Vector2(scale, scale), offset);
        }

        public static void DrawStringShadow(SpriteBatch sb, DynamicSpriteFont font, string text, Vector2 pos, Color color, float rotation, Vector2 origin, Vector2 scale, int offset = 1) {
            
            color = new Color(color.R, color.G, color.B, ((byte)(Math.Pow((color.A / 255f), 2.0) * 255.0)));
            
            foreach (int[] t in shadowOffset)
                sb.DrawString(font, text, new Vector2(pos.X + (t[0] * offset), pos.Y + (t[1] * offset)), color, rotation, origin, scale, SpriteEffects.None, 0f);

        }

        public static void DrawPoints(SpriteBatch spriteBatch, Vector2 position, IList<Vector2> points, Color color, float thickness) {

            if (points.Count < 2)
                return;

            for (int i = 1; i < points.Count; i++)
                spriteBatch.DrawLine(points[i - 1] + position, points[i] + position, color, thickness);

        }

        #endregion

    }

}
