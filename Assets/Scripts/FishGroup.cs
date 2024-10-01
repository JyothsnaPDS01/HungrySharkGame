using System.Collections;
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
        private float minX = -50f; // Left boundary
        private float maxX = 80f;  // Right boundary

        [SerializeField] private int capacity;
        [SerializeField] private int destroyCount = 0;

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
            if (SharkGameManager.Instance.CurrentLevel == 1 || SharkGameManager.Instance.CurrentLevel == 2)
            {
                StartCoroutine(MoveHorizontally());
            }
            else
            {
                StartCoroutine(MoveThroughWaypoints());
            }
        }

        // Horizontal movement for level 1
        private IEnumerator MoveHorizontally()
        {
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

        // Move along waypoints for other levels
        private IEnumerator MoveThroughWaypoints()
        {
            while (true)
            {
                if (waypoints.Count < 4)
                {
                    Debug.LogError("Need at least 4 waypoints for smooth Catmull-Rom movement.");
                    yield break; // Not enough waypoints for Catmull-Rom spline
                }

                foreach (var item in smallFishes)
                {
                    item.PlaySwimAnimation();
                }

                // Loop through the waypoints
                while (true)
                {
                    Transform p0 = waypoints[(currentWaypointIndex - 1 + waypoints.Count) % waypoints.Count];
                    Transform p1 = waypoints[currentWaypointIndex];
                    Transform p2 = waypoints[(currentWaypointIndex + 1) % waypoints.Count];
                    Transform p3 = waypoints[(currentWaypointIndex + 2) % waypoints.Count];

                    float t = 0f;
                    float curveSpeed = 1f / Vector3.Distance(p1.position, p2.position); // Adjust speed based on distance between waypoints

                    while (t <= 1f)
                    {
                        // Get the next position on the Catmull-Rom spline
                        Vector3 newPosition = CatmullRom(p0.position, p1.position, p2.position, p3.position, t);

                        // Smoothly rotate towards the new position
                        Vector3 directionToWaypoint = (newPosition - transform.position).normalized;
                        if (directionToWaypoint != Vector3.zero)
                        {
                            Quaternion targetRotation = Quaternion.LookRotation(directionToWaypoint);
                            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                        }

                        // Move the fish to the new position
                        transform.position = newPosition;

                        // Increment t based on speed
                        t += moveSpeed * Time.deltaTime * curveSpeed;

                        yield return null; // Wait for the next frame
                    }

                    // Move to the next waypoint, looping if needed
                    currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;

                    yield return null; // Wait before moving to the next waypoint
                }
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
                // Return the entire fish group to the pool
                Debug.LogError("Pool Object after group size meet" + this.gameObject.name);
                ObjectPooling.Instance.ReturnToPool(this.gameObject, _smallFishType);

                // Reset the destroy count
                destroyCount = 0;
            }
        }

    }
}
