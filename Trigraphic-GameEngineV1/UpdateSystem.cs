

namespace Trigraphic_GameEngineV1
{
    internal static class UpdateSystem
    {
        static Queue<GameObject> _transformUpdateQueue = new();
        public static void EnqueueTransformUpdate(GameObject obj)
        {
            if (_transformUpdateQueue.Contains(obj)) throw new InvalidOperationException("obj already in queue");
            _transformUpdateQueue.Enqueue(obj);
        }
        public static void ProcessTransformUpdateQueue()
        {
            while (_transformUpdateQueue.Count > 0)
            {
                var obj = _transformUpdateQueue.Dequeue();
                obj.UpdateTransform();
            }
        }

        public static event Action<float>? ComponentUpdateAction;
        public static void UpdateComponents(float deltaTime)
        {
            ComponentUpdateAction?.Invoke(deltaTime);
        }
    }
}
