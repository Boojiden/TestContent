using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics.Metrics;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Buffs;
using TestContent.Dusts;
using TestContent.Items.Pets;
using TestContent.Items.Weapons;

namespace TestContent.Projectiles.Weapons.Gambling.Hardmode
{
    public class MidasHeldProjectile : ModProjectile
    {
        public static int cost = 2;
        public int Timer
        {
            get => (int)Projectile.ai[0];
            set { Projectile.ai[0] = value; }
        }

        private Player Owner => Main.player[Projectile.owner];
        public Vector2 mousePos;
        private Item item;

        public int shootDelay = 8;

        public SoundStyle charge, bigFire;
        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 32;
            Projectile.scale = 0.7f;
            Projectile.tileCollide = false;

            item = ModContent.GetInstance<LuckyShot>().Entity;

            charge = new SoundStyle("TestContent/Assets/Sounds/PowerShotCharge")
            {
                Volume = 0.6f,
                PitchVariance = 0.3f,
                PlayOnlyIfFocused = true
            };
            bigFire = new SoundStyle("TestContent/Assets/Sounds/PowerShoot")
            {
                Volume = 0.6f,
                PitchVariance = 0.3f,
                PlayOnlyIfFocused = true
            };
        }

        public override void OnSpawn(IEntitySource source)
        {
            Timer = shootDelay;
        }



        public override void AI()
        {
            Vector2 rrp = Owner.RotatedRelativePoint(Owner.MountedCenter, addGfxOffY: true) + new Vector2(0, -4);

            Vector2 aim;
            if (Owner.whoAmI == Main.myPlayer)
            {
                aim = Vector2.Normalize(Main.MouseWorld - rrp);
            }
            else
            {
                aim = Projectile.velocity;
            }
            if (aim.HasNaNs())
            {
                aim = -Vector2.UnitY;
            }
            float offset = 20f;
            UpdatePlayerVisuals(rrp, aim);
            UpdateAim(aim, offset);
            Timer++;
            if (Main.myPlayer == Projectile.owner)
            {
                bool stillInUse = !Owner.noItems && !Owner.CCed && Owner.channel;
                if (!stillInUse)
                {
                    Projectile.Kill();
                }
                else
                {
                    if (Timer >= shootDelay)
                    {
                        if (!ShootBullet(rrp, aim, 20f))
                        {
                            Projectile.Kill();
                        }
                        Timer = 0;
                    }
                }
            }
            Projectile.timeLeft = 2;
        }

        public Vector2 GetShootingPosition(Vector2 origin, Vector2 dir)
        {
            var offset = dir;
            if (Owner.direction > 0)
            {
                offset = dir.RotatedBy(-Math.PI / 2);
            }
            else
            {
                offset = dir.RotatedBy(Math.PI / 2);
                offset *= 6f;
                origin += offset;
            }

            if (Projectile.ai[1] == 2f)
            {
                origin += dir * 42f;
            }

            return origin;
        }

        private bool ShootBullet(Vector2 origin, Vector2 dir, float shootSpeed)
        {
            bool canShoot = Owner.CheckMana(cost, true);
            if (!canShoot)
            {
                return false;
            }
            var pos = GetShootingPosition(origin, dir);
            origin = pos;
            Projectile.NewProjectile(Terraria.Entity.InheritSource(Entity), origin, dir * 10, ModContent.ProjectileType<MidasLaser>(), Projectile.damage, Projectile.knockBack);
            SoundEngine.PlaySound(SoundID.Item65);


            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 vel = dir * 2f;
                    int dust = Dust.NewDust(origin + Projectile.velocity * 1.1f, 0, 0, DustID.GoldCoin, vel.X, vel.Y, Scale: Main.rand.NextFloat(0.5f, 1.2f));

                    vel *= Main.rand.NextFloat(0.5f, 6f);
                    int smoke = Dust.NewDust(origin + Projectile.velocity * 1.1f, 0, 0, DustID.GoldFlame, vel.X, vel.Y, Scale: Main.rand.NextFloat(0.8f, 0.9f));
                    Main.dust[smoke].noGravity = true;

                }
                //int cartridge = Dust.NewDust(Projectile.Center, 1, 1, ModContent.DustType<M24CartdridgeDust>(), offset.X, offset.Y);
            }
            Projectile.netUpdate = true;
            return true;
        }

        private void UpdateAim(Vector2 aim, float speed)
        {
            // Get the player's current aiming direction as a normalized vector.
            float AimResponsiveness = 1f;

            // Change a portion of the Prism's current velocity so that it points to the mouse. This gives smooth movement over time.
            aim = Vector2.Normalize(Vector2.Lerp(Projectile.velocity, aim, AimResponsiveness));
            aim *= speed;

            float range = 16f;
            Vector2 recoilAim;
            Vector2 newAim;
            if (Projectile.spriteDirection == 1)
            {
                recoilAim = aim.RotatedBy(MathHelper.ToRadians(-range));
            }
            else
            {
                recoilAim = aim.RotatedBy(MathHelper.ToRadians(range));
            }
            newAim = Vector2.Lerp(recoilAim, aim, ShootingLerp(Timer / (float)shootDelay));
            Projectile.velocity = Vector2.Lerp(aim, newAim, 0.5f);
            Projectile.rotation = newAim.ToRotation();
        }
        private void UpdatePlayerVisuals(Vector2 playerHandPos, Vector2 direction)
        {
            var playerDir = direction.X > 0 ? 1 : -1;
            Projectile.Center = playerHandPos;
            //Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.spriteDirection = playerDir;
            // The Prism is a holdout Projectile, so change the player's variables to reflect that.
            // Constantly resetting player.itemTime and player.itemAnimation prevents the player from switching items or doing anything else.
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, direction.ToRotation() - (float)Math.PI / 2);
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, 0);

            Owner.ChangeDir(playerDir);
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            // If you do not multiply by Projectile.direction, the player's hand will point the wrong direction while facing left.
            Owner.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.hide)
            {
                return false;
            }
            SpriteEffects effects;
            effects = Projectile.spriteDirection == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            float rotOffset = Projectile.spriteDirection == 1 ? (float)Math.PI / 2 : -(float)Math.PI / 2;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            Vector2 center = Projectile.Center;

            Vector2 sheetInsertPosition = center + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
            Color col = lightColor;

            if (Projectile.ai[1] == 2f)
            {
                float percent = Timer / (float)shootDelay;
                col = Color.Lerp(lightColor, Color.Yellow, percent);
            }
            Main.spriteBatch.Draw(texture, sheetInsertPosition, default, col, Projectile.rotation - rotOffset, new Vector2(texture.Width / 2f, texture.Height / 2f), Projectile.scale, effects, 0f);
            return false;
        }
        public static float ShootingLerp(float k)
        {

            //if (k == 0) return 0;
            //if (k == 1) return 1;
            return (float)(Math.Pow(2f, -4f * k) * Math.Sin(4 * k - 0.75f) * 4f);
        }
    }
}
