using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trigraphic_GameEngineV1
{
    internal sealed class RootGameObject : GameObject
    {
        public RootGameObject(out Action rootLoad, out Action rootUnload, out Action<float> rootUpdate) 
            : base([],null)
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
        public new T? GetComponent<T>() where T : Component
        {
            throw new InvalidOperationException("rootgameobject cannot have components");
        }
        public new ref Matrix4 GetModelMatrixRef()
        {
            throw new InvalidOperationException("rootgameobject cannot have render element");
        }
        #endregion
    }
}
