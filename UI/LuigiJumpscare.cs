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
    public class LuigiJumpscare : UIState
    {
        public int currentDuration = 0;
        public SlotId sound;
        public LuigiJumpscareUISystem system;

        public UIImage wega;
        public static SoundStyle wegaJumpscare;
        public bool justActivated = false;

        public override void OnInitialize()
        {
            wegaJumpscare = new SoundStyle("TestContent/Assets/Sounds/Wega")
            {
                Volume = 0.8f,
                MaxInstances = 1
            };
            var texture = ModContent.Request<Texture2D>("TestContent/UI/Jumpscare");
            wega = new UIImage(texture);
            wega.Left.Set(-512f, 0.5f);
            wega.Top.Set(-384f, 0.5f);

            Append(wega);
        }

        public override void Update(GameTime gameTime)
        {
            if (justActivated)
            {
                sound = SoundEngine.PlaySound(wegaJumpscare, null);
                justActivated = false;
            }
            if(currentDuration-- < 0)
            {
                ActiveSound result;
                SoundEngine.TryGetActiveSound(sound, out result);
                if(result != null)
                {
                    result.Stop();
                }
                system.HideMyUI();
            }
        }

        public LuigiJumpscare(LuigiJumpscareUISystem uISystem)
        {
            this.system = uISystem;
        }
    }

    [Autoload(Side = ModSide.Client)]
    public class LuigiJumpscareUISystem : ModSystem
    {
        private UserInterface UI;
        internal LuigiJumpscare JumpscareUI;
        public static bool doJumpscare = true;

        public bool active = false;

        // These two methods will set the state of our custom UI, causing it to show or hide
        public void ShowMyUI(int duration)
        {
            if (doJumpscare)
            {
                UI?.SetState(JumpscareUI);
                JumpscareUI.justActivated = true;
                JumpscareUI.currentDuration = duration;
                active = true; 
            }
            //BossUI.OnOpen(imageID);
            //SlotMachineUI.Initialize();
        }

        public void HideMyUI()
        {
            UI?.SetState(null);
            active = false;
        }

        public override void Load()
        {
            // Create custom interface which can swap between different UIStates
            UI = new UserInterface();
            // Creating custom UIState
            JumpscareUI = new LuigiJumpscare(this);

            // Activate calls Initialize() on the UIState if not initialized, then calls OnActivate and then calls Activate on every child element
            JumpscareUI.Activate();
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
                    "TestContent: Luigi Jumpscare",
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
