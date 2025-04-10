using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent;
using TestContent.Players;

namespace TestContent.Projectiles.Weapons
{
    public class CautionSignProjectile : ModProjectile
    {
        // We define some constants that determine the swing range of the sword
        // Not that we use multipliers here since that simplifies the amount of tweaks for these interactions
        // You could change the values or even replace them entirely, but they are tweaked with looks in mind
        private const float SWINGRANGE = 1.67f * (float)Math.PI;
        private const float SPINRANGE = 5.2f * (float)Math.PI;
        private const float FIRSTHALFSWING = 0.45f; // How much of the swing happens before it reaches the target angle (in relation to swingRange)
        private const float UNWIND = 0.4f; // When should the sword start disappearing
        private const float SPINTIME = 2.5f;

        private SoundStyle swing;
        private SoundStyle goreDeath;
        private SoundStyle goreVar1;
        private SoundStyle goreVar2;

        private enum AttackType // Which attack is being performed
        {
            Swing,
            Upswing,
            Spin,
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
        private float execTime = 15f;
        private float hideTime = 15f;

        public override string Texture => "TestContent/Items/Weapons/CautionSign"; // Use texture of item as projectile texture
        private Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }

        public override void SetDefaults()
        {
            Projectile.width = 56; // Hitbox width of projectile
            Projectile.height = 56; // Hitbox height of projectile
            Projectile.friendly = true; // Projectile hits enemies
            Projectile.timeLeft = 10000; // Time it takes for projectile to expire
            Projectile.penetrate = -1; // Projectile pierces infinitely
            Projectile.tileCollide = false; // Projectile does not collide with tiles
            Projectile.usesLocalNPCImmunity = true; // Uses local immunity frames
            Projectile.localNPCHitCooldown = -1; // We set this to -1 to make sure the projectile doesn't hit twice
            Projectile.ownerHitCheck = true; // Make sure the owner of the projectile has line of sight to the target (aka can't hit things through tile).
            Projectile.DamageType = DamageClass.Melee; // Projectile is a melee projectile

            swing = new SoundStyle("TestContent/Assets/Sounds/swing")
            {
                Volume = 0.8f,
                PitchVariance = 0.2f,
                MaxInstances = 3,
            };
            goreDeath = new SoundStyle("TestContent/Assets/Sounds/goreExplode")
            {
                Volume = 0.3f,
                PitchVariance = 0.2f,
                MaxInstances = 3,
            };
            goreVar1 = new SoundStyle("TestContent/Assets/Sounds/Flesh_hit_1")
            {
                Volume = 0.3f,
                PitchVariance = 0.2f,
                MaxInstances = 1,
            };
            goreVar2 = new SoundStyle("TestContent/Assets/Sounds/Flesh_hit_2")
            {
                Volume = 0.3f,
                PitchVariance = 0.2f,
                MaxInstances = 1,
            };
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (CurrentAttack == AttackType.Spin)
            {
                Projectile.damage = (int)(Projectile.originalDamage * 0.75f);
            }

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
            InitialAngle = targetAngle - FIRSTHALFSWING * SWINGRANGE * Projectile.spriteDirection; // Otherwise, we calculate the angle
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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            PlayerWeapons wep = Owner.GetModPlayer<PlayerWeapons>();
            if (target.netID != NPCID.TargetDummy && !target.SpawnedFromStatue)
            {
                wep.GrantSignAltAmmo(2);
            }

            if (Main.netMode != NetmodeID.Server)
            {
                if (target.life <= 0)
                {
                    SoundEngine.PlaySound(goreDeath, target.Center);
                    GoreDeath(target.Center, Owner.Center);
                }
                else
                {
                    if (hit.Crit)
                    {
                        SoundEngine.PlaySound(goreVar1, target.Center);
                    }
                    else
                    {
                        SoundEngine.PlaySound(goreVar2, target.Center);
                    }
                    GoreDust(target.Center, Owner.Center);
                }
            }
        }

        public void GoreDust(Vector2 pos, Vector2 playerPos)
        {
            Vector2 dir = Vector2.Normalize(playerPos - pos);
            dir = dir.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(80f, 100f)));
            for (int i = 0; i < 10; i++)
            {
                int dust = Dust.NewDust(pos, 0, 0, DustID.Blood, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), Scale: Main.rand.NextFloat(1.3f, 2f));
            }
            for (int j = 0; j < 15; j++)
            {
                Vector2 line = dir * Main.rand.NextFloat(-8f, 8f);
                int moreDust = Dust.NewDust(pos, 0, 0, DustID.Blood, line.X, line.Y, Scale: Main.rand.NextFloat(1.9f, 2f));

            }
        }

        public void GoreDeath(Vector2 pos, Vector2 playerPos)
        {
            Vector2 dir = Vector2.Normalize(playerPos - pos);
            dir = dir.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(80f, 100f)));
            for (int i = 0; i < 20; i++)
            {
                int dust = Dust.NewDust(pos, 0, 0, DustID.Blood, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), Scale: Main.rand.NextFloat(2f, 2.5f));
            }
            for (int j = 0; j < 15; j++)
            {
                Vector2 line = dir * Main.rand.NextFloat(-15f, 15f);
                int moreDust = Dust.NewDust(pos, 0, 0, DustID.Blood, line.X, line.Y, Scale: Main.rand.NextFloat(2.5f, 3f));

            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Calculate origin of sword (hilt) based on orientation and offset sword rotation (as sword is angled in its sprite)
            Vector2 origin;
            float rotationOffset;
            SpriteEffects effects;
            SpriteEffects swingEffects;
            float angleOffset = 10f;
            Vector2 pos = Projectile.position - Main.screenPosition;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Main.instance.LoadProjectile(ProjectileID.TheHorsemansBlade);
            Texture2D swing = TextureAssets.Projectile[ProjectileID.TheHorsemansBlade].Value;

            int swingHeight = swing.Height / 4;
            int frame = (int)(Timer * 0.1f) % 4;
            int startY = swingHeight * frame;
            Rectangle swingRect = new Rectangle(1, startY, swing.Width, swingHeight);
            Vector2 swingOrigin = new Vector2(swingRect.Size().X / 4, swingRect.Size().Y / 2);



            float swingOffset = 0f;

            //Main.NewText(swingHeight + " " + frame + " " + startY);

            float rotV;

            if (Projectile.spriteDirection > 0)
            {
                origin = new Vector2(texture.Width / 2, texture.Height / 2);
                rotationOffset = MathHelper.ToRadians(25f - angleOffset);
                rotV = MathHelper.ToRadians(15f);
                effects = SpriteEffects.None;
                swingEffects = SpriteEffects.None;
                swingOffset -= MathHelper.ToRadians(33f);
            }
            else
            {
                origin = new Vector2(texture.Width / 2, texture.Height / 2);
                rotationOffset = MathHelper.ToRadians(145f + angleOffset);
                rotV = MathHelper.ToRadians(10f);
                effects = SpriteEffects.FlipHorizontally;
                swingEffects = SpriteEffects.FlipVertically;
                swingOffset += MathHelper.ToRadians(22f);
            }

            rotV += rotationOffset;
            if (CurrentAttack == AttackType.Upswing)
            {
                swingOffset = 0;
                if (Projectile.spriteDirection > 0)
                {
                    effects = SpriteEffects.FlipHorizontally;
                    rotationOffset += MathHelper.ToRadians(110f);
                    rotV -= MathHelper.ToRadians(13f);
                }
                else
                {
                    effects = SpriteEffects.None;
                    rotationOffset += MathHelper.ToRadians(-110f);
                    rotV += MathHelper.ToRadians(13f);
                    //origin = origin.RotatedBy(MathHelper.ToDegrees());
                }

            }

            Vector2 rotationOffsetV = (Projectile.rotation - rotV).ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale) * 0.4f;
            if (Projectile.spriteDirection > 0)
            {
                rotationOffsetV *= -1;
            }
            PlayerWeapons wep = Owner.GetModPlayer<PlayerWeapons>();
            float ammoPerc = wep.SignAltAmmo / (float)PlayerWeapons.SIGNMAXAMMO;
            Color swingColor;
            if (ammoPerc >= 1)
            {
                swingColor = Color.Red;
                lightColor = Color.Lerp(lightColor, swingColor, 0.5f);
            }
            else
            {
                swingColor = Color.Lerp(Color.Black, Color.Firebrick, ammoPerc);
            }
            swingColor = Color.Lerp(swingColor, lightColor, 0.1f);
            float drawScale = 0;
            Vector2 start = Owner.MountedCenter;
            float lengthMod = 1f;
            Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale) * lengthMod;
            if (Projectile.spriteDirection > 0)
            {
                end = start + (Projectile.rotation - MathHelper.ToRadians(20f)).ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale) * lengthMod;
            }

            if (CurrentAttack != AttackType.Spin)
            {
                end = Vector2.Lerp(start, end, 0.13f);
                drawScale = 1.6f * Owner.GetAdjustedItemScale(Owner.HeldItem);
            }
            else
            {
                end = Vector2.Lerp(start, end, 0.35f);
                drawScale = 1f * Owner.GetAdjustedItemScale(Owner.HeldItem);
            }
            end.Y += Owner.gfxOffY;
            end -= Main.screenPosition;
            Main.spriteBatch.Draw(swing, end + rotationOffsetV, swingRect, swingColor * Projectile.Opacity, Projectile.rotation + swingOffset, swingOrigin, drawScale, swingEffects, 0);
            Main.spriteBatch.Draw(texture, pos - rotationOffsetV, default, lightColor, Projectile.rotation + rotationOffset, origin, Projectile.scale, effects, 0);
            // Since we are doing a custom draw, prevent it from normally drawing
            return false;
        }

        // Find the start and end of the sword and use a line collider to check for collision with enemies
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 start = Owner.MountedCenter;
            float colScale = Projectile.scale;
            if (colScale > 0.9f)
            {
                colScale = CurrentAttack == AttackType.Spin ? 1f : 1.5f;
            }
            colScale *= 1.5f;
            Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * colScale);
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 15f * colScale, ref collisionPoint);
        }

        // Do a similar collision check for tiles
        public override void CutTiles()
        {
            Vector2 start = Owner.MountedCenter;
            Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale);
            Utils.PlotTileLine(start, end, Projectile.width * Projectile.scale, DelegateMethods.CutTiles);
        }

        public void SpawnDust()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                return;
            }

            float lengthMod = 1.1f;
            Vector2 start = Owner.MountedCenter;
            Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale) * lengthMod;

            if (Projectile.spriteDirection > 0)
            {
                end = start + (Projectile.rotation - MathHelper.ToRadians(20f)).ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale) * lengthMod;
            }

            for (int i = 0; i < 3; i++)
            {
                Vector2 line = Vector2.Lerp(start, end, Main.rand.NextFloat(0.4f, 1f));
                Vector2 dir = end - start;
                dir.Normalize();
                dir = dir.RotatedBy(MathHelper.ToRadians(90f));
                float randSpeedMod = Main.rand.NextFloat(5f, 10f);
                int dust = Dust.NewDust(line, 0, 0, DustID.Ash, SpeedX: dir.X * randSpeedMod, SpeedY: dir.Y * randSpeedMod, Scale: Main.rand.NextFloat(0.6f, 1.5f));
                Main.dust[dust].noGravity = true;
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

            if (Main.rand.NextBool(3))
            {
                target.AddBuff(BuffID.Confused, 600, true);
            }
            //Inflict On Fire! for 10 sec

            // If the NPC is hit by the spin attack, increase knockback slightly
            if (CurrentAttack == AttackType.Spin)
            {
                modifiers.Knockback -= 1;
            }
        }

        // Function to easily set projectile and arm position
        public void SetSwordPosition()
        {
            Projectile.rotation = InitialAngle + Projectile.spriteDirection * Progress; // Set projectile rotation

            // Set composite arm allows you to set the rotation of the arm and stretch of the front and back arms independently
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f)); // set arm position (90 degree offset since arm starts lowered)
            Vector2 armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)Math.PI / 2); // get position of hand

            armPosition.Y += Owner.gfxOffY;
            Projectile.position = armPosition; // Set projectile to arm position
            Projectile.scale = Size * 1.2f * Owner.GetAdjustedItemScale(Owner.HeldItem); // Slightly scale up the projectile and also take into account melee size modifiers

            Owner.heldProj = Projectile.whoAmI; // set held projectile to this projectile
        }


        // Function facilitating the first half of the swing
        private void ExecuteStrike()
        {
            if (CurrentAttack == AttackType.Swing)
            {
                Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) * Timer / execTime);
            }
            else if (CurrentAttack == AttackType.Upswing)
            {
                Progress = SWINGRANGE - MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) * Timer / execTime);
            }
            else
            {
                DoSpinExecute();
                return;
            }

            Size = MathHelper.SmoothStep(0, 1.5f, MathF.Pow(Timer / execTime, 2f));
            Projectile.Opacity = MathHelper.SmoothStep(0, 0.3f, MathF.Pow(Timer / execTime, 2f));
            if (Timer == (int)(execTime * 0.5f))
            {
                SoundEngine.PlaySound(swing);
            }
            if (Timer >= execTime * 0.5f)
            {
                SpawnDust();
            }

            if (Timer >= execTime)
            {
                CurrentStage = AttackStage.Unwind;
            }
        }

        private void DoSpinExecute()
        {
            Progress = MathHelper.SmoothStep(0, SPINRANGE, (1f - UNWIND / 2) * Timer / (execTime * SPINTIME));
            Size = MathHelper.SmoothStep(0, 1f, Timer / (execTime * 0.3f * SPINTIME));
            Projectile.Opacity = MathHelper.SmoothStep(0, 0.3f, MathF.Pow(Timer / execTime, 2f));
            if (Timer >= execTime * 0.2f * SPINTIME)
            {
                SpawnDust();
            }
            float repTime = (int)(SPINTIME * execTime / 3);
            if (Timer % repTime == 0 && Timer >= repTime)
            {
                SoundEngine.PlaySound(swing); // Play sword sound again
                Projectile.ResetLocalNPCHitImmunity(); // Reset the local npc hit immunity for second half of spin
            }
            if (Timer >= execTime * SPINTIME)
            {
                CurrentStage = AttackStage.Unwind;
            }
        }

        // Function facilitating the latter half of the swing where the sword disappears
        private void UnwindStrike()
        {
            if (CurrentAttack == AttackType.Swing)
            {
                Progress = MathHelper.SmoothStep(0, SWINGRANGE, 1f - UNWIND + UNWIND * Timer / hideTime);

            }
            else if (CurrentAttack == AttackType.Upswing)
            {
                Progress = SWINGRANGE - MathHelper.SmoothStep(0, SWINGRANGE, 1f - UNWIND + UNWIND * Timer / hideTime);
            }
            else
            {
                DoSpinWindDown();
                return;
            }

            Size = 1.5f - MathHelper.SmoothStep(0, 1.5f, Timer / hideTime); // Make sword slowly decrease in size as we end the swing to make a smooth hiding animation
            Projectile.Opacity = 0.3f - MathHelper.SmoothStep(0, 0.3f, MathF.Pow(Timer / (hideTime / 2f), 2f));

            if (Timer >= hideTime)
            {
                Projectile.Kill();
            }
            if (Timer <= hideTime / 2)
            {
                SpawnDust();
            }
        }

        private void DoSpinWindDown()
        {
            Progress = MathHelper.SmoothStep(0, SPINRANGE, 1f - UNWIND / 2 + UNWIND / 2 * Timer / (hideTime * SPINTIME / 2));
            Size = 1f - MathHelper.SmoothStep(0, 1f, Timer / (hideTime * SPINTIME / 2));
            Projectile.Opacity = 0.3f - MathHelper.SmoothStep(0, 0.3f, MathF.Pow(Timer / (hideTime / 2f), 2f));

            if (Timer <= hideTime * SPINTIME / 2 / 2)
            {
                SpawnDust();
            }
            if (Timer >= hideTime * SPINTIME / 2)
            {
                Projectile.Kill();
            }
        }


    }
}
