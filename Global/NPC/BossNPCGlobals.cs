using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace TestContent.Global.NPC
{
    public class BossNPCGlobals : GlobalNPC
    {
        public static int minos = -1;
        public static int minosorb = -1;
        public static DateTime? minosOffIntro;

        public override void AI(Terraria.NPC npc)
        {
            if(minos != -1)
            {
                if(minosOffIntro == null)
                {
                    minosOffIntro = DateTime.Now + new TimeSpan(0, 0, 38);
                }
            }
            else
            {
                minosOffIntro = null;
            }
            //string debugText = minosOffIntro == null ? "null" : minosOffIntro.ToString();
            //Main.NewText(debugText);
        }
    }
}
