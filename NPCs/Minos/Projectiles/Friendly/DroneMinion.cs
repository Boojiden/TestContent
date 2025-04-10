using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using TestContent.Buffs.Minions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TestContent.Players;
using Terraria.Audio;
using TestContent.NPCs.Minos.Buffs;
using System.Net.Mail;
using System.IO;
using TestContent.Utility;
using TestContent.NPCs.Minos.Projectiles.Friendly;

namespace TestContent.NPCs.Minos.Projectiles.Friendly
{
    public class DroneMinion : ModProjectile
    { 
        public bool firstTimeCheck = false;

        public float offsetDistance = 150f;
        public float offsetModifier = 1f;
        public float randomTargetAngle = 0f;

        public int Timer
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public int ShootTimer
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        public int shootTime = 120;

        public bool hasTarget = false;

        public int idleTickTime = 60;
        public int targetTickTime = 45;

        public Vector2 initPos;
        public Vector2 targetPos;

        public SoundStyle shoot = new SoundStyle(ModUtils.GetSoundFileLocation("shootImpact"))
        {
            Volume = 0.4f,
            MaxInstances = 3,
            Pitch = 0.75f,
            PitchVariance = 0.25f,
            PlayOnlyIfFocused = true,
        };
        public override void SetStaticDefaults()
        {
            // Sets the amount of frames this minion has on its spritesheet
            Main.projFrames[Projectile.type] = 1;
            // This is necessary for right-click targeting
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

            Main.projPet[Projectile.type] = true; // Denotes that this projectile is a pet or minion

            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Make the cultist resistant to this projectile, as it's resistant to all homing projectiles.

            //ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5; // The length of old position to be recorded
            //ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
        }

        public override bool? CanDamage()
        {
            return false;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        // This is mandatory if your minion deals contact damage (further related stuff in AI() in the Movement region)
        public override bool MinionContactDamage()
        {
            return true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(initPos);
            writer.WriteVector2(targetPos);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            initPos = reader.ReadVector2();
            targetPos = reader.ReadVector2();
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<DroneMinionBuff>());

                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<DroneMinionBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            return true;
        }

        public override void AI()
        {

            Player owner = Main.player[Projectile.owner];

            if(!firstTimeCheck)
            {
                firstTimeCheck = true;
                initPos = Projectile.Center;
                targetPos = Projectile.Center;
            }

            if (!CheckActive(owner))
            {
                return;
            }
            //UpdateDamage(owner);
            GeneralBehavior(owner, out Vector2 vectorToIdlePosition, out float distToIdle, out Vector2 idlePosition);
            SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
            Movement(foundTarget, distanceFromTarget, targetCenter, distToIdle, vectorToIdlePosition, idlePosition);
            Visuals(foundTarget, targetCenter);
        }

        /*
        private void UpdateDamage(Player owner)
        {
            var weapons = owner.GetModPlayer<PlayerWeapons>();
            int bonusDamage = 5;

            bonusDamage += weapons.GetCardValue(MinionType);

            Projectile.damage = (int)MathHelper.Lerp(Projectile.originalDamage, Projectile.originalDamage + bonusDamage, weapons.CardMinionCount / 21f);
        }
        */

        private void GeneralBehavior(Player owner, out Vector2 vectorToIdle, out float distToIdle, out Vector2 IdlePosition)
        {
            Vector2 origin = owner.Center;

            //origin = GetGatheringPoint(owner, origin, 60f);

            vectorToIdle = origin - Projectile.Center;
            distToIdle = vectorToIdle.Length();

            if (Main.myPlayer == owner.whoAmI && distToIdle > 2000f)
            {
                // Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
                // and then set netUpdate to true
                Projectile.Center = origin;
                initPos = origin;
                //Projectile.velocity *= 0.1f;
                Projectile.netUpdate = true;
            }

            IdlePosition = origin;

        }

        private Vector2 GetMinionTargetOffset(Vector2 target, float offset)
        {
            Vector2 pointOffset = new Vector2((float)Math.Sin(MathHelper.ToRadians(60 * Projectile.minionPos)), (float)Math.Cos(60 * Projectile.minionPos));

            return target + (pointOffset * offset);
        }

        private Vector2 GetGatheringPoint(Player owner, Vector2 origin, float radius)
        {
            var topPoint = origin;
            topPoint.Y -= radius;

            topPoint = GetMinionTargetOffset(topPoint, offsetDistance);
            
            return topPoint;
        }

        private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
        {
            // Starting search distance
            distanceFromTarget = 700f;
            targetCenter = Projectile.position;
            foundTarget = false;

            // This code is required if your minion weapon has the targeting feature
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC npc = Main.npc[owner.MinionAttackTargetNPC];
                float between = Vector2.Distance(npc.Center, Projectile.Center);

                // Reasonable distance away so it doesn't target across multiple screens
                if (between < 2000f)
                {
                    distanceFromTarget = between;
                    targetCenter = npc.Center;
                    foundTarget = true;
                }
            }

            if (!foundTarget)
            {
                // This code is required either way, used for finding a target
                foreach (var npc in Main.ActiveNPCs)
                {
                    if (npc.CanBeChasedBy())
                    {
                        float between = Vector2.Distance(npc.Center, Projectile.Center);
                        bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
                        bool inRange = between < distanceFromTarget;
                        bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
                        // Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
                        // The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
                        bool closeThroughWall = between < 100f;

                        if ((closest && inRange || !foundTarget) && (lineOfSight || closeThroughWall))
                        {
                            distanceFromTarget = between;
                            targetCenter = npc.Center;
                            foundTarget = true;
                        }
                    }
                }
            }

            // friendly needs to be set to true so the minion can deal contact damage
            // friendly needs to be set to false so it doesn't damage things like target dummies while idling
            // Both things depend on if it has a target or not, so it's just one assignment here
            // You don't need this assignment if your minion is shooting things instead of dealing contact damage
            Projectile.friendly = foundTarget;
        }

        private void Movement(bool foundTarget, float distanceFromTarget, Vector2 targetCenter, float distanceToIdlePosition, Vector2 vectorToIdlePosition, Vector2 IdlePosition)
        {
            // Default movement parameters (here for attacking)
            targetCenter = GetMinionTargetOffset(targetCenter, 5f);
            int droneDelay = Projectile.minionPos;
            Timer++;
            if (foundTarget)
            {
                ShootTimer++;
                if (ShootTimer >= shootTime + droneDelay)
                {
                    ShootTimer = 0;
                    var dir = Projectile.Center.GetVectorPointingTo(targetCenter);
                    SoundEngine.PlaySound(shoot, Projectile.Center);
                    if((Main.netMode != NetmodeID.MultiplayerClient))
                    {
                        for (int i = -1; i < 2; i++)
                        {
                            Projectile.NewProjectile(Terraria.Entity.InheritSource(Entity), Projectile.Center, dir.RotatedBy(MathHelper.Pi / 12 * i) * 20f, ModContent.ProjectileType<SkullBall>(), Projectile.damage, Projectile.knockBack);
                        }
                        Projectile.netUpdate = true;
                    }
                }
                if (Main.myPlayer == Projectile.owner)
                {
                    if (!hasTarget)
                    {
                        hasTarget = true;
                        Timer = 0;
                        initPos = Projectile.Center;
                        targetPos = targetCenter + (randomTargetAngle.ToRotationVector2() * offsetDistance * offsetModifier);
                        Projectile.netUpdate = true;
                    }
                    if (Timer > targetTickTime + droneDelay)
                    {
                        Timer = 0;
                        initPos = Projectile.Center;
                        targetPos = targetCenter + (randomTargetAngle.ToRotationVector2() * offsetDistance * offsetModifier);
                        Projectile.netUpdate = true;
                    }
                }
            }
            else
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    if (hasTarget)
                    {
                        hasTarget = false;
                        Timer = 0;
                        initPos = Projectile.Center;
                        targetPos = IdlePosition + (randomTargetAngle.ToRotationVector2() * offsetDistance * offsetModifier);
                        Projectile.netUpdate = true;
                    }
                    if (Timer > idleTickTime + droneDelay)
                    {
                        Timer = 0;
                        initPos = Projectile.Center;
                        targetPos = IdlePosition + (randomTargetAngle.ToRotationVector2() * offsetDistance * offsetModifier);
                        Projectile.netUpdate = true;
                    }
                }
            }

            if (Main.myPlayer == Projectile.owner && Projectile.netUpdate)
            {
                offsetModifier = 1f + 0.25f * ((Main.rand.NextFloat() * 2f) - 1f);
                randomTargetAngle = (float)Math.Tau * Main.rand.NextFloat();
            }
            //Main.NewText($"Drone Debug: {initPos} {targetPos} {Projectile.position} {GameplayUtils.GetTimeFromInts(Timer, hasTarget ? targetTickTime : idleTickTime)} {offsetModifier} {randomTargetAngle}");
            //Dust.QuickDust(targetPos, Color.White);
            Projectile.Center = Vector2.Lerp(initPos, targetPos, MathHelper.Hermite(0f, 3f, 1f, 0f, GameplayUtils.GetTimeFromInts(Timer, hasTarget ? targetTickTime + droneDelay : idleTickTime + droneDelay)));
        }

        private void Visuals(bool foundTarget, Vector2 targetCenter)
        {
            if(!foundTarget)
            {
                Projectile.spriteDirection = Main.player[Projectile.owner].direction;
                Projectile.rotation = 0f;
            }
            else
            {
                var dir = Projectile.Center.GetVectorPointingTo(targetCenter);
                Projectile.rotation = dir.ToRotation();
                Projectile.spriteDirection = Math.Sign(dir.X) == 0 ? 1 : Math.Sign(dir.X);

            }
            // Set Frame to be the correct card type
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if(hasTarget)
            {
                effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            }
            Main.EntitySpriteDraw(Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, Projectile.Hitbox.Size() / 2, Projectile.scale, effects);
            return false;
        }
    }
}
