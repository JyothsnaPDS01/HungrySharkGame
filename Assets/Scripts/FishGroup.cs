using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SharkGame
{
    public class FishGroup : MonoBehaviour
    {
        private List<Transform> waypoints;

        private int currentWaypointIndex = 0;

        [SerializeField] private List<SmallFish> smallFishes;

        [SerializeField] private float moveSpeed = 1f;

        public void SetWaypoints(List<Transform> waypointList)
        {
            waypoints = waypointList;
            // Start moving through waypoints once assigned
            StartCoroutine(MoveThroughWaypoints());
        }

        private IEnumerator MoveThroughWaypoints()
        {
            float minX = -100f; // Left boundary
            float maxX = 150f;  // Right boundary

            while (true)
            {
                if (waypoints.Count == 0)
                {
                    yield break; // No waypoints available
                }

                //_fishAnimator.SetFloat("moveAmount", .5f);

                foreach (var item in smallFishes)
                {
                    item.PlaySwimAnimation();
                }

                // Move the small fish towards the current waypoint
                Transform targetWaypoint = waypoints[currentWaypointIndex];

                // Rotate the fish towards the next waypoint
                Vector3 directionToWaypoint = (targetWaypoint.position - transform.position).normalized;
                if (directionToWaypoint != Vector3.zero) // Check to avoid zero vector issues
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToWaypoint);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // Smooth rotation
                }

                while (Vector3.Distance(transform.position, targetWaypoint.position) > 0.1f)
                {
                    // Move towards the target waypoint
                    transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, moveSpeed * Time.deltaTime);
                    yield return null; // Wait for the next frame

                    // Update the direction and rotation during movement
                    directionToWaypoint = (targetWaypoint.position - transform.position).normalized;
                    if (directionToWaypoint != Vector3.zero) // Check to avoid zero vector issues
                    {
                       
                            Quaternion targetRotation = Quaternion.LookRotation(directionToWaypoint);
                           transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // Smooth rotation
                        
                        
                    }
                }

                // Move to the next waypoint, looping if needed
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
                yield return null; // Wait before moving to the next waypoint
            }
        }
    }
}
