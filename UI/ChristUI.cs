using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace TestContent.UI
{
    public class ChristUI : UIState
    {
        public int currentDuration = 0;
        public int loopDuration = 140;
        public int totalDuration = 0;
        public SlotId sound;
        public ChristUISystem system;

        public UIImage jesus;
        public static SoundStyle bellToll;
        public bool justActivated = false;

        public override void OnInitialize()
        {
            bellToll = new SoundStyle("TestContent/Assets/Sounds/BellToll")
            {
                Volume = 0.8f,
                MaxInstances = 1
            };
            var texture = ModContent.Request<Texture2D>("TestContent/UI/Jesus");
            jesus = new UIImage(texture);
            jesus.Left.Set(-960f, 0.5f);
            jesus.Top.Set(-540f, 0.5f);

            Append(jesus);
        }

        public override void Update(GameTime gameTime)
        {

            if(currentDuration-- < 0)
            {
                SoundEngine.PlaySound(bellToll);
                currentDuration = loopDuration;
            }

            jesus.Color.A = (byte)(((float)currentDuration / (float)loopDuration) * 255f);

            if(totalDuration++ > 1800)
            {
                system.HideMyUI();
            }
            //Main.NewText(jesus.Color.A);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone);
            jesus.Draw(spriteBatch);
            spriteBatch.End();
            spriteBatch.Begin();
        }

        public ChristUI(ChristUISystem uISystem)
        {
            this.system = uISystem;
        }
    }

    [Autoload(Side = ModSide.Client)]
    public class ChristUISystem : ModSystem
    {
        private UserInterface UI;
        internal ChristUI christ;
        public static bool doJumpscare = true;

        public bool active = false;

        // These two methods will set the state of our custom UI, causing it to show or hide
        public void ShowMyUI()
        {
            UI?.SetState(christ);
            active = true; 
            //BossUI.OnOpen(imageID);
            //SlotMachineUI.Initialize();
        }

        public void HideMyUI()
        {
            christ.currentDuration = 0;
            christ.totalDuration = 0;
            UI?.SetState(null);
            active = false;
        }

        public override void Load()
        {
            // Create custom interface which can swap between different UIStates
            UI = new UserInterface();
            // Creating custom UIState
            christ = new ChristUI(this);

            // Activate calls Initialize() on the UIState if not initialized, then calls OnActivate and then calls Activate on every child element
            christ.Activate();
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
                    "TestContent: Jesus Christ",
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
