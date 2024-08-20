﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trigraphic_GameEngineV1
{
    internal class MeshRenderer : ElementRenderer
    {
        public Mesh mesh;
        public MeshRenderer()
        {
            this.mesh = Mesh.PRIMITIVE_CUBE;
        }
        public MeshRenderer(Mesh mesh)
        {
            this.mesh = mesh;
        }
        public MeshRenderer(Mesh mesh, Material material) : base(material)
        {
            this.mesh = mesh;
        }

        public override void RenderElement()
        {
            base.RenderElement();
            mesh.Draw();
        }
    }
}
