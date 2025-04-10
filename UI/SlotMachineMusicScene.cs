using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TestContent.UI
{
    internal class SlotMachineMusicScene : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/Gambling");

        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

        public override bool IsSceneEffectActive(Player player)
        {
            var modplayer = player.GetModPlayer<SlotMachineSystem>();
            if(modplayer.system == null)
            {
                return false;
            }
            return modplayer.system.active;
        }
    }
}
