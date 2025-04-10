using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TestContent.Buffs;
using TestContent.Dusts;
using TestContent.Items;
using TestContent.Items.Pets;

namespace TestContent.Mounts
{
    public class HorseMount: ModMount
    {
        public override void SetStaticDefaults()
        {
            MountData.jumpHeight = 45; // How high the mount can jump.
            MountData.acceleration = 0.19f; // The rate at which the mount speeds up.
            MountData.jumpSpeed = 12f; // The rate at which the player and mount ascend towards (negative y velocity) the jump height when the jump button is pressed.
            MountData.blockExtraJumps = false; // Determines whether or not you can use a double jump (like cloud in a bottle) while in the mount.
            MountData.constantJump = true; // Allows you to hold the jump button down.
            MountData.heightBoost = 50; // Height between the mount and the ground
            MountData.fallDamage = 1f; // Fall damage multiplier.
            MountData.runSpeed = 11f; // The speed of the mount
            MountData.dashSpeed = 8f; // The speed the mount moves when in the state of dashing.
            MountData.flightTimeMax = 0; // The amount of time in frames a mount can be in the state of flying.

            MountData.fatigueMax = 0;
            MountData.buff = ModContent.BuffType<HorseMountBuff>();
            MountData.spawnDust = ModContent.DustType<Explosion>();

            MountData.totalFrames = 2;
            MountData.playerYOffsets = Enumerable.Repeat(50, MountData.totalFrames).ToArray();
            MountData.playerXOffset = 0;
            MountData.playerHeadOffset = 64;

            MountData.standingFrameCount = 1;
            MountData.standingFrameDelay = 12;
            MountData.standingFrameStart = 0;
            // Running
            MountData.runningFrameCount = 1;
            MountData.runningFrameDelay = 12;
            MountData.runningFrameStart = 1;
            // Flying
            MountData.flyingFrameCount = 0;
            MountData.flyingFrameDelay = 0;
            MountData.flyingFrameStart = 0;
            // In-air
            MountData.inAirFrameCount = 1;
            MountData.inAirFrameDelay = 12;
            MountData.inAirFrameStart = 1;
            // Idle
            MountData.idleFrameCount = 1;
            MountData.idleFrameDelay = 12;
            MountData.idleFrameStart = 0;
            // Swim
            MountData.swimFrameCount = MountData.inAirFrameCount;
            MountData.swimFrameDelay = MountData.inAirFrameDelay;
            MountData.swimFrameStart = MountData.inAirFrameStart;

            if (!Main.dedServ)
            {
                MountData.textureWidth = MountData.backTexture.Width();
                MountData.textureHeight = MountData.backTexture.Height();
            }
        }
        public override void SetMount(Player player, ref bool skipDust)
        {
            if (!Main.dedServ)
            {
                Vector2 pos = player.position;
                pos.X -= MountData.textureWidth / 2;
                pos.Y -= (MountData.textureHeight / 2) - 24;
                //Dust.NewDustPerfect(pos, MountData.spawnDust);
                //SoundEngine.PlaySound(CarRare.Kaboom, player.position);
                skipDust = true;
            }

            if(player.whoAmI != Main.myPlayer)
            {
                player.position.Y += 64;
            }
            
        }

        public override void Dismount(Player player, ref bool skipDust)
        {
            if(!Main.dedServ)
            {
                skipDust = true;
            }

            if (player.whoAmI != Main.myPlayer)
            {
                player.position.Y -= 64;
            }
        }
        public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow)
        {
            playerDrawData.Clear();
            //drawPosition.X -= MountData.textureWidth / 2;
            var pos = drawPosition;
            //pos.X -= MountData.textureWidth / 2;
            pos.Y -= 8;
            if(drawPlayer.direction < 0)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            var data = new DrawData(MountData.frontTexture.Value, pos, frame, drawColor, 0, drawOrigin, 1f, spriteEffects);
            data.shader = drawPlayer.miscDyes[3].dye;
            playerDrawData.Add(data);
            return true;
        }
    }
}
