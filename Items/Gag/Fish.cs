using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace TestContent.Items.Gag
{
    public class Fish : ModItem
    {
        public static SoundStyle fish;
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Bass);
            fish = new SoundStyle("TestContent/Assets/Sounds/fish")
            {
                MaxInstances = 0
            };
            Item.width = 16;
            Item.height = 16;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.maxStack = Item.CommonMaxStack;
            Item.useStyle = ItemUseStyleID.HoldUp; // how the player's arm moves when using the item
            Item.value = Terraria.Item.sellPrice(silver: 50);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = fish;// What sound should play when using the item
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
