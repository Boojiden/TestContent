using Microsoft.Xna.Framework;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestContent.Utility
{
    public class CubicBezierCurveLerp
    {
        public float x1;
        public float x2;
        public float y1;
        public float y2;

        public CubicBezierCurveLerp(float x1,  float x2, float y1, float y2)
        {
            this.x1 = x1;
            this.x2 = x2;
            this.y1 = y1;
            this.y2 = y2;
        }

        public float GetLerp(float time)
        {
            if(x1 == y1 && x2 == y2)
            {
                return time;
            }

            if (time > 1f)
            {
                return 1f;
            }
            else if (time < 0f)
            {
                return 0f;
            }

            return CalcBezier(GetTForX(time), y1, y2);
        }

        private float CalcBezier(float time, float py1, float py2)
        {
            return ((First(py1, py2) * time + Second(py1, py2)) * time + Third(py1)) * time;
        }

        private float First(float a1, float a2)
        {
            return 1f - 3f * a2 + 3f * a1;
        }

        private float Second(float a1, float a2)
        {
            return 3f * a2 - 6f * a1;
        }

        private float Third(float a1) 
        {
            return 3f * a1;
        }

        public float GetTForX(float px)
        {
            float tGuess = px;
            for(int i = 0; i < 4; i++)
            {
                float slope = GetSlope(tGuess, x1, x2);
                if (slope == 0)
                {
                    return tGuess;
                }
                float currentX = CalcBezier(tGuess, x1, x2) - px;
                tGuess -= currentX / slope;
            }
            return tGuess;
        }

        private float GetSlope(float tGuess, float px1, float px2)
        {
            return 3f * First(px1, px2) * tGuess * tGuess + 2f * Second(px1, px2) * tGuess + Third(px1);
        }
    }
}
