using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using System.IO;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using TestContent.Items.Gag;

namespace TestContent.NPCs
{
    public class Pingas: ModNPC
    {

        public int laserTimer;
        public int laserTimerMax = 300;
        public int laserTimerMin = 240;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1; // make sure to set this for your modnpcs.

            // Specify the debuffs it is immune to.
            // This NPC will be immune to the Poisoned debuff.
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                PortraitPositionYOverride = 60f,
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }

        public override void SetDefaults()
        {
            NPC.width = 128; // The width of the npc's hitbox (in pixels)
            NPC.height = 128; // The height of the npc's hitbox (in pixels)
            NPC.aiStyle = NPCAIStyleID.Bat; // This npc has a completely unique AI, so we set this to -1. The default aiStyle 0 will face the player, which might conflict with custom AI code.
            NPC.damage = 34; // The amount of damage that this npc deals
            NPC.defense = 10; // The amount of defense that this npc has
            NPC.lifeMax = 3400; // The amount of health that this npc has
            NPC.HitSound = SoundID.NPCHit1; // The sound the NPC will make when being hit.
            NPC.DeathSound = SoundID.NPCDeath1; // The sound the NPC will make when it dies.
            NPC.value = 2500f; // How many copper coins the NPC will drop when killed.
            NPC.knockBackResist = 0f;
            NPC.boss = true;
            if(Main.netMode != NetmodeID.Server)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/Robotnick");
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(laserTimer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            laserTimer = reader.ReadInt32();
        }

        public override void OnSpawn(IEntitySource source)
        {
            laserTimer = Main.rand.Next(laserTimerMin,laserTimerMax);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if(spawnInfo.Player.ZoneSkyHeight && Main.hardMode)
            {
                return 0.05f;
            }
            else
            {
                return 0f;
            }
        }

        public override bool PreKill()
        {
            NPC.boss = false;
            return true;
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            base.BossLoot(ref name, ref potionType);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            // Sets the description of this NPC that is listed in the bestiary
            bestiaryEntry.UIInfoProvider = new CommonEnemyUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[NPC.type], quickUnlock: true);
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(), // Plain black background
				new FlavorTextBestiaryInfoElement("Oh No! The Evil Pingas Man has invaded the world of Terraria! What will the Terrarian do?????")
            });
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PingasNoiseMaker>(), 5));
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }

            laserTimer--;
            if(laserTimer < 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Player player = Main.player[NPC.target];
                    int shots = Main.rand.Next(3, 6);
                    for(int i = 0; i < shots; i++)
                    {
                        var dir = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                        dir = dir.RotatedByRandom(Math.PI / 16);
                        var vel = dir * 40f;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel, ProjectileID.SaucerLaser, NPC.damage, 1f);
                    }
                    laserTimer = Main.rand.Next(laserTimerMin, laserTimerMax);
                    NPC.netUpdate = true;
                    SoundEngine.PlaySound(PingasNoiseMaker.pingas, NPC.Center);
                }
                else if(Main.netMode == NetmodeID.MultiplayerClient) 
                {
                    SoundEngine.PlaySound(PingasNoiseMaker.pingas, NPC.Center);
                    laserTimer = laserTimerMax;
                }
            }
        }
    }
}
