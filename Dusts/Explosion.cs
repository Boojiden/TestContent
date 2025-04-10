using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace TestContent.Dusts
{
    public class Explosion : ModDust
    {
        public static int rows = 6;
        public static int cols = 3;

        public int lifetime = 120;
        public override void OnSpawn(Dust dust)
        {
            dust.customData = 0;
        }
        public override bool Update(Dust dust)
        {
            dust.customData = (int)dust.customData+1;
            int currentFrame = (int)(((float)((int)dust.customData) / (float)lifetime) * 18f);

            Texture2D texture = Texture2D.Value;
            int height = texture.Height / cols;
            int width = texture.Width / rows;

            Rectangle source = new Rectangle((currentFrame % 6) * width, (currentFrame / 6) * height, width, height);

            dust.frame = source;

            if (currentFrame > 18) 
            {
                dust.active = false;
            }
            return false;
        }
        public override bool PreDraw(Dust dust)
        {
            Texture2D texture = Texture2D.Value;
            int height = texture.Height / cols;
            int width = texture.Width / rows;

            Rectangle source = dust.frame;
            Vector2 origin = source.Size() / 2f;
            //Main.NewText(currentFrame + " " + Texture);
            Main.spriteBatch.Draw(Texture2D.Value, dust.position - Main.screenPosition, source, Color.White, dust.rotation, origin, dust.scale, SpriteEffects.None, 0f);
            //Main.EntitySpriteDraw(texture, dust.position, source, Color.White, 0, origin, 1, SpriteEffects.None);
            return false;
        }
    }
}
