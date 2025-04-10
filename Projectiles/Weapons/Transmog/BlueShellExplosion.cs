using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.ID;
using TestContent.Projectiles.Weapons.Gambling.Prehardmode;

namespace TestContent.Projectiles.Weapons.Transmog
{
    public class BlueShellExplosion : DeathCoinExplosion
    {
        public override Color ExplosionColor => Color.Blue;

        public override int DustType1 => DustID.Wraith;

        public override int DustType2 => DustID.FireworkFountain_Blue;
    }
}
