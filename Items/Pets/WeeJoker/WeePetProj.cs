using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.Audio;
using System.IO;
using Terraria.DataStructures;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using TestContent.Utility;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using ReLogic.Content;
using ReLogic.Graphics;

namespace TestContent.Items.Pets.WeeJoker
{
    public class WeePetProj : ModProjectile
    {

        public SoundStyle voice = new SoundStyle("TestContent/Assets/Sounds/weeVoice")
        {
            Volume = 0.8f,
            PlayOnlyIfFocused = true,
            PitchVariance = 0.7f
        };

        public int maxVoiceLines = 25;

        public int maxWaitTime = 14400 / 2;
        public int minWaitTime = 10800 / 2;

        public int timeTillVoiceLine = 0;
        public int voiceLineID = 1;

        public bool playingVoice = false;
        public int voiceDelay = 0;
        public int voiceInterval = 7;
        public int voiceBlips = 0;

        private int currentTextID = -1;
        private int currentTextDuration = 0;

        private CubicBezierCurveLerp rotationLerp = new CubicBezierCurveLerp(1f, 2f, 1f, 0f);
        private CubicBezierCurveLerp scaleLerp = new CubicBezierCurveLerp(1f, 2f, 1f, 0f);

        private float scaleMod = 0.1f;
        private float rotMod = (float)(Math.PI / 12.0);
        private int randRotMod = 1;

        private Asset<Texture2D> speechBubble;

        private string locText = "";

        public Vector2 textOffset = new Vector2(0, 0);

        private int totalTalkingTime = 0;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            Main.projPet[Projectile.type] = true;

            // This code is needed to customize the vanity pet display in the player select screen. Quick explanation:
            // * It uses fluent API syntax, just like Recipe
            // * You start with ProjectileID.Sets.SimpleLoop, specifying the start and end frames as well as the speed, and optionally if it should animate from the end after reaching the end, effectively "bouncing"
            // * To stop the animation if the player is not highlighted/is standing, as done by most grounded pets, add a .WhenNotSelected(0, 0) (you can customize it just like SimpleLoop)
            // * To set offset and direction, use .WithOffset(x, y) and .WithSpriteDirection(-1)
            // * To further customize the behavior and animation of the pet (as its AI does not run), you have access to a few vanilla presets in DelegateMethods.CharacterPreview to use via .WithCode(). You can also make your own, showcased in MinionBossPetProjectile
            ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(0, Main.projFrames[Projectile.type], 6)
                .WithOffset(10f, -20f)
                .WithSpriteDirection(-1)
                .WithCode(DelegateMethods.CharacterPreview.Float);
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.ZephyrFish); // Copy the stats of the Zephyr Fish
            Projectile.scale = 0.35f;
            AIType = ProjectileID.ZephyrFish; // Mimic as the Zephyr Fish during AI.

            speechBubble = Mod.Assets.Request<Texture2D>("Items/Pets/WeeJoker/WeeJokerSpeechBubble");
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(timeTillVoiceLine);
            writer.Write(voiceLineID);
        }

        public override void OnSpawn(IEntitySource source)
        {
            timeTillVoiceLine = Main.rand.Next(minWaitTime, maxWaitTime);
            voiceLineID = Main.rand.Next(1, maxVoiceLines + 1);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            timeTillVoiceLine = reader.ReadInt32();
            voiceLineID = reader.ReadInt32();
        }

        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];
            //player.QuickSpawnItem
            player.zephyrfish = false; // Relic from AIType

            return true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // Keep the projectile from disappearing as long as the player isn't dead and has the pet buff.
            if (!player.dead && player.HasBuff(ModContent.BuffType<WeePetBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            timeTillVoiceLine--;

            if (timeTillVoiceLine < 0 && !Main.dedServ)
            {
                string line = Language.GetTextValue("Mods.TestContent.Misc.WeeJokerLines." + voiceLineID.ToString());
                voiceBlips = Math.Clamp(line.Length / 4, 1, int.MaxValue);
                textOffset = new Vector2(-4f * line.Length, -55);
                locText = line;
                /*
                AdvancedPopupRequest popup = default;
                popup.Text = "";
                popup.Color = new Color(0.3058823529411765f, 0.7568627450980392f, 0.9568627450980393f);
                popup.DurationInFrames = (voiceBlips * voiceInterval) + 120;
                popup.Velocity = -Vector2.UnitY * 3f;

                currentTextID = PopupText.NewText(popup, Projectile.position);
                */
                currentTextDuration = (voiceBlips * voiceInterval) + 120;

                totalTalkingTime = voiceBlips * voiceInterval;

                timeTillVoiceLine = Main.rand.Next(minWaitTime, maxWaitTime);
                voiceLineID = Main.rand.Next(1, maxVoiceLines + 1);
                playingVoice = true;
                Projectile.netUpdate = true;
                
            }

            if(playingVoice) {
                if (voiceDelay < 0 && voiceBlips > 0)
                {
                    voiceBlips--;
                    SoundEngine.PlaySound(voice, Projectile.Center);
                    randRotMod = Main.rand.NextBool() ? -1 : 1;
                    
                    voiceDelay = voiceInterval;
                }
                else if(voiceBlips <= 0)
                {
                    playingVoice = false;
                }
                //Projectile.rotation = rotationLerp.GetLerp(t) * rotMod * randRotMod;
                //Projectile.scale = 0.35f + Math.Clamp(scaleLerp.GetLerp(t) * scaleMod, 0, scaleMod);
            }

            if(voiceDelay >= 0)
            {
                float t = ((float)voiceDelay / (float)voiceInterval);
                Projectile.rotation = rotationLerp.GetLerp(t) * rotMod * randRotMod;
                Projectile.scale = 0.35f + Math.Clamp(scaleLerp.GetLerp(t) * scaleMod, 0, scaleMod);
                voiceDelay--;
                
            }

            currentTextDuration--;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle source = new Rectangle(1, 1, texture.Width, texture.Height);
            Vector2 origin = source.Size() / 2;

            if(currentTextDuration > 0)//Draw text box
            {
                string textToDraw = locText;

                var font = FontAssets.MouseText.Value;

                Vector2 textSize = font.MeasureString(textToDraw) / 2;

                var color1 = new Color(0.3058823529411765f, 0.7568627450980392f, 0.9568627450980393f);
                var color2 = Color.Blue;

                var lerpedColor = Color.Lerp(color1, color2, ((float)Math.Sin(Main.timeForVisualEffects * 0.05) / 2) + 0.5f);

                var tex = speechBubble.Value;

                var textTime = 1 - (float)((float)(voiceBlips * voiceInterval + voiceDelay) / (float)totalTalkingTime);

                int letters = (int)((float)textToDraw.Length * textTime);



                textToDraw = textToDraw.Substring(0, letters);

                Vector2 startpoint = Projectile.Center + textOffset;
                //Vector2 midpoint = Projectile.Center + textOffset + new Vector2(Projectile.Center.X - textOffset.X, 0);
                Vector2 endpoint = startpoint + (textSize * 2);

                Vector2 offset = new Vector2(-9, 0);

                Vector2 textStart = startpoint;

                startpoint += offset;
                endpoint -= offset;

                Rectangle left = new Rectangle(0, 0, 14, 32);
                Rectangle mid = left;
                Rectangle right = left;

                Vector2 bubbleOrigin = left.Size() / 8;

                mid.X = 16;
                right.X = 32;

                float length = (startpoint - endpoint).Length();

                int draws = (int)(length / 14);

                var spritebatch = Main.spriteBatch;
                spritebatch.Draw(tex, startpoint - Main.screenPosition, left, Color.White, 0, bubbleOrigin, 1f, SpriteEffects.None, 0f);

                for (int i = 0; i < draws; i++)
                {
                    spritebatch.Draw(tex, (startpoint + new Vector2(14f * (i + 1), 0)) - Main.screenPosition, mid, Color.White, 0, bubbleOrigin, 1f, SpriteEffects.None, 0f);
                }

                spritebatch.Draw(tex, (startpoint + new Vector2(14f * draws, 0)) - Main.screenPosition, right, Color.White, 0, bubbleOrigin, 1f, SpriteEffects.None, 0f);


                

                //spritebatch.End();
                //spritebatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
                spritebatch.DrawString(font, textToDraw, textStart - Main.screenPosition, lerpedColor);

                //spritebatch.End();
                //spritebatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
            }

            Main.EntitySpriteDraw(texture, (Projectile.Center - Main.screenPosition), source, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
