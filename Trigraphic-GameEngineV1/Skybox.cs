using OpenTK.Mathematics;

namespace Trigraphic_GameEngineV1
{
    internal sealed class Skybox
    {
        public Skybox(Color4? dirLightColor = null)
        {
            DirLightColor = dirLightColor ?? Color4.White;
        }

        Color4 _color;
        public Color4 DirLightColor
        {
            get => _color;
            set
            {
                _color = value;
                var baseColor = new Vector3(_color.R, _color.G, _color.B) * _color.A;
                Ambient = baseColor * .1f;
                Diffuse = baseColor;
                Specular = baseColor * .3f;
            }
        }

        public Vector3 DirLightDirection = (.81f, -.72f, -.9f);
        public Vector3 Ambient { get; private set; }
        public Vector3 Diffuse { get; private set; }
        public Vector3 Specular { get; private set; }
    }
}
