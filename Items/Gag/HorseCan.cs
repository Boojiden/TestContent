using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using TestContent.Mounts;
using TestContent.Global.ModSystems;

namespace TestContent.Items.Gag
{
    public class HorseCan : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing; // how the player's arm moves when using the item
            Item.value = Item.sellPrice(gold: 7);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = null; // What sound should play when using the item
            Item.noMelee = true; // this item doesn't do any melee damage
            Item.mountType = ModContent.MountType<HorseMount>();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Artifacting>(), 10)
                .AddRecipeGroup(CustomRecipeGroup.SaddleRecipeGroup)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
