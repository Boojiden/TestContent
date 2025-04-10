using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TestContent.Items.Weapons
{
    public class LmaoBox : ModItem
    {
        // The Display Name and Tooltip of this item can be edited in the Localization/en-US_Mods.TestContent.hjson file.

        public override void SetDefaults()
        {
            Item.damage = 70;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6;
            Item.value = 10000;
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.DirtBlock, 10);
            recipe.AddIngredient(ItemID.LunarOre, 1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }
    }
}