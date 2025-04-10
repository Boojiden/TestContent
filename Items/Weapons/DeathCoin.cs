using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using TestContent.Projectiles.Weapons.Gambling.Prehardmode;

namespace TestContent.Items.Weapons
{
    public class DeathCoin : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(10, 3));
            ItemID.Sets.AnimatesAsSoul[Item.type] = true; // Makes the item have an animation while in world (not held.). Use in combination with RegisterItemAnimation
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 64;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Terraria.Item.buyPrice(gold: 12);
            Item.DamageType = DamageClass.Magic;
            Item.damage = 35;
            Item.crit = 0;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.UseSound = new Terraria.Audio.SoundStyle("TestContent/Assets/Sounds/CoinFlip")
            {
                Volume = 0.6f,
                PitchVariance = 0.3f,
                PlayOnlyIfFocused = true
            };
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.Rapier;
            Item.noUseGraphic = true;
            Item.knockBack = 1f;
            Item.autoReuse = true;
            Item.shootSpeed = 10f;
            Item.shoot = ModContent.ProjectileType<DeathCoinProjectile>();
            Item.mana = 25;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var heads = 0f;
            var additionalChance = MathHelper.Lerp(45f, 0f, ((float)player.statLife - (float)player.statLifeMax2*0.2f )/ (float)player.statLifeMax2);
            additionalChance = Math.Clamp(additionalChance, 0f, 45f);

            if(Main.rand.Next(1,101) <= 50 + (int)additionalChance) 
            {
                heads = 1f;
            }
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, ai1: heads);
            return false;
        }
    }
}
