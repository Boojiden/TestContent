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
using TestContent.Dusts;
using TestContent.Projectiles.Weapons.Gambling.Prehardmode;

namespace TestContent.Items.Weapons
{
    public class JohnnySword : ModItem
    {
        public SoundStyle cardThrow;
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 96;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Terraria.Item.buyPrice(gold: 12);
            Item.DamageType = DamageClass.Melee;
            Item.damage = 16;
            Item.crit = 0;
            Item.channel = true;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noUseGraphic = true;
            Item.knockBack = 1f;
            Item.autoReuse = true;
            Item.shootSpeed = 1f;
            Item.shoot = ModContent.ProjectileType<JohnnySwordProjectile>();

            cardThrow = new SoundStyle("TestContent/Assets/Sounds/JCardThrow");
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if(player.altFunctionUse == 2)
            {
                type = ModContent.ProjectileType<JohnnyCard>();
                velocity *= 20f;
                velocity += player.velocity;
                SoundEngine.PlaySound(cardThrow);
            }
        }

        public override bool AltFunctionUse(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<JohnnyCard>()] < 1;
        }
    }
}
