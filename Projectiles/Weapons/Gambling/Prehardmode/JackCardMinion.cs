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

namespace TestContent.Projectiles.Weapons.Gambling.Prehardmode
{
    public class JackCardMinion : ModProjectile
    {

        public int MinionType
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        public int attackTimer
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public int state
        {
            get => (int)Projectile.ai[2];
            set => Projectile.ai[2] = value;
        }

        public Vector2 dashVector;

        public int attackReset = 30;

        public int damageDone;
        public int damageLimit = 300;

        public bool firstTimeCheck = false;
        public override void SetStaticDefaults()
        {
            // Sets the amount of frames this minion has on its spritesheet
            Main.projFrames[Projectile.type] = 4;
            // This is necessary for right-click targeting
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

            Main.projPet[Projectile.type] = true; // Denotes that this projectile is a pet or minion

            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Make the cultist resistant to this projectile, as it's resistant to all homing projectiles.

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5; // The length of old position to be recorded
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
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
            Projectile.minionSlots = 0f;
            Projectile.penetrate = -1;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            this.damageDone += damageDone;
            if (this.damageDone >= damageLimit)
            {
                Player owner = Main.player[Projectile.owner];
                var weapons = owner.GetModPlayer<PlayerWeapons>();
                Projectile.Kill();
                if (!Main.dedServ)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        var rand = Main.rand.NextFloat(5f, 6f);
                        var circRand = Main.rand.NextVector2CircularEdge(rand, rand);
                        int dust = Dust.NewDust(Projectile.Center, 2, 2, 312, circRand.X, circRand.Y);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].scale = 2f;
                    }
                }
                SoundEngine.PlaySound(SoundID.Item43, Projectile.Center);
                weapons.UpdateCardMinionCounts();
            }
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
                owner.ClearBuff(ModContent.BuffType<JacksMinionBuff>());

                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<JacksMinionBuff>()))
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
            UpdateDamage(owner);
            GeneralBehavior(owner, out Vector2 vectorToIdlePosition, out float distToIdle, out Vector2 idlePosition);
            SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
            Movement(foundTarget, distanceFromTarget, targetCenter, distToIdle, vectorToIdlePosition, idlePosition);
            Visuals();
        }

        private void UpdateDamage(Player owner)
        {
            var weapons = owner.GetModPlayer<PlayerWeapons>();
            int bonusDamage = 5;

            bonusDamage += weapons.GetCardValue(MinionType);

            Projectile.damage = (int)MathHelper.Lerp(Projectile.originalDamage, Projectile.originalDamage + bonusDamage, weapons.CardMinionCount / 21f);
        }

        private void GeneralBehavior(Player owner, out Vector2 vectorToIdle, out float distToIdle, out Vector2 IdlePosition)
        {
            Vector2 origin = owner.Center;

            origin = GetCirclePositionForMinion(owner, origin, 60f);

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

        private Vector2 GetCirclePositionForMinion(Player owner, Vector2 origin, float radius)
        {
            var topPoint = origin;
            topPoint.Y += radius;

            var dir = (origin - topPoint).SafeNormalize(Vector2.UnitY);

            float degrees = 0;
            int posIndex = 0;
            posIndex = Projectile.minionPos;
            posIndex -= Projectile.minionPos / 2;
            if (Projectile.minionPos % 2 == 0)
            {
                posIndex *= -1;
            }

            if (owner.ownedProjectileCounts[Projectile.type] % 2 == 0)
            {
                degrees = -5f;
            }

            //Main.NewText(posIndex);
            float rotation = MathHelper.ToRadians(15f * posIndex + degrees);

            dir = dir.RotatedBy(rotation);

            var pos = origin + dir * radius;
            if (state == 0)
            {
                Projectile.rotation = dir.RotatedBy(Math.PI / 2).ToRotation();
            }
            return pos;
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
            float speed = 8f;
            float inertia = 20f;

            float dashSpeed = 15f;

            if (foundTarget && state != 1)
            {
                // Minion has a target: attack (here, fly towards the enemy)
                /*if (distanceFromTarget > 40f)
                {
                    // The immediate range around the target (so it doesn't latch onto it when close)
                    Vector2 direction = targetCenter - Projectile.Center;
                    direction.Normalize();
                    direction *= speed;

                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
                }*/
                state = 1;
                Vector2 direction = targetCenter - Projectile.Center;
                direction.Normalize();

                var maxVelocity = direction * dashSpeed;

                dashVector = maxVelocity;
                Projectile.velocity = dashVector;
            }
            else if (foundTarget)
            {
                attackTimer++;
                Projectile.velocity = Vector2.Lerp(dashVector, dashVector * 0.2f, attackTimer / (float)attackReset);
                if (attackTimer >= attackReset)
                {
                    attackTimer = 0;
                    state = 0;
                }
            }
            else
            {
                speed = 12f;
                inertia = 10f;
                // Minion doesn't have a target: return to player and idle
                /*if (distanceToIdlePosition > 600f)
                {
                  
                    
                }
                else
                {
      
                    speed = 4f;
                    inertia = 80f;
                }*/

                if (distanceToIdlePosition > 10f)
                {
                    // The immediate range around the player (when it passively floats about)

                    // This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
                    vectorToIdlePosition.Normalize();
                    vectorToIdlePosition *= speed;
                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
                    state = 2;
                }
                else
                {
                    Projectile.velocity = Vector2.Zero;
                    Projectile.Center = IdlePosition;
                    state = 0;
                }
            }
        }

        private void Visuals()
        {
            // So it will lean slightly towards the direction it's moving
            if (state != 0)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2;
            }
            // Set Frame to be the correct card type
            Projectile.frame = MinionType % 4;

            // Some visuals here
            Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.78f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            var rect = new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Type], texture.Width, texture.Height / Main.projFrames[Type]);
            Vector2 origin = rect.Size() / 2;

            Color col = Color.DarkBlue;
            col.A = 55;

            if (state == 1)
            {
                int iterations = 0;
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] != Vector2.Zero)
                    {
                        iterations++;
                    }
                }

                for (int i = 0; i < iterations; i++)
                {
                    col = Color.DarkGray;
                    col *= 0.25f - 0.05f * i;
                    Main.spriteBatch.Draw(texture, Projectile.oldPos[i] - Main.screenPosition + origin, rect, col, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
                }
            }
            return true;
        }
    }
}
