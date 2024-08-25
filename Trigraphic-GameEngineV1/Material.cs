using OpenTK.Mathematics;

namespace Trigraphic_GameEngineV1
{
    internal sealed class Material
    {
        public static readonly Material DEFAULT = new(Shader.DEFAULT)
        {
            Color = Color4.White,
            DiffuseMap = new Texture("...//..//..//..//..//Rsc//Common//Textures//uvcheck1k.jpg"),
            SpecularMap = new Texture("...//..//..//..//..//Rsc//Common//Textures//default_specular.png"),
            Shininess = 8f,
        };

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
