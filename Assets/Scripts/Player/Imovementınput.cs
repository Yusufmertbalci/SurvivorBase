using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Abstraction over a movement input source.
    /// Returns a 2D vector where x = horizontal (-1..1) and y = vertical/forward (-1..1).
    /// Implement this for keyboard now, and for touch/joystick later, without changing PlayerMovement.
    /// </summary>
    public interface IMovementInput
    {
        Vector2 ReadMovement();
    }
}
