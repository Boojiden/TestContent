using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.UI;
using TestContent.Utility;
using Terraria.GameContent.UI.Elements;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Effects;
using System.Linq.Expressions;
using ReLogic.Graphics;
using Terraria.Audio;

namespace TestContent.UI
{
    public class MinosIntroText: UIState
    {
        public static MinosIntroTextUISystem UISystem;

        public int timer = 0;
        public int subTime = 30;
        public int delay = 40;
        public int titleTime = 70;
        public int deacTime = 300;

        public SoundStyle blip = new SoundStyle(ModUtils.GetSoundFileLocation("Minos/UIBlip"))
        {
            Volume = 0.5f,
            PitchVariance = 0.5f,
            PlayOnlyIfFocused = true,
            MaxInstances = 5
        };
        public override void OnInitialize()
        {
            
        }

        public override void Update(GameTime gameTime)
        {
            timer++;
            if(timer > deacTime)
            {
                UISystem.HideMyUI();
            }
            if(timer < subTime || (timer > delay && timer < titleTime))
            {
                if(timer % 2 == 0)
                {
                    SoundEngine.PlaySound(blip);
                }
            }
            //TODO: Sound Logic
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            string sub = "DISEASE /// FINAL";
            string title = "SOUL SURVIVOR";
            var drawPos = new Vector2(Main.screenWidth / 2, Main.screenHeight / 8);
            var titleSize = TestContent.compFontTitle.Value.MeasureString(title);
            var subSize = TestContent.compFontSub.Value.MeasureString(sub);
            var titleDrawPos = drawPos + new Vector2(0f, subSize.Y + (titleSize.Y/4));

            StringBuilder subDraw = new StringBuilder();
            StringBuilder titleDraw = new StringBuilder();

            int subLetters = 0;
            int titleLetters = 0;
            if(timer < subTime)
            {
                var subLife = GameplayUtils.GetTimeFromInts(timer, subTime);
                subLetters = (int)((float)sub.Length * subLife);
                if(subLetters > 0)
                {
                    subDraw = new StringBuilder(sub.Substring(0, subLetters));
                }
            }
            else
            {
                subDraw = new StringBuilder(sub);
                if(timer > delay && timer < titleTime)
                {
                    var titleLife = GameplayUtils.GetTimeFromInts(timer - delay, titleTime - delay);
                    titleLetters = (int)((float)title.Length * titleLife);
                    if(titleLetters > 0)
                    {
                        titleDraw = new StringBuilder(title.Substring(0, titleLetters));
                    }
                }
                else if(timer >= titleTime)
                {
                    titleDraw = new StringBuilder(title);
                }
            }

            spriteBatch.DrawString(TestContent.compFontSub.Value, subDraw, drawPos, Color.White, 0f, subSize * 0.5f, 1f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(TestContent.compFontTitle.Value, titleDraw, titleDrawPos, Color.White, 0f, titleSize * 0.5f, 1f, SpriteEffects.None, 0f);
        }

        public MinosIntroText(MinosIntroTextUISystem sys)
        {
            UISystem = sys;
        }
    }

    [Autoload(Side = ModSide.Client)]
    public class MinosIntroTextUISystem : ModSystem
    {
        private UserInterface UI;
        internal MinosIntroText BossUI;

        public bool active = false;

        // These two methods will set the state of our custom UI, causing it to show or hide
        public void ShowMyUI()
        {
            UI?.SetState(BossUI);
            active = true;
            BossUI.timer = 0;
            //Filters.Scene.Activate("TestContent:RedBossTint");
            //BossUI.OnOpen(imageID);
            //SlotMachineUI.Initialize();
        }

        public void HideMyUI()
        {
            UI?.SetState(null);
            active = false;
            //Filters.Scene["TestContent:RedBossTint"].Deactivate();
        }

        public void ToggleUI()
        {
            if (active)
            {
                HideMyUI();
            }
            else
            {
                ShowMyUI();
            }
        }

        public override void Load()
        {
            // Create custom interface which can swap between different UIStates
            UI = new UserInterface();
            // Creating custom UIState
            BossUI = new MinosIntroText(this);

            // Activate calls Initialize() on the UIState if not initialized, then calls OnActivate and then calls Activate on every child element
            BossUI.Activate();
        }

        public override void UpdateUI(GameTime gameTime)
        {
            // Here we call .Update on our custom UI and propagate it to its state and underlying elements
            if (UI?.CurrentState != null)
            {
                UI?.Update(gameTime);
            }
        }

        // Adding a custom layer to the vanilla layer list that will call .Draw on your interface if it has a state
        // Setting the InterfaceScaleType to UI for appropriate UI scaling
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int dialogueTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: NPC / Sign Dialog"));
            if (dialogueTextIndex != -1)
            {
                layers.Insert(dialogueTextIndex, new LegacyGameInterfaceLayer(
                    "TestContent: Minos Intro Text",
                    delegate {
                        if (UI?.CurrentState != null)
                        {
                            UI.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}
