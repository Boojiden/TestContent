using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using TestContent.NPCs.Minos;

namespace TestContent.Commands
{
    public class SetMinosDebug : ModCommand
    {
        public override string Command => "SetMinosDebug";

        public override CommandType Type => CommandType.Console;

        public override string Description => "Debug command for MP Minos Prime";

        public static HashSet<String> ValidArgs = new HashSet<String> {"timer", "attack", "collision"};

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            bool set = false;
            bool flag1 = args.Length == 2;
            bool flag2 = flag1 && ValidArgs.Contains(args[0]);
            bool flag3 = flag1 && bool.TryParse(args[1], out set);

            if(flag1 && flag2 && flag3)
            {
                switch(args[0])
                {
                    case "timer":
                        MinosDebug.PrintTimer = set;
                        break;
                    case "attack":
                        MinosDebug.PrintAttack = set;
                        break;
                    case "collision":
                        MinosDebug.PrintGroundCollision = set;
                        break;
                }
            }
            else
            {
                foreach (string arg in args)
                {
                    Console.Write(arg + " ");
                }
                Console.WriteLine($"{flag1}, {flag2}, {flag3}");
                Console.WriteLine();
                Console.WriteLine("Usage: \n"+
                    "SetMinosDebug {\'timer\' or \'attack\' or \'collision\'} {Boolean} \n+"+
                    "Sets the appropraite flag for debugging");
            }
        }
    }
}
