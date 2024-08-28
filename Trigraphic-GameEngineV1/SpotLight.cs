using OpenTK.Mathematics;
using System.Collections.ObjectModel;


namespace Trigraphic_GameEngineV1
{
    internal class SpotLight : LightSource
    {
        static readonly List<SpotLight> _spotLights = new List<SpotLight>();
        public static readonly ReadOnlyCollection<SpotLight> SpotLights =
            new ReadOnlyCollection<SpotLight>(_spotLights);

        public SpotLight(Color4? color = null)
        {
            Color = color ?? Color4.White;
            Range = 32;
            ConeAngleDeg = 12.5f;
            SmoothingAngleDeg = 5f;
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            _spotLights.Add(this);
        }
        protected override void OnUnload()
        {
            base.OnUnload();
            _spotLights.Remove(this);
        }

        Color4 _color;
        public Color4 Color
        {
            get => _color;
            set
            {
                _color = value;
                var baseColor = new Vector3(_color.R, _color.G, _color.B) * _color.A;
                Ambient = baseColor * 0.05f;
                Diffuse = baseColor * 1;
                Specular = baseColor * .5f;
                material.Color = _color;
            }
        }

        float _coneAngleDeg;
        public float ConeAngleDeg
        {
            get => _coneAngleDeg;
            set
            {
                _coneAngleDeg = value;
                Phi_cutoff = MathF.Cos(MathHelper.DegreesToRadians(value));
            }
        }
        float _smoothingAngleDeg;
        public float SmoothingAngleDeg
        {
            get => _smoothingAngleDeg;
            set
            {
                _smoothingAngleDeg = value;
                Gamma_outercutoff = Phi_cutoff - MathF.Sin(MathHelper.DegreesToRadians(value));
            }
        }

        public float Phi_cutoff { get; private set; }
        public float Gamma_outercutoff { get; private set; } //phi - epsilon
    }
}