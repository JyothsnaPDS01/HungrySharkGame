using UnityEngine;
using SharkGame.Models;

namespace SharkGame
{
    public class CameraFollow : MonoBehaviour
    {
        public Rigidbody targetRigidbody;  // The Rigidbody of the target the camera will follow
        public Vector3 offset;             // The offset from the target position
        public float smoothSpeed = 0.125f; // Speed at which the camera smooths to its position

        private Vector3 velocity = Vector3.zero; // Velocity for smoothing

        private bool isFollowing = false;


        #region Events
        private void OnEnable()
        {
            SharkGameManager.Instance.OnGameModeChanged += HandleGameMode;
        }

        private void OnDisable()
        {
            SharkGameManager.Instance.OnGameModeChanged -= HandleGameMode;
        }

        public void HandleGameMode(SharkGameDataModel.GameMode currentGameMode)
        {
            if(currentGameMode == SharkGameDataModel.GameMode.GameStart)
            {
                isFollowing = true;
                InitialisePlayerProperties();
            }
        }
        private void InitialisePlayerProperties()
        {
            if (targetRigidbody != null)
            {
                // Ensure the target's Rigidbody is set to interpolate to smooth out visual movement
                targetRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }
        #endregion

        private void LateUpdate()
        {
            if (isFollowing == true)
            {
                if (targetRigidbody == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning("Target Rigidbody is not assigned.");
#endif
                    return;
                }
               
                
                    // Calculate the desired position based on the target's Rigidbody position + offset
                    Vector3 desiredPosition = targetRigidbody.position + offset;

                    // Smoothly interpolate between the camera's current position and the desired position
                    Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

                // Set the camera's position to the smoothed position
                transform.position = smoothedPosition;

            }
        }
    }
}
