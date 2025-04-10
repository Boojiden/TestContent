using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace TestContent.Items.Pets.YesMan
{
    public class YesManItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.ZephyrFish); // Copy the Defaults of the Zephyr Fish Item.
            Item.UseSound = null;
            Item.shoot = ModContent.ProjectileType<YesManProj>(); // "Shoot" your pet projectile.
            Item.buffType = ModContent.BuffType<YesManBuff>(); // Apply buff upon usage of the Item.
            Item.rare = ItemRarityID.LightRed;
            Item.value = Terraria.Item.buyPrice(gold: 1);
        }
    }
}
