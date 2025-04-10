using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TestContent.Items.Pets.YesMan
{
    public class YesManProj : ModProjectile
    {
        public bool canJump = false;
        public int internalFrameLoopCount = 0;
        public bool doStatic = false;

        public bool flying = false;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 10;
            Main.projPet[Projectile.type] = true;

            // This code is needed to customize the vanity pet display in the player select screen. Quick explanation:
            // * It uses fluent API syntax, just like Recipe
            // * You start with ProjectileID.Sets.SimpleLoop, specifying the start and end frames as well as the speed, and optionally if it should animate from the end after reaching the end, effectively "bouncing"
            // * To stop the animation if the player is not highlighted/is standing, as done by most grounded pets, add a .WhenNotSelected(0, 0) (you can customize it just like SimpleLoop)
            // * To set offset and direction, use .WithOffset(x, y) and .WithSpriteDirection(-1)
            // * To further customize the behavior and animation of the pet (as its AI does not run), you have access to a few vanilla presets in DelegateMethods.CharacterPreview to use via .WithCode(). You can also make your own, showcased in MinionBossPetProjectile
            ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(0, 4, 6)
                .WithOffset(-10, 0f)
                .WithSpriteDirection(1);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            canJump = true;
            Projectile.velocity.Y = 0;
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Turtle); // Copy the stats of the Zephyr Fish
            Projectile.aiStyle = -1; // Mimic as the Zephyr Fish during AI.
            Projectile.width = 50;
            Projectile.height = 45;
        }

        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];
            //player.QuickSpawnItem
            player.zephyrfish = false; // Relic from AIType

            return true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // Keep the projectile from disappearing as long as the player isn't dead and has the pet buff.
            if (!player.dead && player.HasBuff(ModContent.BuffType<YesManBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            Projectile.velocity.Y += 0.2f;
            if(Projectile.velocity.Y > 5f) 
            {
                Projectile.velocity.Y = 5f;
            }
            if(Projectile.velocity.X > 0)
            {
                Projectile.spriteDirection = 1;
            }
            else
            {
                Projectile.spriteDirection = -1;
            }

            Vector2 target = player.Center + (-player.direction * new Vector2(30f, 0));
            target.Y -= 50f;
            Vector2 dir = (target - Projectile.Center).SafeNormalize(Vector2.UnitX);
            float dist = (target - Projectile.Center).Length();
            float xdist = Math.Abs(target.X - Projectile.Center.X);
            float ydist = Math.Abs(target.Y - Projectile.Center.Y);
            int state = 0;
            if(dist > 1000f)
            {
                Projectile.Center = target;
                return;
            }
            if (dist > 500f && !flying)
            {
                flying = true;
            }
            else if (dist < 50f && flying)
            {
                flying = false;
            }

            if (flying)
            {
                state = 1;
            }

            if(state == 0)
            {
                Projectile.rotation = 0;
                Projectile.tileCollide = true;
                Projectile.velocity = new Vector2(dir.X * (float)Math.Sqrt(xdist) * 0.5f, Projectile.velocity.Y);
            }
            else
            {
                Projectile.rotation = Projectile.velocity.X * 0.05f;
                Projectile.tileCollide = false;
                Projectile.velocity = dir * (float)Math.Sqrt(dist);
            }


            if(++Projectile.frameCounter > 10)
            {
                Projectile.frameCounter = 0;
                int maxFrames = 4;
                if (doStatic && state == 0)
                {
                    maxFrames += 2;
                }
                if(state == 0)
                {
                    Projectile.frame = (internalFrameLoopCount + 1) % maxFrames;
                    if(Main.rand.Next(0, 11) == 0)
                    {
                        doStatic = true;
                    }
                    if(Projectile.frame == 5)
                    {
                        doStatic = false;
                    }
                }
                else
                {
                    Projectile.frame = ((internalFrameLoopCount + 1) % maxFrames) + 6;
                }
                internalFrameLoopCount = (internalFrameLoopCount + 1) % maxFrames;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects effects = SpriteEffects.None;
            if(Projectile.spriteDirection == -1)
            {
                effects = SpriteEffects.FlipHorizontally;
            }
            TestContent.AnimatedProjectileDraw(Projectile, lightColor, effects: effects);
            return false;
        }
    }
}
