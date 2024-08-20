using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trigraphic_GameEngineV1
{
    internal sealed class RootGameObject : GameObject
    {
        public RootGameObject(out Action rootLoad, out Action rootUnload, out Action<float> rootUpdate) : base(null, new Component[0])
        {
            rootLoad = _Load;
            rootUnload = _Unload;
            rootUpdate = _Update;
        }

        #region disable redundant funcionality
        public new GameObject? Parent
        {
            get => null;
            set => throw new InvalidOperationException("rootgameobject cannot have a parent");
        }
        public new void AddComponent(Component component)
        {
            throw new InvalidOperationException("rootgameobject cannot have components");
        }

        public new void RemoveComponent(Component component)
        {
            throw new InvalidOperationException("rootgameobject cannot have components");
        }
        #endregion
    }
}
