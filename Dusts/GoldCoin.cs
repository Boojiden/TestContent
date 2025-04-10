using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TestContent.Dusts
{
    public class GoldCoin : ModDust
    {
        public Vector2 center;
        public override void OnSpawn(Dust dust)
        {
            var tex = Texture2D;
            dust.frame = new Rectangle(0, 0, tex.Width(), tex.Height());
        }

        public override bool Update(Dust dust)
        {
            center = dust.position - new Vector2(Texture2D.Width(), Texture2D.Height());
            dust.velocity.Y += 0.1f;
            if(Main.timeForVisualEffects % 10 == 0)
            {
                Dust newdust = Dust.NewDustDirect(center, 1, 1, DustID.GoldCoin, 0f, 0f, 1);
            }
            dust.position += dust.velocity;
            dust.alpha += 10;
            if (dust.alpha > 250)
            {
                dust.active = false;
            }
            return false;
        }
    }
}
