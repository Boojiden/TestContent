using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;
using TestContent.Players;
using TestContent.UI;

namespace TestContent
{
    public class DiseaseClientConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("Audio/Visuals")]
        [DefaultValue(true)]
        public bool PlayBadReforgeNoise;

        [DefaultValue(true)]
        public bool PlayLuigiJumpscare;

        public override void OnChanged()
        {
            UghReforge.playSound = PlayBadReforgeNoise;
            LuigiJumpscareUISystem.doJumpscare = PlayLuigiJumpscare;
        }
    }
}
