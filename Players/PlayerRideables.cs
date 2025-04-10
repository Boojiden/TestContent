using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Utility;

namespace TestContent.Players
{
    public class PlayerRideables : ModPlayer
    {
        public static ModKeybind MountTrigger {  get; private set; }

        public int ridingProjectileId = -1;

        public bool currentRideDebuffed = false;

        public event Action RideDebuffed;
        public event Action RideDismounted;

        public int currentRocketTime = 8 * 60;
        public int maxRocketTime = 8 * 60;
        public int rocketTimeDebuffStacks = 1;

        public int rocketModifierDecreaseRate = 15;

        public int debuffTimer = 0;

        public bool canRideRocket = true;
        public int rideCooldownTimer = 0;
        public int rideCooldownTimerMax = 30;

        public override void Load()
        {
            MountTrigger = KeybindLoader.RegisterKeybind(Mod, "Mount Rocket", Microsoft.Xna.Framework.Input.Keys.LeftControl);
        }

        public override void Unload()
        {
            MountTrigger = null;
        }

        public bool TrySetRidingProjectile(Projectile proj)
        {
            if (ridingProjectileId == -1 && canRideRocket)
            {
                ridingProjectileId = proj.identity;
                canRideRocket = false;
                rideCooldownTimer = rideCooldownTimerMax;
                if(Main.netMode != NetmodeID.SinglePlayer)
                {
                    SyncPlayer(-1, Main.myPlayer, false);
                }
                return true;
            }
            return false;
        }

        public void UnsetRidingProjectile()
        {
            ridingProjectileId = -1;
            var invokes = RideDebuffed?.GetInvocationList();
            if(invokes != null)
            {
                foreach (var d in invokes)
                {
                    RideDebuffed -= (Action)d;
                }
            }
            RideDismounted?.Invoke();
            if (Player.whoAmI == Main.myPlayer && Main.netMode != NetmodeID.SinglePlayer)
            {
                SyncPlayer(-1, Main.myPlayer, false);
            }
        }

        public override void PostUpdate() 
        {
            if (ridingProjectileId != -1)
            {
                Projectile? ride = null;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    var projectile = Main.projectile[i];
                    if (projectile.identity == ridingProjectileId && projectile.active)
                    {
                        ride = projectile;
                    }
                }
                if (ride == null)
                {
                    UnsetRidingProjectile();
                    return;
                }
                Player.SetFeetPosition(ride.Center);
                Player.wingTime = 0f;
                Player.dashDelay = 2;
                float degrees = MathHelper.ToDegrees(ride.rotation);
                int dir = degrees < 90 && degrees > -90 ? 1 : -1;
                Player.direction = dir == 0 ? 1 : dir;
                if (!currentRideDebuffed)
                {
                    currentRocketTime--;
                }
                if(currentRocketTime < 0)
                {
                    currentRideDebuffed = true;
                    RideDebuffed?.Invoke();
                    rocketTimeDebuffStacks = Math.Clamp(rocketTimeDebuffStacks + 1, 1, 15);
                    currentRocketTime = maxRocketTime / rocketTimeDebuffStacks;
                }
                if (Main.myPlayer == Player.whoAmI && PlayerInput.Triggers.JustPressed.Jump)
                {
                    UnsetRidingProjectile();
                }
            }
            else
            {
                currentRocketTime = maxRocketTime / rocketTimeDebuffStacks;
                currentRideDebuffed = false;
                if (rocketTimeDebuffStacks > 1)
                {
                    debuffTimer++;
                    if (debuffTimer > rocketModifierDecreaseRate)
                    {
                        rocketTimeDebuffStacks--;
                        debuffTimer = 0;
                    }
                }
                if(!canRideRocket)
                {
                    rideCooldownTimer--;
                    if(rideCooldownTimer <= 0)
                    {
                        canRideRocket = true;
                    }
                }
            }
        }

        public void SyncProjectile(int id)
        {
            ridingProjectileId = id;
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)TestContent.NetMessageType.SyncRidable);
            packet.Write((byte)Player.whoAmI);
            packet.Write(ridingProjectileId);
            packet.Send();
        }

        public override void CopyClientState(ModPlayer targetCopy)
        {
            var clone = (PlayerRideables)targetCopy;
            clone.ridingProjectileId = ridingProjectileId;
        }

        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            var client = (PlayerRideables)clientPlayer;
            if(client.ridingProjectileId != ridingProjectileId)
            {
                SyncPlayer(-1, Main.myPlayer, false);
            }
        }
    }
}
