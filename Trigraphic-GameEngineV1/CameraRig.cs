using OpenTK.Mathematics;

namespace Trigraphic_GameEngineV1
{
    internal sealed class CameraRig : ComponentStatic
    {
        Camera _targetCamera;

        public CameraRig(Camera targetCamera)
        {
            _targetCamera = targetCamera;
        }

        protected override void OnLoad()
        {
            _targetCamera.AddToCameraRigList(this);
        }

        protected override void OnUnload()
        {
            _targetCamera.RemoveFromCameraRigList(this);
        }
    }
}
