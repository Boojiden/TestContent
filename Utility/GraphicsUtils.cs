using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;

namespace TestContent.Utility
{
    public static partial class GraphicsUtils
    {

        internal static readonly FieldInfo BeginCalled = typeof(SpriteBatch).GetField("beginCalled", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static readonly FieldInfo UImageFieldMisc0 = typeof(MiscShaderData).GetField("_uImage0", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static readonly FieldInfo UImageFieldMisc1 = typeof(MiscShaderData).GetField("_uImage1", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static readonly FieldInfo UImageFieldArmor = typeof(ArmorShaderData).GetField("_uImage", BindingFlags.NonPublic | BindingFlags.Instance);
        

        public static void ExitShaderRegion(this SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static bool TryBegin(this SpriteBatch spriteBatch, SpriteSortMode sortMode,
            BlendState blendState,
            SamplerState samplerState,
            DepthStencilState depthStencilState,
            RasterizerState rasterizerState,
            Effect effect,
            Matrix transformMatrix)
        {
            if (spriteBatch.HasBeginBeenCalled())
            {
                return false;
            }
            else
            {
                spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
                return true;
            }
        }

        public static bool TryEnd(this SpriteBatch spriteBatch)
        {
            if (!spriteBatch.HasBeginBeenCalled())
            {
                return false;
            }
            else
            {
                spriteBatch.End();
                return true;
            }
        }

        /// <summary>
        /// Determines if a <see cref="SpriteBatch"/> is in a lock due to a <see cref="SpriteBatch.Begin"/> call.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to check.</param>
        public static bool HasBeginBeenCalled(this SpriteBatch spriteBatch)
        {
            return (bool)BeginCalled.GetValue(spriteBatch);
        }

        public static void CalculatePerspectiveMatricies(out Matrix viewMatrix, out Matrix projectionMatrix)
        {
            Vector2 zoom = Main.GameViewMatrix.Zoom;
            Matrix zoomScaleMatrix = Matrix.CreateScale(zoom.X, zoom.Y, 1f);

            // Screen bounds.
            int width = Main.instance.GraphicsDevice.Viewport.Width;
            int height = Main.instance.GraphicsDevice.Viewport.Height;

            // Get a matrix that aims towards the Z axis (these calculations are relative to a 2D world).
            viewMatrix = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up);

            // Offset the matrix to the appropriate position.
            viewMatrix *= Matrix.CreateTranslation(0f, -height, 0f);

            // Flip the matrix around 180 degrees.
            viewMatrix *= Matrix.CreateRotationZ(MathHelper.Pi);

            // Account for the inverted gravity effect.
            if (Main.LocalPlayer.gravDir == -1f)
                viewMatrix *= Matrix.CreateScale(1f, -1f, 1f) * Matrix.CreateTranslation(0f, height, 0f);

            // And account for the current zoom.
            viewMatrix *= zoomScaleMatrix;

            projectionMatrix = Matrix.CreateOrthographicOffCenter(0f, width * zoom.X, 0f, height * zoom.Y, 0f, 1f) * zoomScaleMatrix;
        }

        /// <summary>
        /// Sets the current render target to the provided one.
        /// </summary>
        /// <param name="target">The render target to swap to</param>
        /// <param name="flushColor">The color to clear the screen with. Transparent by default</param>
        public static void SwapTo(this RenderTarget2D target, Color? flushColor = null)
        {
            // If we are in the menu, a server, or any of these are null, return.
            if (Main.gameMenu || Main.dedServ || target is null || Main.instance.GraphicsDevice is null || Main.spriteBatch is null)
                return;

            Main.instance.GraphicsDevice.SetRenderTarget(target);
            Main.instance.GraphicsDevice.Clear(flushColor ?? Color.Transparent);
        }

        /// <summary>
        /// Manually sets the texture of a <see cref="MiscShaderData"/> instance, since vanilla's implementation only supports strings that access vanilla textures.
        /// </summary>
        /// <param name="shader">The shader to bind the texture to.</param>
        /// <param name="texture">The texture to bind.</param>
        public static MiscShaderData SetShaderTexture(this MiscShaderData shader, Asset<Texture2D> texture, int index = 1)
        {
            switch (index)
            {
                case 0:
                    UImageFieldMisc0.SetValue(shader, texture);
                    break;
                case 1:
                    UImageFieldMisc1.SetValue(shader, texture);
                    break;
            }
            return shader;
        }

        public static MiscShaderData SetTrailTextureWidth(this MiscShaderData shader ,float? width)
        {
            if(width != null)
            {
                shader.Shader.Parameters["trailWidth"]?.SetValue((float)width);
            }
            else
            {
                shader.Shader.Parameters["trailWidth"]?.SetValue(1f);
            }
            return shader;
        }
    }
}

