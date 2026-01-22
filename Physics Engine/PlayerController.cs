using OpenTK.Mathematics; 
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys; 

namespace Physics_Engine
{
    internal class PlayerController
    {
        // Static property to hold all controllers for keybinding updates
        private static List<PlayerController> Controllers = [];
        public static void UpdateKeybindings(Keys keyPress, bool down)
        {
            foreach (var controller in Controllers)
                foreach (var (key, value) in controller.KeyStates)
                    if (value.Key == keyPress)
                    {
                        Keybind kb = value;
                        kb.IsActive = down;
                        controller.KeyStates[key] = kb;
                    }
        }

        // Instance property to hold key states for this controller
        public Dictionary<string, Keybind>  KeyStates => _keyStates;
        internal Dictionary<string, Keybind> _keyStates = new()
        {
            { "Forward",  new(Keys.W, new Vector3( 0, 0, -1)) },
            { "Backward", new(Keys.S, new Vector3( 0, 0,  1)) },
            { "Left",     new(Keys.A, new Vector3(-1, 0,  0)) },
            { "Right",    new(Keys.D, new Vector3( 1, 0,  0)) },
        };
        public Matrix4 Transform => _transform;
        internal Matrix4 _transform;
        internal float
            _drag = 0.001f,
            _pitch,
            _yaw;

        internal float _accelleration = 50f;
        internal Vector3 _velocity;

        // Constructors
        public PlayerController(Matrix4 transform) 
        { 
            Controllers.Add(this);

            _transform = transform;
            
            var euler = -Matrix4.LookAt(transform.ExtractTranslation(), new Vector3(0, transform.ExtractTranslation().Y, 0), Vector3.UnitY).ExtractRotation().ToEulerAngles();
            _pitch = euler.X;
            _yaw = euler.Y;
        }
        public PlayerController(Vector3 position, Vector3 lookAt, Vector3 upDirection)
            : this(Matrix4.LookAt(position, lookAt, upDirection).Inverted()) { }
        public PlayerController(Vector3 position, Vector3 lookAt) 
            : this(position, lookAt, Vector3.UnitY) { }

        public virtual void Update()
        {
            // Apply rotation to transform
            Quaternion rotation = Quaternion.FromAxisAngle(Vector3.UnitY, _yaw);

            float accel = _accelleration * Global.Deltatime;

            foreach (var (key, value) in _keyStates)
                if (value.IsActive)
                    _velocity += Vector3.Transform(value.Direction, rotation) * accel;

            Vector3 pos = _transform.ExtractTranslation() + _velocity * Global.Deltatime;
            _transform = Matrix4.CreateFromQuaternion(_transform.ExtractRotation()) * Matrix4.CreateTranslation(pos);

            _velocity *= float.Pow(_drag, Global.Deltatime);

        }
    }
}
