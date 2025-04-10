using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace TestContent.Projectiles.Weapons.Gambling.Prehardmode
{
    public class LuckyShotBigShot : ModProjectile
    {
        private const float MaxBeamLength = 400f;
        private const float BeamTileCollisionWidth = 1f;
        private const float BeamHitboxCollisionWidth = 22f;
        private const int NumSamplePoints = 3;
        private const float BeamLengthChangeFactor = 0.75f;
        private const float OuterBeamOpacityMultiplier = 0.75f;
        private const float InnerBeamOpacityMultiplier = 0.1f;
        private const float BeamLightBrightness = 0.75f;

        public bool doFirstFrame = true;

        private float BeamLength
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private int revolverIndex
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public int Timer
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            // The beam itself still stops on tiles, but its invisible "source" Projectile ignores them.
            // This prevents the beams from vanishing if the player shoves the Prism into a wall.
            Projectile.tileCollide = false;

            // Using local NPC immunity allows each beam to strike independently from one another.
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;

        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Main.expertMode)
            {
                if (target.type >= NPCID.EaterofWorldsHead && target.type <= NPCID.EaterofWorldsTail)
                {
                    modifiers.FinalDamage /= 5;
                }
            }
        }

        public override void AI()
        {
            if (doFirstFrame)
            {
                doFirstFrame = false;
                Projectile.alpha = 0;
                Projectile revolver = Main.projectile[revolverIndex];
                if (!revolver.active || revolver.type != ModContent.ProjectileType<LuckyShotProjectile>())
                {
                    Projectile.Kill();
                    return;
                }
                Vector2 dir = revolver.velocity.SafeNormalize(Vector2.UnitY);
                Projectile.rotation = dir.ToRotation();

                float hitscanBeamLength = PerformBeamHitscan(revolver);
                BeamLength = hitscanBeamLength;

                Vector2 beamDims = new Vector2(Projectile.velocity.Length() * BeamLength, Projectile.width * Projectile.scale);
                if (!Main.dedServ)
                {
                    ProduceBeamDust(GetOuterBeamColor());
                }
                //Main.NewText("DONE");
                //Main.NewText($"Beam Length: {BeamLength}");
            }
            Timer++;
            Projectile.scale = 0.85f * ShootInLerp(1 - (60f - Timer) / 60f);
            if (Timer >= 60)
            {
                Projectile.Kill();
            }


        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // If the target is touching the beam's hitbox (which is a small rectangle vaguely overlapping the host Prism), that's good enough.
            if (Projectile.scale <= 0.1f)
            {
                return false;
            }
            if (projHitbox.Intersects(targetHitbox))
            {
                return true;
            }

            // Otherwise, perform an AABB line collision check to check the whole beam.
            float _ = float.NaN;
            Vector2 beamEndPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * BeamLength;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, beamEndPos, BeamHitboxCollisionWidth * Projectile.scale, ref _);
        }

        private float PerformBeamHitscan(Projectile revolver)
        {
            // By default, the hitscan interpolation starts at the Projectile's center.
            // If the host Prism is fully charged, the interpolation starts at the Prism's center instead.
            Vector2 samplingPoint = revolver.Center;

            // Overriding that, if the player shoves the Prism into or through a wall, the interpolation starts at the player's center.
            // This last part prevents the player from projecting beams through walls under any circumstances.
            Player player = Main.player[Projectile.owner];
            if (!Collision.CanHitLine(player.Center, 0, 0, revolver.Center, 0, 0))
            {
                samplingPoint = player.Center;
            }

            // Perform a laser scan to calculate the correct length of the beam.
            // Alternatively, if you want the beam to ignore tiles, just set it to be the max beam length with the following line.
            // return MaxBeamLength;
            float[] laserScanResults = new float[NumSamplePoints];
            Collision.LaserScan(samplingPoint, Projectile.rotation.ToRotationVector2(), 0 * Projectile.scale, MaxBeamLength, laserScanResults);
            float averageLengthSample = 0f;
            for (int i = 0; i < laserScanResults.Length; ++i)
            {
                averageLengthSample += laserScanResults[i];
            }
            averageLengthSample /= NumSamplePoints;

            return averageLengthSample;
        }

        public override bool PreDraw(ref Color lightColor)
        {

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 centerFloored = Projectile.Center.Floor() + Projectile.rotation.ToRotationVector2() * Projectile.scale * 10.5f;
            Vector2 drawScale = new Vector2(Projectile.scale);

            // Reduce the beam length proportional to its square area to reduce block penetration.
            float visualBeamLength = BeamLength - 14.5f * Projectile.scale * Projectile.scale;

            DelegateMethods.f_1 = 1f; // f_1 is an unnamed decompiled variable whose function is unknown. Leave it at 1.
            Vector2 startPosition = centerFloored - Main.screenPosition;
            Vector2 endPosition = startPosition + Projectile.rotation.ToRotationVector2() * visualBeamLength;

            // Draw the outer beam.
            DrawBeam(Main.spriteBatch, texture, startPosition, endPosition, drawScale, GetOuterBeamColor() * OuterBeamOpacityMultiplier * Projectile.Opacity);

            // Draw the inner beam, which is half size.
            drawScale *= 0.5f;
            DrawBeam(Main.spriteBatch, texture, startPosition, endPosition, drawScale, GetInnerBeamColor() * Projectile.Opacity);

            // Returning false prevents Terraria from trying to draw the Projectile itself.
            return false;
        }
        private void DrawBeam(SpriteBatch spriteBatch, Texture2D texture, Vector2 startPosition, Vector2 endPosition, Vector2 drawScale, Color beamColor)
        {
            Utils.LaserLineFraming lineFraming = new Utils.LaserLineFraming(DelegateMethods.RainbowLaserDraw);

            // c_1 is an unnamed decompiled variable which is the render color of the beam drawn by DelegateMethods.RainbowLaserDraw.
            DelegateMethods.c_1 = beamColor;
            Utils.DrawLaser(spriteBatch, texture, startPosition, endPosition, drawScale, lineFraming);
        }

        private Color GetOuterBeamColor() => Color.Yellow;

        // Inner beams are always pure white so that they act as a "blindingly bright" center to each laser.
        private Color GetInnerBeamColor() => Color.White;

        private void ProduceBeamDust(Color beamColor)
        {
            // Create one dust per frame a small distance from where the beam ends.
            const int type = 133;
            Vector2 endPosition = Projectile.Center + Projectile.rotation.ToRotationVector2() * (BeamLength - 14.5f * Projectile.scale);

            // Main.rand.NextBool is used to give a 50/50 chance for the angle to point to the left or right.
            // This gives the dust a 50/50 chance to fly off on either side of the beam.
            for (int i = 0; i < 15; i++)
            {
                float angle = Projectile.rotation + (Main.rand.NextBool() ? 1f : -1f) * MathHelper.PiOver2;
                float startDistance = Main.rand.NextFloat(1f, 1.8f);
                float scale = Main.rand.NextFloat(0.7f, 1.1f);
                Vector2 velocity = angle.ToRotationVector2() * startDistance;
                Dust dust = Dust.NewDustDirect(endPosition, 0, 0, type, velocity.X, velocity.Y, 0, beamColor, scale);
                dust.color = beamColor;
                dust.noGravity = true;
            }
        }

        public static float ShootInLerp(float time)
        {
            float c1 = 1.70158f;
            float c2 = c1 + 1f;

            if (time > 1f)
            {
                return 1f;
            }
            if (time < 0f)
            {
                return 0f;
            }
            return (float)Math.Pow(2, -8 * time + 1);// * ((float)Math.Sin(time * 5) * ((float)Math.PI * 2f / 3f));
        }


    }
}
