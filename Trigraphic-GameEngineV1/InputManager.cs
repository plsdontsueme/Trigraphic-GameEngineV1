using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Trigraphic_GameEngineV1
{
    internal static class InputManager
    {
        static EngineWindow _engineWindow;
        static bool _engineWindowAssigned;
        public static void EngineWindowAssign(EngineWindow instance)
        {
            if (_engineWindowAssigned == true)
                throw new InvalidOperationException("enginewindow already assigned");
            _engineWindowAssigned = true;
            _engineWindow = instance;
        }



        public static bool GetKey() => _engineWindow.KeyboardState.IsAnyKeyDown;
        public static bool GetKey(Keys key) => _engineWindow.KeyboardState.IsKeyDown(key);
        public static bool GetKeyDown(Keys key) => _engineWindow.KeyboardState.IsKeyPressed(key);
        public static bool GetKeyUp(Keys key) => _engineWindow.KeyboardState.IsKeyReleased(key);

        public static bool GetMouse() => _engineWindow.MouseState.IsAnyButtonDown;
        public static bool GetMouse(MouseButton button) => _engineWindow.MouseState.IsButtonDown(button);
        public static bool GetMouseDown(MouseButton button) => _engineWindow.MouseState.IsButtonPressed(button);
        public static bool GetMouseUp(MouseButton button) => _engineWindow.MouseState.IsButtonReleased(button);

        public static Vector2 MousePosition => _engineWindow.MousePosition;
        public static Vector2 MousePositionCameraSpace(Camera camera)
        {
            if (!camera.IsOrthographic) throw new ArgumentException("camera must be orthographic");

            var x = (MousePosition.X * EngineWindow.InverseSize.X - .5f) * camera.ViewportHeight * EngineWindow.AspectRatio;
            var y = (MousePosition.Y * EngineWindow.InverseSize.Y - .5f) * camera.ViewportHeight;
            return new Vector2(x, -y);
        }


        public static bool CursorGrabbed { get; private set; }
        public static void GrabCursor(bool state = true)
        {
            if (state) _engineWindow.CursorState = CursorState.Grabbed;
            else _engineWindow.CursorState = CursorState.Normal;

            CursorGrabbed = state;
        }

        public static event Action<TextInputEventArgs> TextInput
        {
            add
            {
                _engineWindow.TextInput += value;
            }
            remove
            {
                _engineWindow.TextInput -= value;
            }
        }

        public static event Action<KeyboardKeyEventArgs> KeyDown
        {
            add
            {
                _engineWindow.KeyDown += value;
            }
            remove
            {
                _engineWindow.KeyDown -= value;
            }
        }
        public static event Action<KeyboardKeyEventArgs> KeyUp
        {
            add
            {
                _engineWindow.KeyUp += value;
            }
            remove
            {
                _engineWindow.KeyUp -= value;
            }
        }

        public static event Action<MouseMoveEventArgs> MouseMove
        {
            add
            {
                _engineWindow.MouseMove += value;
            }
            remove
            {
                _engineWindow.MouseMove -= value;
            }
        }
        public static event Action<MouseButtonEventArgs> MouseDown
        {
            add
            {
                _engineWindow.MouseDown += value;
            }
            remove
            {
                _engineWindow.MouseDown -= value;
            }
        }
        public static event Action<MouseButtonEventArgs> MouseUp
        {
            add
            {
                _engineWindow.MouseUp += value;
            }
            remove
            {
                _engineWindow.MouseUp -= value;
            }
        }
        public static event Action<MouseWheelEventArgs> MouseWheel
        {
            add
            {
                _engineWindow.MouseWheel += value;
            }
            remove
            {
                _engineWindow.MouseWheel -= value;
            }
        }
    }
}
