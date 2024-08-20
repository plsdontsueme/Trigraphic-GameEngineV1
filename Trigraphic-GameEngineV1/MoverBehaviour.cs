using OpenTK.Mathematics;

namespace Trigraphic_GameEngineV1
{
    internal class MoverBehaviour : Component
    {
        Vector3? _translation;
        Vector3? _rotationDirection;
        Vector3? _stretchDirection;
        public MoverBehaviour(Vector3? translation = null, Vector3? rotationDirection = null, Vector3? stretchDirection = null)
        {
            _translation = translation;
            _rotationDirection = rotationDirection;
            _stretchDirection = stretchDirection;
        }

        float time;
        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
            if (_translation.HasValue) gameObject.Position += _translation.Value * deltaTime;
            time += deltaTime;
            if (_rotationDirection.HasValue) gameObject.Rotation = Quaternion.FromEulerAngles(_rotationDirection.Value * time);
            if (_stretchDirection.HasValue) gameObject.Scale = Vector3.One + _stretchDirection.Value * 0.5f * (float)MathHelper.Cos(time);
        }
    }
}
