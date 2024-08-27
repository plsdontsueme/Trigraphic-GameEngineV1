using OpenTK.Mathematics;

namespace Trigraphic_GameEngineV1
{
    internal sealed class Camera : Component
    {
        #region main camera logic
        static Dictionary<int, Camera> _mainCameras = new();
        int? _cameraSlot;
        public Camera(int mainCameraSlotTarget = 0, bool isOrthographic = false)
        {
            _cameraSlot = mainCameraSlotTarget;
            _isOrthographic = isOrthographic;
        }

        public static Camera? MainCamera(int slot = 0) => _mainCameras[slot];
        public void SetMainCamera(int slot = 0)
        {
            if (_mainCameras.ContainsKey(slot))
            {
                _mainCameras[slot]._cameraSlot = null;
                _mainCameras.Remove(slot);
            }
            _mainCameras.Add(slot, this);
            _cameraSlot = slot;
        }
        protected override void OnLoad()
        {
            if (_cameraSlot.HasValue && !_mainCameras.ContainsKey(_cameraSlot.Value))
            {
                _mainCameras.Add(_cameraSlot.Value, this);
            }
            else
            {
                _cameraSlot = null;
            }
        }
        protected override void OnUnload()
        {
            if (_cameraSlot.HasValue)
            {
                _mainCameras.Remove(_cameraSlot.Value);
                _cameraSlot = null;
            } 
        }
        #endregion

        #region perspective camera
        static readonly Vector3 _UP = Vector3.UnitY;
        static readonly Vector3 _FORWARD = -Vector3.UnitZ;

        float _fov = float.Pi * 0.4f;
        float _near = 0.01f;
        float _far = 100;
        public void SetPerspectiveCamera(float? fov = null, float? near = null, float? far = null)
        {
            if (fov.HasValue) _fov = fov.Value;
            if (near.HasValue) _near = near.Value;
            if (far.HasValue) _far = far.Value;
            _isOrthographic = false;
            _aspectRatioCheck = 0;
        }
        #endregion

        bool _isOrthographic;

        #region orthographic camera
        float _height = 20;
        float _nearOrtho = 0f;
        float _farOrtho = 10;
        public void SetOrthographicCamera(float? height = null, float? near = null, float? far = null)
        {
            if (height.HasValue) _height = height.Value;
            if (near.HasValue) _nearOrtho = near.Value;
            if (far.HasValue) _farOrtho = far.Value;
            _isOrthographic = true;
            _aspectRatioCheck = 0;
        }
        #endregion

        #region matrix logic
        Matrix4 _viewMatrix;
        Quaternion _rotationCheck;
        Vector3 _positionCheck;
        public ref Matrix4 GetViewMatrixRef()
        {
            if (_positionCheck != gameObject.GlobalPosition || _rotationCheck != gameObject.GlobalRotation)
            {
                _rotationCheck = gameObject.GlobalRotation;
                _positionCheck = gameObject.GlobalPosition;

                var cameraDirection = gameObject.GlobalRotation * _FORWARD;
                var cameraRight = Vector3.Normalize(Vector3.Cross(_UP, cameraDirection));
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
                _projectionMatrix = _isOrthographic 
                    ? Matrix4.CreateOrthographic(_height * _aspectRatioCheck, _height, _nearOrtho, _farOrtho)
                    : Matrix4.CreatePerspectiveFieldOfView(_fov, _aspectRatioCheck, _near, _far);
            }         
            return ref _projectionMatrix;
        }
        #endregion
    }
}
