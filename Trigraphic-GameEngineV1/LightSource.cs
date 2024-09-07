

using OpenTK.Mathematics;
using System.Reflection.Metadata;

namespace Trigraphic_GameEngineV1
{
    internal class LightSource : MeshRenderer
    {
        protected LightSource()
            : base(Mesh.Static.LIGHTBULB, new Material(Shader.Static.UNLIT_SINGLECOLOR))
        { }

        int _range;
        public int Range
        {
            get => _range;
            set
            {
                _range = value;
                switch (_range)
                {
                    case <= 7:
                        Constant = 1.0f;
                        Linear = 0.7f;
                        Quadratic = 1.8f;
                        break;
                    case <= 13:
                        Constant = 1.0f;
                        Linear = 0.35f;
                        Quadratic = 0.44f;
                        break;
                    case <= 20:
                        Constant = 1.0f;
                        Linear = 0.22f;
                        Quadratic = 0.20f;
                        break;
                    case <= 32:
                        Constant = 1.0f;
                        Linear = 0.14f;
                        Quadratic = 0.07f;
                        break;
                    case <= 50:
                        Constant = 1.0f;
                        Linear = 0.09f;
                        Quadratic = 0.032f;
                        break;
                    case <= 65:
                        Constant = 1.0f;
                        Linear = 0.07f;
                        Quadratic = 0.017f;
                        break;
                    case <= 100:
                        Constant = 1.0f;
                        Linear = 0.045f;
                        Quadratic = 0.0075f;
                        break;
                    case <= 160:
                        Constant = 1.0f;
                        Linear = 0.027f;
                        Quadratic = 0.0028f;
                        break;
                    case <= 200:
                        Constant = 1.0f;
                        Linear = 0.022f;
                        Quadratic = 0.0019f;
                        break;
                    case <= 325:
                        Constant = 1.0f;
                        Linear = 0.014f;
                        Quadratic = 0.0007f;
                        break;
                    case <= 600:
                        Constant = 1.0f;
                        Linear = 0.007f;
                        Quadratic = 0.0002f;
                        break;
                    case <= 3250:
                        Constant = 1.0f;
                        Linear = 0.0014f;
                        Quadratic = 0.000007f;
                        break;
                    default:
                        throw new ArgumentException("Distance not supported");
                }
            }
        }

        public float Constant { get; private set; }
        public float Linear { get; private set; }
        public float Quadratic { get; private set; }

        public Vector3 Ambient { get; protected set; }
        public Vector3 Diffuse { get; protected set; }
        public Vector3 Specular { get; protected set; }
    }
}
