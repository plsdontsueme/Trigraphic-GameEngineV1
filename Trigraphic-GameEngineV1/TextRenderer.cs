using OpenTK.Graphics.OpenGL4;

namespace Trigraphic_GameEngineV1
{
    internal class TextRenderer : ElementRenderer
    {
        public string Text;
        public Font font;

        int _vao;
        int _vbo;
        public TextRenderer(string text = "text renderer") : base(ResourceManager.DEFAULT_FONT)
        {
            font = ResourceManager.DEFAULT_FONT;
            Text = text;
            Setup();
        }
        public TextRenderer(Font font, string text = "text renderer") : base(font)
        {
            this.font = font;
            Text = text;
            Setup();
        }
        void Setup()
        {
            float[] vertices = {
            0, 0, 0, 0, 0, //Bottom-left vertex
            0, 0, 0, 1, 0, //Bottom-right vertex
            0, 0, 0, 0, 1,  //Top left
            0, 0, 0, 1, 1  //Top right
            };
            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, 80, vertices, BufferUsageHint.StreamDraw); //80 = vertices.Length * sizeof(float)
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);  
        }

        public override void RenderElement()
        {
            base.RenderElement();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            string[] lines = Text.Split(Environment.NewLine);
            float y = -font.lineHeight;
            foreach (string line in lines)
            {
                float x = 0;
                foreach (char c in line)
                {
                    var g = font.glyphs[c];

                    float gy = y - g.YOffset;
                    float gx = x + g.XOffset;
                    float[] vertices = {
                    gx,          gy - g.Height, 0, g.X0, g.Y0, //Bottom-left vertex
                    gx+g.Width,  gy - g.Height, 0, g.X1, g.Y0, //Bottom-right vertex
                    gx,          gy,            0, g.X0, g.Y1, //Top left
                    gx+g.Width,  gy,            0, g.X1, g.Y1  //Top right
                    };
                    x += g.XAdvance;

                    GL.BufferSubData(BufferTarget.ArrayBuffer, 0, vertices.Length * sizeof(float), vertices);
                    GL.BindVertexArray(_vao);
                    GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
                }
                y -= font.lineHeight;
            }
        }
    }
}
