using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using System.Reflection;
using rail;
using TestContent.Utility;
using Terraria.Audio;
using ReLogic.Utilities;

namespace TestContent.NPCs.Minos.Projectiles.Friendly
{
    public class WhiplashProjectile : ModProjectile
    {
        private static Asset<Texture2D> chainTexture;

        public NPC? latchedTo
        {
            get
            {
                var npc = Main.npc[(int)Projectile.ai[1]];
                if (npc.active && numHits >= 1)
                {
                    return npc;
                }
                else
                {
                    return null;
                }
            }

            set => Projectile.ai[1] = value.whoAmI;
        }

        int numHits = 0;
        public bool travelTo = false;

        public SoundStyle hookHit = new SoundStyle(ModUtils.GetSoundFileLocation("hookPullStart"))
        {
            Volume = 0.4f,
            MaxInstances = 3,
            PlayOnlyIfFocused = true,
        };
        public SoundStyle hookThrow = new SoundStyle(ModUtils.GetSoundFileLocation("hookThrow"))
        {
            Volume = 0.4f,
            MaxInstances = 3,
            PlayOnlyIfFocused = true,
            IsLooped = true,
        };
        public SoundStyle hookPull = new SoundStyle(ModUtils.GetSoundFileLocation("hookPull"))
        {
            Volume = 0.4f,
            MaxInstances = 3,
            PlayOnlyIfFocused = true,
        };
        public SoundStyle hookCatch = new SoundStyle(ModUtils.GetSoundFileLocation("hookCatch"))
        {
            Volume = 0.4f,
            MaxInstances = 3,
            PlayOnlyIfFocused = true,
        };

        public SlotId thrown;
        public SlotId pull;

        public override void Load()
        {
            chainTexture = ModContent.Request<Texture2D>("TestContent/NPCs/Minos/Projectiles/Friendly/WhiplashRope");
        }

        public override bool? CanDamage()
        {
            return numHits < 1;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.GemHookAmethyst); // Copies the attributes of the Amethyst hook's projectile.
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            Player owner  = Main.player[Projectile.owner];

            if (Projectile.ai[0] == 1f || Projectile.ai[0] == 2f)
            {
                if (SoundEngine.TryGetActiveSound(thrown, out ActiveSound? result))
                {
                    result.Stop();
                }
                if (Projectile.active && !SoundEngine.TryGetActiveSound(pull, out ActiveSound? result2))
                {
                    pull = SoundEngine.PlaySound(hookPull, owner.Center);
                }
            }
            else
            {
                if (!SoundEngine.TryGetActiveSound(thrown, out ActiveSound? result))
                {
                    thrown = SoundEngine.PlaySound(hookThrow, owner.Center);
                }
            }
            if (latchedTo != null)
            {
                if (travelTo)
                {
                    Projectile.ai[0] = 2f;
                    Projectile.Center = latchedTo.Center;
                    owner.grapCount = 1;
                    owner.grappling[0] = Projectile.whoAmI;
                    owner.GetGrappleForces(owner.Center, out int? prefDir, out float velx, out float vely);
                    //Main.NewText($"{velx} {vely}");
                    owner.velocity = new Vector2(velx, vely);
                }
                else
                {
                    latchedTo.Center = Projectile.Center;
                }
                var dist = Vector2.Distance(Projectile.Center, Main.player[Projectile.owner].Center);
                if(dist < 50f && numHits >= 1)
                {
                    SoundEngine.PlaySound(hookCatch, owner.Center);
                    Projectile.ai[0] = 1f;
                    Projectile.Kill();
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            if(SoundEngine.TryGetActiveSound(thrown, out ActiveSound? result))
            {
                result.Stop();
            }
            if(SoundEngine.TryGetActiveSound(pull, out ActiveSound? result2))
            {
                result2.Stop();
            }
            Player owner = Main.player[Projectile.owner];
            if(latchedTo != null && travelTo)
            {
                owner.velocity = owner.velocity * 0.2f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //Go to target
            
            if(target.knockBackResist == 0f)
            {
                travelTo = true;
            }
            else //target goes to you
            {
                travelTo = false;
            }
            SoundEngine.PlaySound(hookHit, target.Center);
            latchedTo = target;
            Projectile.netUpdate = true;
            numHits++;

            if(Main.netMode == NetmodeID.MultiplayerClient && Main.myPlayer == Projectile.owner)
            {
                ModPacket packet = TestContent.Instance.GetPacket();
                packet.Write((byte)TestContent.NetMessageType.SyncWhiplashTarget);
                packet.Write((byte)Projectile.identity);
                packet.Write((byte)latchedTo.whoAmI);
                packet.Write(travelTo);
                packet.Send();
            }
            //Main.NewText("Hit them");
        }

        // Use this hook for hooks that can have multiple hooks mid-flight: Dual Hook, Web Slinger, Fish Hook, Static Hook, Lunar Hook.
        public override bool? CanUseGrapple(Player player)
        {
            int hooksOut = 0;
            foreach (var projectile in Main.ActiveProjectiles)
            {
                if (projectile.owner == Main.myPlayer && projectile.type == Projectile.type)
                {
                    hooksOut++;
                }
            }
            return hooksOut < 1;
        }

        // Use this to kill oldest hook. For hooks that kill the oldest when shot, not when the newest latches on: Like SkeletronHand
        // You can also change the projectile like: Dual Hook, Lunar Hook
        // public override void UseGrapple(Player player, ref int type) {
        //	int hooksOut = 0;
        //	int oldestHookIndex = -1;
        //	int oldestHookTimeLeft = 100000;
        //	foreach (var otherProjectile in Main.ActiveProjectiles) {
        //		if (otherProjectile.owner == player.whoAmI && otherProjectile.type == type) {
        //			hooksOut++;
        //			if (otherProjectile.timeLeft < oldestHookTimeLeft) {
        //				oldestHookIndex = otherProjectile.whoAmI;
        //				oldestHookTimeLeft = otherProjectile.timeLeft;
        //			}
        //		}
        //	}
        //	if (hooksOut > 1) {
        //		Main.projectile[oldestHookIndex].Kill();
        //	}
        // }

        // Amethyst Hook is 300, Static Hook is 600.
        public override float GrappleRange()
        {
            return 500f;
        }

        public override void NumGrappleHooks(Player player, ref int numHooks)
        {
            numHooks = 1; // The amount of hooks that can be shot out
        }

        // default is 11, Lunar is 24
        public override void GrappleRetreatSpeed(Player player, ref float speed)
        {
            speed = 18f; // How fast the grapple returns to you after meeting its max shoot distance
        }

        public override void GrapplePullSpeed(Player player, ref float speed)
        {
            speed = 20; // How fast you get pulled to the grappling hook projectile's landing position
        }

        // Adjusts the position that the player will be pulled towards. This will make them hang 50 pixels away from the tile being grappled.
        public override void GrappleTargetPoint(Player player, ref float grappleX, ref float grappleY)
        {
            Vector2 dirToPlayer = Projectile.DirectionTo(player.Center);
            float hangDist = 10f;
            grappleX += dirToPlayer.X * hangDist;
            grappleY += dirToPlayer.Y * hangDist;
        }

        // Can customize what tiles this hook can latch onto, or force/prevent latching altogether, like Squirrel Hook also latching to trees
        public override bool? GrappleCanLatchOnTo(Player player, int x, int y)
        {
            return false;
        }

        // Draws the grappling hook's chain.
        public override bool PreDrawExtras()
        {
            var player = Main.player[Projectile.owner];
            Vector2 playerCenter = Main.player[Projectile.owner].MountedCenter;
            Vector2 center = Projectile.Center;
            Vector2 directionToPlayer = playerCenter - Projectile.Center;
            float chainRotation = directionToPlayer.ToRotation();
            float distanceToPlayer = directionToPlayer.Length();
            Color drawColor = Lighting.GetColor((int)center.X / 16, (int)(center.Y / 16));
            while (distanceToPlayer > 20f && !float.IsNaN(distanceToPlayer))
            {
                directionToPlayer /= distanceToPlayer; // get unit vector
                directionToPlayer *= chainTexture.Height(); // multiply by chain link length

                center += directionToPlayer; // update draw position
                directionToPlayer = playerCenter - center; // update distance
                distanceToPlayer = directionToPlayer.Length();

               

                // Draw chain
                Main.EntitySpriteDraw(chainTexture.Value, center - Main.screenPosition,
                    chainTexture.Value.Bounds, drawColor, chainRotation,
                    chainTexture.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            }
            
            var arm = ModContent.Request<Texture2D>(ModUtils.GetNamespaceFileLocation(typeof(WhiplashProjectile))+"/WhiplashThrowArm").Value;
            var dirtoProj = playerCenter + (playerCenter.GetVectorPointingTo(center) * 20f);
            Main.EntitySpriteDraw(arm, dirtoProj - Main.screenPosition, null, drawColor, chainRotation, arm.Size() * 0.5f, 1f, player.direction == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None);
            // Stop vanilla from drawing the default chain.
            return false;
        }
    }
}
