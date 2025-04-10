using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;

namespace TestContent.Items.Placeables.Furniture.Paintings
{
    public class DudeThisItemSucks: ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.Paintings.DudeThisSucks>(), 0);
            Item.value = Terraria.Item.buyPrice(gold: 12);
            Item.rare = ItemRarityID.Purple;
        }
    }
}
