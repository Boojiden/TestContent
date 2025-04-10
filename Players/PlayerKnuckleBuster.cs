using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TestContent.NPCs.Minos.Items;
using TestContent.NPCs.Minos.Projectiles.Friendly;

namespace TestContent.Players
{
    public class PlayerKnuckleBuster : ModPlayer
    {
        public bool hideArm = false;
        public override void PostUpdate()
        {
            if(Player.HeldItem.type == ModContent.ItemType<KnuckleBlaster>())
            {
                hideArm = true;
            }
            else
            {
                if (Player.ownedProjectileCounts[ModContent.ProjectileType<KnuckleBlasterHandProjectile>()] > 0)
                {
                    var proj = Main.projectile.FirstOrDefault(x => x.type == ModContent.ProjectileType<KnuckleBlasterHandProjectile>() && x.owner == Player.whoAmI);
                    proj.Kill();
                }
                hideArm = false;
            }
        }
        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            if(hideArm)
            { 
                foreach(var layer in PlayerDrawLayerLoader.DrawOrder)
                {
                    if(layer.Name == "Skin")
                    {
                        layer.Hide();
                    }
                }
            }
        }
    }
}
