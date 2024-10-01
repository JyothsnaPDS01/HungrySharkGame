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
                }
            }
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
            if (ObjectPooling.Instance._fishPoolList.Count > 0)
            {
                return ObjectPooling.Instance._fishPoolList[Random.Range(0, ObjectPooling.Instance._fishPoolList.Count)]._smallFishType;
            }
            else return SharkGameDataModel.SmallFishType.None;
            
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
