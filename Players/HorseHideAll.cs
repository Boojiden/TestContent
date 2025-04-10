using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace TestContent.Players
{
    public class HorseHideAll: ModPlayer
    {
        public bool horse = false;

        public override void ResetEffects()
        {
            horse = false;
        }

        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            if(horse) {
                PlayerDrawLayers.ArmOverItem.Hide();
                PlayerDrawLayers.BackAcc.Hide();
                PlayerDrawLayers.BalloonAcc.Hide();
                PlayerDrawLayers.FaceAcc.Hide();
                PlayerDrawLayers.FrontAccBack.Hide();
                PlayerDrawLayers.FrontAccFront.Hide();
                PlayerDrawLayers.HandOnAcc.Hide();
                PlayerDrawLayers.NeckAcc.Hide();
                PlayerDrawLayers.OffhandAcc.Hide();
                PlayerDrawLayers.WaistAcc.Hide();
                PlayerDrawLayers.Shield.Hide();
            }
        }
    }
}
