using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace TestContent.Dusts
{
    public class PlatCoin : ModDust
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
            if (Main.timeForVisualEffects % 30 == 0)
            {
                Dust newdust = Dust.NewDustDirect(center, 1, 1, DustID.PlatinumCoin, 0f, 0f, 1);
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
