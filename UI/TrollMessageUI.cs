using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.GameInput;
using Terraria.DataStructures;

namespace TestContent.UI
{
    public class TrollMessageUI: UIState
    {
        public int currentImage = 0;
        public static int MaxImages = 3;
        public List<UIImage> images = new List<UIImage>();
        public UIImage trollage;
        public UIImage despair;
        public UIImage bargaining;
        public UIImage acceptance;
        public TrollMessageUISystem system;

        public TrollMessageUI(TrollMessageUISystem UI)
        {
            system = UI;
        }

        public override void OnInitialize()
        {
            images =
            [
                LoadImage("Stop", 524f, 426f),
                LoadImage("KeepGambling", 1099f, 940f),
                LoadImage("Pig", 959f, 606f),
                LoadImage("Xbox", 828f, 639f),
                LoadImage("Ineedtokill", 816f, 520f),
                LoadImage("loss", 140f, 140f),
                LoadImage("oh", 469f, 391f),
                LoadImage("solidify", 1170f, 1155f),
                LoadImage("Xbox", 828f, 639f),
                LoadImage("SourceCode", 456f, 454f),
                LoadImage("tddb", 750f, 601f)
            ];
            for(int i = 0; i < images.Count; i++)
            {
                Append(images[i]);
            }
            MaxImages = images.Count - 1;
            Main.OnResolutionChanged += SetMiddleScreenPosition;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            GetCurrentImage().Draw(spriteBatch);
        }

        public UIImage LoadImage(string fileName, float width, float height)
        {
            var texture = ModContent.Request<Texture2D>($"TestContent/UI/Silly/{fileName}");
            var image = new UIImage(texture);
            //image.Width.Set(524f, 0f);
            //image.Height.Set(426f, 0f);
            image.Width.Set(width, 0f);
            image.Height.Set(height, 0f);
            return image;
        }

        public void SetMiddleScreenPosition(Vector2 size)
        {
            float midx = size.X / 2;
            float midy = size.Y / 2;
            var image = GetCurrentImage();
            midx -= image.GetInnerDimensions().Width/2;
            midy -= image.GetInnerDimensions().Height/2;
            image.Left.Set(midx, 0f);
            image.Top.Set(midy, 0f);
        }

        public void OnOpen(int imageID)
        {
            currentImage = imageID;
            SetMiddleScreenPosition(Main.ScreenSize.ToVector2());
            //Main.NewText(currentImage);
        }

        public UIImage GetCurrentImage()
        {
            return images[currentImage];
        }

        public override void Update(GameTime gameTime)
        {
            if(PlayerInput.Triggers.Current.Inventory && system.active)
            {
                system.HideMyUI();
            }
        }
    }

    [Autoload(Side = ModSide.Client)]
    public class TrollMessageUISystem : ModSystem
    {
        private UserInterface UI;
        internal TrollMessageUI TrollUI;

        public bool active = false;

        // These two methods will set the state of our custom UI, causing it to show or hide
        public void ShowMyUI(int imageID)
        {
            UI?.SetState(TrollUI);
            active = true;
            TrollUI.OnOpen(imageID);
            //SlotMachineUI.Initialize();
        }

        public void HideMyUI()
        {
            UI?.SetState(null);
            active = false;
        }

        public void ToggleUI(int imageID)
        {
            if(active)
            {
                HideMyUI();
            }
            else
            {
                ShowMyUI(imageID);
            }
        }

        public override void Load()
        {
            // Create custom interface which can swap between different UIStates
            UI = new UserInterface();
            // Creating custom UIState
            TrollUI = new TrollMessageUI(this);

            // Activate calls Initialize() on the UIState if not initialized, then calls OnActivate and then calls Activate on every child element
            TrollUI.Activate();
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
                    "TestContent: Troll Message",
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
