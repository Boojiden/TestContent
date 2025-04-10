using Microsoft.Xna.Framework;
using System;
using System.Net.Http.Headers;
using Terraria;
using Terraria.ID;

namespace TestContent.Utility
{
    public static class DustUtils
    {
        public static void CreateDustBurstCirclePerfect(int type, Vector2 center, float radius, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                Vector2 vel = (((float)i / (float)amount) * (float)Math.Tau).ToRotationVector2() * radius;
                Dust.NewDustPerfect(center, type, vel);
            }
        }

        public static void CreateDustBurstCircle(int type, Vector2 center, float minSpeed, float maxSpeed, int amount, Color? col = null, float scale = 1f)
        {
            for (int i = 0; i < amount; i++)
            {
                Vector2 vel = (((float)i / (float)amount) * (float)Math.Tau).ToRotationVector2() * (minSpeed + (Main.rand.NextFloat() * maxSpeed));
                Dust dust = Dust.NewDustPerfect(center, type, vel);
                dust.scale = scale;
                dust.color = col == null ? dust.color : (Color)col;
            }
        }

        public static void CreateDustBurst(int type, Vector2 center, float minSpeed, float maxSpeed, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                Vector2 vel = (Main.rand.NextFloat() * (float)Math.Tau).ToRotationVector2() * (minSpeed + (Main.rand.NextFloat() * maxSpeed));
                Dust.NewDustPerfect(center, type, vel);
            }
        }

        public static void CreateDustLine(int type, Vector2 start, Vector2 end, float width = 1f, float density = 10f, Color? col = null, float scale = 1f) 
        {
            float num = 1f / density;
            for (float num2 = 0f; num2 < 1f; num2 += num)
            {
                Dust dust = Dust.NewDustPerfect(Vector2.Lerp(start, end, num2), type);
                dust.scale = scale;
                dust.color = col == null ? dust.color : (Color)col;
            }
        }
    }
}
