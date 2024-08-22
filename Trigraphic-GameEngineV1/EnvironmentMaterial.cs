using OpenTK.Mathematics;

namespace Trigraphic_GameEngineV1
{
    internal sealed class EnvironmentMaterial
    {
        public Vector3 DirectionalLightDirection = (1, -1, -1);
        public Vector3 AmbientColor = (0.1f, 0.1f, 0.1f);
        public Vector3 DiffuseColor = (0.5f, 0.5f, 0.5f);
        public Vector3 SpecularColor = (1.0f, 1.0f, 1.0f);
    }
}
