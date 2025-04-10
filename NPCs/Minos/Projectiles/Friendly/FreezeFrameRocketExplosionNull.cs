using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.Utilities.Terraria.Utilities;
using Terraria.Utilities;
using TestContent.Projectiles.Weapons.Gambling.Prehardmode;

namespace TestContent.NPCs.Minos.Projectiles.Friendly
{
    public class FreezeFrameRocketExplosionNull : UltraExplosion
    {
        public override int Radius => 50;
        public override int VisualRadiusExtension => 150;

        public override Color ExplosionColor => Color.White * 0.5f;
        public override int DustType1 => DustID.BubbleBurst_White;
        public override int DustType2 => DustID.PlatinumCoin;

        public override IntRange SmokeAmount => new IntRange(2, 3);
        public override FloatRange SmokeScale => new FloatRange(0.6f, 0.7f);

        public override FloatRange SmokeSpeed => new FloatRange(2f, 3f);

        public override Color SmokeColor => base.SmokeColor;
    }
}
