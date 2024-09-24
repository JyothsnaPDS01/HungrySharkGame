using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharkGame.Models;

namespace SharkGame
{
    public class ObjectPooling : MonoBehaviour
    {
        #region Private Variables
        [Header("List of Fish Objects")]
        [SerializeField] public List<SharkGameDataModel.FishPool> _fishPoolList = new List<SharkGameDataModel.FishPool>();

        private Dictionary<SharkGameDataModel.SmallFishType, Queue<GameObject>> _fishPoolDictionary = new Dictionary<SharkGameDataModel.SmallFishType, Queue<GameObject>>();
        #endregion

        #region Instance Creation
        public static ObjectPooling _instance;
        public static ObjectPooling Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ObjectPooling>();
                    if (_instance == null)
                    {
                        Debug.LogError("There is no ObjectPooling in the scene!");
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
            InstantiatePoolObjects();
        }
        #endregion

        #region Private Methods
        private void InstantiatePoolObjects()
        {
            foreach (var item in _fishPoolList)
            {
                Queue<GameObject> fishObjectQueue = new Queue<GameObject>();

                for (int i = 0; i < item._capacity; i++)
                {
                    GameObject obj = Instantiate(item._fishObject);
                    obj.SetActive(false);
                    fishObjectQueue.Enqueue(obj);
                }

                _fishPoolDictionary.Add(item._smallFishType, fishObjectQueue);

               
            }
        }

        internal GameObject SpawnFromPool(SharkGameDataModel.SmallFishType _fishType, Vector3 _position, Quaternion _rotation)
        {
            if (!_fishPoolDictionary.ContainsKey(_fishType) || _fishPoolDictionary[_fishType].Count == 0)
                return null;

            // Dequeue from inactive pool
            GameObject spawnedObject = _fishPoolDictionary[_fishType].Dequeue();
            spawnedObject.SetActive(true);
            spawnedObject.transform.position = _position;
            spawnedObject.transform.rotation = _rotation;

            // Check if the object has the SmallFish component
            if (spawnedObject.GetComponent<SmallFish>() == null)
            {
                Debug.LogError($"Spawned object {spawnedObject.name} does not have a SmallFish component.");
                return null; // Handle the error as needed
            }

            // Add to active pool (Consider creating a separate active pool if needed)
            // This may need adjustment if you're maintaining a separate active list.
            return spawnedObject;
        }


        // Method to deactivate and return the object to the inactive pool
        // Method to deactivate and return the object to the inactive pool
        internal void ReturnToPool(GameObject obj, SharkGameDataModel.SmallFishType _fishType)
        {
            if (!_fishPoolDictionary.ContainsKey(_fishType))
            {
                Debug.LogError($"Fish type {_fishType} not found in the pool.");
                return;
            }

            // Check if the object is already inactive
            if (!obj.activeInHierarchy)
            {
                Debug.LogWarning("Object is already inactive.");
                return;
            }
#if UNITY_EDITOR
            Debug.Log("Returning to pool");
#endif

            // Deactivate the object
            obj.SetActive(false);

            // Enqueue back to the inactive pool
            _fishPoolDictionary[_fishType].Enqueue(obj);
        }
        #endregion
    }
}
