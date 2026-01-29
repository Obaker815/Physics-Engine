using OpenTK.Mathematics;

namespace Physics_Engine
{
    internal class SceneObject(Mesh mesh, Matrix4 transform, int texID)
    {
        public Mesh Mesh = mesh;
        public Matrix4 Transform = transform;
        public int TextureID = texID;
    }
}
