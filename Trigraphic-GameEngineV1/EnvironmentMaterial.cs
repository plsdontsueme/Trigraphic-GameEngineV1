using OpenTK.Mathematics;

namespace Trigraphic_GameEngineV1
{
    internal sealed class EnvironmentMaterial
    {
        public Vector3 DirectionalLightDirection = (1, -1, -1);
        public Vector3 AmbientColor = (.2f, .2f, .2f);
        public Vector3 DiffuseColor = (1f, 1f, 1f);
        public Vector3 SpecularColor = (1f, 1f, 1f);
    }
}
