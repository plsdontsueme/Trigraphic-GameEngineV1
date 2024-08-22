using OpenTK.Mathematics;
using System.Reflection.Metadata;

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

        GameObject? _parent;
        public GameObject? Parent
        {
            get => _parent;
            set
            {
                if (_parent != value)
                {
                    if (value != null)
                    {
                        if (_InquireThePresenceOfAParticularInstanceOfTheGameObjectClassAmongOnesOwnSubordinates(value))
                            throw new InvalidOperationException("a child cannot be the parent of their own parent");
                        _parent?._RemoveChild(this);
                        _parent = value;
                        _parent._AddChild(this);
                        if (_parent.Loaded)
                        {
                            if (!Loaded) _Load();
                        }
                        else
                        {
                            if(Loaded) _Unload();
                        }
                    }
                    else
                    {
                        _parent = null;
                        if (Loaded) _Unload();
                    }

                    _InvalidateTransform();
                }
            }
        }
        bool _InquireThePresenceOfAParticularInstanceOfTheGameObjectClassAmongOnesOwnSubordinates(GameObject subject)
        {
            if (subject == this) return true;
            foreach (GameObject child in _children)
            {
                if (child._InquireThePresenceOfAParticularInstanceOfTheGameObjectClassAmongOnesOwnSubordinates(subject)) 
                    return true;
            }
            return false;
        }
        #endregion

        #region transformation logic
        private Vector3 _elementtDimensions = Vector3.One;

        private Vector3 _localPosition = Vector3.Zero;
        private Quaternion _localRotation = Quaternion.Identity;
        private Vector3 _localScale = Vector3.One;

        private Vector3 _globalPosition = Vector3.Zero;
        private Quaternion _globalRotation = Quaternion.Identity;
        private Matrix4 _globalScaleMatrix = Matrix4.Identity;

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
            if (Loaded) component.Load();
        }
        public void RemoveComponent(Component component)
        {
            _components.Remove(component);
            component.Unload();
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
        public bool Loaded { get; private set; }
        protected void _Load()
        {
            if (Loaded)
                throw new InvalidOperationException("gameobject already loaded");
            Loaded = true;
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
            if (!Loaded)
                throw new InvalidOperationException("gameobject already unloaded");
            Loaded = false;
            foreach (var component in _components)
            {
                component.Unload();
            }
            foreach (var child in _children)
            {
                child._Unload();
            }
        }
        protected void _Update(float deltaTime)
        {
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
            foreach (var component in components)
            {
                AddComponent(component);
            }
            _parent = CompositionManager.GetSelectedRootObject();
            _parent._children.Add(this);
            if (_parent.Loaded) _Load();
        }
        public GameObject(GameObject? parent, params Component[] components)
        {
            foreach (var component in components)
            {
                AddComponent(component);
            }
            Parent = parent;
        }
        #endregion
    }
}
