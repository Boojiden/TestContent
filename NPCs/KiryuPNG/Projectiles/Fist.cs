using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace TestContent.NPCs.KiryuPNG.Projectiles
{
    public class Fist : KiryuItemProjectile
    {
        public override void SetProjectileProperties()
        {
            width = 62;
            height = 64;
            projName = "Fist";
            goreAmount = 0;
            effectDrawSize = 3f;
            doDefaultBehavior = false;

            Projectile.tileCollide = false;
        }

        public override void AdditionalBehavior()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();// + (float)Math.PI/2;
            Projectile.velocity *= 0.95f;
            if(Projectile.velocity.Length() <= 3f)
            {
                Projectile.alpha += 12;
                //Main.NewText(Projectile.alpha);
                if(Projectile.alpha > 240)
                {
                    Projectile.Kill();
                }
            }
        }
    }
}
