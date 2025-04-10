using Microsoft.Xna.Framework;
using Terraria;

namespace TestContent.Effects.IK
{
    public class Limb
    {
        // Doubles are used instead of floats as a means of providing sufficient precision to not cause erroneous results when
        // doing approximations of derivative limits with small divisors.
        public double Rotation;
        public double Length;
        public Vector2 ConnectPoint;
        public Vector2 EndPoint => ConnectPoint + ((float)Rotation).ToRotationVector2() * (float)Length;
        public Vector2 MidPoint => Vector2.Lerp(ConnectPoint, EndPoint, 0.5f);

        public Limb(float rotation, float length)
        {
            Rotation = rotation;
            Length = length;
        }
    }
}
