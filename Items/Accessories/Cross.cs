using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using TestContent.UI;
using TestContent.Players;

namespace TestContent.Items.Accessories
{
    public class Cross : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 37;
            Item.height = 16;
            Item.accessory = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Terraria.Item.buyPrice(0, 14, 0, 0);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ReviveEffect>().hasReviveAccessory = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddTile(TileID.MythrilAnvil)
                .AddIngredient(ItemID.CrossNecklace)
                .AddIngredient(ModContent.ItemType<Artifacting>(), 10)
                .Register();
        }
    }
}
