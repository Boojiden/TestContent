using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace TestContent.Utility
{
    public static class CollisionUtility
    {
        public static void LaserScanTopSolid(Vector2 samplingPoint, Vector2 directionUnit, float samplingWidth, float maxDistance, float[] samples)
        {
            for (int i = 0; i < samples.Length; i++)
            {
                float num = (float)i / (float)(samples.Length - 1);
                Vector2 vector = samplingPoint + directionUnit.RotatedBy(1.5707963705062866) * (num - 0.5f) * samplingWidth;
                int num2 = (int)vector.X / 16;
                int num3 = (int)vector.Y / 16;
                Vector2 vector2 = vector + directionUnit * maxDistance;
                int num4 = (int)vector2.X / 16;
                int num5 = (int)vector2.Y / 16;
                float num6 = 0f;
                num6 = (TupleHitLineTopSolid(num2, num3, num4, num5, 0, 0, new List<Tuple<int, int>>(), out var col) ? ((col.Item1 != num4 || col.Item2 != num5) ? (new Vector2(Math.Abs(num2 - col.Item1), Math.Abs(num3 - col.Item2)).Length() * 16f) : maxDistance) : (new Vector2(Math.Abs(num2 - col.Item1), Math.Abs(num3 - col.Item2)).Length() * 16f));
                samples[i] = num6;
            }
        }

        /// <summary>
        /// Copy of TupleHitLine in Terraria.Collision edited to also collide with topsolid tiles.
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="ignoreX"></param>
        /// <param name="ignoreY"></param>
        /// <param name="ignoreTargets"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public static bool TupleHitLineTopSolid(int x1, int y1, int x2, int y2, int ignoreX, int ignoreY, List<Tuple<int, int>> ignoreTargets, out Tuple<int, int> col)
        {
            int value = x1;
            int value2 = y1;
            int value3 = x2;
            int value4 = y2;
            value = Utils.Clamp(value, 1, Main.maxTilesX - 1);
            value3 = Utils.Clamp(value3, 1, Main.maxTilesX - 1);
            value2 = Utils.Clamp(value2, 1, Main.maxTilesY - 1);
            value4 = Utils.Clamp(value4, 1, Main.maxTilesY - 1);
            float num = Math.Abs(value - value3);
            float num2 = Math.Abs(value2 - value4);
            if (num == 0f && num2 == 0f)
            {
                col = new Tuple<int, int>(value, value2);
                return true;
            }

            float num3 = 1f;
            float num4 = 1f;
            if (num == 0f || num2 == 0f)
            {
                if (num == 0f)
                    num3 = 0f;

                if (num2 == 0f)
                    num4 = 0f;
            }
            else if (num > num2)
            {
                num3 = num / num2;
            }
            else
            {
                num4 = num2 / num;
            }

            float num5 = 0f;
            float num6 = 0f;
            int num7 = 1;
            if (value2 < value4)
                num7 = 2;

            int num8 = (int)num;
            int num9 = (int)num2;
            int num10 = Math.Sign(value3 - value);
            int num11 = Math.Sign(value4 - value2);
            bool flag = false;
            bool flag2 = false;
            try
            {
                do
                {
                    switch (num7)
                    {
                        case 2:
                            {
                                num5 += num3;
                                int num13 = (int)num5;
                                num5 %= 1f;
                                for (int j = 0; j < num13; j++)
                                {
                                    if (Main.tile[value, value2 - 1] == null)
                                    {
                                        col = new Tuple<int, int>(value, value2 - 1);
                                        return false;
                                    }

                                    if (Main.tile[value, value2 + 1] == null)
                                    {
                                        col = new Tuple<int, int>(value, value2 + 1);
                                        return false;
                                    }

                                    Tile tile4 = Main.tile[value, value2 - 1];
                                    Tile tile5 = Main.tile[value, value2 + 1];
                                    Tile tile6 = Main.tile[value, value2];
                                    if (!ignoreTargets.Contains(new Tuple<int, int>(value, value2)) && !ignoreTargets.Contains(new Tuple<int, int>(value, value2 - 1)) && !ignoreTargets.Contains(new Tuple<int, int>(value, value2 + 1)))
                                    {
                                        if (ignoreY != -1 && num11 < 0 && !tile4.IsActuated && tile4.HasTile && (Main.tileSolid[tile4.TileType] || Main.tileSolidTop[tile4.TileType]))
                                        {
                                            col = new Tuple<int, int>(value, value2 - 1);
                                            return true;
                                        }

                                        if (ignoreY != 1 && num11 > 0 && !tile5.IsActuated && tile5.HasTile && (Main.tileSolid[tile5.TileType] || Main.tileSolidTop[tile5.TileType]))
                                        {
                                            col = new Tuple<int, int>(value, value2 + 1);
                                            return true;
                                        }

                                        if (!tile6.IsActuated && tile6.HasTile && (Main.tileSolid[tile6.TileType] || Main.tileSolidTop[tile6.TileType]))
                                        {
                                            col = new Tuple<int, int>(value, value2);
                                            return true;
                                        }
                                    }

                                    if (num8 == 0 && num9 == 0)
                                    {
                                        flag = true;
                                        break;
                                    }

                                    value += num10;
                                    num8--;
                                    if (num8 == 0 && num9 == 0 && num13 == 1)
                                        flag2 = true;
                                }

                                if (num9 != 0)
                                    num7 = 1;

                                break;
                            }
                        case 1:
                            {
                                num6 += num4;
                                int num12 = (int)num6;
                                num6 %= 1f;
                                for (int i = 0; i < num12; i++)
                                {
                                    if (Main.tile[value - 1, value2] == null)
                                    {
                                        col = new Tuple<int, int>(value - 1, value2);
                                        return false;
                                    }

                                    if (Main.tile[value + 1, value2] == null)
                                    {
                                        col = new Tuple<int, int>(value + 1, value2);
                                        return false;
                                    }

                                    Tile tile = Main.tile[value - 1, value2];
                                    Tile tile2 = Main.tile[value + 1, value2];
                                    Tile tile3 = Main.tile[value, value2];
                                    if (!ignoreTargets.Contains(new Tuple<int, int>(value, value2)) && !ignoreTargets.Contains(new Tuple<int, int>(value - 1, value2)) && !ignoreTargets.Contains(new Tuple<int, int>(value + 1, value2)))
                                    {
                                        if (ignoreX != -1 && num10 < 0 && !tile.IsActuated && tile.HasTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]))
                                        {
                                            col = new Tuple<int, int>(value - 1, value2);
                                            return true;
                                        }

                                        if (ignoreX != 1 && num10 > 0 && !tile2.IsActuated && tile2.HasTile && (Main.tileSolid[tile2.TileType] || Main.tileSolidTop[tile2.TileType]))
                                        {
                                            col = new Tuple<int, int>(value + 1, value2);
                                            return true;
                                        }

                                        if (!tile3.IsActuated && tile3.HasTile && (Main.tileSolid[tile3.TileType] || Main.tileSolidTop[tile3.TileType]))
                                        {
                                            col = new Tuple<int, int>(value, value2);
                                            return true;
                                        }
                                    }

                                    if (num8 == 0 && num9 == 0)
                                    {
                                        flag = true;
                                        break;
                                    }

                                    value2 += num11;
                                    num9--;
                                    if (num8 == 0 && num9 == 0 && num12 == 1)
                                        flag2 = true;
                                }

                                if (num8 != 0)
                                    num7 = 2;

                                break;
                            }
                    }

                    if (Main.tile[value, value2] == null)
                    {
                        col = new Tuple<int, int>(value, value2);
                        return false;
                    }

                    Tile tile7 = Main.tile[value, value2];
                    if (!ignoreTargets.Contains(new Tuple<int, int>(value, value2)) && !tile7.IsActuated && tile7.HasTile && (Main.tileSolid[tile7.TileType] || Main.tileSolidTop[tile7.TileType]))
                    {
                        col = new Tuple<int, int>(value, value2);
                        return true;
                    }
                } while (!(flag || flag2));

                col = new Tuple<int, int>(value, value2);
                return true;
            }
            catch
            {
                col = new Tuple<int, int>(x1, y1);
                return false;
            }
        }
    }
}
