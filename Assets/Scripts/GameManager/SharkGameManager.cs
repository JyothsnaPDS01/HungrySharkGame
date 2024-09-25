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

        [Header("Game Level")]
        [SerializeField] private int _currentLevel;

        [Header("Current Level SharkAmount")]
        [SerializeField] private int _targetAmount;
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
            _playerSharkPrefab.GetComponent<Player>().StartGameStartSequence();
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
            SpawnManager.Instance.PushBackObjectsToPool();
            StopGameAudio();

            _currentGameMode = SharkGameDataModel.GameMode.MissionMode;
            _currentLevel = _currentLevel + 1;
            PlayerPrefs.SetInt("CurrentLevel", _currentLevel);
            PlayerPrefs.Save();

            UIController.Instance.LoadNextLevel();
        }
        #endregion
    }
}
