using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Buffs;
using TestContent.Global;

namespace TestContent.Items.Transmog
{
    public class TransmogStar : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.rare = ModContent.GetInstance<TransmogRarity>().Type;
            Item.value = Terraria.Item.buyPrice(silver: 50);
            Item.UseSound = SoundID.Item4;
            Item.consumable = true;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.maxStack = Terraria.Item.CommonMaxStack;
            Item.buffTime = 15 * 60;
            Item.buffType = ModContent.BuffType<StarBuff>();
        }

        public override void OnConsumeItem(Player player)
        {
            if(player.whoAmI == Main.myPlayer)
            {
                player.AddBuff(BuffID.PotionSickness, player.potionDelayTime);
            }
        }

        public override bool CanUseItem(Player player)
        {
            return !player.HasBuff(BuffID.PotionSickness);
        }
    }
}
