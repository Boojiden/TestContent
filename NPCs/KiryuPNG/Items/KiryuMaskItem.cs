using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace TestContent.NPCs.KiryuPNG.Items
{
    [AutoloadEquip(EquipType.Head)]
    public class KiryuMaskItem: ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 28;

            // Common values for every boss mask
            Item.rare = ItemRarityID.Blue;
            Item.value = Terraria.Item.sellPrice(silver: 75);
            Item.vanity = true;
            Item.maxStack = 1;
        }
    }
}
