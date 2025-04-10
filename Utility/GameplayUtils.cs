using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace TestContent.Utility
{
    public static class GameplayUtils
    {
        public static bool IsInAir(this NPC NPC)
        {
            Point leftmost = new Point((int)(NPC.position.X / 16f), (int)(NPC.position.Y / 16f)) + new Point(0, (NPC.height / 16) + 1);
            int end = NPC.width / 16;
            bool col = false;
            for (int i = 0; i < end; i++)
            {
                Tile offendingTile = Main.tile[leftmost + new Point(i, 0)];
                if (offendingTile.HasTile && (Main.tileSolid[offendingTile.TileType] || Main.tileSolidTop[offendingTile.TileType])){
                    col = true;
                    break;
                }
            }

            if (col && NPC.velocity.Y == 0)
            {
                return false;
            }

            return true;
        }

        public static void DrawDebugGroundCollision(this NPC NPC, SpriteBatch batch)
        {
            Point leftmost = new Point((int)(NPC.position.X / 16f), (int)(NPC.position.Y / 16f)) + new Point(0, (NPC.height / 16) + 1);
            int end = NPC.width / 16;

            Color col = Color.Green;
            if (NPC.IsInAir())
            {
                col = Color.Red;
            }
            for (int i = 0; i < end; i++)
            {
                Vector2 pos = (((leftmost).ToVector2() + new Vector2(i, 0)) * 16f) - Main.screenPosition;
                batch.Draw(TextureAssets.MagicPixel.Value, pos, col);
            }
        }

        public static bool IsInGround(this NPC NPC)
        {
            Point leftmost = new Point((int)(NPC.position.X / 16f), (int)(NPC.position.Y / 16f)) + new Point(0, (NPC.height / 16));
            int end = NPC.width / 16;
            bool col = false;
            for (int i = 0; i < end; i++)
            {
                Tile offendingTile = Main.tile[leftmost + new Point(i, 0)];
                if (offendingTile.HasTile && (Main.tileSolid[offendingTile.TileType] || Main.tileSolidTop[offendingTile.TileType]))
                {
                    col = true;
                    break;
                }
            }

            return col && !NPC.IsInAir();
        }

        public static Vector2 GetFeetPosition(this NPC NPC)
        {
            return new Vector2(NPC.position.X + (float)(NPC.width / 2), NPC.position.Y + (float)(NPC.height));
        }
        public static void SetFeetPosition(this NPC NPC, Vector2 position)
        {
            NPC.position = NPC.ConvertToBottomPosition(position);
        }

        public static void SetFeetPosition(this Player player, Vector2 position)
        {
            player.position = position + new Vector2(-(float)player.width / 2f, -(float)player.height);
        }

        /// <summary>
        /// Converts a world space vector to the same vector adjusted for the bottom of an npc
        /// </summary>
        /// <param name="NPC"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector2 ConvertToBottomPosition(this NPC NPC, Vector2 position)
        {
            return new Vector2(position.X - (float)(NPC.width / 2), position.Y - (float)(NPC.height));
        }

        public static Vector2 ConvertCenterToBottomPosition(this NPC NPC, Vector2 center)
        {
            return center - new Vector2(0, (-NPC.height / 2) - 12);
        }
        /// <summary>
        /// Check if world-position target Vector lies inside of a cone. Cone is oriented according to coneDir with the point starting at the NPC.
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="target"> Target Vector to check (i.e. player Center) </param>
        /// <param name="coneDir"> Normalized Vector representing direction of the cone</param>
        /// <param name="coneAngle"> Angle (in radians) of the cone</param>
        /// <param name="maxDist">Maximum distance that the cone stretches</param>
        /// <returns></returns>
        public static bool ConeCheck(this NPC npc, Vector2 target, Vector2 coneDir, float coneAngle, float maxDist, Vector2? posOverride = null)
        {
            var npcPos = npc.Center;

            if (posOverride != null)
            {
                npcPos = (Vector2)posOverride;
            }

            if((target - npcPos).Length() > maxDist)
            {
                return false;
            }

            float angle = npcPos.GetAngleTo(target);

            float diff = Math.Abs(GameplayUtils.GetAngleDifference(angle, coneDir.ToRotation()));

            if(diff > coneAngle)
            {
                return false;
            }

            return true;
        }

        public static void MoveToWithInertia(this NPC NPC, Vector2 target, float speed, float inertia)
        {
            var maxVelocity = NPC.Center.GetVectorPointingTo(target) * speed;

            NPC.velocity = (NPC.velocity * (inertia - 1) + maxVelocity) / inertia;
        }
        /// <summary>
        /// Get the angle (in radians) to another vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static float GetAngleTo(this Vector2 vector, Vector2 other)
        {
            return (other - vector).ToRotation();
        }
        /// <summary>
        /// Get a normalized vector pointing to the other vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Vector2 GetVectorPointingTo(this Vector2 vector, Vector2 other)
        {
            var final = other - vector;
            final = final.SafeNormalize(Vector2.Zero);
            return final;
        }
        /// <summary>
        /// Get the difference between angles. Always returns the smallest absolute value (i.e. if an angle can be described on a unit circle as 90 degrees or -270 degrees, this will return 90 degrees in radians)
        /// </summary>
        /// <param name="rot1"></param>
        /// <param name="rot2"></param>
        /// <returns></returns>
        public static float GetAngleDifference(float rot1, float rot2)
        {
            float angleChange = rot1 - rot2;

            if (Math.Abs(angleChange) > MathHelper.ToRadians(180f))
            {
                angleChange += angleChange < 0 ? MathHelper.ToRadians(360f) : -MathHelper.ToRadians(360f);
            }

            return angleChange;
        }

        /// <summary>
        /// Determines the angular distance between two vectors based on dot product comparisons. This method ensures underlying normalization is performed safely.
        /// </summary>
        /// <param name="v1">The first vector.</param>
        /// <param name="v2">The second vector.</param>
        public static float AngleBetween(this Vector2 v1, Vector2 v2) => (float)Math.Acos(Vector2.Dot(v1.SafeNormalize(Vector2.Zero), v2.SafeNormalize(Vector2.Zero)));

        public static float AngleBetween(this float angle, float otherAngle) => ((otherAngle - angle) + MathHelper.Pi).Modulo(MathHelper.TwoPi) - MathHelper.Pi;

        public static float Modulo(this float dividend, float divisor)
        {
            return dividend - (float)Math.Floor(dividend / divisor) * divisor;
        }


            /// <summary>
            /// Get the facing direction of the npc to face a world space position
            /// </summary>
            /// <param name="NPC"></param>
            /// <param name="other"></param>
            /// <returns></returns>
            public static int GetFacingDirection(this NPC NPC, Vector2 position)
        {
            int dir = Math.Sign(NPC.Center.GetVectorPointingTo(position).X);
            if (dir == 0)
            {
                dir = 1;
            }

            return dir;
        }

        public static bool IsInAir(this Player player)
        {
            Point leftmost = new Point((int)(player.position.X / 16f), (int)(player.position.Y / 16f)) + new Point(0, (player.height / 16) + 1);
            int end = player.width / 16;
            bool col = false;
            for (int i = 0; i < end; i++)
            {
                Tile offendingTile = Main.tile[leftmost + new Point(i, 0)];
                if (offendingTile.HasTile && (Main.tileSolid[offendingTile.TileType] || Main.tileSolidTop[offendingTile.TileType]))
                {
                    col = true;
                    break;
                }
            }

            if (col && player.velocity.Y == 0)
            {
                return false;
            }

            return true;
        }

        public static float GetTimeFromInts(int current, int max)
        {
            return (float)current / (float)max;
        }

        public static void GetGrappleForces(this Player player, Vector2 fromPosition, out int? preferredPlayerDirectionToSet, out float preferedPlayerVelocityX, out float preferedPlayerVelocityY)
        {
            float num = 0f;
            float num2 = 0f;
            preferredPlayerDirectionToSet = null;
            int num3 = 0;
            for (int i = 0; i < player.grapCount; i++)
            {
                Projectile projectile = Main.projectile[player.grappling[i]];
                //Main.NewText($"{projectile.whoAmI}");
                if (projectile.ai[0] != 2f || projectile.position.HasNaNs())
                    continue;

                int type = projectile.type;
                bool useAiType = projectile.ModProjectile != null && projectile.ModProjectile.AIType > 0;
                if (useAiType)
                {
                    projectile.type = projectile.ModProjectile.AIType;
                }

                num += projectile.position.X + (float)(projectile.width / 2);
                num2 += projectile.position.Y + (float)(projectile.height / 2);
                num3++;
                if (projectile.type == 446)
                {
                    Vector2 vector = new Vector2(player.controlRight.ToInt() - player.controlLeft.ToInt(), (float)(player.controlDown.ToInt() - player.controlUp.ToInt()) * player.gravDir);
                    if (vector != Vector2.Zero)
                        vector.Normalize();

                    vector *= 100f;
                    Vector2 vec = Vector2.Normalize(player.Center - projectile.Center + vector);
                    if (vec.HasNaNs())
                        vec = -Vector2.UnitY;

                    float num4 = 200f;
                    num += vec.X * num4;
                    num2 += vec.Y * num4;
                }
                else if (projectile.type == 652)
                {
                    Vector2 vector2 = new Vector2(player.controlRight.ToInt() - player.controlLeft.ToInt(), (float)(player.controlDown.ToInt() - player.controlUp.ToInt()) * player.gravDir).SafeNormalize(Vector2.Zero);
                    Vector2 vector3 = projectile.Center - player.Center;
                    Vector2 vector4 = vector3.SafeNormalize(Vector2.Zero);
                    Vector2 value = Vector2.Zero;
                    if (vector2 != Vector2.Zero)
                        value = vector4 * Vector2.Dot(vector4, vector2);

                    float num5 = 6f;
                    if (Vector2.Dot(value, vector3) < 0f && vector3.Length() >= 600f)
                        num5 = 0f;

                    num += 0f - vector3.X + value.X * num5;
                    num2 += 0f - vector3.Y + value.Y * num5;
                }
                else if (projectile.type == 865)
                {
                    Vector2 vector5 = (projectile.rotation - (float)Math.PI / 2f).ToRotationVector2().SafeNormalize(Vector2.UnitY);
                    Vector2 vector6 = -vector5 * 28f;
                    num += vector6.X;
                    num2 += vector6.Y;
                    if (vector5.X != 0f)
                        preferredPlayerDirectionToSet = Math.Sign(vector5.X);
                }

                if (useAiType)
                {
                    projectile.type = type;
                }

                ProjectileLoader.GrappleTargetPoint(projectile, player, ref num, ref num2);
            }

            if (num3 == 0)
            {
                preferedPlayerVelocityX = player.velocity.X;
                preferedPlayerVelocityY = player.velocity.Y;
                return;
            }

            float num6 = num / (float)num3;
            float num7 = num2 / (float)num3;
            Vector2 vector7 = fromPosition;
            preferedPlayerVelocityX = num6 - vector7.X;
            preferedPlayerVelocityY = num7 - vector7.Y;
            float num8 = (float)Math.Sqrt(preferedPlayerVelocityX * preferedPlayerVelocityX + preferedPlayerVelocityY * preferedPlayerVelocityY);
            float num9 = 11f;
            if (Main.projectile[player.grappling[0]].type == 315)
                num9 = 14f;

            if (Main.projectile[player.grappling[0]].type == 487)
                num9 = 12f;

            if (Main.projectile[player.grappling[0]].type >= 646 && Main.projectile[player.grappling[0]].type <= 649)
                num9 = 16f;

            ProjectileLoader.GrapplePullSpeed(Main.projectile[player.grappling[0]], player, ref num9);
            float num10 = num8;
            num10 = ((!(num8 > num9)) ? 1f : (num9 / num8));
            preferedPlayerVelocityX *= num10;
            preferedPlayerVelocityY *= num10;
        }
    }
}
