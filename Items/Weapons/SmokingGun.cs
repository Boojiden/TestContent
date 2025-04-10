using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using rail;
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
using TestContent.Global.ModSystems;
using TestContent.Items.Consumables;
using TestContent.Projectiles.Weapons;

namespace TestContent.Items.Weapons
{
    public class SmokingGun : ModItem
    {
        public static SoundStyle shootSniper;
        public override void SetDefaults()
        {
            shootSniper = new SoundStyle("TestContent/Assets/Sounds/Sniper")
            {
                Volume = 1f,
                PitchVariance = 0.2f,
                MaxInstances = 1,
                PlayOnlyIfFocused = true
            };
            Item.width = 64;
            Item.height = 26;
            Item.rare = ItemRarityID.Orange;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.UseSound = shootSniper;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 80;
            Item.knockBack = 4f;
            Item.noMelee = true;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Bullet;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            var mousepos = Main.MouseWorld;
            var dir = (mousepos - player.Center).SafeNormalize(Vector2.UnitX);
            float time = 1 - ((float)player.itemTime / (float)Item.useTime);
            var shootoff = (M24HoldOut.ShootingLerpAlt(time) * 0.05f);
            if (player.direction < 0)
            {
                player.itemRotation = dir.ToRotation() + (shootoff);
                player.itemRotation -= (float)Math.PI;
            }
            else
            {
                player.itemRotation = dir.ToRotation() - (shootoff);
            }
            //player.itemRotation += ;            
            //Main.NewText($"Rotation: {player.itemRotation}, ItemTime: {player.itemTime}");
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-Item.width / 2, -(Item.height / 2) + 2f);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if(type == ProjectileID.Bullet)
            {
                type = ModContent.ProjectileType<SmokingGunBlunt>();
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            /*
            var mousepos = Main.MouseWorld;
            var dir = (mousepos - player.Center).SafeNormalize(Vector2.UnitX);
            var vel = dir*Item.shootSpeed;
            */
            int first = Projectile.NewProjectile(source, player.Center, velocity, type, damage, knockback);
            int second = Projectile.NewProjectile(source, player.Center, velocity, ProjectileID.BallofFire, (int)(damage*1.2f), knockback);
            Main.projectile[second].DamageType = DamageClass.Ranged;

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddTile(TileID.MythrilAnvil)
                .AddRecipeGroup(CustomRecipeGroup.AdamantiteRecipeGroup, 7)
                .AddIngredient<BigWeed>()
                .AddIngredient(ItemID.Musket)
                .Register();
        }
    }
}
