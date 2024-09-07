

using OpenTK.Mathematics;

namespace Trigraphic_GameEngineV1
{
    internal sealed class Shader : IDisposable
    {
        public static class Static
        {
            public static readonly Shader LIT =
                new("...//..//..//..//..//Rsc//Common//Shaders//DefaultShader");
            public static readonly Shader UNLIT =
                new("...//..//..//..//..//Rsc//Common//Shaders//DefaultShaderUnlit");
            public static readonly Shader UNLIT_SINGLECOLOR =
                new Shader("...//..//..//..//..//Rsc//Common//Shaders//LightShader");
        }

        #region constructor logic
        public readonly GraphicsCore.ShaderProgram ShaderProgram;
        public Shader(string shaderDirectory)
        {
            ShaderProgram = new(shaderDirectory);
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
                }

                //-free unmanaged resources
                ShaderProgram.Dispose();

                _disposed = true;
            }
            else DebugManager.throwNewOperationRedundancyWarning("dispose wal already called");
            DebugManager.Send("dispose called");
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
            DebugManager.Send("finalizer called");
        }
        #endregion
    }
}
