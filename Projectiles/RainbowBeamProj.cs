using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace NeoLightBeams.Projectiles {

    public class RainbowBeamProj : ModProjectile {

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Rainbow Beam Projectile");
        }

		public override void SetDefaults() {

			projectile.aiStyle = -1;
			projectile.alpha = 235;
			projectile.damage = 0;
			projectile.friendly = true;
			projectile.height = 2;
			projectile.ignoreWater = true;
			projectile.light = 0.3f;
			projectile.MaxUpdates = 4;
			projectile.penetrate = -1;
			projectile.tileCollide = true;
			projectile.timeLeft = 90;
			projectile.width = 2;

		}

        public override void AI() {

            projectile.scale = 0.5f;
            
            Projectile.NewProjectile(projectile.Center.X, projectile.Center.Y, projectile.velocity.X * 0.001f, projectile.velocity.Y * 0.001f, ProjectileType<RainbowBeamProj2>(), projectile.damage, projectile.knockBack, projectile.owner, 0f, 0f);

            projectile.ai[0]++;

            float num542 = projectile.velocity.X;
            float num543 = projectile.velocity.Y;
            float num544 = (float)Math.Sqrt(num542 * num542 + num543 * num543);

            num544 = 15.95f * projectile.scale / num544;
            num542 *= num544;
            num543 *= num544;

            projectile.velocity.X = num542;
            projectile.velocity.Y = num543;
            projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) - 1.57f;

        }

    }

}
