using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharkGame.Models;


namespace SharkGame
{
    public class SmallFish : MonoBehaviour
    {
        private List<Transform> waypoints;
        private int currentWaypointIndex = 0;
        private float moveSpeed = 1f;
        [SerializeField] private Animator _fishAnimator;

        [SerializeField] public SharkGameDataModel.SmallFishType _smallFishType;

        private void Start()
        {
            transform.rotation = Quaternion.Euler(0, 90, 0);
        }

        public void SetWaypoints(List<Transform> waypointList)
        {
            waypoints = waypointList;
            // Start moving through waypoints once assigned
            StartCoroutine(MoveThroughWaypoints());
        }

        private IEnumerator MoveThroughWaypoints()
        {
           
                while (true)
                {
                    if (waypoints.Count == 0)
                    {
                        yield break; // No waypoints available
                    }

                    _fishAnimator.SetFloat("moveAmount", .5f);
                    // Move the small fish towards the current waypoint
                    Transform targetWaypoint = waypoints[currentWaypointIndex];

                    // Rotate the fish towards the next waypoint
                    Vector3 directionToWaypoint = (targetWaypoint.position - transform.position).normalized;
                    Quaternion targetRotation = Quaternion.LookRotation(directionToWaypoint);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);  // Adjust the rotation speed as necessary

                    while (Vector3.Distance(transform.position, targetWaypoint.position) > 0.1f)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, moveSpeed * Time.deltaTime);
                        yield return null; // Wait for the next frame
                    }

                    // Move to the next waypoint, looping if needed
                    currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
                    yield return null; // Wait before moving to the next waypoint
                }
            
        }

    }
}
