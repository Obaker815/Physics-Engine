using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Physics_Engine
{
    internal class RenderWindow : GameWindow
    {
        public RenderWindow() 
            : base(
                  GameWindowSettings.Default,
                   nativeWindowSettings: new NativeWindowSettings()
                   {
                       ClientSize = new Vector2i(800, 600),
                       Title = "Render"
                   })
        { }

        protected override void OnLoad()
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1f);

            float[] vertices =
            {
                 // position    // uv
                 0f, 0f,        0f, 0f,
                 100f, 0f,      1f, 0f,
                 100f, 100f,    1f, 1f,
                 0f, 100f,      0f, 1f
            };

            uint[] indices =
            {
                0, 1, 2,
                2, 3, 0
            };

            // genarate vertex array, vertex buffer, and index buffer
            int vao = GL.GenVertexArray();
            int vbo = GL.GenBuffer();
            int ebo = GL.GenBuffer();

            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float),
                          vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint),
                          indices, BufferUsageHint.StaticDraw);

            // position (locaion: 0)
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // uv (location = 1)
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false,
                                   4 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            SwapBuffers();
        }
    }
}
