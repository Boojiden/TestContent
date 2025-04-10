using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Utility;
using TextContent.Effects.Graphics.Primitives;

namespace TestContent.NPCs.Minos.Projectiles.Friendly
{
    public class RailCannonShot : ModProjectile, IPixelatedPrimitiveRenderer
    {
        public override string Texture => "TestContent/ExtraTextures/InvisibleSprite";
        public Asset<Texture2D> electricTexture;
        public Vector2 End
        {
            get
            {
                return new Vector2(Projectile.ai[0], Projectile.ai[1]);
            }
            set
            {
                Projectile.ai[0] = value.X; Projectile.ai[1] = value.Y; 
            }
        }

        public PrimitiveSettings settings;
        public PrimitiveSettings settingsOutline;
        public PrimitiveSettings settingsElectric;
        public int lifeTime = 30;
        public float beamWidth = 5f;
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 2;
            Projectile.timeLeft = lifeTime;
            electricTexture = ModContent.Request<Texture2D>("TestContent/NPCs/Minos/Extras/ElectricTrail");
            if(Main.netMode != NetmodeID.Server)
            {
                settings = new PrimitiveSettings(BeamWidth, BeamColor, pixelate: true);
                settingsOutline = new PrimitiveSettings(BeamWidthOutline, BeamColorOutline, pixelate: true);
                settingsElectric = new PrimitiveSettings((ratio) => BeamWidthOutline(ratio) * 5f, (_) => Color.White, pixelate: true, shader: GameShaders.Misc["TestContent:TrailDirect"]);
            }
        }

        public float BeamWidth(float compRatio)
        {
            if(compRatio > 0.9f)
            {
                return 0f;
            }
            return MathHelper.Hermite(0.5f, 4.1f, 0f, 0f, 1f - GameplayUtils.GetTimeFromInts(Projectile.timeLeft, lifeTime)) * beamWidth;
        }

        public Color BeamColor(float compRatio)
        {
            return Color.Cyan;
        }

        public float BeamWidthOutline(float compRatio)
        {
            return BeamWidth(compRatio) * 1.5f;
        }

        public Color BeamColorOutline(float compRatio)
        {
            return Color.Lerp(Color.Cyan, Color.White, 0.5f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, End, beamWidth, ref collisionPoint);
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;

            if (!Main.dedServ)
            {
                Dust.NewDustPerfect(End, DustID.Electric, Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(5f, 15f));
            }
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            var pointList = new List<Vector2> { Projectile.Center };
            for (int i = 0; i < 21; i++)
            {
                float time = (float)i / 20f;
                pointList.Add(Vector2.Lerp(Projectile.Center, End, time));
            }
            pointList.Add(End);
            PrimitiveRenderer.RenderTrail(pointList, settingsOutline);
            PrimitiveRenderer.RenderTrail(pointList, settings);
            settingsElectric.Shader.SetShaderTexture(electricTexture);
            settingsElectric.Shader.SetTrailTextureWidth((Projectile.Center - End).Length());
            PrimitiveRenderer.RenderTrail(pointList, settingsElectric);
        }
    }
}
