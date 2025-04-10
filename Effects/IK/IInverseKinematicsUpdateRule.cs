using Microsoft.Xna.Framework;

namespace TestContent.Effects.IK
{
    public interface IInverseKinematicsUpdateRule
    {
        void Update(LimbCollection limbs, Vector2 destination, Vector2? pole);
    }
}
