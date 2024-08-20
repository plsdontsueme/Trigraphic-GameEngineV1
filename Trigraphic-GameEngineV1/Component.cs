using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public virtual void OnLoad()
        {
            if (_isLoaded)
                EngineDebugManager.throwNewOperationRedundancyWarning("component already loaded");
            _isLoaded = true;
        }
        public virtual void OnUnload()
        {
            if (!_isLoaded)
                EngineDebugManager.throwNewOperationRedundancyWarning("component already unloaded");
            _isLoaded = false;
        }
        public virtual void OnUpdate(float deltaTime) { }
        #endregion
    }
}
