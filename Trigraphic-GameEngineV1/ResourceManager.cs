using OpenTK.Mathematics;
using StbImageSharp;

namespace Trigraphic_GameEngineV1
{
    internal static class ResourceManager
    {
        public static readonly Shader DEFAULT_SHADER_LIT =
            new("...//..//..//..//..//Rsc//Common//Shaders//DefaultShader");
        public static readonly Shader DEFAULT_SHADER_UNLIT =
            new("...//..//..//..//..//Rsc//Common//Shaders//DefaultShaderUnlit");
        public static readonly Shader DEFAULT_SHADER_LIGHTSOURCE =
            new Shader("...//..//..//..//..//Rsc//Common//Shaders//LightShader");

        public static readonly Font DEFAULT_FONT = new Font(
                "...//..//..//..//..//Rsc//Common//Fonts//arial.ttf",
                ResourceManager.DEFAULT_SHADER_UNLIT,
                80,
                Font.CharacterRange.BasicLatin, Font.CharacterRange.Latin1Supplement
                )
        {
        };

        public static readonly Mesh PRIMITIVE_QUAD =
            ImportTgxmMesh("...//..//..//..//..//Rsc//Common//Primitives//Quad.tgxm");
        public static readonly Mesh PRIMITIVE_CUBE =
            ImportTgxmMesh("...//..//..//..//..//Rsc//Common//Primitives//Cube.tgxm");
        public static readonly Mesh PRIMITIVE_SPHERE =
            ImportTgxmMesh("...//..//..//..//..//Rsc//Common//Primitives//Sphere.tgxm");
        public static readonly Mesh PRIMITIVE_CONE =
            ImportTgxmMesh("...//..//..//..//..//Rsc//Common//Primitives//Cone.tgxm");

        public static readonly Mesh UTILITYMESH_LIGHTSOURCE =
            ImportTgxmMesh("...//..//..//..//..//Rsc//Common//Primitives//LightBulb.tgxm");



        static Mesh ImportTgxmMesh(string filePath)
        {
            if (!new FileInfo(filePath).Extension.Equals(".tgxm"))
                throw new ArgumentException("file is not of the TGXM-Format");

            var byteData = File.ReadAllBytes(filePath);

            int offset = 0;

            int vertexElementCount = BitConverter.ToInt32(byteData, offset);
            offset += sizeof(int);
            int indexCount = BitConverter.ToInt32(byteData, offset);
            offset += sizeof(int);

            float[] vertexData = new float[vertexElementCount];
            Buffer.BlockCopy(byteData, offset, vertexData, 0, vertexElementCount * sizeof(float));
            offset += vertexElementCount * sizeof(float);

            uint[] indexData = new uint[indexCount];
            Buffer.BlockCopy(byteData, offset, indexData, 0, indexCount * sizeof(uint));

            return new Mesh(vertexData, indexData);
        }

        public static GameObject ImportTgxPrefab(string filePath, Shader shader)
        {
            var rootObjects = ImportTgxPrefabScene(filePath, shader);
            if (rootObjects.Count == 1) return rootObjects[0];
            else
            {
                GameObject root = GameObject.CreatePrefab();
                foreach (var child in rootObjects)
                {
                    child.Parent = root;
                }
                return root;
            }
        }
        static List<GameObject> ImportTgxPrefabScene(string filePath, Shader shader)
        {
            if (!new FileInfo(filePath).Extension.Equals(".tgx"))
                throw new ArgumentException("file is not of the TGX-Format");

            filePath = Path.Combine("...//..//..//..//..//Rsc//TGX_Files", filePath);
            var byteData = File.ReadAllBytes(filePath);

            int offset = 0;

            #region read functions
            bool ReadBool()
            {
                bool value = BitConverter.ToBoolean(byteData, offset);
                offset += sizeof(bool);
                return value;
            }
            int ReadInt()
            {
                int value = BitConverter.ToInt32(byteData, offset);
                offset += sizeof(int);
                return value;
            }
            float ReadFloat()
            {
                float value = BitConverter.ToSingle(byteData, offset);
                offset += sizeof(float);
                return value;
            }
            Vector3 ReadVector()
            {
                float x = ReadFloat();
                float y = ReadFloat();
                float z = ReadFloat();
                return new Vector3(x, y, z);
            }
            Quaternion ReadQuaternion()
            {
                float w = ReadFloat();
                float x = ReadFloat();
                float y = ReadFloat();
                float z = ReadFloat();
                return new Quaternion(x, y, z, w);
            }
            Texture? ReadImage()
            {
                bool hasImage = ReadBool();
                if (!hasImage) return null;

                int width = ReadInt();
                int height = ReadInt();
                int length = ReadInt();

                byte[] imageData = new byte[length];
                Array.Copy(byteData, offset, imageData, 0, length);
                offset += length;

                return new Texture (new ImageResult
                {
                    Width = width,
                    Height = height,
                    Data = imageData
                });
            }
            #endregion

            // Begin reading data
            float versionNumber = ReadFloat();
            if (versionNumber != 1.0f)
                throw new InvalidDataException("incompatible version of the TGX-Format");

            int materialCount = ReadInt();
            int meshCount = ReadInt();
            int nodeCount = ReadInt();

            Material[] materials = new Material[materialCount];
            Mesh[] meshes = new Mesh[meshCount];
            int[] meshMaterialIndices = new int[meshCount];
            GameObject[] objects = new GameObject[nodeCount];
            List<GameObject> rootObjects = new List<GameObject>();

            for (int i = 0; i < materialCount; i++)
            {
                var diffuseMap = ReadImage();
                var specularMap = ReadImage();
                float shininess = ReadFloat();

                materials[i] = new Material(
                    shader,
                    null,
                    diffuseMap ?? null,
                    specularMap ?? null,
                    shininess
                    );
            }

            for (int i = 0; i < meshCount; i++)
            {
                int materialIndex = ReadInt();
                int vertexElementCount = ReadInt();
                int indexCount = ReadInt();

                float[] vertexData = new float[vertexElementCount];
                Buffer.BlockCopy(byteData, offset, vertexData, 0, vertexElementCount * sizeof(float));
                offset += vertexElementCount * sizeof(float);

                uint[] indexData = new uint[indexCount];
                Buffer.BlockCopy(byteData, offset, indexData, 0, indexCount * sizeof(uint));
                offset += indexCount * sizeof(uint);

                meshes[i] = new Mesh(vertexData, indexData);
                meshMaterialIndices[i] = materialIndex;
            }

            for (int i = 0; i < nodeCount; i++)
            {
                bool hasParent = ReadBool();
                GameObject? parent;

                if (hasParent)
                {
                    int parentIndex = ReadInt();
                    parent = objects[parentIndex];
                }
                else parent = null;

                Vector3 position = ReadVector();
                Quaternion rotation = ReadQuaternion();
                Vector3 scale = ReadVector();

                int attachedMeshesCount = ReadInt();
                MeshRenderer[] meshRenderers = new MeshRenderer[attachedMeshesCount];

                for (int mi = 0; mi < attachedMeshesCount; mi++)
                {
                    int meshIndex = ReadInt();
                    meshRenderers[mi] = new MeshRenderer(
                        meshes[meshIndex],
                        materials[meshMaterialIndices[meshIndex]]
                        );
                }

                GameObject @object = GameObject.CreatePrefab(meshRenderers);
                @object.Position = position;
                @object.Rotation = rotation;
                @object.Scale = scale;
                if (parent == null) rootObjects.Add(@object);
                else @object.Parent = parent;
                objects[i] = @object;
            }

            return rootObjects;
        }

    }
}