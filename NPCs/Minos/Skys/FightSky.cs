using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using Terraria;
using TestContent.Global.NPC;
using Terraria.Graphics.Shaders;
using Steamworks;
using System.IO.Pipelines;
using TestContent.Utility;

namespace TestContent.NPCs.Minos.Skys
{
    public class FightSky : CustomSky
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
                if (BossNPCGlobals.minos != -1)
                {
                    float dist = Vector2.Distance(Main.LocalPlayer.position, Main.npc[BossNPCGlobals.minos].position);
                    float intensity = MathHelper.SmoothStep(4500f, 9000f, dist) / 9000f;
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.BackgroundViewMatrix.ZoomMatrix);
                    var shader = GameShaders.Misc["TestContent:MinosFightBackground"];
                    GameShaders.Misc[$"TestContent:MinosFightBackground"].SetShaderTexture(ModContent.Request<Texture2D>("TestContent/NPCs/Minos/Extras/Flesh"), index: 1);
                    shader.Apply();
                    spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth * 2, Main.screenHeight * 2), Color.White * intensity);
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.BackgroundViewMatrix.ZoomMatrix);
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
            if (BossNPCGlobals.minos == -1)
            {
                active = false;
                return;
            }
            NPC orb = Main.npc[BossNPCGlobals.minos];
            if (!orb.active || orb.type != ModContent.NPCType<MinosPrime>())
            {
                active = false;
            }
            Player player = Main.LocalPlayer;
            Lighting.AddLight(player.Center, new Vector3(5, 5, 5));
        }

        public override float GetCloudAlpha()
        {
            return 0f;
        }
    }
}
