using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace TestContent.NPCs.KiryuPNG.Projectiles
{
    public class Bucket : KiryuItemProjectile
    {
        public override void SetProjectileProperties()
        {
            projName = "Bucket";
            goreAmount = 2;
            width = 50;
            height = 50;
            rotationalForce = (float)Math.PI / 8;
            effectDrawSize = 2.5f;
            deathSound = SoundID.Item51;
        }
    }
}
