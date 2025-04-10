using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using TestContent.Players;
using TestContent.Projectiles.Weapons.Transmog;

namespace TestContent.Buffs
{
    public class StarBuff : ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            var starPlayer = player.GetModPlayer<PlayerStarBuff>();
            starPlayer.invincible = true;
            bool unused = false;
            player.BuffHandle_SpawnPetIfNeeded(ref unused, ModContent.ProjectileType<StarBuffContactDamage>(), buffIndex);
        }
    }
}
