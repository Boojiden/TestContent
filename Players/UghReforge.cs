using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace TestContent.Players
{
    public class UghReforge : GlobalItem
    {
        public static HashSet<int> badPrefixes = new HashSet<int>();

        public static bool playSound = true;

        public static SoundStyle ugh;

        public override void Load()
        {
            ugh = new SoundStyle("TestContent/Assets/Sounds/uuuh")
            {
                Volume = 0.8f,
                MaxInstances = 1
            };
            int[] preFixes = [8, 10, 11, 13, 22, 23, 24, 29, 30, 31, 39, 40, 41, 47, 48, 49, 50, 56];
            for( int i = 0; i < preFixes.Length; i++)
            {
                badPrefixes.Add(preFixes[i]);
            }
        }
        public override void PostReforge(Item item)
        {
            //Main.NewText(item.prefix);
            if (badPrefixes.Contains(item.prefix) && playSound)
            {
                SoundEngine.PlaySound(ugh, item.position);
            }
        }
    }
}
