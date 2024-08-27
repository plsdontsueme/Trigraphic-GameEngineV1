using OpenTK.Mathematics;

namespace Trigraphic_GameEngineV1
{
    internal class Material
    {
        public Material(Shader shader)
        {
            this.shader = shader;
        }

        public readonly Shader shader;

        public Color4 Color;
        public Texture DiffuseMap;
        public Texture SpecularMap;
        public float Shininess;
    }
}
