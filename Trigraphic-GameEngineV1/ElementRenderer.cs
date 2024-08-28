using OpenTK.Mathematics;

namespace Trigraphic_GameEngineV1
{
    internal class ElementRenderer : Component
    {
        Material _material;
        public Material material
        {
            get => _material;
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (_isLoaded)
                {
                    _material?.shader.RemoveElementRenderer(this);
                    _material = value;
                    _material.shader.AddElementRenderer(this);
                }
                else
                {
                    _material = value;
                }
            }
        }

        protected ElementRenderer()
        {
            _material = Material.DEFAULT;
        }
        protected ElementRenderer(Material material)
        {
            _material = material;
        }

        protected override void OnLoad()
        {
            material?.shader.AddElementRenderer(this);
        }
        protected override void OnUnload()
        {
            material?.shader.RemoveElementRenderer(this);
        }

        public virtual void RenderElement()
        {
            EngineDebugManager.Send("render element");
        }
    }
}
