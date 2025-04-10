using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace TestContent.NPCs.KiryuPNG.Items
{
    public class KiryuBossBag : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.BossBag[Type] = true;

            Item.ResearchUnlockCount = 3;
        }

        public override void SetDefaults()
        {
            Item.maxStack = Terraria.Item.CommonMaxStack;
            Item.consumable = true;
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.expert = true; // This makes sure that "Expert" displays in the tooltip and the item name color changes
        }

        public override bool CanRightClick()
        {
            return true;
        }

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            // We have to replicate the expert drops from MinionBossBody here

            itemLoot.Add(ItemDropRule.NotScalingWithLuck(ModContent.ItemType<KiryuMaskItem>(), 7));
            var box = ItemDropRule.Common(ModContent.ItemType<DameDaneBoxItem>(), 4);
            var little = ItemDropRule.Common(ModContent.ItemType<LittleKiryuMountItem>(), 5);
            var fists = ItemDropRule.Common(ModContent.ItemType<FistItem>(), 4);
            var bike = ItemDropRule.Common(ModContent.ItemType<BicycleThrowingItem>(), 4);
            itemLoot.Add(ItemDropRule.AlwaysAtleastOneSuccess([box, little, fists, bike]));
            itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<KiryuPNG>()));
        }
    }
}
