using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace TestContent.NPCs.KiryuPNG.Projectiles
{
    public class Crate : KiryuItemProjectile
    {
        public override void SetProjectileProperties()
        {
            projName = "Crate";
            goreAmount = 2;
            width = 62;
            height = 52;
            rotationalForce = (float)Math.PI / 8;
            effectDrawSize = 2.5f;
            deathSound = SoundID.Item51;
        }
    }
}
