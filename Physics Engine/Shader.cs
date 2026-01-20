using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Physics_Engine
{
    internal class Shader
    {
        // The shader handle
        public int Handle { get; }

        // The locations of the active uniforms
        private Dictionary<string, int> _uniformLocations = [];

        public Shader(string vertexSource, string fragmentSource)
        {
            int vertex = CompileShader(ShaderType.VertexShader, vertexSource);
            int fragment = CompileShader(ShaderType.FragmentShader, fragmentSource);

            // Attach the shaders to the program
            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertex);
            GL.AttachShader(Handle, fragment);
            GL.LinkProgram(Handle);

            // Log the program info log if the link was unsucessful
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
                throw new Exception(GL.GetProgramInfoLog(Handle));

            // Detach and delete the shaders, they are no longer needed
            GL.DetachShader(Handle, vertex);
            GL.DetachShader(Handle, fragment);
            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);

            CacheUniforms();
        }

        public static int CompileShader(ShaderType shaderType, string source)
        {
            // Create the shader
            int shader = GL.CreateShader(shaderType);
            // Link the shader source
            GL.ShaderSource(shader, source);
            // Compile the shader
            GL.CompileShader(shader);

            // Get the compile status of the shader
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            // Throw the errors
            if (success == 0)
                throw new Exception(GL.GetShaderInfoLog(shader));

            return shader;
        }

        private void CacheUniforms()
        {
            // Get the count of active uniforms
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out int count);

            for (int i = 0; i < count; i++)
            {
                // Cache the locaion of a uniform
                string name = GL.GetActiveUniform(Handle, i, out _, out _);
                int locaion = GL.GetUniformLocation(Handle, name);
                _uniformLocations[name] = locaion;
            }
        }
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public int GetLocation(string name)
        {
            return _uniformLocations[name];
        }

        public void SetMatrix4(string name, Matrix4 value)
        {
            GL.UniformMatrix4(GetLocation(name), false, ref value);
        }

        public void SetVector4(string name, Vector4 value)
        {
            GL.Uniform4(GetLocation(name), value);
        }

        public void Dispose()
        {
            GL.DeleteProgram(Handle);
        }
    }
}
