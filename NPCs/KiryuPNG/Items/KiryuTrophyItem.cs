using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.NPCs.KiryuPNG.Tiles;

namespace TestContent.NPCs.KiryuPNG.Items
{
    public class KiryuTrophyItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<KiryuTrophy>(), 0);
            Item.value = Terraria.Item.buyPrice(gold: 12);
            Item.rare = ItemRarityID.Blue;
        }
    }
}
