using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.NPCs.Minos.Tiles;

namespace TestContent.NPCs.Minos.Items.BossDecorum
{
    public class MinosTrophyItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<MinosTrophyTile>(), 0);
            Item.value = Terraria.Item.buyPrice(gold: 12);
            Item.rare = ItemRarityID.Blue;
        }
    }
}
