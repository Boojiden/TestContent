using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using TestContent.Projectiles;
using TestContent.Utility;

namespace TestContent.NPCs.Minos.Projectiles
{
    public class SoulOrbParticle : BasicTrailProjectile
    {
        protected override string trailTextureName => "";

        public int lifeTime = 120;

        public Vector2 initPos;

        public NPC target
        {
            get
            {
                return Main.npc[(int)Projectile.ai[0]];
            }
            set
            {
                Projectile.ai[0] = value.whoAmI;
            }
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.height = 8;
            Projectile.width = 8;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = lifeTime;
        }

        public override bool? CanDamage()
        {
            return false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            initPos = Projectile.Center;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            float time = GameplayUtils.GetTimeFromInts(Projectile.timeLeft, lifeTime);
            Projectile.Opacity = time;
            Projectile.Center = Vector2.Lerp(initPos, target.Center, 1f - time);
        }

        public override Color PrimitiveColorFunction(float completionRatio)
        {
            return Color.Lerp(Color.Transparent, Color.Cyan, 1f - completionRatio) * Projectile.Opacity;
        }
        public override float PrimitiveWidthFunction(float completionRatio)
        {
            return MathHelper.Lerp(4f, 0f, completionRatio);
        }
    }
}
