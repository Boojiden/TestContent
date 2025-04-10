using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using TestContent.Global.NPC;

namespace TestContent.NPCs.Minos.Skys
{
    public class IntroSky : CustomSky
    {
        public bool active = false;
        public override void Activate(Vector2 position, params object[] args)
        {
            active = true;
        }

        public override void Deactivate(params object[] args)
        {
            active = false;
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0 && minDepth < 0)
            {
                if(BossNPCGlobals.minosorb != -1)
                {
                    float dist = Vector2.Distance(Main.LocalPlayer.position, Main.npc[BossNPCGlobals.minosorb].position);
                    float intensity = MathHelper.SmoothStep(4500f, 9000f, dist) / 9000f;
                    spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth * 2, Main.screenHeight * 2), Color.Black * intensity);
                }
            }
        }

        public override bool IsActive()
        {
            return active;
        }

        public override void Reset()
        {
            active = false;
        }

        public override void Update(GameTime gameTime)
        {
            if(BossNPCGlobals.minosorb == -1)
            {
                active = false;
                return;
            }
            NPC orb = Main.npc[BossNPCGlobals.minosorb];
            if (!orb.active || orb.type != ModContent.NPCType<SoulOrb>())
            {
                active = false;
            }
        }

        public override float GetCloudAlpha()
        {
            return 0f;
        }
    }
}
