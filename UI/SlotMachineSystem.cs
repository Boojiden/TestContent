using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.RGB;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TestContent.Dusts;
using TestContent.Global.Items;
using TestContent.Items.Consumables;
using TestContent.Items.Placeables.Furniture.Paintings;
using TestContent.Tiles.Furniture;

namespace TestContent.UI
{
    public class SlotMachineSystem: ModPlayer
    {
        public int currentBet = 0;
        public int setBet = 0;
        public const int maxBet = 10000000;

        public int lastBet = 0;

        public SlotMachineUISystem system;

        public Point blockInteractionPoint;
        public Rectangle dustRectangle;
        public Tile slotTile;

        public float distanceThreshold = 10;

        public int timer = 0;
        public int timerForDustSpawn = 60;
        private bool _EmitDust;

        public bool platDust = false;
        public bool EmitDust
        {
            get => _EmitDust;
            set
            {
                timer = 0;
                _EmitDust = value;
            }
        }

        public bool betterChances = false;
        public bool skipPaymentChance = false;
        public bool doubleOrNothing = false;
        public bool weighGifts = false;

        public override void ResetEffects()
        {
            betterChances = false;
            skipPaymentChance = false;
            doubleOrNothing = false;
            weighGifts = false;
        }

        public override void OnEnterWorld()
        {
            blockInteractionPoint = new Point(-1, -1);
            system = ModContent.GetInstance<SlotMachineUISystem>();
        }
        public void AdjustBet(int change)
        {
            var newBet = currentBet + change;
            newBet = Math.Clamp(newBet, 0, maxBet);
            if(Player.CanAfford(newBet))
            {
                currentBet = newBet;
            }
        }

        public void DoResult(RollResult result)
        {
            lastBet = setBet;
            int winnings = 0;
            switch (result)
            {
                case RollResult.Cherry:
                    winnings = setBet * 2;
                    break;
                case RollResult.Lemon:
                    winnings = setBet * 3;
                    break;
                case RollResult.Grape:
                    winnings = setBet * 4;
                    break;
                case RollResult.Bell:
                    winnings = setBet * 5;
                    break;
                case RollResult.Gift:
                    int slot = Player.QuickSpawnItem(Player.GetSource_DropAsItem("Bet:" + setBet), ModContent.ItemType<SlotGiftBag>());
                    winnings = setBet * 2;
                    break;
                case RollResult.Seven:
                    winnings = setBet * 10;
                    break;
                case RollResult.Skull:
                    Player.KillMe(PlayerDeathReason.ByCustomReason($"{Player.name} got extremely unlucky"), 9999, 1);
                    break;
                case RollResult.Fail:
                    break;
                case RollResult.Close:
                    break;

            }
            SpawnCoins(winnings);
            ResetBet();
        }

        public static RollResult SimulateRoll(bool betterChances, bool doubleOrNothing, bool weighGifts)
        {
            int success = 85;
            if (betterChances)
            {
                success -= 10;
            }
            if (doubleOrNothing)
            {
                success += 10;
            }
            if (Main.rand.Next(1, 101) < success)
            {
                //Main.NewText("Failed initial roll");
                return Main.rand.NextBool() ? RollResult.Fail : RollResult.Close;
            }
            LayeredRollingSystem lrs = new LayeredRollingSystem();
            lrs.Add(60);//Cherry
            lrs.Add(30);//Lemon
            lrs.Add(20);//Grape
            lrs.Add(1); //777
            lrs.Add(10); //bell
            if (weighGifts)//Make gifts more common for weighted players
            {
                lrs.Add(60);
            }
            else
            {
                lrs.Add(20);
            }
            lrs.Add(1); //skull

            int roll = lrs.Roll();
            return (RollResult)(roll);
        }
        public RollResult Roll()
        {
            if (currentBet <= 0 || !Player.CanAfford(currentBet))
            {
                return RollResult.None;
            }
            setBet = currentBet;
            if (!skipPaymentChance || Main.rand.Next(1,6) != 1) 
            {
                Player.PayCurrency(currentBet);
            }

            if (doubleOrNothing)
            {
                setBet *= 2;
            }
            //Luck can reroll a bad outcome (or good outcome given bad luck)
            var result = SimulateRoll(betterChances, doubleOrNothing, weighGifts);
            var luck = Player.luck;
            if (luck > 0f && (int)result > 6)
            {
                if(Main.rand.NextFloat() < luck)
                {
                    //Main.NewText($"Positive Luck Roll: Roll was {result.ToString()}, Luck is {luck}");
                    result = SimulateRoll(betterChances, doubleOrNothing, weighGifts);
                }
            }
            else if(luck < 0f && (int)result <= 6)
            {
                if (Main.rand.NextFloat() < Math.Abs(luck))
                {
                    //Main.NewText($"Negative Luck Roll: Roll was {result.ToString()}, Luck is {luck}");
                    result = SimulateRoll(betterChances, doubleOrNothing, weighGifts);
                }
            }
            return result;

            /*int roll = Main.rand.Next(1, 101);

            if(roll < 60)
            {
                
            }
            if (roll < 70)
            {
                return RollResult.Bell;
            }
            if(roll < 98)
            {
                return RollResult.Gift;
            }
            if(roll == 99)
            {
                return RollResult.Skull;
            }
            else
            {
                return RollResult.Seven;
            }*/
        }

        public void SpawnCoins(int amount)
        {
            int plat = amount / 1000000;
            int gold = (amount % 1000000) / 10000;
            platDust = false;

            if(plat >= 1)
            {
                Player.QuickSpawnItem(Player.GetSource_DropAsItem(), ItemID.PlatinumCoin, plat);
                EmitDust = true;
                platDust = true;
            }

            if(gold >= 1)
            {
                Player.QuickSpawnItem(Player.GetSource_DropAsItem(), ItemID.GoldCoin, gold);
                EmitDust = true;
            }
            
        }

        public void ResetBet()
        {
            int plat = 0;
            var index = Player.FindItem(ItemID.PlatinumCoin);
            if (index != -1)
            {
                plat = Player.CountItem(ItemID.PlatinumCoin);
            }
            int gold = 0;
            index = Player.FindItem(ItemID.GoldCoin);
            if (index != -1)
            {
                gold = Player.CountItem(ItemID.GoldCoin);
            }

            var maxbet = (plat * 1000000) + (gold * 10000);
            currentBet = Math.Clamp(currentBet, 0, maxbet);
        }

        public void ToggleUI()
        {
            if(system.active == true)
            {
                system.HideMyUI();
            }
            else
            {
                system.ShowMyUI();
            }
        }

        public override void PostUpdate()
        {
            if(Main.myPlayer == Player.whoAmI && system.active == true)
            {
                if(!slotTile.HasTile || slotTile.TileType != ModContent.TileType<SlotMachineTile>())
                {
                    system.HideMyUI();
                }
                var dist = (Player.Bottom.ToTileCoordinates() - blockInteractionPoint).ToVector2().Length();
                if(dist > distanceThreshold)
                {
                    system.HideMyUI();
                    //Main.NewText($"Hidden: {Player.Bottom.ToTileCoordinates()} {blockInteractionPoint}");
                }

                if (!Player.CanAfford(currentBet))
                {
                    currentBet = 0;
                }

                if (_EmitDust)
                {
                    DoDustEffects();
                    if (timer++ > timerForDustSpawn)
                    {
                        EmitDust = false;
                    }
                }
            }
        }

        public void DoDustEffects()
        {
            if (Main.gamePaused || !Main.instance.IsActive)
            {
                return;
            }
            if (!Lighting.UpdateEveryFrame && _EmitDust)
            {
                if(Main.timeForVisualEffects % 5 != 0)
                {
                    return;
                }

                //Main.NewText($"{dustRectangle} {Player.position}");
                Tile tile = slotTile;
                int j = dustRectangle.Y;
                float avg = ((dustRectangle.X * 16 + 2) + ((dustRectangle.X + 1) * 16 + 2)) / 2;
                Vector2 pos = new Vector2(avg, j * 16);
                // Only emit dust from the top tiles, and only if toggled on. This logic limits dust spawning under different conditions.
                var dir = -Vector2.UnitY;
                dir = dir.RotatedByRandom(Math.PI / 4);
                dir *= 40;
                Dust dust = Dust.NewDustDirect(pos, 1, 1, ModContent.DustType<GoldCoin>(), dir.X, dir.Y, 1);
                dust.position.X += Main.rand.Next(-8, 8);

                dust.alpha += Main.rand.Next(100);
                dust.velocity *= 0.2f;
                dust.velocity.Y -= 0.5f + Main.rand.Next(10) * 0.1f;
                dust.fadeIn = 0.5f + Main.rand.Next(10) * 0.1f;

                if (platDust)
                {
                    dir = -Vector2.UnitY;
                    dir = dir.RotatedByRandom(Math.PI / 4);
                    dir *= 40;
                    dust = Dust.NewDustDirect(pos, 1, 1, ModContent.DustType<PlatCoin>(), dir.X, dir.Y, 1);
                    dust.position.X += Main.rand.Next(-8, 8);

                    dust.alpha += Main.rand.Next(100);
                    dust.velocity *= 0.2f;
                    dust.velocity.Y -= 0.5f + Main.rand.Next(10) * 0.1f;
                    dust.fadeIn = 0.5f + Main.rand.Next(10) * 0.1f;
                }
            }
        }

        public enum RollResult
        {
            Cherry,
            Lemon,
            Grape,
            Seven,
            Bell,
            Gift,
            Skull,
            Fail,
            Close,
            None
        }

        public class LayeredRollingSystem
        {
            private List<int> layers;

            public LayeredRollingSystem()
            {
                layers = new List<int>();
            }

            public int Add(int chance)
            {
                layers.Add(chance);
                return layers.Count-1;
            }

            public void Remove(int index)
            {
                if(layers.Count >= index || index < 0)
                {
                    return;
                }
                layers.Remove(index);
            }

            public int getSum()
            {
                int sum = 0;
                for (int i = 0; i < layers.Count; i++)
                {
                    sum += layers[i];
                }
                return sum;
            }

            public void ModifyLayer(int index, int newAmount)
            {
                if (layers.Count >= index || index < 0)
                {
                    return;
                }
                layers[index] = newAmount;
            }

            public int Roll()
            {
                int sum = getSum();
                int roll = Main.rand.Next(1, sum+1);
                /*Main.NewText("LRS total:" + sum);
                Main.NewText("LRS roll: " + roll);*/
                for (int i = 0; i < layers.Count; i++) 
                {
                    var prev = roll;
                    roll -= layers[i];
                    /*Main.NewText(prev + "-" + layers[i] +"(index "+i+ ") =" + roll);*/
                    if(roll <= 0)
                    {
                        /*Main.NewText("Returns " + i);*/
                        return i;
                    }
                }
                return 0;
            }
        }
    }
}
