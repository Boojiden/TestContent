using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Utility;
using TextContent.Effects.Graphics.Primitives;

namespace TestContent.Projectiles
{
    public abstract class BasicTrailProjectile : ModProjectile, IPixelatedPrimitiveRenderer
    {
        protected virtual int trailLength => 15;
        protected int trailType = 0;

        protected PrimitiveSettings trailSettings;

        protected abstract string trailTextureName { get; }

        private Asset<Texture2D> trailTexture;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = trailType;
            ProjectileID.Sets.TrailCacheLength[Type] = trailLength;
        }

        public override void SetDefaults()
        {
            if(Main.netMode != NetmodeID.Server)
            {
                trailSettings = SetPrimitiveSettings();
            }
            if(!trailTextureName.Equals(""))
            {
                trailTexture = ModContent.Request<Texture2D>(trailTextureName);
            }
        }

        public virtual PrimitiveSettings SetPrimitiveSettings()
        {
            return new PrimitiveSettings(PrimitiveWidthFunction, PrimitiveColorFunction, PrimitiveOffsetFunction, pixelate: true);
        }

        public virtual float PrimitiveWidthFunction(float completionRatio)
        {
            return completionRatio * Projectile.scale;
        }

        public virtual Color PrimitiveColorFunction(float completionRatio) 
        {
            return Color.Lerp(Color.Red, Color.Blue, completionRatio);
        } 

        public virtual Vector2 PrimitiveOffsetFunction(float  completionRatio)
        {
            return new Vector2(Projectile.width / 2, Projectile.height/2);
        }


        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            if(trailSettings.Shader != null)
            {
                if(trailTexture != null)
                {
                    trailSettings.Shader.SetShaderTexture(trailTexture);
                }
                trailSettings.Shader.SetTrailTextureWidth(trailLength);
            }
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, trailSettings);
        }
    }
}
