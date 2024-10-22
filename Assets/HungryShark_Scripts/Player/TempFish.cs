using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SharkGame
{
    public class TempFish : MonoBehaviour
    {
        public Vector3 target;  // The target position (can be set via inspector)
        public float moveSpeed = 5f;  // Speed of movement

        private Rigidbody rb;

        public Vector3 _initialPositon;

        void Start()
        {
            // Get the Rigidbody component
            rb = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            // Move towards the target position using Lerp
            if (gameObject.activeInHierarchy)
            {
                MoveTowardsTarget();
            }
        }

        void MoveTowardsTarget()
        {
            // Current position of the shark
            Vector3 currentPosition = rb.position;

            // Target position to move the shark towards
            Vector3 targetPosition = target;

            // Lerp towards the target position
            Vector3 newPosition = Vector3.Lerp(currentPosition, targetPosition, moveSpeed * Time.fixedDeltaTime);

            // Apply the new position to the Rigidbody
            rb.MovePosition(newPosition);
        }

        public void ResetPositon()
        {
            rb.position = _initialPositon;
        }
    }
}
