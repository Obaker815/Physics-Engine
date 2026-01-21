using OpenTK.Mathematics;

namespace Physics_Engine
{
    struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 UV;

        public override bool Equals(object? obj)
        {
            return obj is Vertex v &&
                   Position == v.Position &&
                   Normal == v.Normal &&
                   UV == v.UV;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position, Normal, UV);
        }
    }
}
