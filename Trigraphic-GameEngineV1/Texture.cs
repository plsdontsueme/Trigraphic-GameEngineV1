using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trigraphic_GameEngineV1
{
    internal sealed class Texture
    {
        public static readonly Texture DEFAULT = new("...//..//..//..//..//Rsc//Common//Textures//uvcheck1k.jpg");

        public readonly int Handle;
        int _width, _height;
        public Texture(string filename)
        {
            GraphicsCore.CreateTextureBuffer(filename, out Handle, out _width, out _height);
        }
    }
}
