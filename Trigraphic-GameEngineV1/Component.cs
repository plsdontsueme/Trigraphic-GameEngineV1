
namespace Trigraphic_GameEngineV1
{
    internal class Component
    {
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

        #region event logic
        protected bool _isLoaded;
        public void Load()
        {
            if (_isLoaded)
                EngineDebugManager.throwNewOperationRedundancyWarning("component already loaded");
            _isLoaded = true;
            OnLoad();
        }
        public void Unload()
        {
            if (!_isLoaded)
                EngineDebugManager.throwNewOperationRedundancyWarning("component already unloaded");
            _isLoaded = false;
            OnUnload();
        }
        public void Update(float deltaTime) => OnUpdate(deltaTime);
        protected virtual void OnLoad() { }
        protected virtual void OnUnload() { }
        protected virtual void OnUpdate(float deltaTime) { }
        #endregion
    }
}
