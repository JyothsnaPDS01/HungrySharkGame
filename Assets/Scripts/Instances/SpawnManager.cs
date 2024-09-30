using SharkGame.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SharkGame
{
    public class SpawnManager : MonoBehaviour
    {
        [Header("Waypoints and Fish")]
        [SerializeField] public List<Transform> waypoints;

        [Header("Player Shark")]
        [SerializeField] private Transform _playerShark;

        [Header("Spawn Point")]
        [SerializeField] private Transform _spawnPoint;

        [Header("Fish Settings")]
        [SerializeField] private float minSpawnDistanceBetweenFishes = .5f;
        [SerializeField] private float minDistanceFromShark = 2f;

        [SerializeField] private List<GameObject> activeFishes = new List<GameObject>(); // Active fish tracking

        public static SpawnManager _instance;
        public static SpawnManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SpawnManager>();
                    if (_instance == null)
                    {
#if UNITY_EDITOR
                        Debug.LogError("There is no SpawnManager in the scene!");
#endif
                    }
                }
                return _instance;
            }
        }

        #region Events
        internal void HandleGameMode(SharkGameDataModel.GameMode currentGameMode)
        {
            if (currentGameMode == SharkGameDataModel.GameMode.GameStart)
            {
                Debug.Log("CallSpawnFishesFrequently");
                StartCoroutine(CallSpawnFishesFrequently());
            }
        }
        #endregion
        private IEnumerator CallSpawnFishesFrequently()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                SpawnFishesAtWaypoints();
            }
        }

        public void SpawnFishesAtWaypoints()
        {
            foreach (var waypoint in waypoints)
            {
               // if (Vector3.Distance(_playerShark.position, waypoint.position) > minDistanceFromShark)
                {
                    List<Vector3> fishOffsets = new List<Vector3>(); // To hold the offsets
                    List<GameObject> fishesToMove = new List<GameObject>(); // Keep track of spawned fishes

                    //int fishCount = Random.Range(4, 6); // Randomly choose between 4 and 5 fishes
                    int fishCount = 1;
                    for (int i = 0; i < fishCount; i++)
                    {
                        Vector3 spawnPosition = waypoint.position;

                        if (!IsWaypointOccupied(spawnPosition))
                        {
                            GameObject fish = ObjectPooling.Instance.SpawnFromPool(GetRandomSmallFishType(), spawnPosition, Quaternion.identity);

                            if (fish == null)
                            {
#if UNITY_EDITOR
                                Debug.LogError("Fish is null case");
#endif
                                continue; // Skip further processing if the fish is null
                            }

                            FishGroup smallFish = fish.GetComponent<FishGroup>();
                            if (smallFish != null)
                            {
                                smallFish.SetWaypoints(waypoints); // Assign waypoints to the fish
                                fishesToMove.Add(fish); // Track the spawned fish
                                fishOffsets.Add(spawnPosition - waypoint.position); // Store the relative position
                            }
                        }
                    }

                    //// Check if fishesToMove and fishOffsets are the same length before starting movement
                    //if (fishesToMove.Count > 0 && fishesToMove.Count == fishOffsets.Count)
                    //{
                    //    activeFishes.AddRange(fishesToMove); // Add the new fishes to the active list
                    //    if (SharkGameManager.Instance.CurrentLevel == 2 || SharkGameManager.Instance.CurrentLevel == 1 || SharkGameManager.Instance.CurrentLevel == 3 || SharkGameManager.Instance.CurrentLevel == 5)
                    //    {
                    //        StartCoroutine(MoveFishGroup(fishesToMove, fishOffsets, waypoint.position));
                    //    }
                    //    else if (SharkGameManager.Instance.CurrentLevel == 4)
                    //    {
                    //        StartCoroutine(EscapeFishGroup(fishesToMove, fishOffsets, waypoint.position, _playerShark));
                    //    }
                    //}
                }
            }
        }

        //private IEnumerator MoveFishGroup(List<GameObject> fishesToMove, List<Vector3> fishOffsets, Vector3 groupCenter)
        //{
        //    float speed = .5f; // Adjust speed as needed
        //    Vector3 direction = Vector3.right; // Set the movement direction

        //        while (true)
        //        {
        //            // Move each fish while maintaining their relative offsets
        //            for (int i = 0; i < fishesToMove.Count; i++)
        //            {
        //                GameObject fish = fishesToMove[i];
        //                if (fish != null)
        //                {
        //                    // Update position
        //                    Vector3 targetPosition = groupCenter + fishOffsets[i] + (direction * speed * Time.deltaTime);
        //                    fish.transform.position = targetPosition;

        //                    if (targetPosition.x >= 100f || targetPosition.x <= -55f)
        //                    {
        //                        // Reverse direction
        //                        direction = -direction;
        //                        fish.transform.rotation = Quaternion.Euler(fish.transform.rotation.x, -fish.transform.rotation.y, fish.transform.rotation.z);
        //                    }
        //                }
        //            }

        //            // Update the group center to maintain their movement
        //            groupCenter += direction * speed * Time.deltaTime;

        //            yield return null; // Wait for the next frame
        //        }
        //}


        private IEnumerator MoveFishGroup(List<GameObject> fishesToMove, List<Vector3> fishOffsets, Vector3 waypointPosition)
        {
            float minX = -100f; // Left boundary
            float maxX = 150f;  // Right boundary

            while (true) // Keep moving fishes indefinitely
            {
                foreach (var fish in fishesToMove)
                {
                    if (fish == null) continue;

                    SmallFish smallFishComponent = fish.GetComponent<SmallFish>();
                    if (smallFishComponent == null)
                    {
                        Debug.LogError($"Fish {fish.name} does not have a SmallFish component.");
                        continue; // Skip this fish if it doesn't have the component
                    }

                    float moveSpeed = smallFishComponent.SmallFishSpeed; // Get the speed from the fish script

                    // Check whether the fish is moving right or left
                    bool movingRight = fish.transform.position.x < maxX;

                    smallFishComponent.isMovingRight = movingRight;

                    Vector3 targetPosition = fish.transform.position;

                    // If the fish is moving right and has not reached maxX, keep moving to the right
                    if (movingRight)
                    {
                        targetPosition.x = maxX;
                        // Rotate the fish to face the right direction (if not already facing right)
                        fish.transform.rotation = Quaternion.Euler(0, -90, 0); // Face right
                    }
                    else
                    {
                        Debug.Log("Moving Left");
                        targetPosition.x = minX;
                        // Rotate the fish to face the left direction (if not already facing left)
                        fish.transform.rotation = Quaternion.Euler(0, 90, 0); // Face left
                    }

                    // Move the fish towards the target position, no idling
                    fish.transform.position = Vector3.MoveTowards(fish.transform.position, targetPosition, moveSpeed * Time.deltaTime);

                    // If the fish reaches the target position, switch direction
                    if (Mathf.Abs(fish.transform.position.x - targetPosition.x) <= 0.1f)
                    {
                        // If moving right, switch to moving left, and vice versa

                        Debug.Log("Moving right" + movingRight);
                        movingRight = !movingRight;

                        smallFishComponent.isMovingRight = movingRight;

                        Debug.Log("Moving right 1" + movingRight);

                        // Update target position based on new direction
                        targetPosition.x = movingRight ? maxX : minX;

                        // Ensure the fish rotates correctly for the new direction
                        fish.transform.rotation = movingRight ? Quaternion.Euler(0, -90, 0) : Quaternion.Euler(0, 90, 0);

                    }

                    yield return null; // Continue every frame
                }

                // No wait time, continuous movement
            }
        }



        private IEnumerator EscapeFishGroup(List<GameObject> fishesToMove, List<Vector3> fishOffsets, Vector3 waypointPosition, Transform playerTransform)
        {
            float minX = -100f; // Left boundary
            float maxX = 150f;  // Right boundary
            float escapeDistance = 20f; // Distance at which fish will escape from the player
            float curveIntensity = 2f;  // Intensity of the curved path while escaping
            float normalWaitTime = 1.0f; // Normal wait time between movements
            float escapeSpeedMultiplier = 4f; // Speed multiplier when escaping

            while (true) // Keep moving fishes indefinitely
            {
                foreach (var fish in fishesToMove)
                {
                    if (fish == null) continue;

                    SmallFish smallFishComponent = fish.GetComponent<SmallFish>();
                    if (smallFishComponent == null)
                    {
                        Debug.LogError($"Fish {fish.name} does not have a SmallFish component.");
                        continue; // Skip this fish if it doesn't have the component
                    }

                    float moveSpeed = smallFishComponent.SmallFishSpeed; // Get normal speed from the fish script
                    Vector3 targetPosition = fish.transform.position;
                    bool movingRight = fish.transform.position.x < maxX;

                    // Check if player is near the fish
                    bool isPlayerNear = Vector3.Distance(fish.transform.position, playerTransform.position) <= escapeDistance;

                    // If player is near, move faster and in a curved path
                    if (isPlayerNear)
                    {
                        moveSpeed *= escapeSpeedMultiplier; // Move faster
                        targetPosition = GetEscapeCurvePosition(fish.transform.position, playerTransform.position, curveIntensity); // Curve escape path

                        // Move the fish to the curved escape position
                        while (Vector3.Distance(fish.transform.position, targetPosition) > 0.1f)
                        {
                            fish.transform.position = Vector3.MoveTowards(fish.transform.position, targetPosition, moveSpeed * Time.deltaTime);
                            yield return null;
                        }
                    }
                    else
                    {
                        // Regular waypoint movement
                        if (movingRight)
                        {
                            targetPosition.x = maxX;
                            fish.transform.rotation = Quaternion.Euler(0, -90, 0); // Face right
                        }
                        else
                        {
                            targetPosition.x = minX;
                            fish.transform.rotation = Quaternion.Euler(0, 90, 0); // Face left
                        }

                        // Move the fish towards the target position
                        while (Vector3.Distance(fish.transform.position, targetPosition) > 0.1f)
                        {
                            fish.transform.position = Vector3.MoveTowards(fish.transform.position, targetPosition, moveSpeed * Time.deltaTime);
                            yield return null;
                        }

                        // Toggle movement direction after reaching the edge
                        movingRight = !movingRight;
                    }
                }

                // Wait briefly before repeating the loop
                yield return new WaitForSeconds(normalWaitTime);
            }
        }

        // Helper method to create a curved escape path
        private Vector3 GetEscapeCurvePosition(Vector3 fishPosition, Vector3 playerPosition, float curveIntensity)
        {
            // Calculate the direction away from the player
            Vector3 escapeDirection = (fishPosition - playerPosition).normalized;

            // Apply a curve to the escape path (add some vertical offset)
            Vector3 curveOffset = new Vector3(0, Mathf.Sin(Time.time * curveIntensity) * 0.5f, 0);

            // Return the new target position for the fish to escape to
            return fishPosition + (escapeDirection * curveIntensity) + curveOffset;
        }



        private IEnumerator MoveLevelThreeFishGroup(List<GameObject> fishesToMove, List<Vector3> fishOffsets, Vector3 groupCenter)
        {
            // Dictionary to store the current waypoint index for each fish
            Dictionary<GameObject, int> fishWaypointIndices = new Dictionary<GameObject, int>();
            Dictionary<GameObject, float> fishTimers = new Dictionary<GameObject, float>();

            foreach (var fish in fishesToMove)
            {
                // Start each fish at the first waypoint
                fishWaypointIndices[fish] = 0;
                fishTimers[fish] = 0f; // Timer for sine wave calculation
            }

            while (true)
            {
                // Move each fish towards the current target waypoint
                for (int i = 0; i < fishesToMove.Count; i++)
                {
                    GameObject fish = fishesToMove[i];

                    if (fish != null)
                    {
                        SmallFish smallFishComponent = fish.GetComponent<SmallFish>();
                        if (smallFishComponent == null)
                        {
                            Debug.LogError($"Fish {fish.name} does not have a SmallFish component.");
                            continue; // Skip this fish if it doesn't have the component
                        }

                        float speed = smallFishComponent.SmallFishSpeed; // Get the speed from the fish script

                        int currentWaypointIndex = fishWaypointIndices[fish];
                        Transform targetWaypoint = waypoints[currentWaypointIndex]; // Current target waypoint

                        // Calculate direction to the target waypoint
                        Vector3 directionToWaypoint = (targetWaypoint.position - fish.transform.position).normalized;

                        // Rotate the fish to face the target waypoint smoothly
                        Quaternion targetRotation = Quaternion.LookRotation(directionToWaypoint);
                        fish.transform.rotation = Quaternion.Slerp(fish.transform.rotation, targetRotation, Time.deltaTime * 2.0f); // Smooth rotation

                        // Calculate sine wave offset based on time
                        fishTimers[fish] += Time.deltaTime; // Increment timer
                        float sineWaveAmplitude = 0.5f; // Adjust amplitude as needed
                        float sineWaveFrequency = 1.0f; // Control frequency of the sine wave
                        float sineWaveOffset = Mathf.Sin(fishTimers[fish] * sineWaveFrequency) * sineWaveAmplitude;

                        // Calculate the final target position with the sine wave offset
                        Vector3 targetPosition = new Vector3(targetWaypoint.position.x, targetWaypoint.position.y + sineWaveOffset, targetWaypoint.position.z);

                        // Move the fish towards the target waypoint with smooth damping
                        fish.transform.position = Vector3.MoveTowards(fish.transform.position, targetPosition, speed * Time.deltaTime);

                        // Check if the fish has reached the target waypoint
                        if (Vector3.Distance(fish.transform.position, targetWaypoint.position) < 0.1f)
                        {
                            // Move to the next waypoint
                            currentWaypointIndex++;
                            if (currentWaypointIndex >= waypoints.Count)
                            {
                                // Loop back to the first waypoint if necessary
                                currentWaypointIndex = 0;
                            }

                            // Update the current waypoint index for the fish
                            fishWaypointIndices[fish] = currentWaypointIndex;
                        }
                    }
                }

                yield return null; // Wait for the next frame
            }
        }






        private Vector3 GetRandomSpawnOffset()
        {
            float offsetX = Random.Range(-1f, 1f);
            float offsetY = Random.Range(-1f, 1f);
            float offsetZ = 0f;

            return new Vector3(offsetX, offsetY, offsetZ);
        }

        private bool IsWaypointOccupied(Vector3 waypoint)
        {
            foreach (GameObject fish in activeFishes)
            {
                if (Vector3.Distance(fish.transform.position, waypoint) < minSpawnDistanceBetweenFishes)
                {
                    Debug.Log("IswaypointOccupied");
                    return true; // Waypoint is occupied
                }
            }
            return false; // No fish at this waypoint
        }

        private SharkGameDataModel.SmallFishType GetRandomSmallFishType()
        {
            Debug.Log("FishPoolList Length" + ObjectPooling.Instance._fishPoolList.Count);
            return ObjectPooling.Instance._fishPoolList[Random.Range(0, ObjectPooling.Instance._fishPoolList.Count)]._smallFishType;
        }

        internal void PushBackObjectsToPool()
        {
            if(_spawnPoint.childCount > 0)
            {
                for(int i=0;i<_spawnPoint.childCount;i++)
                {
                    Transform childObject = _spawnPoint.GetChild(i);
                    if (childObject.gameObject.activeInHierarchy)
                    {
                        childObject.gameObject.SetActive(false);
                        childObject.GetComponent<BackToPool>().PushBackToPool();
                    }
                }
            }
            activeFishes.Clear();
        }

        internal void ClearActiveFishList()
        {
            foreach(var item in activeFishes)
            {
                item.GetComponent<BackToPool>().PushBackToPool();
            }
            activeFishes.Clear();
        }
    }
}
