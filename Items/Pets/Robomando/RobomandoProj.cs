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
using Terraria.GameContent;

namespace TestContent.Items.Pets.Robomando
{
    public class RobomandoProj : ModProjectile
    {
        public bool canJump = false;
        public int internalFrameLoopCount = 0;
        public bool doStatic = false;

        public int jumpTimer = 0;

        public int lastDir;

        public bool flying = false;

        public int baseFrames = 10;
        public int maxFrameSpeedup = 8;

        public Point tilePoint;
        public Tile offendingTile;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 8;
            Main.projPet[Projectile.type] = true;

            // This code is needed to customize the vanity pet display in the player select screen. Quick explanation:
            // * It uses fluent API syntax, just like Recipe
            // * You start with ProjectileID.Sets.SimpleLoop, specifying the start and end frames as well as the speed, and optionally if it should animate from the end after reaching the end, effectively "bouncing"
            // * To stop the animation if the player is not highlighted/is standing, as done by most grounded pets, add a .WhenNotSelected(0, 0) (you can customize it just like SimpleLoop)
            // * To set offset and direction, use .WithOffset(x, y) and .WithSpriteDirection(-1)
            // * To further customize the behavior and animation of the pet (as its AI does not run), you have access to a few vanilla presets in DelegateMethods.CharacterPreview to use via .WithCode(). You can also make your own, showcased in MinionBossPetProjectile
            ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(2, 4, 6)
                .WithOffset(-5, 0f)
                .WithSpriteDirection(1);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if(jumpTimer <= 0)
            {
                canJump = true;
            }
            //Projectile.velocity.Y = 0;
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Turtle); 
            Projectile.aiStyle = -1; 
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
            if (!player.dead && player.HasBuff(ModContent.BuffType<RobomandoBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            if(jumpTimer > 0)
            {
                jumpTimer--;
            }

            Projectile.velocity.Y += 0.2f;
            if(Projectile.velocity.Y > 5f) 
            {
                Projectile.velocity.Y = 5f;
            }
            if(Projectile.velocity.X > 0.3f)
            {
                Projectile.spriteDirection = 1;
            }
            else if (Projectile.velocity.X < -0.3f)
            {
                Projectile.spriteDirection = -1;
            }
            else
            {
                Projectile.spriteDirection = lastDir;
            }
            lastDir = Projectile.spriteDirection;

            tilePoint = new Point((int)(Projectile.Center.X / 16f), (int)(Projectile.Center.Y / 16f));
            tilePoint.Y += 1;
            tilePoint.X += Projectile.spriteDirection;

            offendingTile = Main.tile[tilePoint];

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
                if(xdist > 50f)
                {
                    Projectile.velocity = new Vector2(dir.X * (float)Math.Sqrt(xdist) * 0.5f, Projectile.velocity.Y);
                }
                else
                {
                    Projectile.velocity = new Vector2(0f, Projectile.velocity.Y);
                }
                
            }
            else
            {
                Projectile.rotation = Projectile.velocity.X * 0.05f;
                Projectile.tileCollide = false;
                Projectile.velocity = dir * (float)Math.Sqrt(dist);
            }
            //Main.NewText(Main.tileSolid[offendingTile.TileType].ToString() + "" + Main.tileSolidTop[offendingTile.TileType]);

            if(offendingTile.HasTile && (Main.tileSolid[offendingTile.TileType] || Main.tileSolidTop[offendingTile.TileType]) && canJump)
            {
                Projectile.velocity.Y -= 4f;
                canJump = false;
                jumpTimer = 30;
            }

            //Animation Logic
            if(Math.Abs(Projectile.velocity.X) <= 0.3f)
            {
                Projectile.frame = 0;
            }
            else if(flying)
            {
                Projectile.frame = 1;
            }
            else
            {
                int frames = (int)(Math.Clamp((float)maxFrameSpeedup * (Math.Abs(Projectile.velocity.X) / 8f), 0, (float)maxFrameSpeedup));
                //Main.NewText(frames);
                if (++Projectile.frameCounter > baseFrames - frames)
                {
                    Projectile.frameCounter = 0;
                    int frameOffset = 2;
                    int animLength = 6;
                    Projectile.frame = (internalFrameLoopCount % animLength) + frameOffset;
                    internalFrameLoopCount = (internalFrameLoopCount + 1) % animLength;
                }
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

            //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, Projectile.Center - Main.screenPosition, Color.Red);
            //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, tilePoint.ToVector2() * 16f - Main.screenPosition,Color.White);
            return false;
        }
    }
}
