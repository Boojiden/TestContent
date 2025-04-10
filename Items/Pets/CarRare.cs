using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TestContent.Dusts;
using Terraria.ID;

namespace TestContent.Items.Pets
{
    public class CarRare : ModProjectile
    {
        //TODO: Other projectiles spawn just fine, something is causing this one to not show up. What could it be?
        public float speed = 10f;
        public Player target;
        public static SoundStyle Horn,Kaboom;
        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 68;
            Projectile.alpha = 0;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.aiStyle = -1;
            Projectile.extraUpdates = 0;
            Horn = new SoundStyle("TestContent/Assets/Sounds/Horn")
            {
                Volume = 0.8f
            };
            Kaboom = new SoundStyle("TestContent/Assets/Sounds/Kaboom")
            {
                Volume = 0.8f
            };
        }
        public override void OnSpawn(IEntitySource source) //This doesn't run on servers. I need a better solution
        {
            
        }
        public override void AI() 
        {
            if (target == null || target.dead || !target.active) //Potential checking a null here
            {
                target = null;
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (player.active && !player.dead)
                    {
                        target = player;
                        break;
                    }
                }
                if(target == null)
                {
                    Projectile.Kill();
                    return;
                }
            }

            Projectile.timeLeft = 2;
            Vector2 playerpos = target.Center;
            Vector2 dir = (playerpos - Projectile.Center);
            dir.Normalize();
            Projectile.velocity = dir * speed;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(Kaboom, Projectile.position);
            Dust.NewDustDirect(Projectile.position, 1, 1, ModContent.DustType<Explosion>(),0,0,0).scale = 3f;
        }
    }
}
