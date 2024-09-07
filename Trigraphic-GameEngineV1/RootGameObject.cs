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
        public RootGameObject(out Action rootLoad, out Action rootUnload) 
            : base([],null)
        {
            rootLoad = _Load;
            rootUnload = _Unload;
        }

        #region disable redundant funcionality
        public new Vector3? Position
        {
            get => null;
            set => throw new InvalidOperationException("rootgameobject cannot have a position");
        }
        public new Vector3? Scale
        {
            get => null;
            set => throw new InvalidOperationException("rootgameobject cannot have a scale");
        }
        public new Quaternion? Rotation
        {
            get => null;
            set => throw new InvalidOperationException("rootgameobject cannot have a rotation");
        }
        public new GameObject? Parent
        {
            get => null;
            set => throw new InvalidOperationException("rootgameobject cannot have a parent");
        }
        public new void AddComponent(ComponentStatic component)
        {
            throw new InvalidOperationException("rootgameobject cannot have components");
        }
        public new void RemoveComponent(ComponentStatic component)
        {
            throw new InvalidOperationException("rootgameobject cannot have components");
        }
        public new T? GetComponent<T>() where T : ComponentStatic
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
