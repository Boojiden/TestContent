using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using TestContent.Dusts;

namespace TestContent.Projectiles.Weapons
{
    public class SmokingGunBlunt : ModProjectile
    {

        public int SmokeTimer
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16; // The width of projectile hitbox
            Projectile.height = 16; // The height of projectile hitbox
            Projectile.friendly = true; // Can the projectile deal damage to enemies?
            Projectile.hostile = false; // Can the projectile deal damage to the player?
            Projectile.DamageType = DamageClass.Ranged; // Is the projectile shoot by a ranged weapon?
            Projectile.penetrate = 3; // How many monsters the projectile can penetrate. (OnTileCollide below also decrements penetrate for bounces as well)
            Projectile.timeLeft = 600; // The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
            Projectile.alpha = 0; // The transparency of the projectile, 255 for completely transparent. (aiStyle 1 quickly fades the projectile in) Make sure to delete this if you aren't using an aiStyle that fades in. You'll wonder why your projectile is invisible.
            //Projectile.light = 0.25f;
            Projectile.ignoreWater = true; // Does the projectile's speed be influenced by water?
            Projectile.tileCollide = true; // Can the projectile collide with tiles?
            Projectile.extraUpdates = 1; // Set to above 0 if you want the projectile to update multiple time in a frame
            // Act exactly like default Bullet
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
        }

        public override bool PreAI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Vector3 color = Color.OrangeRed.ToVector3();
            Lighting.AddLight(Projectile.Center, color);
            if (!Main.dedServ)
            {
                SmokeTimer--;
                if (SmokeTimer < 0)
                {
                    Dust.NewDust(Projectile.Center, 0, 0, ModContent.DustType<Smoke>(), 0, 0, newColor: Color.White, Alpha: 130, Scale: 0.5f);
                    SmokeTimer = 0;
                }
            }
            return false;
        }
    }
}
