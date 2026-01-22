using OpenTK.Mathematics; 
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys; 

namespace Physics_Engine
{
    internal struct Keybind(Keys key, Vector3 direction)
    {
        public Keys Key = key;
        public Vector3 Direction = direction;
        public bool IsActive = false;
    }
}
