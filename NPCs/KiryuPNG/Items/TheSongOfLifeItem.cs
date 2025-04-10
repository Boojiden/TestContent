using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using TestContent.Items.Pets.TheJonkler;
using TestContent.NPCs.KiryuPNG.Projectiles;
using TestContent.NPCs.KiryuPNG.Buffs;

namespace TestContent.NPCs.KiryuPNG.Items
{
    public class TheSongOfLifeItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.ZephyrFish); // Copy the Defaults of the Zephyr Fish Item.
            Item.UseSound = SoundID.Item2;
            Item.shoot = ModContent.ProjectileType<TheSongOfLife>(); // "Shoot" your pet projectile.
            Item.buffType = ModContent.BuffType<TheSongOfLifeBuff>(); // Apply buff upon usage of the Item.
            Item.master = true;
            Item.value = Terraria.Item.buyPrice(gold: 3);
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
