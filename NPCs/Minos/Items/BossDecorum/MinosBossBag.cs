using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.NPCs.Minos.Items;

namespace TestContent.NPCs.Minos.Items.BossDecorum
{
    public class MinosBossBag : ModItem
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

            itemLoot.Add(ItemDropRule.NotScalingWithLuck(ModContent.ItemType<MinosMaskItem>(), 7));
            IItemDropRule[] ruleList = [
                ItemDropRule.Common(ModContent.ItemType<DroneController>(), 4),
                ItemDropRule.Common(ModContent.ItemType<FreezeFrame>(), 4),
                ItemDropRule.Common(ModContent.ItemType<KnuckleBlaster>(), 4),
                ItemDropRule.Common(ModContent.ItemType<RailCannon>(), 4)
                ];
            itemLoot.Add(ItemDropRule.AlwaysAtleastOneSuccess(ruleList));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<WhiplashItem>(), 4));
            itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<MinosPrime>()));
        }
    }
}
