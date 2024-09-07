using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trigraphic_GameEngineV1
{
    internal sealed class Mesh : IDisposable
    {
        public static class Static
        {
            public static readonly Mesh QUAD =
                ImportTgxm("...//..//..//..//..//Rsc//Common//Primitives//Quad.tgxm");
            public static readonly Mesh CUBE =
                ImportTgxm("...//..//..//..//..//Rsc//Common//Primitives//Cube.tgxm");
            public static readonly Mesh SPHERE =
                ImportTgxm("...//..//..//..//..//Rsc//Common//Primitives//Sphere.tgxm");
            public static readonly Mesh CONE =
                ImportTgxm("...//..//..//..//..//Rsc//Common//Primitives//Cone.tgxm");
            public static readonly Mesh LIGHTBULB =
                ImportTgxm("...//..//..//..//..//Rsc//Common//Primitives//LightBulb.tgxm");

            public static Mesh ImportTgxm(string filePath)
            {
                if (!new FileInfo(filePath).Extension.Equals(".tgxm"))
                    throw new ArgumentException("file is not of the TGXM-Format");

                var byteData = File.ReadAllBytes(filePath);

                int offset = 0;

                int vertexElementCount = BitConverter.ToInt32(byteData, offset);
                offset += sizeof(int);
                int indexCount = BitConverter.ToInt32(byteData, offset);
                offset += sizeof(int);

                float[] vertexData = new float[vertexElementCount];
                Buffer.BlockCopy(byteData, offset, vertexData, 0, vertexElementCount * sizeof(float));
                offset += vertexElementCount * sizeof(float);

                uint[] indexData = new uint[indexCount];
                Buffer.BlockCopy(byteData, offset, indexData, 0, indexCount * sizeof(uint));

                return new Mesh(vertexData, indexData);
            }
        }

        int vao, vbo, ebo, indexLength;

        public Mesh(float[] vertexData, uint[] indexData)
        {
            indexLength = indexData.Length;
            GraphicsCore.CreateMeshBuffer(vertexData, indexData, out vbo, out vao, out ebo);
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
            else DebugManager.throwNewOperationRedundancyWarning("dispose wal already called");
            DebugManager.Send("dispose called");
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
            DebugManager.Send("finalizer called");
        }
        #endregion
    }
}
