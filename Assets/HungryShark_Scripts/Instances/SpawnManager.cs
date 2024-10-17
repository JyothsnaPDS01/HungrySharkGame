using SharkGame.Models;
using System;
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
        [SerializeField] private float minSpawnDistanceBetweenFishes = 20f;
        [SerializeField] private float minDistanceFromShark = 2f;

        [SerializeField] private List<GameObject> activeFishes = new List<GameObject>(); // Active fish tracking

        [Header("Bomb Spawn Positions")]
        [SerializeField] private List<Transform> bombSpawnPoints;

        public List<Transform> BombSpawnPoints
        {
            get
            {
                return bombSpawnPoints;
            }
        }

        private GameObject bombObject;

        public GameObject BombObject { get { return bombObject; } }

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
                Debug.LogError("HandleGameMode SPawnmanager");
                if (SharkGameManager.Instance.CurrentLevel > 1)
                {
                    if (bombObject != null)
                    {
                        Destroy(bombObject);
                    }
                }
                SpawnLevelBomb();
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
                // Check if waypoint is occupied or too close to the shark
                if (Vector3.Distance(_playerShark.position, waypoint.position) > minDistanceFromShark && !IsWaypointOccupied(waypoint.position))
                {
                    int fishCount = 1;

                    for (int i = 0; i < fishCount; i++)
                    {
                        Vector3 spawnPosition = waypoint.position;

                        // Ensure enough space between the new fish and all existing active fishes
                        if (!IsFishTooCloseToOthers(spawnPosition))
                        {
                            GameObject fish = ObjectPooling.Instance.SpawnFromPool(GetRandomSmallFishType(), spawnPosition, Quaternion.identity);

                            

                            if (fish != null)
                            {
                                activeFishes.Add(fish); // Track spawned fish

                                FishGroup smallFish = fish.GetComponent<FishGroup>();
                                if (smallFish != null)
                                {
                                    smallFish.SetWaypoints(waypoints); // Assign waypoints to the fish
                                }
                                else
                                {
                                    Debug.LogWarning("FishGroup component is missing on the spawned fish!");
                                }
                            }
                            else
                            {
                                Debug.LogWarning("SpawnFromPool returned null for fish.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Not enough space to spawn fish at the current waypoint.");
                        }
                    }
                }
            }
        }

        // Checks if the current waypoint is already occupied by any fish
        private bool IsWaypointOccupied(Vector3 spawnPosition)
        {
            foreach (GameObject fish in activeFishes)
            {
                if (fish != null && Vector3.Distance(fish.transform.position, spawnPosition) < minSpawnDistanceBetweenFishes)
                {
                    return true; // Waypoint is occupied
                }
            }
            return false; // No fish at this waypoint
        }

        // This method ensures that the new fish is not too close to other active fishes
        private bool IsFishTooCloseToOthers(Vector3 spawnPosition)
        {
            foreach (GameObject fish in activeFishes)
            {
                if (fish != null && Vector3.Distance(fish.transform.position, spawnPosition) < minSpawnDistanceBetweenFishes)
                {
                    return true; // Too close to another fish
                }
            }
            return false; // Safe to spawn
        }

        private SharkGameDataModel.SmallFishType GetRandomSmallFishType()
        {
            if (ObjectPooling.Instance._fishPoolList.Count > 0)
            {
                return ObjectPooling.Instance._fishPoolList[UnityEngine.Random.Range(0, ObjectPooling.Instance._fishPoolList.Count)]._smallFishType;
            }
            else
            {
                Debug.LogWarning("FishPoolList is empty.");
                return SharkGameDataModel.SmallFishType.None;
            }
        }


        internal void ClearActiveFishList()
        {
            foreach (var item in activeFishes)
            {
                if (item != null)
                {
                    item.GetComponent<FishGroup>().FishGroupDestroyCount = 0;

                    if(item.transform.childCount > 0)
                    {
                        for (int i = 0; i < item.transform.childCount; i++)
                        {
                            Transform childObject = item.transform.GetChild(i);
                            if (childObject.gameObject.activeInHierarchy)
                            {
                                childObject.gameObject.SetActive(false);
                            }
                        }
                    }
                    item.GetComponent<BackToPool>().PushBackToPool();
                }
            }
            activeFishes.Clear();
        }

        public void SpawnLevelBomb()
        {
            Debug.LogError("SpawnLevelBomb");
            SharkGameDataModel.Level currentLevelData = UIController.Instance.GetCurrentLevelData();
            if(currentLevelData.enemies.Count > 0)
            {
                for(int i=0;i< currentLevelData.enemies.Capacity;i++)
                {
                    GameObject _bombObject = SharkGameManager.Instance.BombObjectLists.Find(x => x._bombType == GetBombType(currentLevelData.enemies[i].bomb))._bombObject;
                    Instantiate(_bombObject, bombSpawnPoints[i]);
                }
            }
        }

        public void RespawnBomb(SharkGameDataModel.BombType _bombType)
        {
            Debug.LogError("RespawnBomb");
            Debug.LogError("BombType" + _bombType);
            GameObject bomb = SharkGameManager.Instance.BombObjectLists.Find(x => x._bombType == _bombType)._bombObject;
            Debug.LogError("BombObject" + bomb.name);
            foreach(var item in bombSpawnPoints)
            {
                if(item.childCount == 0)
                {
                    Instantiate(bomb, item);
                    break;
                }
            }
            
        }

        SharkGameDataModel.BombType bombType;
        private SharkGameDataModel.BombType GetBombType(string bombName)
        {
            // Attempt to parse the string into the enum type
            if (Enum.TryParse(bombName, true, out bombType))
            {
                return bombType; // Successfully parsed
            }
            else
            {
                return SharkGameDataModel.BombType.None;
            }
        }

    }
}