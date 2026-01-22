using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Diagnostics;

namespace Physics_Engine
{
    internal class RenderWindow : GameWindow
    {
        private Stopwatch _time = new();

        private Matrix4 _projection;
        private Matrix4 _view;

        private List<SceneObject> _objects = new();
        private int _texture;

        public RenderWindow()
            : base(GameWindowSettings.Default,
                   new NativeWindowSettings()
                   {
                       ClientSize = new Vector2i(800, 600),
                       Title = "Textured + Lit Renderer"
                   })
        { }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);
            float aspectRatio = Size.X / (float)Size.Y;

            // Camera
            _view = Matrix4.LookAt(
                new Vector3(5, 5, 5),
                new Vector3(0, 5, 0),
                Vector3.UnitY
            );

            _projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(105f),
                aspectRatio,
                0.1f,
                100f
            );
        }

        protected override void OnLoad()
        {
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1f);


            // Load shader (texture + lighting combined)
            ShaderManager.Load(
                "textured_lit",
                new Dictionary<ShaderType, string>
                {
                    { ShaderType.VertexShader, "./Shaders/Projection3D.vert" },
                    { ShaderType.FragmentShader, "./Shaders/TexturedLit3D.frag" }
                }
            );

            // Load model
            Model model = new(@"C:\Users\Obaker815\Downloads\Springtrap\Model.obj", 0.05f);
            Mesh modelMesh = new(model.Vertices, model.Indices);

            for (int i = 0; i < 50; i++)
            {
                Vector3 position = new(
                    float.Lerp(-5f, 5f, i / 100),
                    float.Lerp(-5f, 5f, i / 100),
                    0
                    );

                _objects.Add(new SceneObject(
                    modelMesh,
                    Matrix4.CreateTranslation(position),
                    TextureLoader.UploadTexture(model.Texture)
                ));
            }

            _time.Start();
        }

        private Queue<double> FrameTime = [];
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            FrameTime.Enqueue((int)(e.Time * 1000));
            if (FrameTime.Count > 100)
            {
                double averageFps = 1000 / (FrameTime.Average());
                Title = $"Textured + Lit Renderer - FPS: {averageFps}";
                FrameTime.Clear();
            }

            float rot = (float)_time.Elapsed.TotalSeconds * 0.25f;

            Shader shader = ShaderManager.Get("textured_lit");
            shader.Use();

            shader.SetMatrix4("uView", _view);
            shader.SetMatrix4("uProjection", _projection);

            shader.SetVector3("uLightDir", new Vector3(1, 1, 1).Normalized());
            shader.SetVector3("uLightColor", new Vector3(1, 1, 1));
            shader.SetFloat("uAmbient", 0.1f);


            foreach (var obj in _objects)
            {
                Matrix4 model =
                    Matrix4.CreateRotationY(rot) *
                    obj.Transform;

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, obj.TextureID);
                shader.SetInt("uTexture", 0);

                shader.SetMatrix4("uModel", model);
                obj.Mesh.Draw();
            }

            SwapBuffers();
        }
    }
}
