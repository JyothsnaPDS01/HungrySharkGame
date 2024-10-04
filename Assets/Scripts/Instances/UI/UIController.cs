using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SharkGame.Models;
using SharkGame;
using DG.Tweening;

public class UIController : MonoBehaviour
{
    #region Creating Instance
    private static UIController _instance;

    public static UIController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UIController>();

                // If still null, create a new instance
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(UIController).ToString());
                    _instance = singletonObject.AddComponent<UIController>();
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

    #region Private Variables
    [Header("Mission UI Components")]
    [SerializeField] private Text _levelNumberTMP;
    [SerializeField] private Text _targetDescTMP;
    [SerializeField] private Text _killDescText;
    [SerializeField] private GameObject _killImage;

    [Header("Level Panel")]
    [SerializeField] private GameObject _missionPanel;

    [Header("GameOver Panel")]
    [SerializeField] private GameObject _gameOverPanel;

    [Header("Hunt Complete Panel")]
    [SerializeField] private GameObject _huntCompletePanel;

    [Header("Game UI Panel")]
    [SerializeField] private GameObject _GamePanel;

    [SerializeField] private SharkGameDataModel.LevelConfig levelConfig;

    [Header("Data Load Manager")]
    [SerializeField] private DataLoadManager _dataLoadManager;

    [Header("Player Reference")]
    [SerializeField] private GameObject _player;

    private SharkGameDataModel.Level _currentLevelData;

    [Header("In Game UI Components")]
    [SerializeField] private Text _killAmountTMP;
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Slider _ammoSlider;

    [Header("Player Health Values")]
    [SerializeField] private int _playerMaxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("Ammo Values")]
    [SerializeField] private int _ammoMaxValue = 100;
    [SerializeField] private int currentAmmoValue;

    public int CurrentAmmo { get { return currentAmmoValue; } }

    #endregion

    #region MonoBehaviour Methods
    private void Start()
    {
        levelConfig = _dataLoadManager.GetLevelConfig();
        currentHealth = _playerMaxHealth;
        currentAmmoValue = _ammoMaxValue;
        LoadInitialLevel();
    }
    #endregion

    #region Button Actions
    public void LevelButtonClick()
    {
        SoundManager.Instance.PlayAudioClip(SharkGameDataModel.Sound.Button);
#if UNITY_EDITOR
        Debug.LogError("LevelButtonClick");
#endif

        StartCoroutine(GiveSlightDelay());
    }

    private IEnumerator GiveSlightDelay()
    {
        yield return new WaitForSeconds(.5f);
        _missionPanel.SetActive(false);

        _killAmountTMP.text = "0 /" + _currentLevelData.targets[0].amount.ToString();

        _GamePanel.SetActive(true);
        SharkGameManager.Instance.CurrentGameMode = SharkGameDataModel.GameMode.GameStart;
        SharkGameManager.Instance.PlayGameAudio();
        SharkGameManager.Instance.InitializePlayer();
    }

    public void UpdateKillAmount()
    {
        _killAmountTMP.text = SharkGameManager.Instance.DestroyCount + " / " + _currentLevelData.targets[0].amount.ToString();
    }

    public void SetCurrentLevelConfig()
    {
        _currentLevelData = levelConfig.levels[SharkGameManager.Instance.CurrentLevel - 1];
    }
    #endregion

    #region Private Methods
    private void LoadInitialLevel()
    {
        SharkGameManager.Instance.InitializeLevel();

        _currentLevelData = levelConfig.levels[SharkGameManager.Instance.CurrentLevel - 1];
        _levelNumberTMP.text = "Level Number : " + _currentLevelData.levelNumber.ToString();
        _targetDescTMP.text = _currentLevelData.targets[0].description.ToString();

        Debug.Log("pOOL QUANTITY" + _currentLevelData.smallObjects.Capacity);

        SetObjectPool();
    }

    internal void SetObjectPool()
    {
        ObjectPooling.Instance.SetPoolData(_currentLevelData.bufferAmount, _currentLevelData.smallObjects);
    }

    internal int GetTargetAmount(int _level)
    {
        _dataLoadManager.targetAmount = 0;
        return _dataLoadManager.GetTargetAmount(_level);
    }

    internal SharkGameDataModel.Level GetCurrentLevelData()
    {
        return _currentLevelData;
    }

    public void LoadNextLevel()
    {
        SoundManager.Instance.PlayAudioClip(SharkGameDataModel.Sound.Button);

        _missionPanel.SetActive(true);
        _huntCompletePanel.SetActive(false);

        _currentLevelData = levelConfig.levels[SharkGameManager.Instance.CurrentLevel - 1];
        _levelNumberTMP.text = "Level Number : " + _currentLevelData.levelNumber.ToString();
        _targetDescTMP.text = _currentLevelData.targets[0].description.ToString();
    }

    internal void EnableKillUI()
    {
        SoundManager.Instance.PlayAudioClip(SharkGameDataModel.Sound.MissionPassed);

        _missionPanel.SetActive(true);
        _GamePanel.SetActive(false);

        _targetDescTMP.text = _currentLevelData.targets[0].description.ToString();
        _killImage.SetActive(true);

        _killImage.transform.DOScale(Vector3.one, .5f);
    }

    internal void DisableKillUI()
    {
        _killImage.SetActive(false);
        _killImage.transform.DOScale(Vector3.zero, .2f);
    }

    internal void EnableHuntCompleteScreen()
    {
        _missionPanel.SetActive(false);
        _huntCompletePanel.SetActive(true);

        DisableKillUI();
    }

    internal void UpdatePlayerHealth(int _damageAmount)
    {
        if (currentHealth > 0)
        {
            currentHealth -= _damageAmount;
        }
        _healthSlider.value = (float)currentHealth / _playerMaxHealth;
    }

    internal void MakeMaxHealth()
    {
        currentHealth = 100;
        _healthSlider.value = (float)currentHealth / _playerMaxHealth;
        SharkGameManager.Instance.PlayerHealthTimerRemaining = SharkGameManager.Instance._healthDuration;
        SharkGameManager.Instance.StartTimer();
    }

    internal void UpdateAmmoHealth(int _damageAmount)
    {
        if (currentAmmoValue > 0)
        {
            currentAmmoValue -= _damageAmount;

            // Prevent currentAmmoValue from going below zero
            if (currentAmmoValue < 0)
            {
                currentAmmoValue = 0;
            }

            // Update the slider value safely
            if (_ammoMaxValue > 0)
            {
                _ammoSlider.value = (float)currentAmmoValue / _ammoMaxValue;
            }
            else
            {
                _ammoSlider.value = 0; // Handle the case where ammo max value is zero
            }
        }
    }


    internal void SetGameOver()
    {
        SoundManager.Instance.PlayAudioClip(SharkGameDataModel.Sound.MissionFail);
        _GamePanel.SetActive(false);
        _gameOverPanel.SetActive(true);
    }
    #endregion
}
