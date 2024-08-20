using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;
using System.Reflection.Metadata;

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

        public static void CreateShaderProgram(string shaderPath, out int handle)
        {
            string VertexShaderSource = File.ReadAllText(Path.Combine(shaderPath, "vertex.glsl"));
            string FragmentShaderSource = File.ReadAllText(Path.Combine(shaderPath, "fragment.glsl"));

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

            handle = GL.CreateProgram();

            GL.AttachShader(handle, VertexShader);
            GL.AttachShader(handle, FragmentShader);

            GL.LinkProgram(handle);

            GL.GetProgram(handle, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(handle);
                throw new Exception($"Error compiling Shader Program: {Environment.NewLine}{infoLog}");
            }

            //cleanup
            GL.DetachShader(handle, VertexShader);
            GL.DetachShader(handle, FragmentShader);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);
        }
        public static void DeleteShaderProgram(int handle)
        {
            GL.DeleteProgram(handle);
        }
        public static class UniformConvention
        {
            public const string TEXTURE_DIFFUSE = "diffuse";
            public const string MATRIX_VIEW = "view";
            public const string MATRIX_MODEL = "model";
            public const string MATRIX_PROJECTION = "projection";
            public const string V3_COLOR = "color";
        }
        public static void GetShaderProgramUniforms(int handle, out Dictionary<string, int> locations)
        {
            locations = new Dictionary<string, int>();
            GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out int uniformCount);
            for (int i = 0; i < uniformCount; i++)
            {
                string key = GL.GetActiveUniform(handle, i, out _, out _);
                int location = GL.GetUniformLocation(handle, key);
                locations.Add(key, location);
            }

            foreach (var key in locations.Keys)
            {
                EngineDebugManager.Send(key + " -- " + locations[key]);
            }
            if (locations.ContainsKey(UniformConvention.TEXTURE_DIFFUSE))
                GL.Uniform1(locations[UniformConvention.TEXTURE_DIFFUSE], 0);
        }
        public static Shader? ActiveShader;
        public static void UseShaderProgram(int handle, Shader shader)
        {
            GL.UseProgram(handle);
            ActiveShader = shader;
        }
        public static void SetModelMatrix(ref Matrix4 matrix)
        {
            if (ActiveShader == null) throw new InvalidOperationException();
            GL.UniformMatrix4(ActiveShader.GetUniformLocation(UniformConvention.MATRIX_MODEL), true, ref matrix);
        }
        public static void SetViewMatrix(ref Matrix4 matrix)
        {
            if (ActiveShader == null) throw new InvalidOperationException();
            GL.UniformMatrix4(ActiveShader.GetUniformLocation(UniformConvention.MATRIX_VIEW), true, ref matrix);
        }
        public static void SetProjectionMatrix(ref Matrix4 matrix)
        {
            if (ActiveShader == null) throw new InvalidOperationException();
            GL.UniformMatrix4(ActiveShader.GetUniformLocation(UniformConvention.MATRIX_PROJECTION), true, ref matrix);
        }



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
                5 * sizeof(float), 0
                );
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 
                5 * sizeof(float), 3 * sizeof(float)
                );
            GL.EnableVertexAttribArray(1);
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

        public static void CreateTextureBuffer(string filename, out int handle, out int width, out int height)
        {
            var image = _LoadImageData(filename);
            width = image.Width; 
            height = image.Height;

            handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, handle);
            GL.TexImage2D(
                TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 
                image.Width, image.Height, 0, 
                PixelFormat.Rgba, PixelType.UnsignedByte, image.Data
                );
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }
        static ImageResult _LoadImageData(string filename)
        {
            //-stb loads from top-left pixel, opengl loads from  bottom-left)
            StbImage.stbi_set_flip_vertically_on_load(1);
            var result = ImageResult.FromStream(File.OpenRead(filename), ColorComponents.RedGreenBlueAlpha);
            return result;
        }
        public static void ApplyTexture(int handle, TextureUnit textureUnit)
        {
            GL.ActiveTexture(textureUnit);
            GL.BindTexture(TextureTarget.Texture2D, handle);
        }
        public static void ApplyMaterial(Material material)
        {
            Shader s = material.shader;
            if (s.GetUniform(UniformConvention.TEXTURE_DIFFUSE)) 
                material.diffuse.Apply(TextureUnit.Texture0);
            if (s.GetUniform(UniformConvention.V3_COLOR))
                GL.Uniform3(s.GetUniformLocation(UniformConvention.V3_COLOR), material.color);
        }
    }
}
