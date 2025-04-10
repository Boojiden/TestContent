using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Utilities.Terraria.Utilities;
using Terraria.Utilities;
using TestContent.NPCs.Minos.Projectiles.Friendly;
using TestContent.Projectiles.Weapons.Gambling.Prehardmode;

namespace TestContent.NPCs.Minos.Projectiles
{
    public class JudgementExplosion : UltraExplosion
    {
        public override Color ExplosionColor => Color.Cyan;
        public override IntRange TrailNumRange => new IntRange(20, 25);
        public override Color TrailColor => base.TrailColor;
        public override FloatRange TrailLengthRange => new FloatRange(200f, 300f);
        public override FloatRange TrailOffsetRange => new FloatRange(Radius * 0.3f, Radius * 0.5f);

        public override float InnerExplosionBoost => 1.2f;
        public override int Radius
        {
            get
            {
                if (Main.expertMode)
                {
                    return 525;
                }
                return 450;
            }
        }
        public override int VisualRadiusExtension
        {
            get
            {
                if (Main.expertMode)
                {
                    return 1200;
                }
                return 900;
            }
        }
        public override int DustType1 => DustID.Wraith;
        public override int DustType2 => DustID.FireworkFountain_Blue;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = false;
            Projectile.hostile = true;
        }

        public override IntRange SmokeAmount => new IntRange(7, 9);
        public override FloatRange SmokeScale => new FloatRange(2f, 2.5f);

        public override FloatRange SmokeSpeed => new FloatRange(7f, 8f);

        public override Color SmokeColor => new Color(0.486f, 0.769f, 0.769f);
    }
}
