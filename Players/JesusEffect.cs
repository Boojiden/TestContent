using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ModLoader;
using TestContent.Buffs;
using TestContent.UI;

namespace TestContent.Players
{
    public class JesusEffect : ModPlayer
    {
        public bool seeingGod = false;
        public override void ResetEffects()
        {
            seeingGod = false;
        }

        public override void PostUpdate()
        {
            //Main.NewText($"Post Check: {seeingGod}, {sys.active}");
        }
    }

    public class ReviveEffect : ModPlayer
    {
        public bool canRevive = false;
        public bool hasReviveAccessory = false;

        public int reviveCooldown = 0;
        public int reviveMaxCooldown = 1800;

        public int restoredHealth = 50;

        public override void ResetEffects()
        {
            hasReviveAccessory = false;
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
        {

            var sys = ModContent.GetInstance<ChristUISystem>();
            if (hasReviveAccessory && canRevive && Main.LocalPlayer == Player)
            {
                canRevive = false;
                Player.Heal(restoredHealth);
                reviveCooldown = reviveMaxCooldown;
                Player.AddBuff(ModContent.BuffType<Touched>(), reviveMaxCooldown);
                sys.ShowMyUI();
                //Main.NewText("Dies (Revives)");
                return false;
            }
            else
            {
                canRevive = true;
                reviveCooldown = 0;
                if(Player.HasBuff<Touched>())
                {
                    Player.ClearBuff(ModContent.BuffType<Touched>());
                }
                if(Main.LocalPlayer == Player)
                {
                    sys.HideMyUI();
                }
            }

            //Main.NewText("Dies (Real)");
            return true;
        }

        public override void PostUpdate()
        {
            if(reviveCooldown <= 0)
            {
                canRevive = true;
            }
            else
            {
                reviveCooldown--;
            }
        }
    }
}
