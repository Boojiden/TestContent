using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;

namespace TestContent.NPCs.KiryuPNG.Projectiles
{
    public class FistUser : KiryuItemProjectile
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
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.ownerHitCheck = true;
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

        public override bool PreDraw(ref Color lightColor)
        {
            var ballTexture = TextureAssets.Extra[91].Value;
            float scale = (62f / (float)ballTexture.Height) * effectDrawSize;
            var rect = new Rectangle(0, 0, ballTexture.Width, ballTexture.Height);
            Vector2 origin = rect.Size() / 2;
            origin.Y -= rect.Height / 4;
            Color col = KiryuPNG.stateInfos[currentState].stateColor;
            col.A = 30;
            col *= Projectile.Opacity;

            var pos = Projectile.Center;// + new Vector2(0, -50f);

            Main.spriteBatch.Draw(ballTexture, pos - Main.screenPosition, rect, col,
                Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin, scale, SpriteEffects.None, 0f);

            for (int i = 0; i < 3; i++)
            {
                float time = (float)(Main.timeForVisualEffects + (15 * i)) % 30 / 30f;
                float newScale = scale * time;
                float newOpacity = Projectile.Opacity * (1f - time);
                col = (col * 8) * newOpacity;
                Main.spriteBatch.Draw(ballTexture, pos - Main.screenPosition, rect, col,
                    Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin, newScale, SpriteEffects.None, 0f);
            }
            var text = TextureAssets.Projectile[Type].Value;
            rect = new Rectangle(0, 0, text.Width, text.Height);
            origin = rect.Size() / 2;
            var effects =SpriteEffects.None;
            if(Projectile.direction == -1)
            {
                effects = SpriteEffects.FlipVertically;
            }
            Main.EntitySpriteDraw(text, Projectile.Center - Main.screenPosition, rect, lightColor * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, effects);
            return false;
        }
    }
}
