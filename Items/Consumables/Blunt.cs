using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using TestContent.Buffs;

namespace TestContent.Items.Consumables
{
    public class Blunt : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;

            // Dust that will appear in these colors when the item with ItemUseStyleID.DrinkLiquid is used
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(240, 240, 240),
                new Color(200, 200, 200),
                new Color(140, 140, 140)
            };
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 26;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item104;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(gold: 1);
            Item.buffType = ModContent.BuffType<Buzzed>(); // Specify an existing buff to be applied when used.
            Item.buffTime = 14400; // The amount of time the buff declared in Item.buffType will last in ticks. 5400 / 60 is 90, so this buff will last 90 seconds.
            Item.holdStyle = ItemHoldStyleID.HoldFront;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {

        }

        public override void HoldStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.direction < 0)
            {
                player.itemLocation.X += Item.width - 5;
            }
            else
            {
                player.itemLocation.X -= Item.width - 5;
            }
            player.itemLocation.Y += Item.height / 2 + 4;

        }

        public override bool CanUseItem(Player player)
        {
            return !player.HasBuff(ModContent.BuffType<Zonked>());
        }

        public override void AddRecipes()
        {
            CreateRecipe(5)
                .AddIngredient(ItemID.Hay, 10)
                .AddIngredient(ItemID.TatteredCloth, 2)
                .AddTile(TileID.Furnaces)
                .Register();
        }
    }
}
