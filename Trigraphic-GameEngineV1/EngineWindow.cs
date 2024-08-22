using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Trigraphic_GameEngineV1
{
    internal sealed class EngineWindow : GameWindow
    {
        public EngineWindow(int width, int height, string title)
            : base(GameWindowSettings.Default, new NativeWindowSettings()
            {
                ClientSize = (width, height),
                Title = title,
                StartVisible = false,
                NumberOfSamples = 4, //AntiAliasing
                WindowBorder = WindowBorder.Fixed,
            })
        {
            CompositionManager.EngineWindowAssign(this);
            InputManager.EngineWindowAssign(this);
        }

        public static float aspectRatio { get; private set; }
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            aspectRatio = (float)e.Width / e.Height;
            GL.Viewport(0, 0, e.Width, e.Height);
        }
        protected override void OnLoad()
        {
            GraphicsCore.SetRenderParameters(new(0.04f, 0.0f, 0.07f, 1.0f));
            CenterWindow();
            IsVisible = true;

            //test code
            CompositionManager.AddComposition();
            CompositionManager.SelectComposition();

            var Player = new GameObject(new PlayerBehaviour());
            Player.Position = (0, 0, 6);
            var Cam = new GameObject(Player, new Camera());
            Cam.Position = (0, 1.8f, 0);

            var Obj = new GameObject(new MeshRenderer());
            Obj.Scale = (1, .3f, 1);
            Obj.Position = (0, 1.3f, 0);
            var Obj0 = new GameObject(Obj, new MeshRenderer(), new MoverBehaviour
                (rotationDirection: (0, 0, 1))
                );
            Obj0.Position = (1.5f, 0, 0);

            var Obj1 = new GameObject(new MeshRenderer());
            Obj1.Scale = (1, 1f, 1);
            Obj1.Position = (0, 2.3f, 0);
            var Obj10 = new GameObject(Obj1, new MeshRenderer(), new MoverBehaviour
                (rotationDirection: (0, 0, 1))
                );
            Obj10.Position = (1.5f, 0, 0);




            CompositionManager.LoadSelectedComposition();
            //test code
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }
            if (KeyboardState.IsKeyDown(Keys.Tab))
            {
                InputManager.GrabCursor(!InputManager.CursorGrabbed);
            }

            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            EngineDebugManager.Send("clear screen");
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            base.OnRenderFrame(args);
            SwapBuffers();
            EngineDebugManager.Send("swapbuffers");
        }
    }
}
