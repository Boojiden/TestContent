using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Utility;

namespace TestContent.NPCs.Minos.Projectiles
{
    public class MinosSlamHitbox : ModProjectile
    {

        public override string Texture => "TestContent/ExtraTextures/InvisibleSprite";
        public Vector2 endPos
        {
            get { return new Vector2(Projectile.ai[0], Projectile.ai[1]); }
            set { Projectile.ai[0] = value.X; Projectile.ai[1] = value.Y; }
        }
        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(48);
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.timeLeft = 15;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            var collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, endPos, Projectile.width, ref collisionPoint);
        }

        public override void AI()
        {
            if (!Main.dedServ)
            {
                DustUtils.CreateDustLine(DustID.Wraith, Projectile.Center, endPos);
                DustUtils.CreateDustLine(DustID.FireworkFountain_Blue, Projectile.Center, endPos);
            }
        }
    }
}
