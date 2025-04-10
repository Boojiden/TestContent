using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Buffs;
using static Terraria.Recipe;

namespace TestContent.Items.Consumables
{
    public class GiftPotion : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;

            ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<GiftPotion>()] = ItemID.CopperCoin;

            // Dust that will appear in these colors when the item with ItemUseStyleID.DrinkLiquid is used
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(147, 125, 30),
                new Color(229, 214, 127),
                new Color(255, 249, 181)
            };
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 26;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = Terraria.Item.CommonMaxStack;
            Item.consumable = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Terraria.Item.buyPrice(gold: 1);
            Item.buffType = ModContent.BuffType<GiftBuff>(); // Specify an existing buff to be applied when used.
            Item.buffTime = 18000; // The amount of time the buff declared in Item.buffType will last in ticks. 5400 / 60 is 90, so this buff will last 90 seconds.
        }

        public override void OnConsumeItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                player.AddBuff(BuffID.Tipsy, Item.buffTime);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Ale, 1)
                .AddIngredient(ItemID.Fireblossom, 1)
                .AddIngredient(ItemID.GoldCoin, 5)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}
