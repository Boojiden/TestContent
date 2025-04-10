using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using TestContent.Players;

namespace TestContent.Buffs
{
    public class Buzzed : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            PlayerWeedEffect w = player.GetModPlayer<PlayerWeedEffect>();
            w.buzzed = true;
        }
    }
}
