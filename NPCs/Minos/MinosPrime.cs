using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using Stubble.Core.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Bestiary = Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.CameraModifiers;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Dusts;
using TestContent.Effects.Music;
using TestContent.Global.NPC;
using TestContent.NPCs.Minos.Items;
using TestContent.NPCs.Minos.Items.BossDecorum;
using TestContent.NPCs.Minos.Items.BossDecorum.Pet;
using TestContent.NPCs.Minos.Projectiles;
using TestContent.UI.BossBars;
using TestContent.Utility;
using TextContent.Effects.Graphics.Primitives;
using static TestContent.UI.SlotMachineSystem;

namespace TestContent.NPCs.Minos
{
    public enum AttackIdentity
    {
        Walk,
        TripleHit,
        QuadHit,
        SeekingSnake,
        GroundSlam,
        Judgement,
        Jump,
        Spike,
        Uppercut,
        DiveAimed,
        DiveDown
    }
    [AutoloadBossHead]
    //Append abstract here to stop the class from compiling
    /// <summary>
    /// TODO: Attacks come out too fast. Add end lag to uppercut/ spike. Add starting lag to three/four hit combos. Sometimes minos jumps but doesnt do anything. Add music
    /// </summary>
    public class MinosPrime : ModNPC, IPixelatedPrimitiveRenderer
    {
        //Remove when we actually have the sprites
        //public override string Texture => "TestContent/ExtraTextures/InvisibleSprite";

        public SoundStyle concrete;
        public SoundStyle dash;
        public SoundStyle judgement;
        public SoundStyle slam;
        public static SoundStyle attackPlink;
        public SoundStyle swing;

        List<MinosAttack> attacks;

        public static int maxStamina = 10;
        public int stamina = 10;

        public int staminaTick = 10;

        public int diveCancelCooldown = 0;
        public int DIVE_CANCEL_COOLDOWN_MAX = 360;

        /// <summary>
        /// Positions to draw afterimages for after teleportation. In world space (not screen space)
        /// </summary>
        private List<MinosTrail> afterImagetrails = new List<MinosTrail>();
        public List<MinosTrail> trailQueue = new List<MinosTrail>();

        public PrimitiveSettings telegraphSettings;
        public PrimitiveSettings telegraphOutlineSettings;
        public Vector2 telegraphStart = Vector2.Zero;
        public Vector2 telegraphEnd = Vector2.Zero;
        public float telegraphTime = -1f;

        PixelationPrimitiveLayer IPixelatedPrimitiveRenderer.LayerToRenderTo => PixelationPrimitiveLayer.BeforeNPCs;

        public int maxShockwaveTime = 180;
        public int currentShockwaveTime = 0;
        //public Vector2 shockWavePosition = Vector2.Zero;

        private int rippleCount = 2;
        private int rippleSize = 3;
        private int rippleSpeed = 25;
        private float distortStrength = 50f;

        public MinosVoiceLineController voiceController = ModContent.GetInstance<MinosVoiceLineController>();
        public MinosAnimationController animController = new MinosAnimationController();

        public int baseDamage
        {
            get
            {
                return 35;
            }        
        }

        private bool secondPhaseTransition = false;

        public AttackIdentity currentAttackType
        {
            get => (AttackIdentity)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }

        public AttackIdentity clientAttack = AttackIdentity.Walk;

        public int attackTimer
        {
            get => (int)NPC.ai[1];
            set => NPC.ai[1] = (float)value;
        }

        public int StaminaTimer
        {
            get => (int)NPC.ai[2];
            set => NPC.ai[2] = (float)value;
        }

        public bool inSecondPhase
        {
            get => (int)NPC.ai[3] == 1;
            set => NPC.ai[3] = value ? 1 : 0;
        }

        public AttackIdentity nextAttack;

        public MinosAttack currentAttack
        {
            get
            {
                if(attacks == null)
                {
                    CreateAttacks();
                }
                return attacks[(int)currentAttackType];
            }
        }

        public bool doContactDamage = true;
        public Vector2 knockbackDir = Vector2.Zero;
        public float knockbackStrength = 0f;
        public bool canDiveCancel = false;


        public bool shouldDrawTrail = false;

        public void CreateAttacks()
        {
            attacks = [new MinosWalk(this), new MinosTriple(this), new MinosQuad(this), new MinosSnake(this),
            new MinosSlam(this), new MinosJudgement(this), new MinosJump(this), new MinosSpike(this), new MinosUppercut(this),
            new MinosDiveAimed(this), new MinosDiveDown(this)];
        }

        public override void Load()
        {
            Terraria.On_NPC.CheckActive += On_NPC_CheckActive;
            attackPlink = new SoundStyle(ModUtils.GetSoundFileLocation("Minos/AttackIndicator"))
            {
                PlayOnlyIfFocused = true,
                Volume = 0.4f,
                MaxInstances = 3,
            };
        }

        private void On_NPC_CheckActive(On_NPC.orig_CheckActive orig, NPC self)
        {
            bool active = self.active;
            orig(self);
            if(self.netID == ModContent.NPCType<MinosPrime>() && active && !self.active) 
            {
                MinosPrime inst = (MinosPrime)self.ModNPC;
                inst.ClearEffects();
            }
        }

        public override void SetStaticDefaults()
        {
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.TrailingMode[Type] = 0;
            NPCID.Sets.TrailCacheLength[Type] = 3;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                PortraitPositionYOverride = -60f,
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }

        public override void SetBestiary(Bestiary.BestiaryDatabase database, Bestiary.BestiaryEntry bestiaryEntry)
        {
            // Sets the description of this NPC that is listed in the bestiary
            bestiaryEntry.UIInfoProvider = new Bestiary.CommonEnemyUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[NPC.type], quickUnlock: true);
            bestiaryEntry.Info.AddRange(new List<Bestiary.IBestiaryInfoElement> {
                new Bestiary.MoonLordPortraitBackgroundProviderBestiaryInfoElement(), // Plain black background
				new Bestiary.FlavorTextBestiaryInfoElement("Oh no, Minos. What could he possibly want?")
            });
        }

        public override void SetDefaults()
        {
            NPC.height = 142;
            NPC.width = 64;
            NPC.lifeMax = 125000;
            NPC.defense = 70;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(platinum: 3);
            NPC.HitSound = new SoundStyle(ModUtils.GetSoundFileLocation("MinosHit"))
            {
                Volume = 0.4f,
                PlayOnlyIfFocused = true,
                PitchVariance = 0.5f,
                MaxInstances = 5
            };
            NPC.damage = baseDamage * 2;
            NPC.boss = true;
            NPC.BossBar = ModContent.GetInstance<UltraBossBar>();

            concrete = new SoundStyle(ModUtils.GetSoundFileLocation("Minos/Concrete"))
            {
                PlayOnlyIfFocused = true,
                Volume = 0.4f,
                IsLooped = true,
            };
            dash = new SoundStyle(ModUtils.GetSoundFileLocation("Minos/Dodge"))
            {
                PlayOnlyIfFocused = true,
                Volume = 0.55f,
                MaxInstances = 3,
            };
            judgement = new SoundStyle(ModUtils.GetSoundFileLocation("Minos/Judgement"))
            {
                PlayOnlyIfFocused = true,
                Volume = 0.4f,
            };
            slam = new SoundStyle(ModUtils.GetSoundFileLocation("Minos/Slam"))
            {
                PlayOnlyIfFocused = true,
                Volume = 0.4f,
                MaxInstances = 3,
            };
            swing = new SoundStyle(ModUtils.GetSoundFileLocation("Minos/Swing"))
            {
                PlayOnlyIfFocused = true,
                Volume = 0.45f,
                MaxInstances = 3,
            };

            telegraphSettings = new PrimitiveSettings(PrimitiveWidthFunction, PrimitiveColorFunction, pixelate: true);
            telegraphOutlineSettings = new PrimitiveSettings(PrimitiveWidthFunctionOutline, PrimitiveColorFunctionOutline, pixelate: true);
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * balance * bossAdjustment * 0.65f);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MinosTrophyItem>(), 10));

            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());

            notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<MinosMaskItem>(), 7));

            IItemDropRule[] ruleList = [
                ItemDropRule.Common(ModContent.ItemType<DroneController>(), 4),
                ItemDropRule.Common(ModContent.ItemType<FreezeFrame>(), 4),
                ItemDropRule.Common(ModContent.ItemType<KnuckleBlaster>(), 4),
                ItemDropRule.Common(ModContent.ItemType<RailCannon>(), 4)
                ];
            notExpertRule.OnSuccess(ItemDropRule.AlwaysAtleastOneSuccess(ruleList));
            notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<WhiplashItem>(), 4));
            // Finally add the leading rule
            npcLoot.Add(notExpertRule);

            // Add the treasure bag using ItemDropRule.BossBag (automatically checks for expert mode)
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<MinosBossBag>()));

            // ItemDropRule.MasterModeCommonDrop for the relic
            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<MinosRelicItem>()));

            // ItemDropRule.MasterModeDropOnAllPlayers for the pet
            npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<MinosPetItem>(), 4));
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.SuperHealingPotion;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return doContactDamage;
        }

        public override void OnKill()
        {
            BossNPCGlobals.minos = -1;
        }

        public override bool CheckActive()
        {
            return true;
        }

        public void ClearEffects()
        {
            if (!Main.dedServ)
            {
                if (Filters.Scene["TestContent:ShockwaveFilter"].IsActive())
                {
                    Filters.Scene["TestContent:ShockwaveFilter"].Deactivate();
                }
                currentAttack.PostAttack();
                //Main.NewText("oughhh");
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if(NPC.life <= 0)
            {
                ClearEffects(); 
            }
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            //15% resistance to homing projectiles
            if (ProjectileID.Sets.CultistIsResistantTo[projectile.type])
            {
                modifiers.FinalDamage -= 0.15f;
            }
        }

        public void UseStamina(int amount)
        {
            if(Main.expertMode && inSecondPhase)
            {
                return;
            }
            if(Main.expertMode || inSecondPhase)
            {
                amount /= 2;
            }
            stamina = Math.Clamp(stamina - amount, 0, maxStamina);
        }

        public MinosAttack GetAttack(AttackIdentity id)
        {
            if(attacks == null)
            {
                CreateAttacks();
            }
            return attacks[(int)id];
        }

        public void DoSecondPhaseTransition()
        {
            if(Main.netMode != NetmodeID.Server && !secondPhaseTransition)
            {
                secondPhaseTransition = true;
                if(Main.rand.Next(1, 100000) == 1)
                {
                    voiceController.OverwriteVoiceLine(MinosVoiceLineController.MinosVoiceID.Weed, NPC.Center);
                }
                else
                {
                    voiceController.OverwriteVoiceLine(MinosVoiceLineController.MinosVoiceID.Weak, NPC.Center);
                }
            }
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            currentAttack.SendExtraAI(writer);
            writer.Write(stamina);
            writer.Write((int)nextAttack);
            writer.Write(diveCancelCooldown);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            currentAttack.RecieveExtraAI(reader);
            stamina = reader.ReadInt32();
            nextAttack = (AttackIdentity)reader.ReadInt32();
            diveCancelCooldown = reader.ReadInt32();
            if(clientAttack != currentAttackType)
            {
                var attack = currentAttack;
                attack.ChangeNPCParams();
                attack.PreAttack();
                attack.moveRNG = Main.rand.Next(0, 100001);
            }
        }

        public override void OnSpawn(IEntitySource source)
        {
            if(Main.netMode != NetmodeID.MultiplayerClient)
            {
                nextAttack = AttackIdentity.TripleHit;
                NPC.netUpdate = true;
            }
        }

        public override void AI() 
        {
            currentAttack.AttackAI();

            bool diveCooldownUp = diveCancelCooldown <= 0;

            if(!diveCooldownUp)
            {
                diveCancelCooldown--;
            }
            else
            {
                if (canDiveCancel)
                {
                    var Target = currentAttack.Target;
                    bool cancelled = false;
                    if (NPC.ConeCheck(Target.Center, Vector2.UnitY, MathHelper.ToRadians(25f), 1000f))
                    {
                        MinosAttack.CancelIntoAttack(this, AttackIdentity.DiveDown);
                        cancelled = true;
                    }
                    else if (NPC.ConeCheck(Target.Center, Vector2.UnitY, MathHelper.ToRadians(70f), 1000f))
                    {
                        MinosAttack.CancelIntoAttack(this, AttackIdentity.DiveAimed);
                        cancelled = true;
                    }

                    if (cancelled)
                    {
                        diveCancelCooldown = DIVE_CANCEL_COOLDOWN_MAX;
                    }
                }
            }

            BossNPCGlobals.minos = NPC.whoAmI;

            if(currentAttackType == AttackIdentity.Walk)
            {
                if(StaminaTimer++ >= staminaTick)
                {
                    if (stamina < maxStamina)
                    {
                        stamina++;
                    }
                    StaminaTimer = 0;
                }
            }
            for(int i = 0; i < afterImagetrails.Count; i++)
            {
                var trail = afterImagetrails[i];
                trail.timeLeft--;
                if (trail.timeLeft < 0)
                {
                    afterImagetrails.Remove(trail);
                }
            }
            foreach(var trail in trailQueue)
            {
                afterImagetrails.Add(trail);
            }
            trailQueue.Clear();

            if(Main.netMode != NetmodeID.Server)
            {
                if (currentShockwaveTime > 0 && Filters.Scene["TestContent:ShockwaveFilter"].IsActive())
                {
                    float progress = ((int)maxShockwaveTime - (float)currentShockwaveTime) / 60f;
                    Filters.Scene["TestContent:ShockwaveFilter"].GetShader().UseProgress(progress).UseOpacity(distortStrength * (1 - progress / ((float)maxShockwaveTime / 60f)));
                    currentShockwaveTime--;
                }
                else if (Filters.Scene["TestContent:ShockwaveFilter"].IsActive())
                {
                    Filters.Scene["TestContent:ShockwaveFilter"].Deactivate();
                }

                if (inSecondPhase)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<Smoke>(), 0, Main.rand.NextFloat(-3f, 0f), 0, Color.Cyan * 0.25f, Main.rand.NextFloat(0.5f, 0.75f));
                }
            }

            voiceController.UpdateVoiceLinePosition(NPC.Center);

            animController.Update();
            
            if(Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (!inSecondPhase && NPC.life <= (NPC.lifeMax / 2))
                {
                    inSecondPhase = true;
                    NPC.netUpdate = true;
                }
            }

            if (inSecondPhase)
            {
                DoSecondPhaseTransition();
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if(knockbackStrength > 0)
            {
                target.velocity += knockbackDir * knockbackStrength;
                //Main.NewText($"Should take kb, velocity : {target.velocity}");
                //Maybe not synced?
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var tuple = animController.GetAnimData();
            if(currentAttack.moveRNG == 100000 && animController.currentSprite == MinosAnimationController.MinosSprite.Crouching)
            {
                tuple = animController.GetAnimData(MinosAnimationController.MinosSprite.AssOut);
            }
            if (NPC.IsABestiaryIconDummy)
            {
                //tuple = animController.GetAnimData(MinosAnimationController.MinosSprite.Standing);
                //spriteBatch.Draw(TextureAssets.MagicPixel.Value, NPC.position - Main.screenPosition, tuple.Item2, Color.White, NPC.rotation, tuple.Item2.Size() / 2f, NPC.scale, SpriteEffects.None, 0f);
                return true;
            }
            var texture = tuple.Item1;
            var effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            foreach (DrawData d in currentAttack.preDraws)
            {
                spriteBatch.Draw(d.texture, d.position, d.sourceRect, d.color, d.rotation, d.origin, d.scale, d.effect, 0f);
            }

            var rect = tuple.Item2;
            var origin = rect.Size() / 2;
            var drawpos = NPC.Center - Main.screenPosition;
            bool bottomCentered = false;
            if(animController.currentSprite == MinosAnimationController.MinosSprite.Crouching || animController.currentSprite == MinosAnimationController.MinosSprite.JudgementPose || animController.currentSprite == MinosAnimationController.MinosSprite.AssOut)
            {
                origin = new Vector2(rect.Width / 2, rect.Height);
                drawpos = NPC.GetFeetPosition() - Main.screenPosition;
                bottomCentered = true;
            }
            //Teleport Trails
            foreach (var trail in afterImagetrails)
            {
                var trailTuple = animController.GetAnimData(trail.sprite);
                foreach (var pos in trail.positions)
                {
                    var position = pos;
                    var col = trail.trailColor;
                    col *= 0.5f * ((float)trail.timeLeft / (float)trail.maxTime);
                    //if(trail.sprite == MinosAnimationController.MinosSprite.Crouching || trail.sprite == MinosAnimationController.MinosSprite.JudgementPose)
                    //{
                    //    position = pos + new Vector2(0f, NPC.height / 2);
                    //}
                    spriteBatch.Draw(trailTuple.Item1, (position) - Main.screenPosition, trailTuple.Item2, col, NPC.rotation, trailTuple.Item2.Size() / 2, NPC.scale, trail.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
                }
            }
            //Cached Trails
            if(shouldDrawTrail)
            {
                
                for (int i = 0; i < NPCID.Sets.TrailCacheLength[Type]; i++)
                {
                    var oldPosition = NPC.oldPos[i];
                    var trailColor = Color.White;
                    trailColor *= 0.5f * ((float)i / (float)NPCID.Sets.TrailCacheLength[Type]);

                    Vector2 offset = NPC.Hitbox.Size() / 2;
                    if(bottomCentered)
                    {
                        offset = new Vector2(NPC.width / 2, NPC.height);
                    }
                    spriteBatch.Draw(texture, (oldPosition + offset) - Main.screenPosition, rect, trailColor, NPC.rotation, origin, NPC.scale, effects, 0f);
                }
            }

            if (inSecondPhase)
            {
                float sin = (float)Math.Sin(Main.timeForVisualEffects * 0.5f);
                var glowColor = Color.Cyan;
                glowColor.A = 0;
                Vector2 drawOffset = new Vector2(sin);
                Vector2 drawOffset2 = new Vector2(-drawOffset.X, drawOffset.Y);
                spriteBatch.Draw(texture, drawpos + drawOffset, rect, glowColor, NPC.rotation, origin, NPC.scale * 1.05f, effects, 0f);
                spriteBatch.Draw(texture, drawpos + drawOffset2, rect, glowColor, NPC.rotation, origin, NPC.scale * 1.05f, effects, 0f);
            }
            spriteBatch.Draw(texture, drawpos, rect, Color.White, NPC.rotation, origin, NPC.scale, effects, 0f);
            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            foreach (DrawData d in currentAttack.postDraws)
            {
                spriteBatch.Draw(d.texture, d.position, d.sourceRect, d.color, d.rotation, d.origin, d.scale, d.effect, 0f);
            }
            if (MinosDebug.PrintGroundCollision)
            {
                NPC.DrawDebugGroundCollision(spriteBatch);
            }
        }

        internal void ActivateShockwaveFilter(Vector2 center)
        {
            Filters.Scene.Activate("TestContent:ShockwaveFilter", center).GetShader().UseColor(rippleCount, rippleSize, rippleSpeed).UseTargetPosition(center);
            currentShockwaveTime = maxShockwaveTime;
        }

        public float PrimitiveWidthFunction(float completionRatio)
        {
            float size = 30f * telegraphTime;
            float cutoff = 0.8f;
            if (completionRatio >= cutoff)
            {
                size *= (completionRatio - cutoff) / 1f - cutoff;
                size = Math.Clamp(size, 0f, 1f);
            }
            return size;
        }
        public float PrimitiveWidthFunctionOutline(float completionRatio)
        {
            float size = 30f;
            float cutoff = 0.8f;
            if (completionRatio >= cutoff)
            {
                size *= (completionRatio - cutoff) / 1f - cutoff;
                size = Math.Clamp(size, 0f, 1f);
            }
            return size;
        }

        public Color PrimitiveColorFunction(float completionRatio)
        {
            var col = Color.Blue * 0.5f;
            float cutoff = 0.2f;
            if(completionRatio <= cutoff)
            {
                col *= (completionRatio - cutoff) / (1f - cutoff);
            }
            return col;
        }

        public Color PrimitiveColorFunctionOutline(float completionRatio)
        {
            var col = Color.Cyan * 0.3f;
            float cutoff = 0.2f;
            if (completionRatio <= cutoff)
            {
                col *= (completionRatio - cutoff) / (1f - cutoff);
            }
            return col;
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            if (telegraphTime > -1f)
            {
                //Main.NewText("Drawing?");
                var pointList = new List<Vector2> { telegraphStart };
                for(int i = 0; i < 16; i++)
                {
                    float time = (float)i / 15f;
                    pointList.Add(Vector2.Lerp(telegraphStart, telegraphEnd, time));
                }
                pointList.Add(telegraphEnd);
                PrimitiveRenderer.RenderTrail(pointList, telegraphSettings);
                PrimitiveRenderer.RenderTrail(pointList, telegraphOutlineSettings);
            }
        }
    }

    public abstract class MinosAttack
    {
        /// <summary>
        /// Total time the attack will take to do. Is not changed in the attack
        /// </summary>
        public abstract int AttackTime { get; }
        /// <summary>
        /// Current time of the attack. Increments every frame
        /// </summary>
        protected int currentTime 
        {
            get => inst.attackTimer;
            set => inst.attackTimer = value;
        }

        /// <summary>
        /// How much stamina this attack uses. 
        /// </summary>
        public abstract int StaminaCost { get; }

        public abstract AttackIdentity ID { get; }

        public bool multInitialized = false;

        public int NPCFrame;

        public List<DrawData> preDraws;
        public List<DrawData> postDraws;

        protected MinosPrime inst;

        public int moveRNG = 0;

        public NPC NPC
        {
            get => inst.NPC;
        }

        public Player Target
        {
            get
            {
                if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                {
                    NPC.TargetClosest();
                }
                return Main.player[NPC.target];
            }
        }

        public MinosAttack(MinosPrime inst)
        {
            this.inst = inst;
            preDraws = new List<DrawData>();
            postDraws = new List<DrawData>();
        }

        public static void CancelIntoAttack(MinosPrime boss, AttackIdentity cancelInto)
        {
            var attack = boss.currentAttack;
            attack.ResetNPCParams();
            attack.PostAttack();
            boss.currentAttackType = cancelInto;
            boss.clientAttack = boss.currentAttackType;
            attack = boss.currentAttack;
            //Attacks cancelled into cost no stamina

            attack.SetNewAttack();
        }

        public static void SwitchAttack(MinosPrime boss)
        {
            var attack = boss.currentAttack;
            attack.ResetNPCParams();
            attack.PostAttack();
            boss.currentAttackType = boss.nextAttack;
            boss.clientAttack = boss.currentAttackType;
            attack = boss.currentAttack;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var dict = attack.PruneAttackDict(attack.DefineAttackDict());
                var type = attack.RollNewAttack(dict);
                if (attack.Target.dead)
                {
                    type = AttackIdentity.Walk;
                }
                boss.nextAttack = type;
                boss.NPC.netUpdate = true;
            }
            if (MinosDebug.PrintAttack)
            {
                Main.NewText($"New Attack:  {boss.currentAttackType} Next Attck: {boss.nextAttack}");
            }
            boss.UseStamina(attack.StaminaCost);
            boss.currentAttack.SetNewAttack();
        }

        public void SetNewAttack()
        {
            var attack = inst.currentAttack;
            attack.currentTime = 0;
            attack.ChangeNPCParams();
            attack.PreAttack();
            attack.moveRNG = Main.rand.Next(0, 100001);
        }
        /// <summary>
        /// Determines whether or not Minos can currently cancel his attack. Runs check every update.
        /// </summary>
        /// <returns></returns>
        public virtual bool CanCancelAttack()
        {
            return false;
        }

        public virtual void SetAnimationFrame(ref MinosAnimationController.MinosSprite sprite)
        {

        }

        public virtual void SendExtraAI(BinaryWriter writer)
        {

        }

        public virtual void RecieveExtraAI(BinaryReader reader)
        {

        }

        /// <summary>
        /// Remove attacks from the dict that Minos cannot use right now (Stamina cost)
        /// </summary>
        /// <param name="orig"></param>
        /// <returns></returns>
        public Dictionary<AttackIdentity, int> PruneAttackDict(Dictionary<AttackIdentity, int> orig)
        {
            if (Main.expertMode && inst.inSecondPhase)
            {
                //In expert mode, Minos bypasses the stamina system in his second phase. Pray
                return orig;
            }
            //If we have the option to jump but can't do anything after, then don't jump
            if (orig.TryGetValue(AttackIdentity.Jump, out int _))
            {
                var check = GetInAirDict();
                bool canJump = false;
                foreach (var attackId in check.Keys)
                {
                    var attack = inst.GetAttack(attackId);
                    int cost = attack.StaminaCost;
                    if (Main.expertMode || inst.inSecondPhase)
                    {
                        cost /= 2;
                    }
                    if (cost < inst.stamina)
                    {
                        canJump = true;
                    }
                }
                if(!canJump)
                {
                    orig.Remove(AttackIdentity.Jump);
                }
            }
            var dict = new Dictionary<AttackIdentity, int>();
            foreach(var attackId in orig.Keys)
            {
                var attack = inst.GetAttack(attackId);
                int cost = attack.StaminaCost;
                if (Main.expertMode || inst.inSecondPhase)
                {
                    cost /= 2; //Use half stamina in second phase or by default in expert mode
                }
                if ((inst.currentAttack.ID == AttackIdentity.TripleHit && attackId == AttackIdentity.SeekingSnake) || inst.currentAttack.ID == AttackIdentity.Walk)
                {
                    cost = 0;
                }
                if (cost > inst.stamina)
                {
                    continue;
                }
                int chance = orig[attackId];
                dict.Add(attackId, chance);
            }

            if(dict.Keys.Count == 0)
            {
                dict.Add(AttackIdentity.Walk, 100);
            }

            return dict;
        }
        /// <summary>
        /// Add Attacks according to global parameters
        /// </summary>
        /// <param name="orig"></param>
        /// <returns></returns>
        public Dictionary<AttackIdentity, int> AppendAttackDict(Dictionary<AttackIdentity, int> orig)
        {
            if(!Target.dead && Target.IsInAir())
            {
                if(!orig.TryGetValue(AttackIdentity.Uppercut, out int value))
                {
                    orig.Add(AttackIdentity.Uppercut, 100);
                }
                if (!orig.TryGetValue(AttackIdentity.Spike, out int value2))
                {
                    orig.Add(AttackIdentity.Spike, 100);
                }
            }

            return orig;
        }

        /// <summary>
        /// Use this method to change params on the NPC for the attack. For example: Editing an NPC's gravity multiplier
        /// </summary>
        public virtual void ChangeNPCParams()
        {

        }

        public void ResetNPCParams()
        {
            inst.knockbackDir = Vector2.Zero;
            inst.knockbackStrength = 0f;
            inst.shouldDrawTrail = false;
            inst.doContactDamage = true;
            inst.canDiveCancel = false;
        }

        /// <summary>
        /// Use this method to define the attacks in whihc minos can go into after this attack ends. Called right before an attack is rolled for.
        /// </summary>
        public abstract Dictionary<AttackIdentity, int> DefineAttackDict();
        /// <summary>
        /// Called once before Attack Behavior starts being called. Use this to setup inital variables.
        /// </summary>
        public virtual void PreAttack()
        {

        }
        /// <summary>
        /// Equivalent of AI for the attack. 
        /// </summary>
        public virtual void AttackBehavior()
        {

        }
        /// <summary>
        /// Called once right before attack is switched out
        /// </summary>
        public virtual void PostAttack()
        {

        }

        /// <summary>
        /// Set Data for attional draws, drawn either in front of or behind the NPC.
        /// </summary>
        public virtual void SetDrawData()
        {

        }

        public void AttackAI()
        {
            AttackBehavior();
            if(Main.netMode != NetmodeID.Server)
            {
                SetDrawData();
                var frame = MinosAnimationController.MinosSprite.Standing;
                SetAnimationFrame(ref frame);
                inst.animController.currentSprite = frame;
            }
            inst.canDiveCancel = CanCancelAttack();
            currentTime++;
            if (MinosDebug.PrintTimer)
            {
                Main.NewText($"Time: {currentTime}");
                if(Main.netMode == NetmodeID.Server)
                {
                    Console.WriteLine($"Time: {currentTime}");
                }
            }
            if(currentTime > AttackTime)
            {
                SwitchAttack(inst);
            }
        }
        private AttackIdentity RollNewAttack(Dictionary<AttackIdentity, int> attackDict)
        {
            var rng = new LayeredRollingSystem();
            var attackOrder = new Dictionary<int, AttackIdentity>();
            int ind = 0;
            foreach (AttackIdentity i in attackDict.Keys)
            {
                attackOrder[ind] = i;
                ind++;
            }

            foreach (int i in attackDict.Values)
            {
                rng.Add(i);
            }
            int attack = rng.Roll();
            return attackOrder[attack];
        }
        protected Vector2 GetLandingPoint(Vector2 direction, float maxTime)
        {
            return GetLandingPoint(NPC.Center, direction, Target.Center, maxTime, NPC.height);
        }
        public static Vector2 GetLandingPoint(Vector2 userCenter, Vector2 direction, Vector2 targetCenter, float maxLength, int userHeight)
        {
            //Get length from point;
            Vector2 samplingPoint = userCenter;

            // Perform a laser scan to calculate where Minos should land
            float initDist = (userCenter - targetCenter).Length();

            float dist1 = initDist;
            float dist2 = dist1;

            Vector2 point1 = samplingPoint;
            Vector2 point2 = samplingPoint;

            for(int i = 0; i < 10; i++)
            {
                point1 = point2;
                dist1 = dist2;

                float[] laserScanResults = new float[10];
                CollisionUtility.LaserScanTopSolid(samplingPoint, direction, 0, maxLength, laserScanResults);
                point2 = (samplingPoint + (direction * laserScanResults[0]));

                dist2 = (point2 - targetCenter).Length();
                samplingPoint = point2 + (direction * 16f);
                //Projectile.NewProjectileDirect(null, samplingPoint, Vector2.Zero, ModContent.ProjectileType<ChipProjectile>(), 0, 0, ai0: 0).scale = 1f + (0.5f * i);
                //Main.NewText($"{point1} : {point2} : {laserScanResults[0]}");
                if(dist2 >= dist1 && point1.Y > targetCenter.Y)
                {
                    break;
                }
            }

            return point1 + new Vector2(0, (-userHeight / 2) - 12);
        }

        protected Dictionary<AttackIdentity, int> GetInAirDict(Vector2? posOverride = null)
        {
            var dict = new Dictionary<AttackIdentity, int> {
                { AttackIdentity.Uppercut, 100 },
                { AttackIdentity.Spike, 100 },
            };
            //Main.NewText($"{outer} : {inner}");
            return dict;
        }

        public void Teleport(Vector2 destination, float afterImageSpacing = 50f, bool hostile = false)
        {
            var origPos = NPC.Center;
            NPC.Center = destination;
            Vector2 dir = NPC.Center - origPos;
            float length = dir.Length();
            dir.Normalize();

            int copies = (int)(length / afterImageSpacing);

            var newTrail = new MinosTrail(60);
            newTrail.trailColor = Color.Cyan;
            newTrail.sprite = inst.animController.currentSprite;
            newTrail.direction = NPC.spriteDirection;
            var teleportDrawPositions = newTrail.positions;
            teleportDrawPositions.Clear();
            //Add line of after images to draw
            for (int i = 0; i < copies; i++)
            {
                float quotient = (float)i / (float) copies;
                teleportDrawPositions.Add(Vector2.Lerp(origPos, NPC.Center, quotient));
            }
            if (hostile)
            {
                newTrail.trailColor = Color.Red;
            }
            if(Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (hostile)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), origPos, Vector2.Zero, ModContent.ProjectileType<MinosSlamHitbox>(), inst.baseDamage + 25, 0.5f, ai0 : NPC.Center.X,  ai1 : NPC.Center.Y);
                }
            }
            if(Main.netMode != NetmodeID.SinglePlayer)
            {
                //NPC.netUpdate = true;
            }

            //Set how long the after images last
            inst.trailQueue.Add(newTrail);
            SoundEngine.PlaySound(inst.dash, NPC.Center);
        }

        public void DoScreenShake()
        {
            PunchCameraModifier modifier = new PunchCameraModifier(NPC.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 7f, 3f, 20, 10000f, inst.FullName);
            Main.instance.CameraModifiers.Add(modifier);
        }

        public void SpawnRocks(int number, float speed)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                float randOffset = (float)Math.Tau * Main.rand.NextFloat();
                for (int i = 0; i < number; i++)
                {
                    float angle = (((float)i / (float)number) * (float)Math.Tau) + randOffset;
                    Vector2 velocity = Vector2.UnitY.RotatedBy(angle) * ((speed * 0.8f) + (speed * Main.rand.NextFloat() * 0.4f));
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, ModContent.ProjectileType<MinosRock>(), inst.baseDamage, 0.5f, ai0: Main.rand.Next(0, 3));
                }
            }
        }

        public void SpawnRocksFromGround(int number, float speed, float range)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var start = new Vector2(NPC.Center.X + range, NPC.Center.Y);
                var end = new Vector2(NPC.Center.X - range, NPC.Center.Y);
                float sect = 1f / (float)number;
                for (float o = 0f; o <= 1f; o += sect)
                {
                    var pos = Vector2.Lerp(start, end, o);
                    var rot = -(float)Math.PI * o;
                    var vel = rot.ToRotationVector2() * speed;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, vel, ModContent.ProjectileType<MinosRock>(), inst.baseDamage, 0.5f, ai0: Main.rand.Next(0, 3));
                }
            }
        }
        public void SpawnShockwave(float speed)
        {
            var vel = Vector2.UnitX * speed;
            if(Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.GetFeetPosition(), vel, ModContent.ProjectileType<Shockwave>(), inst.baseDamage + 25, 0.5f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.GetFeetPosition(), -vel, ModContent.ProjectileType<Shockwave>(), inst.baseDamage + 25, 0.5f);
            }
        }

        public void SpawnBlueVisualSnake(Vector2 parentPos, int lifetime = 30)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center + parentPos, Vector2.Zero, ModContent.ProjectileType<ViperBlue>(), 0, 0, ai0: NPC.whoAmI, ai1: parentPos.X, ai2: parentPos.Y);
                proj.timeLeft = lifetime;
                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj.identity);
                }
            }
        }

        public void TrySpawnShockwaveEffect()
        {
            if (Main.netMode != NetmodeID.Server && !Filters.Scene["TestContent:ShockwaveFilter"].IsActive())
            {
                inst.ActivateShockwaveFilter(NPC.Center);
            }
        }
        public (DrawData, DrawData) DrawDefaultAttackIndicator(float time)
        {
            return DrawAttackIndicator(NPC.Center + (new Vector2(30f * NPC.spriteDirection, -30f)), time, Color.Cyan);
        }
        public static (DrawData, DrawData) DrawAttackIndicator(Vector2 position, float time, Color color)
        {
            color.A = 100;
            var shineTexture = TextureAssets.Extra[98].Value;
            var rect = new Rectangle(0, 0, shineTexture.Width, shineTexture.Height);
            Vector2 origin = rect.Size() / 2;

            float scale = MathHelper.Hermite(0f, 6.7f, 0f, 0f, time);
            return (new DrawData(shineTexture, position - Main.screenPosition, rect, color, time, origin, scale, SpriteEffects.None),
                new DrawData(shineTexture, position - Main.screenPosition, rect, color, time + (float)Math.PI / 2, origin, scale, SpriteEffects.None));
        }

        public void DoAttackIndicator(int minTime, int maxTime)
        {
            if (currentTime >= minTime && currentTime <= maxTime)
            {
                float time = (float)(currentTime - minTime) / (float)(maxTime - minTime);
                var draws = DrawDefaultAttackIndicator(time);
                postDraws = new List<DrawData> { draws.Item1, draws.Item2 };
            }
            else
            {
                postDraws.Clear();
            }
        }
        public void UpdateTelegraph(int start, int end, Vector2 startPoint, Vector2 endPoint)
        {
            if (currentTime >= start && currentTime <= end)
            {
                float time = (float)(currentTime - start) / (float)(end - start);
                inst.telegraphStart = startPoint;
                inst.telegraphEnd = endPoint;
                inst.telegraphTime = time;
            }
            else
            {
                inst.telegraphTime = -1f;
            }
        }

        public void SendSync(BinaryWriter writer)
        {
            writer.Write(currentTime);
        }

        public void RecieveSync(BinaryReader reader)
        {
            currentTime = reader.ReadInt32();
        }
    }

    public class MinosWalk : MinosAttack
    {
        float speed = 2.5f;
        float inertia = 30f;
        public override int AttackTime => 120;

        public override int StaminaCost => 0;

        public override AttackIdentity ID => AttackIdentity.Walk;

        public MinosWalk(MinosPrime inst) : base(inst)
        {

        }

        public override Dictionary<AttackIdentity, int> DefineAttackDict()
        {
            if (Target.dead)
            {
                return new Dictionary<AttackIdentity, int>
                {
                    {AttackIdentity.Walk, 100 }
                };
            }
            return new Dictionary<AttackIdentity, int> {
                { AttackIdentity.TripleHit, 100 },
                { AttackIdentity.SeekingSnake, 80 },
                { AttackIdentity.QuadHit, 100 },
                { AttackIdentity.Judgement, 75 },
                { AttackIdentity.Jump, 80 },
            };
        }

        public override void SetAnimationFrame(ref MinosAnimationController.MinosSprite sprite)
        {
            if (NPC.IsInAir())
            {
                sprite = MinosAnimationController.MinosSprite.Falling;
            }
            else
            {
                sprite = MinosAnimationController.MinosSprite.Walking;
            }
        }
        public override void AttackBehavior()
        {
            if (Target.dead)
            {
                NPC.velocity.X = NPC.spriteDirection * speed;
                NPC.velocity.Y -= 0.5f;
                NPC.GravityMultiplier *= 0f;
                NPC.EncourageDespawn(10);
                return;
            }

            //Detect if Minos is in the ground, and raise him up if he is
            if (NPC.IsInGround())
            {
                NPC.position.Y -= 16;
            }

            int dir = Math.Sign(Target.Center.X - NPC.Center.X);
            var max =  dir * speed;

            NPC.velocity.X = (NPC.velocity.X * (inertia - 1) + max) / inertia;

            NPC.spriteDirection = Math.Sign(dir);
        }
    }

    public class MinosTriple : MinosAttack
    {
        public override int AttackTime => 94;
        public override int StaminaCost => 2;

        public override AttackIdentity ID => AttackIdentity.TripleHit;

        public virtual MinosVoiceLineController.MinosVoiceID voiceToPlay => MinosVoiceLineController.MinosVoiceID.Prepare;

        public int initWindup = 15;
        public int halves = 40;
        public int cutoff = 20;

        public int attackDir = 1;
        public float attackSpacing
        {
            get
            {
                if (Main.expertMode)
                {
                    return 300f;
                }
                return 250f;
            }
        }
        public float postAttackSpacing
        {
            get
            {
                if (Main.expertMode)
                {
                    return 250f;
                }
                return 100f;
            }
        }

        public Vector2 teleport = Vector2.Zero;
        public Vector2 initPos = Vector2.Zero;

        public MinosTriple(MinosPrime inst) : base(inst)
        {

        }

        public override Dictionary<AttackIdentity, int> DefineAttackDict()
        {
            return new Dictionary<AttackIdentity, int> {
                { AttackIdentity.SeekingSnake, 100 }
            };
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(attackDir);
        }

        public override void RecieveExtraAI(BinaryReader reader)
        {
            attackDir = reader.ReadInt32(); 
            //SetTargetParameters();
        }

        public void SetTargetParameters()
        {
            var tar = Target.Center;
            var vecDir = new Vector2(attackDir, 0);
            initPos = tar + -(vecDir * attackSpacing);
        }

        public override void PreAttack()
        {
            SetTargetParameters();
            NPC.velocity = Vector2.Zero;
            if (!Main.dedServ)
            {
                inst.voiceController.PlayVoiceLine(voiceToPlay, NPC.Center);
            }
        }

        public override void ChangeNPCParams()
        {
            inst.shouldDrawTrail = true;
        }

        public override void SetAnimationFrame(ref MinosAnimationController.MinosSprite sprite)
        {
            if(currentTime < initWindup)
            {
                sprite = MinosAnimationController.MinosSprite.Standing;
            }
            else
            {
                float strikeTime = (currentTime - initWindup) % halves;
                if(strikeTime < cutoff)
                {
                    sprite = MinosAnimationController.MinosSprite.PunchWindup;
                }
                else
                {
                    sprite = MinosAnimationController.MinosSprite.Punch;
                }
            }
        }
        public override void AttackBehavior()
        {
            NPC.GravityMultiplier *= 0f;

            if (currentTime >= initWindup)
            {
                float strikeTime = (currentTime - initWindup) % halves;
                //Main.NewText($"StrikeTime = {strikeTime}");
                if (strikeTime == 0)
                {
                    attackDir *= -1;
                    SetTargetParameters();
                    Teleport(initPos);
                    //NPC.Center = new Vector2(teleport.X + attackDir * attackSpacing, teleport.Y);
                }
                if(inst.inSecondPhase && strikeTime < cutoff)
                {
                    SetTargetParameters();
                    NPC.MoveToWithInertia(initPos, 5f, 8f);
                }
                NPC.spriteDirection = attackDir;
                if (strikeTime == cutoff)
                {
                    SoundEngine.PlaySound(inst.swing, NPC.Center);
                    SpawnBlueVisualSnake(new Vector2(50 * NPC.spriteDirection, -8), halves - cutoff);
                    var vecDir = new Vector2(attackDir, 0);
                    initPos = NPC.Center;
                    teleport = NPC.Center + (vecDir * postAttackSpacing) + (vecDir * attackSpacing);
                }
                if (strikeTime >= cutoff)
                {
                    NPC.velocity = Vector2.Zero;
                    float time = (strikeTime - cutoff) / ((float)halves - cutoff);
                    //Main.NewText($"StrikeTime = {strikeTime}, Time = {time}");
                    NPC.Center = Vector2.Lerp(initPos, teleport, MathHelper.Hermite(0, 3f, 1f, 0f, time));
                }
            }
        }
    }

    public class MinosQuad : MinosTriple
    {
        public override int AttackTime => 134;
        public override int StaminaCost => 4;

        public override MinosVoiceLineController.MinosVoiceID voiceToPlay => MinosVoiceLineController.MinosVoiceID.ThyEnd;

        public override AttackIdentity ID => AttackIdentity.QuadHit;
        public MinosQuad(MinosPrime inst) : base(inst)
        {

        }

        public override Dictionary<AttackIdentity, int> DefineAttackDict()
        {
            return new Dictionary<AttackIdentity, int> {
                { AttackIdentity.GroundSlam, 100 }
            };
        }
    }

    public class MinosSnake : MinosAttack
    {
        public override int AttackTime => 60;
        public override int StaminaCost => 1;

        public override AttackIdentity ID => AttackIdentity.SeekingSnake;

        public int delay = 30;
        public MinosSnake(MinosPrime inst) : base(inst)
        {

        }

        public override bool CanCancelAttack()
        {
            return currentTime >= 50;
        }

        public override void SetDrawData()
        {
            var texture = TextureAssets.Extra[174].Value;
            var color = Color.Yellow;
            float time = (float)currentTime / (float)AttackTime;
            Vector2 position = NPC.Center;
            color.A = 100;
            var rect = new Rectangle(0, 0, texture.Width, texture.Height);
            Vector2 origin = rect.Size() / 2;

            float scale = MathHelper.Hermite(0f, 2f, 0f, -2f, time) * 2;
            postDraws = new List<DrawData> {new DrawData(texture, position - Main.screenPosition, rect, color, time, origin, scale, SpriteEffects.None),
                new DrawData(texture, position - Main.screenPosition, rect, color, - time + (float)Math.PI / 2, origin, scale * 1.5f, SpriteEffects.None) };
        }

        public override void PreAttack()
        {
            int dir = Math.Sign(NPC.Center.GetVectorPointingTo(Target.Center).X);
            NPC.spriteDirection = dir;
        }

        public override void SetAnimationFrame(ref MinosAnimationController.MinosSprite sprite)
        {
            if (currentTime < delay)
            {
                sprite = MinosAnimationController.MinosSprite.PunchWindup;
            }
            else
            {
                sprite = MinosAnimationController.MinosSprite.Punch;
            }
        }
        public override void AttackBehavior()
        {
            NPC.GravityMultiplier *= 0f;

            NPC.velocity = Vector2.Zero;

            if (Main.netMode != NetmodeID.Server && currentTime < delay)
            { 
                float time = (float)currentTime / (float)delay;
                Vector2 vel = (time * (float)Math.Tau).ToRotationVector2() * 10f;
                Dust.NewDustPerfect(NPC.Center, DustID.YellowStarDust, vel).scale = 2f;
            }
            else if(currentTime == delay)
            {
                if(Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 dir = Target.Center - NPC.Center;
                    dir.Normalize();
                    int count = 0;
                    foreach (var proj in Main.ActiveProjectiles)
                    {
                        if(proj.type == ModContent.ProjectileType<Viper>())
                        {
                            count++;
                        }
                    }
                    if(count >= 2)
                    {
                        for(int i = -1; i < 2; i++)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (dir.RotatedBy((MathHelper.Pi/6f) * i)) * 15f, ModContent.ProjectileType<ViperSmall>(), inst.baseDamage - 10, 0.5f);
                        }
                    }
                    else
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, dir * 15f, ModContent.ProjectileType<Viper>(), inst.baseDamage, 0.5f);
                    }
                }
                if (!Main.dedServ)
                {
                    DustUtils.CreateDustBurstCirclePerfect(DustID.YellowStarDust, NPC.Center, 5f, 15);
                    SoundEngine.PlaySound(inst.swing, NPC.Center);
                }
            }
        }
        public override Dictionary<AttackIdentity, int> DefineAttackDict()
        {
            if (!NPC.IsInAir())
            {
                var dict = new Dictionary<AttackIdentity, int> {
                    { AttackIdentity.TripleHit, 100 },
                    { AttackIdentity.QuadHit, 100 },
                    { AttackIdentity.Judgement, 75 },
                    { AttackIdentity.Jump, 80 },
                };
                dict = AppendAttackDict(dict);
                return dict;
            }
            else
            {
                return GetInAirDict();
            }
        }
    }

    public class MinosSlam : MinosAttack
    {
        public MinosSlam(MinosPrime inst) : base(inst)
        {

        }

        public override int AttackTime => 100;
        public override int StaminaCost => 0;

        public override AttackIdentity ID => AttackIdentity.GroundSlam;

        public float heightOffset = 300f;
        public int trackingCutoff = 45;
        public int slamTime = 60;

        public Vector2 targetPos = Vector2.Zero;
        public override Dictionary<AttackIdentity, int> DefineAttackDict()
        {
            var dict = new Dictionary<AttackIdentity, int> {
                    { AttackIdentity.TripleHit, 100 },
                    { AttackIdentity.SeekingSnake, 80},
                    { AttackIdentity.Judgement, 75 },
                    { AttackIdentity.Jump, 80 },
                };
            dict = AppendAttackDict(dict);
            return dict;
        }

        public override void ChangeNPCParams()
        {
            inst.shouldDrawTrail = true;
        }

        public override void PreAttack()
        {
            var teleportPoint = Target.Center + new Vector2(0, -heightOffset);
            NPC.velocity = Vector2.Zero;
            Teleport(teleportPoint);
        }

        public override void SetDrawData()
        {
            DoAttackIndicator(trackingCutoff, slamTime);
        }

        public override void SetAnimationFrame(ref MinosAnimationController.MinosSprite sprite)
        {
            if (currentTime < trackingCutoff)
            {
                sprite = MinosAnimationController.MinosSprite.Falling;
            }
            else if (currentTime < slamTime)
            {
                sprite = MinosAnimationController.MinosSprite.SlamPose;
            }
            else
            {
                sprite = MinosAnimationController.MinosSprite.Crouching;
            }
        }

        public override void AttackBehavior()
        {
            //Track player position
            NPC.GravityMultiplier *= 0;
            if(currentTime < trackingCutoff)
            {
                var target = Target.Center + new Vector2(0, -heightOffset);
                //NPC.Center = target;
                NPC.MoveToWithInertia(target, 16f, 10f);
                int dir = Math.Sign(NPC.Center.GetVectorPointingTo(Target.Center).X);
                NPC.spriteDirection = dir;
            }
            if(currentTime == trackingCutoff)
            {
                NPC.velocity = Vector2.Zero;
                targetPos = GetLandingPoint(Vector2.UnitY, 2500f);
                SoundEngine.PlaySound(MinosPrime.attackPlink, NPC.Center);
            }
            else if(currentTime == slamTime) 
            {
                Teleport(targetPos, hostile : true);
                NPC.velocity = Vector2.Zero;
                DoScreenShake();
                SpawnRocksFromGround(6, 20f, 100f);
                SpawnShockwave(15f);
                SoundEngine.PlaySound(inst.slam, NPC.Center);
            }
            UpdateTelegraph(trackingCutoff, slamTime, NPC.Center, NPC.ConvertCenterToBottomPosition(targetPos));
            //Main.NewText(NPC.Center);
        }
    }

    public class MinosJudgement : MinosAttack
    {
        public MinosJudgement(MinosPrime inst) : base(inst)
        {

        }
        public override int AttackTime => 150;
        public override int StaminaCost => 4;

        public override AttackIdentity ID => AttackIdentity.Judgement;

        public int initFrameTime = 20;

        public int windup = 70;
        public int prehit = 85;
        public int tohit = 90;

        public float offset = 150f;

        public Vector2 initPos = Vector2.Zero;
        public Vector2 targetPos = Vector2.Zero;

        public override bool CanCancelAttack()
        {
            return currentTime >= 120;
        }

        public override Dictionary<AttackIdentity, int> DefineAttackDict()
        {
            if (!NPC.IsInAir())
            {
                var dict = new Dictionary<AttackIdentity, int> {
                    { AttackIdentity.TripleHit, 100 },
                    { AttackIdentity.QuadHit, 100 },
                    { AttackIdentity.SeekingSnake, 80},
                    { AttackIdentity.Jump, 80 },
                };
                dict = AppendAttackDict(dict);
                return dict;
            }
            else
            {
                return GetInAirDict();
            }
        }

        public override void PreAttack()
        {
            if (!Main.dedServ)
            {
                inst.voiceController.OverwriteVoiceLine(MinosVoiceLineController.MinosVoiceID.Judgement, NPC.Center);
            }
        }

        public override void ChangeNPCParams()
        {
            inst.shouldDrawTrail = true;
            inst.doContactDamage = false;
        }

        public override void SetDrawData()
        {
            DoAttackIndicator(windup, prehit);
        }

        public override void SetAnimationFrame(ref MinosAnimationController.MinosSprite sprite)
        {
            if(currentTime < initFrameTime)
            {
                sprite = MinosAnimationController.MinosSprite.Crouching;
            }
            else if(currentTime < windup)
            {
                sprite = MinosAnimationController.MinosSprite.JudgementPose;
            }
            else if(currentTime < tohit)
            {
                sprite = MinosAnimationController.MinosSprite.JudgementPreKick;
            }
            else
            {
                sprite = MinosAnimationController.MinosSprite.JudgementPostKick;
            }
        }

        public override void AttackBehavior()
        {
            NPC.GravityMultiplier *= 0f;
            NPC.velocity = Vector2.Zero;
            if (currentTime == windup)
            {
                int dir = Math.Sign(NPC.Center.GetVectorPointingTo(Target.Center).X);
                NPC.spriteDirection = dir;
                targetPos = Target.Center;
                initPos = Target.Center + new Vector2(-dir * offset, 0);
                Teleport(initPos);
                SoundEngine.PlaySound(MinosPrime.attackPlink, NPC.Center);
            }
            else if (currentTime > prehit && currentTime < tohit)
            {
                NPC.Center = Vector2.Lerp(initPos, targetPos, MathHelper.SmoothStep(0f, 1f, (currentTime - prehit) / ((float)tohit - prehit)));
            }
            else if(currentTime == tohit)
            {
                NPC.Center = targetPos;
                DoScreenShake();
                TrySpawnShockwaveEffect();
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<JudgementExplosion>(), inst.baseDamage + 45, 0.5f);
                    SpawnRocks(6, 20f);
                }
                SoundEngine.PlaySound(inst.judgement, NPC.Center);
            }
        }
    }

    public class MinosJump : MinosAttack
    {
        public MinosJump(MinosPrime inst) : base(inst)
        {

        }
        public override int AttackTime => 30;
        public override int StaminaCost => 1;
        public override AttackIdentity ID => AttackIdentity.Jump;

        public float heightOffset = 300f;

        public Vector2 initialPos;
        public Vector2 heightPos;

        public override bool CanCancelAttack()
        {
            return currentTime >= 25;
        }

        public override Dictionary<AttackIdentity, int> DefineAttackDict()
        {
            return GetInAirDict(NPC.Center + new Vector2(0, -heightOffset));
        }

        public override void ChangeNPCParams()
        {
            inst.shouldDrawTrail = true;
        }

        public override void PreAttack()
        {
            initialPos = NPC.Center;
            heightPos = initialPos + new Vector2(0, -heightOffset);
        }

        public override void SetAnimationFrame(ref MinosAnimationController.MinosSprite sprite)
        {
            sprite = MinosAnimationController.MinosSprite.Falling;
        }

        public override void AttackBehavior()
        {
            NPC.velocity = Vector2.Zero;
            NPC.GravityMultiplier *= 0;

            NPC.Center = Vector2.Lerp(initialPos, heightPos, MathHelper.Hermite(0, 3f, 1f, 0f, (float)currentTime / (float)AttackTime));
        }
    }

    public class MinosSpike : MinosAttack
    {
        public MinosSpike(MinosPrime inst) : base(inst)
        {

        }
        public override int AttackTime => 50;

        public override int StaminaCost => 2;

        public override AttackIdentity ID => AttackIdentity.Spike;

        public int windup = 25;
        public int cooldown = 35;
        public float offset = 300f;
        public float postOffset = 200f;

        public int dir = -1;

        public Vector2 targetPos;
        public Vector2 offsetPos;

        public override bool CanCancelAttack()
        {
            return currentTime >= 40;
        }

        public override Dictionary<AttackIdentity, int> DefineAttackDict()
        {
            var cancelAttacks = new Dictionary<AttackIdentity, int> {
                    { AttackIdentity.TripleHit, 100 },
                    { AttackIdentity.QuadHit, 100 },
                    { AttackIdentity.SeekingSnake, 80},
                    { AttackIdentity.Jump, 80 },
            };
            if (NPC.IsInAir())
            {
                var dict = GetInAirDict();
                foreach (var key in dict.Keys)
                {
                    if (key != AttackIdentity.Spike)
                    {
                        dict[key] = (dict[key] / 2);
                    }
                }
                cancelAttacks.MergeLeft(dict);
            }
            return cancelAttacks;
        }

        public override void SetDrawData()
        {
            DoAttackIndicator(0, windup);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(dir);
        }

        public override void RecieveExtraAI(BinaryReader reader)
        {
            dir = reader.ReadInt32();
        }

        public override void ChangeNPCParams()
        {
            inst.knockbackDir = Vector2.UnitY;
            inst.knockbackStrength = 10f;
            inst.shouldDrawTrail = true;
            inst.doContactDamage = false;
        }

        public override void PreAttack()
        {
            targetPos = Target.Center;
            dir *= -1;
            NPC.spriteDirection = -dir;
            offsetPos = targetPos + new Vector2(dir * offset, 0);
            Teleport(offsetPos);
            NPC.velocity = Vector2.Zero;
            SoundEngine.PlaySound(MinosPrime.attackPlink, NPC.Center);
        }

        public override void SetAnimationFrame(ref MinosAnimationController.MinosSprite sprite)
        {
            if(currentTime < windup)
            {
                sprite = MinosAnimationController.MinosSprite.PunchWindup;
            }
            else
            {
                sprite = MinosAnimationController.MinosSprite.Punch;
            }
        }

        public override void AttackBehavior()
        {
            NPC.GravityMultiplier *= 0;
            if(inst.inSecondPhase && currentTime < windup)
            {
                targetPos = Target.Center;
                offsetPos = targetPos + new Vector2(dir * offset, 0);
                NPC.MoveToWithInertia(offsetPos, 8f, 8f);
            }
            if(currentTime == windup)
            {
                inst.doContactDamage = true;
                targetPos = NPC.Center + new Vector2(-dir * (postOffset + offset), 0);
                offsetPos = NPC.Center;
                SoundEngine.PlaySound(inst.swing, NPC.Center);
                SpawnBlueVisualSnake(new Vector2(50 * NPC.spriteDirection, -8), AttackTime - windup);
            }
            if (currentTime > windup && currentTime < cooldown)
            {
                NPC.velocity = Vector2.Zero;
                NPC.Center = Vector2.Lerp(offsetPos, targetPos, MathHelper.Hermite(0, 3f, 1f, 0f, (float)(currentTime - windup) / (float)(AttackTime - windup)));
            }
        }
    }

    public class MinosUppercut : MinosAttack
    {
        public MinosUppercut(MinosPrime inst) : base(inst)
        {

        }
        public override int AttackTime => 50;
        public override int StaminaCost => 2;

        public override AttackIdentity ID => AttackIdentity.Uppercut;

        public int windup = 25;
        public int cooldown = 35;

        public float hOffset = 15f;
        public float vPreOffset = 200f;
        public float vOffset = 400f;

        public Vector2 targetPos;
        public Vector2 offsetPos;

        public override bool CanCancelAttack()
        {
            return currentTime >= 40;
        }

        public override Dictionary<AttackIdentity, int> DefineAttackDict()
        {
            var cancelAttacks = new Dictionary<AttackIdentity, int> {
                    { AttackIdentity.TripleHit, 100 },
                    { AttackIdentity.QuadHit, 100 },
                    { AttackIdentity.SeekingSnake, 80},
                    { AttackIdentity.Jump, 80 },
            };
            if (NPC.IsInAir())
            {
                var dict = GetInAirDict();
                foreach (var key in dict.Keys)
                {
                    if (key != AttackIdentity.Uppercut)
                    {
                        dict[key] = (dict[key] / 2);
                    }
                }
                cancelAttacks.MergeLeft(dict);
            }
            return cancelAttacks;
        }

        public override void SetDrawData()
        {
            DoAttackIndicator(0, windup);
        }

        public override void ChangeNPCParams()
        {
            inst.knockbackDir = -Vector2.UnitY;
            inst.knockbackStrength = 10f;
            inst.shouldDrawTrail = true;
            inst.doContactDamage = false;
        }
        public override void PreAttack()
        {
            var targetLoc = Target.Center;
            int dir = NPC.GetFacingDirection(targetLoc);
            NPC.spriteDirection = dir;
            offsetPos = Target.Center + new Vector2(dir * hOffset, vPreOffset);
            targetPos = offsetPos + new Vector2(0, -vOffset);
            Teleport(offsetPos);
            NPC.velocity = Vector2.Zero;
            SoundEngine.PlaySound(MinosPrime.attackPlink, NPC.Center);
        }

        public override void SetAnimationFrame(ref MinosAnimationController.MinosSprite sprite)
        {
            if (currentTime < windup)
            {
                sprite = MinosAnimationController.MinosSprite.PunchWindup;
            }
            else
            {
                sprite = MinosAnimationController.MinosSprite.Uppercut;
            }
        }
        public override void AttackBehavior()
        {
            NPC.GravityMultiplier *= 0;
            if (inst.inSecondPhase && currentTime < windup)
            {
                offsetPos = Target.Center + new Vector2(NPC.spriteDirection * hOffset, vPreOffset);
                targetPos = offsetPos + new Vector2(0, -vOffset);
                NPC.MoveToWithInertia(offsetPos, 8f, 8f);
            }
            if (currentTime == windup)
            {
                inst.doContactDamage = true;
                offsetPos = NPC.Center;
                targetPos = offsetPos + new Vector2(0, -vOffset);
                SoundEngine.PlaySound(inst.swing, NPC.Center);
                SpawnBlueVisualSnake(new Vector2(15 * NPC.spriteDirection, -55), AttackTime - windup);
            }
            if (currentTime > windup && currentTime < cooldown)
            {
                NPC.velocity = Vector2.Zero;
                NPC.Center = Vector2.Lerp(offsetPos, targetPos, MathHelper.Hermite(0, 3f, 1f, 0f, ((float)currentTime - windup) / ((float)cooldown - windup)));
            }
        }
    }

    public class MinosDiveAimed : MinosAttack
    {
        public MinosDiveAimed(MinosPrime inst) : base(inst)
        {

        }
        public override int AttackTime => 60;

        public override int StaminaCost => 2;

        public override AttackIdentity ID => AttackIdentity.DiveAimed;

        public int windup = 30;

        public Vector2 targetPos;
        public override Dictionary<AttackIdentity, int> DefineAttackDict()
        {
            return new Dictionary<AttackIdentity, int> {
                { AttackIdentity.TripleHit, 100 },
                { AttackIdentity.SeekingSnake, 80 },
                { AttackIdentity.QuadHit, 100 },
                { AttackIdentity.Judgement, 75 },
                { AttackIdentity.Jump, 80 },
            };
        }

        public override void PreAttack()
        {
            targetPos = GetLandingPoint(NPC.Center.GetVectorPointingTo(Target.Center), 1000f);
            int dir = NPC.GetFacingDirection(Target.Center);
            NPC.spriteDirection = dir;
            if (!Main.dedServ)
            {
                SoundEngine.PlaySound(MinosPrime.attackPlink, NPC.Center);
                inst.voiceController.PlayVoiceLine(MinosVoiceLineController.MinosVoiceID.Die, NPC.Center);
            }
            
        }

        public override void SetDrawData()
        {
            DoAttackIndicator(0, windup);
        }

        public override void SetAnimationFrame(ref MinosAnimationController.MinosSprite sprite)
        {
            if (currentTime < windup)
            {
                sprite = MinosAnimationController.MinosSprite.SlamPose;
            }
            else
            {
                sprite = MinosAnimationController.MinosSprite.Crouching;
            }
        }

        public override void AttackBehavior()
        {
            NPC.GravityMultiplier *= 0;
            NPC.velocity = Vector2.Zero;
            UpdateTelegraph(0, windup, NPC.Center, NPC.ConvertCenterToBottomPosition(targetPos));
            if(currentTime == windup)
            {
                Teleport(targetPos, hostile: true);
                DoScreenShake();
                SpawnShockwave(15f);
                SoundEngine.PlaySound(inst.slam, NPC.Center);
            }
        }
    }

    public class MinosDiveDown : MinosAttack
    {
        public MinosDiveDown(MinosPrime inst) : base(inst)
        {

        }
        public override int AttackTime => 60;

        public override int StaminaCost => 2;

        public override AttackIdentity ID => AttackIdentity.DiveDown;

        public int windup = 30;

        public Vector2 targetPos;
        public override Dictionary<AttackIdentity, int> DefineAttackDict()
        {
            return new Dictionary<AttackIdentity, int> {
                { AttackIdentity.TripleHit, 100 },
                { AttackIdentity.SeekingSnake, 80 },
                { AttackIdentity.QuadHit, 100 },
                { AttackIdentity.Judgement, 75 },
                { AttackIdentity.Jump, 80 },
            };
        }

        public override void PreAttack()
        {
            targetPos = GetLandingPoint(Vector2.UnitY, 500f);
            int dir = NPC.GetFacingDirection(Target.Center);
            NPC.spriteDirection = dir;
            if (!Main.dedServ)
            {
                SoundEngine.PlaySound(MinosPrime.attackPlink, NPC.Center);
                inst.voiceController.PlayVoiceLine(MinosVoiceLineController.MinosVoiceID.Crushed, NPC.Center);
            }
        }

        public override void SetDrawData()
        {
            DoAttackIndicator(0, windup);
        }
        public override void SetAnimationFrame(ref MinosAnimationController.MinosSprite sprite)
        {
            if (currentTime < windup)
            {
                sprite = MinosAnimationController.MinosSprite.SlamPose;
            }
            else
            {
                sprite = MinosAnimationController.MinosSprite.Crouching;
            }
        }
        public override void AttackBehavior()
        {
            NPC.GravityMultiplier *= 0;
            NPC.velocity = Vector2.Zero;
            UpdateTelegraph(0, windup, NPC.Center, NPC.ConvertCenterToBottomPosition(targetPos));
            if (currentTime == windup)
            {
                Teleport(targetPos, hostile : true);
                DoScreenShake();
                SpawnShockwave(15f);
                SoundEngine.PlaySound(inst.slam, NPC.Center);
            }
        }
    }

    public class MinosDebug : ModSystem
    {
        public static bool PrintTimer = false;
        public static bool PrintAttack = false;
        public static bool PrintGroundCollision = false;
    }

    public class MinosTrail
    {
        public List<Vector2> positions;
        public Color trailColor;
        public int timeLeft;
        public int maxTime;
        public int direction;
        public MinosAnimationController.MinosSprite sprite;
        public MinosTrail(int time)
        {
            positions = new List<Vector2>();
            timeLeft = time;
            maxTime = time;
        }
    }

    public class MinosVoiceLineController : ModSystem
    {
        public enum MinosVoiceID
        {
            Intro,
            Outro,
            Die,
            Prepare,
            Judgement,
            ThyEnd,
            Crushed,
            Weak,
            Weed
        }

        public Dictionary<MinosVoiceID,(SoundStyle, SoundStyle?)> sounds;

        public SlotId? currentSlotID;
        public void PlayVoiceLine(MinosVoiceID id, Vector2 Position)
        {
            if (GetCurrentVoiceLine(out var sound))
            {
                return; //Don't allow voice line spam;
            }
            var tuple = sounds[id];
            if(tuple.Item2 != null)
            {
                currentSlotID = Main.rand.NextBool() ? SoundEngine.PlaySound(tuple.Item1, Position) : SoundEngine.PlaySound(tuple.Item2, Position);
            }
            else
            {
                currentSlotID = SoundEngine.PlaySound(tuple.Item1, Position);
            }
        }
        public bool GetCurrentVoiceLine(out ActiveSound? sound)
        {
            sound = null;
            if(currentSlotID != null)
            {
                var slot = (SlotId)currentSlotID;
                if (SoundEngine.TryGetActiveSound(slot, out sound))
                {
                    return true;
                }
            }
            return false;
        }
        public void UpdateVoiceLinePosition(Vector2 pos)
        {
            if(GetCurrentVoiceLine(out var sound))
            {
                sound.Position = pos;
            }
        }

        public void OverwriteVoiceLine(MinosVoiceID id, Vector2 Position)
        {
            if(GetCurrentVoiceLine(out var sound))
            {
                sound.Stop();
                currentSlotID = null;
            }
            PlayVoiceLine(id, Position);
        }

        public override void OnModLoad()
        {
            sounds = new Dictionary<MinosVoiceID, (SoundStyle, SoundStyle?)>();
            for(int i = 0; i < 8; i++)
            {
                var id = (MinosVoiceID)i;
                if (id == MinosVoiceID.Die || id == MinosVoiceID.Judgement || id == MinosVoiceID.Prepare || id == MinosVoiceID.ThyEnd) 
                {
                    sounds.Add(id,(LoadSoundStyle(id.ToString() + "1"), LoadSoundStyle(id.ToString() + "2")));
                }
                else
                {
                    sounds.Add(id, (LoadSoundStyle(id.ToString()), null));
                }
            }
        }

        public static SoundStyle LoadSoundStyle(string fileName)
        {
            return new SoundStyle(ModUtils.GetSoundFileLocation($"Minos/Voice/{fileName}")){
                Volume = 0.6f,
                MaxInstances = 1,
                PlayOnlyIfFocused = true
            };
        }
    }

    public class MinosAnimationController
    {
        public enum MinosSprite
        {
            Standing,
            Falling,
            Crouching,
            SlamPose,
            PunchWindup,
            Punch,
            JudgementPose,
            JudgementPreKick,
            JudgementPostKick,
            Uppercut,
            AssOut,
            Walking,
            PreDeath,
            Death
        }

        public MinosAnimationController()
        {
            Load();
        }

        public Dictionary<MinosSprite, Asset<Texture2D>> sprites;
        public MinosSprite currentSprite = MinosSprite.Standing;

        private int internalTime = 0;
        private int frameTime = 20;
        private int internalFrame = 0;
        private int maxFrames = 4;
        public void Update()
        {
            if(currentSprite == MinosSprite.Walking)
            {
                internalTime++;
                if(internalTime >= frameTime)
                {
                    internalTime = 0;
                    internalFrame = (internalFrame + 1) % maxFrames;
                }
            }
        }
        public void Load()
        {
            sprites = new Dictionary<MinosSprite, Asset<Texture2D>>();
            for(int i = 0; i < 14; i++)
            {
                var asset = ModContent.Request<Texture2D>("TestContent/NPCs/Minos/BossSprites/m" + i.ToString());
                sprites.Add((MinosSprite)i, asset);
            }
        }
        public (Texture2D, Rectangle) GetAnimData()
        { 
            if(currentSprite != MinosSprite.Walking)
            {
                var text = sprites[currentSprite].Value;
                return (text, text.Bounds);
            }
            else
            {
                var text = sprites[currentSprite].Value;
                var rect = sprites[currentSprite].Frame(1, 4, 0, internalFrame);
                return (text, rect);
            }
        }

        public (Texture2D, Rectangle) GetAnimData(MinosSprite sprite)
        {
            if (sprite != MinosSprite.Walking)
            {
                var text = sprites[sprite].Value;
                return (text, text.Bounds);
            }
            else
            {
                var text = sprites[sprite].Value;
                var rect = sprites[sprite].Frame(1, 4, 0, internalFrame);
                return (text, rect);
            }
        }
    }
    public class MinosMusicEffect : BaseMusicSceneEffect
    {
        public override int NPCType => ModContent.NPCType<MinosPrime>();
        public override int ModMusic => MusicLoader.GetMusicSlot(Mod, "Assets/Music/MinosLoop");
        public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;

        public override int SetMusic()
        {
            Main.musicFade[MusicLoader.GetMusicSlot(Mod, "Assets/Music/MinosIntro")] = 0f;
            return ModMusic;
        }

        public override bool AdditionalCheck()
        {
            return BossNPCGlobals.minos != -1;
        }

        public override void SpecialVisuals(Player player, bool isActive)
        {
            player.ManageSpecialBiomeVisuals("TestContent:MinosFight", isActive);
        }
    }
}
