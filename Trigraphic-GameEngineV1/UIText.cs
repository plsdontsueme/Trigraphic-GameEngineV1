using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Trigraphic_GameEngineV1
{
    internal class UIText : UIElement
    {
        public string Text;
        public Font Font;
        public Color4 Color = Color4.White;

        int _vao;
        int _vbo;
        public UIText(string text = "UIText", Font? font = null)
        {
            Font = font ?? Font.Static.ARIAL;
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

        public override bool GetRenderParameters(out Color4? color, out Texture? texture)
        {
            color = Color;
            texture = Font;
            return true;
        }

        public override void Render()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            string[] lines = Text.Split(Environment.NewLine);
            float y = -Font.LineHeight;
            foreach (string line in lines)
            {
                float x = 0;
                foreach (char c in line)
                {
                    var g = Font.Glyphs[c];

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
                y -= Font.LineHeight;
            }
        }

        public override void OnMouseClick(Vector2 mouse)
        {
        }
    }
}
