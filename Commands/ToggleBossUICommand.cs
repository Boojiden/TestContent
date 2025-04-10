using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using TestContent.UI;

namespace TestContent.Commands
{
    public class ToggleBossUICommand : ModCommand
    {
        public override CommandType Type
           => CommandType.Chat;

        // The desired text to trigger this command
        public override string Command
            => "ToggleBossUI";

        // A short description of this command
        public override string Description
            => "Toggle the Kiryu Boss intro for debugging purposes";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var system = ModContent.GetInstance<KiryuBossIntroUISystem>();
            //system.BossUI.OnInitialize();
            system.ToggleUI();
        }
        // /ToggleBossUI
    }
}
