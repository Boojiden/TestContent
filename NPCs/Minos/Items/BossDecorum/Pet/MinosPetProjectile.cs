using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using TestContent.UI;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using Microsoft.Xna.Framework.Graphics;
using TestContent.Utility;

namespace TestContent.NPCs.Minos.Items.BossDecorum.Pet
{
    public class MinosPetProjectile : ModProjectile
    {
        public Asset<Texture2D> ripple;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            Main.projPet[Projectile.type] = true;

            // This code is needed to customize the vanity pet display in the player select screen. Quick explanation:
            // * It uses fluent API syntax, just like Recipe
            // * You start with ProjectileID.Sets.SimpleLoop, specifying the start and end frames as well as the speed, and optionally if it should animate from the end after reaching the end, effectively "bouncing"
            // * To stop the animation if the player is not highlighted/is standing, as done by most grounded pets, add a .WhenNotSelected(0, 0) (you can customize it just like SimpleLoop)
            // * To set offset and direction, use .WithOffset(x, y) and .WithSpriteDirection(-1)
            // * To further customize the behavior and animation of the pet (as its AI does not run), you have access to a few vanilla presets in DelegateMethods.CharacterPreview to use via .WithCode(). You can also make your own, showcased in MinionBossPetProjectile
            ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(0, Main.projFrames[Projectile.type], 6)
                .WithOffset(-10, -20f)
                .WithSpriteDirection(-1)
                .WithCode(DelegateMethods.CharacterPreview.Float);
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.ZephyrFish); // Copy the stats of the Zephyr Fish
            Projectile.scale = 1f;
            Projectile.width = 24;
            Projectile.height = 24;
            AIType = ProjectileID.ZephyrFish; // Mimic as the Zephyr Fish during AI.
            ripple = ModContent.Request<Texture2D>("TestContent/NPCs/Minos/SoulOrbRipple");
        }

        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];
            //player.QuickSpawnItem
            player.zephyrfish = false; // Relic from AIType

            return true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            //Main.NewText(timeUntillJumpscare);

            // Keep the projectile from disappearing as long as the player isn't dead and has the pet buff.
            if (!player.dead && player.HasBuff(ModContent.BuffType<MinosPetBuff>()))
            {
                Projectile.timeLeft = 2;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            int rippleAmount = 6;
            float rippleRadius = 1.5f;
            float rippleTime = ((float)Main.timeForVisualEffects % 90f) / 90f;
            for (int i = 0; i < rippleAmount; i++)
            {
                float rippleSeq = GameplayUtils.GetTimeFromInts(i, rippleAmount - 1);
                float realTime = rippleSeq - rippleTime;
                if (realTime <= 0f)
                {
                    realTime = 1f + realTime;
                }

                float scale = rippleRadius * realTime;
                Color color = Color.Lerp(Color.White, Color.Transparent, realTime);
                TestContent.CenteredDraw(ripple.Value, Projectile.Center, color, scale: scale);
            }
            return true;
        }
    }
}
