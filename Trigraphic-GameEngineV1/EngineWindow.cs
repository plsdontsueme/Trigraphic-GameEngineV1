﻿using OpenTK.Graphics.OpenGL4;
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
                WindowBorder = WindowBorder.Resizable,
            })
        {
            SceneManager.Initialize(this);
            InputManager.EngineWindowAssign(this);
        }

        public static float AspectRatio { get; private set; }
        public static Vector2 InverseSize { get; private set; }
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            InverseSize = (1f / e.Size.X, 1f / e.Size.Y);
            AspectRatio = (float)e.Width / e.Height;
            GL.Viewport(0, 0, e.Width, e.Height);
        }
        protected override void OnLoad()
        {
            GraphicsCore.SetRenderParameters(new(0.04f, 0.0f, 0.07f, 1.0f));
            CenterWindow();
            IsVisible = true;

            //test code
            var FpsText = new UIText();
            FpsText.Position = (-5, 5, 0);
            FpsText.Size *= .5f;

            var Player = GameObject.CreatePrefab(new PlayerBehaviour(FpsText));
            Player.Position = (0, 0, 6);
            var Cam = GameObject.CreatePrefab(Player, new CameraRig(SceneManager.GameCamera));
            Cam.Position = (0, 1.8f, 0);
            var Headlight = GameObject.CreatePrefab(Cam, new SpotLight());
            Headlight.Position = (0, 0.1f, -0.1f);
            Headlight.Rotation = Quaternion.FromEulerAngles(float.Pi/2, 0, 0);
            Headlight.Dimensions *= .1f;
            Player.Instantiate();

            var random = new Random();
            var radius = 50;
            for (int i = 0; i < 1000; i++)
            {
                var obj = new GameObject(new MeshRenderer(Mesh.Static.CUBE), new MoverBehaviour( rotationDirection: (random.Next(-radius, radius) / (float)radius, random.Next(-radius, radius) / (float)radius, random.Next(-radius, radius) / (float)radius)));
                obj.Position = (random.Next(-radius, radius), random.Next(-radius, radius), random.Next(-radius, radius));
                obj.Rotation = Quaternion.FromEulerAngles(random.Next(-radius, radius) / MathHelper.PiOver4, random.Next(-radius, radius) / MathHelper.PiOver4, random.Next(-radius, radius) / MathHelper.PiOver4);
            }

            var canvas = new GameObject(new UICanvas()).GetComponent<UICanvas>();
            var image = new UIImage("...//..//..//..//..//Rsc//Images//chromecore.jpg");
            image.Size *= 1.5f;
            canvas.AddElement(image);
            var imagePix = new UIImage("...//..//..//..//..//Rsc//Images//pix.png", false);
            imagePix.Size *= 1.5f;
            imagePix.Position = (2,0,0);
            canvas.AddElement(imagePix);
            canvas.AddElement(FpsText);

            var lightingScene = new GameObject();
            lightingScene.Position = (0, 0, 0);

            var spotlight = new GameObject(lightingScene, new SpotLight(Color4.Red) { SmoothingAngleDeg = 1f } );
            spotlight.Position = (0, 3, 0);

            var pointLight = new GameObject(lightingScene, new PointLight(Color4.Yellow));
            pointLight.Position = (9, 0.4f, 0);

            var eaglePrefab = ResourceManager.ImportTgxPrefab("eagle.tgx");
            eaglePrefab.Children[0].Dimensions *= .1f;
            eaglePrefab.Instantiate(lightingScene);

            var primitivesPrefab = ResourceManager.ImportTgxPrefab("objblender_primitives.tgx");
            float offset = 0.0f;
            foreach (var c in primitivesPrefab.Children)
            {
                c.Position = (offset, 0, 0);
                offset += 2.5f;
            }
            primitivesPrefab.Instantiate(lightingScene);
            //test code

            base.OnLoad();
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
            if (DebugManager.RENDERMESSAGES) DebugManager.Send("clear screen");
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            base.OnRenderFrame(args);
            SwapBuffers();
            if (DebugManager.RENDERMESSAGES) DebugManager.Send("swapbuffers");
        }
    }
}