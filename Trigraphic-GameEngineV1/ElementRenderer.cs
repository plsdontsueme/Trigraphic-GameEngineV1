using OpenTK.Mathematics;

namespace Trigraphic_GameEngineV1
{
    internal abstract class ElementRenderer : ComponentStatic
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
                    RenderSystem.RemoveElement(this);
                    _material = value;
                    RenderSystem.AddElement(this);
                }
                else
                {
                    _material = value;
                }
            }
        }

        protected ElementRenderer(Material? material = null)
        {
            _material = material ?? Material.Static.DEFAULT;
        }

        protected override void OnLoad()
        {
            RenderSystem.AddElement(this);
        }
        protected override void OnUnload()
        {
            RenderSystem.RemoveElement(this);
        }

        public abstract void RenderElement();
    }
}
