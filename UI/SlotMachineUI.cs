using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Logs;
using ReLogic.Content;
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
using static System.Net.Mime.MediaTypeNames;
using static TestContent.UI.SlotMachineSystem;

namespace TestContent.UI
{
    public class SlotMachineUI: UIState
    {
        public IncreaseBetButton up;
        public DecreaseBetButton down;
        public BetHandle bet;

        public Machine machine;
        public SlotsReal slots;
        public SlotTexturedText text;

        public UIText currentBetText;

        public UIElement basePlate;

        public int frameCounter = 0;
        public int machineFrame = 0;

        public float scale = 1.5f;

        public SoundStyle Spin, Win, BuzzerSoft, BuzzerLoud;

        public static SlotMachineSystem slotSystem;

        public override void OnInitialize()
        {
            basePlate = new UIElement();
            basePlate.SetPadding(0);
            SetRectangle(basePlate, 0f, 0f, 135f, 170f);

            basePlate.Left.Set(0f, 0.47f);
            base.Top.Set(0f, 0.29f);

            Asset<Texture2D> machineTexture = ModContent.Request<Texture2D>("TestContent/UI/Machine");
            machine = new Machine(machineTexture);
            SetRectangle(machine, 0f, 0f, 133f, 84f);
            //machine.scale = scale;
            basePlate.Append(machine);
            Asset<Texture2D> handleTexture = ModContent.Request<Texture2D>("TestContent/UI/BetHandle");
            bet = new BetHandle(handleTexture, this);
            SetRectangle(bet, 114f, 10f, 20f, 102f);
            //bet.scale = scale;
            basePlate.Append(bet);

            Asset<Texture2D> UpTexture = ModContent.Request<Texture2D>("TestContent/UI/UpButton");
            Asset<Texture2D> DownTexture = ModContent.Request<Texture2D>("TestContent/UI/DownButton");
            up = new IncreaseBetButton(UpTexture);
            SetRectangle(up, 10f, 114f, 20f, 20f);
            //SetRectangle(up, 0f, 0f, 20f, 20f);
            basePlate.Append(up);

            down = new DecreaseBetButton(DownTexture);
            SetRectangle(down, 35f, 114f, 20f, 20f);
            //SetRectangle(down, 30f, 0f, 20f, 20f);
            basePlate.Append(down);

            text = new SlotTexturedText();
            text.Initialize();
            SetRectangle(text, 26f, 38f, 0f, 0f);
            basePlate.Append(text);

            slots = new SlotsReal(this);
            slots.Initialize();
            basePlate.Append(slots);

            currentBetText = new UIText("0000", 1f);
            SetRectangle(currentBetText, 71f, 119f, 0f, 0f);
            basePlate.Append(currentBetText);

            //Initialize the machine then all elements
            //we'll need more trackers to keep the animations in check.
            Append(basePlate);

            Spin = new SoundStyle("TestContent/Assets/Sounds/SlotMachine/Spin")
            {
                Volume = 0.8f,
                PlayOnlyIfFocused = true
            };

            Win = new SoundStyle("TestContent/Assets/Sounds/SlotMachine/Win")
            {
                Volume = 0.8f,
                PlayOnlyIfFocused = true
            };

            BuzzerSoft = new SoundStyle("TestContent/Assets/Sounds/SlotMachine/BuzzerSoft")
            {
                Volume = 0.8f,
                PlayOnlyIfFocused = true
            };

            BuzzerLoud = new SoundStyle("TestContent/Assets/Sounds/SlotMachine/BuzzerLoud")
            {
                Volume = 0.8f,
                PlayOnlyIfFocused = true
            };
        }
        public static void SetRectangle(UIElement uiElement, float left, float top, float width, float height)
        {
            uiElement.Left.Set(left, 0f);
            uiElement.Top.Set(top, 0f);
            uiElement.Width.Set(width, 0f);
            uiElement.Height.Set(height, 0f);
        }

        public override void Update(GameTime gameTime)
        {
            currentBetText.SetText((slotSystem.currentBet/10000).ToString());
            base.Update(gameTime);
        }

        public void rollGame()
        {
            RollResult result = slotSystem.Roll();
            //Main.NewText(result.ToString());
            if(result == RollResult.None)
            {
                Main.NewText("You have to bet something to play the slots!", Color.Gold);
                bet.resetASAP = true;
                return;
            }
            SoundEngine.PlaySound(Spin);
            slots.TriggerSlotDisplay(result);
            text.state = SlotGameState.Idle;
        }

        public void TriggerSystem(RollResult result)
        {
            if((int)result < 6)
            {
                text.state = SlotGameState.Win;
                SoundEngine.PlaySound(Win);
            }
            else if(result == RollResult.Skull) 
            {
                text.state = SlotGameState.Lose;
                SoundEngine.PlaySound(BuzzerLoud);
            }
            else
            {
                text.state = SlotGameState.Lose;
                SoundEngine.PlaySound(BuzzerSoft);
            }
            slotSystem.DoResult(result);
        }

        public void resetHandle()
        {
            bet.ResetClick();
        }
    }

    public class IncreaseBetButton : UIAnimatedButton
    {
        public IncreaseBetButton(Asset<Texture2D> texture) : base(texture)
        {
            tex = texture;
            frameCounterLimit = 5;
            totalFrames = 2;
            leftClickInteruptsAnim = true;
            frameToResetTo = 1;
            clickResetFrame = 1;
            canHoldDown = true;
        }

        public override void LeftClickAction()
        {
            SlotMachineUI.slotSystem.AdjustBet(10000);
        }
    }

    public class DecreaseBetButton : UIAnimatedButton
    {
        public DecreaseBetButton(Asset<Texture2D> texture) : base(texture)
        {
            tex = texture;
            frameCounterLimit = 5;
            totalFrames = 2;
            leftClickInteruptsAnim = true;
            frameToResetTo = 1;
            clickResetFrame = 1;
            canHoldDown = true;
        }

        public override void LeftClickAction()
        {
            SlotMachineUI.slotSystem.AdjustBet(-10000);
        }
    }

    public class BetHandle : UIAnimatedButton
    {
        private SlotMachineUI parent;
        private int timer = 0;
        private int resetFailsafe = 30;
        public bool resetASAP = false;
        public BetHandle(Asset<Texture2D> texture, SlotMachineUI parent) : base(texture)
        {
            tex = texture;
            this.parent = parent;
            frameCounterLimit = 5;
            totalFrames = 6;
            manualClickReset = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if(resetASAP)
            {
                timer++;
                if(timer > resetFailsafe)
                {
                    timer = 0;
                    ResetClick();
                    resetASAP = false;
                }
            }
        }

        public override void LeftClickAction()
        {
            parent.rollGame();
        }
    }

    public class UIAnimatedButton : AnimatedUI
    {
        private bool playAnim = false;
        private bool resetAnimThisFrame = false;
        private bool clickable = true;
        private bool isHeldDown = false;
        private bool isHeldDownLong = false;
        private int holdDownTime = 0;

        public bool canHoldDown = false;
        public int repeatDelay = 30;
        public int clickResetFrame = 0;
        public bool leftClickInteruptsAnim = false;
        public int frameToResetTo = 0;
        public bool manualClickReset = false;

        public override void TickFrame()
        {
            if (!playAnim)
            {
                frame = 0;
            }
            else
            {
                frame = Math.Clamp(frame, 1, totalFrames);
                if (resetAnimThisFrame)
                {
                    frame = frameToResetTo;
                    resetAnimThisFrame = false;
                    if (!clickable && frame == clickResetFrame && !manualClickReset)
                    {
                        clickable = true;
                    }
                    return;
                }
                if(++frameCount > frameCounterLimit)
                {
                    frameCount = 0;
                    frame = (frame + 1);
                    if(frame == totalFrames)
                    {
                        playAnim = false;
                        frame = 0;
                    }
                }
            }

            if(!clickable && frame == clickResetFrame && !manualClickReset)
            {
                clickable = true;
            }
        }
        public UIAnimatedButton(Asset<Texture2D> texture) : base(texture)
        {
            tex = texture;
            frameCounterLimit = 5;
            totalFrames = 6;
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            if(clickable && !canHoldDown)
            {
                playAnim = true;
                if(leftClickInteruptsAnim)
                {
                    resetAnimThisFrame = true;
                }
                LeftClickAction();
                //Main.NewText("LMB");
                clickable = false;
            }
        }
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            if (canHoldDown)
            {
                isHeldDown = true;
            }
        }

        public override void LeftMouseUp(UIMouseEvent evt)
        {
            if(isHeldDown)
            {
                isHeldDown = false;
                isHeldDownLong = false;
            }
            holdDownTime = 0;
        }

        public virtual void LeftClickAction()
        {

        }

        public void ResetClick()
        {
            if (manualClickReset)
            {
                clickable = true;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if(canHoldDown && isHeldDown)
            {
                //Main.NewText("Holding");
                playAnim = true;
                if (leftClickInteruptsAnim)
                {
                    resetAnimThisFrame = true;
                }
                if (holdDownTime == 0)
                {
                    LeftClickAction();
                }
                holdDownTime++;
                if (holdDownTime > repeatDelay)
                {
                    isHeldDownLong = true;
                    holdDownTime--;
                }
                if (isHeldDownLong)
                {
                    LeftClickAction();
                }
            }
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            // When you override UIElement methods, don't forget call the base method
            // This helps to keep the basic behavior of the UIElement
            base.DrawSelf(spriteBatch);
        }
    }

    public class Machine : AnimatedUI
    {
        public Machine(Asset<Texture2D> texture) : base(texture)
        {
            tex = texture;
            frameCounterLimit = 15;
            totalFrames = 2;
        }
    }


    public class AnimatedUI : UIElement
    {
        public int frameCounterLimit;
        public int frameCount;
        public int totalFrames;
        public int frame;

        public float scale = 1f;

        public Asset<Texture2D> tex;
        public AnimatedUI(Asset<Texture2D> texture)
        {
            tex = texture;
        }

        public virtual void TickFrame()
        {
            if(frameCount++ > frameCounterLimit)
            {
                frameCount = 0;
                frame = (frame + 1) % totalFrames;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            TickFrame();
            Rectangle dim = GetInnerDimensions().ToRectangle();
            //Main.NewText($"{new Vector2(dim.Left, dim.Y)} {Main.MouseScreen}");
            var texture = tex.Value;
            var pos = new Vector2(dim.Left, dim.Y);
            spriteBatch.Draw(texture, pos , new Rectangle((texture.Width/totalFrames)*frame, 0, (texture.Width/totalFrames), texture.Height) , Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }

    public class SlotTexturedText : UIElement
    {
        public UIScrollingElement idleText;
        public UIScrollingElement loseText;
        public UIScrollingElement winText;

        public int timer = 0;
        public int timeTillSwitchIdle = 300;

        private SlotGameState internalState;
        public SlotGameState state
        {
            get => internalState;
            set 
            { 
                timer = 0;
                internalState = value;
            }
        }

        public override void OnInitialize()
        {
            Asset<Texture2D> idleTexture = ModContent.Request<Texture2D>("TestContent/UI/Idle");
            idleText = new UIScrollingElement(idleTexture, new Rectangle(0, 0, 130, 10), 62);
            idleText.backwards = true;
            //SlotMachineUI.SetRectangle(idleText, 26f, 39f, 100f, 100f);
            Append(idleText);

            Asset<Texture2D> loseTexture = ModContent.Request<Texture2D>("TestContent/UI/Lose");
            loseText = new UIScrollingElement(loseTexture, new Rectangle(0, 0, 78, 10), 10);
            loseText.backwards = true;
            //SlotMachineUI.SetRectangle(loseText, 46f, 39f, 100f, 100f);
            Append(loseText);

            Asset<Texture2D> winTexture = ModContent.Request<Texture2D>("TestContent/UI/Win");
            winText = new UIScrollingElement(winTexture, new Rectangle(0, 0, 78, 10), 10);
            winText.backwards = true;
            //SlotMachineUI.SetRectangle(winText, 66f, 39f, 100f, 100f);
            Append(winText);

            state = SlotGameState.Idle;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (state)
            {
                case SlotGameState.Idle:
                    idleText.Draw(spriteBatch);
                    break;
                case SlotGameState.Lose:
                    loseText.Draw(spriteBatch);
                    timer++;
                    break;
                case SlotGameState.Win:
                    winText.Draw(spriteBatch);
                    timer++;
                    break;
            }
            if(timer >= timeTillSwitchIdle)
            {
                state = SlotGameState.Idle;
                timer = 0;
            }
        }
    }

    public enum SlotGameState
    {
        Idle,
        Lose,
        Win
    }

    public enum SlotDisplayState
    {
        Idle,
        Playing
    }

    public enum SlotID
    {
        Cherry,
        Lemon,
        Grape,
        Seven,
        Bell,
        Gift,
        Skull
    }
    public class SlotsReal : UIElement
    {
        public UIScrollingElement iconSet1;
        public UIScrollingElement iconSet2;
        public UIScrollingElement iconSet3;

        public SlotMachineUI parent;

        public SlotsReal(SlotMachineUI parent)
        {
            this.parent = parent;
        }

        public static float[] offsets = { 0.06f, 0.2f, 0.33f, 0.48f, 0.63f, 0.77f, 0.92f };

        public SlotDisplayState state;
        public RollResult resultToDisplay;

        public int[] visualResult = [0, 0, 0];

        public int timer = 0;
        public int timeTillNextEvent = 120;
        public int currentEvent = 0;

        public void TriggerSlotDisplay(RollResult result)
        {
            state = SlotDisplayState.Playing;
            resultToDisplay = result;
            if((int)result <= 6)
            {
                for(int i = 0; i < 3; i++)
                {
                    visualResult[i] = (int)result;
                }
            }
            else if ((int)result == 7)
            {
                int result1 = -1;
                int result2 = -1;
                for (int i = 0; i < 3; i++)
                {
                    int rolled = Main.rand.Next(0, 7);
                    while(rolled == result1 || rolled == result2)
                    {
                        rolled = Main.rand.Next(0, 7);
                    }
                    if (i == 0)
                        result1 = rolled;
                    if (i == 1)
                        result2 = rolled;
                    visualResult[i] = rolled;
                }
            }
            else
            {
                int result1 = Main.rand.Next(0, 7);
                int result2 = Main.rand.Next(0, 7);
                while (result1 == result2)
                {
                    result2 = Main.rand.Next(0, 7);
                }

                int which = Main.rand.Next(0, 3);
                switch (which) 
                {

                    case 0:
                        visualResult = [result1, result1, result2];
                        break;
                    case 1:
                        visualResult = [result1,result2, result1];
                        break;
                    case 2:
                        visualResult = [result2, result1, result1];
                        break;
                }
            }
        }

        public override void OnInitialize()
        {
            Asset<Texture2D> icons = ModContent.Request<Texture2D>("TestContent/UI/IconStrip");

            iconSet1 = new UIScrollingElement(icons, new Rectangle(0, 0, 20, 150), 112);
            SlotMachineUI.SetRectangle(iconSet1, 22, 58, 0, 0);
            iconSet2 = new UIScrollingElement(icons, new Rectangle(0, 0, 20, 150), 112);
            SlotMachineUI.SetRectangle(iconSet2, 50, 58, 0, 0);
            iconSet3 = new UIScrollingElement(icons, new Rectangle(0, 0, 20, 150), 112);
            SlotMachineUI.SetRectangle(iconSet3, 78, 58, 0, 0);

            iconSet1.horizontalScroll = false;
            iconSet2.horizontalScroll = false;
            iconSet3.horizontalScroll = false;

            iconSet1.autoScroll = false;
            iconSet2.autoScroll = false;
            iconSet3.autoScroll = false;

            iconSet2.backwards = true;

            Append(iconSet1);
            Append(iconSet2);
            Append(iconSet3);

            state = SlotDisplayState.Idle;
        }

        public override void Update(GameTime gameTime)
        {
            switch (state)
            {
                case SlotDisplayState.Idle:
                    iconSet1.setTime((float)Main.timeForVisualEffects * 0.2f);
                    iconSet2.setTimeBackwards((float)Main.timeForVisualEffects * 0.2f);
                    iconSet3.setTime((float)Main.timeForVisualEffects * 0.2f);
                    break;
                case SlotDisplayState.Playing:
                    timer++;
                    switch (currentEvent)
                    {
                        case 0:
                            iconSet1.setTime((float)Main.timeForVisualEffects * 2f);
                            iconSet2.setTimeBackwards((float)Main.timeForVisualEffects * 2f);
                            iconSet3.setTime((float)Main.timeForVisualEffects * 2f);
                            break;
                        case 1:
                            iconSet2.setTimeBackwards((float)Main.timeForVisualEffects * 2f);
                            iconSet3.setTime((float)Main.timeForVisualEffects * 2f);
                            break;
                        case 2:
                            iconSet3.setTime((float)Main.timeForVisualEffects * 2f);
                            break;
                    }
                    checkTimeExpired();
                    break;
            }
        }

        public void checkTimeExpired()
        {
            if(timer == timeTillNextEvent)
            {
                timer = 0;
                switch (currentEvent)
                {
                    case 0:
                        iconSet1.setTimeDirect(offsets[visualResult[0]]);
                        timeTillNextEvent = 30;
                        break;
                    case 1:
                        iconSet2.setTimeDirect(offsets[visualResult[1]]);
                        timeTillNextEvent = 30;
                        break;
                    case 2:
                        iconSet3.setTimeDirect(offsets[visualResult[2]]);
                        timeTillNextEvent = 60;
                        break;
                    case 3:
                        parent.TriggerSystem(resultToDisplay);
                        timeTillNextEvent = 120;
                        break;
                    case 4:
                        state = SlotDisplayState.Idle;
                        parent.resetHandle();
                        timeTillNextEvent = 120;
                        break;
                }
                currentEvent = (currentEvent + 1) % 5;
            }
        }


    }

    public class UIScrollingElement : UIElement
    {
        public Asset<Texture2D> texture;
        public Rectangle area;
        public bool horizontalScroll = true;
        public bool backwards = false;
        public int leftMask = 0;

        public bool autoScroll = true;
        public float scrollSpeed = 1f;

        public float time = 0f;
        public UIScrollingElement(Asset<Texture2D> texture, Rectangle rect, int offset)
        {
            this.texture = texture;
            area = rect;
            this.leftMask = offset;
        }

        public void setTime(float inputTime)
        {
            inputTime = Math.Clamp(inputTime, 0f, float.MaxValue);
            time = (inputTime % area.Width) / area.Width;
        }

        public void setTimeDirect(float inputTime)
        {
            inputTime = Math.Clamp(inputTime, 0f, 1f);
            time = inputTime;
        }

        public void setTimeBackwards(float inputTime)
        {
            inputTime = Math.Clamp(inputTime, 0f, float.MaxValue);
            time = (area.Width - (inputTime % area.Width)) / area.Width;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle dim = GetInnerDimensions().ToRectangle();
            var pos = new Vector2(dim.Left, dim.Y);
            var pos2 = pos;

            var mask = area;
            var mask2 = area;

            var val = texture.Value;
            if (horizontalScroll)
            {
                int offset;
                if (autoScroll)
                {
                    offset = backwards ? area.Width - (int)Main.timeForVisualEffects % area.Width : (int)Main.timeForVisualEffects % area.Width;
                    offset = (int)((float)offset * scrollSpeed);
                }
                else
                {
                    offset = (int)((float)area.Width * time);
                }
                mask.X = area.Width - offset;
                mask.Width = area.Width - mask.X;

                pos2.X += offset;
                mask2.Width = area.Width - offset;

                var cutoff = leftMask;
                mask2.Width -= cutoff;

                if(mask2.Width < 0)
                {
                    mask.Width += mask2.Width;
                }

            }
            else
            {
                int offset;
                if (autoScroll)
                {
                    offset = backwards ? area.Height - (int)Main.timeForVisualEffects % area.Height : (int)Main.timeForVisualEffects % area.Height;
                    offset = (int)((float)offset * scrollSpeed);
                }
                else
                {
                    offset = (int)((float)area.Height * time);
                }
                mask.Y = area.Height - offset;
                mask.Height = area.Height - mask.Y;

                pos2.Y += offset;
                mask2.Height = area.Height - offset;

                var cutoff = leftMask;
                mask2.Height -= cutoff;

                if (mask2.Height < 0)
                {
                    mask.Height += mask2.Height;
                }
            }

            //Main.NewText($"{mask.X} {mask.Width} {pos2.X} {mask2.Width} ");
            spriteBatch.Draw(texture.Value, pos, mask, Color.White);
            spriteBatch.Draw(texture.Value, pos2, mask2, Color.White);
        }
    }

    [Autoload(Side = ModSide.Client)]
    public class SlotMachineUISystem: ModSystem
    {
        private UserInterface SlotMachineInterface;
        internal SlotMachineUI SlotMachineUI;

        public bool active = false;

        public override void OnWorldLoad()
        {
            SlotMachineUI.slotSystem = Main.LocalPlayer.GetModPlayer<SlotMachineSystem>();
        }

        // These two methods will set the state of our custom UI, causing it to show or hide
        public void ShowMyUI()
        {
            SlotMachineInterface?.SetState(SlotMachineUI);
            active = true;
            //SlotMachineUI.Initialize();
        }

        public void HideMyUI()
        {
            SlotMachineInterface?.SetState(null);
            active = false;
        }

        public override void Load()
        {
            // Create custom interface which can swap between different UIStates
            SlotMachineInterface = new UserInterface();
            // Creating custom UIState
            SlotMachineUI = new SlotMachineUI();

            // Activate calls Initialize() on the UIState if not initialized, then calls OnActivate and then calls Activate on every child element
            SlotMachineUI.Activate();
        }

        public override void UpdateUI(GameTime gameTime)
        {
            // Here we call .Update on our custom UI and propagate it to its state and underlying elements
            if (SlotMachineInterface?.CurrentState != null)
            {
                SlotMachineInterface?.Update(gameTime);
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
                    "TestContent: Slot Machine",
                    delegate {
                        if (SlotMachineInterface?.CurrentState != null)
                        {
                            SlotMachineInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}
