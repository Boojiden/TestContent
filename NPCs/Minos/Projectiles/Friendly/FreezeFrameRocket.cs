using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Players;
using TestContent.Projectiles;
using TestContent.Utility;

namespace TestContent.NPCs.Minos.Projectiles.Friendly
{
    public class FreezeFrameRocket : BasicTrailProjectile
    {
        protected override string trailTextureName => "";
        public Asset<Texture2D> FreezeRocketAura;

        public Vector2 prevVelocity;

        public float rot = 0f;

        public int empoweredTimer = 0;
        public int empoweredTimerMax = 60 * 3;

        public float controlDegreeChange = 5f;

        protected Player Owner => Main.player[Projectile.owner];

        protected PlayerWeapons weapons;

        public SoundStyle ExplosionSound => new SoundStyle(ModUtils.GetSoundFileLocation("RocketExplosion"))
        {
            Volume = 0.4f,
            PlayOnlyIfFocused = true,
            PitchVariance = 0.5f,
            MaxInstances = 3
        };
        public bool Frozen
        {
            get
            {
                return Projectile.ai[0] == 1f;
            }
            set
            {
                Projectile.ai[0] = value ? 1f : 0f;
                Projectile.netUpdate = true;
            }
        }

        public bool Buffed
        {
            get
            {
                return Projectile.ai[1] == 1f;
            }
            set
            {
                Projectile.ai[1] = value ? 1f : 0f;
                Projectile.netUpdate = true;
            }
        }

        public bool Controlled
        {
            get
            {
                return Projectile.ai[2] == 1f;
            }
            set
            {
                Projectile.ai[2] = value ? 1f : 0f;
                Projectile.netUpdate = true;
            }
        }

        public bool controlDebuff = false;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.tileCollide = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            FreezeRocketAura = ModContent.Request<Texture2D>("TestContent/NPCs/Minos/Extras/FreezeAura");
        }

        public override void OnSpawn(IEntitySource source)
        {
            prevVelocity = Projectile.velocity;
            weapons = Owner.GetModPlayer<PlayerWeapons>();
            if (weapons.rocketsFrozen)
            {
                Frozen = true;
            }
            Projectile.netUpdate = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(controlDebuff);
            writer.WriteVector2(prevVelocity);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            controlDebuff = reader.ReadBoolean();
            prevVelocity = reader.ReadVector2();
        }

        public override void AI()
        {
            Projectile.rotation = prevVelocity.ToRotation();

            Player Owner = Main.player[Projectile.owner];
            var ride = Owner.GetModPlayer<PlayerRideables>();
            if(Main.myPlayer == Projectile.owner && ride.ridingProjectileId == -1)
            {
                if(PlayerRideables.MountTrigger.Current && Collision.CheckAABBvAABBCollision(Projectile.position, Projectile.Hitbox.Size(), Owner.position, Owner.Hitbox.Size()))
                {
                    if (ride.TrySetRidingProjectile(Projectile))
                    {
                        Controlled = true;
                        ride.RideDebuffed += Ride_RideDebuffed;
                        ride.RideDismounted += Ride_RideDismounted;
                    }
                }
            }

            if(Controlled && Main.myPlayer == Projectile.owner)
            {
                int dir = PlayerInput.Triggers.Current.Left.ToInt() - PlayerInput.Triggers.Current.Right.ToInt();
                if(Math.Abs(dir) != 0)
                {
                    prevVelocity = prevVelocity.RotatedBy(MathHelper.ToRadians(controlDegreeChange) * dir);
                    Projectile.netUpdate = true;
                }
            }

            if(Frozen)
            {
                Projectile.velocity = Vector2.Zero;
                empoweredTimer++;
                if(empoweredTimer >= empoweredTimerMax)
                {
                    Buffed = true;
                }
            }
            else
            {
                if (controlDebuff)
                {
                    Projectile.velocity.Y += 0.48f;
                }
                else
                {
                    Projectile.velocity = prevVelocity;
                }
            }
        }

        private void Ride_RideDismounted()
        {
            controlDebuff = false;
            Controlled = false;
            Player Owner = Main.player[Projectile.owner];
            Owner.velocity.Y = -10f;
        }

        private void Ride_RideDebuffed()
        {
            controlDebuff = true;
            Projectile.netUpdate = true;
        }

        public override bool? CanDamage()
        {
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                if (Buffed)
                {
                    Projectile.NewProjectile(Projectile.InheritSource(Entity), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FreezeFrameRocketExplosionEmpowered>(), (int)(Projectile.damage * 1.5f), Projectile.knockBack);
                }
                else if (Projectile.numHits > 0)
                {
                    Projectile.NewProjectile(Projectile.InheritSource(Entity), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FreezeFrameRocketExplosion>(), Projectile.damage, Projectile.knockBack);
                }
                else
                {
                    Projectile.NewProjectile(Projectile.InheritSource(Entity), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FreezeFrameRocketExplosionNull>(), Projectile.damage / 2, Projectile.knockBack);
                }
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = TestContent.Instance.GetPacket();
                packet.Write((byte)TestContent.NetMessageType.SpawnRocketExplosion);
                packet.Write((byte)Projectile.identity);
                packet.Write((byte)Projectile.numHits);
                packet.Send();
            }
            if (Main.myPlayer == Projectile.owner)
            {
                var owner = Main.player[Main.myPlayer];
                var ridable = owner.GetModPlayer<PlayerRideables>();
                if(Projectile.identity == ridable.ridingProjectileId)
                {
                    ridable.UnsetRidingProjectile();
                }
            }
            SoundEngine.PlaySound(ExplosionSound, Projectile.Center);
        }

        public override float PrimitiveWidthFunction(float completionRatio)
        {
            return (1 - completionRatio) * Projectile.scale * 4f;
        }

        public override Color PrimitiveColorFunction(float completionRatio)
        {
            return Color.Lerp(Color.Transparent, Color.White, 1f - completionRatio);
        }

        public override Vector2 PrimitiveOffsetFunction(float completionRatio)
        {
            return new Vector2(15f, 15f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            TestContent.CenteredProjectileDraw(Projectile, Buffed ? Color.Lerp(lightColor, Color.Red, 0.5f) : lightColor, effects: Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            if(Frozen)
            {
                var texture = FreezeRocketAura.Value;
                var rect = texture.Bounds;
                var origin = rect.Size() / 2f;

                var rot = (float)Main.timeForVisualEffects * 0.01f;

                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, rect, Color.White, rot, origin, Projectile.scale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, rect, Color.White * 0.5f, -rot, origin, Projectile.scale * 1.5f, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}
