using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TestContent.NPCs.Minos;
using TestContent.Projectiles.Weapons.Gambling.Hardmode;

namespace TestContent.Commands
{
    public class LandingPointCommand : ModCommand
    {
        public override string Command => "LandingPoint";

        public override CommandType Type => CommandType.Chat;

        public override string Description => "Test how LandingPoint works.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var userPos = caller.Player.Center;
            var targetPos = userPos + (Vector2.UnitY * 16f * 25f);
            var recievedPos = MinosAttack.GetLandingPoint(userPos, Vector2.UnitY, targetPos, 1000f, caller.Player.height);
            //Green is target, red is recievedPoint
            Projectile.NewProjectileDirect(caller.Player.GetSource_FromAI(), targetPos, Vector2.Zero, ModContent.ProjectileType<ChipProjectile>(), 0, 0, ai0: 1);
            Projectile.NewProjectileDirect(caller.Player.GetSource_FromAI(), recievedPos, Vector2.Zero, ModContent.ProjectileType<ChipProjectile>(), 0, 0, ai0: 2);

            Main.NewText($"Results: {userPos} : {targetPos} : {recievedPos}");
        }
    }
}
