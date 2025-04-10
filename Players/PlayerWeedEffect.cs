using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using TestContent.Buffs;

namespace TestContent.Players
{
    public class PlayerWeedEffect : ModPlayer
    {
        public bool buzzed = false;
        public bool zonked = false;


        public readonly int BUZZED_DAMAGE_CAP = 25;
        public readonly int ZONKED_DAMAGE_CAP = 25;

        public readonly int BUZZED_DAMAGE_REDUCTION = 10;
        public readonly int ZONKED_DAMAGE_REDUCTION = 25;

        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            DecreaseDamage(npc.damage, ref modifiers);
        }
        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            DecreaseDamage(proj.damage, ref modifiers);
        }
        
        public void DecreaseDamage(int damage, ref Player.HurtModifiers mod)
        {
            if(buzzed)
            {
                if(damage < BUZZED_DAMAGE_CAP)
                {
                    return;
                }
                int reduction = BUZZED_DAMAGE_REDUCTION;
                if(damage - reduction < BUZZED_DAMAGE_CAP)
                {
                    reduction = damage - BUZZED_DAMAGE_CAP;
                }
                mod.FinalDamage.Base -= reduction;
                
            }
            else if (zonked)
            {
                if (damage < ZONKED_DAMAGE_CAP)
                {
                    return;
                }
                int reduction = ZONKED_DAMAGE_REDUCTION;
                if (damage - reduction < ZONKED_DAMAGE_CAP)
                {
                    reduction = damage - ZONKED_DAMAGE_CAP;
                }
                mod.FinalDamage.Base -= reduction;
            }
        }

        public override void PostUpdateRunSpeeds()
        {
            if (Player.accRunSpeed >= 6f && zonked)
            {
                Player.accRunSpeed = Player.accRunSpeed - (Player.accRunSpeed * 0.1f);
            }
        }

        public override void ResetEffects()
        {
            buzzed = false;
            zonked = false;
        }
    }
}
