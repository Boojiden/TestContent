using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Projectiles;
using TestContent.Utility;
using TextContent.Effects.Graphics.Primitives;

namespace TestContent.NPCs.Minos.Projectiles
{
    public class Viper : BasicTrailProjectile
    {
        protected new int trailLength = 25;
        //protected new string trailTextureName = $"NPCs/Minos/Extras/ViperTrail";

        private int targetIndex = -1;

        private float detectionRange = 200f;

        private float turningRate = 3f;

        protected override string trailTextureName => $"TestContent/NPCs/Minos/Extras/ViperTrail";

        public static SoundStyle snakeShatter;

        public override PrimitiveSettings SetPrimitiveSettings()
        {
            return new PrimitiveSettings(PrimitiveWidthFunction, PrimitiveColorFunction, PrimitiveOffsetFunction, pixelate: true, shader: GameShaders.Misc["TestContent:TrailDirect"]);
        }

        public override void Load()
        {
            snakeShatter = new SoundStyle(ModUtils.GetSoundFileLocation("Minos/SnakeShatter"))
            {
                PlayOnlyIfFocused = true,
                Volume = 0.4f,
                MaxInstances = 3,
            };
        }
        public override void SetDefaults()
        {
            base.SetDefaults();//Primitive Settings are set here
            Projectile.width = 50;
            Projectile.height = 40;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.timeLeft = 60 * 10;
            Projectile.tileCollide = false;
            
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            Projectile.Kill();
            if(Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.identity);
            }
        }

        public override void AI()
        {
            Vector2 ownerPos = Vector2.Zero;
            if (targetIndex < 0 || targetIndex == 255 || Main.player[targetIndex].dead || !Main.player[targetIndex].active)
            {
                int playerIndex = 0;
                float minDistance = detectionRange;
                foreach(Player p in Main.ActivePlayers)
                {
                    float playerDist = (p.position - Projectile.position).Length();
                    if (playerDist < minDistance)
                    {
                        minDistance = playerDist;
                        playerIndex = p.whoAmI;
                    }
                }
                targetIndex = playerIndex;
            }

            Player target = Main.player[targetIndex];

            var dir = target.position - Projectile.position;

            var targetRot = dir.ToRotation();

            var velRot = Projectile.velocity.ToRotation();

            float maxRot = MathHelper.ToRadians(turningRate);

            float angleChange = targetRot - velRot;

            if(Math.Abs(angleChange) > MathHelper.ToRadians(180f))
            {
                angleChange += angleChange < 0 ? MathHelper.ToRadians(360f) : -MathHelper.ToRadians(360f);
            }

            float aRot = Math.Clamp(angleChange, -maxRot, maxRot);

            Vector2 travelDir = Projectile.velocity.RotatedBy(aRot);
            travelDir.Normalize();

            var dist = dir.Length();
            dir.Normalize();

            var speed = 20f;
            var cutoff = 175f;
            if (Main.expertMode)
            {
                speed = 22f;
                cutoff = 150f;
            }
            

            var maxVelocity = travelDir * speed;
            if (dist > 175f)
            {
                //Projectile.velocity = (Projectile.velocity * (inertia - 1) + maxVelocity) / inertia;
                Projectile.velocity = maxVelocity;
            }

            //Main.NewText(Projectile.numHits);
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.spriteDirection = 1;
            Lighting.AddLight(Projectile.position, Color.Yellow.ToVector3());
        }

        public override float PrimitiveWidthFunction(float completionRatio)
        {
            float splitPoint = 0.55f;
            float inverse = 1f - splitPoint;
            if(completionRatio < splitPoint)
            {
                return 1f * Projectile.scale * 12;
            }
            return ((inverse - (completionRatio - splitPoint)) / inverse) * Projectile.scale * 12;
        }

        public override Vector2 PrimitiveOffsetFunction(float completionRatio)
        {
            Vector2 orig = base.PrimitiveOffsetFunction(completionRatio);
            var forward = Projectile.velocity;
            forward.Normalize();
            orig += forward * -7;
            return orig;
        }

        public override Color PrimitiveColorFunction(float completionRatio)
        {
            var col1 = Color.White;
            var col2 = Color.Transparent;
            return Color.Lerp(col1, col2, completionRatio);
        }

        public override void OnKill(int timeLeft)
        {
            if (!Main.dedServ)
            {
                DustUtils.CreateDustBurstCircle(DustID.YellowStarDust, Projectile.Center, 1f, 5f, 10);
            }
            SoundEngine.PlaySound(snakeShatter, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var effects = Projectile.direction == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Main.EntitySpriteDraw(Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, Projectile.Hitbox.Size() / 2, Projectile.scale, effects);
            return false;
        }
    }
}
