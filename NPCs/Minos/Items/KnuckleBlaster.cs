using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.NPCs.Minos.Projectiles.Friendly;

namespace TestContent.NPCs.Minos.Items
{
    public class KnuckleBlaster : ModItem
    {

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 48;
            Item.value = Terraria.Item.buyPrice(gold: 15);
            Item.rare = ItemRarityID.Red;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.damage = 619;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.None;
            Item.knockBack = 3f;
            Item.autoReuse = true;
        }

        public override void HoldItem(Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<KnuckleBlasterHandProjectile>()] < 1)
            {
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<KnuckleBlasterHandProjectile>(), (int)player.GetTotalDamage(this.Item.DamageType).ApplyTo(Item.damage), Item.knockBack);
            }
        }
    }
}
