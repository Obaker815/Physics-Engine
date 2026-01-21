using OpenTK.Mathematics; 
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys; 

namespace Physics_Engine
{
    internal class PlayerController
    {
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

        public  Dictionary<string, Keybind>  KeyStates => _keyStates;
        internal Dictionary<string, Keybind> _keyStates = new()
        {
            { "Forward",  new(Keys.W, new Vector3( 0, 0,  1)) },
            { "Backward", new(Keys.S, new Vector3( 0, 0, -1)) },
            { "Left",     new(Keys.A, new Vector3(-1, 0,  0)) },
            { "Right",    new(Keys.D, new Vector3( 1, 0,  0)) },
        };

        internal float _accelleration = 5f;
        internal Matrix4 _transform;
        internal Vector3 _velocity;

        public Matrix4 Transform => _transform;

        public PlayerController(Matrix4 transform) 
        { 
            _transform = transform;
            Controllers.Add(this);
        }
        public PlayerController(Vector3 position, Vector3 lookAt, Vector3 upDirection)
            : this(Matrix4.LookAt(position, lookAt, upDirection).Inverted()) { }
        public PlayerController(Vector3 position, Vector3 lookAt) 
            : this(position, lookAt, Vector3.UnitY) { }

        public virtual void Update()
        {
            void Accellerate(Vector3 dir, float accel)
            { _velocity += dir * accel * Global.Deltatime; }

            Quaternion rotation = _transform.ExtractRotation();

            foreach (var (key, value) in _keyStates)
                if (value.IsActive) Accellerate(Vector3.Transform(value.Direction, rotation), _accelleration);
        }
    }
}
