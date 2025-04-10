using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace TestContent.Items.Armour.Vanity
{
    [AutoloadEquip(EquipType.Head)]
    public class ShyHead: ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 18; // Width of the item
            Item.height = 18; // Height of the item
            Item.value = Terraria.Item.sellPrice(gold: 1); // How many coins the item is worth
            Item.rare = ItemRarityID.Cyan; // The rarity of the item
            Item.vanity = true;
        }
    }
}
