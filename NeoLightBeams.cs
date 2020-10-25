using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace NeoLightBeams {

	public class NeoLightBeams : Mod {

        public const int MaxBeamLength = 3000;
        public const int OffscreenBlockRange = 25;

        public static bool ResetLitUpGlassList;

        public static int TotalBeamLength;

        public static Texture2D Beam;
        public static Texture2D Beam0;
        public static Texture2D Beam1;
        public static Texture2D Beam2;
        public static Texture2D Beam3;
        public static Texture2D Beam4;
        public static Texture2D BeamBounce45;
        public static Texture2D BeamBounce90;
        public static Texture2D BeamBounce90Rotated;
        public static Texture2D BeamCorner;
        public static Texture2D BeamDiagonal;
        public static Texture2D BeamDiagonal0;
        public static Texture2D BeamDiagonal1;
        public static Texture2D BeamDiagonal2;
        public static Texture2D BeamDiagonal3;
        public static Texture2D BeamDiagonal4;
        public static Texture2D BeamDiagonalStart;
        public static Texture2D BeamFull;
        public static Texture2D AngledLensOverlay;

        public static List<Vector2> HitWireList;
        public static List<Tuple<Point, Color>> LitUpGlassList2;

        public override void Load() {

            HitWireList = new List<Vector2>();
            LitUpGlassList2 = new List<Tuple<Point, Color>>();

            if (Main.dedServ)
                return;

            Beam                = ModContent.GetTexture("NeoLightBeams/Textures/Beam");
            Beam0               = ModContent.GetTexture("NeoLightBeams/Textures/Beam0");
            Beam1               = ModContent.GetTexture("NeoLightBeams/Textures/Beam1");
            Beam2               = ModContent.GetTexture("NeoLightBeams/Textures/Beam2");
            Beam3               = ModContent.GetTexture("NeoLightBeams/Textures/Beam3");
            Beam4               = ModContent.GetTexture("NeoLightBeams/Textures/Beam4");
            BeamBounce45        = ModContent.GetTexture("NeoLightBeams/Textures/BeamBounce45");
            BeamBounce90        = ModContent.GetTexture("NeoLightBeams/Textures/BeamBounce90");
            BeamBounce90Rotated = ModContent.GetTexture("NeoLightBeams/Textures/BeamBounce90Rotated");
            BeamCorner          = ModContent.GetTexture("NeoLightBeams/Textures/BeamCorner");
            BeamDiagonal        = ModContent.GetTexture("NeoLightBeams/Textures/BeamDiagonal");
            BeamDiagonal0       = ModContent.GetTexture("NeoLightBeams/Textures/BeamDiagonal0");
            BeamDiagonal1       = ModContent.GetTexture("NeoLightBeams/Textures/BeamDiagonal1");
            BeamDiagonal2       = ModContent.GetTexture("NeoLightBeams/Textures/BeamDiagonal2");
            BeamDiagonal3       = ModContent.GetTexture("NeoLightBeams/Textures/BeamDiagonal3");
            BeamDiagonal4       = ModContent.GetTexture("NeoLightBeams/Textures/BeamDiagonal4");
            BeamDiagonalStart   = ModContent.GetTexture("NeoLightBeams/Textures/BeamDiagonalStart");
            BeamFull            = ModContent.GetTexture("NeoLightBeams/Textures/BeamFull");
            AngledLensOverlay   = ModContent.GetTexture("NeoLightBeams/Textures/FocusedLensAngledColor");

        }

        public override void PreUpdateEntities() {

            if (Main.gameMenu)
                return;

            TotalBeamLength = 0;

            if (ResetLitUpGlassList) {
                LitUpGlassList2.Clear();
                ResetLitUpGlassList = false;
            }

        }

    }
}