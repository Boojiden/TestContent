using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Mounts;

namespace TestContent.Items.Gag
{
    public class PingasNoiseMaker : ModItem
    {
        public static SoundStyle pingas;
        public override void SetDefaults()
        {
            pingas = new SoundStyle("TestContent/Assets/Sounds/Pingas")
            {
                MaxInstances = 0
            };
            Item.width = 16;
            Item.height = 16;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp; // how the player's arm moves when using the item
            Item.value = Item.sellPrice(gold: 7);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = pingas;// What sound should play when using the item
            Item.noMelee = true; // this item doesn't do any melee damage
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            var pos = player.position;
            if (player.direction < 0)
            {
                pos.X += Item.width;
            }
            else
            {
                pos.X -= Item.width;
            }
            pos.Y += Item.height * 2;
            player.itemLocation = pos;
        }
    }
}
