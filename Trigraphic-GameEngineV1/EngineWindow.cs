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
            CompositionManager.AddComposition(
                "main",
                new EnvironmentMaterial(Color4.White),
                ResourceManager.DEFAULT_SHADER_LIT,
                ResourceManager.DEFAULT_SHADER_UNLIT,
                ResourceManager.DEFAULT_SHADER_LIGHTSOURCE
                );
            CompositionManager.SelectComposition();

            var uiCam = new GameObject(new Camera(1, true));
            ResourceManager.DEFAULT_SHADER_UNLIT.CameraSlot = 1;
            var Text = new GameObject(new TextRenderer());
            Text.Position = (-12, 10, 0);

            var Player = GameObject.CreatePrefab(new PlayerBehaviour(Text.GetComponent<TextRenderer>()));
            Player.Position = (0, 0, 6);
            var Cam = GameObject.CreatePrefab(Player, new Camera());
            Cam.Position = (0, 1.8f, 0);
            var Headlight = GameObject.CreatePrefab(Cam, new SpotLight());
            Headlight.Position = (0, 0.1f, -0.1f);
            Headlight.Rotation = Quaternion.FromEulerAngles(float.Pi/2, 0, 0);
            Headlight.Dimensions = (0.1f, 0.1f, 0.1f);
            Player.Instantiate();

            var lightingScene = new GameObject();
            lightingScene.Position = (0, 0, 0);

            var spotlight = new GameObject(lightingScene, new SpotLight(Color4.Red) { SmoothingAngleDeg = 1f } );
            spotlight.Position = (0, 3, 0);

            var pointLight = new GameObject(lightingScene, new PointLight(Color4.Yellow));
            pointLight.Position = (9, 0.4f, 0);

            var eaglePrefab = ResourceManager.ImportTgxPrefab(
                 Path.Combine("...//..//..//..//..//Rsc//Files3d", "eagle.tgx"), ResourceManager.DEFAULT_SHADER_LIT);
            eaglePrefab.Scale *= .1f;
            eaglePrefab.Instantiate(lightingScene);

            var primitivesPrefab = ResourceManager.ImportTgxPrefab(
                Path.Combine("...//..//..//..//..//Rsc//Files3d", "objblender_primitives.tgx"), ResourceManager.DEFAULT_SHADER_LIT);
            float offset = 0.0f;
            foreach (var c in primitivesPrefab.Children)
            {
                c.Position = (offset, 0, 0);
                offset += 2.5f;
            }
            primitivesPrefab.Instantiate(lightingScene);


            CompositionManager.LoadSelectedComposition();
            //test code
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
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
