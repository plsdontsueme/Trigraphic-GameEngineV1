using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trigraphic_GameEngineV1
{
    internal sealed class Camera : Component
    {
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
        }
        public override void OnLoad()
        {
            base.OnLoad();
            if (!_mainCameras.ContainsKey(0))
            {
                _mainCameras.Add(0, this);
                cameraSlot = 0;
            }
        }
        public override void OnUnload()
        {
            base.OnUnload();
            if (cameraSlot.HasValue)
                _mainCameras.Remove(cameraSlot.Value);
        }


        static Vector3 up = Vector3.UnitY;
        static Vector3 forward = -Vector3.UnitZ;
        float fov = float.Pi * 0.4f;
        float near = 0.01f;
        float far = 100;

        public Matrix4 GetViewMatrix()
        {
            Vector3 cameraDirection = gameObject.GlobalRotation * forward;
            Vector3 cameraRight = Vector3.Normalize(Vector3.Cross(up, cameraDirection));
            Vector3 cameraUp = Vector3.Cross(cameraDirection, cameraRight);

            Matrix4 mat0 = new Matrix4(new Vector4(-cameraRight, 0), new Vector4(cameraUp, 0), new Vector4(-cameraDirection, 0), new Vector4(Vector3.Zero, 1));
            Matrix4 mat1 = Matrix4.Identity;
            mat1.Column3 = new Vector4(-gameObject.GlobalPosition, 1);

            Matrix4 final = mat0 * mat1;
            final.Transpose();

            return final;
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(fov, EngineWindow.aspectRatio, near, far);
        }

    }
}
