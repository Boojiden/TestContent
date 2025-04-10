using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Utility;

namespace TestContent.Projectiles.Weapons.Transmog
{
    public class BlueShell : ModProjectile
    {
        private enum ShellState
        {
            Traveling,
            LockedOn,
            Falling
        }

        public CubicBezierCurveLerp fallLerp = new CubicBezierCurveLerp(0.947f, 0.348f, -0.08f, -0.472f);

        public int Timer
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public int fallingTime = 30;
        private ShellState State
        {
            get => (ShellState)Projectile.ai[1];
            set => Projectile.ai[1] = (float)value;
        }

        private int npcIndex = 0;

        private float npcLocationOffset = 150f;

        public Asset<Texture2D> wingAsset;
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.tileCollide = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60 * 10;

            wingAsset = ModContent.Request<Texture2D>("TestContent/Projectiles/Weapons/Transmog/BlueShell_Wing");
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BlueShellExplosion>(), Projectile.damage, Projectile.knockBack);
            }
        }

        public int LookForNPC()
        {
            Vector2 targetCenter = new Vector2(0f, 0f);
            float maxDistance = 500f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy())
                {
                    float between = Vector2.Distance(npc.Center, Projectile.Center);
                    bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
                    bool inRange = between < maxDistance;
                    bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
                    // Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
                    // The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
                    bool closeThroughWall = between < 100f;

                    if (closest && inRange && (lineOfSight || closeThroughWall))
                    {
                        targetCenter = npc.Center;
                        return i;
                    }
                }
            }
            return -1;
        }

        public bool CheckNPC()
        {
            try
            {
                NPC target = Main.npc[npcIndex];
                if (target == null)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public override void AI()
        {
            switch (State)
            {
                case ShellState.Traveling:
                    int index = LookForNPC();
                    if (index != -1)
                    {
                        npcIndex = index;
                        State = ShellState.LockedOn;
                    }
                    break;
                case ShellState.LockedOn:
                    if (!CheckNPC())
                    {
                        npcIndex = -1;
                        State = ShellState.Traveling;
                        return;
                    }
                    NPC target = Main.npc[npcIndex];
                    Vector2 goal = target.Center;
                    goal.Y -= npcLocationOffset;

                    //Dust.QuickDust(goal , Color.Red);

                    Vector2 to = goal - Projectile.Center;

                    if (to.Length() < 40f)
                    {
                        State = ShellState.Falling;
                        return;
                    }
                    var dir = to.SafeNormalize(Vector2.UnitY);
                    var velDir = Projectile.velocity.SafeNormalize(Vector2.UnitY);
                    float angle = dir.ToRotation() - velDir.ToRotation();

                    angle = (float)Math.Clamp(angle, -Math.PI / 6, Math.PI / 6);
                    //Main.NewText(angle + " " + to.Length());
                    Projectile.velocity = Projectile.velocity.RotatedBy(angle);
                    break;
                case ShellState.Falling:
                    if (!CheckNPC())
                    {
                        npcIndex = -1;
                        State = ShellState.Traveling;
                        return;
                    }
                    NPC target2 = Main.npc[npcIndex];
                    float time = Timer / (float)fallingTime;
                    Vector2 goal2 = target2.Center;
                    goal2.Y -= npcLocationOffset;
                    Projectile.Center = Vector2.Lerp(goal2, target2.Center, fallLerp.GetLerp(time));
                    if (time >= 1f)
                    {
                        Projectile.Kill();
                    }
                    Timer++;
                    break;
            }
            Projectile.spriteDirection = Projectile.direction;
            if (State == ShellState.Falling)
            {
                float goalRot = 0f;
                if (Projectile.spriteDirection == -1)
                {
                    goalRot = (float)Math.PI;
                }
                goalRot += (float)Math.PI * 2;
                Projectile.rotation = MathHelper.Lerp(goalRot, Projectile.rotation, 0.9f);
            }
            else
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
            }
        }

        public void DrawWing(Color lightColor, Vector2 hinge)
        {
            var tex = wingAsset.Value;
            var rect = new Rectangle(0, 0, tex.Width, tex.Height);
            var origin = new Vector2(tex.Width, tex.Height);

            var rot = Projectile.rotation;
            rot += (float)Math.Sin(Main.GlobalTimeWrappedHourly * 12) * (float)(Math.PI / 4);

            var effects = SpriteEffects.FlipHorizontally;
            if (Projectile.spriteDirection == -1)
            {
                effects = SpriteEffects.None;
                origin = new Vector2(0f, tex.Height);
                rot -= (float)Math.PI;
            }

            Main.spriteBatch.Draw(tex, hinge - Main.screenPosition, rect, lightColor, rot, origin, 1f, effects, 0f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 wingpos = Projectile.Center;
            Vector2 offset = new Vector2(-5f, -(Projectile.height / 2 - 15f));
            if (Projectile.spriteDirection == 1)
            {
                offset.X *= -1;
            }
            wingpos += offset;
            DrawWing(lightColor, wingpos);

            var mainTex = TextureAssets.Projectile[Type].Value;
            var origin = new Vector2(mainTex.Width / 2, mainTex.Height / 2);
            var effects = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
            {
                effects = SpriteEffects.FlipVertically;
            }
            Main.EntitySpriteDraw(mainTex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects);
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            Vector2 wingpos = Projectile.Center;
            Vector2 offset = new Vector2(5f, -(Projectile.height / 2 - 15f));
            if (Projectile.spriteDirection == 1)
            {
                offset.X *= -1;
            }
            wingpos += offset;
            DrawWing(lightColor, wingpos);
            base.PostDraw(lightColor);
        }
    }
}
