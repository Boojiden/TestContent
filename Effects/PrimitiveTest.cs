using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using Terraria.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria.ID;
using TestContent.Items.Weapons;

namespace TestContent.Effects
{
    class PrimitiveTest : ModSystem
    {
        BasicEffect basicEffect;
        Vector2 oldMousePos;
        Vector2 oldDir;
        VertexPositionColor[] vertices;
        int vertexAmount = 0;

        int timer = 0;
        public override void Load()
        {
            Main.RunOnMainThread(() =>
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    basicEffect = new BasicEffect(Main.instance.GraphicsDevice);
                    basicEffect.VertexColorEnabled = true;
                    vertices = new VertexPositionColor[30];
                }
            });
        }

        public void PassEffects()
        {
            for(int i = 0; i < vertexAmount; i++)
            {
                //vertices[i].Color *= 0.99f;
                vertices[i].Position += new Vector3(Main.rand.NextFloat(-10f, 10f), -1f, 0f);
            }
        }

        public void ShiftVertecies()
        {
            for(int i = 3; i < vertices.Length; i++)
            {
                vertices[i-3] = vertices[i];
            }
            vertexAmount = vertices.Length - 3;
        }
        public override void PostDrawTiles()
        {
            base.PostDrawTiles();
            Player player = Main.player[Main.myPlayer];

            //Main.NewText("" + player.inventory[player.selectedItem].netID + " " + ModContent.ItemType<LmaoBox>());
            if (player.inventory[player.selectedItem].netID == ModContent.ItemType<LmaoBox>())
            {
                //Main.NewText("drawing...");

                var gd = Main.instance.GraphicsDevice;
                var viewport = gd.Viewport;

                Vector2 screenpos = Main.screenPosition;
                basicEffect.World = Matrix.CreateTranslation(-new Vector3(screenpos.X, screenpos.Y, 0));
                basicEffect.View = Main.GameViewMatrix.TransformationMatrix;
                basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, -1, 1);

                if(oldMousePos.Equals(null))
                {
                    oldMousePos = Main.MouseWorld;
                }

                Vector2 dir = (Main.MouseWorld - oldMousePos);
                Vector2 unNormdir = dir;
                dir.Normalize();

                if(oldMousePos == Main.MouseWorld) {
                    dir = oldDir;
                }

                if (timer++ >= 30 && !Main.gamePaused)
                {
                    if (vertexAmount >= vertices.Length - 1)
                    {
                        ShiftVertecies();
                    }

                    timer = 0;
                    vertices[vertexAmount] = new(new Vector3(Main.MouseWorld + new Vector2(0, -50), 0), Color.White);
                    vertices[vertexAmount + 1] = new(new Vector3(Main.MouseWorld + new Vector2(50, 50), 0), Color.Blue);
                    vertices[vertexAmount + 2] = new(new Vector3(Main.MouseWorld + new Vector2(-50, 50), 0), Color.Blue);
                    vertexAmount += 3;
                    //Main.NewText(vertexAmount);
                }

                if(!Main.gamePaused)
                {
                    PassEffects();
                }

                gd.RasterizerState = RasterizerState.CullNone;
                basicEffect.CurrentTechnique.Passes[0].Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length);
                
                
                oldMousePos = Main.MouseWorld;
                oldDir = dir;
            }
            else
            {
                vertices = new VertexPositionColor[vertices.Length];
                vertexAmount= 0;
                oldMousePos = Main.MouseWorld;
            }
        }
    }
}
