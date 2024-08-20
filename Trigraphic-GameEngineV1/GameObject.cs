using OpenTK.Mathematics;

namespace Trigraphic_GameEngineV1
{
    internal class GameObject
    {
        #region parenting logic
        GameObject? _parent;
        List<GameObject> _children = new();
        void _AddChild(GameObject child)
        {
            _children.Add(child);
        }
        void _RemoveChild(GameObject child)
        {
            _children.Remove(child);
        }

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
        private Vector3 _localPosition = Vector3.Zero;
        private Quaternion _localRotation = Quaternion.Identity;
        private Vector3 _localScale = Vector3.One;

        private Vector3 _globalPosition = Vector3.Zero;
        private Quaternion _globalRotation = Quaternion.Identity;
        private Matrix4 _globalScaleMatrix = Matrix4.Identity;

        private bool _transformValid;
        void _InvalidateTransform(int aspect = -1)
        {
            _transformValid = false;
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

        public Vector3 GlobalPosition => _globalPosition;
        public Quaternion GlobalRotation => _globalRotation;
        public Matrix4 GlobalScaleMatrix => _globalScaleMatrix;
        Vector3 _GetGlobalScaleApproximation()
        {
            return new Vector3(_globalScaleMatrix.M11, _globalScaleMatrix.M22, _globalScaleMatrix.M33);
        }

        void _UpdateTransform()
        {
            if (!_transformValid)
            {
                Console.WriteLine("update transform");
                if (_parent == null)
                {
                    _globalPosition = _localPosition;
                    _globalRotation = _localRotation;
                    _globalScaleMatrix = Matrix4.CreateScale(_localScale);
                }
                else
                {
                    _globalPosition = Vector3.Transform(_localPosition * _parent._GetGlobalScaleApproximation(), _parent._globalRotation) + _parent._globalPosition;
                    _globalRotation = _parent._globalRotation * _localRotation;

                    Quaternion inverseRotationQuaternion = _localRotation.Inverted();
                    //-apply the inverse rotation to the parents scale matrix
                    Matrix4 inverseRotationMatrix = Matrix4.CreateFromQuaternion(inverseRotationQuaternion);
                    Matrix4 correctedParentScale = inverseRotationMatrix * _parent._globalScaleMatrix;
                    //-apply the parents scale to the child's scale
                    Matrix4 transformedScaleMatrix = Matrix4.CreateScale(_localScale) * correctedParentScale;
                    //-reapply the original rotation using the quaternion
                    Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(_localRotation);
                    _globalScaleMatrix = transformedScaleMatrix * rotationMatrix;
                }

                _UpdateModelMatrix();
                _transformValid = true;

                foreach (var child in _children)
                {
                    child._InvalidateTransform();
                }
            }
        }
        #endregion

        #region model matrix logic
        public Matrix4 ModelMatrix { get; private set; }
        void _UpdateModelMatrix()
        {
            ModelMatrix = _globalScaleMatrix
                * Matrix4.CreateFromQuaternion(_globalRotation)
                * Matrix4.CreateTranslation(_globalPosition);
        }
        #endregion

        #region component logic
        List<Component> _components = new();
        public void AddComponent(Component component)
        {
            component.ObjAssign(this);
            _components.Add(component);
            if (Loaded) component.OnLoad();
        }
        public void RemoveComponent(Component component)
        {
            _components.Remove(component);
            component.OnUnload();
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
            T? component = GetComponent<T>();
            if (component != null)
            {
                return component;
            }

            foreach (var child in _children)
            {
                T? childComponent = child.GetComponentInChildren<T>();
                if (childComponent != null)
                {
                    return childComponent;
                }
            }

            EngineDebugManager.throwNewOperationRedundancyWarning($"No Component of Type '{typeof(T)}' found in GameObject or its children.");
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
                component.OnLoad();
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
                component.OnUnload();
            }
            foreach (var child in _children)
            {
                child._Unload();
            }
        }
        protected void _Update(float deltaTime)
        {
            Console.WriteLine("update obj");
            Console.WriteLine(_children.Count);
            _UpdateTransform();
            foreach (var component in _components)
            {
                component.OnUpdate(deltaTime);
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
