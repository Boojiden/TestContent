using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using rail;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Items;
using TestContent.Players;
using TestContent.Projectiles;
using TestContent.Utility;

namespace TestContent.NPCs.Minos.Projectiles.Friendly
{
    public class RailCannonHeldProjectile : BasicHeldGunProjectile
    {
        public override int PULLOUTTIME => 0;
        public override int BaseFireDelay => 30;
        public override int BulletType => ModContent.ProjectileType<RailCannonShot>();
        public override float ShootSpeed => 0f;
        public override float DegreesRecoil => 40f;
        public override bool CheckActive => false;

        public override int DustType1 => DustID.FireworkFountain_Blue;
        public override int DustType2 => DustID.Electric;

        public override SoundStyle? Sound => new SoundStyle(ModUtils.GetSoundFileLocation("RailcannonFire"))
        {
            Volume = 0.4f,
            PlayOnlyIfFocused = true,
            PitchVariance = 0.5f
        };

        public override bool CanShoot(out int type)
        {
            type = BulletType;
            return Owner.GetModPlayer<PlayerWeapons>().canUseRailCannon;
        }

        public override void SpawnProjectile(Vector2 origin, Vector2 dir)
        {
            shootPos = origin + dir * (BULLETOFFSET);
            var end = GetBeamEndPosition(origin, dir);
            Terraria.Projectile.NewProjectile(Terraria.Entity.InheritSource(Entity), shootPos, dir * ShootSpeed, BulletType, Projectile.damage, Projectile.knockBack, ai0: end.X, ai1: end.Y);
            Owner.GetModPlayer<PlayerWeapons>().UseRailCannon();
            SoundEngine.PlaySound(Sound, Projectile.Center);
        }

        public Vector2 GetBeamEndPosition(Vector2 origin, Vector2 dir)
        {
            Vector2 samplingPoint = Projectile.Center;

            // Overriding that, if the player shoves the Prism into or through a wall, the interpolation starts at the player's center.
            // This last part prevents the player from projecting beams through walls under any circumstances.
            Player player = Main.player[Projectile.owner];
            if (!Collision.CanHitLine(player.Center, 0, 0, Projectile.Center, 0, 0))
            {
                samplingPoint = player.Center;
            }

            // Perform a laser scan to calculate the correct length of the beam.
            // Alternatively, if you want the beam to ignore tiles, just set it to be the max beam length with the following line.
            // return MaxBeamLength;
            int samples = 10;
            float[] laserScanResults = new float[samples];
            Collision.LaserScan(samplingPoint, dir, 0 * Projectile.scale, 1000f, laserScanResults);
            float averageLengthSample = 0f;
            for (int i = 0; i < laserScanResults.Length; ++i)
            {
                averageLengthSample += laserScanResults[i];
            }
            averageLengthSample /= samples;

            return origin + (dir * averageLengthSample);
        }
    }
}
