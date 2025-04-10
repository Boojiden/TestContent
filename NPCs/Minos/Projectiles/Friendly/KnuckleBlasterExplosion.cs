using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.Utilities.Terraria.Utilities;
using Terraria.Utilities;

namespace TestContent.NPCs.Minos.Projectiles.Friendly
{
    public class KnuckleBlasterExplosion : UltraExplosion
    {
        public override int Radius => 300;
        public override int VisualRadiusExtension => 150;

        public override Color ExplosionColor => Color.Orange * 0.75f;

        public override Color OuterExplosionColor => Color.White;
        public override int DustType1 => DustID.InfernoFork;
        public override int DustType2 => DustID.Asphalt;

        public override IntRange SmokeAmount => new IntRange(3, 5);
        public override FloatRange SmokeScale => new FloatRange(1f, 1.5f);

        public override FloatRange SmokeSpeed => new FloatRange(3f, 4f);

        public override Color SmokeColor => base.SmokeColor;
    }
}
