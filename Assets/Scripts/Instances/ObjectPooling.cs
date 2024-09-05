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

        private Dictionary<SharkGameDataModel.SmallFishType, Queue<GameObject>> _inactiveFishPool = new Dictionary<SharkGameDataModel.SmallFishType, Queue<GameObject>>();
        private Dictionary<SharkGameDataModel.SmallFishType, List<GameObject>> _activeFishPool = new Dictionary<SharkGameDataModel.SmallFishType, List<GameObject>>();
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
                List<GameObject> activeFishList = new List<GameObject>();

                for (int i = 0; i < item._capacity; i++)
                {
                    GameObject obj = Instantiate(item._fishObject);
                    obj.SetActive(false);
                    fishObjectQueue.Enqueue(obj);
                }

                _inactiveFishPool.Add(item._smallFishType, fishObjectQueue);
                _activeFishPool.Add(item._smallFishType, activeFishList);
            }
        }

        internal GameObject SpawnFromPool(SharkGameDataModel.SmallFishType _fishType, Vector3 _position, Quaternion _rotation)
        {
            if (!_inactiveFishPool.ContainsKey(_fishType) || _inactiveFishPool[_fishType].Count == 0)
                return null;

            // Dequeue from inactive pool
            GameObject spawnedObject = _inactiveFishPool[_fishType].Dequeue();
            spawnedObject.SetActive(true);
            spawnedObject.transform.position = _position;
            spawnedObject.transform.rotation = _rotation;

            // Add to active pool
            _activeFishPool[_fishType].Add(spawnedObject);

            return spawnedObject;
        }

        // Method to deactivate and return the object to the inactive pool
        internal void ReturnToPool(GameObject obj, SharkGameDataModel.SmallFishType _fishType)
        {
            if (!_activeFishPool.ContainsKey(_fishType))
                return;

            // Deactivate and remove from active list
            obj.SetActive(false);
            _activeFishPool[_fishType].Remove(obj);

            // Enqueue back to the inactive pool
            _inactiveFishPool[_fishType].Enqueue(obj);
        }
        #endregion
    }
}
