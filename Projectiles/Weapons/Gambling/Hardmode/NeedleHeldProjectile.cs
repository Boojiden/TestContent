using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.CodeAnalysis;
using ReLogic.Content;
using rail;
using TestContent.Utility;

namespace TestContent.Projectiles.Weapons.Gambling.Hardmode
{
    public class NeedleHeldProjectile : ModProjectile
    {
        // We define some constants that determine the swing range of the sword
        // Not that we use multipliers here since that simplifies the amount of tweaks for these interactions
        // You could change the values or even replace them entirely, but they are tweaked with looks in mind
        private const float SWINGRANGE = 1.67f * (float)Math.PI; // The angle a swing attack covers (300 deg)
        private const float FIRSTHALFSWING = 0.45f; // How much of the swing happens before it reaches the target angle (in relation to swingRange)
        private const float UNWIND = 0.4f; // When should the sword start disappearing

        private float maxOpacity = 0.225f;

        private Asset<Texture2D> swing;
        private Asset<Texture2D> swingHighlight;
        private Asset<Texture2D> poke;
        private Asset<Texture2D> pokeHighlight;

        private enum AttackType // Which attack is being performed
        {
            // Swings are normal sword swings that can be slightly aimed
            // Swings goes through the full cycle of animations
            Swing,
            // Spins are swings that go full circle
            // They are slower and deal more knockback
            Upswing,

            Thrust,
        }

        private enum AttackStage // What stage of the attack is being executed, see functions found in AI for description
        {
            Execute,
            Unwind
        }

        // These properties wrap the usual ai and localAI arrays for cleaner and easier to understand code.
        private AttackType CurrentAttack
        {
            get => (AttackType)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }

        private AttackStage CurrentStage
        {
            get => (AttackStage)Projectile.localAI[0];
            set
            {
                Projectile.localAI[0] = (float)value;
                Timer = 0; // reset the timer when the projectile switches states
            }
        }

        // Variables to keep track of during runtime
        private ref float InitialAngle => ref Projectile.ai[1]; // Angle aimed in (with constraints)
        private ref float Timer => ref Projectile.ai[2]; // Timer to keep track of progression of each stage
        private ref float Progress => ref Projectile.localAI[1]; // Position of sword relative to initial angle
        private ref float Size => ref Projectile.localAI[2]; // Size of sword

        // We define timing functions for each stage, taking into account melee attack speed
        // Note that you can change this to suit the need of your projectile
        private float execTime = 10f;
        private float hideTime = 10f;
        private Player Owner => Main.player[Projectile.owner];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8; // Hitbox width of projectile
            Projectile.height = 36; // Hitbox height of projectile
            Projectile.friendly = true; // Projectile hits enemies
            Projectile.timeLeft = 10000; // Time it takes for projectile to expire
            Projectile.penetrate = -1; // Projectile pierces infinitely
            Projectile.tileCollide = false; // Projectile does not collide with tiles
            Projectile.usesLocalNPCImmunity = true; // Uses local immunity frames
            Projectile.localNPCHitCooldown = -1; // We set this to -1 to make sure the projectile doesn't hit twice
            Projectile.ownerHitCheck = true; // Make sure the owner of the projectile has line of sight to the target (aka can't hit things through tile).
            Projectile.DamageType = DamageClass.Melee; // Projectile is a melee projectile

            var nameSpace = ModUtils.GetNamespaceFileLocation(typeof(NeedleHeldProjectile), true);

            swing = Mod.Assets.Request<Texture2D>($"{nameSpace}/NeedleSwing");
            swingHighlight = Mod.Assets.Request<Texture2D>($"{nameSpace}/NeedleSwingHighlight");

            poke = Mod.Assets.Request<Texture2D>($"{nameSpace}/NeedlePoke");
            pokeHighlight = Mod.Assets.Request<Texture2D>($"{nameSpace}/NeedlePokeHighlight");
        }

        public override void OnSpawn(IEntitySource source)
        {
            //Main.NewText(CurrentAttack.ToString());
            float speed = Owner.GetTotalAttackSpeed(Projectile.DamageType);
            if (speed >= 1f)
            {
                execTime /= speed - (speed - 1) * 0.5f;
                hideTime /= speed - (speed - 1) * 0.5f;
            }
            else
            {
                execTime /= speed;
                hideTime /= speed;
            }


            Projectile.spriteDirection = Main.MouseWorld.X > Owner.MountedCenter.X ? 1 : -1;
            float targetAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();
            SoundEngine.PlaySound(SoundID.Item1);
            /*
            if (CurrentAttack == AttackType.Spin)
            {
                InitialAngle = (float)(-Math.PI / 2 - Math.PI * 1 / 3 * Projectile.spriteDirection); // For the spin, starting angle is designated based on direction of hit
            }
            else
            {
                
            */

            if (CurrentAttack != AttackType.Thrust)
            {
                if (Projectile.spriteDirection == 1)
                {
                    // However, we limit the rangle of possible directions so it does not look too ridiculous
                    targetAngle = MathHelper.Clamp(targetAngle, (float)-Math.PI * 1 / 3, (float)Math.PI * 1 / 6);
                }
                else
                {
                    if (targetAngle < 0)
                    {
                        targetAngle += 2 * (float)Math.PI; // This makes the range continuous for easier operations
                    }

                    targetAngle = MathHelper.Clamp(targetAngle, (float)Math.PI * 5 / 6, (float)Math.PI * 4 / 3);
                }
            }

            if (CurrentAttack == AttackType.Thrust)
            {
                InitialAngle = targetAngle;
            }
            else
            {
                InitialAngle = targetAngle - FIRSTHALFSWING * SWINGRANGE * Projectile.spriteDirection; // Otherwise, we calculate the angle
            }
        }


        public override void SendExtraAI(BinaryWriter writer)
        {
            // Projectile.spriteDirection for this projectile is derived from the mouse position of the owner in OnSpawn, as such it needs to be synced. spriteDirection is not one of the fields automatically synced over the network. All Projectile.ai slots are used already, so we will sync it manually. 
            writer.Write((sbyte)Projectile.spriteDirection);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.spriteDirection = reader.ReadSByte();
        }

        public override void AI()
        {
            // Extend use animation until projectile is killed
            Owner.itemAnimation = 2;
            Owner.itemTime = 2;

            // Kill the projectile if the player dies or gets crowd controlled
            if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed)
            {
                //Main.NewText("CCDies");
                Projectile.Kill();
                return;
            }



            // AI depends on stage and attack
            // Note that these stages are to facilitate the scaling effect at the beginning and end
            // If this is not desireable for you, feel free to simplify
            switch (CurrentStage)
            {
                case AttackStage.Execute:
                    ExecuteStrike();
                    break;
                default:
                    UnwindStrike();
                    break;
            }

            SetSwordPosition();
            Timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Calculate origin of sword (hilt) based on orientation and offset sword rotation (as sword is angled in its sprite)

            GetSwordPoints(out Vector2 start, out Vector2 end);

            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            SpriteEffects effects = Projectile.spriteDirection == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Texture2D tex, highlight;
            if (CurrentAttack == AttackType.Thrust)
            {
                tex = poke.Value;
                highlight = pokeHighlight.Value;
            }
            else
            {
                tex = swing.Value;
                highlight = swingHighlight.Value;
            }

            var swingOrigin = new Vector2(tex.Width / 2, tex.Height / 2);

            bool switchEffect = Projectile.spriteDirection == 1 && CurrentAttack == AttackType.Upswing || Projectile.spriteDirection == -1 && CurrentAttack == AttackType.Swing;

            if (CurrentAttack != AttackType.Thrust)
            {
                if (switchEffect)
                {
                    swingOrigin += new Vector2(-6, -6);
                }
                else
                {
                    swingOrigin += new Vector2(6, -6);
                }
            }

            var swingColor = Color.Lerp(new Color(255, 255, 255, 0), lightColor, 0.1f);

            SpriteEffects swingEffect = switchEffect ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.spriteBatch.Draw(tex, end - Main.screenPosition, null, swingColor * Projectile.Opacity, Projectile.rotation + MathHelper.ToRadians(90), swingOrigin, Projectile.scale, swingEffect, 0f);
            Main.spriteBatch.Draw(highlight, end - Main.screenPosition, null, swingColor * (Projectile.Opacity * 2), Projectile.rotation + MathHelper.ToRadians(90), swingOrigin, Projectile.scale, swingEffect, 0f);
            Main.spriteBatch.Draw(texture, end - Main.screenPosition, null, lightColor, Projectile.rotation + MathHelper.ToRadians(135), Vector2.Zero, Projectile.scale, effects, 0f);

            // Since we are doing a custom draw, prevent it from normally drawing
            return false;
        }

        // Find the start and end of the sword and use a line collider to check for collision with enemies
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            GetSwordPoints(out Vector2 start, out Vector2 end);
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, Projectile.width * Projectile.scale, ref collisionPoint);
        }

        // Do a similar collision check for tiles
        public override void CutTiles()
        {
            GetSwordPoints(out Vector2 start, out Vector2 end);
            Utils.PlotTileLine(start, end, 15 * Projectile.scale, DelegateMethods.CutTiles);
        }

        public void SpawnDust()
        {
            GetSwordPoints(out Vector2 start, out Vector2 end);
            for (int i = 0; i < 3; i++)
            {
                Vector2 line = Vector2.Lerp(start, end, Main.rand.NextFloat(0.5f, 1f));
                Vector2 dir = end - start;
                dir.Normalize();
                if (CurrentAttack != AttackType.Thrust)
                {
                    dir = dir.RotatedBy(MathHelper.ToRadians(90f));
                }
                else
                {
                    dir = dir.RotatedBy(MathHelper.ToRadians(180f));
                    line -= dir * 15f;
                }

                float randSpeedMod = Main.rand.NextFloat(8f, 16f);
                int dust = Dust.NewDust(line, 0, 0, DustID.GoldCoin, SpeedX: dir.X * randSpeedMod, SpeedY: dir.Y * randSpeedMod, Scale: Main.rand.NextFloat(0.5f, 2f));
                Main.dust[dust].noGravity = true;

                //Dust.NewDustPerfect(start, DustID.GemRuby, Vector2.Zero).noGravity = true; 
                //Dust.NewDustPerfect(end, DustID.GemEmerald, Vector2.Zero).noGravity = true;
            }
        }

        // We make it so that the projectile can only do damage in its release and unwind phases
        public override bool? CanDamage()
        {
            return base.CanDamage();
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Make knockback go away from player
            modifiers.HitDirectionOverride = target.position.X > Owner.MountedCenter.X ? 1 : -1;

            //target.AddBuff(BuffID.OnFire, 600, true); //Inflict On Fire! for 10 sec

            // If the NPC is hit by the spin attack, increase knockback slightly
            if (CurrentAttack == AttackType.Upswing)
                modifiers.Knockback += 1;
        }

        // Function to easily set projectile and arm position
        public void SetSwordPosition()
        {
            if (CurrentAttack != AttackType.Thrust)
                Projectile.rotation = InitialAngle + Projectile.spriteDirection * Progress; // Set projectile rotation
            else
                Projectile.rotation = InitialAngle;// + Projectile.spriteDirection;
            // Set composite arm allows you to set the rotation of the arm and stretch of the front and back arms independently
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f)); // set arm position (90 degree offset since arm starts lowered)
            Vector2 armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)Math.PI / 2); // get position of hand

            armPosition.Y += Owner.gfxOffY;
            Projectile.position = armPosition; // Set projectile to arm position

            if (CurrentAttack == AttackType.Thrust)
            {
                Vector2 direction = new Vector2((float)Math.Cos(Projectile.rotation), (float)Math.Sin(Projectile.rotation));
                Projectile.position = armPosition + direction * Progress;
            }

            Projectile.scale = Size * 1.6f * Owner.GetAdjustedItemScale(Owner.HeldItem); // Slightly scale up the projectile and also take into account melee size modifiers

            Owner.heldProj = Projectile.whoAmI; // set held projectile to this projectile
        }


        // Function facilitating the first half of the swing
        private void ExecuteStrike()
        {
            Size = 1f;
            if (CurrentAttack == AttackType.Swing || CurrentAttack == AttackType.Thrust)
            {
                Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) * Timer / execTime);
            }
            else
            {
                Progress = MathHelper.SmoothStep(SWINGRANGE, 0, (1f - UNWIND) * Timer / execTime);
            }
            Size = MathHelper.SmoothStep(0, 1, Timer / (execTime * 0.5f));
            Projectile.Opacity = MathHelper.SmoothStep(0, maxOpacity, MathF.Pow(Timer / execTime, 2f));

            if (Timer >= execTime * 0.5f)
            {
                SpawnDust();
            }

            if (Timer >= execTime)
            {
                CurrentStage = AttackStage.Unwind;
            }
        }

        // Function facilitating the latter half of the swing where the sword disappears
        private void UnwindStrike()
        {
            if (CurrentAttack == AttackType.Swing || CurrentAttack == AttackType.Thrust)
            {
                Progress = MathHelper.SmoothStep(0, SWINGRANGE, 1f - UNWIND + UNWIND * Timer / hideTime);

            }
            else
            {
                Progress = MathHelper.SmoothStep(SWINGRANGE, 0, 1f - UNWIND + UNWIND * Timer / hideTime);
            }

            Size = 1f - MathHelper.SmoothStep(0, 1, Timer / hideTime); // Make sword slowly decrease in size as we end the swing to make a smooth hiding animation
            Projectile.Opacity = maxOpacity - MathHelper.SmoothStep(0, maxOpacity, MathF.Pow(Timer / (hideTime / 2f), 2f));
            if (Timer >= hideTime)
            {
                //Main.NewText("Dies");
                Projectile.Kill();
            }
            if (Timer <= hideTime / 2)
            {
                SpawnDust();
            }
        }

        private void GetSwordPoints(out Vector2 start, out Vector2 end)
        {
            start = Owner.MountedCenter;
            end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale) * 1.45f;
        }
    }
}
