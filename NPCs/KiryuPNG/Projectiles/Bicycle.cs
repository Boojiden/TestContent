using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace TestContent.NPCs.KiryuPNG.Projectiles
{
    public class Bicycle : KiryuItemProjectile
    {
        public override void SetProjectileProperties()
        {
            projName = "Bicycle";
            goreAmount = 4;
            width = 128;
            height = 128;
            rotationalForce = (float)Math.PI / 8;
            effectDrawSize = 1.5f;
            deathSound = SoundID.Item52;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var ballTexture = TextureAssets.Extra[91].Value;
            float scale = (286f / (float)ballTexture.Height) * effectDrawSize;
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
            Main.EntitySpriteDraw(text, Projectile.Center - Main.screenPosition, rect, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }


}
