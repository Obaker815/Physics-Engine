using OpenTK.Graphics.OpenGL4;

namespace Physics_Engine
{
    public static class ShaderManager
    {
        private static readonly Dictionary<string, Shader> _shaders = new();

        public static Shader Load(string name, Dictionary<ShaderType, string> stages)
        {
            if (_shaders.TryGetValue(name, out Shader? shader))
                return shader;

            shader = new Shader(stages);
            _shaders[name] = shader;
            return shader;
        }

        public static Shader Get(string name)
        {
            return _shaders[name];
        }

        public static void DisposeAll()
        {
            foreach (var shader in _shaders.Values)
                shader.Dispose();

            _shaders.Clear();
        }
    }
}
