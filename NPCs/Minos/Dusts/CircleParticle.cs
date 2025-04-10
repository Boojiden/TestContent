using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace TestContent.NPCs.Minos.Dusts
{
    public class CircleParticle : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.color = Color.Red;
            dust.scale = 0.75f + 0.25f * Main.rand.NextFloat();
            var tex = Texture2D;
            dust.frame = new Microsoft.Xna.Framework.Rectangle(0, 0, tex.Width(), tex.Height());
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.9f;
            dust.scale -= 0.05f;
            if(dust.scale <= 0.1f)
            {
                dust.active = false;
            }
            return false;
        }
    }
}
