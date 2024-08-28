using OpenTK.Mathematics;

namespace Trigraphic_GameEngineV1
{
    internal class Material
    {
        public static readonly Material DEFAULT =
            new Material(
                ResourceManager.DEFAULT_SHADER_LIT,
                diffuseMap: new Texture("...//..//..//..//..//Rsc//Common//Textures//uvcheck1k.jpg"),
                specularMap: new Texture("...//..//..//..//..//Rsc//Common//Textures//white.png")
                );

        public Material(Shader shader, Color4? color = null, Texture? diffuseMap = null, Texture? specularMap = null, float shininess = 8f)
        {
            this.shader = shader;

            Color = color ?? Color4.White;
            DiffuseMap = diffuseMap ?? DEFAULT.DiffuseMap;
            SpecularMap = specularMap ?? DEFAULT.SpecularMap;
            Shininess = shininess > 0 ? shininess : DEFAULT.Shininess;
        }

        public readonly Shader shader;

        public Color4 Color;
        public Texture DiffuseMap;
        public Texture SpecularMap;
        public float Shininess;
    }
}