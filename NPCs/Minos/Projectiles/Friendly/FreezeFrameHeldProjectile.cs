using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ModLoader;
using TestContent.Players;
using TestContent.Projectiles;
using TestContent.Utility;

namespace TestContent.NPCs.Minos.Projectiles.Friendly
{
    public class FreezeFrameHeldProjectile : BasicHeldGunProjectile
    {
        public override int PULLOUTTIME => 0;
        public override int BaseFireDelay => 40;
        public override int BulletType => ModContent.ProjectileType<FreezeFrameRocket>();
        public override float ShootSpeed => 33f;
        public override float DegreesRecoil => 12f;
        public override SoundStyle? Sound => new SoundStyle(ModUtils.GetSoundFileLocation("FreezeFrameShoot"))
        {
            Volume = 0.4f,
            PlayOnlyIfFocused = true,
            PitchVariance = 0.5f
        };

        public override void GunAI()
        {
            if(Projectile.owner == Main.myPlayer)
            {
                if (PlayerInput.Triggers.JustPressed.MouseRight)
                {
                    Owner.GetModPlayer<PlayerWeapons>().ToggleRockets();
                }
            }
        }
        public override bool CanShoot(out int type)
        {
            type = BulletType;
            return Owner.PickAmmo(Owner.HeldItem, out int proj, out float speed, out int damage, out float knockBack, out int ammo, false);
        }
    }
}
