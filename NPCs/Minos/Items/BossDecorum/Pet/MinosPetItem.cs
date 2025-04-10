using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TestContent.NPCs.Minos.Items.BossDecorum.Pet
{
    public class MinosPetItem : ModItem
    {
        public override string Texture => "TestContent/NPCs/Minos/Items/BossDecorum/Pet/MinosPetProjectile";
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.ZephyrFish); // Copy the Defaults of the Zephyr Fish Item.
            Item.UseSound = SoundID.Item2;
            Item.shoot = ModContent.ProjectileType<MinosPetProjectile>(); // "Shoot" your pet projectile.
            Item.buffType = ModContent.BuffType<MinosPetBuff>(); // Apply buff upon usage of the Item.
            Item.master = true;
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
