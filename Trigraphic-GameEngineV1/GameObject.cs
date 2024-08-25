using OpenTK.Mathematics;
using StbImageSharp;
using System.Collections.ObjectModel;

namespace Trigraphic_GameEngineV1
{
    internal class GameObject
    {
        #region parenting logic
        List<GameObject> _children = new();
        ReadOnlyCollection<GameObject> _readOnlyChildren;
        void _AddChild(GameObject child)
        {
            _children.Add(child);
        }
        void _RemoveChild(GameObject child)
        {
            _children.Remove(child);
        }
        GameObject? _parent;

        public IReadOnlyList<GameObject> Children => _readOnlyChildren;
        public GameObject? Parent
        {
            get => _parent;
            set
            {
                if (value == null) 
                    throw new NullReferenceException("ordinary gameobjects must have a parent");
                if (_parent != value)
                {
                    if (isPrefab != value.isPrefab) throw new InvalidOperationException("non prefabs and prefabs cannot mix in the hirarchy");
                    if (_IsThisOrChild(value))
                        throw new InvalidOperationException("a child cannot be the its own or parent of their own parent");
                    _parent?._RemoveChild(this);
                    _parent = value;
                    _parent._AddChild(this);
                    if (_parent.isLoaded)
                    {
                        if (!isLoaded) _Load();
                    }
                    else
                    {
                        if (isLoaded) _Unload();
                    }

                    _InvalidateTransform();
                }
            }
        }
        bool _IsThisOrChild(GameObject subject)
        {
            if (subject == this) return true;
            foreach (GameObject child in _children)
            {
                if (child._IsThisOrChild(subject)) 
                    return true;
            }
            return false;
        }
        #endregion

        #region transformation logic
        Vector3 _elementtDimensions = Vector3.One;

        Vector3 _localPosition = Vector3.Zero;
        Quaternion _localRotation = Quaternion.Identity;
        Vector3 _localScale = Vector3.One;

        Vector3 _globalPosition = Vector3.Zero;
        Quaternion _globalRotation = Quaternion.Identity;
        Matrix4 _globalScaleMatrix = Matrix4.Identity;

        public Vector3 GlobalPosition => _globalPosition;
        public Quaternion GlobalRotation => _globalRotation;
        public Matrix4 GlobalScaleMatrix => _globalScaleMatrix;

        public Vector3 Dimensions
        {
            get => _elementtDimensions;
            set
            {
                if (_elementtDimensions != value)
                {
                    _elementtDimensions = value;
                    _modelMatrixValid = false;
                }
            }
        }

        public Vector3 Position
        {
            get => _localPosition;
            set
            {
                if (_localPosition != value)
                {
                    _localPosition = value;
                    _InvalidateTransform(0);
                }
            }
        }
        public Quaternion Rotation
        {
            get => _localRotation;
            set
            {
                if (_localRotation != value)
                {
                    _localRotation = value;
                    _InvalidateTransform(1);
                }
            }
        }
        public Vector3 Scale
        {
            get => _localScale;
            set
            {
                if (_localScale != value)
                {
                    _localScale = value;
                    _InvalidateTransform(2);
                }
            }
        }

        bool _transformValid;
        bool _gPositionValid;
        bool _gRotationValid;
        bool _gScaleValid;
        void _InvalidateTransform(int aspect = -1)
        {
            _transformValid = false;
            _modelMatrixValid = false;

            switch (aspect)
            {
                case 0: _gPositionValid = false; break;
                case 1: _gRotationValid = false; break;
                case 2: _gScaleValid = false; break;
                case -1: _gPositionValid = false;
                    _gRotationValid = false;
                    _gScaleValid = false;
                    break;
            }
        }

        Vector3 _GetGlobalScaleApproximation()
        {
            return new Vector3(_globalScaleMatrix.M11, _globalScaleMatrix.M22, _globalScaleMatrix.M33);
        }
        void _UpdateTransform()
        {
            if (!_transformValid)
            {
                EngineDebugManager.Send("update transform");
                if (_parent == null)
                {
                    _globalPosition = _localPosition;
                    _globalRotation = _localRotation; 
                    _globalScaleMatrix = Matrix4.CreateScale(_localScale);
                }
                else
                {
                    if (!_gPositionValid)
                    {
                        var scaledLocalPosition = Vector3.Transform(_localPosition, _parent._globalRotation);
                        _globalPosition = scaledLocalPosition * _parent._GetGlobalScaleApproximation() + _parent._globalPosition;
                        //if approximation proves inaccurate:
                        //_globalPosition = Vector3.TransformPosition(scaledLocalPosition, _parent._globalScaleMatrix) + _parent._globalPosition;
                        
                        _gPositionValid = true;
                    }

                    if (!_gRotationValid)
                    {
                        _globalRotation = _parent._globalRotation * _localRotation;

                        _gRotationValid = true;
                        _gScaleValid = false;
                    }

                    if (!_gScaleValid)
                    {
                        var rotationMatrix = Matrix4.CreateFromQuaternion(_globalRotation);
                        var inverseRotationMatrix = Matrix4.Invert(rotationMatrix);
                        var scalingMatrix = Matrix4.CreateScale(_localScale);
                        _globalScaleMatrix = _parent._globalScaleMatrix * inverseRotationMatrix * scalingMatrix * rotationMatrix;

                        _gScaleValid = true;
                    }

                }

                _transformValid = true;

                foreach (var child in _children)
                {
                    child._InvalidateTransform();
                }
            }
        }
        #endregion

        #region model matrix logic
        Matrix4 _modelMatrix;
        bool _modelMatrixValid;
        public ref Matrix4 GetModelMatrixRef()
        {
            if (!_modelMatrixValid)
            {
                _modelMatrix =
                Matrix4.CreateScale(_elementtDimensions)
                * Matrix4.CreateFromQuaternion(_globalRotation)
                * _globalScaleMatrix
                * Matrix4.CreateTranslation(_globalPosition);
                _modelMatrixValid = true;
            }
            return ref _modelMatrix;
        }
        #endregion

        #region component logic
        List<Component> _components = new();
        public void AddComponent(Component component)
        {
            component.ObjAssign(this);
            _components.Add(component);
            if (isLoaded) component.Load();
        }
        public void RemoveComponent(Component component)
        {
            if (isLoaded) component.Unload();
            _components.Remove(component);
            //delete component
        }
        public T? GetComponent<T>() where T : Component
        {
            T? result = _components.OfType<T>().FirstOrDefault();
            if (result == null) EngineDebugManager.throwNewOperationRedundancyWarning($"GameObject does not contain a Component of Type '{typeof(T)}'");
            return result;
        }
        public T? GetComponentInChildren<T>() where T : Component
        {
            foreach (var child in _children)
            {
                T? childComponent = child._GetComponentInSelfOrChildren<T>();
                if (childComponent != null)
                {
                    return childComponent;
                }
            }

            EngineDebugManager.throwNewOperationRedundancyWarning($"No Component of Type '{typeof(T)}' found in GameObject or its children.");
            return null;
        }
        T? _GetComponentInSelfOrChildren<T>() where T : Component
        {
            T? component = _components.OfType<T>().FirstOrDefault();
            if (component != null)
            {
                return component;
            }

            foreach (var child in _children)
            {
                T? childComponent = child._GetComponentInSelfOrChildren<T>();
                if (childComponent != null)
                {
                    return childComponent;
                }
            }

            return null;
        }
        #endregion

        #region event logic
        public bool isLoaded { get; private set; }
        protected void _Load()
        {
            if (isLoaded)
                throw new InvalidOperationException("gameobject already loaded");
            isLoaded = true;
            foreach (var component in _components)
            {
                component.Load();
            }
            foreach (var child in _children)
            {
                child._Load();
            }
        }
        protected void _Unload()
        {
            if (!isLoaded)
                throw new InvalidOperationException("gameobject already unloaded");
            foreach (var component in _components)
            {
                component.Unload();
            }
            isLoaded = false;
            foreach (var child in _children)
            {
                child._Unload();
            }
        }
        protected void _Update(float deltaTime)
        {
            if(!isLoaded) throw new InvalidOperationException("cant update a unloaded gameobject");

            EngineDebugManager.Send("update obj");
            EngineDebugManager.Send(_children.Count);
            _UpdateTransform();
            foreach (var component in _components)
            {
                component.Update(deltaTime);
            }
            foreach (var child in _children)
            {
                child._Update(deltaTime);
            }
        }
        #endregion

        #region creation logic
        public GameObject(params Component[] components)
        {
            _readOnlyChildren = new ReadOnlyCollection<GameObject>(_children);

            foreach (var component in components)
            {
                AddComponent(component);
            }
            _parent = CompositionManager.GetSelectedRootObject();
            _parent._children.Add(this);
            if (_parent.isLoaded) _Load();
        }
        public GameObject(GameObject? parent, params Component[] components)
        {
            _readOnlyChildren = new ReadOnlyCollection<GameObject>(_children);

            foreach (var component in components)
            {
                AddComponent(component);
            }
            Parent = parent;
        }
        public bool isPrefab {  get; private set; }
        protected GameObject(Component[] components, GameObject? transformRefenence)
        {
            _readOnlyChildren = new ReadOnlyCollection<GameObject>(_children);

            if (transformRefenence != null)
            {
                _elementtDimensions = transformRefenence._elementtDimensions;
                _localPosition = transformRefenence._localPosition;
                _localRotation = transformRefenence._localRotation;
                _localScale = transformRefenence._localScale;
            }
            foreach (var component in components)
            {
                AddComponent(component);
            }
        }
        public static GameObject CreatePrefab(params Component[] components)
        {
            var prefab = new GameObject(components, null);
            prefab.isPrefab = true;
            return prefab;
        }
        void _RemovePrefabLabel()
        {
            isPrefab = false;
            foreach (var child in _children)
            {
                child._RemovePrefabLabel();
            }
        }
        public void Instantiate()
        {
            if (!isPrefab) throw new InvalidOperationException("only a prefab can be instantiated manually");

            _RemovePrefabLabel();
            _parent = CompositionManager.GetSelectedRootObject();
            _parent._children.Add(this);
            if (_parent.isLoaded) _Load();
        }
        public void Instantiate(GameObject parent)
        {
            if (!isPrefab) throw new InvalidOperationException("only a prefab can be instantiated manually");

            _RemovePrefabLabel();
            Parent = parent;
        }
        public GameObject InstantiateCopy()
        {
            if (!isPrefab) throw new InvalidOperationException("only a prefab can be instantiated manually");

            var componentsCopy = new Component[_components.Count];
            for (int i = 0; i < componentsCopy.Length; i++)
                componentsCopy[i] = _components[i].ShallowCopy();
            
            var prefabCopy = new GameObject(componentsCopy, this);
            foreach (var child in _children)
            {
                child.InstantiateCopy(prefabCopy);
            }

            prefabCopy._parent = CompositionManager.GetSelectedRootObject();
            prefabCopy._parent._children.Add(prefabCopy);
            if (prefabCopy._parent.isLoaded) _Load();

            return prefabCopy;
        }
        public GameObject InstantiateCopy(GameObject parent)
        {
            if (!isPrefab) throw new InvalidOperationException("only a prefab can be instantiated manually");

            var componentsCopy = new Component[_components.Count];
            for (int i = 0; i < componentsCopy.Length; i++)
                componentsCopy[i] = _components[i].ShallowCopy();

            var prefabCopy = new GameObject(componentsCopy, this);
            foreach (var child in _children)
            {
                child.InstantiateCopy(prefabCopy);
            }

            prefabCopy.Parent = parent;

            return prefabCopy;
        }
        #endregion

        #region tgx import
        public static GameObject ImportTgxAsPrefab(string filePath, Shader shader)
        {
            var rootObjects = ImportTgxSceneAsPrefabs(filePath, shader);
            if (rootObjects.Count == 1) return rootObjects[0];
            else
            {
                GameObject root = CreatePrefab();
                foreach (var child in rootObjects)
                {
                    child.Parent = root;
                }
                return root;
            }
        }
        public static List<GameObject> ImportTgxSceneAsPrefabs(string filePath, Shader shader)
        {
            if (!new FileInfo(filePath).Extension.Equals(".tgx"))
                throw new ArgumentException("file is not of the TGX-Format");

            var byteData = File.ReadAllBytes(filePath);

            int offset = 0;

            #region read functions
            bool ReadBool()
            {
                bool value = BitConverter.ToBoolean(byteData, offset);
                offset += sizeof(bool);
                return value;
            }
            int ReadInt()
            {
                int value = BitConverter.ToInt32(byteData, offset);
                offset += sizeof(int);
                return value;
            }
            float ReadFloat()
            {
                float value = BitConverter.ToSingle(byteData, offset);
                offset += sizeof(float);
                return value;
            }
            Vector3 ReadVector()
            {
                float x = ReadFloat();
                float y = ReadFloat();
                float z = ReadFloat();
                return new Vector3(x, y, z);
            }
            Quaternion ReadQuaternion()
            {
                float w = ReadFloat();
                float x = ReadFloat();
                float y = ReadFloat();
                float z = ReadFloat();
                return new Quaternion(x, y, z, w);
            }
            ImageResult? ReadImage()
            {
                bool hasImage = ReadBool();
                if (!hasImage) return null;

                int width = ReadInt();
                int height = ReadInt();
                int length = ReadInt();

                byte[] imageData = new byte[length];
                Array.Copy(byteData, offset, imageData, 0, length);
                offset += length;

                return new ImageResult
                {
                    Width = width,
                    Height = height,
                    Data = imageData
                };
            }
            #endregion

            // Begin reading data
            float versionNumber = ReadFloat();
            if (versionNumber != 1.0f)
                throw new InvalidDataException("incompatible version of the TGX-Format");

            int materialCount = ReadInt();
            int meshCount = ReadInt();
            int nodeCount = ReadInt();

            Material[] materials = new Material[materialCount];
            Mesh[] meshes = new Mesh[meshCount];
            int[] meshMaterialIndices = new int[meshCount];
            GameObject[] objects = new GameObject[nodeCount];
            List<GameObject> rootObjects = new List<GameObject>();

            for (int i = 0; i < materialCount; i++)
            {
                var diffuseMap = ReadImage();
                var specularMap = ReadImage();
                float shininess = ReadFloat();

                materials[i] = new Material(shader)
                {
                    DiffuseMap = diffuseMap == null
                     ? Material.DEFAULT.DiffuseMap
                     : new Texture(diffuseMap),
                    SpecularMap = specularMap == null
                     ? Material.DEFAULT.SpecularMap
                     : new Texture(specularMap),
                    Shininess = shininess > 0 
                     ? shininess 
                     : Material.DEFAULT.Shininess
                };
            }

            for (int i = 0; i < meshCount; i++)
            {
                int materialIndex = ReadInt();
                int vertexElementCount = ReadInt();
                int indexCount = ReadInt();

                float[] vertexData = new float[vertexElementCount];
                Buffer.BlockCopy(byteData, offset, vertexData, 0, vertexElementCount * sizeof(float));
                offset += vertexElementCount * sizeof(float);

                uint[] indexData = new uint[indexCount];
                Buffer.BlockCopy(byteData, offset, indexData, 0, indexCount * sizeof(uint));
                offset += indexCount * sizeof(uint);

                meshes[i] = new Mesh(vertexData, indexData);
                meshMaterialIndices[i] = materialIndex;
            }

            for (int i = 0; i < nodeCount; i++)
            {
                bool hasParent = ReadBool();
                GameObject? parent;

                if (hasParent)
                {
                    int parentIndex = ReadInt();
                    parent = objects[parentIndex];
                }
                else parent = null;

                Vector3 position = ReadVector();
                Quaternion rotation = ReadQuaternion();
                Vector3 scale = ReadVector();

                int attachedMeshesCount = ReadInt();
                MeshRenderer[] meshRenderers = new MeshRenderer[attachedMeshesCount];

                for (int mi = 0; mi < attachedMeshesCount; mi++)
                {
                    int meshIndex = ReadInt();
                    meshRenderers[mi] = new MeshRenderer(
                        meshes[meshIndex],
                        materials[meshMaterialIndices[meshIndex]]
                        );
                }

                GameObject @object = CreatePrefab(meshRenderers);
                @object._localPosition = position;
                @object._localRotation = rotation;
                @object._localScale = scale;
                if (parent == null) rootObjects.Add(@object);
                else @object.Parent = parent;
                objects[i] = @object;
            }

            return rootObjects;
        }
        #endregion
    }
}
