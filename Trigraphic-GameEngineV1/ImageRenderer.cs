using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trigraphic_GameEngineV1
{
    internal sealed class ImageRenderer : ElementRenderer
    {
        public ImageRenderer(string path, Shader shader) 
            : base(new Material(shader, diffuseMap: new Texture(Path.Combine("...//..//..//..//..//Rsc//Images", path))))
        { }

        protected override void OnLoad()
        {
            base.OnLoad();
            gameObject.Dimensions = ((float)material.DiffuseMap.ImageWidth / material.DiffuseMap.ImageHeight, 1, 1);
        }

        public override void RenderElement()
        {
            base.RenderElement();
            ResourceManager.PRIMITIVE_QUAD.Draw();
        }
    }
}