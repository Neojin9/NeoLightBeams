using System;
using Terraria.ModLoader;

namespace NeoLightBeams.Projectiles {

    public class RainbowBeamProj2 : ModProjectile {

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

            if (projectile.localAI[0] == 0f) {

                if (projectile.velocity.X > 0f) {
                    projectile.spriteDirection = -1;
                    projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) - 1.57f;
                }
                else {
                    projectile.spriteDirection = 1;
                    projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) - 1.57f;
                }

                projectile.localAI[0] = 1f;

            }

            projectile.velocity.X = projectile.velocity.X * 0.98f;
            projectile.velocity.Y = projectile.velocity.Y * 0.98f;

        }

    }

}
