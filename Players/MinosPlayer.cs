using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using TestContent.NPCs.Minos;

namespace TestContent.Players
{
    public class MinosPlayer : ModPlayer
    {
        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if(npc.netID == ModContent.NPCType<MinosPrime>())
            {
                //Minos gives no knockback, even without cobalt shield. Knockback is handled by the boss itself
                modifiers.Knockback *= 0;
            }
        }
    }
}
