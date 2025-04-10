using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Projectiles;
using TestContent.Projectiles.Weapons.Gambling.Hardmode;

namespace TestContent.Items.Weapons
{
    public class TestTrailWeapon : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.rare = ItemRarityID.Red;
            Item.value = Terraria.Item.buyPrice(gold: 8);
            Item.DamageType = DamageClass.Magic;
            Item.damage = 44;
            Item.crit = 0;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 1f;
            Item.autoReuse = true;
            Item.shootSpeed = 10f;
            Item.shoot = ModContent.ProjectileType<TestTrailProjectile>();
            Item.mana = 10;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] < 3;
        }
    }
}
