

using OpenTK.Mathematics;

namespace Trigraphic_GameEngineV1
{
    internal sealed class Shader : IDisposable
    {
        public static readonly Shader DEFAULT = new("...//..//..//..//..//Rsc//Common//Shaders//DefaultShader");

        #region renderer logic
        List<ElementRenderer> _elementRenderers = new();
        public void AddElementRenderer(ElementRenderer renderer)
        {
            if (renderer.material.shader != this)
                throw new InvalidOperationException("added renderer doesnt match shader");
            if (_elementRenderers.Contains(renderer))
                throw new InvalidOperationException("renderer already added");
            _elementRenderers.Add(renderer);
        }
        public void RemoveElementRenderer(ElementRenderer renderer)
        {
            if (!_elementRenderers.Contains(renderer))
                throw new InvalidOperationException("renderer already removed");
            _elementRenderers.Remove(renderer);
        }

        public void RenderAllElements(EnvironmentMaterial environment)
        {
            Camera? camera = Camera.MainCamera(CameraSlot);
            if (camera == null) throw new NullReferenceException("chosen camera slot not populated");

            _shaderProgram.UseProgram();
            _shaderProgram.ApplyEnvironmentMaterial(camera, environment);
            foreach (ElementRenderer renderer in _elementRenderers)
            {
                _shaderProgram.ApplyModelTransform(renderer);
                _shaderProgram.ApplyMaterial(renderer.material);
                renderer.RenderElement();
            }          
        }
        #endregion

        #region constructor logic
        public int CameraSlot;

        GraphicsCore.ShaderProgram _shaderProgram;
        public Shader(string shaderDirectory, int usedCameraSlot = 0)
        {
            CameraSlot = usedCameraSlot;
            _shaderProgram = new(shaderDirectory);
        }
        #endregion

        #region IDisposable Support
        bool _disposed = false;
        void _Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //-free managed resources (if any)
                    _elementRenderers.Clear();
                }

                //-free unmanaged resources
                _shaderProgram.Dispose();

                _disposed = true;
            }
            else EngineDebugManager.throwNewOperationRedundancyWarning("dispose wal already called");
            EngineDebugManager.Send("dispose called");
        }
        public void Dispose()
        {
            _Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~Shader()
        {
            if (_disposed == false)
            {
                throw new Exception("GPU Resource leak - Dispose wasnt called 0o0");
            }
            EngineDebugManager.Send("finalizer called");
        }
        #endregion
    }
}
