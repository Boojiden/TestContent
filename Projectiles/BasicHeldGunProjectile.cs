using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics;
using TestContent.Utility;
using Terraria.Audio;

namespace TestContent.Projectiles
{
    public abstract class BasicHeldGunProjectile : ModProjectile
    {
        public enum GunStage
        {
            PullOut,
            Shoot,
            PutBack
        }

        /// <summary>
        /// How long it takes to pull out the Held Projectile
        /// </summary>
        public virtual int PULLOUTTIME => 30;
        /// <summary>
        /// How long it takes to put back the Held Projectile
        /// </summary>
        public virtual int PUTBACKTIME => 20;

        /// <summary>
        /// How far away from the Player's mountedCenter should the weapon be?
        /// </summary>
        public virtual float WEAPONOFFSET => 20f;
        /// <summary>
        /// How Far from the weapon Offset does the bullet shoot?
        /// </summary>
        public virtual float BULLETOFFSET => 0f;
        /// <summary>
        /// What Projectile does the weapon shoot?
        /// </summary>
        public abstract int BulletType { get; }

        public virtual int DustType1 => DustID.Torch;
        public virtual int DustType2 => DustID.Asphalt;

        /// <summary>
        /// How long (in ticks) does it take to shoot a projectile?
        /// </summary>
        public virtual int BaseFireDelay => 30;
        /// <summary>
        /// Number subtracted from BaseFireDelay to augmnet shooting speed
        /// </summary>
        public virtual int FireRateModifier => 0;
        /// <summary>
        /// How fast should shot projetiles travel?
        /// </summary>
        public virtual float ShootSpeed => 20f;

        /// <summary>
        /// Whether to continuously check for owner input
        /// </summary>
        public virtual bool CheckActive => true;
        /// <summary>
        /// How much recoil should the gun give the player? In degrees rotation.
        /// </summary>
        public virtual float DegreesRecoil => 10f;

        public virtual SoundStyle? Sound => null;

        private bool startedThisFrame = true;
        public bool stillInUse = false;
        public Vector2 mousePos;
        public Vector2 shootPos;
        public Item weapon;

        public int Timer
        {
            get { return (int)Projectile.ai[0]; }
            set
            {
                if (Owner.whoAmI == Main.myPlayer)
                    Projectile.ai[0] = (float)value;
            }
        }

        public GunStage Mode
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

        protected Player Owner => Main.player[Projectile.owner];
        public override void SetDefaults()
        {
            Projectile.tileCollide = false;
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

        /// <summary>
        /// Runs once when the first run of AI() is done
        /// </summary>
        public virtual void FirstFrameAction()
        {

        }
        /// <summary>
        /// Insertion point for running AI behavior. Runs before the state logic runs
        /// </summary>
        public virtual void GunAI()
        {

        }

        public override void AI()
        {
            if (startedThisFrame)
            {
                FirstFrameAction();
                startedThisFrame = false;
            }
            GunAI();
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
                        if (CheckActive && !stillInUse)
                        {
                            //Main.NewText($"Hello Again");
                            Mode = GunStage.PutBack;
                            break;
                        }
                        if (Timer >= BaseFireDelay - FireRateModifier)
                        {
                            ShootBullet(rrp, aim, ShootSpeed);
                            if(Mode != GunStage.Shoot)
                            {
                                break;
                            }
                        }
                        UpdateAim(aim, WEAPONOFFSET);
                        break;

                    case GunStage.PutBack:

                        DoPutBackAnim(aim, WEAPONOFFSET);
                        break;
                }
            }
            //Main.NewText($"{Mode} : {Timer}");
            Timer++;
            Projectile.timeLeft = 2;
            Projectile.netUpdate = true;
        }

        private void UpdatePlayerVisuals(Vector2 playerHandPos)
        {
            Projectile.Center = playerHandPos;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.spriteDirection = Projectile.direction;
            Owner.ChangeDir(Projectile.direction);
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            // If you do not multiply by Projectile.direction, the player's hand will point the wrong direction while facing left.
            Owner.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
        }
        /// <summary>
        /// Determines how the gun will look when shooting
        /// </summary>
        /// <param name="aim"></param>
        /// <param name="offset"></param>
        public virtual void UpdateAim(Vector2 aim, float offset)
        {
            // Get the player's current aiming direction as a normalized vector.
            float AimResponsiveness = 1f;

            // Change a portion of the Prism's current velocity so that it points to the mouse. This gives smooth movement over time.
            aim = Vector2.Normalize(Vector2.Lerp(Projectile.velocity, aim, AimResponsiveness));
            aim *= offset;

            Vector2 recoilAim;
            if (Projectile.spriteDirection == 1)
            {
                recoilAim = aim.RotatedBy(MathHelper.ToRadians(-DegreesRecoil));
            }
            else
            {
                recoilAim = aim.RotatedBy(MathHelper.ToRadians(DegreesRecoil));
            }
            aim = Vector2.Lerp(aim, recoilAim, ShootingLerp(GameplayUtils.GetTimeFromInts(Timer, BaseFireDelay)));

            Projectile.velocity = aim;
        }
        /// <summary>
        /// Determines how the gun will look when pulling it out / in
        /// </summary>
        /// <param name="aim"></param>
        /// <param name="offset"></param>
        public virtual void DoPulloutAnim(Vector2 aim, float offset)
        {
            float prog;
            float pulloutProg;
            if (PULLOUTTIME <= 0)
            {
                prog = 1f;
                pulloutProg = 1f;
            }
            else
            {
                float time = GameplayUtils.GetTimeFromInts(Timer, PULLOUTTIME);
                prog = MathHelper.SmoothStep(0, 1, PullInAndOutLerp(time));
                pulloutProg = MathHelper.SmoothStep(1, 0, ShootingLerpAlt(time));
            }
            Projectile.scale = prog;
            Projectile.velocity = Vector2.Lerp(-Vector2.UnitY * offset, aim * offset, pulloutProg);
            if (Timer >= PULLOUTTIME)
            {
                Mode = GunStage.Shoot;
                Timer = BaseFireDelay;
                Projectile.velocity = aim * offset;
            }
        }

        private void DoPutBackAnim(Vector2 aim, float speed)
        {
            float prog = MathHelper.SmoothStep(0, 1, GameplayUtils.GetTimeFromInts(Timer , PUTBACKTIME));
            Projectile.scale = 1f - prog;
            Projectile.velocity = Vector2.Lerp(aim * speed, Vector2.UnitY * speed, prog);
            if (Timer >= PUTBACKTIME)
            {
                Projectile.Kill();
            }
        }
        /// <summary>
        /// USe this to spawn dusts when the gun fires
        /// </summary>
        /// <param name="dir"></param>
        public virtual void ShootDust(Vector2 dir) 
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = dir * 5f;
                int dust = Dust.NewDust(shootPos, 0, 0, DustType1, vel.X, vel.Y, Scale: Main.rand.NextFloat(0.5f, 1.2f));

                vel *= Main.rand.NextFloat(0.5f, 6f);
                int smoke = Dust.NewDust(shootPos, 0, 0, DustType2, vel.X, vel.Y, Scale: Main.rand.NextFloat(0.8f, 0.9f));
                Main.dust[smoke].noGravity = true;
            }
        }
        /// <summary>
        /// Determines if your weapon can shoot. Weapon dies if it can't
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual bool CanShoot(out int type)
        {
            type = BulletType;
            return true;
        }

        public virtual void SpawnProjectile(Vector2 origin, Vector2 dir)
        {
            shootPos = origin + dir * (WEAPONOFFSET + BULLETOFFSET);
            Projectile.NewProjectile(Terraria.Entity.InheritSource(Entity), shootPos, dir * ShootSpeed, BulletType, Projectile.damage, Projectile.knockBack);
        }

        /// <summary>
        /// Logic for shooting bullets
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="dir"></param>
        /// <param name="speed"></param>
        public virtual void ShootBullet(Vector2 origin, Vector2 dir, float speed)
        {
            //TODO: Bullet shooting Logic
            if(CanShoot(out int type))
            {
                SpawnProjectile(origin, dir);
                if(Sound != null)
                {
                    SoundEngine.PlaySound((SoundStyle)Sound, Projectile.Center);
                }
                if (Main.netMode != NetmodeID.Server)
                {
                    ShootDust(dir);
                    //int cartridge = Dust.NewDust(Projectile.Center, 1, 1, ModContent.DustType<M24CartdridgeDust>(), offset.X, offset.Y);
                }
                Timer = 0;
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

        public virtual float ShootingLerp(float k)
        {

            if (k == 0) return 0;
            if (k == 1) return 1;
            return (float)(Math.Pow(2f, -10f * k) * Math.Sin(10 * k - 0.75f) * 4f);
        }

        public virtual float ShootingLerpAlt(float k)
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

    }
}
