using OpenTK.Mathematics;
using System.Drawing;
using System.IO;
using System.Collections.Generic;

namespace Physics_Engine
{
    internal class Model
    {
        private static readonly Random random = new Random();

        private Bitmap _texture;
        private float _scale;

        // Raw OBJ data
        private List<Vector3> _positions = new();
        private List<Vector3> _normals = new();
        private List<Vector2> _uvs = new();
        private List<(int pos, int uv, int norm)[]> _faces = new();

        // Expanded arrays for OpenGL
        private List<float> _vertices = new();
        private List<uint> _indices = new();

        public float[] Vertices => _vertices.ToArray();
        public uint[] Indices => _indices.ToArray();
        public Bitmap Texture => _texture;

        public Model(string path, float scale, Bitmap? texture = null!)
        {
            _scale = scale;
            _texture = texture ?? GenerateDefaultTexture();

            LoadOBJ(path);
            ExpandVertices();
        }

        private Bitmap GenerateDefaultTexture()
        {
            int resolution = 100;
            int divisions = 8;
            Bitmap bmp = new(divisions * resolution, divisions * resolution);

            // Color col1 = Color.FromArgb(
            //     random.Next(0, 255), 
            //     random.Next(0, 255), 
            //     random.Next(0, 255), 
            //     255);
            // Color col2 = Color.FromArgb(
            //     random.Next(0, 255), 
            //     random.Next(0, 255), 
            //     random.Next(0, 255), 
            //     255);

            Color col1 = Color.DarkGreen;
            Color col2 = Color.ForestGreen;

            for (int i = 0; i < bmp.Width; i++)
                for (int j = 0; j < bmp.Height; j++)
                    if (i / resolution % 2 != j / resolution % 2) bmp.SetPixel(i, j, col1);
                    else bmp.SetPixel(i, j, col2);

            return bmp;
        }

        private void LoadOBJ(string path)
        {
            string[] lines = File.ReadAllLines(path);

            foreach (string line in lines)
            {
                string l = line.Trim();
                if (string.IsNullOrEmpty(l) || l.StartsWith("#")) continue;

                string[] parts = l.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts[0] == "v") // position
                {
                    _positions.Add(new Vector3(
                        float.Parse(parts[1]) * _scale,
                        float.Parse(parts[2]) * _scale,
                        float.Parse(parts[3]) * _scale
                    ));
                }
                else if (parts[0] == "vn") // normal
                {
                    _normals.Add(new Vector3(
                        float.Parse(parts[1]),
                        float.Parse(parts[2]),
                        float.Parse(parts[3])
                    ));
                }
                else if (parts[0] == "vt") // uv
                {
                    _uvs.Add(new Vector2(
                        float.Parse(parts[1]),
                        float.Parse(parts[2])
                    ));
                }
                else if (parts[0] == "f") // face
                {
                    var face = new List<(int pos, int uv, int norm)>();

                    for (int i = 1; i < parts.Length; i++)
                    {
                        string[] indices = parts[i].Split('/');
                        int pos = int.Parse(indices[0]) - 1;
                        int uv = (indices.Length > 1 && !string.IsNullOrEmpty(indices[1])) ? int.Parse(indices[1]) - 1 : -1;
                        int norm = (indices.Length > 2) ? int.Parse(indices[2]) - 1 : -1;

                        face.Add((pos, uv, norm));
                    }

                    _faces.Add(face.ToArray());
                }
            }
        }

        private void ExpandVertices()
        {

            Dictionary<Vertex, uint> vertexMap = new();
            uint indexCounter = 0;

            foreach (var face in _faces)
            {
                // Triangulate face if more than 3 vertices (fan method)
                for (int i = 1; i<face.Length - 1; i++)
                {
                    var tri = new[] { face[0], face[i], face[i + 1] };

                    foreach (var f in tri)
                    {
                        Vector3 pos = _positions[f.pos];
                        Vector2 uv = (f.uv >= 0 && f.uv < _uvs.Count) ? _uvs[f.uv] : Vector2.Zero;
                        Vector3 norm = (f.norm >= 0 && f.norm < _normals.Count) ? _normals[f.norm] : Vector3.UnitY;

                        Vertex vert = new() { Position = pos, UV = uv, Normal = norm };

                        if (!vertexMap.TryGetValue(vert, out uint index))
                        {
                            index = indexCounter++;
                            vertexMap[vert] = index;

                            _vertices.Add(pos.X);
                            _vertices.Add(pos.Y);
                            _vertices.Add(pos.Z);

                            _vertices.Add(norm.X);
                            _vertices.Add(norm.Y);
                            _vertices.Add(norm.Z);

                            _vertices.Add(uv.X);
                            _vertices.Add(uv.Y);
                        }

                        _indices.Add(index);
                    }
                }
            }
        }
    }
}
