using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using TestContent.Items.Armour.Vanity;
using TestContent.Items.Placeables.Furniture.Paintings;
using TestContent.Items.Pets.TheJonkler;
using System.Runtime.ConstrainedExecution;
using TestContent.Items.Pets.Luigi;
using Steamworks;
using TestContent.Items.Transmog;
using TestContent.Items.Pets.Robomando;

namespace TestContent.Global.Items
{
    public class DevSetDrops: GlobalItem
    {
        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            if (ItemID.Sets.BossBag[item.type] && !ItemID.Sets.PreHardmodeLikeBossBag[item.type])
            {
                var rule = ItemDropRule.Common(ModContent.ItemType<ShyHead>(), 16);
                rule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<ShyTop>(), 1));
                rule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<ShyPants>(), 1));
                itemLoot.Add(rule);

                var sucks = ItemDropRule.Common(ModContent.ItemType<DudeThisItemSucks>(), 20);

                itemLoot.Add(sucks);
            }

            if(item.type == ItemID.WallOfFleshBossBag)
            {
                //LogItemRules(item, itemLoot);

                var rule = ItemDropRule.Common(ModContent.ItemType<LuigiPetItem>(), 5);
                var rule2 = ItemDropRule.Common(ModContent.ItemType<TransmogItem>(), 1);
                itemLoot.Add(rule);
                itemLoot.Add(rule2);
            }

            if(item.type == ItemID.IronCrate || item.type == ItemID.IronCrateHard)
            {
                itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<RobomandoItem>(), 10));
            }
        }

        public void LogItemRules(Item item, ItemLoot itemLoot)
        {
            foreach (var rule in itemLoot.Get())
            {
                Console.WriteLine(rule.ToString());
                if (rule is ItemDropWithConditionRule condRule)
                {
                    Console.WriteLine(condRule.itemId);
                }
                else if (rule is CommonDropNotScalingWithLuck comNonLuck)
                {
                    Console.WriteLine(comNonLuck.itemId);
                }
                else if (rule is CommonDrop com)
                {
                    Console.WriteLine(com.itemId);
                }
                else if (rule is OneFromOptionsNotScaledWithLuckDropRule dropRule)
                {
                    foreach (int i in dropRule.dropIds)
                    {
                        Console.WriteLine(i.ToString());
                    }
                }
            }
        }

        public override void SetDefaults(Item entity)
        {
            if(entity.netID == ItemID.KOCannon)
            {
                ItemID.Sets.ShimmerTransformToItem[entity.netID] = ModContent.ItemType<JonklerPetItem>();
            }
        }
    }
}
