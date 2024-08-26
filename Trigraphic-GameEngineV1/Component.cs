
namespace Trigraphic_GameEngineV1
{
    internal class Component
    {
        protected Component() { }
        public GameObject gameObject {  get; private set; }

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
        public Component ShallowCopy()
        {
            if (!_assigned || !gameObject.isPrefab) throw new InvalidOperationException("cant copy a component not assigned to a prefab");
            
            var copy = (Component)this.MemberwiseClone();
            copy._assigned = false;

            return copy;
        }
        #endregion

        #region event logic
        protected bool _isLoaded;
        public void Load()
        {
            if (!gameObject.isLoaded) throw new InvalidOperationException("component of an unloaded gameobject cannot be loaded");
            if (_isLoaded)
                throw new InvalidOperationException("component already loaded");
            _isLoaded = true;
            OnLoad();
        }
        public void Unload()
        {
            if (!gameObject.isLoaded) throw new InvalidOperationException("component of an unloaded gameobject cannot be unloaded");
            if (!_isLoaded)
                throw new InvalidOperationException("component already unloaded");
            _isLoaded = false;
            OnUnload();
        }
        public void Update(float deltaTime)
        {
            if (!_isLoaded) throw new InvalidOperationException("unloaded component cannot be updated");
            OnUpdate(deltaTime);
        }
        protected virtual void OnLoad() { }
        protected virtual void OnUnload() { }
        protected virtual void OnUpdate(float deltaTime) { }
        #endregion
    }
}
