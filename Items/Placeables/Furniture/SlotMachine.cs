using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace TestContent.Items.Placeables.Furniture
{
    public class SlotMachine : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.SlotMachineTile>(), 0);
            Item.value = Terraria.Item.buyPrice(gold: 30);
            Item.rare = ItemRarityID.LightRed;
        }
    }
}
