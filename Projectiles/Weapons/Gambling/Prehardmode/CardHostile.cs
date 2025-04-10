using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace TestContent.Projectiles.Weapons.Gambling.Prehardmode
{
    public class CardHostile : ModProjectile
    {
        public int CardType
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        public int Owner
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public int Timer
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        public int chargeUpTime = 60;
        public bool charging = false;

        public override void SetStaticDefaults()
        {
            // Sets the amount of frames this minion has on its spritesheet
            Main.projFrames[Projectile.type] = 4;

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5; // The length of old position to be recorded
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 26;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
        }
        public override void AI()
        {
            float chargeSpeed = 20f;
            Player owner = Main.player[Owner];
            Timer++;
            if (!charging)
            {
                Projectile.timeLeft = 2;
                var dir = Projectile.Center - owner.Center;
                Projectile.rotation = dir.ToRotation() + (float)Math.PI / 2;
                if (Timer >= chargeUpTime)
                {
                    charging = true;
                    Timer = 0;
                    Projectile.timeLeft = 120;
                }
            }
            else
            {
                var rot = Projectile.rotation;
                rot += (float)Math.PI / 2;
                Projectile.velocity = rot.ToRotationVector2() * chargeSpeed;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Projectile.frame = CardType;
            var rect = new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Type], texture.Width, texture.Height / Main.projFrames[Type]);
            Vector2 origin = rect.Size() / 2;

            lightColor = lightColor.MultiplyRGB(new Color(242, 196, 255));

            Color col = Color.DarkBlue;
            col.A = 55;

            if (charging)
            {
                int iterations = 0;
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] != Vector2.Zero)
                    {
                        iterations++;
                    }
                }

                for (int i = 0; i < iterations; i++)
                {
                    col = Color.Purple;
                    col *= 0.25f - 0.05f * i;
                    Main.spriteBatch.Draw(texture, Projectile.oldPos[i] - Main.screenPosition + origin, rect, col, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
                }
            }
            return true;
        }
    }
}
