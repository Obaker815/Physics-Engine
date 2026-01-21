using OpenTK.Mathematics;

internal class SceneObject
{
    public Mesh Mesh;
    public Matrix4 Transform;
    public int TextureID;

    public SceneObject(Mesh mesh, Matrix4 transform, int texID)
    {
        Mesh = mesh;
        Transform = transform;
        TextureID = texID;
    }
}
