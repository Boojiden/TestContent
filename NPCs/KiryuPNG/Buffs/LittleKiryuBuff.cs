using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using TestContent.Mounts;
using TestContent.NPCs.KiryuPNG.Mounts;
using TestContent.Players;

namespace TestContent.NPCs.KiryuPNG.Buffs
{
    public class LittleKiryuBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true; // The time remaining won't display on this buff
            Main.buffNoSave[Type] = true; // This buff won't save when you exit the world
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.mount.SetMount(ModContent.MountType<LittleKiryu>(), player);
            player.buffTime[buffIndex] = 10; // reset buff time

            var h = player.GetModPlayer<HorseHideAll>();
            h.horse = true;
        }
    }
}
