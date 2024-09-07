
namespace Trigraphic_GameEngineV1
{
    internal abstract class ComponentStatic
    {
        public GameObject gameObject { get; private set; }

        #region assignment logic
        bool _assigned;
        public void ObjAssign(GameObject obj)
        {
            if (_assigned)
                throw new InvalidOperationException("component already assigned");
            _assigned = true;
            gameObject = obj;
        }
        #endregion

        #region prefab copy logic
        public ComponentStatic ShallowCopy()
        {
            if (!_assigned || !gameObject.IsPrefab) throw new InvalidOperationException("cant copy a component not assigned to a prefab");

            var copy = (ComponentStatic)this.MemberwiseClone();
            copy._assigned = false;

            return copy;
        }
        #endregion

        #region event logic
        protected bool _isLoaded;
        public void Load()
        {
            if (!gameObject.IsLoaded) throw new InvalidOperationException("component of an unloaded gameobject cannot be loaded");
            if (_isLoaded)
                throw new InvalidOperationException("component already loaded");
            _isLoaded = true;
            RegisterUpdate();
            OnLoad();
        }
        public void Unload()
        {
            if (!_isLoaded)
                throw new InvalidOperationException("component already unloaded");
            _isLoaded = false;
            UnregisterUpdate();
            OnUnload();
        }

        protected abstract void OnLoad();
        protected abstract void OnUnload();
        protected virtual void RegisterUpdate() { }
        protected virtual void UnregisterUpdate() { }
        #endregion
    }
}
