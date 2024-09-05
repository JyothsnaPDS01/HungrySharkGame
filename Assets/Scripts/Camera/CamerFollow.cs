using UnityEngine;

namespace SharkGame
{
    public class CameraFollow : MonoBehaviour
    {
        public Rigidbody targetRigidbody;  // The Rigidbody of the target the camera will follow
        public Vector3 offset;             // The offset from the target position
        public float smoothSpeed = 0.05f;  // The speed at which the camera smooths to its position (adjust for desired smoothness)
        private Vector3 velocity = Vector3.zero;

        // New variables for managing camera position locking
        private bool isCameraYLocked = false; // Flag to check if camera Y is locked
        private float lockedYPosition = 0.01f; // The fixed Y position when locked

        void FixedUpdate()  // Use FixedUpdate to align with Rigidbody updates
        {
            if (targetRigidbody == null)
            {
                Debug.LogWarning("Target Rigidbody is not assigned.");
                return;
            }

            // Determine whether the camera Y position should be locked
            if (targetRigidbody.position.y >= 0f)
            {
                // If shark is at or above the surface, lock the camera Y position
                isCameraYLocked = true;
            }
            else
            {
                // If the shark is below the surface, unlock the camera Y position
                isCameraYLocked = false;
            }

            // Calculate the desired position based on the target's Rigidbody position + offset
            Vector3 desiredPosition = targetRigidbody.position + offset;

            // If the camera Y position is locked, override the Y component to stay fixed
            if (isCameraYLocked)
            {
                desiredPosition.y = lockedYPosition; // Set camera Y to locked position
            }

            // Smoothly interpolate between the camera's current position and the desired position
            Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

            // Set the camera's position to the smoothed position
            transform.position = smoothedPosition;
        }

        void Start()
        {
            // Ensure the player's Rigidbody is set to interpolate to smooth out visual movement
            if (targetRigidbody != null)
            {
                targetRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }
    }
}
