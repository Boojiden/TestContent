using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TestContent.Buffs.Minions;
using TestContent.Mounts;
using TestContent.NPCs.Minos.Projectiles.Friendly;
using TestContent.Projectiles.Weapons.Gambling.Prehardmode;
using TestContent.Utility;
using static TestContent.TestContent;

namespace TestContent.Players
{
    public class PlayerWeapons: ModPlayer
    {
        public int SignAltAmmo = 0;
        public const int SIGNMAXAMMO = 50;

        public int CardMinionCount = 0;
        public const int BustLimit = 21;

        public int railCannonCooldown = 0;
        public int RAILCANNONMAXCOOLDOWN = 15 * 60;

        public bool canUseRailCannon = true;

        public int inventoryValue = 0;

        private int valueCheckInterval = 60 * 5;
        private int currentValueTimer = 0;

        public int MAXFREEZETIMER = 60 * 10;
        public int freezeTimer = 60 * 10;
        public int FREEZETIMERRECHARGERATE = 2;

        public bool canFreeze = true;
        public bool rocketsFrozen = false;

        private static Dictionary<int, int> coinValues = new Dictionary<int, int>
        {
            {ItemID.CopperCoin, 1},
            {ItemID.SilverCoin, 100},
            {ItemID.GoldCoin, 10000},
            {ItemID.PlatinumCoin, 1000000},
        };

        public SoundStyle RailCannonRecharged => new SoundStyle(ModUtils.GetSoundFileLocation("RailcannonCharged"))
        {
            Volume = 0.4f,
            PlayOnlyIfFocused = true,
            PitchVariance = 0.5f,
            MaxInstances = 1
        };

        public SoundStyle FreezeStart => new SoundStyle(ModUtils.GetSoundFileLocation("FreezeStart"))
        {
            Volume = 0.4f,
            PlayOnlyIfFocused = true,
            PitchVariance = 0.5f,
            MaxInstances = 1
        };

        public SoundStyle FreezeStop => new SoundStyle(ModUtils.GetSoundFileLocation("FreezeStop"))
        {
            Volume = 0.4f,
            PlayOnlyIfFocused = true,
            PitchVariance = 0.5f,
            MaxInstances = 1
        };

        public override void OnEnterWorld()
        {
            inventoryValue = GetCoinValueInInventory(Player);
        }

        public override void OnRespawn()
        {
            inventoryValue = GetCoinValueInInventory(Player);
        }

        public override void PostUpdate()
        {
            if(++currentValueTimer >= valueCheckInterval)
            {
                inventoryValue = GetCoinValueInInventory(Player);
                currentValueTimer = 0;
            }
            if(railCannonCooldown > 0)
            {
                railCannonCooldown--;
                if(railCannonCooldown <= 0)
                {
                    canUseRailCannon = true;
                    if(Player.whoAmI == Main.myPlayer)
                    {
                        SoundEngine.PlaySound(RailCannonRecharged);
                    }
                }
            }
            if(rocketsFrozen)
            {
                freezeTimer--;
                if(freezeTimer <= 0)
                {
                    //Unfreeze all player rockets
                    UnfreezeRockets();
                    canFreeze = false;
                }
            }
            else
            {
                freezeTimer = Math.Clamp(freezeTimer + FREEZETIMERRECHARGERATE, 0, MAXFREEZETIMER);
            }
            if (!canFreeze)
            {
                freezeTimer += FREEZETIMERRECHARGERATE;
                if(freezeTimer >= MAXFREEZETIMER)
                {
                    canFreeze = true;
                    freezeTimer = MAXFREEZETIMER;
                }
            }
        }

        public void ToggleRockets()
        {
            if (canFreeze)
            {
                if (!rocketsFrozen)
                {
                    foreach (var proj in Main.ActiveProjectiles)
                    {
                        if (proj.type == ModContent.ProjectileType<FreezeFrameRocket>() && proj.owner == Player.whoAmI)
                        {
                            proj.ai[0] = 1;
                            proj.netUpdate = true;
                        }
                        
                    }
                    rocketsFrozen = true;
                    if (Player.whoAmI == Main.myPlayer)
                    {
                        SoundEngine.PlaySound(FreezeStart);
                    }
                }
                else
                {
                    foreach (var proj in Main.ActiveProjectiles)
                    {
                        if (proj.type == ModContent.ProjectileType<FreezeFrameRocket>() && proj.owner == Player.whoAmI)
                        {
                            proj.ai[0] = 0;
                            proj.netUpdate = true;
                        }

                    }
                    rocketsFrozen = false;
                    if (Player.whoAmI == Main.myPlayer)
                    {
                        SoundEngine.PlaySound(FreezeStop);
                    }
                }
            }

            if(Main.netMode == NetmodeID.Server || (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer))
            {
                ModPacket packet = Instance.GetPacket();
                packet.Write((byte)NetMessageType.ToggleRockets);
                packet.Write((byte)Player.whoAmI);
                packet.Send();
            }
        }

        private void UnfreezeRockets()
        {
            foreach (var proj in Main.ActiveProjectiles)
            {
                if (proj.type == ModContent.ProjectileType<FreezeFrameRocket>() && proj.owner == Player.whoAmI)
                {
                    proj.ai[0] = 0;
                    proj.netUpdate = true;
                }

            }
            rocketsFrozen = false;
            if(Player.whoAmI == Main.myPlayer)
            {
                SoundEngine.PlaySound(FreezeStop);
            }
        }

        public void UseRailCannon()
        {
            canUseRailCannon = false;
            railCannonCooldown = RAILCANNONMAXCOOLDOWN;
        }

        public bool UpdateCardMinionCounts()
        {
            int sum = 0;
            foreach(Projectile proj in Main.ActiveProjectiles)
            {
                if(proj.type == ModContent.ProjectileType<JackCardMinion>() && proj.owner == Main.myPlayer)
                {
                    JackCardMinion jack = proj.ModProjectile as JackCardMinion;
                    sum += GetCardValue(jack.MinionType);
                }
            }
            if (sum > BustLimit)
            {
                TriggerBust();
                CardMinionCount = 0;
                return true;
            }
            else
            {
                CardMinionCount = sum;
                return false;
            }
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
        {
            if (damageSource.SourceProjectileType == ModContent.ProjectileType<CardHostile>())
            {
                if (Main.projectile[damageSource.SourceProjectileLocalIndex].owner == Player.whoAmI)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(NetworkText.FromKey("Mods.TestContent.Misc.DeathReasons.Bust", [Player.name]));
                }
                else
                {
                    Player other = Main.player[Main.projectile[damageSource.SourceProjectileLocalIndex].owner];
                    damageSource = PlayerDeathReason.ByCustomReason(NetworkText.FromKey("Mods.TestContent.Misc.DeathReasons.BustNotOwner", [Player.name, other.name]));
                }
            }
            if(Player.mount.Type == ModContent.MountType<HorseMount>())
            {
                damageSource = PlayerDeathReason.ByCustomReason(NetworkText.FromKey("Mods.TestContent.Misc.DeathReasons.Horse", [Player.name]));
            }
            return true;
        }

        public int GetCardValue(int cardType)
        {
            switch (cardType)
            {
                case 0:
                    return 1;
                case 1:
                    return 2;
                case 2:
                    return 5;
                case 3:
                    return 10;
                default:
                    return 0;
            }
        }

        public void TriggerBust()
        {
            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.type == ModContent.ProjectileType<JackCardMinion>() && proj.owner == Main.myPlayer)
                {
                    float owner = proj.owner;
                    JackCardMinion jack = proj.ModProjectile as JackCardMinion;
                    Projectile.NewProjectile(proj.GetSource_FromThis(), proj.Center, Vector2.Zero,
                           ModContent.ProjectileType<CardHostile>(), proj.damage, proj.knockBack, ai1: jack.MinionType, ai0: proj.owner);
                    if (!Main.dedServ)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            var rand = Main.rand.NextFloat(10f, 25f);
                            var circRand = Main.rand.NextVector2Circular(rand, rand);
                            int dust = Dust.NewDust(proj.Center, 2, 2, DustID.Shadowflame, circRand.X, circRand.Y);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].scale = 2f;
                        }
                    }
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj.identity);
                    }
                    proj.Kill();
                }

            }
            SoundEngine.PlaySound(SoundID.Item122, Player.Center);
            Player.ClearBuff(ModContent.BuffType<JacksMinionBuff>());
            AdvancedPopupRequest popup = default;
            popup.Text = "Bust!";
            popup.Color = Color.Gray;
            popup.DurationInFrames = 120;
            popup.Velocity = -Vector2.UnitY * 3f;
            PopupText.NewText(popup, Player.position);
        }

        public void GrantSignAltAmmo(int amount)
        {
            int total = SignAltAmmo + amount;
            if(total >= SIGNMAXAMMO)
            {
                total = SIGNMAXAMMO;
            }
            SignAltAmmo = total;
        }

        public bool UseAltSignAmmo(int amount)
        {
            int total = SignAltAmmo - amount;
            if(total < 0)
            {
                return false;
            }
            else
            {
                SignAltAmmo = total;
                return true;
            }
        }
        /// <summary>
        /// Gets coin value from a player's inventory only (excluding piggy bank, safe, vault, etc.). Remember that a copper coin represents 1,
        /// so silver represents 100, gold represents 10,000, and platinum represents 1,000,000
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static int GetCoinValueInInventory(Player player)
        {
            var inv = player.inventory;

            int start = 0;
            int end = 53;

            int total = 0;

            for (int i = start; i <= end; i++)
            {
                var item = player.inventory[i];
                if (coinValues.ContainsKey(item.netID))
                {
                    total += coinValues[item.netID] * item.stack;
                }
            }

            return total;
        }

        
    }
}
