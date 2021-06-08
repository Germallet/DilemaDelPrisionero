using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DilemaPrisionero
{
    public static class Dilema
    {
        [STAThread]
        static void Main()
        {
            using var game = new TGCGame();
            game.Run();
        }
    }

    internal class TGCGame : Game
    {
        private SpriteBatch spriteBatch;
        private Effect dilemaShader;
        private FullScreenQuad fullScreenQuad;
        private RenderTarget2D renderInput, renderPoints, renderTarget;
        private const int width = 1024, height = 786;

        internal TGCGame() => new GraphicsDeviceManager(this);

        protected override void LoadContent()
        {
            base.LoadContent();
            Window.Title = "Dilema del Prisionero";
            spriteBatch = new SpriteBatch(GraphicsDevice);
            dilemaShader = Content.Load<Effect>("Content/Effects/DilemaShader");
            fullScreenQuad = new FullScreenQuad(GraphicsDevice);
            renderInput = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);
            renderPoints = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Vector2, DepthFormat.None);
            renderTarget = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);
            renderInput.SetData(InitialCenter());   
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(renderPoints);
            dilemaShader.CurrentTechnique = dilemaShader.Techniques["CalculatePoints"];
            dilemaShader.Parameters["inputTexture"].SetValue(renderInput);
            fullScreenQuad.Draw(dilemaShader);

            GraphicsDevice.SetRenderTarget(renderTarget);
            dilemaShader.CurrentTechnique = dilemaShader.Techniques["ChooseStrategy"];
            dilemaShader.Parameters["inputTexture"].SetValue(renderPoints);
            fullScreenQuad.Draw(dilemaShader);

            GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin();
            spriteBatch.Draw(renderTarget, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);
            spriteBatch.End();

            RenderTarget2D renderSwap = renderInput;
            renderInput = renderTarget;
            renderTarget = renderSwap;
        }

        private Color[] InitialCenter()
        {
            Color[] colors = new Color[width * height];
            colors[width * height / 2 - width / 2] = new Color(255, 0, 0, 0);
            return colors;
        }

        protected override void UnloadContent()
        {
            Content.Unload();
            fullScreenQuad.Dispose();
        }
    }

    internal class FullScreenQuad
    {
        private readonly GraphicsDevice device;
        private IndexBuffer indexBuffer;
        private VertexBuffer vertexBuffer;

        internal FullScreenQuad(GraphicsDevice device)
        {
            this.device = device;
            CreateVertexBuffer();
            CreateIndexBuffer();
        }

        private void CreateVertexBuffer()
        {
            var vertices = new VertexPositionTexture[4];
            vertices[0].Position = new Vector3(-1f, -1f, 0f);
            vertices[0].TextureCoordinate = new Vector2(0f, 1f);
            vertices[1].Position = new Vector3(-1f, 1f, 0f);
            vertices[1].TextureCoordinate = new Vector2(0f, 0f);
            vertices[2].Position = new Vector3(1f, -1f, 0f);
            vertices[2].TextureCoordinate = new Vector2(1f, 1f);
            vertices[3].Position = new Vector3(1f, 1f, 0f);
            vertices[3].TextureCoordinate = new Vector2(1f, 0f);

            vertexBuffer = new VertexBuffer(device, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);
        }

        private void CreateIndexBuffer()
        {
            ushort[] indices = new ushort[6] { 0, 1, 3, 0, 3, 2 };
            indexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }

        internal void Draw(Effect effect)
        {
            device.SetVertexBuffer(vertexBuffer);
            device.Indices = indexBuffer;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            }
        }

        internal void Dispose()
        {
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
        }
    }
}