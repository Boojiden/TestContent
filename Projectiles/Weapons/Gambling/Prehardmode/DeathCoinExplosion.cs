using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Stubble.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace TestContent.Projectiles.Weapons.Gambling.Prehardmode
{
    public class DeathCoinExplosion : ModProjectile
    {
        public override string Texture => "TestContent/ExtraTextures/InvisibleSprite";
        public int Timer
        {
            get => (int)Projectile.ai[0];
            set { Projectile.ai[0] = value; }
        }
        public virtual int Lifetime => 60;
        public int timerwhenHitStart = 5;
        public int timerwhenHitEnd = 15;

        public virtual Asset<Texture2D> MainAsset => ModContent.Request<Texture2D>("TestContent/Dusts/gfxCircle");

        public float Time => Timer / (float)Lifetime;

        public virtual Color ExplosionColor => Color.Magenta;
        public virtual int DustType1 => DustID.Shadowflame;
        public virtual int DustType2 => DustID.Firework_Pink;

        public virtual int Radius => 250;

        public virtual int VisualRadiusExtension => 74;

        public virtual bool DoDefaultDraw => true;
        public override void SetStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            Projectile.width = Radius;
            Projectile.height = Radius;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override bool? CanDamage()
        {
            return Timer >= timerwhenHitStart && Timer <= timerwhenHitEnd;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Main.expertMode)
            {
                if (target.type >= NPCID.EaterofWorldsHead && target.type <= NPCID.EaterofWorldsTail)
                {
                    modifiers.FinalDamage /= 5;
                }
            }
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            if (Timer == 0 && !Main.dedServ)
            {
                ExplosionEffects();
                for (int i = 0; i < 30; i++)
                {
                    var rand = Main.rand.NextFloat(10f, 25f);
                    var circRand = Main.rand.NextVector2Circular(rand, rand);
                    int dust = Dust.NewDust(Projectile.Center, 2, 2, DustType1, circRand.X, circRand.Y);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].scale = 2f;
                    rand = Main.rand.NextFloat(10f, 25f);
                    circRand = Main.rand.NextVector2Circular(rand, rand);
                    dust = Dust.NewDust(Projectile.Center, 2, 2, DustType2, circRand.X, circRand.Y);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].scale = 1f;
                }
            }
            Timer++;
        }

        public virtual void ExplosionEffects()
        {

        }
            

        public override bool PreDraw(ref Color lightColor)
        {
            var texture = MainAsset.Value;
            var rect = new Rectangle(0, 0, texture.Width, texture.Height);
            var origin = rect.Size() / 2;

            float scale = ExplosionLerp(Time) * ((Radius + (float)VisualRadiusExtension) / texture.Width);
            float alpha = FadeOutLerp(Time);

            Color col = ExplosionColor * alpha;

            //Main.NewText($"{time}, {scale}, {alpha}");
            //lightColor *= alpha;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.Transform);
            if (DoDefaultDraw)
            {
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, rect, col, 0f, origin, scale, SpriteEffects.None, 1f);
            }
            AdditionalDraws(lightColor, Time, scale, alpha);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.Transform);

            return false;
        }

        public virtual void AdditionalDraws(Color lightColor, float time, float origScale, float origAlpha)
        {
            
        }

        public float ExplosionLerp(float time)
        {
            if (time > 1f)
            {
                return 1f;
            }
            if (time < 0f)
            {
                return 0f;
            }
            return 1f - (float)Math.Pow(2, -10 * time);
        }

        public float FadeOutLerp(float time)
        {
            if (time > 1f)
            {
                return 1f;
            }
            if (time < 0f)
            {
                return 0f;
            }
            return 1 - (float)Math.Pow(2, 10 * time - 10);
        }

    }
}
