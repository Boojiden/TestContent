using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using rail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace TestContent.Projectiles.Weapons.Gambling.Prehardmode
{
    public class JohnnySwordProjectile : ModProjectile
    {
        public SoundStyle slash, cardhit;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.AllowsContactDamageFromJellyfish[Type] = true;
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
            Main.projFrames[Type] = 16;
        }

        public float offset = 10f;
        public bool flip = false;
        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ownerHitCheck = true;
            Projectile.aiStyle = -1;
            Projectile.hide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
            Projectile.noEnchantmentVisuals = true;

            Projectile.stopsDealingDamageAfterPenetrateHits = true;

            slash = new SoundStyle("TestContent/Assets/Sounds/JSlash")
            {
                Volume = 0.3f,
                PitchVariance = 0.5f,
                MaxInstances = 2,
                PlayOnlyIfFocused = true
            };
            cardhit = new SoundStyle("TestContent/Assets/Sounds/JCardHit")
            {
                Volume = 0.3f,
                MaxInstances = 2,
                PlayOnlyIfFocused = true
            };
        }

        public override bool? CanDamage()
        {
            var frame = Projectile.frame;
            return frame == 2 || frame == 7 || frame == 10 || frame == 13;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Main.expertMode)
            {
                if (target.type >= NPCID.EaterofWorldsHead && target.type <= NPCID.EaterofWorldsTail)
                {
                    modifiers.FinalDamage /= 5;
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player player = Main.player[Projectile.owner];
            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter);
            var off = 200f;
            if (Projectile.frame == 2)
            {
                off = 110f;
            }
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitY);
            Vector2 length = dir * off;
            Vector2 end = playerCenter + length;

            //int dustType = DustID.Dirt;
            float nan = float.NaN;
            //middleCollision = 
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), playerCenter, end, 80f, ref nan);
            //Dust.NewDustPerfect(end, dustType);
            /*if (middleCollision)
            {
                return true;
            }

            var verticleOff = 45f;
            Vector2 offpoint1 = playerCenter + (dir.RotatedBy(Math.PI / 4) * verticleOff);
            Vector2 offpoint2 = playerCenter + (dir.RotatedBy(-Math.PI / 4) * verticleOff);
            //Dust.NewDustPerfect(offpoint1, dustType);
            //Dust.NewDustPerfect(offpoint2, dustType);
            Vector2 upperPoint1 = offpoint1 + length;
            Vector2 upperPoint2 = offpoint2 + length;
            //Dust.NewDustPerfect(upperPoint1, dustType);
            //Dust.NewDustPerfect(upperPoint2, dustType);
            bool check1 = Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), offpoint1, upperPoint1);
            bool check2 = Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), offpoint2, upperPoint2);
            return check1 || check2;
            //Main.NewText(projHitbox);*/

        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter);
            var dir = Vector2.Normalize(Main.MouseWorld - playerCenter);

            if (Projectile.frameCounter++ > 2)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
            }
            if (Main.myPlayer == Projectile.owner)
            {
                if (player.channel)
                {
                    float holdoutDistance = 10f * Projectile.scale;
                    Vector2 holdoutOffset = holdoutDistance * dir;
                    if (holdoutOffset.X != Projectile.velocity.X || holdoutOffset.Y != Projectile.velocity.Y)
                    {
                        Projectile.netUpdate = true;
                    }
                    Projectile.velocity = holdoutOffset;
                }
                else
                {
                    Projectile.Kill();
                }
            }

            Lighting.AddLight(playerCenter, Color.White.ToVector3() * 0.75f);

            if (Projectile.velocity.X > 0f)
            {
                player.ChangeDir(1);
            }
            else if (Projectile.velocity.X < 0f)
            {
                player.ChangeDir(-1);
            }

            //Projectile.spriteDirection = Projectile.direction;
            player.ChangeDir(Projectile.direction); // Change the player's direction based on the projectile's own
            player.heldProj = Projectile.whoAmI; // We tell the player that the drill is the held projectile, so it will draw in their hand
            player.SetDummyItemTime(2); // Make sure the player's item time does not change while the projectile is out
            Projectile.Center = playerCenter; // Centers the projectile on the player. Projectile.velocity will be added to this in later Terraria code causing the projectile to be held away from the player at a set distance.
            Projectile.rotation = Projectile.velocity.ToRotation(); //+MathHelper.PiOver2;
            player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();

            if ((bool)CanDamage() && Projectile.frameCounter == 0)
            {
                SoundEngine.PlaySound(slash, Projectile.Center);
                if (Main.netMode != NetmodeID.Server)
                {
                    var num = Main.rand.Next(5, 10);
                    for (int i = 0; i < num; i++)
                    {
                        var dustDir = Projectile.velocity.SafeNormalize(Vector2.UnitY);
                        //dir = dir.RotatedByRandom(Math.PI/4);
                        int dust = Dust.NewDust(playerCenter, 30, 30, DustID.Ash, dustDir.X * Main.rand.NextFloat(5f, 20f), dustDir.Y * Main.rand.NextFloat(5f, 20f));
                        Main.dust[dust].noGravity = true;
                    }
                }
            }

            //Check Collision for cards

            if ((bool)CanDamage())
            {
                int cardType = ModContent.ProjectileType<JohnnyCard>();
                if (player.ownedProjectileCounts[cardType] >= 1 && Main.myPlayer == Projectile.owner)
                {
                    //There HAS to be a better way of doing this
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile proj = Main.projectile[i];
                        if (proj.owner == player.whoAmI && proj.type == cardType)
                        {
                            bool colliding = (bool)Colliding(Projectile.Hitbox, proj.Hitbox);
                            if (colliding)
                            {
                                var endpoint = Vector2.Zero;
                                float maxDetectDistance = 400f;
                                NPC closestNPC = null;
                                float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;
                                foreach (var target in Main.ActiveNPCs)
                                {
                                    if (target.CanBeChasedBy())
                                    {
                                        float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);
                                        if (sqrDistanceToTarget < sqrMaxDetectDistance)
                                        {
                                            sqrMaxDetectDistance = sqrDistanceToTarget;
                                            closestNPC = target;
                                        }
                                    }
                                }
                                if (closestNPC != null)
                                {
                                    endpoint = closestNPC.Center;
                                }
                                else
                                {
                                    endpoint = proj.Center + dir * maxDetectDistance;
                                }
                                int who = Projectile.NewProjectile(Projectile.GetSource_FromAI(), proj.Center, Vector2.Zero, ModContent.ProjectileType<MistFiner>(), (int)(Projectile.damage * 1.5), Projectile.knockBack
                                    , player.whoAmI, endpoint.X, endpoint.Y); //endpoint.X, endpoint.Y);
                                SoundEngine.PlaySound(cardhit, Projectile.Center);

                                proj.Kill();
                                if (Main.netMode == NetmodeID.Server)
                                {
                                    NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, i);
                                }
                            }
                        }
                    }
                }
            }


        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int startY = frameHeight * Projectile.frame;
            Rectangle sourceRectangle = new Rectangle(1, startY, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2;

            Vector2 visualOffset = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 200f;

            origin.X = Projectile.spriteDirection == 1 ? sourceRectangle.Width - 30 : 30;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + visualOffset,
            sourceRectangle, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

    }
}
