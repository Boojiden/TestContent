using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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

namespace TestContent.Projectiles.Weapons.Gambling.Prehardmode
{
    public class LuckyShotProjectile : ModProjectile
    {

        public int Timer
        {
            get => (int)Projectile.ai[0];
            set { Projectile.ai[0] = value; }
        }

        public bool SuicideAttempt
        {
            get => Projectile.ai[1] == 1f;
        }
        private Player Owner => Main.player[Projectile.owner];
        public Vector2 mousePos;
        private Item item;

        public int shootDelay = 30;
        public int suicideDelay = 60;
        public bool rolledSuicideAttempt = false;
        public bool justShotLaser = false;

        public SoundStyle charge, bigFire;
        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 32;
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



        public override void AI()
        {
            Vector2 rrp = Owner.RotatedRelativePoint(Owner.MountedCenter, addGfxOffY: true);
            UpdatePlayerVisuals(rrp);
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
            if (SuicideAttempt)
            {
                offset = 30f;
            }
            UpdateAim(aim, offset);
            if (Projectile.ai[1] == 2f)
            {
                DoChargingDust(rrp, aim);
            }
            Timer++;
            if (Main.myPlayer == Projectile.owner && !SuicideAttempt)
            {
                bool stillInUse = !Owner.noItems && !Owner.CCed && (Owner.channel || SuicideAttempt);
                if (!stillInUse)
                {
                    Projectile.Kill();
                }
                else
                {
                    if (!SuicideAttempt)
                    {
                        if (Timer >= shootDelay)
                        {
                            if (!ShootBullet(rrp, aim, 20f) && Projectile.ai[1] != 2f)
                            {
                                Projectile.Kill();
                            }
                            Timer = 0;
                        }
                    }
                }
            }
            else if (SuicideAttempt)
            {
                //Main.NewText($"{Timer} {suicideDelay} {rolledSuicideAttempt}");
                if (Timer >= suicideDelay)
                {
                    if (rolledSuicideAttempt)
                    {
                        if (!Projectile.hide)
                        {
                            if (!Main.dedServ)
                            {
                                EmboldDust();
                            }
                            Owner.AddBuff(ModContent.BuffType<Emboldened>(), 1800, false);
                            SoundEngine.PlaySound(SoundID.Item119);
                        }
                        Projectile.Kill();
                    }
                    else
                    {
                        RollSuicideAttempt();
                        Timer = 0;
                    }
                }
            }
            Projectile.timeLeft = 2;
        }

        private void DoChargingDust(Vector2 origin, Vector2 dir)
        {
            float percent = Timer / (float)shootDelay;
            var pos = GetShootingPosition(origin, dir);

            int dust = Dust.NewDust(pos, 0, 0, DustID.Firework_Yellow, Scale: percent);
            Main.dust[dust].noGravity = true;
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
            }

            offset *= 12f;
            origin += offset;

            if (Projectile.ai[1] == 2f)
            {
                origin += dir * 42f;
            }

            return origin;
        }

        public void EmboldDust()
        {
            for (int i = 0; i < 15; i++)
            {
                var rand = Main.rand.NextFloat(10f, 12f);
                var circRand = Main.rand.NextVector2Circular(rand, rand);
                int dust = Dust.NewDust(Owner.Center, 2, 2, DustID.GemRuby, circRand.X, circRand.Y);
                Main.dust[dust].noGravity = true;
            }
        }

        public void RollSuicideAttempt()
        {
            rolledSuicideAttempt = true;
            if (Projectile.ai[2] == 2)
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    //Main.NewText("INNER");
                    SoundEngine.PlaySound(CarRare.Kaboom, Projectile.Center);
                    int dust = Dust.NewDust(Owner.position, 0, 0, ModContent.DustType<Explosion>());
                }
                Owner.Hurt(PlayerDeathReason.ByCustomReason($"{Owner.name} rolled the bones"), Owner.statLifeMax2 / 2, 1);
                //Main.NewText("OUTER");
                Projectile.hide = true;
            }
            else
            {
                SoundEngine.PlaySound(M24HoldOut.outOfAmmo);
            }
        }

        private bool ShootBullet(Vector2 origin, Vector2 dir, float shootSpeed)
        {
            if (Owner.HasBuff<Emboldened>() && Main.rand.Next(0, 6) == 0 && Projectile.ai[1] != 2f)
            {
                Projectile.ai[1] = 2f;
                SoundEngine.PlaySound(charge);
                return false;
            }
            if (!Owner.HasAmmo(item))
            {
                return false;
            }
            bool canShoot = Owner.PickAmmo(item, out int projID, out float speed, out int damage, out float knockBack, out int usedAmmoItemId);
            if (!canShoot)
            {
                return false;
            }
            var pos = GetShootingPosition(origin, dir);
            origin = pos;
            if (Projectile.ai[1] != 2f)
            {
                Projectile.NewProjectile(Terraria.Entity.InheritSource(Entity), origin, dir * speed, projID, damage, knockBack);
                SoundEngine.PlaySound(SmokingGun.shootSniper);
                justShotLaser = false;
            }
            else
            {
                Projectile.NewProjectile(Terraria.Entity.InheritSource(Entity), origin, Vector2.Zero, ModContent.ProjectileType<LuckyShotBigShot>(), damage, knockBack
                    , ai0: Projectile.GetByUUID(Projectile.owner, Projectile.whoAmI));
                SoundEngine.PlaySound(bigFire);
                justShotLaser = true;
                Projectile.ai[1] = 0;
            }

            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector2 vel = dir * 5f;
                    int dust = Dust.NewDust(origin + Projectile.velocity * 1.7f, 0, 0, DustID.Torch, vel.X, vel.Y, Scale: Main.rand.NextFloat(0.5f, 1.2f));

                    vel *= Main.rand.NextFloat(0.5f, 6f);
                    int smoke = Dust.NewDust(origin + Projectile.velocity * 1.7f, 0, 0, DustID.Asphalt, vel.X, vel.Y, Scale: Main.rand.NextFloat(0.8f, 0.9f));
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

            float range = 4f;
            if (justShotLaser)
            {
                range = 15f;
            }
            Vector2 recoilAim;
            if (Projectile.spriteDirection == 1)
            {
                recoilAim = aim.RotatedBy(MathHelper.ToRadians(-range));
            }
            else
            {
                recoilAim = aim.RotatedBy(MathHelper.ToRadians(range));
            }
            if (Projectile.ai[1] != 2 && Main.myPlayer == Projectile.owner)
            {
                aim = Vector2.Lerp(recoilAim, aim, M24HoldOut.ShootingLerp(Timer / (float)shootDelay));
            }

            Projectile.velocity = aim;
        }
        private void UpdatePlayerVisuals(Vector2 playerHandPos)
        {
            Projectile.Center = playerHandPos;
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (SuicideAttempt)
            {
                Projectile.rotation += (float)Math.PI;
            }
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

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.hide)
            {
                return false;
            }
            SpriteEffects effects;
            if (!SuicideAttempt)
            {
                effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            }
            else
            {
                effects = Projectile.spriteDirection == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            }
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 sheetInsertPosition = Projectile.Center + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
            Color col = lightColor;

            if (Projectile.ai[1] == 2f)
            {
                float percent = Timer / (float)shootDelay;
                col = Color.Lerp(lightColor, Color.Yellow, percent);
            }
            Main.spriteBatch.Draw(texture, sheetInsertPosition, default, col, Projectile.rotation, new Vector2(texture.Width / 2f, texture.Height / 2f), Projectile.scale, effects, 0f);
            return false;
        }
    }
}
