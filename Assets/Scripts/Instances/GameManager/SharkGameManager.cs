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

        [Header("Bomb Prefabs")]
        [SerializeField] private List<SharkGameDataModel.BombObject> bombPrefabsList;

        [Header("Health Duration")]
        [SerializeField] public float _healthDuration;

        [Header("SharkEating Collision")]
        [SerializeField] private GameObject _sharkEatingCollision;

        [Header("Shark Eating Position")]
        [SerializeField] private GameObject _sharkEatingPosition;

        [Header("Time Remaining")]
        [SerializeField] private float timeRemaining;

        [Header("Player Shark Original Position")]
        [SerializeField] private Vector3 _playerSharkOriginalPosition;

        public float PlayerHealthTimerRemaining
        {
            set { timeRemaining = value; }
            get { return timeRemaining;  }
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

        public List<SharkGameDataModel.BombObject> BombObjectLists
        {
            get { return bombPrefabsList; }
        }
        [Header("Current Player coins")]
        [SerializeField] private int currentCoins = 0;

        public int CurrentCoins
        {
            get { return currentCoins; }
            set { currentCoins = value; }
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
            SoundManager.Instance.PlayGameAudioClip(SharkGameDataModel.Sound.UnderWaterSound, true);
        }

        internal void InitializePlayer()
        {
            _playerSharkPrefab.SetActive(true);
            if (!UIController.Instance.quitButtonClicked)
            {
                if (_currentLevel == 1) _playerSharkPrefab.GetComponent<Player>().StartGameStartSequence();
                else
                {
                    _playerSharkPrefab.GetComponent<Player>().GameSequence();
                }
                _playerSharkPrefab.GetComponent<Player>().EnableInput();
            }
            else if(UIController.Instance.quitButtonClicked)
            {
                _playerSharkPrefab.GetComponent<Player>().GameSequence();
                UIController.Instance.quitButtonClicked = false;
            }
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

            UIController.Instance.DestroyBombs();

            UIController.Instance.EnableKillUI();
            
            StartCoroutine(DelayTheLevel());
        }

        private IEnumerator DelayTheLevel()
        {
            yield return new WaitForSeconds(4f);

            _sharkEatingCollision.GetComponent<SmallFishTrigger>().IsOnCoolDown = false;

            ResetPlayerAndObjectPooling();

            _currentLevel = _currentLevel + 1;
            PlayerPrefs.SetInt("CurrentLevel", _currentLevel);
            PlayerPrefs.Save();
            UIController.Instance.SetCurrentLevelConfig();

            UIController.Instance.SetObjectPool();

            UIController.Instance.EnableHuntCompleteScreen();

            yield return new WaitForSeconds(3f);
            
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

        public IEnumerator StartHealthTimer(float duration)
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
                if(UIController.Instance.CurrentPlayerHealth > 0)
                {
                    UIController.Instance.UpdatePlayerHealth(10);
                    StartTimer();
                }
                else if(UIController.Instance.CurrentPlayerHealth == 0)
                {
                    SetGameOver();
                }
                
            }
        }

        #endregion

        #region GameOver

        internal void SetGameOver()
        {
            CurrentGameMode = SharkGameDataModel.GameMode.GameOver;
            StartCoroutine(DelayGameOverUIPanel());
        }

        private IEnumerator DelayGameOverUIPanel()
        {
            yield return new WaitForSeconds(.25f);

            _playerSharkPrefab.GetComponent<Player>().StartDieAnimation();

            _spawnManager.GetComponent<SpawnManager>().ClearActiveFishList();
            _spawnManager.SetActive(false);
            destroyCount = 0;

            StopGameAudio();
            ObjectPooling.Instance.ClearFishPoolList();

            yield return new WaitForSeconds(1f);

            UIController.Instance.SetGameOver();
        }

        internal void ResetGame()
        {
            PlayerPrefs.SetInt("CurrentLevel", 1);
            PlayerPrefs.Save();
            _spawnManager.GetComponent<SpawnManager>().ClearActiveFishList();
            _spawnManager.SetActive(false);
            destroyCount = 0;

            ObjectPooling.Instance.ClearFishPoolList();
        }
        #endregion

        #region ResetPlayerPosition & ObjectPooling

        //This is the common method to set the player position and Set the items back to pool
        internal void ResetPlayerAndObjectPooling()
        {
            _playerSharkPrefab.SetActive(false);

            _playerSharkPrefab.transform.position = _playerSharkOriginalPosition;
            _playerSharkPrefab.transform.rotation = Quaternion.Euler(0, 0, 0);

            _spawnManager.GetComponent<SpawnManager>().ClearActiveFishList();
            _spawnManager.SetActive(false);
            destroyCount = 0;

            StopGameAudio();
            ObjectPooling.Instance.ClearFishPoolList();
        }
        #endregion


        #endregion
    }
}
