using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.Localization;
using Terraria;
using Terraria.ModLoader;

namespace TestContent.Items.Consumables
{
    public class CokeAndFriesHalo : ModItem
    {
        public static LocalizedText RestoreLifeText { get; private set; }

        public override void SetStaticDefaults()
        {
            RestoreLifeText = this.GetLocalization(nameof(RestoreLifeText));

            Item.ResearchUnlockCount = 30;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 26;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item2;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(gold: 10);

            Item.healLife = 100; // While we change the actual healing value in GetHealLife, Item.healLife still needs to be higher than 0 for the item to be considered a healing item
            Item.potion = true; // Makes it so this item applies potion sickness on use and allows it to be used with quick heal
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // Find the tooltip line that corresponds to 'Heals ... life'
            // See https://tmodloader.github.io/tModLoader/html/class_terraria_1_1_mod_loader_1_1_tooltip_line.html for a list of vanilla tooltip line names
            TooltipLine line = tooltips.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "HealLife");

            if (line != null)
            {
                // Change the text to 'Heals max/2 (max/4 when quick healing) life'
                line.Text = Language.GetTextValue("CommonItemTooltip.RestoresLife", 200);
            }
        }

        public override void GetHealLife(Player player, bool quickHeal, ref int healValue)
        {
            // Make the item heal half the player's max health normally, or one fourth if used with quick heal
            healValue = 200;
        }

        public override void OnConsumeItem(Player player)
        {
            player.AddBuff(BuffID.Slow, 1200);
        }

        public override void AddRecipes()
        {
            CreateRecipe(5)
                .AddIngredient(ItemID.CreamSoda)
                .AddIngredient(ItemID.Fries)
                .AddIngredient(ModContent.ItemType<Artifacting>(), 5)
                .Register();
        }
    }
}