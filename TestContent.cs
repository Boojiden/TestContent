using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using TestContent.Items.Consumables;
using TestContent.Items.Pets;
using TestContent.Projectiles;
using TestContent.UI;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using TextContent.Effects.Graphics.Primitives;
using TestContent.Players;
using TestContent.NPCs.Minos;
using TestContent.NPCs.Minos.Skys;
using TestContent.Utility;
using ReLogic.Graphics;
using Terraria.GameInput;
using System.Reflection;
using System.Text;
using System;
using TestContent.NPCs;
using System.Linq;
using TestContent.NPCs.Minos.Projectiles.Friendly;

namespace TestContent
{
	public class TestContent : Mod
	{
        private static Asset<Effect> _transmogEffect;
        public static Asset<DynamicSpriteFont> compFont;
        public static Asset<DynamicSpriteFont> compFontSub;
        public static Asset<DynamicSpriteFont> compFontTitle;
        public static Effect TransmogEffect;
        public static Asset<Texture2D> noise;
        public static Asset<Texture2D> flesh;

        public static readonly FieldInfo CurrentPlayerProfileField = typeof(PlayerInput).GetField("_currentProfile", BindingFlags.NonPublic | BindingFlags.Static);
        public static readonly PlayerInputProfile CurrentInputProfile = (PlayerInputProfile)(TestContent.CurrentPlayerProfileField.GetValue(null));

        public static Mod Instance;

        public static string GetControlString(string knownTrigger)
        {
            if (CurrentInputProfile.InputModes[InputMode.Keyboard].KeyStatus.ContainsKey(knownTrigger))
            {
                var stringList = CurrentInputProfile.InputModes[InputMode.Keyboard].KeyStatus[knownTrigger];
                StringBuilder builder = new StringBuilder();
                for(int i = 0; i < stringList.Count; i++)
                {
                    if(i == stringList.Count - 1)
                    {
                        builder.Append(stringList[i]);
                    }
                    else
                    {
                        builder.Append(stringList[i] + " or ");
                    }
                }
                return builder.ToString();
            }
            return "";
        }

        /// <summary>
        /// Draw a projectile based on the Projectile's total frames vs the current frame. This implementation expects a vertical spritesheet
        /// </summary>
        /// <param name="Projectile"></param>
        /// <param name="lightColor"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <param name="offsetScale"></param>
        /// <param name="effects"></param>
        public static void AnimatedProjectileDraw(Projectile Projectile, Color lightColor, float offsetX = 0f, float offsetY = 0f, float offsetScale = 1f, SpriteEffects effects = SpriteEffects.None)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int startY = frameHeight * Projectile.frame;
            Rectangle sourceRectangle = new Rectangle(1, startY, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2;

            Vector2 visualOffset = new Vector2(offsetX, offsetY) * offsetScale;

            //origin.X = (float)((Projectile.spriteDirection == 1) ? (sourceRectangle.Width - offsetX) : offsetX);
            Main.EntitySpriteDraw(texture, (Projectile.Center - Main.screenPosition) + visualOffset,
            sourceRectangle, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);
        }

        public static void CenteredProjectileDraw(Projectile Projectile, Color lightColor, float offsetX = 0f, float offsetY = 0f, float offsetScale = 1f, SpriteEffects effects = SpriteEffects.None)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle sourceRectangle = new Rectangle(1, 1, texture.Width, texture.Height);
            Vector2 origin = sourceRectangle.Size() / 2;

            Vector2 visualOffset = new Vector2(offsetX, offsetY) * offsetScale;

            //origin.X = (float)((Projectile.spriteDirection == 1) ? (sourceRectangle.Width - offsetX) : offsetX);
            Main.EntitySpriteDraw(texture, (Projectile.Center - Main.screenPosition) + visualOffset,
            sourceRectangle, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);
        }

        public static void CenteredDraw(Texture2D texture, Vector2 worldPosition, Color lightColor, float rot = 0f, float scale = 1f, SpriteEffects effects = SpriteEffects.None, float offsetX = 0f, float offsetY = 0f)
        {
            var rect = texture.Bounds;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 offset = new Vector2(offsetX, offsetY);

            Main.EntitySpriteDraw(texture, (worldPosition + offset) - Main.screenPosition, rect, lightColor, rot, origin, scale, effects);

        }
        public override void Load()
        {
            Instance = this;
            if (!Main.dedServ)
            {
                PrimitiveRenderer.Initialize();
                LoadShaders();
                compFont = ModContent.Request<DynamicSpriteFont>("TestContent/Assets/Fonts/compFont", AssetRequestMode.ImmediateLoad);
                compFontSub = ModContent.Request<DynamicSpriteFont>("TestContent/Assets/Fonts/compFontSub", AssetRequestMode.ImmediateLoad);
                compFontTitle = ModContent.Request<DynamicSpriteFont>("TestContent/Assets/Fonts/compFontLarge", AssetRequestMode.ImmediateLoad);
            }
            
            
        }

        public void LoadShaders()
        {
            LoadFilter("ShockwaveFilter", "ShockwaveFilter", "TestContentShockwave", EffectPriority.VeryHigh);
            LoadFilter("RedBossTint", "RedBossTint", "ModdersToolkitShaderPass");

            Filters.Scene[$"TestContent:MinosIntro"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(Color.DarkRed).UseOpacity(0.25f), EffectPriority.VeryHigh);
            SkyManager.Instance["TestContent:MinosIntro"] = new IntroSky();

            LoadMiscShader("MinosSkyEffect", "MinosFightBackground", "SkyPass");
            Filters.Scene[$"TestContent:MinosFight"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(Color.DarkRed).UseOpacity(0.45f), EffectPriority.VeryHigh);
            SkyManager.Instance["TestContent:MinosFight"] = new FightSky();

            //Asset<Effect> bossFilter = this.Assets.Request<Effect>("Effects/RedBossTint");
            //Filters.Scene["RedBossTint"] = new Filter(new ScreenShaderData(bossFilter, "ModdersToolkitShaderPass"), EffectPriority.High);
            //Asset<Effect> basePrimitiveShader = this.Assets.Request<Effect>("Effects/StandardPrimitiveShader");
            //GameShaders.Misc["TestContent:StandardPrimitiveShader"] = new MiscShaderData(basePrimitiveShader, "PrimitivePass");
            LoadMiscShader("StandardPrimitiveShader", "StandardPrimitiveShader", "PrimitivePass");
            LoadMiscShader("FadedUVMapStreak", "TrailStreak", "TrailPass");
            LoadMiscShader("FadingSolidTrail", "TrailSolid", "TrailPass");
            LoadMiscShader("DirectSolidTrail", "TrailDirect", "TrailPass");
            Asset<Effect> transmog = Assets.Request<Effect>("Effects/TransmogShader");
            noise = Assets.Request<Texture2D>("Effects/smallNoise");
            flesh = Assets.Request<Texture2D>("NPCs/Minos/Extras/Flesh");
            _transmogEffect = transmog;
        }

        public void LoadMiscShader(string shaderName, string referenceName, string passName)
        {
            Asset<Effect> asset = Assets.Request<Effect>($"Effects/{shaderName}");
            GameShaders.Misc[$"{Instance.Name}:{referenceName}"] = new MiscShaderData(asset, passName);
        }

        public void LoadFilter(string shaderName, string referenceName, string passName, EffectPriority priority = EffectPriority.High)
        {
            Asset<Effect> asset = Assets.Request<Effect>($"Effects/{shaderName}");
            Filters.Scene[$"{Instance.Name}:{referenceName}"] = new Filter(new ScreenShaderData(asset, passName), priority);
            Filters.Scene[$"{Instance.Name}:{referenceName}"].Load();
        }

        public override void PostSetupContent()
        {
            if (!Main.dedServ)
            {
                var tex = noise.Value;
                TransmogEffect = _transmogEffect.Value;
                TransmogEffect.Parameters["uImageSize0"].SetValue(new Vector2(tex.Width, tex.Height));
            }
        }

        public enum NetMessageType
        {
            CarSpawns,
            CarUsed,
            KiryuBossIntro,
            SyncRidable,
            MinosBossIntro,
            ToggleRockets,
            SpawnRocketExplosion,
            SyncWhiplashTarget
        }
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            NetMessageType type = (NetMessageType)reader.ReadByte();
            Player localPlayer = Main.player[Main.myPlayer];
            //Main.NewText("Packet Type: " + type);
            switch (type)
            {
                //Car spawns
                case NetMessageType.CarSpawns:
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        SoundEngine.PlaySound(CarRare.Horn, localPlayer.position);
                    break;
                //Car used
                case NetMessageType.CarUsed:
                    int sound = (int)reader.ReadByte();
                    localPlayer = Main.player[reader.ReadInt32()];
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        switch (sound)
                        {
                            case 1:
                                SoundEngine.PlaySound(CarPetProjectile.Car1, localPlayer.position);
                                break;
                            case 2:
                                SoundEngine.PlaySound(CarPetProjectile.Car2, localPlayer.position);
                                break;
                            case 3:
                                SoundEngine.PlaySound(CarPetProjectile.Car3, localPlayer.position);
                                break;
                        }
                    }
                    break;
                case NetMessageType.KiryuBossIntro:
                    Vector2 bossPos = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    var dist = (Main.LocalPlayer.Center - bossPos).Length();
                    if (dist < 10000f)
                    {
                        var system = ModContent.GetInstance<KiryuBossIntroUISystem>();
                        //system.BossUI.OnInitialize();
                        system.ToggleUI();
                    }
                    break;
                case NetMessageType.SyncRidable:
                    byte playerNum = reader.ReadByte();
                    if(playerNum >= Main.player.Length)
                    {
                        break;
                    }
                    PlayerRideables ride = Main.player[playerNum].GetModPlayer<PlayerRideables>();
                    ride.SyncProjectile(reader.ReadInt32());

                    if (Main.netMode == NetmodeID.Server)
                    {
                        ride.SyncPlayer(-1, whoAmI, false);
                        //Console.WriteLine("Synced Riding Projectile");
                    }
                    break;
                case NetMessageType.MinosBossIntro:
                    Player summoningPlayer = Main.player[reader.ReadByte()];
                    if (Main.netMode == NetmodeID.Server)
                    {
                        int spot = NPC.NewNPC(null, (int)summoningPlayer.position.X, (int)(summoningPlayer.position.Y - SoulOrb.UpperOffset), ModContent.NPCType<SoulOrb>());
                        //Console.WriteLine($"{summoningPlayer.position}");
                        Main.npc[spot].netUpdate2 = true;
                        //NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, spot);
                    }
                    break;
                case NetMessageType.ToggleRockets:
                    Player activePlayer = Main.player[reader.ReadByte()];
                    if(Main.netMode == NetmodeID.Server || activePlayer.whoAmI != localPlayer.whoAmI)
                    {
                        activePlayer.GetModPlayer<PlayerWeapons>().ToggleRockets();
                    }
                    break;
                case NetMessageType.SpawnRocketExplosion: //WHY ISN'T NUMHITS OR PENETRATE SYNCED BETWEEN CLIENTS
                    if(Main.netMode == NetmodeID.Server)
                    {
                        int identity = reader.ReadByte();
                        Projectile proj = Main.projectile.FirstOrDefault(x => x.identity ==  identity);
                        int hits = reader.ReadByte();
                        if (proj != null)
                        {
                            FreezeFrameRocket rocket = proj.ModProjectile as FreezeFrameRocket;
                            if(rocket == null)
                            {
                                break;
                            }

                            if (rocket.Buffed)
                            {
                                Projectile.NewProjectile(Projectile.InheritSource(proj), proj.Center, Vector2.Zero, ModContent.ProjectileType<FreezeFrameRocketExplosionEmpowered>(), (int)(proj.damage * 1.5f), proj.knockBack);
                            }
                            else if (hits > 0)
                            {
                                Projectile.NewProjectile(Projectile.InheritSource(proj), proj.Center, Vector2.Zero, ModContent.ProjectileType<FreezeFrameRocketExplosion>(), proj.damage, proj.knockBack);
                            }
                            else
                            {
                                Projectile.NewProjectile(Projectile.InheritSource(proj), proj.Center, Vector2.Zero, ModContent.ProjectileType<FreezeFrameRocketExplosionNull>(), proj.damage / 2, proj.knockBack);
                            }
                        }
                    }
                    break;
                case NetMessageType.SyncWhiplashTarget:
                    int id = reader.ReadByte();
                    Projectile proj2 = Main.projectile.FirstOrDefault(x => x.identity == id);
                    if(proj2 != null)
                    {
                        var whip = proj2.ModProjectile as WhiplashProjectile;
                        if(whip != null)
                        {
                            int npcid = reader.ReadByte();
                            whip.latchedTo = Main.npc[npcid];
                            whip.travelTo = reader.ReadBoolean();

                            if(Main.netMode == NetmodeID.Server)
                            {
                                ModPacket packet = TestContent.Instance.GetPacket();
                                packet.Write((byte)TestContent.NetMessageType.SyncWhiplashTarget);
                                packet.Write((byte)proj2.identity);
                                packet.Write((byte)npcid);
                                packet.Write(whip.travelTo);
                                packet.Send();
                            }
                        }
                    }
                    break;
            }
        }
    }
}