using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace TestContent.Players
{
    public class PlayerStarBuff : ModPlayer
    {
        public bool invincible = false;
        public int projectileIndex = 0;

        public override void ResetEffects()
        {
            invincible = false;
        }

        public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
        {
            return !invincible;
        }

        public override bool CanBeHitByProjectile(Projectile proj)
        {
            return !invincible;
        }

        public override bool CanUseItem(Item item)
        {
            return !invincible;
        }
    }
}
