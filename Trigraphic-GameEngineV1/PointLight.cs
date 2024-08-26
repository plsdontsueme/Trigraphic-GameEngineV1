using OpenTK.Mathematics;
using System.Collections.ObjectModel;

namespace Trigraphic_GameEngineV1
{
    internal sealed class PointLight : LightSource
    {
        static readonly List<PointLight> _pointLights = new List<PointLight>();
        public static readonly ReadOnlyCollection<PointLight> PointLights = 
            new ReadOnlyCollection<PointLight>(_pointLights);

        public PointLight(Color4? color = null)
        {
            Color = color ?? Color4.White;
            Range = 20;
        }

        Color4 _color;
        public Color4 Color
        {
            get => _color;
            set
            {
                _color = value;
                var baseColor = new Vector3(_color.R, _color.G, _color.B) * _color.A;
                Ambient = baseColor * 0.1f;
                Diffuse = baseColor * 1;
                Specular = baseColor * 1;
                material.Color = _color;
            }
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            _pointLights.Add(this);
        }
        protected override void OnUnload()
        {
            base.OnUnload();
            _pointLights.Remove(this);
        }
    }
}
