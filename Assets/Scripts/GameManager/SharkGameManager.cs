using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharkGame.Models;
using System;

namespace SharkGame
{
    public class SharkGameManager : MonoBehaviour
    {
        #region Creating Instance

        private static SharkGameManager _instance;
        public static SharkGameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SharkGameManager>();

                    // If still null, create a new instance
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(SharkGameManager).ToString());
                        _instance = singletonObject.AddComponent<SharkGameManager>();
                    }
                }

                return _instance;
            }
        }
        #endregion

        #region Private Variables
        [Header("Shark Game Mode")]
        [SerializeField] private SharkGameDataModel.GameMode _currentGameMode;

        [Header("UnderWaterAudio")]
        [SerializeField] private AudioSource _underWaterAudio;

        [Header("Player Shark Prefab")]
        [SerializeField] private GameObject _playerSharkPrefab;

        [Header("SpawnManager")]
        [SerializeField] private GameObject _spawnManager;

        [Header("ObjectPooling")]
        [SerializeField] private GameObject _objectPooling;

        [Header("Game Level")]
        [SerializeField] private int _currentLevel;

        [Header("Current Level SharkAmount")]
        [SerializeField] private int _targetAmount;

        [Header("Destroy Count")]
        [SerializeField] private int destroyCount = 0;

        [Header("Small Fishes Prefabs List")]
        [SerializeField] private List<SharkGameDataModel.SmallFishes> smallFishesPrefabList;

        [Header("Health Duration")]
        [SerializeField] private float _healthDuration;

        [Header("SharkEating Collision")]
        [SerializeField] private GameObject _sharkEatingCollision;

        [Header("Shark Eating Position")]
        [SerializeField] private GameObject _sharkEatingPosition;

        [Header("Time Remaining")]
        [SerializeField] private float timeRemaining;

        public float PlayerHealthTimerRemaining
        {
            get { return timeRemaining; }
            set { timeRemaining = value; }
        }

        public int CurrentLevel
        {
            get
            {
                return _currentLevel;
            }
        }

        public int CurrentLevelTargetAmount
        {
            get
            {
                return _targetAmount;
            }
        }
       
        public int DestroyCount
        {
            get
            {
                return destroyCount;
            }
            set
            {
                destroyCount = value;
            }
        }
        #endregion

        #region MonoBehaviour Methods
        // Optional: Prevent multiple instances from existing
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject); // Keep the instance between scenes
            }
            else if (_instance != this)
            {
                Destroy(gameObject); // Destroy duplicate instance
            }
        }
        #endregion

        #region GameMode

        #region Events
        public event Action<SharkGameDataModel.GameMode> OnGameModeChanged;
        #endregion

        public SharkGameDataModel.GameMode CurrentGameMode
        {
            get
            {
                return _currentGameMode;
            }
            set
            {
                _currentGameMode = value;
                OnGameModeChanged?.Invoke(CurrentGameMode);
            }
        }
        #endregion

        #region Public Methods
        internal void PlayGameAudio()
        {
            _underWaterAudio.Play();
        }

        internal void InitializePlayer()
        {
            _playerSharkPrefab.SetActive(true);
            if(_currentLevel == 1) _playerSharkPrefab.GetComponent<Player>().StartGameStartSequence();
            _playerSharkPrefab.GetComponent<Player>().DisableBloodEffect();
            if (_currentLevel == 2) _sharkEatingCollision.GetComponent<SmallFishTrigger>().DetectionRadius = 1.5f;
            else if (_currentLevel == 3) _sharkEatingCollision.GetComponent<SmallFishTrigger>().DetectionRadius = 2f;
            _spawnManager.SetActive(true);
            _objectPooling.GetComponent<ObjectPooling>().HandleGameMode(CurrentGameMode);
            _spawnManager.GetComponent<SpawnManager>().HandleGameMode(CurrentGameMode);
        }

        internal void InitializeLevel()
        {
            _currentGameMode = SharkGameDataModel.GameMode.MissionMode;

            PlayerPrefs.SetInt("CurrentLevel", 1);
            PlayerPrefs.Save();

            _currentLevel = PlayerPrefs.GetInt("CurrentLevel");
            _targetAmount = UIController.Instance.GetTargetAmount(CurrentLevel);
        }

        private void StopGameAudio()
        {
            _underWaterAudio.Stop();
        }

        internal void LoadNextLevel()
        {
            _currentGameMode = SharkGameDataModel.GameMode.MissionMode;

            UIController.Instance.EnableKillUI();

            
            StartCoroutine(DelayTheLevel());
        }

        private IEnumerator DelayTheLevel()
        {
            yield return new WaitForSeconds(1f);

            _sharkEatingCollision.GetComponent<SmallFishTrigger>().IsOnCoolDown = false;
            _playerSharkPrefab.SetActive(false);
            _spawnManager.GetComponent<SpawnManager>().ClearActiveFishList();
            _spawnManager.SetActive(false);
            destroyCount = 0;

            StopGameAudio();
            ObjectPooling.Instance.ClearFishPoolList();

            _currentLevel = _currentLevel + 1;
            PlayerPrefs.SetInt("CurrentLevel", _currentLevel);
            PlayerPrefs.Save();

            UIController.Instance.DisableKillUI();

            yield return new WaitForSeconds(1f);
            
            UIController.Instance.LoadNextLevel();
            _targetAmount = UIController.Instance.GetTargetAmount(CurrentLevel);
        }

        internal GameObject GetSmallFishPrefab(SharkGameDataModel.SmallFishType _smallFishType)
        {
            return smallFishesPrefabList.Find(x => x._smallFishType == _smallFishType)._fishObject;
        }

        #region Health Timer
        internal void StartTimer()
        {
            StartCoroutine(StartHealthTimer(_healthDuration));
        }

        private IEnumerator StartHealthTimer(float duration)
        {
            timeRemaining = duration;

            while(timeRemaining > 0)
            {
                Debug.Log("Time Remaining" + Mathf.Max(timeRemaining, 0).ToString("F2") + " seconds remaining");
                yield return new WaitForSeconds(1f);
                timeRemaining--;
            }

            HealthTimerEnded();
        }

        private void HealthTimerEnded()
        {
            Debug.Log("Health Timer Ended");

            if(timeRemaining == 0)
            {
                UIController.Instance.UpdatePlayerHealth(5);
                StartTimer();
            }
        }
        #endregion


        #endregion
    }
}
