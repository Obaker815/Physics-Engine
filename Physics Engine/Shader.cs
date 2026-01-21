using OpenTK.Graphics.OpenGL4;
public class Shader : IDisposable
{
    public int Handle { get; }

    private readonly Dictionary<string, int> _uniformLocations = new();

    public Shader(Dictionary<ShaderType, string> stages)
    {
        Handle = GL.CreateProgram();
        List<int> compiledShaders = new();

        foreach (var (type, source) in stages)
        {
            int shader = Compile(type, source);
            GL.AttachShader(Handle, shader);
            compiledShaders.Add(shader);
        }

        GL.LinkProgram(Handle);
        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
            throw new Exception(GL.GetProgramInfoLog(Handle));

        foreach (int s in compiledShaders)
        {
            GL.DetachShader(Handle, s);
            GL.DeleteShader(s);
        }

        CacheUniforms();
    }

    private static int Compile(ShaderType type, string source)
    {
        if (!File.Exists(source))
            throw new FileNotFoundException($"Shader source not found at: {source}");

        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

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
