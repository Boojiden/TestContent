using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.Audio;
using TestContent.NPCs.Minos.Projectiles.Friendly;
using System.Security.Cryptography.Xml;
using TestContent.Utility;

namespace TestContent.NPCs.Minos.Items
{
    public class WhiplashItem : ModItem
    {
        public override void SetDefaults()
        {
            // Copy values from the Amethyst Hook
            Item.CloneDefaults(ItemID.AmethystHook);
            Item.shootSpeed = 26f; // This defines how quickly the hook is shot.
            Item.damage = 100;
            Item.UseSound = new SoundStyle(ModUtils.GetSoundFileLocation("hookThrowStart"))
            {
                Volume = 0.4f,
                MaxInstances = 3,
                PlayOnlyIfFocused = true,
            };
            Item.DamageType = DamageClass.Melee;
            Item.shoot = ModContent.ProjectileType<WhiplashProjectile>(); // Makes the item shoot the hook's projectile when used.
            Item.rare = ItemRarityID.Red;
            Item.value = Item.buyPrice(gold: 15);
            // If you do not use Item.CloneDefaults(), you must set the following values for the hook to work properly:
            // Item.useStyle = ItemUseStyleID.None;
            // Item.useTime = 0;
            // Item.useAnimation = 0;
        }
    }
}
