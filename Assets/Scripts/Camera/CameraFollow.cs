using UnityEngine;

namespace SharkGame
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;            // The Transform of the target the camera will follow
        public Vector3 offset;             // The offset from the target position
        public float smoothSpeed = 0.25f;  // The speed at which the camera smooths to its position (adjust for desired smoothness)

        private Vector3 velocity = Vector3.zero;

        void LateUpdate()  // Use FixedUpdate to align with Rigidbody updates
        {
            if (target == null)
            {
                Debug.LogWarning("Target Transform is not assigned.");
                return;
            }

            // Calculate the desired position based on the target's Transform position + offset
            Vector3 desiredPosition = target.position + offset;

            // Smoothly interpolate between the camera's current position and the desired position
            Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

            // Set the camera's position to the smoothed position
            transform.position = smoothedPosition;
        }

        void Start()
        {
            // You can add additional initialization here if needed
            if (target != null)
            {
                target.gameObject.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
            }
        }
    }
}
