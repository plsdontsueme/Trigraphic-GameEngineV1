using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Trigraphic_GameEngineV1
{
    internal sealed class UICanvas : ComponentStatic
    {
        Shader _shader;
        Material _material;

        public UICanvas(Shader? shader = null)
        {
            _shader = shader ?? Shader.Static.UNLIT;
            _material = new Material(_shader);
        }

        List<UIElement> _elements = new();

        public void AddElement(UIElement element)
        {
            if (_elements.Contains(element)) throw new InvalidOperationException("element already in list");
            _elements.Add(element);
        }
        public void RemoveElement(UIElement element)
        {
            if (!_elements.Remove(element)) throw new InvalidOperationException("element not in list");
        }

        public void RenderCanvas()
        {
            var shaderProgram = _shader.ShaderProgram;
            shaderProgram.UseProgram();
            shaderProgram.ApplyCamera(SceneManager.UICamera);
            foreach (var element in _elements)
            {
                if (!element.GetRenderParameters(out var color, out var texture)) continue;
                _material.Color = color.Value;
                _material.DiffuseMap = texture;

                shaderProgram.ApplyMaterial(_material);
                shaderProgram.ApplyModelMatrix(ref element.GetModelTransformRef());
                element.Render();
            }
        }

        private void _InputManager_MouseUp(OpenTK.Windowing.Common.MouseButtonEventArgs obj)
        {
            if (InputManager.CursorGrabbed) return;
            var mouse = InputManager.MousePositionCameraSpace(SceneManager.UICamera);
            foreach (var element in _elements)
            {
                element.OnMouseClick(mouse);
            }
        }

        protected override void OnLoad()
        {
            RenderSystem.AddCanvas(this);
            InputManager.MouseUp += _InputManager_MouseUp;
        }
        protected override void OnUnload()
        {
            RenderSystem.RemoveCanvas(this);
            InputManager.MouseUp -= _InputManager_MouseUp;
        }
    }
}
