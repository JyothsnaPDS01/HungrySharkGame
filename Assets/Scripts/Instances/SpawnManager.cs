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

        [Header("Fish Settings")]
        [SerializeField] private float minSpawnDistanceBetweenFishes = 0.5f;
        [SerializeField] private float minDistanceFromShark = 2f;

        private List<GameObject> activeFishes = new List<GameObject>(); // Active fish tracking

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

        private void Start()
        {
            if (SharkGameManager.Instance.CurrentGameMode == SharkGameDataModel.GameMode.GameStart)
            {
                StartCoroutine(CallSpawnFishesFrequently());
            }
        }

        private IEnumerator CallSpawnFishesFrequently()
        {
            while (true)
            {
                SpawnFishesAtWaypoints();
                yield return new WaitForSeconds(1f);
            }
        }

        public void SpawnFishesAtWaypoints()
        {
            foreach (var waypoint in waypoints)
            {
                if (Vector3.Distance(_playerShark.position, waypoint.position) > minDistanceFromShark)
                {
                    List<Vector3> fishOffsets = new List<Vector3>(); // To hold the offsets
                    List<GameObject> fishesToMove = new List<GameObject>(); // Keep track of spawned fishes

                    int fishCount = Random.Range(4, 6); // Randomly choose between 4 and 5 fishes
                    for (int i = 0; i < fishCount; i++)
                    {
                        Vector3 spawnPosition = waypoint.position + GetRandomSpawnOffset();

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

                            // Reset rotation to horizontal
                            fish.transform.rotation = Quaternion.Euler(0, 90, 0); // Adjust rotation as needed

                            SmallFish smallFish = fish.GetComponent<SmallFish>();
                            if (smallFish == null)
                            {
#if UNITY_EDITOR
                                Debug.LogError($"Spawned fish {fish.name} does not have a SmallFish component.");
#endif
                                ObjectPooling.Instance.ReturnToPool(fish, GetRandomSmallFishType());
                                continue;
                            }

                            smallFish.SetWaypoints(waypoints); // Assign waypoints to the fish
                            fishesToMove.Add(fish); // Track the spawned fish
                            fishOffsets.Add(spawnPosition - waypoint.position); // Store the relative position
                        }
                    }

                    // Check if fishesToMove and fishOffsets are the same length before starting movement
                    if (fishesToMove.Count > 0 && fishesToMove.Count == fishOffsets.Count)
                    {
                        activeFishes.AddRange(fishesToMove); // Add the new fishes to the active list
                        StartCoroutine(MoveFishGroup(fishesToMove, fishOffsets, waypoint.position));
                    }
                }
            }
        }

        private IEnumerator MoveFishGroup(List<GameObject> fishesToMove, List<Vector3> fishOffsets, Vector3 groupCenter)
        {
            float speed = .5f; // Adjust speed as needed
            Vector3 direction = Vector3.right; // Set the movement direction

            while (true)
            {
                // Move each fish while maintaining their relative offsets
                for (int i = 0; i < fishesToMove.Count; i++)
                {
                    GameObject fish = fishesToMove[i];
                    if (fish != null)
                    {
                        // Update position
                        Vector3 targetPosition = groupCenter + fishOffsets[i] + (direction * speed * Time.deltaTime);
                        fish.transform.position = targetPosition;

                        if (targetPosition.x >= 100f || targetPosition.x <= -55f)
                        {
                            // Reverse direction
                            direction = -direction;
                            fish.transform.rotation = Quaternion.Euler(fish.transform.rotation.x, -fish.transform.rotation.y, fish.transform.rotation.z);
                        }
                    }
                }

                // Update the group center to maintain their movement
                groupCenter += direction * speed * Time.deltaTime;

                yield return null; // Wait for the next frame
            }
        }


        private Vector3 GetRandomSpawnOffset()
        {
            float offsetX = Random.Range(-0.5f, 0.5f);
            float offsetY = Random.Range(-0.5f, 0.5f);
            float offsetZ = 0f;

            return new Vector3(offsetX, offsetY, offsetZ);
        }

        private bool IsWaypointOccupied(Vector3 waypoint)
        {
            foreach (GameObject fish in activeFishes)
            {
                if (Vector3.Distance(fish.transform.position, waypoint) < minSpawnDistanceBetweenFishes)
                {
                    return true; // Waypoint is occupied
                }
            }
            return false; // No fish at this waypoint
        }

        private SharkGameDataModel.SmallFishType GetRandomSmallFishType()
        {
            return ObjectPooling.Instance._fishPoolList[Random.Range(0, ObjectPooling.Instance._fishPoolList.Count)]._smallFishType;
        }
    }
}
