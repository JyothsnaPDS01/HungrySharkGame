using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharkGame.Models;

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
        [Header("Shark Game Mode")]
        [SerializeField] private SharkGameDataModel.GameMode _currentGameMode;

        [Header("UnderWaterAudio")]
        [SerializeField] private AudioSource _underWaterAudio;
        public SharkGameDataModel.GameMode CurrentGameMode
        {
            get
            {
                return _currentGameMode;
            }
            set
            {
                _currentGameMode = value;
            }
        }
        #endregion

        #region Public Methods
        internal void PlayGameAudio()
        {
            _underWaterAudio.Play();
        }
        #endregion
    }
}
