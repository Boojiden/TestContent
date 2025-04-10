using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.Utilities;
using Terraria.Utilities.Terraria.Utilities;
using TestContent.Projectiles.Weapons.Gambling.Prehardmode;

namespace TestContent.NPCs.Minos.Projectiles.Friendly
{
    public class FreezeFrameRocketExplosion : UltraExplosion
    {
        public override int Radius => 150;
        public override int VisualRadiusExtension => 300;

        public override Color ExplosionColor => Color.LightGoldenrodYellow * 0.75f;

        public override Color OuterExplosionColor => Color.White;
        public override int DustType1 => DustID.BubbleBurst_White;
        public override int DustType2 => DustID.PlatinumCoin;

        public override IntRange SmokeAmount => new IntRange(3, 5);
        public override FloatRange SmokeScale => new FloatRange(1f, 1.5f);

        public override FloatRange SmokeSpeed => new FloatRange(3f, 4f);

        public override Color SmokeColor => base.SmokeColor;
    }
}
