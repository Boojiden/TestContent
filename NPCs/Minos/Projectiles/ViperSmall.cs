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
    public class ViperSmall : BasicTrailProjectile
    {
        protected new int trailLength = 15;
        //protected new string trailTextureName = $"NPCs/Minos/Extras/ViperTrail";

        protected override string trailTextureName => $"TestContent/NPCs/Minos/Extras/ViperTrail";

        public override PrimitiveSettings SetPrimitiveSettings()
        {
            return new PrimitiveSettings(PrimitiveWidthFunction, PrimitiveColorFunction, PrimitiveOffsetFunction, pixelate: true, shader: GameShaders.Misc["TestContent:TrailDirect"]);
        }
        public override void SetDefaults()
        {
            base.SetDefaults();//Primitive Settings are set here
            Projectile.width = 28;
            Projectile.height = 32;
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
                return 1f * Projectile.scale * 8;
            }
            return ((inverse - (completionRatio - splitPoint)) / inverse) * Projectile.scale * 8;
        }

        public override Vector2 PrimitiveOffsetFunction(float completionRatio)
        {
            Vector2 orig = base.PrimitiveOffsetFunction(completionRatio);
            var forward = Projectile.velocity;
            forward.Normalize();
            orig += forward * -5;
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
            SoundEngine.PlaySound(Viper.snakeShatter, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var effects = Projectile.direction == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Main.EntitySpriteDraw(Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, Projectile.Hitbox.Size() / 2, Projectile.scale, effects);
            return false;
        }
    }
}
