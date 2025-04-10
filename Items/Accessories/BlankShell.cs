using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using TestContent.UI;

namespace TestContent.Items.Accessories
{
    public class BlankShell : ModItem
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
            player.GetDamage(DamageClass.Ranged) += 0.05f;
            player.GetModPlayer<SlotMachineSystem>().doubleOrNothing = true;
        }
    }
}
