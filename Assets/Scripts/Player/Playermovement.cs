using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Moves the player on the X/Z ground plane using an abstracted movement input source.
    /// Frame-rate independent. Y position is left untouched (no gravity/physics in this prototype).
    ///
    /// To switch control schemes later (e.g. touch/joystick), implement IMovementInput
    /// and change the single line in Awake() below. No other code here needs to change.
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    {
        [Tooltip("Movement speed in world units per second.")]
        [SerializeField] private float moveSpeed = 5f;

        private IMovementInput _input;

        private void Awake()
        {
            // The ONLY line to change when swapping to touch/joystick input later.
            _input = new KeyboardMovementInput();
        }

        private void Update()
        {
            Vector2 raw = _input.ReadMovement();

            // Map 2D input (x = horizontal, y = vertical) onto the X/Z plane.
            Vector3 direction = new Vector3(raw.x, 0f, raw.y);

            // Keep diagonal movement from being faster than cardinal movement.
            if (direction.sqrMagnitude > 1f)
                direction.Normalize();

            // Frame-rate independent movement.
            transform.position += direction * (moveSpeed * Time.deltaTime);
        }
    }
}
