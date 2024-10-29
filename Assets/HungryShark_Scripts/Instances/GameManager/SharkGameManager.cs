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
        [SerializeField] private SmallFishTrigger _sharkEatingCollision;

        [Header("Time Remaining")]
        [SerializeField] private float timeRemaining;

        [Header("Player Shark Original Position")]
        [SerializeField] private Vector3 _playerSharkOriginalPosition;

        [Header("Player Sharks List")]
        [SerializeField] private List<SharkGameDataModel.SharkPrefabsWithSmallFishTriggerClass> _playerSharksList;

        [Header("Main Camera")]
        [SerializeField] private CameraFollow _mainCameraFollow;

        [Header("Game Environment SpawnPoint")]
        [SerializeField] private Transform _gameEnvironmentSpawnPoint;
        [Header("Game Environment Prefab")]
        [SerializeField] private GameObject _gameEnvironmentPrefab;
       
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
            set
            {
                _currentLevel = value;
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
        [SerializeField] private int currentCoins;

        public int CurrentCoins
        {
            get { return currentCoins; }
            set { currentCoins = value; }
        }

        [Header("Current Player Gems")]
        [SerializeField] private int currentGems;

        public int CurrentGems {  get { return currentGems;  } set { currentGems = value; } }

        [Header("Level Fail")]
        [SerializeField] private bool isLevelFail;

        public bool IsLevelFail {  get { return isLevelFail; } set { isLevelFail = value; } }

        [Header("DailyRewardData")]
        [SerializeField] private List<SharkGameDataModel.DailyRewardData> _dailyRewardData;

        [SerializeField] private SpawnObjectAddressables _spawnObjectAddressables;
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

        private void Start()
        {
            // Set target frame rate to 60 FPS for smooth performance
            Application.targetFrameRate = 30;

            // Optionally, disable VSync for better control over frame rate
            QualitySettings.vSyncCount = 0;

            //LoadTheGameEnvironmentAssets();
            
        }


        #endregion

        #region GameMode

        #region Events
        public event Action<SharkGameDataModel.GameMode> OnGameModeChanged;

        public event Action<GameObject> OnUnderWaterEnvironmentSetup;
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
                _playerSharkPrefab.GetComponent<Player>().StartGameStartSequence();
            }
            else if(UIController.Instance.quitButtonClicked)
            {
                _playerSharkPrefab.GetComponent<Player>().GameSequence();
                UIController.Instance.quitButtonClicked = false;
            }
            _playerSharkPrefab.GetComponent<Player>().DisableBloodEffect();
            if (_currentLevel == 2) _sharkEatingCollision.DetectionRadius = 1.5f;
            else if (_currentLevel >= 3) _sharkEatingCollision.DetectionRadius = 2f;
            _spawnManager.SetActive(true);
            _objectPooling.GetComponent<ObjectPooling>().HandleGameMode(CurrentGameMode);
            _spawnManager.GetComponent<SpawnManager>().HandleGameMode(CurrentGameMode);
            _spawnManager.GetComponent<SpawnManager>().SetPlayerTransform(_playerSharkPrefab.transform);
        }

        internal void InitializeLevel()
        {
            _currentGameMode = SharkGameDataModel.GameMode.MissionMode;

            if (PlayerPrefs.HasKey("CurrentLevel"))
            {
                // The key "CurrentLevel" exists, you can retrieve its value safely.
                _currentLevel = PlayerPrefs.GetInt("CurrentLevel");
            }
            else
            {
                // The key "CurrentLevel" doesn't exist, you can set it to a default value.
                PlayerPrefs.SetInt("CurrentLevel", 1);
                PlayerPrefs.Save();
                _currentLevel = PlayerPrefs.GetInt("CurrentLevel");
            }

            if(!PlayerPrefs.HasKey("CurrentCoins"))
            {
                PlayerPrefs.SetInt("CurrentCoins", 0);
                PlayerPrefs.Save();
                CurrentCoins = PlayerPrefs.GetInt("CurrentCoins");
            }

            if(!PlayerPrefs.HasKey("CurrentGems"))
            {
                PlayerPrefs.SetInt("CurrentGems", 0);
                currentGems = PlayerPrefs.GetInt("CurrentGems");
            }

            _targetAmount = UIController.Instance.GetTargetAmount(CurrentLevel);

            Debug.Log("TargetAmount" + _targetAmount);
        }

        internal void LoadNextLevel()
        {
            _currentGameMode = SharkGameDataModel.GameMode.MissionMode;

            UIController.Instance.DestroyBombs();

            UIController.Instance.EnableKillUI();
            
            StartCoroutine(DelayTheLevel());
        }

        private bool _loadTheGameFromStart;

        public bool LoadTheGameFromStart { get { return _loadTheGameFromStart;  } set { _loadTheGameFromStart = value; } }
        private IEnumerator DelayTheLevel()
        {
            yield return new WaitForSeconds(4f);

            _sharkEatingCollision.IsOnCoolDown = false;

            ResetPlayerAndObjectPooling();

            if(_currentLevel <= 19)
            {
                _currentLevel = _currentLevel + 1;

                PlayerPrefs.SetInt("CurrentLevel", _currentLevel);
                PlayerPrefs.Save();
                UIController.Instance.SetCurrentLevelConfig();

                UIController.Instance.SetObjectPool();

                UIController.Instance.EnableHuntCompleteScreen();

                yield return new WaitForSeconds(3f);

                _targetAmount = UIController.Instance.GetTargetAmount(CurrentLevel);
            }

            else if (_currentLevel == 20)
            {
                UIController.Instance.EnableHuntCompleteScreen();
                _loadTheGameFromStart = true;
            }
        }

        internal GameObject GetSmallFishPrefab(SharkGameDataModel.SmallFishType _smallFishType)
        {
            return smallFishesPrefabList.Find(x => x._smallFishType == _smallFishType)._fishObject;
        }

        #region Health Timer
        internal void StartTimer()
        {
            if(CurrentGameMode == SharkGameDataModel.GameMode.GameStart)
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
            }
        }

        #endregion

        #region GameOver

        internal void SetGameOver()
        {
            CurrentGameMode = SharkGameDataModel.GameMode.GameOver;
            StartCoroutine(DelayGameOverUIPanel());
            IsLevelFail = true;
        }

        private IEnumerator DelayGameOverUIPanel()
        {
            yield return null;

            if (_playerSharkPrefab.GetComponent<Player>().GetSharkType() == SharkGameDataModel.SharkType.GeneralShark)
            {
                _playerSharkPrefab.GetComponent<Player>().StartDieAnimation();
            }
            else if (_playerSharkPrefab.GetComponent<Player>().GetSharkType() == SharkGameDataModel.SharkType.WhaleShark ||
                _playerSharkPrefab.GetComponent<Player>().GetSharkType() == SharkGameDataModel.SharkType.TigerShark ||
                _playerSharkPrefab.GetComponent<Player>().GetSharkType() == SharkGameDataModel.SharkType.LemonShark ||
                _playerSharkPrefab.GetComponent<Player>().GetSharkType() == SharkGameDataModel.SharkType.SandShark ||
                _playerSharkPrefab.GetComponent<Player>().GetSharkType() == SharkGameDataModel.SharkType.LeopardShark)
            {
                _playerSharkPrefab.GetComponent<Player>().UnlockSharkDieAnimationTrigger();
                _playerSharkPrefab.GetComponent<Player>().UnlockSharkStartDieAnimation();
            }
            UIController.Instance.DestroyBombs();

            _spawnManager.GetComponent<SpawnManager>().ClearActiveFishList();
            _spawnManager.SetActive(false);
            destroyCount = 0;

         //   StopGameAudio();
            ObjectPooling.Instance.ClearFishPoolList();

            yield return new WaitForSeconds(1f);

            UIController.Instance.SetGameOver();
        }

        internal void ResetGameFromStartingLevel()
        {
            Debug.LogError("ResetGame");

            
                PlayerPrefs.SetInt("CurrentLevel", 1);
                PlayerPrefs.Save();
                Debug.LogError("CurrentLevel" + PlayerPrefs.GetInt("CurrentLevel"));
                _currentLevel = PlayerPrefs.GetInt("CurrentLevel");
            

            _spawnManager.GetComponent<SpawnManager>().ClearActiveFishList();
            _spawnManager.SetActive(false);
            destroyCount = 0;

            ObjectPooling.Instance.ClearFishPoolList();
        }

        internal void ResetLevelFromCurrentLevel()
        {
            _currentLevel = PlayerPrefs.GetInt("CurrentLevel");

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
            _playerSharkPrefab.GetComponent<Player>().InitialMovement = false;

            _playerSharkPrefab.transform.position = _playerSharkOriginalPosition;
            _playerSharkPrefab.transform.rotation = Quaternion.Euler(0, 0, 0);

            _spawnManager.GetComponent<SpawnManager>().ClearActiveFishList();
            _spawnManager.SetActive(false);
            destroyCount = 0;

            // StopGameAudio();
            ObjectPooling.Instance.ClearFishPoolList();
        }
        #endregion
        public void SelectedPlayer(int _selectedIndex)
        {
            Debug.LogError("SelectedPlayer Index" + _selectedIndex);

            int _sharkSelectedIndex = _playerSharksList.FindIndex(x => x._sharkIndex == _selectedIndex);
            _playerSharkPrefab = _playerSharksList[_sharkSelectedIndex]._playerObject;
            _sharkEatingCollision = _playerSharksList[_sharkSelectedIndex]._smallFishTrigger;
            if(UIController.Instance.IsTutorialEnabled == 1) _playerSharkPrefab.GetComponent<Player>().EnableInput();

            _mainCameraFollow.targetRigidbody = _playerSharkPrefab.GetComponent<Rigidbody>();

            UIController.Instance.SetPlayer(_playerSharkPrefab);
        }

        public Sprite GetRewardImage(int _index)
        {
            return _dailyRewardData.Find(x => x.index == _index)._rewardImage;
        }

        public int GetRewardAmount(int _index)
        {
            return _dailyRewardData.Find(x => x.index == _index)._amount;
        }

        public SharkGameDataModel.DailyRewardType GetDailyRewardType(int _index)
        {
            return _dailyRewardData.Find(x => x.index == _index)._dailyRewardType;
        }
        #endregion
    }
}
