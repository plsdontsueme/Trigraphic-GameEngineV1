using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trigraphic_GameEngineV1
{
    internal abstract class ComponentDynamic : ComponentStatic
    {
        protected override void RegisterUpdate()
        {
            UpdateSystem.ComponentUpdateAction += _Update;
        }

        protected override void UnregisterUpdate()
        {
            UpdateSystem.ComponentUpdateAction -= _Update;
        }

        void _Update(float deltaTime)
        {
            if (!_isLoaded) throw new InvalidOperationException("unloaded component cannot be updated");
            OnUpdate(deltaTime);
        }

        protected abstract void OnUpdate(float deltaTime);
    }
}
