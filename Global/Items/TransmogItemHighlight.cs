using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using TestContent.Items.Transmog;
using TestContent.Players;

namespace TestContent.Global.Items
{
    public class TransmogItemHighlight : GlobalItem
    {
        public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (Main.mouseItem.netID == ModContent.ItemType<TransmogItem>() && PlayerTransmog.transmogs.ContainsKey(item.netID))
            {
                //Main.NewText("Drawin' " + item.netID);
                var pixel = TestContent.noise.Value;
                var rect = new Rectangle(0, 0, pixel.Width, pixel.Height);
                origin = rect.Size() / 2;
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
                TestContent.TransmogEffect.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly);
                //Main.NewText(Main.GlobalTimeWrappedHourly);
                TestContent.TransmogEffect.CurrentTechnique.Passes[0].Apply();
                spriteBatch.Draw(pixel, position, rect, Color.White, 0f, origin, 1f, SpriteEffects.None, 0f);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
            }
            return base.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        }
    }
}
