using UnityEngine;
using SharkGame.Models;

namespace SharkGame
{
    public class CameraFollow : MonoBehaviour
    {
        public Rigidbody targetRigidbody;  // The Rigidbody of the target the camera will follow
        public Vector3 offset;             // The offset from the target position
        public float smoothSpeed = 0.125f; // Speed at which the camera smooths to its position

        public Player _playerShark;

        private Vector3 velocity = Vector3.zero; // Velocity for smoothing

        private bool isFollowing = false;

        #region Events
        private void OnEnable()
        {
            SharkGameManager.Instance.OnGameModeChanged += HandleGameMode;
        }

        private void OnDisable()
        {
            SharkGameManager.Instance.OnGameModeChanged += HandleGameMode;
        }

        private void HandleGameMode(SharkGameDataModel.GameMode currentGameMode)
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
                if (_playerShark.InitialMovement)
                {
                    if (targetRigidbody.transform.position.y >= -43f && targetRigidbody.transform.position.y <= -20f && targetRigidbody.transform.position.x >= -50f && targetRigidbody.transform.position.x <= 100f)
                    {
                       
                            Debug.Log("On Y clamping");
                            // Calculate the desired position based on the target's Rigidbody position + offset
                            Vector3 desiredPosition = targetRigidbody.position + offset;

                            // Smoothly interpolate between the camera's current position and the desired position
                            Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

                            // Set the camera's position to the smoothed position
                            transform.position = smoothedPosition;

                    }
                    else
                    {
                        Debug.Log("Targetposition" + Mathf.Round(targetRigidbody.transform.position.y));
                        if(targetRigidbody.transform.position.y >=-43f && targetRigidbody.transform.position.y <= -20f && Mathf.Round(targetRigidbody.transform.position.x) == -53f)
                        {
                            Debug.Log("On Y else case");

                            // Calculate the desired position based on the target's Rigidbody position + offset
                            Vector3 desiredPosition = targetRigidbody.position + offset;

                            // Smoothly interpolate between the camera's current position and the desired Y position
                            // Keep the X and Z positions the same, only update the Y position
                            Vector3 smoothedPosition = Vector3.SmoothDamp(
                                new Vector3(transform.position.x, transform.position.y, transform.position.z),
                                new Vector3(transform.position.x, desiredPosition.y, transform.position.z),
                                ref velocity, smoothSpeed
                            );

                            // Set the camera's position to the new smoothed position
                            transform.position = smoothedPosition;
                        }
                        else if(targetRigidbody.transform.position.x >= -50f && targetRigidbody.transform.position.x <= 100f && Mathf.Round(targetRigidbody.transform.position.y) == -45f)
                        {
                            Debug.Log("On X else case");

                            // Calculate the desired position based on the target's Rigidbody position + offset
                            Vector3 desiredPosition = targetRigidbody.position + offset;

                            // Smoothly interpolate between the camera's current position and the desired X position
                            // Keep the Y and Z positions the same, only update the X position
                            Vector3 smoothedPosition = Vector3.SmoothDamp(
                                new Vector3(transform.position.x, transform.position.y, transform.position.z),
                                new Vector3(desiredPosition.x, transform.position.y, transform.position.z),
                                ref velocity, smoothSpeed
                            );

                            // Set the camera's position to the new smoothed position
                            transform.position = smoothedPosition;

                        }
                    }

                }
                else if (!_playerShark.InitialMovement)
                {
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
}
