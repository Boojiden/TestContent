using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Global;
using TestContent.NPCs.KiryuPNG.Projectiles;
using TestContent.Projectiles.Weapons.Transmog;

namespace TestContent.Items.Transmog
{
    public class TransmogShell : ModItem
    {
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.DamageType = DamageClass.Magic;
            Item.shootSpeed = 16f;
            Item.UseSound = SoundID.Item1;
            Item.damage = 43;
            Item.shoot = ModContent.ProjectileType<BlueShell>();
            Item.mana = 15;
            Item.width = 50;
            Item.height = 58;
            Item.useAnimation = 45;
            Item.useTime = 45;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.value = Terraria.Item.buyPrice(0, 3, 0, 0);
            Item.rare = ModContent.GetInstance<TransmogRarity>().Type;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            float dec = (float)damage;
            float result = MathHelper.Lerp(dec, dec * 2, 1f - ((float)player.statLife / (float)player.statLifeMax2));
            damage = (int)result;
        }
    }
}
