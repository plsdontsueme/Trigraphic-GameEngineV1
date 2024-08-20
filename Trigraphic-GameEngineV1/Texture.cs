using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trigraphic_GameEngineV1
{
    internal sealed class Texture
    {
        public static readonly Texture DEFAULT = new("...//..//..//..//..//Rsc//Common//Textures//tile//diffuse.jpg");

        int _handle, _width, _height;
        public Texture(string filename)
        {
            GraphicsCore.CreateTextureBuffer(filename, out _handle, out _width, out _height);
        }

        public void Apply(OpenTK.Graphics.OpenGL4.TextureUnit textureUnit)
        {
            GraphicsCore.ApplyTexture(_handle, textureUnit);
        }
    }
}
