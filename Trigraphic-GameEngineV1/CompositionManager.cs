

namespace Trigraphic_GameEngineV1
{
    internal static class CompositionManager
    {
        static EngineWindow? _engineWindow;
        public static void EngineWindowAssign(EngineWindow instance)
        {
            if (_engineWindow != null)
                throw new InvalidOperationException("enginewindow already assigned");
            _engineWindow = instance;
            instance.RenderFrame += _RenderComposition;
            instance.UpdateFrame += _DelegateUpdates;
            instance.Unload += _DelegateUnload;
        }

        private static void _RenderComposition(OpenTK.Windowing.Common.FrameEventArgs obj)
        {
            foreach (Shader shader in _selectedComposition.Value.ShaderOrder)
            {
                shader.RenderAllElements();
            }
        }

        private static void _DelegateUpdates(OpenTK.Windowing.Common.FrameEventArgs obj)
        {
            if (_selectedComposition.HasValue && _selectedComposition.Value.Loaded)
                _selectedComposition.Value.Update((float)obj.Time);
        }
        private static void _DelegateUnload()
        {
            if (_selectedComposition.HasValue && _selectedComposition.Value.Loaded)
                _selectedComposition.Value.Unload();
            _selectedComposition = null;
        } 




        struct Composition
        {
            public Composition(string name, params Shader[] shaderOrder)
            {
                Name = name;
                ShaderOrder = shaderOrder;
                RootObject = new(out Load, out Unload, out Update);
            }
            public readonly string Name;
            public readonly RootGameObject RootObject;
            public bool Loaded => RootObject.Loaded;
            public Action Load, Unload;
            public Action<float> Update;
            public Shader[] ShaderOrder;
        }
        static List<Composition> _compositions = new();
        static Composition? _selectedComposition;

        public static RootGameObject GetSelectedRootObject()
        {
            if (_selectedComposition == null)
                throw new InvalidOperationException("operation cannot be made without engine initialisation");
            return _selectedComposition.Value.RootObject;
        }

        public static void AddComposition(string? name = null, params Shader[] shaderRenderOrder)
        {
            if (string.IsNullOrEmpty(name))
                name = "NewScene" + _compositions.Count;

            if (_compositions.FindIndex(x => x.Name == name) != -1)
                throw new Exception("composition name not unique");

            if (shaderRenderOrder.Length <= 0) shaderRenderOrder = [Shader.DEFAULT];
            _compositions.Add(new Composition(name,shaderRenderOrder));
        }
        public static void RemoveComposition(string name)
        {
            var index = _compositions.FindIndex(x => x.Name == name);
            if (index < 0)
                throw new Exception("composition with name doesn't exist");
            _compositions.RemoveAt(index);
        }

        public static void SelectComposition(string name)
            => SelectComposition(_compositions.FindIndex(x => x.Name == name));
        public static void SelectComposition(int index = 0)
        {
            if (index < 0 || index >= _compositions.Count)
                throw new ArgumentOutOfRangeException("index");

            if (_selectedComposition.HasValue && _selectedComposition.Value.Loaded)
                _selectedComposition.Value.Unload();

            _selectedComposition = _compositions[index];
        }

        public static void LoadSelectedComposition()
        {
            if (_selectedComposition.HasValue && !_selectedComposition.Value.Loaded)
                _selectedComposition.Value.Load();
            else throw new InvalidOperationException("composition null or loaded");
        }
    }
}
