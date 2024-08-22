using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trigraphic_GameEngineV1
{
    internal sealed class Mesh : IDisposable
    {
        public static readonly Mesh PRIMITIVE_CUBE = new("...//..//..//..//..//Rsc//Common//Primitives//Cube.tgd");

        int vao, vbo, ebo, indexLength;

        public Mesh(string filePath)
        {
            ImportTgd(filePath, out float[] vertexData, out uint[] indexData);
            indexLength = indexData.Length;
            GraphicsCore.CreateMeshBuffer(vertexData, indexData, out vbo, out vao, out ebo);
        }

        static void ImportTgd(string filename, out float[] vertexData, out uint[] indexData)
        {
            byte[] data = File.ReadAllBytes(filename);
            vertexData = new float[BitConverter.ToInt32(data, 0)];
            indexData = new uint[BitConverter.ToInt32(data, 4)];

            int vertByteLength = vertexData.Length * 4;
            System.Buffer.BlockCopy(data, 8, vertexData, 0, vertByteLength);
            System.Buffer.BlockCopy(data, 8 + vertByteLength, indexData, 0, indexData.Length * 4);
        }

        public void Draw()
        {
            GraphicsCore.DrawVao(vao, indexLength);
        }

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
                GraphicsCore.DeleteMeshBuffer(vbo, vao, ebo);

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
        ~Mesh()
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
