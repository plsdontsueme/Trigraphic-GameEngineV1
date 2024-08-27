using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trigraphic_GameEngineV1
{
    internal class PlayerBehaviour : Component
    {
        public PlayerBehaviour(TextRenderer text)
        {
            _text = text;
        }
        TextRenderer _text;
        GameObject _cam;
        protected override void OnLoad()
        {
            _cam = gameObject.GetComponentInChildren<Camera>().gameObject;
            InputManager.MouseMove += InputManager_MouseMove;
            InputManager.GrabCursor();
        }

        Vector2d sensitivity = new(0.001f, 0.001f);
        Vector2d virtualMouse;
        Vector2 lastMouse;
        bool first = true;
        private void InputManager_MouseMove(OpenTK.Windowing.Common.MouseMoveEventArgs e)
        {
            if (!InputManager.CursorGrabbed) return;

            if (first)
            {
                lastMouse = e.Position;
                first = false;
            }
            virtualMouse.X -= ((double)e.Position.X - lastMouse.X) * sensitivity.X;
            virtualMouse.Y = MathHelper.Clamp(virtualMouse.Y - ((double)e.Position.Y - lastMouse.Y) * sensitivity.Y, -1.569d, 1.569d);
            _cam.Rotation = Quaternion.FromAxisAngle(Vector3.UnitX, (float)virtualMouse.Y);
            gameObject.Rotation = Quaternion.FromAxisAngle(Vector3.UnitY, (float)virtualMouse.X);
            lastMouse = e.Position;
        }

        float fpsHigh, fpsLow;
        float resetTime = float.MaxValue;
        float? fpsAvr = null;
        protected override void OnUpdate(float deltaTime)
        {
            var fps = 1f / deltaTime;

            resetTime += deltaTime;
            if (resetTime > 2)
            {
                resetTime = 0;
                fpsHigh = 0;
                fpsLow = float.MaxValue;
                if (fpsAvr == null) fpsAvr = fps;
                else fpsAvr = (fpsAvr + fps) / 2;
            }

            if (fps > fpsHigh) fpsHigh = fps;
            else if (fps < fpsLow) fpsLow = fps;
            _text.Text =
                $"fps avr: {(int)fpsAvr.Value}{Environment.NewLine}" +
                $"fps ^: {(int)fpsHigh}{Environment.NewLine}" +
                $"fps v: {(int)fpsLow}{Environment.NewLine}" +
                $"x: {(int)gameObject.GlobalPosition.X}{Environment.NewLine}" +
                $"y: {(int)gameObject.GlobalPosition.Y}{Environment.NewLine}" +
                $"z: {(int)gameObject.GlobalPosition.Z}{Environment.NewLine}";

            if (InputManager.GetKeyDown(Keys.Tab)) { InputManager.GrabCursor(!InputManager.CursorGrabbed); first = true; }
            if (!InputManager.CursorGrabbed) return;

            Vector3 movement = Vector3.Zero;
            if (InputManager.GetKey(Keys.W)) movement.Z -= 1;
            if (InputManager.GetKey(Keys.S)) movement.Z += 1;
            if (InputManager.GetKey(Keys.A)) movement.X -= 1;
            if (InputManager.GetKey(Keys.D)) movement.X += 1;
            if (InputManager.GetKey(Keys.LeftShift)) movement.Y -= 1;
            if (InputManager.GetKey(Keys.Space)) movement.Y += 1;
            if (movement.Length == 0) return;
            movement.Normalize();
            gameObject.Position += (gameObject.Rotation * movement * deltaTime * 3);
        }
    }
}
