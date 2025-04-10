using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Projectiles.Weapons.Gambling.Prehardmode;

namespace TestContent.Items.Weapons
{
    public class LuckyShot : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 78;
            Item.height = 44;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Terraria.Item.buyPrice(gold: 7);
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 20;
            Item.crit = 0;
            Item.channel = true;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.Rapier;
            Item.noUseGraphic = true;
            Item.knockBack = 1f;
            Item.autoReuse = true;
            Item.shootSpeed = 10f;
            Item.shoot = ModContent.ProjectileType<LuckyShotProjectile>();
            Item.useAmmo = AmmoID.Bullet;
            Item.consumeAmmoOnLastShotOnly = true;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            return player.itemAnimation != 28;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ModContent.ProjectileType<LuckyShotProjectile>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float shootai1 = 0f;
            float shootai2 = 0f;
            if(player.altFunctionUse == 2)
            {
                shootai1 = 1f;
                if(Main.rand.Next(0,6) == 0)
                {
                    shootai2 = 2f;
                }
                else
                {
                    shootai2 = 1f;
                }
            }
            Projectile.NewProjectile(player.GetSource_ItemUse(this.Entity), position, velocity, type, damage, knockback, ai1: shootai1, ai2: shootai2);
            return false;
        }

        public override bool CanUseItem(Player player)
        {
            return player.HasAmmo(this.Entity);
        }
    }
}
