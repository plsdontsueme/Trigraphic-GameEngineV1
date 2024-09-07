

using OpenTK.Mathematics;

namespace Trigraphic_GameEngineV1
{
    internal static class SceneManager
    {
        #region initialization
        static bool _initialized;
        public static void Initialize(EngineWindow instance)
        {
            if (_initialized)
                throw new InvalidOperationException("enginewindow already assigned");
            _initialized = true;
            instance.RenderFrame += _RenderScene;
            instance.UpdateFrame += _UpdateScene;
            instance.Load += _LoadScene;
            instance.Unload += _UnloadScene;
        }
        #endregion

        #region engine window event delegation
        static void _LoadScene()
        {
            _rootLoad();
        }
        static void _RenderScene(OpenTK.Windowing.Common.FrameEventArgs obj)
        {
            RenderSystem.RenderElements();
            RenderSystem.RenderCanvases();
        }

        static void _UpdateScene(OpenTK.Windowing.Common.FrameEventArgs obj)
        {
            UpdateSystem.ProcessTransformUpdateQueue();
            UpdateSystem.UpdateComponents((float)obj.Time);
        }
        static void _UnloadScene()
        {
            _rootUnload();
        }
        #endregion

        static RootGameObject _rootObject = new(out _rootLoad, out _rootUnload);
        static Action _rootLoad, _rootUnload;

        public static GameObject RootGameObject => _rootObject;


        public static readonly Skybox Skybox = new(Color4.DarkGray);
        public static readonly Camera GameCamera = new(false);
        public static readonly Camera UICamera = new(true);
    }
}
