using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Physics;
using TestContent.Projectiles;
using TestContent.Utility;

namespace TestContent.NPCs.Minos.Projectiles
{
    public class MinosRock : BasicTrailProjectile
    {
        protected override string trailTextureName => "";

        public override string Texture => "TestContent/ExtraTextures/InvisibleSprite";

        public static string filePath = "TestContent/NPCs/Minos/Projectiles/";

        public SoundStyle crunch;

        public int rockType
        {
            get => (int)(Projectile.ai[0]);
        }

        public int Timer
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        public static List<(Asset<Texture2D>, int, float)> rockProperties = new List<(Asset<Texture2D>, int, float)>
        {
            (ModContent.Request<Texture2D>(filePath + "Rock1"), 24, 24f),
            (ModContent.Request<Texture2D>(filePath + "Rock2"), 18, 18f),
            (ModContent.Request<Texture2D>(filePath + "Rock3"), 14, 14f)
        };

        public override bool? CanDamage()
        {
            return Timer >= 10;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = rockProperties[rockType].Item2;
            Projectile.height = rockProperties[rockType].Item2;
            Projectile.friendly = false;
            Projectile.hostile = true;
            crunch = new SoundStyle(ModUtils.GetSoundFileLocation("Minos/RockBreak"))
            {
                PlayOnlyIfFocused = true,
                Volume = 0.4f,
                MaxInstances = 3,
            };
        }

        public override void AI()
        {
            Timer++;
            Projectile.velocity.Y += 0.35f;
            Projectile.rotation += (float)Math.PI / 32f;
            if(Projectile.rotation > MathHelper.ToRadians(360f))
            {
                Projectile.rotation = 0;
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (!Main.dedServ)
            {
                //Spawn Dust
                for(int i = 0; i < 10; i++)
                {
                    Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.Stone);
                }
                SoundEngine.PlaySound(crunch, Projectile.Center);
            }
        }

        public override Color PrimitiveColorFunction(float completionRatio)
        {
            return Color.Lerp(Color.White, Color.Transparent, completionRatio);
        }

        public override Vector2 PrimitiveOffsetFunction(float completionRatio)
        {
            return base.PrimitiveOffsetFunction(completionRatio);
        }

        public override float PrimitiveWidthFunction(float completionRatio)
        {
            //rockProperties[rockType].Item3
            return 7f * (1 - completionRatio);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var texture = rockProperties[rockType].Item1.Value;
            var rect = new Rectangle(0, 0, texture.Width, texture.Height);
            var origin = rect.Size() / 2;

            var effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, rect, Color.White, Projectile.rotation, origin, new Vector2(Projectile.scale, Projectile.scale), effects);
            return false;
        }
    }
}
