using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using TestContent.Players;
using TestContent.UI;

namespace TestContent.Buffs
{
    public class GiftBuff: ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<SlotMachineSystem>().weighGifts = true;
        }
    }
}
