using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Bestiary;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;
using TestContent.Dusts;
using TestContent.NPCs.KiryuPNG.Projectiles;
using Terraria.GameContent;
using TestContent.UI;
using Terraria.GameContent.ItemDropRules;
using TestContent.NPCs.KiryuPNG.Items;
using Terraria.GameContent.Creative;
using Terraria.Localization;
using Terraria.Audio;
using TestContent.Projectiles.Weapons.Gambling.Prehardmode;

namespace TestContent.NPCs.KiryuPNG
{
    [AutoloadBossHead]
    public class KiryuPNG : ModNPC
    {
        private static Asset<Texture2D> texture;
        private static Asset<Texture2D> circleTexture;
        private static List<Asset<Texture2D>> deathFrames;
        private static string filepath = "TestContent/NPCs/KiryuPNG";


        public static List<StateInfo> stateInfos = new List<StateInfo>();
        public enum KiryuState
        {
            Brawler,
            Rush,
            Beast,
            Dragon
        }

        public KiryuState currentState
        {
            get => (KiryuState)NPC.ai[0];
            set {
                    NPC.ai[0] = (float)value;
                }
        }

        public KiryuState lastState;

        public StateInfo currentStateInfo
        {
            get => stateInfos[(int)currentState];
        }

        public bool secondStage
        {
            get => NPC.ai[1] == 1f;
            set => NPC.ai[1] = value ? 1f : 0f;
        }
        /// <summary>
        /// 0: No Current Attack
        /// 1: Dashing
        /// 2: Throwing
        /// </summary>
        public int currentAttack
        {
            get => (int)NPC.ai[2];
            set => NPC.ai[2] = (int)value;
        }

        public int deathAnimationTimer
        {
            get => (int)NPC.localAI[2];
            set => NPC.localAI[2] = (int)value;
        }

        public int attackTimer
        {
            get => (int)NPC.localAI[1];
            set => NPC.localAI[1] = (int)value;
        }

        public int stateSwitchTimer
        {
            get => (int)NPC.localAI[0];
            set => NPC.localAI[0] = (int)value;
        }

        public Vector2 goal;

        public Color currentColor
        {
            get => stateInfos[(int)currentState].stateColor;
        }

        public float floatDistance = 400f;

        public int currentDragonAttackTimer = 200;

        public int dragonPhaseSwitchAnimTimer = 0;

        public bool doDyingAnim = false;

        public int totalDyingAnimTime = 240;
        public override void Load()
        {
            deathFrames = new List<Asset<Texture2D>>();
            texture = Mod.Assets.Request<Texture2D>("NPCs/KiryuPNG/KiryuPNGOtherOne");
            for (int i = 0; i < 3; i++)
            {
                deathFrames.Add(Mod.Assets.Request<Texture2D>("NPCs/KiryuPNG/KiryuDeathSprite" + (i + 1)));
            }
            /*stateInfos.Add(new StateInfo(new Color(44, 126, 252), 60, 180, 120, 80, 30f, 1f, 60f));
            stateInfos.Add(new StateInfo(new Color(250, 50, 162), 45, 120, 80, 60, 45f, 1f, 50f));
            stateInfos.Add(new StateInfo(new Color(249, 181, 14), 90, 240, 240, 150, 20f, 1f, 40f));
            stateInfos.Add(new StateInfo(new Color(255, 22, 19), 120, 100, 200, 100, 30f, 1f, 40f));*/

            stateInfos.Add(new StateInfo(new Color(44, 126, 252), 90, 140, 50, 25, 25f, 0.75f, 40f));
            stateInfos.Add(new StateInfo(new Color(250, 50, 162), 30, 160, 35, 20, 40f, 1f, 50f));
            stateInfos.Add(new StateInfo(new Color(249, 181, 14), 65, 240, 80, 50, 20f, 0.5f, 40f));
            stateInfos.Add(new StateInfo(new Color(255, 22, 19), 200, 140, 65, 35, 25f, 1f, 50f));
        }

        public static float GetDifficultyDamageMultiplier()
        {
            switch (Main.GameMode)
            {
                case GameModeID.Normal:
                    return 1f;
                case GameModeID.Expert:
                    return 2f;
                case GameModeID.Master:
                    return 3f;
                case GameModeID.Creative:
                    return CreativePowerManager.Instance.GetPower<CreativePowers.DifficultySliderPower>().StrengthMultiplierToGiveNPCs;
                default:
                    return 1f;
            }
        }

        public override void SetStaticDefaults()
        {
            // Add this in for bosses that have a summon item, requires corresponding code in the item (See MinionBossSummonItem.cs)
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            // Automatically group with other bosses
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                //CustomTexturePath = "ExampleMod/Assets/Textures/Bestiary/MinionBoss_Preview",
                PortraitScale = 0.6f, // Portrait refers to the full picture when clicking on the icon in the bestiary
                PortraitPositionYOverride = 0f,
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }

        public override void SetDefaults()
        {
            NPC.width = 128;
            NPC.height = 128;
            NPC.damage = 120;
            NPC.defense = 40;
            NPC.lifeMax = 37500;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 56);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 10f; // Take up open spawn slots, preventing random NPCs from spawning during the fight
            NPC.aiStyle = -1;

            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/KiryuBossMusic");
            }

            circleTexture = ModContent.Request<Texture2D>("TestContent/Dusts/gfxCircle");
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(goal.X);
            writer.Write(goal.Y);
            writer.Write(attackTimer);
            writer.Write(currentDragonAttackTimer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            goal = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            attackTimer = reader.ReadInt32();
            currentDragonAttackTimer = reader.ReadInt32();
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<KiryuTrophyItem>(), 10));

            // All the Classic Mode drops here are based on "not expert", meaning we use .OnSuccess() to add them into the rule, which then gets added
            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());

            // Notice we use notExpertRule.OnSuccess instead of npcLoot.Add so it only applies in normal mode
            // Boss masks are spawned with 1/7 chance
            notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<KiryuMaskItem>(), 7));

            // This part is not required for a boss and is just showcasing some advanced stuff you can do with drop rules to control how items spawn
            // We make 12-15 ExampleItems spawn randomly in all directions, like the lunar pillar fragments. Hereby we need the DropOneByOne rule,
            // which requires these parameters to be defined
            var box = ItemDropRule.Common(ModContent.ItemType<DameDaneBoxItem>(), 6);
            var little = ItemDropRule.Common(ModContent.ItemType<LittleKiryuMountItem>(), 8);
            var fists = ItemDropRule.Common(ModContent.ItemType<FistItem>(), 6);
            var bike = ItemDropRule.Common(ModContent.ItemType<BicycleThrowingItem>(), 6);
            notExpertRule.OnSuccess(ItemDropRule.AlwaysAtleastOneSuccess([box, little, fists, bike]));

            // Finally add the leading rule
            npcLoot.Add(notExpertRule);

            // Add the treasure bag using ItemDropRule.BossBag (automatically checks for expert mode)
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<KiryuBossBag>()));

            // ItemDropRule.MasterModeCommonDrop for the relic
            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<KiryuRelic>()));

            // ItemDropRule.MasterModeDropOnAllPlayers for the pet
            npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<TheSongOfLifeItem>(), 4));
        }

        public override void OnSpawn(IEntitySource source)
        {
            attackTimer = 300;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                goal = Main.rand.NextVector2CircularEdge(floatDistance, floatDistance);
                NPC.netUpdate = true;
                if (Main.dedServ)
                {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)2);
                    packet.Write(NPC.Center.X);
                    packet.Write(NPC.Center.Y);
                    packet.Send();
                }
                else
                {
                    var dist = (Main.LocalPlayer.Center - NPC.Center).Length();
                    if (dist < 10000f)
                    {
                        var system = ModContent.GetInstance<KiryuBossIntroUISystem>();
                        //system.BossUI.OnInitialize();
                        system.ToggleUI();
                    }
                }
            }
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            // Sets the description of this NPC that is listed in the bestiary
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
                //new MoonLordPortraitBackgroundProviderBestiaryInfoElement(), // Plain black background
				new FlavorTextBestiaryInfoElement("No way.")
            });
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * balance * bossAdjustment * 0.75f);
        }

        public override void OnKill()
        {
            //Main.NewText("Death");
            if (!BossDown.downedKiryuPNG)
            {
                //Do something 
            }

            NPC.SetEventFlagCleared(ref BossDown.downedKiryuPNG, -1);
        }

        public override bool PreKill()
        { 

            return true;
        }

        public override bool CheckDead()
        {
            //Main.NewText("Checking Dead");
            if (!(doDyingAnim && deathAnimationTimer >= totalDyingAnimTime))
            {
                SoundEngine.PlaySound(KiryuBossSounds.death);
                doDyingAnim = true;
                attackTimer = 0;
                NPC.life = 1;
                NPC.damage = 0;
                NPC.velocity = Vector2.Zero;
                NPC.dontTakeDamage = true;
                NPC.netUpdate = true;
                return false;
            }
            return true;
        }

        /*public override bool? CanBeHitByItem(Player player, Item item)
        {
            return !doDyingAnim;
        }

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            return !doDyingAnim;
        }

        public override bool CanBeHitByNPC(NPC attacker)
        {
            return !doDyingAnim;
        }
*/
        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }

        public void DoAttackChecks(Player player, Vector2 dir)
        {
            attackTimer--;
            if (attackTimer < 0)
            {
                if (currentAttack == 0)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        currentAttack = Main.rand.Next(1, 3);
                        attackTimer = currentStateInfo.attackTime;

                        if(currentState == KiryuState.Dragon)
                        {
                            attackTimer = (int)MathHelper.Lerp(currentStateInfo.attackTime, currentStateInfo.attackTime - 70, ((float)((NPC.lifeMax / 2) - NPC.life) / (float)(NPC.lifeMax / 2)));
                            currentDragonAttackTimer = attackTimer;
                        }

                        if (currentAttack == 1)//Dashing
                        {
                            goal = (dir * currentStateInfo.dashSpeed);
                            /*if (currentState == KiryuState.Dragon)//Dragon phase has predictive dashing
                            {
                                var prediction = ((player.Center + player.velocity) - NPC.Center).SafeNormalize(Vector2.UnitY);
                                goal = (prediction * currentStateInfo.dashSpeed) + player.velocity;
                            }
                            else
                            {
                                
                            }*/
                        }
                        else//Shoot proj if not dashing
                        {
                            int[] projs = [];
                            if (currentState == KiryuState.Beast)
                            {
                                projs = [ModContent.ProjectileType<Bicycle>()];
                            }
                            //Rush and Dragon projectiles are handled differently, check attack AI

                            if (projs.Length > 0)
                            {
                                int type = projs[Main.rand.Next(0, projs.Length)];
                                ShootProjectile(type, dir);
                            }
                        }
                        NPC.netUpdate = true;
                    }
                    RandomAttackSound();
                }
                else
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        currentAttack = 0;
                        if (!Main.expertMode)
                        {
                            attackTimer = currentStateInfo.cooldownTime;
                            goal = Main.rand.NextVector2CircularEdge(floatDistance, floatDistance); 
                        }
                        else
                        {
                            if (currentState == KiryuState.Dragon)
                            {
                                attackTimer = (int)MathHelper.Lerp(currentStateInfo.cooldownTime, currentStateInfo.cooldownTime / 2, ((float)((NPC.lifeMax / 2) - NPC.life) / (float)(NPC.lifeMax / 2)));
                                goal = Main.rand.NextVector2CircularEdge(floatDistance, floatDistance);
                            }
                            else
                            {
                                attackTimer = (int)MathHelper.Lerp(currentStateInfo.cooldownTime, currentStateInfo.cooldownTime / 2, ((float)(NPC.lifeMax - NPC.life) / (float)(NPC.lifeMax / 2)));
                                goal = Main.rand.NextVector2CircularEdge(floatDistance + 100f, floatDistance + 100f);
                            }
                        }
                        NPC.netUpdate = true;
                    }
                }
            }
        }

        public void DoCurrentAttackAI(Player player, Vector2 dir)
        {
            if (currentAttack != 0)
            {
                switch (currentAttack)
                {
                    case 1:
                        if (currentState == KiryuState.Brawler || currentState == KiryuState.Dragon)
                        {
                            int dashes = 2;
                            int attackTime = currentStateInfo.attackTime;
                            if(currentState == KiryuState.Dragon)
                            {
                                dashes += 1;
                                attackTime = currentDragonAttackTimer;
                            }
                            if (attackTimer % (attackTime / dashes) == 0 && attackTimer != 0)
                            {
                                goal = (dir * currentStateInfo.dashSpeed);
                                NPC.velocity = goal;
                                NPC.netUpdate = true;
                                break;
                            }
                            else
                            {
                                float time = ((float)attackTimer % (float)(attackTime / dashes)) / (float)(attackTime / dashes);
                                //Main.NewText(time+ " " + NPC.velocity.Length());
                                NPC.velocity = Vector2.Lerp(goal * 0.2f, goal, time);
                            }
                        }
                        else
                        {

                            NPC.velocity = Vector2.Lerp(goal * 0.2f, goal, (float)attackTimer / (float)currentStateInfo.attackTime);
                            /*if (((float)attackTimer / (float)currentStateInfo.attackTime) < 0.5f)
                            {
                                
                            }
                            else
                            {
                                NPC.velocity = goal;
                            }*/
                        }
                        break;
                    case 2:
                        MoveToGoalPosition(player);
                        //Rush attacks in 3 bursts each time it attacks
                        if (Main.netMode != NetmodeID.MultiplayerClient && currentState == KiryuState.Rush && attackTimer % (currentStateInfo.attackTime / 2) == 0)
                        {
                            ShootProjectile(ModContent.ProjectileType<Fist>(), dir);
                        }
                        if (Main.netMode != NetmodeID.MultiplayerClient && currentState == KiryuState.Dragon && attackTimer % (currentDragonAttackTimer / 4) == 0)
                        {
                            int [] projs = [ModContent.ProjectileType<Bucket>(), ModContent.ProjectileType<Crate>(), ModContent.ProjectileType<TrafficCone>(), ModContent.ProjectileType<Bicycle>(), ModContent.ProjectileType<Fist>()];
                            int type = Main.rand.Next(0, projs.Length);
                            var prediction = ((player.Center + player.velocity) - NPC.Center).SafeNormalize(Vector2.UnitY);
                            ShootProjectile(projs[type], prediction);
                        }
                        if (Main.netMode != NetmodeID.MultiplayerClient && currentState == KiryuState.Brawler && attackTimer % (currentStateInfo.attackTime / 2) == 0 && attackTimer != 0)
                        {
                            int [] projs = [ModContent.ProjectileType<Bucket>(), ModContent.ProjectileType<Crate>(), ModContent.ProjectileType<TrafficCone>()];
                            ShootProjectile(projs[Main.rand.Next(0, projs.Length)], dir);
                        }
                        break;
                }
            }
            else
            {
                MoveToGoalPosition(player);
            }
        }

        public void DoStateSwitchChecks(Vector2 dir)
        {
            if(currentState != KiryuState.Dragon)
            {
                stateSwitchTimer--;
            }
            if (stateSwitchTimer < 0 && currentAttack == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if(!Main.expertMode) 
                    {
                        currentState = (KiryuState)Main.rand.Next(0, 3);
                    }
                    else
                    {
                        if(!secondStage) 
                        {
                            currentState = (KiryuState)Main.rand.Next(0, 3);
                        }
                        else
                        {
                            currentState = KiryuState.Dragon;
                            stateSwitchTimer = 500;
                        }
                    }

                    NPC.damage = (int)((float)currentStateInfo.contactDamage * GetDifficultyDamageMultiplier());
                    NPC.netUpdate = true;
                    //Main.NewText(NPC.damage);
                }
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    if (currentState != KiryuState.Dragon)
                    {
                        for (int i = 0; i < 15; i++)
                        {
                            Dust.NewDust(NPC.Center, 64, 64, DustID.Snow, 0, 0, 120, currentColor, Main.rand.NextFloat(0.5f, 1.5f));
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            var vect = Main.rand.NextVector2Circular(30f, 30f);
                            Dust.NewDust(NPC.Center, 64, 64, ModContent.DustType<Smoke>(), vect.X, vect.Y, 0, currentColor, Main.rand.NextFloat(2f, 3f));
                        }
                        SoundEngine.PlaySound(SoundID.Item122, NPC.Center);
                        SoundEngine.PlaySound(KiryuBossSounds.phaseChange, NPC.Center);
                        dragonPhaseSwitchAnimTimer = 150;
                        //Main.NewText("Switch to Dragon Anim?");
                    }
                }
                stateSwitchTimer = 600;
            }
        }

        public void DoMultiplayerStateSwitchVisuals()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                if (currentState != lastState && currentState == KiryuState.Dragon)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        var vect = Main.rand.NextVector2Circular(30f, 30f);
                        Dust.NewDust(NPC.Center, 64, 64, ModContent.DustType<Smoke>(), vect.X, vect.Y, 0, currentColor, Main.rand.NextFloat(2f, 3f));
                    }
                    SoundEngine.PlaySound(SoundID.Item122, NPC.Center);
                    SoundEngine.PlaySound(KiryuBossSounds.phaseChange, NPC.Center);
                    dragonPhaseSwitchAnimTimer = 150;
                    lastState = currentState;
                }
            }
        }

        public void NormalAttackSound()
        {
            int rand = Main.rand.Next(0, KiryuBossSounds.totalNormalAttackLines);
            SoundEngine.PlaySound(KiryuBossSounds.attacks[rand], NPC.Center);
        }

        public void EffortAttackSound()
        {
            int rand = Main.rand.Next(0, KiryuBossSounds.totalEffortAttackLines);
            SoundEngine.PlaySound(KiryuBossSounds.effortAttacks[rand], NPC.Center);
        }

        public void RandomAttackSound()
        {
            if (Main.rand.NextBool())
            {
                if(currentState == KiryuState.Dragon && currentAttack == 1)
                {
                    EffortAttackSound();
                }
                else
                {
                    NormalAttackSound();
                }
            }
        }

        public void ShootProjectile(int type, Vector2 dir)
        {
            float speed = currentStateInfo.projSpeed;
            float damageMult = 1;
            if (Main.expertMode)
            {
                speed += 10f;
            }
            if(currentState == KiryuState.Dragon)
            {
                if (type == ModContent.ProjectileType<Fist>())
                {
                    speed += 20f;
                }
                else if(type == ModContent.ProjectileType<Bicycle>())
                {
                    speed -= 10f;
                    damageMult += 0.2f;
                }
            }
            int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, dir * speed, type, (int)(currentStateInfo.projDamage * damageMult), 2f, ai0: (float)currentState);
            //Main.projectile[proj].damage = (int)(currentStateInfo.projDamage * damageMult * Main.GameModeInfo.EnemyDamageMultiplier);
        }

        public void DoSecondStageChecks()
        {
            if(!secondStage && NPC.life <= NPC.lifeMax / 2)
            {
                secondStage = true;
                stateSwitchTimer = 0;
                NPC.netUpdate = true;
            }
            else if(dragonPhaseSwitchAnimTimer > 0)
            {
                dragonPhaseSwitchAnimTimer--;
                //Main.NewText(dragonPhaseSwitchAnimTimer);
            }
        }

        public void DoDyingAnimChecks()
        {
            deathAnimationTimer++;
            if(deathAnimationTimer >= totalDyingAnimTime) 
            {
                NPC.life = 0;
                NPC.checkDead();
            }
        }
        public override void AI()
        {
            if (doDyingAnim)
            {
                //Main.NewText("HE'S DYING!!!1");
                DoDyingAnimChecks();
                return;
            }
            // This should almost always be the first code in AI() as it is responsible for finding the proper player target
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }
            Player player = Main.player[NPC.target];

            if (player.dead)
            {
                // If the targeted player is dead, flee
                NPC.velocity.Y -= 0.04f;
                // This method makes it so when the boss is in "despawn range" (outside of the screen), it despawns in 10 ticks
                NPC.EncourageDespawn(10);
                return;
            }
            var dir = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            DoAttackChecks(player, dir);
            DoCurrentAttackAI(player, dir);
            DoStateSwitchChecks(dir);
            DoMultiplayerStateSwitchVisuals();
            DoSecondStageChecks();

            if (!Main.dedServ)
            {
                int runs = 1;
                float scale = 0.5f;
                int alpha = 130;
                if (secondStage)
                {
                    runs += 2;
                    alpha -= 50;
                    scale += 0.25f;
                }
                if (currentAttack == 1)
                {
                    runs += 1;
                    alpha -= 50;
                    scale += 1f;
                }

                for (int i = 0; i < runs; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.Hitbox.Width, NPC.Hitbox.Height, ModContent.DustType<Smoke>(), newColor: currentColor, Scale: scale, Alpha: alpha);
                }
                //Dust.QuickDustLine(NPC.Center, player.Center, 20f, Color.White);
            }
        }

        public void MoveToGoalPosition(Player player)
        {
            Vector2 goalLocation = goal + player.Center;
            //goalLocation.Y -= 250;

            var direction = goalLocation - NPC.Center;
            var opposite = NPC.Center - player.Center;
            var oppositeDir = opposite.SafeNormalize(Vector2.Zero);
            var dist = direction.Length();
            var oppDist = opposite.Length();
            direction = direction.SafeNormalize(Vector2.Zero);
            float speed = (float)Math.Sqrt(dist) * currentStateInfo.moveSpeed;
            speed = Math.Clamp(speed, 0, 15);

            float awayFactor = MathHelper.Lerp(30f, 0f, oppDist / 350f);
            awayFactor = Math.Clamp(awayFactor, 0f, 30f);
            //Main.NewText(awayFactor + " " + oppDist);

            NPC.velocity = (direction * speed) + (oppositeDir * awayFactor);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (doDyingAnim)
            {
                DrawDeathAnimation(spriteBatch, screenPos, drawColor);
                return false;
            }
            var state = (int)currentState;
            //Main.NewText(state);
            Color col = stateInfos[state].stateColor;
            col.A = 0;
            float scale = ((float)Math.Sin(Main.timeForVisualEffects*0.1f) / 8f) + 1.25f;
            spriteBatch.Draw(texture.Value, NPC.Center - screenPos, null, col, 0f, new Vector2(texture.Value.Width/2, texture.Value.Height/2), scale, SpriteEffects.None, 1f);

            if(currentAttack == 1)
            {
                var dashColor = col;
                var ballTexture = TextureAssets.Extra[91].Value;
                scale = (NPC.velocity.Length() / currentStateInfo.dashSpeed) * 10f;
                //Main.NewText(scale);
                var rect = new Rectangle(0, 0, ballTexture.Width, ballTexture.Height);
                Vector2 origin = rect.Size() / 2;
                origin.Y -= rect.Height / 4;
                dashColor.A = 30;
                spriteBatch.Draw(ballTexture, NPC.Center - Main.screenPosition, rect, dashColor,
                NPC.velocity.ToRotation() + (float)Math.PI / 2f, origin, scale, SpriteEffects.None, 0f);

                for (int i = 0; i < 3; i++)
                {
                    float time = (float)(Main.timeForVisualEffects + (15 * i)) % 30 / 30f;
                    float newScale = scale * time;
                    float newOpacity = NPC.Opacity * (1f - time);
                    dashColor = (dashColor * 8) * newOpacity;
                    spriteBatch.Draw(ballTexture, NPC.Center - Main.screenPosition, rect, dashColor,
                        NPC.velocity.ToRotation() + (float)Math.PI / 2f, origin, newScale, SpriteEffects.None, 0f);
                }
            }


            if(dragonPhaseSwitchAnimTimer > 0)
            {
                var circle = circleTexture.Value;
                float time = ((float)dragonPhaseSwitchAnimTimer) / 150f;
                var circleColor = col;
                scale = (1f - KiryuBossIntro.scaleLerp.GetLerp(time))  * 2f;
                var opacity = KiryuBossIntro.opacityLerp.GetLerp(time);
                var rect = new Rectangle(0,0,circle.Width,circle.Height);
                var origin = rect.Size() / 2;
                circleColor.A = (byte)(255f * opacity);
                //Main.NewText(scale);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone);
                spriteBatch.Draw(circle, NPC.Center - Main.screenPosition, rect, circleColor, 0f, origin, scale, SpriteEffects.None, 0f);
                spriteBatch.End();
                spriteBatch.Begin();
            }
            
            return true;
        }

        private void DrawDeathAnimation(SpriteBatch batch, Vector2 drawPos, Color drawColor)
        {
            int frame = 0;
            Vector2 scale = new Vector2(1f, 1f);
            if(deathAnimationTimer >= 50 && deathAnimationTimer < 135)
            {
                frame = 1;
            }
            else if(deathAnimationTimer >= 135)
            {
                frame = 2;
            }

            if(frame == 2)
            {
                float time = (((float)deathAnimationTimer - 135f) / ((float)totalDyingAnimTime - 135f));
                scale.X = 1f + 1f * time;
                scale.Y = 1f - time;
            }

            var texture = deathFrames[frame].Value;
            var origin = new Vector2(texture.Width/2, texture.Height/2);
            Main.EntitySpriteDraw(texture, NPC.Center - Main.screenPosition, null, drawColor, NPC.rotation, origin, scale, SpriteEffects.None);

        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (currentAttack == 0 && attackTimer <= 60 && !doDyingAnim)
            {
                Color col = currentStateInfo.stateColor;
                col.A = 100;
                var shineTexture = TextureAssets.Extra[98].Value;
                var rect = new Rectangle(0, 0, shineTexture.Width, shineTexture.Height);
                Vector2 origin = rect.Size() / 2;
                float time = 1 - ((float)attackTimer / 60f);
                float scale = LuckyShotBigShot.ShootInLerp(time) * 3f;
                Vector2 offset = new Vector2(-55f, 70f);
                spriteBatch.Draw(shineTexture, NPC.Center - offset - screenPos, null, col, time, origin, scale, SpriteEffects.None, 0f);
                spriteBatch.Draw(shineTexture, NPC.Center - offset - screenPos, null, col, time + (float)Math.PI/2, origin, scale, SpriteEffects.None, 0f);
            }
        }

        public struct StateInfo
        {
            public StateInfo(Color c, int a, int co, int con, int projD, float projS, float m, float d)
            {
                stateColor = c;
                attackTime = a;
                cooldownTime = co;

                contactDamage = con;

                projDamage = projD;
                projSpeed = projS;
                moveSpeed = m;
                dashSpeed = d;
            }
            public Color stateColor;
            public int attackTime;
            public int cooldownTime;

            public int contactDamage;

            public int projDamage;
            public float projSpeed;

            public float moveSpeed;
            public float dashSpeed;
        }

        public class BossDown : ModSystem
        {
            public static bool downedKiryuPNG = false;
            public static bool downedMinosPrime = false;

            public override void PostSetupContent()
            {
                DoBossChecklistIntegration();
            }

            private void DoBossChecklistIntegration()
            {
                if (!ModLoader.TryGetMod("BossChecklist", out Mod bossChecklistMod))
                {
                    return;
                }
                if (bossChecklistMod.Version < new Version(1, 6))
                {
                    return;
                }
                string internalName = "KiryuPNG";
                float weight = 12.5f;
                Func<bool> downed = () => BossDown.downedKiryuPNG;
                int bossType = ModContent.NPCType<KiryuPNG>();
                int spawnItem = ModContent.ItemType<KiryuSummonItem>();
                List<int> collectibles = new List<int>()
                {
                ModContent.ItemType<KiryuRelic>(),
                ModContent.ItemType<TheSongOfLifeItem>(),
                ModContent.ItemType<KiryuTrophyItem>(),
                ModContent.ItemType<KiryuMaskItem>(),
                };
                LocalizedText summonConditions = Language.GetText("Mods.TestContent.NPCs.KiryuPNG.SummonText").WithFormatArgs(spawnItem);

                bossChecklistMod.Call(
                "LogBoss",
                Mod,
                internalName,
                weight,
                downed,
                bossType,
                new Dictionary<string, object>()
                    {
                    ["spawnItems"] = spawnItem,
                    ["collectibles"] = collectibles,
                    ["spawnInfo"] = summonConditions
                    // Other optional arguments as needed are inferred from the wiki
                    }
                );
            }

            public override void ClearWorld()
            {
                downedKiryuPNG = false;
                downedMinosPrime = false;
            }

            public override void SaveWorldData(TagCompound tag)
            {
                if (downedKiryuPNG)
                {
                    tag["downedKiryuPNG"] = true;
                }
                if(downedMinosPrime)
                {
                    tag["downedMinosPrime"] = true;
                }
            }

            public override void LoadWorldData(TagCompound tag)
            {
                downedKiryuPNG = tag.ContainsKey("downedKiryuPNG");
                downedMinosPrime = tag.ContainsKey("downedMinosPrime");
            }

            public override void NetSend(BinaryWriter writer)
            {
                var flags = new BitsByte();
                flags[0] = downedKiryuPNG;
                flags[1] = downedMinosPrime;
                writer.Write(flags);
            }

            public override void NetReceive(BinaryReader reader)
            {
                BitsByte flags = reader.ReadByte();
                downedKiryuPNG = flags[0];
                downedMinosPrime = flags[1];
            }
        }
    }

    public class KiryuBossSounds : ModSystem
    {
        public static SoundStyle summon, death, phaseChange;
        public static List<SoundStyle> attacks = new List<SoundStyle>();
        public static List<SoundStyle> effortAttacks = new List<SoundStyle>();

        public const int totalNormalAttackLines = 7;
        public const int totalEffortAttackLines = 2;
        public override void OnModLoad()
        {
            summon = LoadSoundStyle("summon");
            death = LoadSoundStyle("death", 1.2f);
            phaseChange = LoadSoundStyle("switchToDragon");

            for (int i = 0; i < totalNormalAttackLines; i++)
            {
                attacks.Add(LoadSoundStyle("attackVar" + (i + 1)));
            }

            for (int i = 0; i < totalEffortAttackLines; i++)
            {
                effortAttacks.Add(LoadSoundStyle("attackEffort" + (i + 1)));
            }
        }

        public SoundStyle LoadSoundStyle(string pathEnd, float volume = 0.8f)
        {
            return new SoundStyle("TestContent/Assets/Sounds/Kiryu/" + pathEnd)
            {
                Volume = 0.8f,
                MaxInstances = 1,
                PlayOnlyIfFocused = true
            };
        }
    }
}
