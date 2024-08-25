using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;

namespace Trigraphic_GameEngineV1
{
    internal static class GraphicsCore
    {
        public static void SetRenderParameters(
            Color4 clearColor,
            bool cullFace = true,
            bool depthTest = true,
            bool multisample = true,
            bool blend = true)
        {
            GL.ClearColor(clearColor);
            if (cullFace) GL.Enable(EnableCap.CullFace);
            if (depthTest) GL.Enable(EnableCap.DepthTest);
            if (multisample) GL.Enable(EnableCap.Multisample); //AntiAlising (neccessary?, see no difference)
            if (blend)
            {
                GL.Enable(EnableCap.Blend); // may disable for regular 3d rendering
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            }
        }

        public sealed class ShaderProgram : IDisposable
        {
            static class _UniformConvention
            {
                public const string MATERIAL_COLOR = "material.color";
                public const string MATERIAL_DIFFUSE = "material.diffuse";
                public const string MATERIAL_SPECULAR = "material.specular";
                public const string MATERIAL_SHININESS = "material.shininess";
                public const string ENVIRONMENT_LIGHT_DIRECTION = "environmentLighting.direction";
                public const string ENVIRONMENT_LIGHT_AMBIENT = "environmentLighting.ambient";
                public const string ENVIRONMENT_LIGHT_DIFFUSE = "environmentLighting.diffuse";
                public const string ENVIRONMENT_LIGHT_SPECULAR = "environmentLighting.specular";
                public const string ENVIRONMENT_VIEWPOS = "viewPos";
                //-the three matrices are not checked for their implementation
                //-as they are mandatory for the engines shader format
                public const string MATRIX_VIEW = "view";
                public const string MATRIX_MODEL = "model";
                public const string MATRIX_PROJECTION = "projection";
            }

            int _handle;
            Dictionary<string, int> _uniforms;
            public ShaderProgram(string shaderDirectory)
            {
                #region buffer creation / loading
                string VertexShaderSource = File.ReadAllText(Path.Combine(shaderDirectory, "vertex.glsl"));
                string FragmentShaderSource = File.ReadAllText(Path.Combine(shaderDirectory, "fragment.glsl"));

                int VertexShader = GL.CreateShader(ShaderType.VertexShader);
                GL.ShaderSource(VertexShader, VertexShaderSource);

                int FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
                GL.ShaderSource(FragmentShader, FragmentShaderSource);

                GL.CompileShader(VertexShader);

                GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out int successV);
                if (successV == 0)
                {
                    string infoLog = GL.GetShaderInfoLog(VertexShader);
                    throw new InvalidDataException($"Error compiling Vertex Shader: {Environment.NewLine}{infoLog}");
                }

                GL.CompileShader(FragmentShader);

                GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out int successF);
                if (successF == 0)
                {
                    string infoLog = GL.GetShaderInfoLog(FragmentShader);
                    throw new InvalidDataException($"Error compiling Fragment Shader: {Environment.NewLine}{infoLog}");
                }

                _handle = GL.CreateProgram();

                GL.AttachShader(_handle, VertexShader);
                GL.AttachShader(_handle, FragmentShader);

                GL.LinkProgram(_handle);

                GL.GetProgram(_handle, GetProgramParameterName.LinkStatus, out int success);
                if (success == 0)
                {
                    string infoLog = GL.GetProgramInfoLog(_handle);
                    throw new Exception($"Error compiling Shader Program: {Environment.NewLine}{infoLog}");
                }

                //-cleanup
                GL.DetachShader(_handle, VertexShader);
                GL.DetachShader(_handle, FragmentShader);
                GL.DeleteShader(FragmentShader);
                GL.DeleteShader(VertexShader);
                #endregion

                #region uniform management
                _uniforms = new Dictionary<string, int>();
                GL.GetProgram(_handle, GetProgramParameterName.ActiveUniforms, out int uniformCount);
                for (int i = 0; i < uniformCount; i++)
                {
                    string key = GL.GetActiveUniform(_handle, i, out _, out _);
                    int location = GL.GetUniformLocation(_handle, key);
                    _uniforms.Add(key, location);
                }

                foreach (var key in _uniforms.Keys)
                {
                    EngineDebugManager.Send(key + " -- " + _uniforms[key]);
                }

                //GL.UseProgram(_handle);
                if (_uniforms.ContainsKey(_UniformConvention.MATERIAL_DIFFUSE))
                    GL.Uniform1(_uniforms[_UniformConvention.MATERIAL_DIFFUSE], 0);
                if (_uniforms.ContainsKey(_UniformConvention.MATERIAL_SPECULAR))
                    GL.Uniform1(_uniforms[_UniformConvention.MATERIAL_SPECULAR], 1);
                #endregion
            }

            static ShaderProgram? _usedShader;

            public void UseProgram()
            {
                GL.UseProgram(_handle);
                _usedShader = this;
            }
            public void ApplyEnvironmentMaterial(Camera camera, EnvironmentMaterial environment)
            {
                if (_usedShader != this) throw new InvalidOperationException("called shader is not used");

                GL.UniformMatrix4(_uniforms[_UniformConvention.MATRIX_VIEW], true, ref camera.GetViewMatrixRef());
                GL.UniformMatrix4(_uniforms[_UniformConvention.MATRIX_PROJECTION], true, ref camera.GetProjectionMatrixRef());

                if (_uniforms.ContainsKey(_UniformConvention.ENVIRONMENT_VIEWPOS))
                    GL.Uniform3(_uniforms[_UniformConvention.ENVIRONMENT_VIEWPOS], camera.gameObject.GlobalPosition);

                if (_uniforms.ContainsKey(_UniformConvention.ENVIRONMENT_LIGHT_DIRECTION))
                    GL.Uniform3(_uniforms[_UniformConvention.ENVIRONMENT_LIGHT_DIRECTION], environment.DirectionalLightDirection);
                if (_uniforms.ContainsKey(_UniformConvention.ENVIRONMENT_LIGHT_AMBIENT))
                    GL.Uniform3(_uniforms[_UniformConvention.ENVIRONMENT_LIGHT_AMBIENT], environment.AmbientColor);
                if (_uniforms.ContainsKey(_UniformConvention.ENVIRONMENT_LIGHT_DIFFUSE))
                    GL.Uniform3(_uniforms[_UniformConvention.ENVIRONMENT_LIGHT_DIFFUSE], environment.DiffuseColor);
                if (_uniforms.ContainsKey(_UniformConvention.ENVIRONMENT_LIGHT_SPECULAR))
                    GL.Uniform3(_uniforms[_UniformConvention.ENVIRONMENT_LIGHT_SPECULAR], environment.SpecularColor);

            }
            public void ApplyModelTransform(ElementRenderer element)
            {
                if (_usedShader != this) throw new InvalidOperationException("called shader is not used");
                //if (element.material.shader.program != this) throw new ArgumentException("argument shader mismatch");
                
                GL.UniformMatrix4(_uniforms[_UniformConvention.MATRIX_MODEL], true, ref element.gameObject.GetModelMatrixRef());

            }
            public void ApplyMaterial(Material material)
            {
                if (_usedShader != this) throw new InvalidOperationException("called shader is not used");
                //if (material.shader.program != this) throw new ArgumentException("argument shader mismatch");

                if (_uniforms.ContainsKey(_UniformConvention.MATERIAL_COLOR))
                    GL.Uniform4(_uniforms[_UniformConvention.MATERIAL_COLOR], material.Color);
                if (_uniforms.ContainsKey(_UniformConvention.MATERIAL_DIFFUSE))
                    _ApplyTexture(material.DiffuseMap.Handle, TextureUnit.Texture0);
                if (_uniforms.ContainsKey(_UniformConvention.MATERIAL_SPECULAR))
                    _ApplyTexture(material.DiffuseMap.Handle, TextureUnit.Texture1);
                if (_uniforms.ContainsKey(_UniformConvention.MATERIAL_SHININESS))
                    GL.Uniform1(_uniforms[_UniformConvention.MATERIAL_SHININESS], material.Shininess);
                
                static void _ApplyTexture(int handle, TextureUnit textureUnit)
                {
                    GL.ActiveTexture(textureUnit);
                    GL.BindTexture(TextureTarget.Texture2D, handle);
                }

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
                    GL.DeleteProgram(_handle);

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
            ~ShaderProgram()
            {
                if (_disposed == false)
                {
                    throw new Exception("GPU Resource leak - Dispose wasnt called 0o0");
                }
                EngineDebugManager.Send("finalizer called");
            }
            #endregion
        }

        #region texture
        public static void CreateTextureBuffer(ImageResult image, out int handle)
        {
            handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, handle);
            GL.TexImage2D(
                TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                image.Width, image.Height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, image.Data
                );
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }
        #endregion

        #region mesh
        public static void CreateMeshBuffer(float[] vertexData, uint[] indexData,
            out int vbo, out int vao, out int ebo)
        {        
            vbo = GL.GenBuffer(); //-VertexBufferObject
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(
                BufferTarget.ArrayBuffer, 
                vertexData.Length * sizeof(float), vertexData, 
                BufferUsageHint.StaticDraw
                );
            
            vao = GL.GenVertexArray(); //-VertexArrayObject
            GL.BindVertexArray(vao);
                   
            ebo = GL.GenBuffer(); //-ElementBufferObject
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(
                BufferTarget.ElementArrayBuffer, 
                indexData.Length * sizeof(uint), indexData, 
                BufferUsageHint.StaticDraw
                );

            GL.VertexAttribPointer(
                0, 3, VertexAttribPointerType.Float, false, 
                8 * sizeof(float), 0
                );
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(
                1, 2, VertexAttribPointerType.Float, false, 
                8 * sizeof(float), 3 * sizeof(float)
                );
            GL.EnableVertexAttribArray(1);

            GL.VertexAttribPointer(
                2, 3, VertexAttribPointerType.Float, false,
                8 * sizeof(float), 5 * sizeof(float)
                );
            GL.EnableVertexAttribArray(2);
        }
        public static void DeleteMeshBuffer(int vbo, int vao, int ebo)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(vbo);
            GL.BindVertexArray(0);
            GL.DeleteVertexArray(vao);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.DeleteBuffer(vbo);
        }
        public static void DrawVao(int vao, int indexLength)
        {
            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, indexLength, DrawElementsType.UnsignedInt, 0);
        }
        #endregion
    }
}
