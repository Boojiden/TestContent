using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TestContent.Utility;

namespace TestContent.UI.BossBars
{
    public class UltraBossBar : ModBossBar
    {
        public Asset<Texture2D> barFrame;
        public Asset<Texture2D> barRed;
        public Asset<Texture2D> barOrange;

        public float prevHealth = 1f;
        public int delayTimer = 0;
        public int delayTimerMax = 300;

        public bool isCatchingUp = false;
        public float catchupRate = 0.001f;
        public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame)
        {
            return ModContent.Request<Texture2D>("TestContent/ExtraTextures/InvisibleSprite");
        }

        public override void Load()
        {
            var t = typeof(UltraBossBar);
            var prefix = ModUtils.GetNamespaceFileLocation(t)+"/";
            barFrame = ModContent.Request<Texture2D>(prefix+"MinosPrimeBossBarFrame");
            barRed = ModContent.Request<Texture2D>(prefix + "MinosPrimeBar");
            barOrange = ModContent.Request<Texture2D>(prefix + "MinosPrimeBarDelay");
        }

        public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
        {
            float lifePercent = drawParams.Life / drawParams.LifeMax;
            Vector2 offset = new Vector2(0f, 0f);

            //Main.NewText($"Debug Boss Bar: delay time: {delayTimer}, delayTimeMax: {delayTimerMax}, prevHealth: {prevHealth}, isCatchingUp:{isCatchingUp}");
            //Draw bar frame
            var barFrameRect = barFrame.Value.Bounds;
            var barFrameOrigin = barFrameRect.Center.ToVector2();
            spriteBatch.Draw(barFrame.Value, drawParams.BarCenter, barFrameRect, Color.White, 0f, barFrameOrigin, 1f, SpriteEffects.None, 0f);

            //Draw Orange Frame
            if(lifePercent != prevHealth)
            {
                if(!isCatchingUp && delayTimer < delayTimerMax)
                {
                    delayTimer++;
                    if(delayTimer >= delayTimerMax) 
                    {
                        isCatchingUp = true;
                    }
                }
                else if(isCatchingUp)
                {
                    delayTimer = 0;
                    prevHealth -= catchupRate;
                    if(prevHealth <= lifePercent)
                    {
                        prevHealth = lifePercent;
                        isCatchingUp = false;
                    }
                }
            }

            var barOrangeRect = barOrange.Value.Bounds;
            var barOrangeOrigin = barOrangeRect.Center.ToVector2();
            barOrangeRect.Width = (int)((float)barOrangeRect.Width * prevHealth);
            //Main.NewText(barOrangeRect.Width);
            spriteBatch.Draw(barOrange.Value, drawParams.BarCenter + offset, barOrangeRect, Color.White, 0f, barOrangeOrigin, 1f, SpriteEffects.None, 0f);

            var barRedRect = barRed.Value.Bounds;
            var barRedOrigin = barRedRect.Center.ToVector2();
            barRedRect.Width = (int)((float)barRedRect.Width * lifePercent);
            //Main.NewText(barRedRect.Width);
            spriteBatch.Draw(barRed.Value, drawParams.BarCenter + offset, barRedRect, Color.White, 0f, barRedOrigin, 1f, SpriteEffects.None, 0f);

            spriteBatch.DrawString(TestContent.compFont.Value, new StringBuilder(npc.FullName), drawParams.BarCenter, Color.White, 0f, TestContent.compFont.Value.MeasureString(npc.FullName) / 2, 1f, SpriteEffects.None, 0f);
            return false;
        }
    }
}
