using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Dusts;
using TestContent.Players;

namespace TestContent.Projectiles.Weapons
{
    public class M24HoldOut : ModProjectile
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

        private SoundStyle reload;
        public static SoundStyle outOfAmmo;
        private SoundStyle shoot;

        public bool stillInUse = false;
        public Vector2 mousePos;

        public override string Texture => "TestContent/Items/Weapons/m24";

        public int pullOutTime = 30;
        public int fireDelay = 6;

        public float shootSpeed = 20f;

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
            reload = new SoundStyle("TestContent/Assets/Sounds/weaponload")
            {
                Volume = 0.9f,
                PitchVariance = 0.2f,
                MaxInstances = 3,
            };
            outOfAmmo = new SoundStyle("TestContent/Assets/Sounds/outofammo")
            {
                Volume = 0.9f,
                PitchVariance = 0.2f,
                MaxInstances = 3,
            };
            shoot = new SoundStyle("TestContent/Assets/Sounds/m249")
            {
                Volume = 0.3f,
                PitchVariance = 0.2f,
                MaxInstances = 3,
            };

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
            SoundEngine.PlaySound(reload);
        }

        public override void AI()
        {
            //Main.NewText(stillInUse);
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
                        if (Timer >= fireDelay)
                        {
                            ShootBullet(rrp, aim, shootSpeed, wep);
                            Timer = 0;
                        }
                        break;

                    case GunStage.PutBack:

                        DoPutBackAnim(aim, WEAPONOFFSET);
                        break;
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
            aim = Vector2.Lerp(recoilAim, aim, ShootingLerp(Timer / fireDelay));

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
                Timer = fireDelay;
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
            if (wep.UseAltSignAmmo(1))
            {
                var offset = dir;
                if (Owner.direction > 0)
                {
                    offset = dir.RotatedBy(-Math.PI / 2);
                }
                else
                {
                    offset = dir.RotatedBy(Math.PI / 2);
                }

                offset *= 12f;
                origin += offset;
                Projectile.NewProjectile(Terraria.Entity.InheritSource(Entity), origin, dir * speed, ModContent.ProjectileType<M24Shot>(), Projectile.damage, Projectile.knockBack);
                SoundEngine.PlaySound(shoot);

                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 vel = dir * 5f;
                        int dust = Dust.NewDust(origin + Projectile.velocity * 4.8f, 0, 0, DustID.Torch, vel.X, vel.Y, Scale: Main.rand.NextFloat(0.5f, 1.2f));

                        vel *= Main.rand.NextFloat(0.5f, 6f);
                        int smoke = Dust.NewDust(origin + Projectile.velocity * 4.8f, 0, 0, DustID.Asphalt, vel.X, vel.Y, Scale: Main.rand.NextFloat(0.8f, 0.9f));
                        Main.dust[smoke].noGravity = true;

                    }
                    int cartridge = Dust.NewDust(Projectile.Center, 1, 1, ModContent.DustType<M24CartdridgeDust>(), offset.X, offset.Y);
                }
            }
            else
            {
                SoundEngine.PlaySound(outOfAmmo);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 sheetInsertPosition = Projectile.Center + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
            Main.spriteBatch.Draw(texture, sheetInsertPosition, default, lightColor, Projectile.rotation, new Vector2(texture.Width / 2f, texture.Height / 2f), Projectile.scale, effects, 0f);
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
