using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TestContent.Items.Accessories;
using TestContent.Items.Ammo;
using TestContent.Items.Pets.WeeJoker;
using TestContent.Items.Pets.YesMan;
using TestContent.Items.Weapons;
using TestContent.UI;

namespace TestContent.Items.Consumables
{
    public class SlotGiftBag: ModItem
    {

        public int bet = 0;

        public const int maxScaledBet = 1000000;
        public override void SetStaticDefaults()
        { 
            Item.ResearchUnlockCount = 3;
        }

        public override void SaveData(TagCompound tag)
        {
            tag["bet"] = bet;
        }

        public override void LoadData(TagCompound tag)
        {
            bet = tag.Get<int>("bet");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(bet);
        }

        public override void NetReceive(BinaryReader reader)
        {
            bet = reader.ReadInt32();
        }

        public override void SetDefaults()
        {
            Item.maxStack = Terraria.Item.CommonMaxStack;
            Item.consumable = true;
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightPurple;
        }

        public override bool CanRightClick()
        {
            return true;
        }

        public override bool CanStack(Item source)
        {
            SlotGiftBag other = source.ModItem as SlotGiftBag;
            return other.bet == bet;
        }
        public override void OnSpawn(IEntitySource source)
        {
            if(source is EntitySource_DropAsItem { Entity : Player })
            {
                var entitysource = source as EntitySource_DropAsItem;
                Player player = entitysource.Entity as Player;
                bet = player.GetModPlayer<SlotMachineSystem>().lastBet;
            }
        }

        public void SpawnFromList(int[] items, Player player)
        {
            int index = Main.rand.Next(0, items.Length);

            if (items[index] == ModContent.ItemType<RouletteGun>())
            {
                SpawnFromListWithStackRange([ModContent.ItemType<ChipAmmo>()], player, 60, 200);
            }
            player.QuickSpawnItem(player.GetSource_ItemUse(Item), items[index]);
        }

        public void SpawnFromListWithStackRange(int[] items, Player player, int min, int max)
        {
            int index = Main.rand.Next(0, items.Length);
            int amount = Main.rand.Next(min, max+1);
            player.QuickSpawnItem(player.GetSource_ItemUse(Item), items[index], amount);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "SlotGiftBag", $"Placed Bet: {(int)(bet / 10000)}") { OverrideColor = Color.Yellow });
        }

        public override void RightClick(Player player)
        {
            bool droppedAccessory = false;
            bool droppedWeapon = false;
            bool droppedSmallItem = false;
            bool droppedFiller = false;
            
            
            bool isHardmode = Main.hardMode;
            int accessoryChance = 10;
            accessoryChance += (int)MathHelper.Lerp(0f, 80f,(float)bet / (float)maxScaledBet);

            int[] items;
            if (Main.rand.Next(1,101) < accessoryChance)
            {
                items = [ModContent.ItemType<PlatinumChip>(), ModContent.ItemType<TrickCoin>(), ModContent.ItemType<BlankShell>(), ModContent.ItemType<YesManItem>(), ModContent.ItemType<WeePetItem>()];
                SpawnFromList(items, player);
                droppedAccessory = true;
            }

            int weaponChance = 5;
            weaponChance += (int)MathHelper.Lerp(0f, 95f, (float)bet / (float)maxScaledBet);
            if (Main.rand.Next(1, 101) < weaponChance)
            {
                items = [ModContent.ItemType<JohnnySword>(), ModContent.ItemType<DeathCoin>(), ModContent.ItemType<LuckyShot>(), ModContent.ItemType<JacksItem>()];
                if(isHardmode)
                {
                    items = [ModContent.ItemType<JohnnySword>(), ModContent.ItemType<DeathCoin>(), ModContent.ItemType<LuckyShot>(), ModContent.ItemType<JacksItem>(),
                    ModContent.ItemType<TheNeedle>(), ModContent.ItemType<MidasTouch>(), ModContent.ItemType<RouletteGun>(), ModContent.ItemType<MicroDemonItem>()];
                }
                SpawnFromList(items, player);
                droppedWeapon = true;
            }

            int fillerChance = 25;
            int rolls = 1;
            fillerChance += (int)MathHelper.Lerp(0f, 1000f, (float)bet / (float)maxScaledBet);
            if(fillerChance > 90f)
            {
                float diff = fillerChance - 90f;
                rolls = ((int)(diff / 50f));
                rolls = Math.Clamp(rolls, 1, 10);
                fillerChance = 90;
            }

            for (int i = 0; i < rolls; i++)
            {
                if(Main.rand.Next(1, 101) < fillerChance)
                {
                    items = [ModContent.ItemType<Blunt>(),
                    ItemID.Emerald, ItemID.Diamond, ItemID.Ruby, ItemID.RestorationPotion, ItemID.HealingPotion, ModContent.ItemType<GiftPotion>()];
                    if (isHardmode)
                    {
                        items = [ModContent.ItemType<BigWeed>(), ModContent.ItemType<Blunt>(), 
                            ItemID.Emerald, ItemID.Diamond, ItemID.Ruby,  
                            ItemID.RestorationPotion, ItemID.GreaterHealingPotion, ModContent.ItemType<GiftPotion>()];
                    }
                    SpawnFromListWithStackRange(items, player, 1, 3 + (int)MathHelper.Lerp(0f, 5f, (float)bet / (float)maxScaledBet));
                    droppedFiller = true;
                }

            }

            int smallItemChance = 15 + (int)MathHelper.Lerp(0f, 85f, (float)bet / (float)maxScaledBet);
            if(Main.rand.Next(1, 101) < smallItemChance)
            {
                items = [ItemID.LifeCrystal, ItemID.ManaCrystal];
                if(isHardmode)
                {
                    items = [ItemID.LifeCrystal, ItemID.ManaCrystal, ItemID.LifeFruit];
                }
                SpawnFromListWithStackRange(items, player, 1, 1 + (int)MathHelper.Lerp(0f, 2f, (float)bet / (float)maxScaledBet));
                droppedSmallItem = true;
            }

            if (!droppedAccessory && !droppedFiller && !droppedSmallItem && !droppedWeapon) 
            {
                player.QuickSpawnItem(player.GetSource_ItemUse(Item), ModContent.ItemType<TrollMessage>());
            }
        }
    }
}
