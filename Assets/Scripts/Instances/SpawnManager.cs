using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharkGame.Models;

namespace SharkGame
{
    public class SpawnManager : MonoBehaviour
    {
        #region Private Variables
        [Header("Eatable Fish Components")]
        [SerializeField] private float spawnDistance = 1f;
        [SerializeField] private float spreadRange = 8f;
        [SerializeField] private float clusterRadius = 2f; // Radius of the fish cluster

        [Header("Player Shark")]
        [SerializeField] private Transform _playerShark;

        [Header("Game Play Objects Parent")]
        [SerializeField] private Transform _gamePlayObjectsParent;

        [SerializeField] private float minSpawnDistanceBetweenFishes = 0.5f;
        [SerializeField] private float minDistanceFromShark = 2f; // Distance to keep from player shark

        private Vector3 spawnPoint;
        private List<Vector3> spawnedPositions = new List<Vector3>();

        [SerializeField] private float spawnDelay = .2f; // Delay between successive fish spawns
        private Coroutine currentSpawnCoroutine; // Reference to the currently running coroutine
        #endregion

        #region Creating Instance
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
                        Debug.LogError("There is no SpawnManager in the scene!");
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region MonoBehaviour Methods
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            _playerShark = GameObject.Find("Player_Shark").transform;
        }
        #endregion

        #region Private Methods
        internal void SpawnFishes(SharkGameDataModel.SharkDirection _sharkDirection)
        {
            // If a spawn coroutine is already running, stop it
            if (currentSpawnCoroutine != null)
            {
                StopCoroutine(currentSpawnCoroutine);
            }

            // Start the spawning coroutine
            currentSpawnCoroutine = StartCoroutine(SpawnFishesWithDelay(_sharkDirection));
        }

        private IEnumerator SpawnFishesWithDelay(SharkGameDataModel.SharkDirection _sharkDirection)
        {
            // Set the spawn point based on shark direction
            switch (_sharkDirection)
            {
                case SharkGameDataModel.SharkDirection.Left:
                    spawnPoint = new Vector3(_playerShark.position.x - spreadRange, _playerShark.position.y, _playerShark.position.z);
                    break;
                case SharkGameDataModel.SharkDirection.Right:
                    spawnPoint = new Vector3(_playerShark.position.x + spreadRange, _playerShark.position.y, _playerShark.position.z);
                    break;
                case SharkGameDataModel.SharkDirection.Down:
                    spawnPoint = new Vector3(_playerShark.position.x, _playerShark.position.y - spreadRange, _playerShark.position.z);
                    break;
            }

            // Adjust the cluster position based on the shark's direction
            Vector3 clusterOffset = Vector3.zero;
            switch (_sharkDirection)
            {
                case SharkGameDataModel.SharkDirection.Left:
                    clusterOffset = Vector3.left * spreadRange;
                    break;
                case SharkGameDataModel.SharkDirection.Right:
                    clusterOffset = Vector3.right * spreadRange;
                    break;
                case SharkGameDataModel.SharkDirection.Down:
                    clusterOffset = Vector3.down * spreadRange;
                    break;
            }

            int _spawnCount = Random.Range(6, 10);
            spawnedPositions.Clear();
            int groupSize = 5; // This seems to be fixed, but you can adjust if needed

            // Use a single group spawn without delays
            for (int i = 0; i < _spawnCount; i++)
            {
                Vector3 spawnPosition;
                bool validPosition = false;
                int attempts = 0; // Count attempts to prevent infinite loop

                while (!validPosition && attempts < 100)
                {
                    spawnPosition = GenerateClusteredSpawnPosition(spawnPoint) + clusterOffset;
                    spawnPosition.y = Mathf.Clamp(spawnPosition.y, -20f, -1f);

                    if (IsPositionValid(spawnPosition))
                    {
                        validPosition = true;
                        spawnedPositions.Add(spawnPosition);
                        attempts = 100; // Exit while loop
                    }
                    else
                    {
                        attempts++;
                    }
                }

                if (!validPosition)
                {
                    Debug.LogWarning("Failed to find a valid spawn position after 100 attempts.");
                }
            }

            // Spawn all fishes in the group with no delay between them
            foreach (Vector3 pos in spawnedPositions)
            {
                GameObject fish = ObjectPooling.Instance.SpawnFromPool(GetRandomSmallFishType(), pos, Quaternion.identity);

                if (fish != null)
                {
                    SmallFish smallFish = fish.GetComponent<SmallFish>();
                    if (smallFish != null)
                    {
                        smallFish.ResetFishState();
                        // Set the movement direction for all fishes in the group
                       // smallFish.SetMovementDirection(clusterOffset.normalized); // Assuming SetMovementDirection sets the direction the fish moves
                    }
                }
            }

            yield return null; // Ensures coroutine completes without delays
        }


        private Vector3 GenerateClusteredSpawnPosition(Vector3 basePosition)
        {
            // Generate a position within a cluster radius around the base position
            float angle = Random.Range(0f, 360f);
            float distance = Random.Range(0f, clusterRadius); // Use cluster radius

            float radian = angle * Mathf.Deg2Rad;

            return basePosition + new Vector3(
                Mathf.Cos(radian) * distance,
                Mathf.Sin(radian) * distance,
                0f
            );
        }

        private bool IsPositionValid(Vector3 newPosition)
        {
            // Check distance from the player shark
            if (Vector3.Distance(newPosition, _playerShark.position) < minDistanceFromShark)
            {
                return false;
            }

            // Check distance from other spawned fish
            foreach (Vector3 existingPosition in spawnedPositions)
            {
                if (Vector3.Distance(existingPosition, newPosition) < minSpawnDistanceBetweenFishes)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsPositionOverlappingWithShark(Vector3 position)
        {
            // Assuming you have assigned the capsule collider to a variable
            CapsuleCollider sharkCapsuleCollider = _playerShark.transform.GetChild(1).GetComponent<CapsuleCollider>();

            if (sharkCapsuleCollider != null)
            {
                // Define the capsule's center and height
                Vector3 capsuleCenter = _playerShark.position + sharkCapsuleCollider.center;
                float capsuleHeight = sharkCapsuleCollider.height;
                float capsuleRadius = sharkCapsuleCollider.radius;

                // Check if the position is inside the capsule collider
                // Calculate the distance to the closest point on the capsule's surface
                float distanceToClosestPoint = Mathf.Sqrt(Mathf.Pow(position.x - capsuleCenter.x, 2) + Mathf.Pow(position.z - capsuleCenter.z, 2)) - capsuleRadius;
                float distanceToTopBottom = Mathf.Abs(position.y - capsuleCenter.y);

                // Check if the position is within the capsule bounds
                if (distanceToClosestPoint <= 0 && distanceToTopBottom <= capsuleHeight / 2)
                {
                    return true;
                }
            }

            return false;
        }

        private SharkGameDataModel.SmallFishType GetRandomSmallFishType()
        {
            return ObjectPooling.Instance._fishPoolList[Random.Range(0, ObjectPooling.Instance._fishPoolList.Count)]._smallFishType;
        }
        #endregion
    }
}
