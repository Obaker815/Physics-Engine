using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Physics_Engine
{
    internal class RenderWindow : GameWindow
    {
        private readonly List<SceneObject> _objects = [];
        private CameraController _camera;

        public RenderWindow()
            : base(GameWindowSettings.Default,
                   new NativeWindowSettings()
                   {
                       ClientSize = new Vector2i(800, 600),
                       Title = "Textured + Lit Renderer"
                   })
        {
            _camera = null!;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);
            UpdateCamera();
        }

        protected override void OnLoad()
        {
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1f);

            float aspectRatio = Size.X / (float)Size.Y;
            // Camera
            Matrix4 cameraTransform =
                Matrix4.CreateTranslation(3f, 2f, 2f);

            _camera = new CameraController(cameraTransform, aspectRatio);

            WindowState = WindowState.Maximized;
            CursorState = CursorState.Grabbed;

            // Load shaders
            ShaderManager.Load(
                "textured_lit",
                new Dictionary<ShaderType, string>
                {
                    { ShaderType.VertexShader, "./Shaders/Projection3D.vert" },
                    { ShaderType.FragmentShader, "./Shaders/TexturedLit3D.frag" }
                }
            );

            // Load model(s)
            Model model = new(@"./Models/Fish/Model.obj", 1);
            Mesh modelMesh = new(model.Vertices, model.Indices);

            Vector3 position = new(0, 0, 0);

            _objects.Add(new SceneObject(
                modelMesh,
                Matrix4.CreateTranslation(position),
                TextureLoader.UploadTexture(model.Texture)
            ));

            Global.StartTimers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            Global.Update();
            Global.MouseDelta = MouseState.Delta;

            _camera.Update();

            while(Global.Deltatimer.Elapsed.TotalMilliseconds < 1000.0 / Global.PhysicsRateCap) { }

            base.OnUpdateFrame(args);
        }   

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            this.Title = $"Textured + Lit Renderer - FPS: {1f / e.Time:0000} - TPS: {1f / Global.Deltatime:00}";

            Shader shader = ShaderManager.Get("textured_lit");
            shader.Use();

            shader.SetMatrix4("uView", Matrix4.Invert(_camera.Transform));
            shader.SetMatrix4("uProjection", _camera.ProjectionMatrix);

            shader.SetVector3("uLightDir", -new Vector3(1, 1, 1).Normalized());
            shader.SetFloat("uAmbient", 0.1f);

            float rot = Global.Elapsedtime * 0.25f;

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

        private void UpdateCamera()
        {
            float aspectRatio = Size.X / (float)Size.Y;
            _camera.AspectRatio = aspectRatio;
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            PlayerController.UpdateKeybindings(e.Key, true);
            base.OnKeyDown(e);
        }
        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            PlayerController.UpdateKeybindings(e.Key, false);
            base.OnKeyDown(e);
        }
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            foreach (var (key, _) in Global.MouseButtonStates)
                if (e.Button == key)
                {
                    Global.MouseButtonStates[key] = true;
                }

            base.OnMouseDown(e);
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            foreach (var (key, _) in Global.MouseButtonStates)
                if (e.Button == key)
                {
                    Global.MouseButtonStates[key] = false;
                }

            base.OnMouseDown(e);
        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            Global.MouseDeltaScroll += e.OffsetY;
            base.OnMouseWheel(e);
        }
    }
}
