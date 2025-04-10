using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TestContent.NPCs.Minos.Dusts;
using TestContent.Projectiles;
using TestContent.Utility;

namespace TestContent.NPCs.Minos.Projectiles.Friendly
{
    public class SkullBall : BasicTrailProjectile
    {
        protected override int trailLength => 5;
        protected override string trailTextureName => "";

        public int Timer
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public SoundStyle impact = new SoundStyle(ModUtils.GetSoundFileLocation("shootImpact"))
        {
            Volume = 0.15f,
            MaxInstances = 3,
            Pitch = 0f,
            PitchVariance = 0.25f,
            PlayOnlyIfFocused = true,
        };

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.height = 22;
            Projectile.width = 22;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.localNPCHitCooldown = 20;
            Projectile.timeLeft = 300;
            Projectile.scale = 0.75f;
        }
        public override void AI()
        {
            Timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();
            //Projectile.spriteDirection = Projectile.direction;
        }
        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);
            SoundEngine.PlaySound(impact, Projectile.Center);
            if (!Main.dedServ)
            {
                DustUtils.CreateDustBurst(ModContent.DustType<CircleParticle>(), Projectile.Center, 10, 12, 10);
            }
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            return Timer >= 10;
        }
        public override Color PrimitiveColorFunction(float completionRatio)
        {
            return Color.Lerp(Color.Transparent, Color.Red, 1f - completionRatio);
        }
        public override float PrimitiveWidthFunction(float completionRatio)
        {
            return MathHelper.Lerp(0f, ((Projectile.width / 2)) * Projectile.scale, 1f - completionRatio);
        }
        public override Vector2 PrimitiveOffsetFunction(float completionRatio)
        {
            return new Vector2(Projectile.width, Projectile.height) * 0.5f;// + (new Vector2(1f, 1f) * Projectile.direction);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var effects = Projectile.direction == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            TestContent.CenteredProjectileDraw(Projectile, Color.White, effects: effects);
            return false;
        }
    }
}
