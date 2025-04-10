using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Items.Transmog;
using TestContent.Items.Weapons;

namespace TestContent.Global.NPC
{
    public class NPCModLoot: GlobalNPC
    {
        public override void ModifyNPCLoot(Terraria.NPC npc, NPCLoot npcLoot)
        {
            if(npc.netID == NPCID.Clown)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CautionSign>(), 5));
            }

            if(npc.netID == NPCID.WallofFlesh)
            {
                npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<TransmogItem>()));
            }
        }
    }
}
