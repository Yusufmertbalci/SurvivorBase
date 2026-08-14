#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Reads WASD and arrow-key input.
    /// Uses Unity's Input System when it is the active input handler, and falls back to the
    /// legacy Input Manager otherwise, so this compiles and runs under any project setting.
    /// This is a plain C# class (not a MonoBehaviour) so it stays cheap and swappable.
    /// </summary>
    public class KeyboardMovementInput : IMovementInput
    {
        public Vector2 ReadMovement()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return Vector2.zero;

            float x = 0f;
            float y = 0f;

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) y -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) y += 1f;

            return new Vector2(x, y);
#else
            // Legacy Input Manager: "Horizontal"/"Vertical" already map WASD + arrow keys by default.
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            return new Vector2(x, y);
#endif
        }
    }
}
