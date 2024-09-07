using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Trigraphic_GameEngineV1
{
    internal abstract class UIElement
    {
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Size { get; set; } = Vector3.One;

        Matrix4 _modelMatrix;
        public ref Matrix4 GetModelTransformRef()
        {
            _modelMatrix = Matrix4.CreateScale(Size) * Matrix4.CreateTranslation(Position);
            return ref _modelMatrix;
        }

        public abstract bool GetRenderParameters(out Color4? color, out Texture? texture);
        public abstract void Render();
        public abstract void OnMouseClick(Vector2 mouse);
    }
}
