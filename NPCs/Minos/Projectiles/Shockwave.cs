using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Dusts;
using TestContent.Utility;

namespace TestContent.NPCs.Minos.Projectiles
{
    public class Shockwave : ModProjectile
    {
        public float heightScale = 0f;
        public int shockwaveTime = (int)(0.5f * 60f);

        public bool spawnedDust = false;

        //Rectangle DebugRect = new Rectangle(0, 0, 0, 0);
        public int Timer
        {
            get
            {
                return (int)Projectile.ai[0];
            }
            set
            {
                Projectile.ai[0] = (int)value;
            }
        }

        public override void SetDefaults()
        {
            Projectile.width = 276;
            Projectile.height = 64;
            Projectile.scale = 3f;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.damage = 10;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.Center -= new Vector2(0, (Projectile.height) / 2f);
            Projectile.netUpdate = true;
        }

        public override void AI()
        {
            if(Main.netMode != NetmodeID.Server && !spawnedDust)
            {
                DustUtils.CreateDustLine(ModContent.DustType<Smoke>(),
                Projectile.position + new Vector2(0f, (float)Projectile.height),
                Projectile.position + new Vector2((float)Projectile.width,
                (float)Projectile.height), density: 10,
                col: Color.White * 0.5f, scale: 1f);
                spawnedDust = true;
            }
            float time = (float)Timer / (float)shockwaveTime;
            heightScale = MathHelper.Hermite(0f, 6.7f, 0f, 0f, time);
            if(Timer >= shockwaveTime) 
            {
                Projectile.Kill();
            }
            int dir = Math.Sign(Projectile.velocity.X);
            if(dir == 0)
            {
                dir = 1;
            }
            Projectile.spriteDirection = dir;
            Timer++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            var rect = new Rectangle(projHitbox.Left, projHitbox.Top + ((Projectile.height * 3) / 4), (int)(Projectile.width), (int)(Projectile.height / 4));
            //DebugRect = rect;
            //Dust.DrawDebugBox(rect);

            return Collision.CheckAABBvAABBCollision(rect.TopLeft(), rect.Size(), targetHitbox.TopLeft(), targetHitbox.Size());
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var texture = ModContent.Request<Texture2D>(Texture).Value;
            var rect = new Rectangle(0, 0, Projectile.width, Projectile.height);
            Vector2 RectSize = rect.Size();
            var origin = new Vector2(0, RectSize.Y/Projectile.scale);

            var drawOffset = new Vector2(0, Projectile.scale * heightScale);

            var effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //var rect = new Rectangle(projHitbox.Left, projHitbox.Top, (int)(Projectile.width), (int)(Projectile.height * heightScale))
            //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, Projectile.position - Main.screenPosition, new Rectangle(0, 0 ,1 ,1), Color.Wheat, 0, Vector2.Zero, DebugRect.Size(), SpriteEffects.None, 0f);
            Main.EntitySpriteDraw(texture, Projectile.position + (origin * Projectile.scale) - Main.screenPosition, null, Color.White, 0f, origin, new Vector2(Projectile.scale, Projectile.scale * heightScale), effects);
            
            return false;
        }
    }
}
