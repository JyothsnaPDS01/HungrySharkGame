using UnityEngine;
using SharkGame.Models;

namespace SharkGame
{
    public class ParallaxObject : MonoBehaviour
    {
        public float parallaxMultiplier = 0.5f;  // Speed at which the object moves (relative to the player's movement)

        public Player _player;

        [Header("Left Value")]
        [SerializeField] private float leftValue;
        [SerializeField] private float rightValue;

        [Header("isLeft")]
        [SerializeField] private bool isLeft;

        private void OnEnable()
        {
            _player.OnSharkMove += HandleSharkMovement;
        }

        private void OnDisable()
        {
            _player.OnSharkMove -= HandleSharkMovement;
        }

        private Vector3 lastSharkPosition;  // Store the last position of the shark
        private Vector3 targetPosition;      // Target position for the parallax object

        private void Start()
        {
            lastSharkPosition = Vector3.zero;  // Initialize to zero or the starting position
            targetPosition = transform.position; // Initialize target position
        }

        private void HandleSharkMovement(Vector3 sharkPosition, bool isRight)
        {
            // Calculate the delta movement based on the current and last shark position
            Vector3 deltaMovement = sharkPosition - lastSharkPosition;

            // Only apply movement if the shark has actually moved
            if (deltaMovement.magnitude > 0)
            {
                // Calculate the new target position for the parallax object
                // Subtract 11 from the shark's X position to maintain a distance of -11
                if (!isRight)
                {
                    targetPosition = new Vector3(sharkPosition.x + leftValue, transform.position.y, transform.position.z);
                }
                else if(isRight)
                {
                    targetPosition = new Vector3(sharkPosition.x + rightValue, transform.position.y, transform.position.z);
                }

                // Optionally clamp the target position to maintain a certain distance
                // For example, limit the X position
                float minX = -85f; // Minimum X position
                float maxX = 86f;  // Maximum X position
                targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            }

            // Adjust parallax movement when shark moves left of a certain position
            if (sharkPosition.x <= -52f)
            {
                // This could be another adjustment if needed, or you can remove this block if unnecessary
                if(isLeft) targetPosition = new Vector3(-65f, transform.position.y, transform.position.z);
            }
            else if(sharkPosition.x>=85f)
            {
                if (!isLeft) targetPosition = new Vector3(95f, transform.position.y, transform.position.z);
            }

            // Move the parallax object towards the target position smoothly
            transform.position = Vector3.Lerp(transform.position, targetPosition, parallaxMultiplier / 2);

            // Update the last position of the shark for the next movement check
            lastSharkPosition = sharkPosition;
        }


    }
}
