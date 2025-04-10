using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.NPCs.KiryuPNG.Projectiles;

namespace TestContent.NPCs.KiryuPNG.Items
{
    public class BicycleThrowingItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.shootSpeed = 16f;
            Item.UseSound = SoundID.Item1;
            Item.damage = 475;
            Item.shoot = ModContent.ProjectileType<BicycleUser>();
            Item.width = 8;
            Item.height = 28;
            Item.useAnimation = 60;
            Item.useTime = 60;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.value = Terraria.Item.buyPrice(0, 3, 0, 0);
            Item.rare = ItemRarityID.Yellow;
        }
    }
}
