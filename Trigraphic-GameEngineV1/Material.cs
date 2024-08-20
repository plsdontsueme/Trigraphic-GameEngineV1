using OpenTK.Mathematics;

namespace Trigraphic_GameEngineV1
{
    internal sealed class Material
    {
        public static readonly Material DEFAULT = new(Shader.DEFAULT);

        public Material(Shader shader)
        {
            this.shader = shader;
        }

        public readonly Shader shader;
        public Texture diffuse = Texture.DEFAULT;
        public Vector3 color = Vector3.One;
    }
}
