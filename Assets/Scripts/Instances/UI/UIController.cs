﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SharkGame.Models;
using SharkGame;
using DG.Tweening;
using TMPro;

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

    [Header("Mission Panel Continue Button")]
    [SerializeField] private GameObject _continueButton;

    [Header("GameOver Panel")]
    [SerializeField] private GameObject _gameOverPanel;

    [Header("Hunt Complete Panel")]
    [SerializeField] private GameObject _huntCompletePanel;
    [SerializeField] private GameObject _rayImage;

    [Header("Hunt Complete Panel Coins TMP")]
    [SerializeField] private TextMeshProUGUI _bonusAmountTMP;
    [SerializeField] private Text _coinsTMP;

    [Header("Game Pause Panel")]
    [SerializeField] private GameObject _gamePausePanel;
    [SerializeField] private GameObject _gamePauseAnimationPanel;

    [Header("Game UI Panel")]
    [SerializeField] private GameObject _GamePanel;
    [Header("InGame Coins Panel")]
    [SerializeField] private Text _inGameCoinsTMP;

    [SerializeField] private SharkGameDataModel.LevelConfig levelConfig;

    [Header("Data Load Manager")]
    [SerializeField] private DataLoadManager _dataLoadManager;

    [Header("CoinSpawnManager")]
    [SerializeField] private CoinSpawnManager _coinSpawnManager;

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

    public int CurrentPlayerHealth { get { return currentHealth; } }

    #endregion

    #region MonoBehaviour Methods
    private void Start()
    {
        levelConfig = _dataLoadManager.GetLevelConfig();
        currentHealth = _playerMaxHealth;
        currentAmmoValue = _ammoMaxValue;
        _inGameCoinsTMP.text = SharkGameManager.Instance.CurrentCoins.ToString();
        LoadInitialLevel();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            _gamePausePanel.SetActive(true);
            _gamePauseAnimationPanel.transform.DOScale(Vector3.one, 1f);
            SharkGameManager.Instance.CurrentGameMode = SharkGameDataModel.GameMode.GamePause;
            SoundManager.Instance.PlayGameAudioClip(SharkGameDataModel.Sound.MainThemeSound, true);
        }
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

        _continueButton.SetActive(true);

        _coinSpawnManager.SpawnCoins();

        _inGameCoinsTMP.text = SharkGameManager.Instance.CurrentCoins.ToString();

        StartCoroutine(IncrementNumber(SharkGameManager.Instance.CurrentCoins - _dataLoadManager.GetCoinsAmount(SharkGameManager.Instance.CurrentLevel), SharkGameManager.Instance.CurrentCoins, 1f));

        StartCoroutine(DelayToLoadNextPanel());
    }

    // Coroutine to animate the number
    private IEnumerator IncrementNumber(int startValue, int endValue, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            // Calculate the current value using Lerp
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, elapsedTime / duration));
            // Update the UI Text
            _coinsTMP.text = currentValue.ToString();
            yield return null; // Wait for the next frame
        }

        // Make sure the final value is set
        _coinsTMP.text = endValue.ToString();
    }

    IEnumerator DelayToLoadNextPanel()
    {
        yield return new WaitForSeconds(3f);
        _missionPanel.SetActive(true);
        _huntCompletePanel.SetActive(false);
        _rayImage.transform.DOKill();

        SoundManager.Instance.PlayGameAudioClip(SharkGameDataModel.Sound.MissionPassed, false);

        _currentLevelData = levelConfig.levels[SharkGameManager.Instance.CurrentLevel - 1];
        _levelNumberTMP.text = "Level Number : " + _currentLevelData.levelNumber.ToString();
        _targetDescTMP.text = _currentLevelData.targets[0].description.ToString();
    }

    internal void EnableKillUI()
    {
        SoundManager.Instance.PlayAudioClip(SharkGameDataModel.Sound.MissionPassed);
        _continueButton.SetActive(false);

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
        SoundManager.Instance.PlayGameAudioClip(SharkGameDataModel.Sound.MainThemeSound, true);

        _bonusAmountTMP.text = "+" + " " +_dataLoadManager.GetCoinsAmount(SharkGameManager.Instance.CurrentLevel).ToString();
        _coinsTMP.text = SharkGameManager.Instance.CurrentCoins.ToString();
        SharkGameManager.Instance.CurrentCoins += _dataLoadManager.GetCoinsAmount(SharkGameManager.Instance.CurrentLevel);
        _huntCompletePanel.SetActive(true);

        // Rotate the object from 0 to -360 on the Z-axis and loop back to 0
        _rayImage.transform.DORotate(new Vector3(0, 0, -360), 5f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Yoyo) // Infinite loop between 0 and -360
            .SetEase(Ease.Linear);

        DisableKillUI();
    }

    internal void UpdatePlayerHealth(int _damageAmount)
    {
        if (currentHealth > 0)
        {
            currentHealth -= _damageAmount;
            _healthSlider.value = (float)currentHealth / _playerMaxHealth;
        }
    }

    internal void MakeMaxHealth()
    {
        currentHealth = 100;
        _healthSlider.value = (float)currentHealth / _playerMaxHealth;
        SharkGameManager.Instance.PlayerHealthTimerRemaining = SharkGameManager.Instance._healthDuration;
        SharkGameManager.Instance.StartHealthTimer(SharkGameManager.Instance.PlayerHealthTimerRemaining);
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

    public void QuitButtonClick()
    {
        _gameUIPanel.SetActive(false);
        _subscriptionPage.SetActive(true);
    }

    public void ResumeButtonClick()
    {
        SharkGameManager.Instance.CurrentGameMode = SharkGameDataModel.GameMode.GameStart;
        SharkGameManager.Instance.PlayGameAudio();
        _gamePausePanel.SetActive(false);
        _gameUIPanel.SetActive(true);
    }

    internal void SetGameOver()
    {
        SoundManager.Instance.PlayAudioClip(SharkGameDataModel.Sound.MissionFail);
        _GamePanel.SetActive(false);
        _gameOverPanel.SetActive(true);
        _player.GetComponent<Player>().ShowDieState();
    }
    #endregion

    #region UI Button Actions

    [Header("UI Screens")]
    [SerializeField] private GameObject _selectionPanel;
    [SerializeField] private GameObject _loadingPanel;

    [SerializeField] private GameObject _gameUIPanel;

    [Header("Duplicate Shark")]
    [SerializeField] private GameObject _duplicateShark;
    public void BiteButtonClick()
    {
        _selectionPanel.SetActive(false);
        _duplicateShark.SetActive(false);
        _loadingPanel.SetActive(true);

        SoundManager.Instance.PlayAudioClip(SharkGameDataModel.Sound.Button);

        StartCoroutine(LoadTheGame());

        IEnumerator LoadTheGame()
        {
            yield return new WaitForSeconds(2f);

            _loadingPanel.SetActive(false);
            _gameUIPanel.SetActive(true);
            _gameUIPanel.transform.DOScale(Vector3.one, .5f);
            SoundManager.Instance.PlayGameAudioClip(SharkGameDataModel.Sound.MissionPassed, false);
            _missionPanel.SetActive(true);
        }
    }

    [SerializeField] private GameObject _subscriptionPage;
    [SerializeField] private GameObject _mainMenuPanel;

    public void SubscriptionButtonClick()
    {
        SoundManager.Instance.PlayAudioClip(SharkGameDataModel.Sound.Button);
        _subscriptionPage.SetActive(false);
        _mainMenuPanel.SetActive(true);
    }

    public void PlayButtonClick()
    {
        SoundManager.Instance.PlayAudioClip(SharkGameDataModel.Sound.Button);

        _mainMenuPanel.SetActive(false);
        _duplicateShark.SetActive(true);
        _selectionPanel.SetActive(true);
    }

    #endregion
}
