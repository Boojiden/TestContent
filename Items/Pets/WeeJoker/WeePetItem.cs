using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TestContent.Items.Pets.WeeJoker
{
    public class WeePetItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.ZephyrFish); // Copy the Defaults of the Zephyr Fish Item.
            Item.UseSound = SoundID.Item2;
            Item.shoot = ModContent.ProjectileType<WeePetProj>(); // "Shoot" your pet projectile.
            Item.buffType = ModContent.BuffType<WeePetBuff>(); // Apply buff upon usage of the Item.
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                player.AddBuff(Item.buffType, 3600);
            }
            return true;
        }

    }
}
