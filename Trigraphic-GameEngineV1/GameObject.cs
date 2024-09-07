using OpenTK.Mathematics;
using StbImageSharp;
using System.Collections.ObjectModel;

namespace Trigraphic_GameEngineV1
{
    internal class GameObject
    {
        #region parenting logic
        List<GameObject> _children = new();
        void _AddChild(GameObject child)
        {
            _children.Add(child);
        }
        void _RemoveChild(GameObject child)
        {
            _children.Remove(child);
        }

        ReadOnlyCollection<GameObject> _readOnlyChildren;
        public IReadOnlyList<GameObject> Children => _readOnlyChildren;

        GameObject? _parent;
        public GameObject? Parent
        {
            get => _parent;
            set
            {
                if (value == null)
                    throw new NullReferenceException("ordinary gameobjects must have a parent");
                if (_parent != value)
                {
                    if (IsPrefab != value.IsPrefab) throw new InvalidOperationException("non prefabs and prefabs cannot mix in the hirarchy");
                    if (_IsThisOrChild(value))
                        throw new InvalidOperationException("a child cannot be the its own or parent of their own parent");
                    _parent?._RemoveChild(this);
                    _parent = value;
                    _parent._AddChild(this);
                    if (_parent.IsLoaded)
                    {
                        if (!IsLoaded) _Load();
                    }
                    else
                    {
                        if (IsLoaded) _Unload();
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

        bool _transformValid = true;
        bool _gPositionValid = true;
        bool _gRotationValid = true;
        bool _gScaleValid = true;
        void _InvalidateTransform(int aspect = -1)
        {
            if (_transformValid)
            {
                _transformValid = false;
                _modelMatrixValid = false;
                UpdateSystem.EnqueueTransformUpdate(this);
            }

            switch (aspect)
            {
                case 0: _gPositionValid = false; break;
                case 1: _gRotationValid = false; break;
                case 2: _gScaleValid = false; break;
                case -1:
                    _gPositionValid = false;
                    _gRotationValid = false;
                    _gScaleValid = false;
                    break;
            }
        }

        Vector3 _GetGlobalScaleApproximation()
        {
            return new Vector3(_globalScaleMatrix.M11, _globalScaleMatrix.M22, _globalScaleMatrix.M33);
        }
        public void UpdateTransform()
        {
            if (!_transformValid)
            {
                if (DebugManager.UPDATETRANSFORMMESSAGES) DebugManager.Send("update transform");
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
                    }

                    if (!_gRotationValid)
                    {
                        _globalRotation = _parent._globalRotation * _localRotation;

                        _gScaleValid = false;
                    }

                    if (!_gScaleValid)
                    {
                        var rotationMatrix = Matrix4.CreateFromQuaternion(_globalRotation);
                        var inverseRotationMatrix = Matrix4.Invert(rotationMatrix);
                        var scalingMatrix = Matrix4.CreateScale(_localScale);
                        _globalScaleMatrix = _parent._globalScaleMatrix * inverseRotationMatrix * scalingMatrix * rotationMatrix;
                    }

                }

                _transformValid = true;

                foreach (var child in _children)
                {
                    if (!_gRotationValid)
                    {
                        child._InvalidateTransform(0);
                        child._InvalidateTransform(1);
                    }
                    else if (!_gPositionValid)
                    { 
                        child._InvalidateTransform(0); 
                    }
                    if (!_gScaleValid)
                    {
                        child._InvalidateTransform(0);
                        child._InvalidateTransform(2);
                    }
                }
                _gPositionValid = true;
                _gRotationValid = true;
                _gScaleValid = true;
            }
        }
        #endregion

        #region model matrix logic
        Matrix4 _modelMatrix;
        bool _modelMatrixValid = false;
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
        List<ComponentStatic> _components = new();
        public void AddComponent(ComponentStatic component)
        {
            _components.Add(component);
            component.ObjAssign(this);
            if (IsLoaded) component.Load();
        }
        public void AddComponents(params ComponentStatic[] components)
        {
            _components.AddRange(components);
            if (IsLoaded)
            {
                foreach (var component in _components)
                {
                    component.ObjAssign(this);
                    component.Load();
                }
            }
            else foreach (var component in _components)
                    component.ObjAssign(this);
        }
        public void RemoveComponent(ComponentStatic component)
        {
            if (IsLoaded) component.Unload();
            if (!_components.Remove(component)) throw new InvalidOperationException("component not in list");
            //delete component
        }
        public T? GetComponent<T>() where T : ComponentStatic
        {
            T? result = _components.OfType<T>().FirstOrDefault();
            if (result == null) DebugManager.throwNewOperationRedundancyWarning($"GameObject does not contain a Component of Type '{typeof(T)}'");
            return result;
        }
        public T? GetComponentInChildren<T>() where T : ComponentStatic
        {
            foreach (var child in _children)
            {
                T? childComponent = child._GetComponentInSelfOrChildren<T>();
                if (childComponent != null)
                {
                    return childComponent;
                }
            }

            DebugManager.throwNewOperationRedundancyWarning($"No Component of Type '{typeof(T)}' found in GameObject or its children.");
            return null;
        }
        T? _GetComponentInSelfOrChildren<T>() where T : ComponentStatic
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
        public bool IsLoaded { get; private set; }
        protected void _Load()
        {
            if (IsLoaded)
                throw new InvalidOperationException("gameobject already loaded");
            IsLoaded = true;
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
            if (!IsLoaded)
                throw new InvalidOperationException("gameobject already unloaded");
            IsLoaded = false;
            foreach (var component in _components)
            {
                component.Unload();
            }
            foreach (var child in _children)
            {
                child._Unload();
            }
        }
        #endregion

        #region creation logic
        public GameObject(params ComponentStatic[] components)
        {
            _readOnlyChildren = new ReadOnlyCollection<GameObject>(_children);

            AddComponents(components);
            _ParentToRoot();
        }
        public GameObject(GameObject? parent, params ComponentStatic[] components)
        {
            _readOnlyChildren = new ReadOnlyCollection<GameObject>(_children);

            AddComponents(components);
            Parent = parent;
        }
        protected GameObject(ComponentStatic[] components, GameObject? transformRefenence)
        {
            _readOnlyChildren = new ReadOnlyCollection<GameObject>(_children);

            if (transformRefenence != null)
            {
                _elementtDimensions = transformRefenence._elementtDimensions;
                _localPosition = transformRefenence._localPosition;
                _localRotation = transformRefenence._localRotation;
                _localScale = transformRefenence._localScale;
            }

            AddComponents(components);
        }
        public void _ParentToRoot()
        {
            _parent = SceneManager.RootGameObject;
            _parent._children.Add(this);
            if (_parent.IsLoaded) _Load();
        }
        #endregion

        #region prefab logic
        public bool IsPrefab { get; private set; }
        public static GameObject CreatePrefab(params ComponentStatic[] components)
        {
            var prefab = new GameObject(components, null);
            prefab.IsPrefab = true;
            return prefab;
        }
        public static GameObject CreatePrefab(GameObject parent, params ComponentStatic[] components)
        {
            var prefab = new GameObject(components, null);
            prefab.IsPrefab = true;
            prefab.Parent = parent;
            return prefab;
        }

        public void Instantiate(GameObject? parent = null)
        {
            if (!IsPrefab) throw new InvalidOperationException("only a prefab can be instantiated manually");

            _RemovePrefabLabel();
            if (parent != null) Parent = parent;
            else _ParentToRoot();
        }
        void _RemovePrefabLabel()
        {
            IsPrefab = false;
            foreach (var child in _children)
            {
                child._RemovePrefabLabel();
            }
        }

        public GameObject InstantiateCopy(GameObject? parent = null)
        {
            if (!IsPrefab) throw new InvalidOperationException("only a prefab can be instantiated manually");

            var componentsCopy = new ComponentStatic[_components.Count];
            for (int i = 0; i < componentsCopy.Length; i++)
                componentsCopy[i] = _components[i].ShallowCopy();

            var prefabCopy = new GameObject(componentsCopy, this);
            foreach (var child in _children)
            {
                child.InstantiateCopy(prefabCopy);
            }

            if (parent != null) prefabCopy.Parent = parent;
            else prefabCopy._ParentToRoot();

            return prefabCopy;
        }
        #endregion
    }
}