

namespace Trigraphic_GameEngineV1
{
    internal static class RenderSystem
    {
        #region elements
        static List<ElementRenderer> _elements = new();
        private class ElementSortComparer : IComparer<ElementRenderer>
        {
            //-Comparer for sorting ElementRenderers by Shader and Material
            public int Compare(ElementRenderer a, ElementRenderer b)
            {
                int shaderComparison = a.material.shader.GetHashCode().CompareTo(b.material.shader.GetHashCode());
                if (shaderComparison != 0) return shaderComparison;
                return a.material.GetHashCode().CompareTo(b.material.GetHashCode());
            }
        }
        public static void AddElement(ElementRenderer element)
        {
            if (_elements.Contains(element)) throw new InvalidOperationException("element already in list");

            int insertIndex = _elements.BinarySearch(element, new ElementSortComparer());
            // When no group match is found BinarySearch returns a negative number
            // indicating the bitwise complement of the insertion point.
            if (insertIndex < 0)
            {
                insertIndex = ~insertIndex;
            }
            _elements.Insert(insertIndex, element);
        }
        public static void RemoveElement(ElementRenderer element)
        {
            if (!_elements.Remove(element)) throw new InvalidOperationException("element not in list");
        }

        static GraphicsCore.ShaderProgram? _shaderProgramCache = null;
        static Material? _materialCache = null;
        public static void RenderElements()
        {
            foreach (var element in _elements)
            {
                var shaderProgram = element.material.shader.ShaderProgram;
                var material = element.material;

                if (_materialCache != material)
                {
                    if (_shaderProgramCache != shaderProgram)
                    {
                        if (DebugManager.RENDERMESSAGES) DebugManager.Send("set shader");
                        _shaderProgramCache = shaderProgram;
                        shaderProgram.UseProgram();
                        shaderProgram.ApplyCamera(SceneManager.GameCamera);
                        shaderProgram.ApplySkybox(SceneManager.Skybox);
                    }
                    if (DebugManager.RENDERMESSAGES) DebugManager.Send("set material");
                    _materialCache = material;
                    shaderProgram.ApplyMaterial(material);
                }
                shaderProgram.ApplyModelMatrix(ref element.gameObject.GetModelMatrixRef());
                if (DebugManager.RENDERMESSAGES) DebugManager.Send("render element");
                element.RenderElement();
            }
        }
        #endregion

        #region canvases
        static List<UICanvas> _canvases = new();
        public static void AddCanvas(UICanvas canvas)
        {
            if (_canvases.Contains(canvas)) throw new InvalidOperationException("canvas already in list");
            _canvases.Add(canvas);
        }
        public static void RemoveCanvas(UICanvas canvas)
        {
            if (!_canvases.Remove(canvas)) throw new InvalidOperationException("canvas not in list");
        }

        public static void RenderCanvases()
        {
            foreach (var canvas in _canvases)
            {
                canvas.RenderCanvas();
            }
        }
        #endregion

        /*
        static Dictionary<Shader, ShaderGroup> shaderGroups = new();

        struct ShaderGroup
        {
            public Dictionary<Material, MaterialGroup> MaterialGroups;
        }
        struct MaterialGroup
        {
            public List<ElementRenderer> Elements;
        }

        public static void AddGameObject(ElementRenderer element)
        {
            var material = element.material;
            var shader = material.shader;

            ShaderGroup shaderGroup;
            if (!shaderGroups.TryGetValue(shader, out shaderGroup))
            {
                shaderGroup = new ShaderGroup { MaterialGroups = new() };
                shaderGroups.Add(shader, shaderGroup );
            }

            MaterialGroup materialGroup;
            if (!shaderGroup.MaterialGroups.TryGetValue(material, out materialGroup))
            {
                materialGroup = new MaterialGroup { Elements = new() };
                shaderGroup.MaterialGroups.Add(material, materialGroup);
            }

            if (materialGroup.Elements.Contains(element))
                throw new InvalidOperationException("element already in list");

            materialGroup.Elements.Add(element);
        }
        public static void RemoveGameObject(ElementRenderer element)
        {
            var shader = element.material.shader;
            if (shaderGroups.TryGetValue(shader, out var shaderGroup))
            {
                if (shaderGroup.MaterialGroups.TryGetValue(element.material, out var materialGroup))
                {
                    materialGroup.Elements.Remove(element);
                    return;
                }
            }

            throw new InvalidOperationException("element not in list");
        }

        //every frame
        public static void RenderAll()
        {
            foreach (var sgPair in shaderGroups)
            {
                DebugManager.Send("set shader");
                var shaderProgram = sgPair.Key.ShaderProgram;
                shaderProgram.UseProgram();
                shaderProgram.ApplyEnvironmentMaterial(SceneManager.GameCamera, SceneManager.Skybox);
                foreach (var mgPair in sgPair.Value.MaterialGroups)
                {
                    DebugManager.Send("set material");
                    shaderProgram.ApplyMaterial(mgPair.Key);
                    foreach (var element in mgPair.Value.Elements)
                    {
                        shaderProgram.ApplyModelMatrix(element);
                        element.RenderElement();
                    }
                }
            }
        }
        */
    }
}
