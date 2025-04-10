using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace TestContent.NPCs.KiryuPNG.Projectiles
{
    public class TrafficCone : KiryuItemProjectile
    {
        public override void SetProjectileProperties()
        {
            projName = "TrafficCone";
            goreAmount = 2;
            width = 54;
            height = 102;
            rotationalForce = (float)Math.PI / 8;
            effectDrawSize = 3f;
            deathSound = SoundID.Item51;
        }
    }
}
