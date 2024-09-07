using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Trigraphic_GameEngineV1
{
    internal sealed class UIImage : UIElement
    {
        public Texture Image;
        public Color4 Color = Color4.White;
        public UIImage(string path, bool smoothe = true) : this(new Texture(path, smoothe)) { }
        public UIImage(Texture image)
        {
            Image = image;
            Size = ((float)Image.ImageWidth / Image.ImageHeight, 1, 1);
        }

        public override bool GetRenderParameters(out Color4? color, out Texture? texture)
        {
            color = Color;
            texture = Image;
            return true;
        }

        public override void OnMouseClick(Vector2 mouse)
        {
            Position = new Vector3(mouse);
        }

        public override void Render()
        {
            Mesh.Static.QUAD.Draw();
        }
    }
}