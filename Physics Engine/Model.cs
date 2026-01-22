using OpenTK.Mathematics;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace Physics_Engine
{
    internal class Model
    {
        private static readonly Random random = new();

        private readonly float _scale;

        // Raw OBJ data
        private readonly List<Vector3> _positions = new();
        private readonly List<Vector3> _normals = new();
        private readonly List<Vector2> _uvs = new();

        // Faces grouped by material
        private readonly Dictionary<string, List<(int pos, int uv, int norm)[]>> _facesByMaterial = new();

        // Texture cache
        private readonly Dictionary<string, int> _textureCache = new();

        // Public: material name -> SceneObject
        public Dictionary<string, SceneObject> SceneObjects { get; } = new();

        public Model(string objPath, float scale = 1f, Image? texture = null!)
        {
            _scale = scale;

            if (texture != null)
            {
                SceneObjects["default"] = new SceneObject(
                    new Mesh(Array.Empty<float>(), Array.Empty<uint>()),
                    Matrix4.Identity,
                    TextureLoader.UploadTexture(texture)
                );
            }
            else
            {
                SceneObjects["default"] = new SceneObject(
                    new Mesh(Array.Empty<float>(), Array.Empty<uint>()),
                    Matrix4.Identity,
                    TextureLoader.UploadTexture(GenerateDefaultTexture())
                );
            }

            ParseOBJ(objPath);
            foreach (var kvp in _facesByMaterial)
            {
                var so = BuildSceneObject(kvp.Key, kvp.Value);
                SceneObjects[kvp.Key] = so;
            }
        }

        /// <summary>
        /// New constructor: objDirectory = folder containing .obj, scale, texturesDirectory
        /// </summary>
        public Model(string objDirectory, float scale, string texturesDirectory)
        {
            _scale = scale;

            // Find the OBJ file (first .obj in folder)
            string[] objFiles = Directory.GetFiles(objDirectory, "*.obj");
            if (objFiles.Length == 0)
                throw new FileNotFoundException("No OBJ file found in directory: " + objDirectory);

            string objPath = objFiles[0];

            // Parse MTL if it exists
            string mtlPath = Path.Combine(objDirectory, Path.GetFileNameWithoutExtension(objPath) + ".mtl");
            if (File.Exists(mtlPath))
                ParseMTL(mtlPath, texturesDirectory);

            // Parse OBJ and group faces per material
            ParseOBJ(objPath);

            // Build SceneObjects dictionary
            foreach (var kvp in _facesByMaterial)
            {
                SceneObjects[kvp.Key] = BuildSceneObject(kvp.Key, kvp.Value, texturesDirectory);
            }

            // Ensure at least one default SceneObject exists
            if (SceneObjects.Count == 0)
            {
                int texID = TextureLoader.UploadTexture(GenerateDefaultTexture());
                SceneObjects["default"] = new SceneObject(new Mesh(Array.Empty<float>(), Array.Empty<uint>()), Matrix4.Identity, texID);
            }
        }

        private void ParseMTL(string mtlPath, string texturesFolder)
        {
            string? currentMaterial = null;

            foreach (string line in File.ReadAllLines(mtlPath))
            {
                string l = line.Trim();
                if (string.IsNullOrEmpty(l) || l.StartsWith("#")) continue;

                string[] parts = l.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

                if (parts[0] == "newmtl")
                {
                    currentMaterial = parts[1];
                }
                else if (parts[0] == "map_Kd" && currentMaterial != null)
                {
                    // Convert the .tga reference in the MTL to .png in the textures folder
                    string textureFile = Path.Combine(texturesFolder, Path.GetFileNameWithoutExtension(parts[1]) + ".png");

                    if (File.Exists(textureFile))
                    {
                        _textureCache[currentMaterial] = TextureLoader.UploadTexture(Image.FromFile(textureFile));
                        Debug.WriteLine($"Loaded texture for {currentMaterial}: {textureFile} -> {_textureCache[currentMaterial]}");
                    }
                    else
                    {
                        Debug.WriteLine($"Texture not found for {currentMaterial}: {textureFile}");
                    }
                }
            }
        }

        private void ParseOBJ(string objPath)
        {
            string currentMaterial = "default";

            foreach (string line in File.ReadAllLines(objPath))
            {
                string l = line.Trim();
                if (string.IsNullOrEmpty(l) || l.StartsWith("#")) continue;

                string[] parts = l.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                switch (parts[0])
                {
                    case "v":
                        _positions.Add(new Vector3(
                            float.Parse(parts[1]) * _scale,
                            float.Parse(parts[2]) * _scale,
                            float.Parse(parts[3]) * _scale));
                        break;

                    case "vn":
                        _normals.Add(new Vector3(
                            float.Parse(parts[1]),
                            float.Parse(parts[2]),
                            float.Parse(parts[3])));
                        break;

                    case "vt":
                        _uvs.Add(new Vector2(
                            float.Parse(parts[1]),
                            float.Parse(parts[2])));
                        break;

                    case "usemtl":
                        currentMaterial = parts[1];
                        break;

                    case "f":
                        if (!_facesByMaterial.ContainsKey(currentMaterial))
                            _facesByMaterial[currentMaterial] = new List<(int pos, int uv, int norm)[]>();

                        var face = new List<(int pos, int uv, int norm)>();
                        for (int i = 1; i < parts.Length; i++)
                        {
                            string[] indices = parts[i].Split('/');
                            int pos = int.Parse(indices[0]) - 1;
                            int uv = (indices.Length > 1 && !string.IsNullOrEmpty(indices[1])) ? int.Parse(indices[1]) - 1 : -1;
                            int norm = (indices.Length > 2) ? int.Parse(indices[2]) - 1 : -1;
                            face.Add((pos, uv, norm));
                        }
                        _facesByMaterial[currentMaterial].Add(face.ToArray());
                        break;
                }
            }
        }

        private SceneObject BuildSceneObject(string materialName, List<(int pos, int uv, int norm)[]> faces, string? texturesFolder = null)
        {
            List<float> vertices = new();
            List<uint> indices = new();
            Dictionary<string, uint> vertexMap = new();
            uint indexCounter = 0;

            foreach (var face in faces)
            {
                for (int i = 1; i < face.Length - 1; i++)
                {
                    var tri = new[] { face[0], face[i], face[i + 1] };

                    foreach (var f in tri)
                    {
                        Vector3 pos = _positions[f.pos];
                        Vector2 uv = (f.uv >= 0 && f.uv < _uvs.Count) ? _uvs[f.uv] : Vector2.Zero;
                        Vector3 norm = (f.norm >= 0 && f.norm < _normals.Count) ? _normals[f.norm] : Vector3.UnitY;

                        string key = $"{pos.X},{pos.Y},{pos.Z},{norm.X},{norm.Y},{norm.Z},{uv.X},{uv.Y}";
                        if (!vertexMap.TryGetValue(key, out uint index))
                        {
                            index = indexCounter++;
                            vertexMap[key] = index;

                            vertices.Add(pos.X); vertices.Add(pos.Y); vertices.Add(pos.Z);
                            vertices.Add(norm.X); vertices.Add(norm.Y); vertices.Add(norm.Z);
                            vertices.Add(uv.X); vertices.Add(uv.Y);
                        }

                        indices.Add(index);
                    }
                }
            }

            Mesh mesh = new(vertices.ToArray(), indices.ToArray());

            // Texture
            int textureID;
            if (_textureCache.TryGetValue(materialName, out int cachedTex))
                textureID = cachedTex;
            else
                textureID = TextureLoader.UploadTexture(GenerateDefaultTexture());

            return new SceneObject(mesh, Matrix4.Identity, textureID);
        }

        private static Image GenerateDefaultTexture()
        {
            int resolution = 100;
            int divisions = 8;
            Bitmap bmp = new(divisions * resolution, divisions * resolution);

            Color col1 = Color.FromArgb(
                random.Next(0, 255), 
                random.Next(0, 255), 
                random.Next(0, 255), 
                255);
            Color col2 = Color.FromArgb(
                random.Next(0, 255), 
                random.Next(0, 255), 
                random.Next(0, 255), 
                255);

            for (int i = 0; i < bmp.Width; i++)
                for (int j = 0; j < bmp.Height; j++)
                    if (i / resolution % 2 != j / resolution % 2) bmp.SetPixel(i, j, col1);
                    else bmp.SetPixel(i, j, col2);

            return bmp;
        }
    }
}
