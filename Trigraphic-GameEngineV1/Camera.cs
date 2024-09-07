using OpenTK.Mathematics;

namespace Trigraphic_GameEngineV1
{
    internal sealed class Camera
    {
        public Camera(bool orthographic = false)
        {
            if (orthographic)
                SetOrthographicCamera();
            else
                SetPerspectiveCamera();
        }

        public bool IsOrthographic { get; private set; }

        #region perspective camera
        float _fov, _near, _far;
        public void SetPerspectiveCamera(float fov = MathHelper.PiOver4, float near = 0.01f, float far = 100)
        {
            _fov = fov;
            _near = near;
            _far = far;
            IsOrthographic = false;
            _aspectRatioCache = 0;
        }
        #endregion

        #region orthographic camera
        public float ViewportHeight { get; private set; }
        float _nearOrtho, _farOrtho;
        public void SetOrthographicCamera(float height = 10, float near = 0, float far = 10)
        {
            ViewportHeight = height;
            _nearOrtho = near;
            _farOrtho = far;
            IsOrthographic = true;
            _aspectRatioCache = 0;
        }
        #endregion

        #region camera rig logic
        CameraRig? _cameraRig;

        public Vector3 Position  => _cameraRig?.gameObject.GlobalPosition ?? Vector3.Zero;

        List<CameraRig> _cameraRigList = new();

        public void AddToCameraRigList(CameraRig cameraRig)
        {
            if (_cameraRigList.Contains(cameraRig)) throw new InvalidOperationException("camera rig already in list");

            _cameraRigList.Add(cameraRig);

            if(_cameraRig == null) _cameraRig = cameraRig;
        }
        public void RemoveFromCameraRigList(CameraRig cameraRig)
        {
            if (!_cameraRigList.Remove(cameraRig)) throw new InvalidOperationException("camera rig not in list");

            if (_cameraRig == cameraRig) 
                _cameraRig = _cameraRigList.Any() ? _cameraRigList[0] : null;
        }
        #endregion

        #region matrix logic
        static readonly Vector3 _UP = Vector3.UnitY;
        static readonly Vector3 _FORWARD = -Vector3.UnitZ;

        Matrix4 _viewMatrix = Matrix4.Identity;
        Quaternion _rotationCache;
        Vector3 _positionCache;
        public ref Matrix4 GetViewMatrixRef()
        {
            if (_cameraRig == null) return ref _viewMatrix;

            if (!_positionCache.Equals(_cameraRig.gameObject.GlobalPosition) || !_rotationCache.Equals(_cameraRig.gameObject.GlobalRotation))
            {
                _rotationCache = _cameraRig.gameObject.GlobalRotation;
                _positionCache = _cameraRig.gameObject.GlobalPosition;

                var cameraDirection = _rotationCache * _FORWARD;
                var cameraRight = Vector3.Normalize(Vector3.Cross(_UP, cameraDirection));
                var cameraUp = Vector3.Cross(cameraDirection, cameraRight);

                var mat0 = new Matrix4(new Vector4(-cameraRight, 0), new Vector4(cameraUp, 0), new Vector4(-cameraDirection, 0), new Vector4(Vector3.Zero, 1));
                var mat1 = Matrix4.Identity;
                mat1.Column3 = new Vector4(-_positionCache, 1);

                var final = mat0 * mat1;
                final.Transpose();

                _viewMatrix = final;
            }

            return ref _viewMatrix;
        }

        Matrix4 _projectionMatrix;
        float _aspectRatioCache;
        public ref Matrix4 GetProjectionMatrixRef()
        {
            if (_aspectRatioCache != EngineWindow.AspectRatio)
            {
                _aspectRatioCache = EngineWindow.AspectRatio;
                _projectionMatrix = IsOrthographic
                    ? Matrix4.CreateOrthographic(ViewportHeight * _aspectRatioCache, ViewportHeight, _nearOrtho, _farOrtho)
                    : Matrix4.CreatePerspectiveFieldOfView(_fov, _aspectRatioCache, _near, _far);
            }
            return ref _projectionMatrix;
        }
        #endregion
    }
}
