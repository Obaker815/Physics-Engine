using OpenTK.Mathematics;
using System.Dynamic;
using System.Reflection;

namespace Physics_Engine
{
    internal class Model
    {
        private Matrix4 _transform;
        private Image _texture;

        private List<Vector3> _vertices;
        private List<Vector3> _normals;
        private List<Vector2> _uvs;
        private List<(int, int, int)> _triangles;

        public Model(string path, Matrix4? transform = null!, Image? texture = null!)
        {
            LoadOBJ(path);
            _transform = transform ?? new Matrix4();
            _texture = texture ?? GenDefaultTexture();
        }

        private Image GenDefaultTexture()
        {
            Bitmap bmp = new(4, 4);
            for (int i = 0; i < bmp.Width; i++)
                for(int j = 0; j < bmp.Height; j++)
                {
                    if (i % 2 != j % 2)
                        bmp.SetPixel(i, j, Color.Black);
                    else
                        bmp.SetPixel(i, j, Color.Purple);
                }

            return bmp;
        }

        private void LoadOBJ(string path)
        {
            string[] objText = File.ReadAllLines(path);

            for (int i = 0; i < objText.Length; i++)
            {
                // Get Line
                string line = objText[i];
                if (line.ToLower().StartsWith("v ")) // Vertex position
                {
                    line = line[2..]; // Trim the "v "

                    // Split into three numbers and make a vector
                    string[] coords = line.Split(' ');
                    Vector3 vertex = new(
                        Convert.ToSingle(coords[0]),
                        Convert.ToSingle(coords[1]),
                        Convert.ToSingle(coords[2])
                        );

                    // Add to _vertices
                    _vertices.Add(vertex);
                }
                else if (line.ToLower().StartsWith("vn ")) // Vertex normal
                {
                    line = line[3..];

                    // Split into three numbers and make a vector
                    string[] direction = line.Split(' ');
                    Vector3 normal = new(
                        Convert.ToSingle(direction[0]),
                        Convert.ToSingle(direction[1]),
                        Convert.ToSingle(direction[2])
                        );

                    // Add to _vertices
                    _normals.Add(normal);
                }
                else if (line.ToLower().StartsWith("vt ")) // Vertex texture uv
                {
                    line = line[3..];

                    // Split into three numbers and make a vector
                    string[] coords = line.Split(' ');
                    Vector2 uv = new(
                        Convert.ToSingle(coords[0]),
                        Convert.ToSingle(coords[1])
                        );

                    // Add to _vertices
                    _uvs.Add(uv);
                }
                else if (line.ToLower().StartsWith("f ")) // Face
                {
                    line = line[2..];

                    string[] tris = line.Split(" ");
                    for (int j = 0; j < tris.Length; j++)
                    {
                        string tri = tris[j];
                        string[] indecies = tri.Split("/");

                        void ProcessTri(int x, int y, int z) { _triangles.Add((x, y, z)); }

                        for (int k = 0; k < indecies.Length - 2; k++)
                        {
                            ProcessTri(
                                Convert.ToInt32(indecies[k]),
                                Convert.ToInt32(indecies[k + 1]),
                                Convert.ToInt32(indecies[k + 2])
                                );
                        }
                    }
                }
            }
        }

        public (Vector3 pos, Vector3 normal, Vector2 uv) GetPoint(int index)
        { return (
                _vertices[index],
                _normals[index],
                _uvs[index]
                );}
    }
}
