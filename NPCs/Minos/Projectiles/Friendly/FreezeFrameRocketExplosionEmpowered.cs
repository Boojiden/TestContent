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
    public class FreezeFrameRocketExplosionEmpowered : UltraExplosion
    {
        public override int Radius => 200;
        public override int VisualRadiusExtension => 400;

        public override Color ExplosionColor => Color.Red;
        public override int DustType1 => DustID.LifeDrain;
        public override int DustType2 => DustID.LavaMoss;

        public override IntRange SmokeAmount => new IntRange(5, 7);
        public override FloatRange SmokeScale => new FloatRange(1.5f, 2f);

        public override FloatRange SmokeSpeed => new FloatRange(5f, 6f);

        public override Color SmokeColor => base.SmokeColor;
    }
}
