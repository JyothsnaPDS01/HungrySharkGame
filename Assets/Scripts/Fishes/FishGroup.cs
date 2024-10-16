﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharkGame.Models;

namespace SharkGame
{
    public class FishGroup : MonoBehaviour
    {
        private List<Transform> waypoints;

        private int currentWaypointIndex = 0;

        [SerializeField] private List<SmallFish> smallFishes;

        [SerializeField] private float moveSpeed = 1f;

        [SerializeField] private SharkGameDataModel.SmallFishType _smallFishType;

        private bool isMovingRight = true; // Used for horizontal movement logic
        private bool isMovingUp = true;
        private float minY =-45f;
        private float maxY = -20f;
        private float minX = -50f; // Left boundary
        private float maxX = 65f;  // Right boundary

        [SerializeField] private int capacity;
        [SerializeField] private int destroyCount = 0;

        [SerializeField] private bool isSineWave;

        public int FishGroupDestroyCount
        {
            get { return destroyCount; }
            set { destroyCount = value; }
        }

        public int FishGroupCapacity
        {
            get { return capacity; }
        }

        public SharkGameDataModel.SmallFishType FishGroupType
        {
            get { return _smallFishType; }
        }

        public void SetWaypoints(List<Transform> waypointList)
        {
            waypoints = waypointList;
            // Start movement logic based on the level
            if (SharkGameManager.Instance.CurrentLevel == 1 || SharkGameManager.Instance.CurrentLevel == 12 || SharkGameManager.Instance.CurrentLevel == 16)
            {
                StartCoroutine(MoveHorizontally());
            }
            else if (SharkGameManager.Instance.CurrentLevel == 2 || SharkGameManager.Instance.CurrentLevel == 11)
            {
                StartCoroutine(MoveInSineWave());
            }
            else if (SharkGameManager.Instance.CurrentLevel == 3 || SharkGameManager.Instance.CurrentLevel == 6 || SharkGameManager.Instance.CurrentLevel == 17)
            {
                StartCoroutine(MoveInCurvedSineWave());
            }
            else if (SharkGameManager.Instance.CurrentLevel == 4 || SharkGameManager.Instance.CurrentLevel == 18 )
            {
                StartCoroutine(MoveInCircularPath());
            }
            else if(SharkGameManager.Instance.CurrentLevel == 5 || SharkGameManager.Instance.CurrentLevel == 20)
            {
                StartCoroutine(EscapeFromPlayer(GameObject.Find("Player_Shark").transform));
            }
            else if(SharkGameManager.Instance.CurrentLevel == 7 || SharkGameManager.Instance.CurrentLevel == 13)
            {
                if(this.isSineWave)
                {
                    StartCoroutine(MoveInSineWave());
                }
                else
                {
                    StartCoroutine(MoveHorizontally());
                }
            }
            else if(SharkGameManager.Instance.CurrentLevel == 8 || SharkGameManager.Instance.CurrentLevel == 19)
            {
                StartCoroutine(MoveZigZag());
            }
            else if(SharkGameManager.Instance.CurrentLevel == 9 || SharkGameManager.Instance.CurrentLevel == 14)
            {
                StartCoroutine(MoveFastAndSlow());
            }
            else if(SharkGameManager.Instance.CurrentLevel == 10 || SharkGameManager.Instance.CurrentLevel == 15)
            {
                StartCoroutine(ReverseEscape(GameObject.Find("Player_Shark").transform));
            }
            else
            {
                StartCoroutine(MoveThroughWaypoints(this));
            }
        }

        // Horizontal movement for level 1
        private IEnumerator MoveHorizontally()
        {
#if UNITY_EDITOR
            Debug.LogError("MoveHorizontally");
#endif
            while (true)
            {
                // Ensure the fish swim animation plays for each small fish
                foreach (var item in smallFishes)
                {
                    item.PlaySwimAnimation();
                }

                // Determine the target X position (either maxX or minX based on direction)
                float targetX = isMovingRight ? maxX : minX;

                // Apply the appropriate rotation directly based on the direction
                transform.rotation = isMovingRight ? Quaternion.Euler(0, 90f, 0f) : Quaternion.Euler(0, -90f, 0f);

                // Calculate new position for the parent object
                Vector3 targetPosition = new Vector3(targetX, transform.position.y, transform.position.z);

                // Move the parent object (and therefore the fish group) towards the target position
                while (Mathf.Abs(transform.position.x - targetX) > 0.1f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                    yield return null;
                }

                // Switch direction after reaching the target position
                isMovingRight = !isMovingRight;

                yield return null; // Wait before reversing
            }
        }

        private IEnumerator MoveInSineWave()
        {
#if UNITY_EDITOR
            Debug.LogError("MoveInSineWave");
#endif
            float horizontalSpeed = moveSpeed;        // Speed for horizontal movement
            float verticalAmplitude = 1f;             // Amplitude for the vertical movement
            float verticalFrequency = 1f;             // Frequency for the vertical sine wave movement
            float time = 0f;                          // Track time to simulate wave motion

            // Capture the initial Y position to keep the sine wave motion centered around it
            float initialY = transform.position.y;

            while (true)
            {
                // Ensure the fish swim animation plays for each small fish
                foreach (var item in smallFishes)
                {
                    item.PlaySwimAnimation();
                }

                // Determine the target X position (either maxX or minX based on direction)
                float targetX = isMovingRight ? maxX : minX;

                // Apply the appropriate rotation based on the direction
                transform.rotation = isMovingRight ? Quaternion.Euler(0, 90f, 0f) : Quaternion.Euler(0, -90f, 0f);

                // Start moving the fish group horizontally while applying sine wave for vertical movement
                while (Mathf.Abs(transform.position.x - targetX) > 0.1f)
                {
                    // Calculate the new X and Y positions
                    float newX = Mathf.MoveTowards(transform.position.x, targetX, horizontalSpeed * Time.deltaTime);
                    float newY = Mathf.Sin(time * verticalFrequency) * verticalAmplitude + initialY; // Center around initial Y

                    // Update the position with both horizontal and sine wave vertical motion
                    transform.position = new Vector3(newX, newY, transform.position.z);

                    time += Time.deltaTime; // Update time to simulate continuous motion

                    yield return null;
                }

                // Switch direction after reaching the target position
                isMovingRight = !isMovingRight;

                yield return null; // Small delay before switching direction
            }
        }

        private IEnumerator MoveInCurvedSineWave()
        {
#if UNITY_EDITOR
            Debug.LogError("MoveInCurvedSineWave");
#endif
            float horizontalSpeed = moveSpeed;   // Increased speed for faster horizontal movement
            float verticalAmplitude = 0.5f;             // Smaller amplitude for a smoother curve
            float verticalFrequency = 2f;               // Higher frequency for more curves in the sine wave
            float time = 0f;                            // Track time to simulate wave motion

            // Capture the initial Y position to keep the sine wave motion centered around it
            float initialY = transform.position.y;

            while (true)
            {
                // Ensure the fish swim animation plays for each small fish
                foreach (var item in smallFishes)
                {
                    item.PlaySwimAnimation();
                }

                // Determine the target X position (either maxX or minX based on direction)
                float targetX = isMovingRight ? maxX : minX;

                // Apply the appropriate rotation based on the direction
                transform.rotation = isMovingRight ? Quaternion.Euler(0, 90f, 0f) : Quaternion.Euler(0, -90f, 0f);

                // Start moving the fish group horizontally while applying a smoother sine wave for vertical movement
                while (Mathf.Abs(transform.position.x - targetX) > 0.1f)
                {
                    // Calculate the new X and Y positions
                    float newX = Mathf.MoveTowards(transform.position.x, targetX, horizontalSpeed * Time.deltaTime);
                    float newY = Mathf.Sin(time * verticalFrequency) * verticalAmplitude + initialY; // Smoother, gentler curve

                    // Update the position with both horizontal and sine wave vertical motion
                    transform.position = new Vector3(newX, newY, transform.position.z);

                    time += Time.deltaTime * 2; // Increase time faster to simulate faster sine wave motion

                    yield return null;
                }

                // Switch direction after reaching the target position
                isMovingRight = !isMovingRight;

                yield return null; // Small delay before switching direction
            }
        }

        private IEnumerator MoveInCircularPath()
        {
#if UNITY_EDITOR
            Debug.LogError("MoveInCircularPath");
#endif
            float horizontalSpeed = moveSpeed;     // Increased speed for faster circular movement
            float radius = 1f;                     // Radius of the circular path
            float angle = 0f;                      // Track the angle for circular movement
            Vector3 centerPosition = transform.position; // Central point of the circular movement

            while (true)
            {
                // Ensure the fish swim animation plays for each small fish
                foreach (var item in smallFishes)
                {
                    item.PlaySwimAnimation();
                }

                // Calculate the next position based on the circular movement formula
                float newX = centerPosition.x + Mathf.Cos(angle) * radius;
                float newY = centerPosition.y + Mathf.Sin(angle) * radius;
                Vector3 newPosition = new Vector3(newX, newY, transform.position.z);

                // Calculate the direction from the current position to the next position
                Vector3 direction = newPosition - transform.position;

                // Apply rotation to face the direction of movement
                if (direction != Vector3.zero)
                {
                    float angleToTarget = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0f, angleToTarget, 0f);
                }

                // Update the position to simulate circular movement
                transform.position = newPosition;

                // Increase the angle over time to continue the circular motion
                angle += Time.deltaTime * horizontalSpeed;

                // Reset the angle to avoid overflow
                if (angle >= 360f)
                {
                    angle = 0f;
                }

                yield return null;
            }
        }


        private IEnumerator EscapeFromPlayer(Transform playerTransform)
        {
#if UNITY_EDITOR
            Debug.LogError("EscapeFromPlayer");
#endif
            float escapeDistance = 1f;      // The distance the fish group should maintain from the player
            float escapeSpeed = moveSpeed;  // Speed of escape (faster than normal movement)

            while (true)
            {
                // Ensure the fish swim animation plays for each small fish
                foreach (var item in smallFishes)
                {
                    item.PlaySwimAnimation();
                }

                // Calculate the distance between the player and the fish group
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

                // If the player is too close, trigger the escape behavior
                if (distanceToPlayer < escapeDistance)
                {
                    // Calculate the escape direction (away from the player)
                    Vector3 escapeDirection = (transform.position - playerTransform.position).normalized;

                    // Move the fish group away from the player in the calculated escape direction
                    Vector3 escapeTarget = transform.position + escapeDirection * escapeSpeed * Time.deltaTime;

                    // Smoothly move towards the escape target
                    transform.position = Vector3.MoveTowards(transform.position, escapeTarget, escapeSpeed * Time.deltaTime);
                }
                else
                {
                    // If the player is not too close, return to normal behavior (e.g., circular movement)
                    yield return StartCoroutine(MoveInCircularPath());
                }

                yield return null;
            }
        }

        private IEnumerator MoveZigZag()
        {
#if UNITY_EDITOR
            Debug.LogError("MoveZigZag");
#endif
            while (true)
            {
                // Ensure the fish swim animation plays for each small fish
                foreach (var item in smallFishes)
                {
                    item.PlaySwimAnimation();
                }

                // Determine the target X position (either maxX or minX based on direction)
                float targetX = isMovingRight ? maxX : minX;

                // Set the zigzag pattern by alternating Y positions between two values (e.g., maxY and minY)
                float targetY = isMovingUp ? maxY : minY;

                // Apply the appropriate rotation directly based on the direction (horizontal movement)
                transform.rotation = isMovingRight ? Quaternion.Euler(0, 90f, 0f) : Quaternion.Euler(0, -90f, 0f);

                // Calculate the new target position including the zigzag pattern
                Vector3 targetPosition = new Vector3(targetX, targetY, transform.position.z);

                // Move the parent object (and therefore the fish group) towards the target position
                while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                    yield return null;
                }

                // Switch direction after reaching the target position
                isMovingRight = !isMovingRight; // Horizontal direction switch
                isMovingUp = !isMovingUp; // Vertical direction switch for zigzag

                yield return null; // Wait before reversing
            }
        }

        public float fastSpeed = 1.5f;
        public float slowSpeed = .5f;

        private IEnumerator MoveFastAndSlow()
        {
#if UNITY_EDITOR
            Debug.LogError("MoveFastAndSlow");
#endif
            while (true)
            {
                // Ensure the fish swim animation plays for each small fish
                foreach (var item in smallFishes)
                {
                    item.PlaySwimAnimation();
                }

                // Determine the target X position (either maxX or minX based on direction)
                float targetX = isMovingRight ? maxX : minX;

                // Apply the appropriate rotation directly based on the direction (right or left)
                transform.rotation = isMovingRight ? Quaternion.Euler(0, 90f, 0f) : Quaternion.Euler(0, -90f, 0f);

                // Calculate new position for the parent object
                Vector3 targetPosition = new Vector3(targetX, transform.position.y, transform.position.z);

                // Randomly determine if the movement will be fast or slow for this segment
                bool isFast = Random.value > 0.5f;  // 50% chance of fast or slow movement
                float currentMoveSpeed = isFast ? fastSpeed : slowSpeed;

                // Move the parent object (and therefore the fish group) towards the target position
                while (Mathf.Abs(transform.position.x - targetX) > 0.1f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentMoveSpeed * Time.deltaTime);
                    yield return null;
                }

                // Switch direction after reaching the target position
                isMovingRight = !isMovingRight;

                // Add a small delay between fast and slow movements to make it more dynamic
                yield return new WaitForSeconds(isFast ? 0.5f : 1f);  // Shorter delay after fast movement, longer after slow
            }
        }

        public Transform sharkTransform;  // Assign the player's shark transform here
        public float approachSpeed = 3f;  // Speed at which fish approach the shark
        public float escapeSpeed = 1.5f;    // Speed at which fish escape the shark
        public float escapeThreshold = 5f; // Distance at which fish escape
        public float escapeDistance = 10f; // Distance fish travel while escaping

        private IEnumerator ReverseEscape(Transform sharkTransform)
        {
#if UNITY_EDITOR
            Debug.LogError("ReverseEscape");
#endif
            while (true)
            {
                // Ensure the fish swim animation plays for each small fish
                foreach (var item in smallFishes)
                {
                    item.PlaySwimAnimation();
                }

                // Calculate the distance between the fish group and the shark
                float distanceToShark = Vector3.Distance(transform.position, sharkTransform.position);

                // If the fish are moving towards the shark
                if (distanceToShark > escapeThreshold)
                {
                    // Move towards the shark (bait movement)
                    Vector3 targetPosition = sharkTransform.position;

                    // Apply the appropriate rotation to face the shark
                    transform.rotation = (targetPosition.x > transform.position.x) ? Quaternion.Euler(0, 90f, 0f) : Quaternion.Euler(0, -90f, 0f);

                    // Move towards the shark
                    while (Vector3.Distance(transform.position, sharkTransform.position) > escapeThreshold)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, sharkTransform.position, approachSpeed * Time.deltaTime);
                        yield return null;
                    }

                    // Once the fish are close enough to the shark, escape in the opposite direction
                    Vector3 escapeDirection = (transform.position - sharkTransform.position).normalized;
                    Vector3 escapeTarget = transform.position + escapeDirection * escapeDistance;

                    // Rotate the fish to face away from the shark
                    transform.rotation = (escapeDirection.x > 0) ? Quaternion.Euler(0, 90f, 0f) : Quaternion.Euler(0, -90f, 0f);

                    // Escape in the opposite direction
                    while (Vector3.Distance(transform.position, escapeTarget) > 0.1f)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, escapeTarget, escapeSpeed * Time.deltaTime);
                        yield return null;
                    }
                }

                // Wait before repeating the escape behavior
                yield return null;
            }
        }



        // Move along waypoints for other levels
        private IEnumerator MoveThroughWaypoints(FishGroup fish)
        {
            if (waypoints.Count < 4)
            {
                Debug.LogError("Need at least 4 waypoints for smooth Catmull-Rom movement.");
                yield break;
            }

            while (true)
            {
                Transform p0 = waypoints[(currentWaypointIndex - 1 + waypoints.Count) % waypoints.Count];
                Transform p1 = waypoints[currentWaypointIndex];
                Transform p2 = waypoints[(currentWaypointIndex + 1) % waypoints.Count];
                Transform p3 = waypoints[(currentWaypointIndex + 2) % waypoints.Count];

                float t = 0f;
                float curveSpeed = 1f / Vector3.Distance(p1.position, p2.position); // Adjust speed based on distance

                while (t <= 1f)
                {
                    Vector3 newPosition = CatmullRom(p0.position, p1.position, p2.position, p3.position, t);

                    // Smoothly rotate towards the new position
                    Vector3 directionToWaypoint = (newPosition - fish.transform.position).normalized;
                    if (directionToWaypoint != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(directionToWaypoint);
                        fish.transform.rotation = Quaternion.Slerp(fish.transform.rotation, targetRotation, Time.deltaTime * 5f);
                    }

                    // Move fish to new position
                    fish.transform.position = newPosition;

                    // Increment t based on speed
                    t += moveSpeed * Time.deltaTime * curveSpeed;

                    yield return null;
                }

                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;

                yield return null;
            }
        }


        // Catmull-Rom spline for smooth curves between waypoints
        public static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            Vector3 a = 2f * p1;
            Vector3 b = p2 - p0;
            Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
            Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

            return 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));
        }

        public void UpdateDestroyCount(int _destroyCount, GameObject _smallFishObject)
        {
            // Deactivate the small fish object first
            _smallFishObject.SetActive(false);

            // Increment the destroy count for this fish group
            destroyCount += _destroyCount;

            // Check if the destroy count reaches the capacity of the group
            if (destroyCount >= capacity)
            {
#if UNITY_EDITOR
                // Return the entire fish group to the pool
                Debug.LogError("Pool Object after group size meet" + this.gameObject.name);
#endif
                SoundManager.Instance.PlayAudioClip(SharkGameDataModel.Sound.SharkSound);
                ObjectPooling.Instance.ReturnToPool(this.gameObject, _smallFishType);

                // Reset the destroy count
                destroyCount = 0;
            }
        }

    }
}
