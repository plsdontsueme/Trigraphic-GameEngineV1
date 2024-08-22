using OpenTK.Mathematics;

namespace Trigraphic_GameEngineV1
{
    internal sealed class Camera : Component
    {
        #region main camera logic
        static Dictionary<int, Camera> _mainCameras = new();
        int? cameraSlot;
        public static Camera? MainCamera(int slot = 0) => _mainCameras[slot];
        public void SetMainCamera(int slot = 0)
        {
            if (_mainCameras.ContainsKey(slot))
            {
                _mainCameras[slot].cameraSlot = null;
                _mainCameras.Remove(slot);
            }
            _mainCameras.Add(slot, this);
            cameraSlot = slot;
        }
        protected override void OnLoad()
        {
            if (!_mainCameras.ContainsKey(0))
            {
                _mainCameras.Add(0, this);
                cameraSlot = 0;
            }
        }
        protected override void OnUnload()
        {
            if (cameraSlot.HasValue)
            {
                _mainCameras.Remove(cameraSlot.Value);
                cameraSlot = null;
            } 
        }
        #endregion


        static readonly Vector3 UP = Vector3.UnitY;
        static readonly Vector3 FORWARD = -Vector3.UnitZ;
        float _fov = float.Pi * 0.4f;
        float _near = 0.01f;
        float _far = 100;

        Matrix4 _viewMatrix;
        Quaternion _rotationCheck;
        Vector3 _positionCheck;
        public ref Matrix4 GetViewMatrixRef()
        {
            if (_positionCheck != gameObject.GlobalPosition || _rotationCheck != gameObject.GlobalRotation)
            {
                _rotationCheck = gameObject.GlobalRotation;
                _positionCheck = gameObject.GlobalPosition;

                var cameraDirection = gameObject.GlobalRotation * FORWARD;
                var cameraRight = Vector3.Normalize(Vector3.Cross(UP, cameraDirection));
                var cameraUp = Vector3.Cross(cameraDirection, cameraRight);

                var mat0 = new Matrix4(new Vector4(-cameraRight, 0), new Vector4(cameraUp, 0), new Vector4(-cameraDirection, 0), new Vector4(Vector3.Zero, 1));
                var mat1 = Matrix4.Identity;
                mat1.Column3 = new Vector4(-gameObject.GlobalPosition, 1);

                var final = mat0 * mat1;
                final.Transpose();

                _viewMatrix = final;
            }

            return ref _viewMatrix;
        }

        Matrix4 _projectionMatrix;
        float _aspectRatioCheck;
        public ref Matrix4 GetProjectionMatrixRef()
        {
            if (_aspectRatioCheck != EngineWindow.aspectRatio)
            {
                _aspectRatioCheck = EngineWindow.aspectRatio;
                _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(_fov, EngineWindow.aspectRatio, _near, _far);
            }         
            return ref _projectionMatrix;
        }

    }
}
