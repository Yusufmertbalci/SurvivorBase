using UnityEngine;

namespace Game.CameraSystem
{
    /// <summary>
    /// Smoothly follows a target while preserving the camera's current framing:
    /// the offset (including height) is captured on Start from the camera's position
    /// relative to the target, so the initial visual composition is unchanged.
    ///
    /// The camera's rotation is never modified, so the authored angled top-down view is
    /// preserved and the camera does not rotate with the player. Attach to the Main Camera.
    /// Do NOT parent the camera to the player.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Tooltip("The Transform the camera follows. Assign the Player here.")]
        [SerializeField] private Transform target;

        [Tooltip("Approximate time (seconds) to catch up to the target. " +
                 "Lower = snappier, higher = smoother/laggier.")]
        [SerializeField] private float smoothTime = 0.2f;

        private Vector3 _offset;
        private Vector3 _velocity = Vector3.zero;

        private void Start()
        {
            if (target == null)
            {
                Debug.LogWarning(
                    $"{nameof(CameraFollow)}: No target assigned. " +
                    "Assign the Player to the Target field in the Inspector.", this);
                enabled = false;
                return;
            }

            // Capture the current framing (offset + height) so we don't change how the scene looks.
            _offset = transform.position - target.position;
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            // Maintain the captured offset. Since the player moves on X/Z and keeps its Y,
            // the camera follows X/Z while its height stays constant.
            Vector3 desiredPosition = target.position + _offset;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref _velocity,
                smoothTime);

            // Rotation is intentionally left untouched to preserve the angled top-down view.
        }
    }
}
