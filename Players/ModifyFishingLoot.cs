using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TestContent.Items.Gag;

namespace TestContent.Players
{
    public class ModifyFishingLoot : ModPlayer
    {
        public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
        {
            bool surface = Player.InZonePurity();
            bool inWater = !attempt.inLava && !attempt.inHoney;
            if (surface & inWater & Main.rand.Next(0, 21) == 0) 
            {
                itemDrop = ModContent.ItemType<Fish>();
                npcSpawn = -1;

                sonar.Text = "FISH";
                sonar.Color = Color.DodgerBlue;
                sonar.Velocity = Vector2.Zero;
                sonar.DurationInFrames = 300;
            }

            return;
        }
    }
}
