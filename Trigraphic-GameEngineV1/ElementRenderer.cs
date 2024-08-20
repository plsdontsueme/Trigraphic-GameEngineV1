using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public ElementRenderer()
        {
            material = Material.DEFAULT;
        }
        public ElementRenderer(Material material)
        {
            this.material = material;
        }

        public override void OnLoad()
        {
            base.OnLoad();
            material?.shader.AddElementRenderer(this);
        }
        public override void OnUnload()
        {
            base.OnUnload();
            material?.shader.RemoveElementRenderer(this);
        }

        public virtual void RenderElement()
        {
            Matrix4 modelMatrix = gameObject.ModelMatrix;
            GraphicsCore.SetModelMatrix(ref modelMatrix);
            GraphicsCore.ApplyMaterial(material);
            Console.WriteLine("render element");
        }
    }
}
