using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.Localization;
using TestContent.Items.Gag;

namespace TestContent.Global.ModSystems
{
    public class CustomRecipeGroup: ModSystem
    {
        public static RecipeGroup MythilRecipeGroup;
        public static RecipeGroup SaddleRecipeGroup;
        public static RecipeGroup AdamantiteRecipeGroup;

        public override void Unload()
        {
            MythilRecipeGroup = null;
            SaddleRecipeGroup = null;
            AdamantiteRecipeGroup = null;
        }

        public override void AddRecipeGroups()
        {
            // Create a recipe group and store it
            // Language.GetTextValue("LegacyMisc.37") is the word "Any" in English, and the corresponding word in other languages
            MythilRecipeGroup = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.MythrilOre)}",
                ItemID.MythrilOre, ItemID.OrichalcumOre);

            SaddleRecipeGroup = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetNPCName(NPCID.BestiaryGirl)} Saddle", 
                ItemID.MajesticHorseSaddle, ItemID.PaintedHorseSaddle, ItemID.DarkHorseSaddle);

            AdamantiteRecipeGroup = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.AdamantiteBar)}",
                ItemID.AdamantiteBar, ItemID.TitaniumBar);

            // To avoid name collisions, when a modded items is the iconic or 1st item in a recipe group, name the recipe group: ModName:ItemName
            RecipeGroup.RegisterGroup("TestContent:MythrilRecipeGroup", MythilRecipeGroup);
            RecipeGroup.RegisterGroup("TestContent:ZoologistSaddleRecipeGroup", SaddleRecipeGroup);
            RecipeGroup.RegisterGroup("TestContent:AdamantiteRecipeGroup", AdamantiteRecipeGroup);

            //RecipeGroup.recipeGroups[RecipeGroupID.Fruit].ValidItems.Add(ModContent.ItemType<Fish>());
        }
    }
}
