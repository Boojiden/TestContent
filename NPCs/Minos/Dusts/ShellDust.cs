using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace TestContent.NPCs.Minos.Dusts
{
    public class ShellDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.scale = 1.2f;
            var tex = Texture2D;
            dust.frame = new Microsoft.Xna.Framework.Rectangle(0, 0, tex.Width(), tex.Height());
            dust.customData = Main.rand.NextFloat(-3f, 3f);
        }
        public override bool Update(Dust dust)
        {
            float rot = (float)dust.customData;
            dust.rotation += rot % 360;
            dust.velocity.Y += 1f;
            dust.position += dust.velocity;
            dust.scale *= 0.99f;
            if (dust.scale < 0.01f)
            {
                dust.active = false;
            }
            return false;

        }
    }
}
