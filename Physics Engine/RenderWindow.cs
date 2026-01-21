using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Diagnostics;

namespace Physics_Engine
{
    internal class RenderWindow : GameWindow
    {
        private Stopwatch _time;

        private Matrix4 _projection;
        private Matrix4 _view;
        private Matrix4 _model;

        private int _indexCount;
        private int _vao;
        private int _vbo;
        private int _ebo;

        public RenderWindow()
            : base(
                  GameWindowSettings.Default,
                  new NativeWindowSettings()
                  {
                      ClientSize = new Vector2i(800, 600),
                      Title = "3D Render"
                  })
        { }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);

            // Perspective projection
            _projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(60f),
                Size.X / (float)Size.Y,
                0.1f,
                100f
            );

            Shader shader = ShaderManager.Get("solid_color");
            shader.Use();
            shader.SetMatrix4("uProjection", _projection);
        }

        protected override void OnLoad()
        {
            GL.Enable(EnableCap.DepthTest); // enable 3D depth testing
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1f);

            // Simple cube
            float[] vertices =
            {
                // positions          // normals
                -0.5f, -0.5f, -0.5f,  -0.577f, -0.577f, -0.577f,
                 0.5f, -0.5f, -0.5f,   0.577f, -0.577f, -0.577f,
                 0.5f,  0.5f, -0.5f,   0.577f,  0.577f, -0.577f,
                -0.5f,  0.5f, -0.5f,  -0.577f,  0.577f, -0.577f,

                -0.5f, -0.5f,  0.5f,  -0.577f, -0.577f,  0.577f,
                 0.5f, -0.5f,  0.5f,   0.577f, -0.577f,  0.577f,
                 0.5f,  0.5f,  0.5f,   0.577f,  0.577f,  0.577f,
                -0.5f,  0.5f,  0.5f,  -0.577f,  0.577f,  0.577f,
            };

            uint[] indices =
            {
                // back face
                0,1,2, 2,3,0,
                // front face
                4,5,6, 6,7,4,
                // left face
                0,3,7, 7,4,0,
                // right face
                1,5,6, 6,2,1,
                // bottom face
                0,1,5, 5,4,0,
                // top face
                3,2,6, 6,7,3
            };

            _indexCount = indices.Length;

            // VAO/VBO/EBO
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float),
                          vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint),
                          indices, BufferUsageHint.StaticDraw);

            int stride = 6 * sizeof(float);

            // Position
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);

            // Normal
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);

            // Camera/view
            _view = Matrix4.LookAt(new Vector3(2f, 2f, 3f), Vector3.Zero, Vector3.UnitY);

            // Model matrix
            _model = Matrix4.Identity;

            // Projection
            _projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(60f),
                Size.X / (float)Size.Y,
                0.1f,
                100f
            );

            // Load shaders
            ShaderManager.Load(
                "solid_color",
                new Dictionary<ShaderType, string>
                {
                    { ShaderType.VertexShader, "./Shaders/Projection3D.vert" },
                    { ShaderType.FragmentShader, "./Shaders/Light3D.frag" }
                }
            );

            _time = Stopwatch.StartNew();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Increment rotation (radians)
            float rotationAngle = (float)_time.Elapsed.TotalSeconds;
            _model = Matrix4.CreateRotationY(rotationAngle);

            Shader shader = ShaderManager.Get("solid_color");
            shader.Use();

            shader.SetMatrix4("uModel", _model);
            shader.SetMatrix4("uView", _view);
            shader.SetMatrix4("uProjection", _projection);
            shader.SetFloat("uTime", (float)_time.Elapsed.TotalSeconds);

            shader.SetVector4("uColor", new Vector4(1f, 1f, 1f, 1f));
            shader.SetVector3("uLightDir", new Vector3(1f, 1f, 1f).Normalized());
            shader.SetFloat("uGlobalLight", 0.1f);

            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }
    }
}
