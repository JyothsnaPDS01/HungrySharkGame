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

        [Header("Player Shark")]
        [SerializeField] private Transform _playerShark;

        [Header("Game Play Objects Parent")]
        [SerializeField] private Transform _gamePlayObjectsParent;

        [SerializeField] private float minSpawnDistanceBetweenFishes = 0.5f;
        [SerializeField] private float minDistanceFromShark = 2f; // Distance to keep from player shark

        private Vector3 spawnPoint;
        private List<Vector3> spawnedPositions = new List<Vector3>();

        private float spawnDelay = 1f; // Delay between successive fish spawns
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
            if (_sharkDirection == SharkGameDataModel.SharkDirection.Left)
            {
                spawnPoint = new Vector3(_playerShark.position.x - spreadRange, _playerShark.position.y, _playerShark.position.z);
            }
            else if (_sharkDirection == SharkGameDataModel.SharkDirection.Right)
            {
                spawnPoint = new Vector3(_playerShark.position.x + spreadRange, _playerShark.position.y, _playerShark.position.z);
            }
            else if(_sharkDirection == SharkGameDataModel.SharkDirection.Down)
            {
                spawnPoint = new Vector3(_playerShark.position.x, _playerShark.position.y-spreadRange, _playerShark.position.z);
            }

            int _spawnCount = Random.Range(4, 7);
            spawnedPositions.Clear();

            for (int i = 0; i < _spawnCount; i++)
            {
                Vector3 spawnPosition;
                bool validPosition = false;

                // Ensure the spawn position is valid
                while (!validPosition)
                {
                    spawnPosition = GenerateSpawnPosition(spawnPoint);
                    spawnPosition.y = Mathf.Clamp(spawnPosition.y, -20f, -0.5f);

                    if (IsPositionValid(spawnPosition))
                    {
                        validPosition = true;
                        spawnedPositions.Add(spawnPosition);

                        // Instantiate the fish at the valid position
                        GameObject obj = ObjectPooling.Instance.SpawnFromPool(
                            GetRandomSmallFishType(),
                            spawnPosition,
                            Quaternion.Euler(0, 90, 0)
                        );

                        //if (_gamePlayObjectsParent != null)
                        //{
                        //    obj.transform.SetParent(_gamePlayObjectsParent);
                        //}

                        // Introduce delay between successive spawns
                        yield return new WaitForSeconds(spawnDelay);
                    }
                }
            }
        }

        private Vector3 GenerateSpawnPosition(Vector3 basePosition)
        {
            float angle = Random.Range(0f, 360f);
            float distance = Random.Range(spawnDistance, spreadRange);

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

            // Check if the new position overlaps with the shark's capsule collider
            //if (IsPositionOverlappingWithShark(newPosition))
            //{
            //    return false;
            //}

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