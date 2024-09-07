using StbImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trigraphic_GameEngineV1
{
    internal class Texture
    {
        public readonly int Handle;
        public readonly int ImageWidth, ImageHeight;
        
        static ImageResult _ImportImage(string path)
        {
            //-stb loads from top-left pixel, opengl loads from  bottom-left)
            StbImage.stbi_set_flip_vertically_on_load(1);
            return ImageResult.FromStream(File.OpenRead(path), ColorComponents.RedGreenBlueAlpha);
        }
        public Texture(string filename, bool smoothe = true) : this(_ImportImage(filename), smoothe) { }
        public Texture(ImageResult image, bool smoothe = true)
        {
            ImageWidth = image.Width;
            ImageHeight = image.Height;

            GraphicsCore.CreateTextureBuffer(image, out Handle, smoothe);
        }
    }
}