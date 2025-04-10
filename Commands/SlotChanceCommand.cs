using Microsoft.Xna.Framework;
using rail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using TestContent.UI;
using static TestContent.UI.SlotMachineSystem;

namespace TestContent.Commands
{
    public class SlotChanceCommand : ModCommand
    {
        // CommandType.Chat means that command can be used in Chat in SP and MP
        public override CommandType Type
            => CommandType.Chat;

        // The desired text to trigger this command
        public override string Command
            => "SimulateSlots";

        // A short description of this command
        public override string Description
            => "Simulates the chances of the slot machine with ";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            Player player = caller.Player;

            bool malformed = false;

            if(args.Length != 2 )
            {
                malformed = true;
            }
            bool usePlayerStats = false;
            if (!malformed)
            {
                switch (args[0])
                {
                    case "default":
                        usePlayerStats = false;
                        break;
                    case "currentStats":
                        usePlayerStats = true;
                        break;
                    default:
                        malformed = true;
                        break;
                } 
            }

            int rolls = 0;
            if (!malformed && int.TryParse(args[1], out int numOfRolls))
            {
                rolls = numOfRolls;
            }
            else
            {
                malformed = true;
            }

            if (malformed)
            {
                Main.NewText("Usage: \n" +
                    "   /SimulateSlots {\"default\" or \"currentStats\"} {int numOfRolls}\n" +
                    "default: Simulate slots with no player modifiers\n" +
                    "currentStats: Simulate slots with current player modifiers\n" +
                    "numOfRolls: How many rolls to simulate", color: Color.LightPink);
                return;
            }

            int[] map = [0,0,0,0,0,0,0,0,0];

            bool[] bools = [false, false, false];
            if(usePlayerStats)
            {
                var slot = player.GetModPlayer<SlotMachineSystem>();
                bools[0] = slot.betterChances;
                bools[1] = slot.doubleOrNothing;
                bools[2] = slot.weighGifts;
            }
            for(int i = 0; i < rolls; i++)
            {
                RollResult result = SimulateRoll(bools[0], bools[1], bools[2]);
                map[(int)result] = map[(int)result] + 1;
            }
            Main.NewText("Results: ", color: Color.Green);
            var printOut = "";
            for (int i = 0; i < 9; i++)
            {
                float percent = ((float)map[i]/(float)rolls * 100f) ;
                var res = (RollResult)i;
                printOut += res.ToString() + ": %" + percent.ToString("0.00")+ " \n";
            }
            Main.NewText(printOut, color: Color.YellowGreen);
            // /SimulateSlots default 100
            // /SimulateSlots currentStats 10000
        }
    }
}
