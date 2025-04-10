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
    public class ViperBlue : BasicTrailProjectile
    {
        protected new int trailLength = 25;
        //protected new string trailTextureName = $"NPCs/Minos/Extras/ViperTrail";
        protected override string trailTextureName => $"TestContent/NPCs/Minos/Extras/ViperTrailBlue";

        public override PrimitiveSettings SetPrimitiveSettings()
        {
            return new PrimitiveSettings(PrimitiveWidthFunction, PrimitiveColorFunction, PrimitiveOffsetFunction, pixelate: true, shader: GameShaders.Misc["TestContent:TrailDirect"]);
        }

        public NPC? Owner
        {
            get
            {
                if (Main.npc[(int)Projectile.ai[0]].active)
                {
                    return Main.npc[(int)Projectile.ai[0]];
                }
                else
                {
                    return null;
                }
            }
        }

        public Vector2 Offset
        {
            get
            {
                return new Vector2(Projectile.ai[1], Projectile.ai[2]);
            }
        }

        public override void Load()
        {

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
            //Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            
        }

        public override bool? CanDamage()
        {
            return false;
        }

        public override void AI()
        {
            //Main.NewText(Projectile.numHits);
            if(Owner == null)
            {
                Projectile.Kill();
                return;
            }
            Projectile.velocity = new Vector2(1f, 0f);
            Projectile.Center = Owner.Center + Offset;
            var dir = Owner.Center.GetVectorPointingTo(Projectile.Center);
            Projectile.rotation = (dir).ToRotation();
            Projectile.spriteDirection = Math.Sign(dir.X) == 0 ? 1 : Math.Sign(dir.X);

            if(Projectile.timeLeft < 15)
            {
                Projectile.Opacity = GameplayUtils.GetTimeFromInts(Projectile.timeLeft, 15);
            }
            Lighting.AddLight(Projectile.position, Color.Cyan.ToVector3());
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
            var forward = Projectile.rotation.ToRotationVector2();
            forward.Normalize();
            orig += forward * -7;
            return orig;
        }

        public override Color PrimitiveColorFunction(float completionRatio)
        {
            var col1 = Color.White;
            var col2 = Color.White * 0.5f;
            return Color.Lerp(col1, col2, completionRatio) * Projectile.Opacity;
        }

        public override void OnKill(int timeLeft)
        {
            if (!Main.dedServ)
            {
                DustUtils.CreateDustBurstCircle(DustID.Clentaminator_Cyan, Projectile.Center, 1f, 5f, 10);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Main.EntitySpriteDraw(Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, null, Color.White * Projectile.Opacity, Projectile.rotation, Projectile.Hitbox.Size() / 2, Projectile.scale, effects);
            return false;
        }
    }
}
