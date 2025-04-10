using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using TestContent.Items.Ammo;
using TestContent.Projectiles.Weapons.Gambling.Hardmode;
using Microsoft.Xna.Framework;

namespace TestContent.Items
{
    public abstract class BasicHeldGunItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.channel = true;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.Rapier;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
        }
    }
}
