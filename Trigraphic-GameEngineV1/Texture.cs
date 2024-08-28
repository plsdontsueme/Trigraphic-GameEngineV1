using StbImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trigraphic_GameEngineV1
{
    internal sealed class Texture
    {
        public readonly int Handle;
        public readonly int ImageWidth, ImageHeight;
        public Texture(string filename)
        {
            //-stb loads from top-left pixel, opengl loads from  bottom-left)
            StbImage.stbi_set_flip_vertically_on_load(1);
            var image = ImageResult.FromStream(File.OpenRead(filename), ColorComponents.RedGreenBlueAlpha);

            ImageWidth = image.Width;
            ImageHeight = image.Height;

            GraphicsCore.CreateTextureBuffer(image, out Handle);
        }
        public Texture(ImageResult image)
        {
            ImageWidth = image.Width;
            ImageHeight = image.Height;

            GraphicsCore.CreateTextureBuffer(image, out Handle);
        }
    }
}