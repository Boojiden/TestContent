using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.NPCs.KiryuPNG.Projectiles;

namespace TestContent.NPCs.KiryuPNG.Items
{
    public class FistItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.DamageType = DamageClass.Melee;
            Item.damage = 53;
            Item.shootSpeed = 24f;
            Item.UseSound = SoundID.Item1;
            Item.shoot = ModContent.ProjectileType<FistUser>();
            Item.width = 8;
            Item.height = 28;
            Item.useAnimation = 12;
            Item.useTime = 12;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.value = Terraria.Item.buyPrice(0, 3, 0, 0);
            Item.rare = ItemRarityID.Yellow;
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            velocity = velocity.RotatedByRandom(MathHelper.Pi / 8);
        }
    }
}
