using Humanizer;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Items;
using TestContent.NPCs.Minos.Projectiles.Friendly;
using TestContent.Players;

namespace TestContent.NPCs.Minos.Items
{
    public class FreezeFrame : BasicHeldGunItem
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.width = 100;
            Item.height = 34;
            Item.rare = ItemRarityID.Red;
            Item.value = Terraria.Item.buyPrice(gold: 20);
            Item.damage = 155;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.knockBack = 1f;
            Item.shootSpeed = 0f;
            Item.shoot = ModContent.ProjectileType<FreezeFrameHeldProjectile>();
            Item.useAmmo = AmmoID.Rocket;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            foreach(TooltipLine line in tooltips)
            {
                if (line.Name.Equals("Tooltip2"))
                {
                    line.Text = line.Text.FormatWith(PlayerRideables.MountTrigger.GetAssignedKeys()[0]);
                }
                else if (line.Name.Equals("Tooltip3"))
                {
                    line.Text = line.Text.FormatWith(TestContent.GetControlString("Left"), TestContent.GetControlString("Right"));
                }
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if(player.altFunctionUse == 2)
            {
                player.GetModPlayer<PlayerWeapons>().ToggleRockets();
                return false;
            }
            return true;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            return player.itemAnimation != 28;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
    }
}
