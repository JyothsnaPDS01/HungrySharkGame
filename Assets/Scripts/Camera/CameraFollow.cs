using UnityEngine;

namespace SharkGame
{
    public class CameraFollow : MonoBehaviour
    {
        public Rigidbody targetRigidbody;  // The Rigidbody of the target the camera will follow
        public Vector3 offset;             // The offset from the target position
        public float smoothSpeed = 0.125f; // Speed at which the camera smooths to its position

        private Vector3 velocity = Vector3.zero; // Velocity for smoothing

        private void LateUpdate()
        {
            if (targetRigidbody == null)
            {
                Debug.LogWarning("Target Rigidbody is not assigned.");
                return;
            }

            // Calculate the desired position based on the target's Rigidbody position + offset
            Vector3 desiredPosition = targetRigidbody.position + offset;

            // Debug information for tracking positions
            Debug.Log($"Target Position: {targetRigidbody.position}");
            Debug.Log($"Desired Camera Position: {desiredPosition}");

            // Smoothly interpolate between the camera's current position and the desired position
            Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

            // Set the camera's position to the smoothed position
            transform.position = smoothedPosition;

            // Optionally, make the camera look at the target
            // transform.LookAt(targetRigidbody.transform);
        }

        private void Start()
        {
            if (targetRigidbody != null)
            {
                // Ensure the target's Rigidbody is set to interpolate to smooth out visual movement
                targetRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }
    }
}
