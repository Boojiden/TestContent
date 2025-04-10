using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Mounts;
using TestContent.NPCs.KiryuPNG.Mounts;

namespace TestContent.NPCs.KiryuPNG.Items
{
    public class LittleKiryuMountItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing; // how the player's arm moves when using the item
            Item.value = Terraria.Item.sellPrice(gold: 7);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = null; // What sound should play when using the item
            Item.noMelee = true; // this item doesn't do any melee damage
            Item.mountType = ModContent.MountType<LittleKiryu>();
        }
    }
}
