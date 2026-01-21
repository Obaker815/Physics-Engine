using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Physics_Engine
{
    internal class RenderWindow : GameWindow
    {
        private Matrix4 _projection;

        private int _indexCount;
        private int _vao;
        private int _vbo;
        private int _ebo;

        public RenderWindow() 
            : base(
                  GameWindowSettings.Default,
                   nativeWindowSettings: new NativeWindowSettings()
                   {
                       ClientSize = new Vector2i(800, 600),
                       Title = "Render"
                   })
        { }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);

            _projection = Matrix4.CreateOrthographicOffCenter(
                0, Size.X,
                Size.Y, 0,
                -1, 1
            );

            ShaderManager.Get("solid_color").Use();
            ShaderManager.Get("solid_color").SetMatrix4("uProjection", _projection);
        }

        protected override void OnLoad()
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1f);

            if (!File.Exists("./RuntimeLog.txt")) File.Create("./RuntimeLog.txt").Close();
            string Log = "";
            Log += GL.GetString(StringName.Version) + "\n";
            Log += GL.GetString(StringName.ShadingLanguageVersion) + "\n";

            File.WriteAllText("./RuntimeLog.txt", Log);

            float[] vertices =
            {
                 // position    // uv       // normals
                 0f, 0f,        0f, 0f,     0f, 0f,
                 100f, 0f,      1f, 0f,     1f, 0f,
                 100f, 100f,    1f, 1f,     1f, 1f,
                 0f, 100f,      0f, 1f,     0f, 1f,
            };

            uint[] indices =
            {
                0, 1, 2,
                2, 3, 0
            };

            // genarate vertex array, vertex buffer, and index buffer
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();
            _indexCount = indices.Length;

            // bind the vertex array
            GL.BindVertexArray(_vao);

            // bind the vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float),
                          vertices, BufferUsageHint.StaticDraw);

            // bind the element buffer
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint),
                          indices, BufferUsageHint.StaticDraw);

            int stride = 6 * sizeof(float);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false,
                                   stride, 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false,
                                   stride, 4 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            GL.BindVertexArray(0);

            _projection = Matrix4.CreateOrthographicOffCenter(
                0, Size.X,
                Size.Y, 0,
                -1, 1
            );

            ShaderManager.Load(
                "solid_color",
                new Dictionary<ShaderType, string>
                {
                    { ShaderType.VertexShader, "./Shaders/Projection2D.vert" },
                    { ShaderType.FragmentShader, "./Shaders/Color.frag" }
                }
            );
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            Shader shader = ShaderManager.Get("solid_color");
            shader.Use();
            shader.SetMatrix4("uProjection", _projection);
            shader.SetVector4("uColor", new Vector4(0f, 1f, 0f, 1f)); // green
            shader.SetVector2("uLightSource", new Vector2(1f, 1f).Normalized());

            GL.BindVertexArray(_vao);
            GL.DrawElements(
                PrimitiveType.Triangles,
                _indexCount,
                DrawElementsType.UnsignedInt,
                0
            );

            SwapBuffers();
        }
    }
}
