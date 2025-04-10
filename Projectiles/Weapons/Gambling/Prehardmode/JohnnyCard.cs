using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace TestContent.Projectiles.Weapons.Gambling.Prehardmode
{
    public class JohnnyCard : ModProjectile
    {
        private bool freeze = false;
        public override void SetStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.width = 14;
            Projectile.height = 20;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            freeze = true;
            Projectile.velocity = Vector2.Zero;
            Projectile.timeLeft = 120;
            Projectile.alpha = 0;
            return false;
        }

        public override void AI()
        {
            if (Projectile.timeLeft <= 60)
            {
                Projectile.alpha = (int)MathHelper.Lerp(255, 0, Projectile.timeLeft / 60f);
            }
            if (freeze)
            {
                return;
            }
            if (Projectile.velocity.Length() > 3f)
            {
                Projectile.velocity *= 0.9f;
            }
            if (Projectile.velocity.Y < 10f)
            {
                Projectile.velocity.Y += 0.3f;
            }

            float sine = (float)Math.Sin(Projectile.timeLeft * 0.1f);
            Projectile.velocity.X += sine * 0.5f;
            Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2;
            //Projectile.spriteDirection = Projectile.direction;
        }
    }
}
