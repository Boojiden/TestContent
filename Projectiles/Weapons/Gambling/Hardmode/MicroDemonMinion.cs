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

namespace TestContent.Projectiles.Weapons.Gambling.Hardmode
{
    public class MicroDemonMinion : ModProjectile
    { 
        public bool firstTimeCheck = false;

        public float offsetDistance = 25f;
        public override void SetStaticDefaults()
        {
            // Sets the amount of frames this minion has on its spritesheet
            Main.projFrames[Projectile.type] = 3;
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
            Projectile.width = 18;
            Projectile.height = 26;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

        }

        // This is mandatory if your minion deals contact damage (further related stuff in AI() in the Movement region)
        public override bool MinionContactDamage()
        {
            return true;
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<MicroDemonMinionBuff>());

                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<MicroDemonMinionBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            return true;
        }

        public override void AI()
        {

            Player owner = Main.player[Projectile.owner];

            if (!CheckActive(owner))
            {
                return;
            }
            //UpdateDamage(owner);
            GeneralBehavior(owner, out Vector2 vectorToIdlePosition, out float distToIdle, out Vector2 idlePosition);
            SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
            Movement(foundTarget, distanceFromTarget, targetCenter, distToIdle, vectorToIdlePosition, idlePosition);
            Visuals();
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

            origin = GetGatheringPoint(owner, origin, 60f);

            vectorToIdle = origin - Projectile.Center;
            distToIdle = vectorToIdle.Length();

            if (Main.myPlayer == owner.whoAmI && distToIdle > 2000f)
            {
                // Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
                // and then set netUpdate to true
                Projectile.position = origin;
                Projectile.velocity *= 0.1f;
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
            float speed = 15f;
            float inertia = 30f;
            targetCenter = GetMinionTargetOffset(targetCenter, 5f);
            if (foundTarget)
            {
                Vector2 direction = targetCenter - Projectile.Center;
                direction.Normalize();

                var maxVelocity = direction * speed;

                Projectile.velocity = (Projectile.velocity * (inertia - 1) + maxVelocity) / inertia;
            }
            else
            {
                speed = 8f;
                inertia = 10f;

                if (distanceToIdlePosition > 90f)
                {
                    // The immediate range around the player (when it passively floats about)

                    // This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
                    vectorToIdlePosition.Normalize();
                    vectorToIdlePosition *= speed;
                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
                }
            }
        }

        private void Visuals()
        {
            // So it will lean slightly towards the direction it's moving
            Projectile.rotation = Projectile.velocity.ToRotation() * 0.05f;
            // Set Frame to be the correct card type
            if(Projectile.frameCounter++ > 2)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects effects;
            if(Projectile.spriteDirection <= -1) 
            {
                effects = SpriteEffects.FlipHorizontally;
            }
            else
            {
                effects = SpriteEffects.None;
            }
            TestContent.AnimatedProjectileDraw(Projectile, lightColor, effects:effects);
            return false;
        }
    }
}
