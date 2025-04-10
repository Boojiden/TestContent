using Microsoft.Xna.Framework;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.Utilities;
using Terraria.Utilities.Terraria.Utilities;
using TestContent.NPCs.Minos.Projectiles.Friendly;

namespace TestContent.NPCs.Minos.Projectiles
{
    public class MinosIntroExplosionEffect : UltraExplosion
    {
        public override int Radius => 500;
        public override int Lifetime => 300;
        public override int DustType1 => DustID.BubbleBurst_White;
        public override int DustType2 => DustID.PlatinumCoin;
        public override Color ExplosionColor => Color.White;
        public override Color OuterExplosionColor => Color.White * 0.75f;
        public override IntRange TrailNumRange => new IntRange(20,25);
        public override Color TrailColor => base.TrailColor;
        public override FloatRange TrailLengthRange => new FloatRange(200f, 300f);
        public override FloatRange TrailOffsetRange => new FloatRange(0f, 0f);

        public override IntRange SmokeAmount => new IntRange(0,0);

        public override bool? CanDamage()
        {
            return false;
        }
    }
}
