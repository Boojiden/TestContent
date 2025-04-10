using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.Utilities.Terraria.Utilities;
using TestContent.Dusts;
using TestContent.Projectiles.Weapons.Gambling.Prehardmode;
using TestContent.Utility;
using TextContent.Effects.Graphics.Primitives;

namespace TestContent.NPCs.Minos.Projectiles.Friendly
{
    public abstract class UltraExplosion : DeathCoinExplosion, IPixelatedPrimitiveRenderer
    {

        public override bool DoDefaultDraw => false;
        public virtual float OuterScale => 2f;
        public virtual float OuterAlpha => 0.3f;

        public virtual float InnerExplosionBoost => 1.4f;
        public virtual Color OuterExplosionColor => ExplosionColor;
        public virtual Color TrailColor => ExplosionColor;
        public virtual float TrailWidth => 3f;
        public virtual float DoubleTime => Math.Clamp(GameplayUtils.GetTimeFromInts(Timer, Lifetime / 2), 0f, 1f);

        public virtual IntRange TrailNumRange => new IntRange(5, 10);
        public virtual FloatRange TrailLengthRange => new FloatRange(100f, 300f);
        public virtual FloatRange TrailOffsetRange => new FloatRange(30f, 70f);

        public virtual IntRange SmokeAmount => new IntRange(5, 20);
        public virtual FloatRange SmokeScale => new FloatRange(1f, 1.5f);
        public virtual FloatRange SmokeSpeed => new FloatRange(5f, 10f);

        public virtual Color SmokeColor => ExplosionColor;

        public float randomRot = 0;
        public float randomAddedRot = 0;

        public virtual FloatRange RotationRange => new FloatRange(MathHelper.Pi / 4, MathHelper.Pi);

        public Asset<Texture2D> Explosion;
        public Asset<Texture2D> Shockwave;

        public PrimitiveSettings settings;

        public int trails = 0;
        /// <summary>
        /// Trail length, Trail Offset, Trail Rotation
        /// </summary>
        public List<(float, float, float)> trailInfo = new List<(float, float, float)>();

        public override void SetDefaults()
        {
            base.SetDefaults();
            settings = new PrimitiveSettings(GetTrailWidth, GetTrailColor, pixelate:true);
            Explosion = ModContent.Request<Texture2D>("TestContent/Dusts/expCircle");
            Shockwave = ModContent.Request<Texture2D>("TestContent/Dusts/expCircleShockwave");
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            randomRot = Main.rand.NextFloat(0f, 2f * MathHelper.Pi);
            randomAddedRot = Main.rand.NextFloat(RotationRange) * Main.rand.Next(0, 2);
        }

        public override void ExplosionEffects()
        {
            trails = Main.rand.Next(TrailNumRange.Minimum, TrailNumRange.Maximum + 1);
            FloatRange rotRange = new FloatRange(0f, MathHelper.TwoPi / (float)trails);
            for(int i = 0; i < trails; i++)
            {
                trailInfo.Add((Main.rand.NextFloat(TrailLengthRange), Main.rand.NextFloat(TrailOffsetRange), (rotRange.Maximum * i) + (rotRange.Maximum * Main.rand.NextFloat())));
            }

            int smoke = Main.rand.Next(SmokeAmount);
            DustUtils.CreateDustBurstCircle(ModContent.DustType<Smoke>(), Projectile.Center, SmokeSpeed.Minimum, SmokeSpeed.Maximum, smoke, SmokeColor, Main.rand.NextFloat(SmokeScale));
        }
        public virtual float GetTrailWidth(float compRatio)
        {
            return (1 - compRatio) * TrailWidth;
        }

        public virtual Color GetTrailColor(float compRatio)
        {
            float epicenter = DoubleTime;
            float colorEffect = 1f - Math.Abs((epicenter * 2) - compRatio);
            return (TrailColor * colorEffect);
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            for(int i = 0; i < trailInfo.Count; i++)
            {
                var rotVector = trailInfo[i].Item3.ToRotationVector2();
                Vector2 start = Projectile.Center + rotVector * trailInfo[i].Item2;
                Vector2 end = start + rotVector * trailInfo[i].Item1 * MathHelper.Hermite(0f, 3f, 1f, 0f, DoubleTime);

                var posList = new List<Vector2>();
                for(int j = 0; j < 10; j++)
                {
                    posList.Add(Vector2.Lerp(start, end, GameplayUtils.GetTimeFromInts(j, 10)));
                }
                posList.Add(end);
                PrimitiveRenderer.RenderTrail(posList, settings);
            }
        }

        public override void AdditionalDraws(Color lightColor, float time, float origScale, float origAlpha)
        {
            var texture = Explosion.Value;
            var rect = new Rectangle(0, 0, texture.Width, texture.Height);
            var origin = rect.Size() / 2;

            float scale = (DoubleTime * ((Radius + (float)VisualRadiusExtension) / texture.Width)) * OuterScale;
            float alpha = FadeOutLerp(DoubleTime) * OuterAlpha;
            float innerAlpha = FadeOutLerp(DoubleTime) * InnerExplosionBoost;

            float rot = MathHelper.Lerp(randomRot, randomRot + randomAddedRot, ExplosionLerp(time));

            Color col = ExplosionColor * innerAlpha;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, rect, col, rot, origin, origScale, SpriteEffects.None, 1f);

            texture = Shockwave.Value;
            rect = new Rectangle(0, 0, texture.Width, texture.Height);
            origin = rect.Size() / 2;

            col = OuterExplosionColor * alpha;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, rect, col, randomRot, origin, scale, SpriteEffects.None, 1f);
        }
    }
}
