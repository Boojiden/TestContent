using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Dusts;
using TestContent.Items.Weapons;
using TestContent.Players;
using TestContent.Utility;
using static TestContent.UI.SlotMachineSystem;

namespace TestContent.Projectiles.Weapons.Gambling.Hardmode
{
    public class RouletteGunHoldout : ModProjectile
    {
        private enum GunStage
        {
            PullOut,
            Shoot,
            PutBack
        }

        private const int PULLOUTTIME = 30;
        private const int PUTBACKTIME = 20;
        private const float WEAPONOFFSET = 20f;

        private SoundStyle startup;
        private SoundStyle revLoop;
        private SoundStyle shoot;

        public bool stillInUse = false;
        public Vector2 mousePos;
        public Vector2 shootPos;

        public int pullOutTime = 30;
        public int baseFireDelay = 30;
        public int fireDelayOffset = 0;
        public int maxOffset = 20;

        public float shootSpeed = 20f;

        private LayeredRollingSystem shotRoll;

        private bool startedThisFrame = true;

        private bool loopPlaying = false;

        private SlotId startupSound;
        private SlotId loopSound;
        public float Timer
        {
            get { return Projectile.ai[0]; }
            set
            {
                if (Owner.whoAmI == Main.myPlayer)
                    Projectile.ai[0] = value;
            }
        }

        private GunStage Mode
        {
            get => (GunStage)Projectile.ai[1];
            set
            {
                if (Owner.whoAmI == Main.myPlayer)
                {
                    Projectile.ai[1] = (float)value;
                    Timer = 0; // reset the timer when the projectile switches states
                }
            }
        }
        private Player Owner => Main.player[Projectile.owner];
        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 32;
            Projectile.tileCollide = false;
            startup = new SoundStyle(ModUtils.GetSoundFileLocation("RouletteGunRev"))
            {
                Volume = 0.4f,
                MaxInstances = 3,
                PlayOnlyIfFocused = true,
            };
            revLoop = new SoundStyle(ModUtils.GetSoundFileLocation("RouletteGunRevLoop"))
            {
                Volume = 0.4f,
                MaxInstances = 3,
                IsLooped = true,
                PlayOnlyIfFocused = true,
            };
            shoot = new SoundStyle(ModUtils.GetSoundFileLocation("RouletteGunShoot"))
            {
                Volume = 0.3f,
                PitchVariance = 0.2f,
                MaxInstances = 3,
                PlayOnlyIfFocused = true,
            };

            shotRoll = new LayeredRollingSystem();

            shotRoll.Add(70);
            shotRoll.Add(20);
            shotRoll.Add(10);
        }

        public override bool? CanDamage()
        {
            return false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            // Projectile.spriteDirection for this projectile is derived from the mouse position of the owner in OnSpawn, as such it needs to be synced. spriteDirection is not one of the fields automatically synced over the network. All Projectile.ai slots are used already, so we will sync it manually. 
            writer.Write((sbyte)Projectile.spriteDirection);
            writer.Write(stillInUse);
            writer.Write(Main.MouseWorld.X);
            writer.Write(Main.MouseWorld.Y);
            writer.Write(Projectile.scale);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.spriteDirection = reader.ReadSByte();
            stillInUse = reader.ReadBoolean();
            mousePos = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            Projectile.scale = reader.ReadSingle();
        }

        public override void OnSpawn(IEntitySource source)
        {
            Mode = GunStage.PullOut;
            Projectile.scale = 0;
            //SoundEngine.PlaySound(startup);
        }

        public override void AI()
        {
            if (startedThisFrame)
            {
                startupSound = SoundEngine.PlaySound(startup, Projectile.Center);
                startedThisFrame = false;
            }

            ActiveSound sound;

            if (!SoundEngine.TryGetActiveSound(startupSound, out sound) && !loopPlaying)
            {
                loopSound = SoundEngine.PlaySound(revLoop with {IsLooped = true}, Projectile.Center);
                loopPlaying = true;
            }

            if(loopPlaying)
            {
                if(SoundEngine.TryGetActiveSound(loopSound, out sound))
                {
                    sound.Position = Projectile.Center;
                    //Main.NewText("Updated Position?");
                }
            }

            

            PlayerWeapons wep = Owner.GetModPlayer<PlayerWeapons>();
            Vector2 rrp = Owner.RotatedRelativePoint(Owner.MountedCenter, addGfxOffY: true);
            UpdatePlayerVisuals(rrp);
            Vector2 aim;
            if (Owner.whoAmI == Main.myPlayer)
            {
                aim = Vector2.Normalize(Main.MouseWorld - rrp);
            }
            else
            {
                aim = Vector2.Normalize(mousePos - rrp);
            }
            if (aim.HasNaNs())
            {
                aim = -Vector2.UnitY;
            }
            if (Owner.whoAmI == Main.myPlayer)
            {
                switch (Mode)
                {
                    case GunStage.PullOut:

                        DoPulloutAnim(aim, WEAPONOFFSET);
                        break;

                    case GunStage.Shoot:
                        stillInUse = Owner.channel && !Owner.noItems && !Owner.CCed;
                        UpdateAim(aim, WEAPONOFFSET);
                        if (!stillInUse)
                        {
                            //Main.NewText($"Hello Again");
                            Mode = GunStage.PutBack;
                            break;
                        }
                        if (Timer >= baseFireDelay - fireDelayOffset)
                        {
                            ShootBullet(rrp, aim, shootSpeed, wep);
                            Timer = 0;
                        }
                        break;

                    case GunStage.PutBack:

                        DoPutBackAnim(aim, WEAPONOFFSET);
                        loopPlaying = false;
                        break;
                }
            }

            if(Mode == GunStage.PutBack)
            {
                if (SoundEngine.TryGetActiveSound(loopSound, out sound))
                {
                    sound.Stop();
                }
                if (SoundEngine.TryGetActiveSound(startupSound, out sound))
                {
                    sound.Stop();
                }
            }

            Timer++;
            Projectile.timeLeft = 2;
            Projectile.netUpdate = true;
        }

        private void UpdatePlayerVisuals(Vector2 playerHandPos)
        {
            Projectile.Center = playerHandPos;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.spriteDirection = Projectile.direction;
            // The Prism is a holdout Projectile, so change the player's variables to reflect that.
            // Constantly resetting player.itemTime and player.itemAnimation prevents the player from switching items or doing anything else.
            Owner.ChangeDir(Projectile.direction);
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            // If you do not multiply by Projectile.direction, the player's hand will point the wrong direction while facing left.
            Owner.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
        }

        private void UpdateAim(Vector2 aim, float speed)
        {
            // Get the player's current aiming direction as a normalized vector.
            float AimResponsiveness = 1f;

            // Change a portion of the Prism's current velocity so that it points to the mouse. This gives smooth movement over time.
            aim = Vector2.Normalize(Vector2.Lerp(Projectile.velocity, aim, AimResponsiveness));
            aim *= speed;

            Vector2 recoilAim;
            if (Projectile.spriteDirection == 1)
            {
                recoilAim = aim.RotatedBy(MathHelper.ToRadians(-4f));
            }
            else
            {
                recoilAim = aim.RotatedBy(MathHelper.ToRadians(4f));
            }
            aim = Vector2.Lerp(recoilAim, aim, ShootingLerp(Timer / baseFireDelay));

            Projectile.velocity = aim;
        }

        private void DoPulloutAnim(Vector2 aim, float speed)
        {
            float prog = MathHelper.SmoothStep(0, 1, PullInAndOutLerp(Timer / PULLOUTTIME));
            Projectile.scale = prog;
            float pulloutProg = MathHelper.SmoothStep(1, 0, ShootingLerpAlt(Timer / PULLOUTTIME));
            Projectile.velocity = Vector2.Lerp(-Vector2.UnitY * speed, aim * speed, pulloutProg);
            if (Timer >= PULLOUTTIME)
            {
                Mode = GunStage.Shoot;
                Timer = baseFireDelay;
                Projectile.velocity = aim * speed;
            }
        }

        private void DoPutBackAnim(Vector2 aim, float speed)
        {
            float prog = MathHelper.SmoothStep(0, 1, Timer / PUTBACKTIME);
            Projectile.scale = 1 - prog;
            Projectile.velocity = Vector2.Lerp(aim * speed, Vector2.UnitY * speed, prog);
            if (Timer >= PUTBACKTIME)
            {
                Projectile.Kill();
            }
        }

        private void ShootBullet(Vector2 origin, Vector2 dir, float speed, PlayerWeapons wep)
        {
            if (Owner.PickAmmo(ModContent.GetInstance<RouletteGun>().Entity, out int type, out float shotspeed, out int damage, out float knockback, out int usedAmmoItemId))
            {
                var offset = dir;
                if (Owner.direction > 0)
                {
                    //offset = dir.RotatedBy(-Math.PI / 2);
                }
                else
                {
                    //offset = dir.RotatedBy(Math.PI / 2);
                }

                offset *= 20f;
                origin -= offset;
                shootPos = origin + Projectile.velocity * 4.8f;

                int attack = shotRoll.Roll();
                int numShots = 0;

                switch (attack)
                {
                    case 0:
                        numShots = 1;
                        break;
                    case 1:
                        numShots = 3;
                        break;
                    case 2:
                        numShots = 5;
                        break;
                }

                float spread = 15f;
                Vector2 shotDir = dir.RotatedBy(MathHelper.ToRadians(-spread * ((numShots - 1) / 2)));

                for(int i = 0; i < numShots; i++)
                {
                    Projectile.NewProjectile(Terraria.Entity.InheritSource(Entity), shootPos, shotDir * shotspeed, type, damage, knockback, ai0: attack);
                    shotDir = shotDir.RotatedBy(MathHelper.ToRadians(spread));
                }

                SoundEngine.PlaySound(shoot);

                fireDelayOffset++;

                fireDelayOffset = Math.Clamp(fireDelayOffset, 0, maxOffset);



                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 vel = dir * 5f;
                        int dust = Dust.NewDust(shootPos, 0, 0, DustID.Torch, vel.X, vel.Y, Scale: Main.rand.NextFloat(0.5f, 1.2f));

                        vel *= Main.rand.NextFloat(0.5f, 6f);
                        int smoke = Dust.NewDust(shootPos, 0, 0, DustID.Asphalt, vel.X, vel.Y, Scale: Main.rand.NextFloat(0.8f, 0.9f));
                        Main.dust[smoke].noGravity = true;

                    }
                    //int cartridge = Dust.NewDust(Projectile.Center, 1, 1, ModContent.DustType<M24CartdridgeDust>(), offset.X, offset.Y);
                }
            }
            else
            {
                Mode = GunStage.PutBack;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 sheetInsertPosition = Projectile.Center + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
            Main.spriteBatch.Draw(texture, sheetInsertPosition, default, lightColor, Projectile.rotation, new Vector2(texture.Width / 2f, texture.Height / 2f), Projectile.scale, effects, 0f);

            //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, shootPos - Main.screenPosition, Color.White);
            return false;
        }

        public static float ShootingLerp(float k)
        {

            if (k == 0) return 0;
            if (k == 1) return 1;
            return (float)(Math.Pow(2f, -10f * k) * Math.Sin(10 * k - 0.75f) * 4f);
        }

        public static float ShootingLerpAlt(float k)
        {

            if (k == 0) return 0;
            if (k == 1) return 1;
            return (float)(Math.Pow(2f, -5f * k) * Math.Sin(5 * k - 0.75f) * 4f);
        }

        public float PullInAndOutLerp(float value)
        {
            return Flip(Quart(Flip(value)));
        }

        public float Flip(float t)
        {
            return 1 - t;
        }

        public float Quart(float t)
        {
            return t * t * t * t;
        }

        public float BounceInLerp(float t)
        {
            return 0;
        }

    }
}
