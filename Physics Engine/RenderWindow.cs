using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Diagnostics;

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
            _camera.NearClip = 0.001f;

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
            Vector3 position = new(0, 0, 0);
            Matrix4 transform = Matrix4.CreateFromAxisAngle(Vector3.UnitX, MathHelper.DegToRad * -90) * Matrix4.CreateTranslation(position);

            Model model = new(
                @"C:\Users\Obaker815\Downloads\de_dust2-cs-map\source\",
                @"C:\Users\Obaker815\Downloads\de_dust2-cs-map\textures\",
                transform,
                new Vector3(0.01f, 0.01f, 0.01f));

            int yPos = 0;
            int yoffset = 6;

            foreach (var (_, obj) in model.SceneObjects)
            {
                obj.Transform *= Matrix4.CreateTranslation(new(0, yPos, 0));

                _objects.Add(obj);
                yPos += yoffset;
            }
            
            position = new(-7, 1.3f, 7);
            transform = Matrix4.CreateTranslation(position);

            model = new(
                @"C:\Users\Obaker815\Downloads\Springtrap\Model.obj",
                new Vector3(0.005f, 0.005f, 0.005f),
                transform);

            yPos = 0;

            foreach (var (_, obj) in model.SceneObjects)
            {
                obj.Transform *= Matrix4.CreateTranslation(new(0, yPos, 0));

                _objects.Add(obj);
                yPos += yoffset;
            }

            Global.StartTimers();
        }

        int _lastFramerateUpdate = -1;
        int _numframes = 0;
        int _framerate = 0;
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            Global.MouseDelta = MouseState.Delta;

            _camera.Update();

            if (Global.FramerateCap != 0)
                while(Global.Deltatimer.Elapsed.TotalMilliseconds < 1000.0 / Global.FramerateCap) { }

            base.OnUpdateFrame(args);
            Global.Update();
        }   

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _numframes++;
            this.Title = $"Textured + Lit Renderer " +
                $"- FOV: {MathHelper.RadiansToDegrees(_camera.FOV):000} " +
                $"- FPS: {_framerate} " +
                $"- CameraPos: {_camera.Transform.ExtractTranslation()} " +
                $"- CameraRotation: {_camera.Transform.ExtractRotation()}";

            if (double.Floor(Global.Elapsedtime) > _lastFramerateUpdate)
            {
                _framerate = _numframes;
                _numframes = 0;
                _lastFramerateUpdate = (int)double.Floor(Global.Elapsedtime);
            }

            Shader shader = ShaderManager.Get("textured_lit");
            shader.Use();

            shader.SetMatrix4("uView", Matrix4.Invert(_camera.Transform));
            shader.SetMatrix4("uProjection", _camera.ProjectionMatrix);

            shader.SetVector3("uLightDir", -new Vector3(3, 3, 2).Normalized());
            shader.SetVector3("uLightColor", new Vector3(1, 1, 1));
            shader.SetFloat("uAmbient", 0.2f);

            foreach (var obj in _objects)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, obj.TextureID);
                shader.SetInt("uTexture", 0);

                shader.SetMatrix4("uModel", obj.Transform);
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
