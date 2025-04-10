using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using TextContent.Effects.Graphics.Primitives;

namespace TestContent.Projectiles
{
    public class TestTrailProjectile : BasicTrailProjectile
    {
        protected new int trailLength = 15;
        public Player Owner
        {
            get
            {
                return Main.player[Projectile.owner];
            }
        }

        protected override string trailTextureName => "";

        public Vector2 ownerMousePos;
        public override void SetDefaults()
        {
            base.SetDefaults();//Primitive Settings are set here
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(ownerMousePos);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            if(Main.myPlayer != Projectile.owner)
            {
                ownerMousePos = reader.ReadVector2();
            }
        }

        public override void AI()
        {
            if(Main.myPlayer == Projectile.owner)
            {
                ownerMousePos = Main.MouseWorld;
            }

            var dir = ownerMousePos - Projectile.position;
            var dist = dir.Length();
            dir.Normalize();

            var speed = 15f;
            var inertia = 10f;

            var maxVelocity = dir * speed;
            if (dist > 50f)
            {
                Projectile.velocity = (Projectile.velocity * (inertia - 1) + maxVelocity) / inertia;
            }

            Lighting.AddLight(Projectile.position, Main.DiscoColor.ToVector3());
        }

        public override float PrimitiveWidthFunction(float completionRatio)
        {
            float startingCutoff = 0.05f;
            if(completionRatio < startingCutoff)
            {
                return completionRatio * Projectile.scale * 8;
            }
            return (1 - completionRatio) * Projectile.scale * 8;
        }

        public override Vector2 PrimitiveOffsetFunction(float completionRatio)
        {
            return base.PrimitiveOffsetFunction(completionRatio);
        }

        public override Color PrimitiveColorFunction(float completionRatio)
        {
            var col1 = Main.DiscoColor;
            var col2 = Color.Transparent;
            return Color.Lerp(col1, col2, completionRatio);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
