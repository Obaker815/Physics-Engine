using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Physics_Engine
{
    public class Shader : IDisposable
    {
        public int Handle { get; }

        private readonly Dictionary<string, int> _uniformLocations = [];

        public Shader(Dictionary<ShaderType, string> stages)
        {
            // Create a OpenGL4 program handle
            Handle = GL.CreateProgram();
            List<int> compiledShaders = new();

            // for each shader in stages, compile and attach to the Handle
            foreach (var (type, source) in stages)
            {
                int shader = Compile(type, source);
                GL.AttachShader(Handle, shader);
                compiledShaders.Add(shader);
            }

            // Link the program handle, and check it's link status
            GL.LinkProgram(Handle);
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
                throw new Exception(GL.GetProgramInfoLog(Handle));

            // Detach and delete the shaders
            foreach (int s in compiledShaders)
            {
                GL.DetachShader(Handle, s);
                GL.DeleteShader(s);
            }

            GL.UseProgram(Handle);
            CacheUniforms();
            GL.UseProgram(0);
        }

        private static int Compile(ShaderType type, string source)
        {
            if (!File.Exists(source))
                throw new FileNotFoundException($"Shader source not found at: {source}");

            // Load the shader from the shader source
            string src = File.ReadAllText(source);
            src = src.TrimStart('\uFEFF', '\u200B', '\u0000', ' ', '\t', '\r', '\n');

            // Create the shader handle
            int shader = GL.CreateShader(type);

            // Link and compile the shader 
            GL.ShaderSource(shader, src);
            GL.CompileShader(shader);

            // Throw any shader compile errors
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
                throw new Exception($"{type}: {GL.GetShaderInfoLog(shader)}");

            return shader;
        }

        private void CacheUniforms()
        {
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out int count);

            for (int i = 0; i < count; i++)
            {
                string name = GL.GetActiveUniform(Handle, i, out _, out _);
                _uniformLocations[name] = GL.GetUniformLocation(Handle, name);
            }
        }

        private int GetLocation(string name)
        {
            if (_uniformLocations.TryGetValue(name, out int loc))
                return loc;

            throw new Exception($"Uniform '{name}' not found.");
        }

        public void SetMatrix4(string name, Matrix4 value) { GL.UniformMatrix4(GetLocation(name), false, ref value); }
        public void SetVector4(string name, Vector4 value) { GL.Uniform4(GetLocation(name), value); }
        public void SetVector3(string name, Vector3 value) { GL.Uniform3(GetLocation(name), value); }
        public void SetVector2(string name, Vector2 value) { GL.Uniform2(GetLocation(name), value); }
        public void SetFloat(string name, float value) { GL.Uniform1(GetLocation(name), value); }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public void Dispatch(int x, int y = 1, int z = 1)
        {
            GL.DispatchCompute(x, y, z);
        }

        public void Dispose()
        {
            GL.DeleteProgram(Handle);
        }
    }
}
