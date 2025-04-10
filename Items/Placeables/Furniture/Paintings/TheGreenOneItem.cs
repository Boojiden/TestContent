using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace TestContent.Items.Placeables.Furniture.Paintings
{
    public class TheGreenOneItem: ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.Paintings.TheGreenOne>(), 0);
            Item.value = Terraria.Item.buyPrice(gold: 15);
        }
    }
}
