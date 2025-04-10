using System;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Mono.Cecil;
using static Terraria.ModLoader.PlayerDrawLayer;
using TestContent.Players;
using System.Collections.Generic;
using System.Data.Common;
using TestContent.Projectiles.Weapons.Gambling.Hardmode;

namespace TestContent.Items.Weapons
{
    public class TheNeedle : ModItem
    {
        public int attackType = 0; // keeps track of which attack it is
        public int comboExpireTimer = 0; // we want the attack pattern to reset if the weapon is not used for certain period of time
        public int attackState = 0;

        public override void SetDefaults()
        {
            // Common Properties
            Item.width = 48;
            Item.height = 48;
            Item.value = Item.sellPrice(gold: 7, silver: 80);
            Item.rare = ItemRarityID.Orange;

            // Use Properties
            // Note that useTime and useAnimation for this item don't actually affect the behavior because the held projectile handles that. 
            // Each attack takes a different amount of time to execute
            // Conforming to the item useTime and useAnimation makes it much harder to design
            // It does, however, affect the item tooltip, so don't leave it out.
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;

            // Weapon Properties
            Item.knockBack = 3.5f;  // The knockback of your sword, this is dynamically adjusted in the projectile code.
            Item.autoReuse = true; // This determines whether the weapon has autoswing
            Item.damage = 65; // The damage of your sword, this is dynamically adjusted in the projectile code.
            Item.DamageType = DamageClass.Melee; // Deals melee damage
            Item.noMelee = true;  // This makes sure the item does not deal damage from the swinging animation
            Item.noUseGraphic = true; // This makes sure the item does not get shown when the player swings his hand

            // Projectile Properties
            Item.shoot = ModContent.ProjectileType<NeedleHeldProjectile>(); // The sword as a projectile
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Main.myPlayer, attackType);
            attackType = (attackType + 1) % 3; // Increment attackType to make sure next swing is different
            comboExpireTimer = 0; // Every time the weapon is used, we reset this so the combo does not expire

            return false; // return false to prevent original projectile from being shot
        }

        public override void UpdateInventory(Player player)
        {
            if (comboExpireTimer++ >= 120) // after 120 ticks (== 2 seconds) in inventory, reset the attack pattern
                attackType = 0;
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            int maxScalingValue = 2000000;
            var weapons = player.GetModPlayer<PlayerWeapons>();

            float bonus = Math.Clamp(MathHelper.Lerp(0, 2, ((float)weapons.inventoryValue / (float)maxScalingValue)), 0f, 2f);

            damage += bonus;
        }

        public override bool MeleePrefix()
        {
            return true; // return true to allow weapon to have melee prefixes (e.g. Legendary)
        }
        /*
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HellstoneBar, 10)
                .AddTile(TileID.Hellforge)
                .Register();
        }
        */

        
    }
}
