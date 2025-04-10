using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace TestContent.Items.Pets.Robomando
{
    public class RobomandoItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.ZephyrFish); // Copy the Defaults of the Zephyr Fish Item.
            Item.UseSound = null;
            Item.shoot = ModContent.ProjectileType<RobomandoProj>(); // "Shoot" your pet projectile.
            Item.buffType = ModContent.BuffType<RobomandoBuff>(); // Apply buff upon usage of the Item.
            Item.rare = ItemRarityID.LightRed;
            Item.value = Terraria.Item.buyPrice(gold: 1);
        }
    }
}
