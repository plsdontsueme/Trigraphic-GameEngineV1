

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

        public void RenderAllElements()
        {
            GraphicsCore.UseShaderProgram(_handle, this);
            Matrix4 viewMatrix = Camera.MainCamera(cameraSlot).GetViewMatrix();
            Matrix4 projectionMatrix = Camera.MainCamera(cameraSlot).GetProjectionMatrix();
            GraphicsCore.SetViewMatrix(ref viewMatrix);
            GraphicsCore.SetProjectionMatrix(ref projectionMatrix);
            foreach (ElementRenderer renderer in _elementRenderers)
                renderer.RenderElement();
        }
        #endregion

        #region constructor logic
        readonly int _handle;
        Dictionary<string, int> _uniformLocations;
        public bool GetUniform(string name) => _uniformLocations.ContainsKey(name);
        public int GetUniformLocation(string name) => _uniformLocations[name];
        public int cameraSlot;


        public Shader(string shaderDirectory, int usedCameraSlot = 0)
        {
            cameraSlot = usedCameraSlot;
            GraphicsCore.CreateShaderProgram(shaderDirectory, out _handle);
            GraphicsCore.GetShaderProgramUniforms(_handle, out _uniformLocations);
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
                GraphicsCore.DeleteShaderProgram(_handle);

                _disposed = true;
            }
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
        }
        #endregion
    }
}
