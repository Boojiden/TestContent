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

namespace TestContent.UI
{
    public class KiryuBossIntro: UIState
    {
        public static KiryuBossIntroUISystem UISystem;

        public float opacity;

        public int Timer;
        public int introLength = 300;

        public float vfxTime = 0;

        public UIImage Title;
        public UIImage gfxCircle;

        public static CubicBezierCurveLerp opacityLerp = new CubicBezierCurveLerp(0.1f, 0.8f, 0.3f, 1f);
        public static CubicBezierCurveLerp scaleLerp = new CubicBezierCurveLerp(0f, 1f, 1f, 0f); //1 - time

        public override void OnInitialize()
        {
            var titleTexture = ModContent.Request<Texture2D>("TestContent/UI/KazumaTitle");
            Title = new UIImage(titleTexture);
            Title.Left.Set(-223f, 0.5f);
            Title.Top.Set(-45f, 0.5f);

            var gfxTexture = ModContent.Request<Texture2D>("TestContent/Dusts/gfxCircle");
            gfxCircle = new UIImage(gfxTexture);
            gfxCircle.Left.Set(-540f, 0.5f);
            gfxCircle.Top.Set(0f, 0f);

            Append(Title);
            Append(gfxCircle);
        }

        public override void Update(GameTime gameTime)
        {
            Timer++;
            vfxTime = ((float)Timer / (float)introLength);
            if(vfxTime > 1f) 
            {
                Timer = 0;
                UISystem.HideMyUI();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float baseScale = 3f;
            var scale = baseScale - baseScale*scaleLerp.GetLerp(vfxTime);
            var opacity = 1 - opacityLerp.GetLerp(vfxTime * 2);
            if( vfxTime > 0.5f)
            {
                opacity = opacityLerp.GetLerp((vfxTime - 0.5f) * 2);
            }
            Title.ImageScale = scale;
            if(vfxTime < 0.5f)
            {
                Title.Color = Color.Lerp(Color.White, Color.Black, opacity);
                Title.Color.A = (byte)(255 - (int)(255f * opacity));
            }
            else
            {
                Title.Color = Color.White;
            }
            Title.Draw(spriteBatch);
            Filters.Scene["TestContent:RedBossTint"].GetShader().UseProgress(1f - opacity);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone);
            gfxCircle.Color = Color.Lerp(Color.Red, Color.Black, opacity);
            gfxCircle.ImageScale = baseScale - scale;
            gfxCircle.Draw(spriteBatch);
            spriteBatch.End();
            spriteBatch.Begin();
        }

        public KiryuBossIntro(KiryuBossIntroUISystem sys)
        {
            UISystem = sys;
        }

        public static float IntroOpacityLerp(float time)
        {
            if(time > 1f || time < 0f)
            {
                return 0f;
            }
            return 1f;
        }

        public static float IntroScaleLerp(float time)
        {
            if (time > 1f)
            {
                return 1f;
            }
            else if (time < 0f)
            {
                return 0f;
            }
            return 1f;
        }
    }

    [Autoload(Side = ModSide.Client)]
    public class KiryuBossIntroUISystem : ModSystem
    {
        private UserInterface UI;
        internal KiryuBossIntro BossUI;

        public bool active = false;

        // These two methods will set the state of our custom UI, causing it to show or hide
        public void ShowMyUI()
        {
            UI?.SetState(BossUI);
            active = true;
            Filters.Scene.Activate("TestContent:RedBossTint");
            //BossUI.OnOpen(imageID);
            //SlotMachineUI.Initialize();
        }

        public void HideMyUI()
        {
            UI?.SetState(null);
            active = false;
            Filters.Scene["TestContent:RedBossTint"].Deactivate();
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
            BossUI = new KiryuBossIntro(this);

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
                    "TestContent: Kiryu Boss Intro",
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
