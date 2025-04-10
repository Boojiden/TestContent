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
    public class Smoke: ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            var tex = Texture2D;
            dust.frame = new Rectangle(0, 0, tex.Width(), tex.Height());
            dust.customData = Main.rand.NextFloat(-0.5f, 0.5f);
            //dust.alpha = Main.rand.Next(100, 130);
            //dust.scale = 0.5f;
        }
        public override bool Update(Dust dust)
        {
            float rot = (float)dust.customData;
            dust.rotation += rot % 360;
            dust.velocity.Y -= 0.1f;
            dust.position += dust.velocity;
            dust.alpha += 10;
            if (dust.alpha > 250)
            {
                dust.active = false;
            }
            return false;
            
        }

        public override bool PreDraw(Dust dust)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone);
            var text = Texture2D.Value;
            var origin = new Vector2((float)text.Width / 2, (float)text.Height / 2);
            var color = Color.Gray;
            color = color.MultiplyRGB(dust.color);
            color.A = (byte)(255 - dust.alpha);
            Main.spriteBatch.Draw(Texture2D.Value, dust.position - Main.screenPosition, dust.frame, color, dust.rotation, origin, dust.scale, SpriteEffects.None, 1f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin();

            return false;
        }
    }
}
