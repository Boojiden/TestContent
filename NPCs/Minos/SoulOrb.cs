using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Effects.Music;
using TestContent.Global.NPC;
using TestContent.NPCs.Minos.Projectiles;
using TestContent.UI;
using TestContent.Utility;
using TextContent.Effects.Graphics.Primitives;

namespace TestContent.NPCs.Minos
{
    public class SoulOrb : ModNPC
    { 
        public Vector2 initPos;

        public static float UpperOffset = 250f;

        public SoundStyle introVoice;

        public int ballSwitch = 600;
        public int UIPopup = 300;
        public int introEnd = 38 * 60; //Update when we have the voiceline
        public int additionalWait = 10;

        private int flashStart = 37 * 60;
        private int flashEnd = (37 * 60) + 30;

        private bool clientIntroPlayed = false;

        public int Timer
        {
            get
            {
                return (int)NPC.ai[1];
            }
            set
            {
                NPC.ai[1] = value;
            }
        }

        public float IntroProgress
        {
            get
            {
                return State == IntroState.Ballin ? 0f : GameplayUtils.GetTimeFromInts(Timer, introEnd);
            }
        }

        public enum IntroState
        {
            Ballin,
            Out
        }

        public IntroState State
        {
            get
            {
                return (IntroState)NPC.ai[0];
            }
            set
            {
                NPC.ai[0] = (int)value;
            }
        }

        public int targetPlayer => (int)NPC.ai[2];

        public override void SetStaticDefaults()
        {
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Hide = true // Hides this NPC from the Bestiary, useful for multi-part NPCs whom you only want one entry.
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, value);
        }

        public override void SetDefaults()
        {
            NPC.width = 50;
            NPC.height = 50;
            NPC.lifeMax = 100;
            NPC.damage = 0;
            NPC.friendly = false;
            NPC.ShowNameOnHover = false;
            NPC.dontTakeDamage = true;
            introVoice = MinosVoiceLineController.LoadSoundStyle("Intro");
        }

        public override void OnSpawn(IEntitySource source)
        {
            initPos = NPC.Center;
            NPC.netUpdate = true;
            //Main.NewText($"Help: {NPC.Center}");
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(initPos);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            initPos = reader.ReadVector2();
        }

        public override void OnKill()
        {
            BossNPCGlobals.minosorb = -1;
        }

        public override void AI()
        {
            Timer++;
            BossNPCGlobals.minosorb = NPC.whoAmI;
            if ((State == IntroState.Ballin && Timer >= ballSwitch) || (State == IntroState.Out && !clientIntroPlayed))
            {
                clientIntroPlayed = true;
                if(State != IntroState.Out)
                {
                    Timer = 0;
                }
                State = IntroState.Out;
                SoundEngine.PlaySound(Viper.snakeShatter, NPC.Center);
                SoundEngine.PlaySound(introVoice, NPC.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MinosIntroExplosionEffect>(), 0, 0);
                    
                    foreach(var proj in Main.ActiveProjectiles)
                    {
                        if(proj.type == ModContent.ProjectileType<SoulOrbParticle>())
                        {
                            proj.Kill();
                        }
                    }
                }
                
            }
            else if(State == IntroState.Out && Timer >= introEnd) 
            {
                if(Main.netMode != NetmodeID.MultiplayerClient && Timer == introEnd)
                {
                    NPC.SpawnBoss((int)NPC.Center.X, (int)NPC.Center.Y + NPC.height / 2, ModContent.NPCType<MinosPrime>(), targetPlayer);
                }
                if(Timer >= introEnd + additionalWait)
                {
                    NPC.life = 0;
                    NPC.checkDead();
                }
            }

            switch (State)
            {
                case IntroState.Ballin:
                    NPC.velocity = Vector2.Zero;
                    NPC.Center = Vector2.Lerp(initPos, initPos + new Vector2(0, UpperOffset), MathHelper.Hermite(0f, 3f, 1f, 0f, GameplayUtils.GetTimeFromInts(Timer, ballSwitch)));
                    if(Timer % 30 == 0)
                    {
                        float angle = MathHelper.TwoPi * Main.rand.NextFloat();
                        float offset = 250f;
                        if(Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + (angle.ToRotationVector2() * offset), Vector2.Zero, ModContent.ProjectileType<SoulOrbParticle>(), 0, 0, ai0: NPC.whoAmI);
                        }
                    }
                    if(Main.netMode != NetmodeID.Server && Timer == UIPopup && Vector2.Distance(Main.LocalPlayer.position, NPC.Center) < 3000f)
                    {
                        ModContent.GetInstance<MinosIntroTextUISystem>().ShowMyUI();
                    }
                    break;
                case IntroState.Out:
                    if (Timer == flashStart)
                    {
                        SoundEngine.PlaySound(MinosPrime.attackPlink, NPC.Center);
                    }
                    break;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var classType = typeof(SoulOrb);
            var ballTex = ModContent.Request<Texture2D>(Texture);
            var rippleTex = ModContent.Request<Texture2D>(ModUtils.GetNamespaceFileLocation(classType) + "/SoulOrbRipple");
            var minosTex = ModContent.Request<Texture2D>(ModUtils.GetNamespaceFileLocation(classType) + "/MinosPrime");

            float time = (float)Main.timeForVisualEffects;

            if(Timer >= introEnd)
            {
                return false;
            }

            if(State == IntroState.Ballin)
            {
                //Ripples
                int rippleAmount = 10;
                float rippleRadius = 10f;
                float rippleTime = (time % 90f) / 90f;
                for (int i = 0; i < rippleAmount; i++)
                {
                    float rippleSeq = GameplayUtils.GetTimeFromInts(i, rippleAmount - 1);
                    float realTime = rippleSeq - rippleTime;
                    if(realTime <= 0f)
                    {
                        realTime = 1f + realTime;
                    }

                    float scale = rippleRadius * realTime;
                    Color color = Color.Lerp(Color.White, Color.Transparent,realTime);
                    TestContent.CenteredDraw(rippleTex.Value, NPC.Center, color, scale: scale);
                }
                TestContent.CenteredDraw(ballTex.Value, NPC.Center, Color.White, scale: NPC.scale);
            }
            else
            {
                var minosRect = minosTex.Value.Bounds;
                var minosorg = minosRect.Center.ToVector2() + new Vector2(0, minosRect.Height / 2);

                float Opacity = GetOpacity();

                float sin = (float)Math.Sin(Main.timeForVisualEffects * 0.5f);
                var glowColor = Color.Cyan;
                glowColor *= Opacity;
                Vector2 drawOffset = new Vector2(sin);
                Vector2 drawOffset2 = new Vector2(-drawOffset.X, drawOffset.Y);

                Vector2 drawPos = NPC.Center + new Vector2(0, ballTex.Height() / 2) - Main.screenPosition;

                Main.EntitySpriteDraw(minosTex.Value, drawPos + drawOffset, null, glowColor, 0f, minosorg, NPC.scale * 1.01f, SpriteEffects.None, 0f);
                Main.EntitySpriteDraw(minosTex.Value, drawPos + drawOffset2, null, glowColor, 0f, minosorg, NPC.scale * 1.01f, SpriteEffects.None, 0f);

                if(Timer > flashStart && Timer < flashEnd)
                {
                    var draws = MinosAttack.DrawAttackIndicator(NPC.Center + new Vector2(30f * NPC.spriteDirection, -90f), GameplayUtils.GetTimeFromInts(Timer - flashStart, flashEnd - flashStart), Color.Cyan);
                    Main.EntitySpriteDraw(draws.Item1);
                    Main.EntitySpriteDraw(draws.Item2);
                }

                Main.EntitySpriteDraw(minosTex.Value, drawPos, null, Color.Black * Opacity, 0f, minosorg, NPC.scale, SpriteEffects.None, 0f);
            }
            return false;
        }

        private float GetOpacity()
        {
            int lerpIn = 25 * 60;
            int lerpOut = 28 * 60;
            if(Timer < lerpIn)
            {
                return 0f;
            }
            else if(Timer > lerpOut)
            {
                return 1f;
            }

            return MathHelper.Hermite(0f, 3f, 1f, 0f, GameplayUtils.GetTimeFromInts(Timer - lerpIn, lerpOut - lerpIn));
        }
    }

    public class MinosIntroEffect : BaseMusicSceneEffect
    {
        public override int NPCType => ModContent.NPCType<SoulOrb>();
        public override int ModMusic => MusicLoader.GetMusicSlot(Mod, "Assets/Music/MinosIntro");
        public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;

        public override int SetMusic()
        {
            if(Main.curMusic != ModMusic && Main.curMusic != 0)
            {
                Main.musicFade[Main.curMusic] = 0f;
                return 0;
            }
            if(Main.curMusic == 0)
            {
                return ModMusic;
            }
            if(BossNPCGlobals.minosorb != -1)
            {
                var orb = (SoulOrb)Main.npc[BossNPCGlobals.minosorb].ModNPC;
                Main.musicFade[MusicLoader.GetMusicSlot(Mod, "Assets/Music/MinosIntro")] = MathHelper.Lerp(0f, 1f, orb.IntroProgress);
            }
            else
            {
                Main.musicFade[MusicLoader.GetMusicSlot(Mod, "Assets/Music/MinosIntro")] = 0f;
            }
            
            return ModMusic;
        }

        public override bool AdditionalCheck()
        {
            if(BossNPCGlobals.minosorb == -1)
            {
                return false;
            }
            NPC npc = Main.npc[BossNPCGlobals.minosorb];
            return npc.ai[0] == (int)SoulOrb.IntroState.Out;
        }

        public override void SpecialVisuals(Player player, bool isActive)
        {
            //Main.NewText($"SkyIntro: IsActive ? : {isActive}");
            player.ManageSpecialBiomeVisuals("TestContent:MinosIntro", isActive);
        }
    }
}
