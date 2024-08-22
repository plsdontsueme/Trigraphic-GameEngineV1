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

        public Color4 color = Color4.White;
        public Texture diffuse = Texture.DEFAULT;
        public Texture specular = Texture.DEFAULT;
        public float shininess = 32f;
    }
}
