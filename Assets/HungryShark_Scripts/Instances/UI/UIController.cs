using System.Collections;
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

        DisbaleInGameParticleEffects();
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
    [SerializeField] private Button _huntPanelContinueButton;

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

    [Header("Water Surface")]
    [SerializeField] private GameObject _waterSurface;

    [Header("In Game Particle Effects")]
    [SerializeField] private List<GameObject> _particleEffectLists;

    [Header("Main Camera")]
    [SerializeField] private Transform _mainCamera;

    [Header("SharkSelection Main Camera Position")]
    [SerializeField] private Vector3 _sharkSelectionMainCameraPosition;

    [Header("InGame Camera Position")]
    [SerializeField] private Vector3 _inGameMainCameraPosition;

    [Header("Player Shark Original Position")]
    [SerializeField] private Vector3 _playerSharkOriginalPosition;

    [Header("Camera Follow")]
    [SerializeField] private GameObject _cameraFollow;

    [Header("Duplicate Camera")]
    [SerializeField] private GameObject _duplicateCamera;

    [Header("Running BGs")]
    [SerializeField] private List<GameObject> _runningBgList;

    [Header("Running BG sprites")]
    [SerializeField] private List<Sprite> _runningBgSpriteList;

    [Header("Plane Material")]
    [SerializeField] private GameObject _planeRandomColorChanger;

    [Header("Shark Selection BG")]
    [SerializeField] private GameObject _sharkSelectionBGPlane;

    [Header("Duplicate Sharks")]
    [SerializeField] private List<GameObject> _duplicateSharks;

    [Header("Shark Title Image Reference")]
    [SerializeField] private Image _sharkTitleIM;

    [Header("Shark Titles")]
    [SerializeField] private List<Sprite> _sharkTitles;

    [Header("HealthPanels")]
    [SerializeField] private List<GameObject> _sharkHealthUIPanels;

    [Header("UnderEnvironment Objects Panel")]
    [SerializeField] private GameObject _underWaterEnvironmentPanel;

    [Header("Main Menu Portal Image Reference")]
    [SerializeField] private Image _portalImage;

    [Header("SharkSelection Coins UI")]
    [SerializeField] private Text _sharkSelectionCoinsTMP;

    [SerializeField] private bool mainMenuPanelOpened = false;

    public int CurrentAmmo { get { return currentAmmoValue; } }

    public int CurrentPlayerHealth { get { return currentHealth; } }

    public bool quitButtonClicked;

    [Header("Current Shark Index")]
    [SerializeField] private int currentSharkIndex = 0;

    [Header("Splash Screen Panel")]
    [SerializeField] private GameObject _splashScreen;

    public int CurrentSharkIndex { get { return currentSharkIndex; } }

    [Header("Current Screen")]
    [SerializeField] private SharkGameDataModel.Screen currentScreen;

    public SharkGameDataModel.Screen CurrentScreen { get { return currentScreen; } set { currentScreen = value; } }

    [SerializeField] private SharkGameDataModel.Screen previousScreen = SharkGameDataModel.Screen.None;

    [Header("Current Screen")]
    [SerializeField] private int screenIndexCount = 0;

    [Header("Unlock Full Game Panel")]
    [SerializeField] private GameObject _unlockFullGamePanel;

    private bool isFullGamePurchased = false;

    [Header("IsTutorialEnabled")]
    [SerializeField] private bool isTutorialEnabled = true;

    [Header("TutorialSharkDirection")]
    [SerializeField] private SharkGameDataModel.TutorialSharkDirections tutorialSharkDirection = SharkGameDataModel.TutorialSharkDirections.None;

    public SharkGameDataModel.TutorialSharkDirections TutorialSharkDirection { get { return tutorialSharkDirection;  } }

    [Header("Duplicate Shark InitialPosition")]
    [SerializeField] private float _duplicateSharkXValue;

    [Header("Mute Button")]
    [SerializeField] private Image muteIM;
    [Header("isMuted")]
    [SerializeField] private bool isMuted = false;

    [Header("Mute Sprite")]
    [SerializeField] private Sprite _muteSprite;
    [SerializeField] private Sprite _unMuteSprite;

    public bool IsTutorialEnabled
    {
        get { return isTutorialEnabled; }
    }

    [Header("Tutorial Panel UI sprites References")]
    [SerializeField] private List<SharkGameDataModel.TutorialDirecionSprite> _remoteDirectionImages;
    [SerializeField] private GameObject _remoteImage;
    [SerializeField] private GameObject _tutorialPanel;

    [SerializeField] private GameObject _heathInfo;
    [SerializeField] private GameObject _healthArrow;
    [SerializeField] private GameObject _ammoInfo;
    [SerializeField] private GameObject _ammoArrow;
    [SerializeField] private GameObject _targetInfo;
    [SerializeField] private GameObject _targetArrow;

    [SerializeField] private Animator _targetPanelAnimator;

    #endregion

    #region MonoBehaviour Methods
    private void Start()
    {
        levelConfig = _dataLoadManager.GetLevelConfig();
        currentHealth = _playerMaxHealth;
        currentAmmoValue = _ammoMaxValue;
        _inGameCoinsTMP.text = SharkGameManager.Instance.CurrentCoins.ToString();
        _sharkSelectionCoinsTMP.text = SharkGameManager.Instance.CurrentCoins.ToString();

        LoadInitialLevel();

        currentScreen = SharkGameDataModel.Screen.SplashScreen;

        for (int i = 0; i < _runningBgList.Count; i++)
        {
            _runningBgList[i].GetComponent<SpriteRenderer>().sprite = _runningBgSpriteList[0];
        }

        _planeRandomColorChanger.GetComponent<RandomColorChanger>().UpdatePlaneMaterial();

        StartCoroutine(PlaySharkSoundRepeatedly());

        StartCoroutine(EnableTheSubscriptionPanel());
        isMuted = false;
}

    private void Update()
    {
        if (SharkGameManager.Instance.CurrentGameMode == SharkGameDataModel.GameMode.GameStart)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _gamePausePanel.SetActive(true);
                _gamePauseAnimationPanel.transform.DOScale(Vector3.one, 1f);
                _GamePanel.SetActive(false);
                SharkGameManager.Instance.CurrentGameMode = SharkGameDataModel.GameMode.GamePause;
                SoundManager.Instance.PlayGameAudioClip(SharkGameDataModel.Sound.MainThemeSound, true);
            }
        }
        else if (currentScreen == SharkGameDataModel.Screen.SelectionPanel)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _mainMenuPanel.SetActive(true);
                _selectionPanel.SetActive(false);
                currentSharkIndex = 0;
                foreach (var item in _duplicateSharks)
                {
                    item.SetActive(false);
                    item.transform.position = new Vector3(_duplicateSharkXValue, item.transform.position.y, item.transform.position.z);
                }
                _bitePanel.SetActive(true);
                _purchasePanel.SetActive(false);
                ResetAllSharkHealthUIPanels();
                currentScreen = SharkGameDataModel.Screen.MainMenuScreen;
            }
        }
        else if (currentScreen == SharkGameDataModel.Screen.UnlockFullGamePanel)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _unlockFullGamePanel.SetActive(false);
                _subscriptionPage.SetActive(true);
                currentScreen = SharkGameDataModel.Screen.SubscriptionPanel;
                SharkGameManager.Instance.ResetGame();
                SharkGameManager.Instance.CurrentLevel = PlayerPrefs.GetInt("CurrentLevel");
            }
        }

        else if(currentScreen == SharkGameDataModel.Screen.FivePackSharkPanel)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!quitButtonClicked)
                {
                    if (SharkGameManager.Instance.IsLevelFail)
                    {
                        _5PackSharkUIPanel.SetActive(false);
                        SharkGameManager.Instance.IsLevelFail = false;
                        EnableLoadingScreen();
                    }
                    else
                    {
                        _5PackSharkUIPanel.SetActive(false);
                        _mainMenuPanel.SetActive(true);
                        currentScreen = SharkGameDataModel.Screen.MainMenuScreen;
                    }
                }
                else
                {
                    _5PackSharkUIPanel.SetActive(false);
                    EnableLoadingScreen();
                }
            }
        }


        else if(currentScreen == SharkGameDataModel.Screen.UnlockAllSharksPanel)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _unlockAllSharksPanel.SetActive(false);
                EnableLoadingScreen();
            }
        }

    }
    #endregion

    private IEnumerator PlaySharkSoundRepeatedly()
    {
        while (true)
        {
            if (SharkGameManager.Instance.CurrentGameMode == SharkGameDataModel.GameMode.GameStart)
            {
                SoundManager.Instance.PlaySharkAudioClip(SharkGameDataModel.Sound.SharkSound);

                yield return new WaitForSeconds(15f);
            }
            else
            {
                // If not in GameStart mode, pause and check again after a short delay
                yield return null;
            }
        }
    }

    private IEnumerator EnableTheSubscriptionPanel()
    {
        yield return new WaitForSeconds(2f);
        _subscriptionPage.SetActive(true);
        _splashScreen.SetActive(false);
        currentScreen = SharkGameDataModel.Screen.SubscriptionPanel;
    }

    #region Button Actions
    public void LevelButtonClick()
    {
        SoundManager.Instance.PlayAudioClip(SharkGameDataModel.Sound.Button);

        MakeMaxAmmo();
        MakeMaxHealth();

#if UNITY_EDITOR
        Debug.LogError("LevelButtonClick");
#endif
        StartCoroutine(GiveSlightDelay());
    }

    private IEnumerator GiveSlightDelay()
    {
        _waterSurface.SetActive(true);
        _player.SetActive(true);
        EnableInGameParticleEffects();
        SetMainCameraOriginalPosition();

        yield return new WaitForSeconds(.5f);

        _missionPanel.SetActive(false);

        currentScreen = SharkGameDataModel.Screen.InGamePanel;

        _killAmountTMP.text = "0 /" + _currentLevelData.targets[0].amount.ToString();

        _GamePanel.SetActive(true);
        _inGameLevelNumberTMP.text = "Level :" + " " + SharkGameManager.Instance.CurrentLevel.ToString();
        SharkGameManager.Instance.CurrentGameMode = SharkGameDataModel.GameMode.GameStart;
        SharkGameManager.Instance.PlayGameAudio();
        SharkGameManager.Instance.InitializePlayer();
    }
    GameObject _directionImage;
    GameObject _directionInfo;
    public void EnableTutorial()
    {
         _tutorialPanel.SetActive(true);
            _remoteImage.transform.DOScale(Vector3.one, 1f);
            isTutorialEnabled = true;
            SharkGameManager.Instance.CurrentGameMode = SharkGameDataModel.GameMode.Tutorial;
            tutorialSharkDirection = SharkGameDataModel.TutorialSharkDirections.Down;
            _directionImage = _remoteDirectionImages.Find(x => x._direction == tutorialSharkDirection)._directionImage;
            _directionImage.SetActive(true);
            _directionInfo = _remoteDirectionImages.Find(x => x._direction == tutorialSharkDirection)._directionInfo;
            _directionInfo.SetActive(true);
            _directionImage.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 1f).SetLoops(-1);
       
    }

    public void ChangeDirection(SharkGameDataModel.TutorialSharkDirections _nextDirection)
    {
        _remoteImage.SetActive(true);
        _remoteImage.transform.DOScale(Vector3.one, 1f);
        tutorialSharkDirection = _nextDirection;
        _directionImage = _remoteDirectionImages.Find(x => x._direction == _nextDirection)._directionImage;
        _directionImage.SetActive(true);
        _directionInfo = _remoteDirectionImages.Find(x => x._direction == _nextDirection)._directionInfo;
        _directionInfo.SetActive(true);
        _directionImage.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 1f).SetLoops(-1);
    }

    public void TutorialInputPressed()
    {
        _remoteImage.SetActive(false);
        _directionImage.transform.DOKill();
        _directionImage.SetActive(false);
        _directionInfo.SetActive(false);
    }

    public void EndDirectionTutorial()
    {
        SharkGameManager.Instance.CurrentGameMode = SharkGameDataModel.GameMode.GameHold;
        StartCoroutine(AnimateTutorialUI());
    }
    IEnumerator AnimateTutorialUI()
    {
        yield return new WaitForSeconds(.5f);
        _targetPanelAnimator.SetBool("ammo", true);
        yield return new WaitForSeconds(28f);
        _tutorialPanel.SetActive(false);
        isTutorialEnabled = false;
        SharkGameManager.Instance.CurrentGameMode = SharkGameDataModel.GameMode.GameStart;
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

        LoadLevelData();

        Debug.Log("pOOL QUANTITY" + _currentLevelData.smallObjects.Capacity);
    }

    internal void LoadLevelData()
    {
        _currentLevelData = levelConfig.levels[SharkGameManager.Instance.CurrentLevel - 1];
        _levelNumberTMP.text = "Level Number : " + _currentLevelData.levelNumber.ToString();
        _targetDescTMP.text = _currentLevelData.targets[0].description.ToString();

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
        _huntPanelContinueButton.interactable = false;

        SoundManager.Instance.PlayAudioClip(SharkGameDataModel.Sound.Button);
        _sharkSelectionCoinsTMP.text = SharkGameManager.Instance.CurrentCoins.ToString();

        _continueButton.SetActive(true);

        _huntCompletePanel.SetActive(false);
        _rayImage.transform.DOKill();

        if (SharkGameManager.Instance.CurrentLevel != 5)
        {
            _missionPanel.SetActive(true);

            currentScreen = SharkGameDataModel.Screen.MissionPanel;

            SoundManager.Instance.PlayGameAudioClip(SharkGameDataModel.Sound.MissionPassed, false);

            _currentLevelData = levelConfig.levels[SharkGameManager.Instance.CurrentLevel - 1];
            _levelNumberTMP.text = "Level Number : " + _currentLevelData.levelNumber.ToString();
            _targetDescTMP.text = _currentLevelData.targets[0].description.ToString();

            for (int i = 0; i < _runningBgList.Count; i++)
            {
                _runningBgList[i].GetComponent<SpriteRenderer>().sprite = _runningBgSpriteList[SharkGameManager.Instance.CurrentLevel % _runningBgSpriteList.Count];
            }
            _planeRandomColorChanger.GetComponent<RandomColorChanger>().UpdatePlaneMaterial();
        }

        if(SharkGameManager.Instance.CurrentLevel == 5)
        {
            if(!isFullGamePurchased)
            {
                _unlockFullGamePanel.SetActive(true);
                CurrentScreen = SharkGameDataModel.Screen.UnlockFullGamePanel;
            }
            else if(isFullGamePurchased)
            {
                _missionPanel.SetActive(true);

                currentScreen = SharkGameDataModel.Screen.MissionPanel;

                SoundManager.Instance.PlayGameAudioClip(SharkGameDataModel.Sound.MissionPassed, false);

                _currentLevelData = levelConfig.levels[SharkGameManager.Instance.CurrentLevel - 1];
                _levelNumberTMP.text = "Level Number : " + _currentLevelData.levelNumber.ToString();
                _targetDescTMP.text = _currentLevelData.targets[0].description.ToString();

                for (int i = 0; i < _runningBgList.Count; i++)
                {
                    _runningBgList[i].GetComponent<SpriteRenderer>().sprite = _runningBgSpriteList[SharkGameManager.Instance.CurrentLevel % _runningBgSpriteList.Count];
                }
                _planeRandomColorChanger.GetComponent<RandomColorChanger>().UpdatePlaneMaterial();
            }
          
        }

        MakeMaxHealth();
        MakeMaxAmmo();
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

    internal void EnableKillUI()
    {
        SoundManager.Instance.PlayAudioClip(SharkGameDataModel.Sound.MissionPassed);
        _continueButton.SetActive(false);

        _missionPanel.SetActive(true);
        _GamePanel.SetActive(false);
        currentScreen = SharkGameDataModel.Screen.MissionPanel;


        _targetDescTMP.text = _currentLevelData.targets[0].targetDescription.ToString();

        StartCoroutine(WaitForTheAnimation());

        IEnumerator WaitForTheAnimation()
        {
            yield return new WaitForSeconds(1f);
            _killImage.SetActive(true);
            _killImage.transform.DOScale(Vector3.one, 2f);
        }
       
    }

    internal void DisableKillUI()
    {
        _killImage.SetActive(false);
        _killImage.transform.DOScale(Vector3.zero, .2f);
    }

    internal void EnableHuntCompleteScreen()
    {
        _huntPanelContinueButton.interactable = true;
        _missionPanel.SetActive(false);
        currentScreen = SharkGameDataModel.Screen.HuntCompletePanel;

        SoundManager.Instance.PlayGameAudioClip(SharkGameDataModel.Sound.MainThemeSound, true);

        _bonusAmountTMP.text = "+" + " " +_dataLoadManager.GetCoinsAmount(SharkGameManager.Instance.CurrentLevel).ToString();
        _coinsTMP.text = SharkGameManager.Instance.CurrentCoins.ToString();
        SharkGameManager.Instance.CurrentCoins += _dataLoadManager.GetCoinsAmount(SharkGameManager.Instance.CurrentLevel);

        PlayerPrefs.SetInt("CurrentCoins", SharkGameManager.Instance.CurrentCoins);
        PlayerPrefs.Save();

        _huntCompletePanel.SetActive(true);

        // Rotate the object from 0 to -360 on the Z-axis and loop back to 0
        _rayImage.transform.DORotate(new Vector3(0, 0, -360), 20f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Yoyo) // Infinite loop between 0 and -360
            .SetEase(Ease.Linear);

        StartCoroutine(StartSpawnCoins());
       
        DisableKillUI();
    }

    IEnumerator StartSpawnCoins()
    {
        yield return new WaitForSeconds(.25f);

        _coinSpawnManager.SpawnCoins();

        SoundManager.Instance.PlayAudioClip(SharkGameDataModel.Sound.CoinsCollect);

        _inGameCoinsTMP.text = SharkGameManager.Instance.CurrentCoins.ToString();

        yield return new WaitForSeconds(.25f);

        StartCoroutine(IncrementNumber(SharkGameManager.Instance.CurrentCoins - _dataLoadManager.GetCoinsAmount(SharkGameManager.Instance.CurrentLevel), SharkGameManager.Instance.CurrentCoins, 1f));
    }

    internal void UpdatePlayerHealth(int _damageAmount)
    {
        if (SharkGameManager.Instance.CurrentGameMode == SharkGameDataModel.GameMode.GameStart)
        {
            if (currentHealth > 0)
            {
                currentHealth -= _damageAmount;
                _healthSlider.value = (float)currentHealth / _playerMaxHealth;
            }

            if (currentHealth == 0)
            {
                SharkGameManager.Instance.SetGameOver();
            }
        }
    }
    internal void MakeMaxHealth()
    {
        currentHealth = 100;
        _healthSlider.value = (float)currentHealth / _playerMaxHealth;
        SharkGameManager.Instance.PlayerHealthTimerRemaining = SharkGameManager.Instance._healthDuration;
        SharkGameManager.Instance.StartHealthTimer(SharkGameManager.Instance.PlayerHealthTimerRemaining);
    }

    internal void MakeMaxAmmo()
    {
        currentAmmoValue = 100;
        _ammoSlider.value = (float)currentAmmoValue / _ammoMaxValue;
    }

    internal void UpdateAmmoHealth(int _damageAmount)
    {
        if (SharkGameManager.Instance.CurrentGameMode == SharkGameDataModel.GameMode.GameStart)
        {
            if (currentAmmoValue > 0)
            {
                currentAmmoValue -= _damageAmount;

                _ammoSlider.value = (float)currentAmmoValue / _ammoMaxValue;
            }
            if (currentAmmoValue == 0)
            {
                SharkGameManager.Instance.SetGameOver();
            }
        }
    }

    public void QuitButtonClick()
    {
        _player.SetActive(false);
        _gamePausePanel.SetActive(false);
        _subscriptionPage.SetActive(true);
        DestroyBombs();
        SharkGameManager.Instance.CurrentGameMode = SharkGameDataModel.GameMode.MissionMode;
        _player.transform.position = _playerSharkOriginalPosition;
        _player.transform.rotation = Quaternion.Euler(0, 0, 0);
        _mainCamera.gameObject.GetComponent<CameraFollow>().smoothSpeed = 0.075f;
        quitButtonClicked = true;

        _player.GetComponent<Player>().ResetPlayer();
        ResetGameData();
    }

    private void ResetGameData()
    {
        _killAmountTMP.text = "0 /" + "0";
        _inGameCoinsTMP.text = SharkGameManager.Instance.CurrentCoins.ToString();

        SharkGameManager.Instance.ResetGame();
    }

    public void ResumeButtonClick()
    {
        SharkGameManager.Instance.CurrentGameMode = SharkGameDataModel.GameMode.GameStart;
        SharkGameManager.Instance.PlayGameAudio();
        _inGameLevelNumberTMP.text = "Level :" + " " + SharkGameManager.Instance.CurrentLevel.ToString();
        _gamePausePanel.SetActive(false);
        _GamePanel.SetActive(true);
    }

    internal void SetGameOver()
    {
        SoundManager.Instance.PlayAudioClip(SharkGameDataModel.Sound.MissionFail);
        _GamePanel.SetActive(false);
        _gameOverPanel.SetActive(true);
        _player.GetComponent<Player>().ShowDieState();
    }

    private void EnableInGameParticleEffects()
    {
        foreach(var item in _particleEffectLists)
        {
            item.SetActive(true);
        }
    }

    private void DisbaleInGameParticleEffects()
    {
        foreach (var item in _particleEffectLists)
        {
            item.SetActive(false);
        }
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
        SharkGameManager.Instance.SelectedPlayer(currentSharkIndex);

        _bitePanel.SetActive(true);
        _purchasePanel.SetActive(false);
        ResetAllSharkHealthUIPanels();
        _selectionPanel.SetActive(false);
        //if(!quitButtonClicked) _loadingPanel.SetActive(true);
        _sharkSelectionBGPlane.SetActive(false);
        _underWaterEnvironmentPanel.SetActive(true);

        Navigation _LeftButton = leftButton.navigation;
        _LeftButton.selectOnDown = _biteButton;
        leftButton.navigation = _LeftButton;

        Navigation _Right = rightButton.navigation;
        _Right.selectOnDown = _biteButton;
        rightButton.navigation = _Right;

      

        currentScreen = SharkGameDataModel.Screen.LoadingPanel;

        foreach (var item in _duplicateSharks)
        {
            item.SetActive(false);
            item.transform.position = new Vector3(_duplicateSharkXValue, item.transform.position.y, item.transform.position.z);
        }

        SoundManager.Instance.PlayAudioClip(SharkGameDataModel.Sound.Button);

        if (previousScreen == SharkGameDataModel.Screen.FivePackSharkPanel)
        {
            EnableUnLockAllSharksPanel();
            previousScreen = SharkGameDataModel.Screen.None;
        }
        else if (previousScreen == SharkGameDataModel.Screen.None)
        {
            if (screenIndexCount % 2 == 0)
            {
                EnableUnLockAllSharksPanel();
            }
            else if(screenIndexCount % 2 == 1)
            {
                _5PackSharkUIPanel.SetActive(true);
                _selectionPanel.SetActive(false);
                currentSharkIndex = 0;
                currentScreen = SharkGameDataModel.Screen.FivePackSharkPanel;
            }
            screenIndexCount++;
        }
        currentSharkIndex = 0;
        _sharkHealthUIPanels[currentSharkIndex].SetActive(true);

    }
    [Header("UI Panels")]
    [SerializeField] private GameObject _subscriptionPage;
    [SerializeField] private GameObject _mainMenuPanel;

    public void SubscriptionButtonClick()
    {
        SoundManager.Instance.PlayAudioClip(SharkGameDataModel.Sound.Button);
        _subscriptionPage.SetActive(false);
        _mainMenuPanel.SetActive(true);
        currentScreen = SharkGameDataModel.Screen.MainMenuScreen;
    }

    public void PlayButtonClick()
    {
        SoundManager.Instance.PlayAudioClip(SharkGameDataModel.Sound.Button);
        mainMenuPanelOpened = true;
        _mainMenuPanel.SetActive(false);
        DisbaleInGameParticleEffects();
        _selectionPanel.SetActive(true);
        _sharkSelectionBGPlane.SetActive(true);
        _underWaterEnvironmentPanel.SetActive(false);
        _duplicateSharks[currentSharkIndex].SetActive(true);
        _duplicateCamera.SetActive(true);
        _sharkTitleIM.sprite = _sharkTitles[currentSharkIndex];
        _portalImage.transform.DOKill();
        currentScreen = SharkGameDataModel.Screen.SelectionPanel;
        _sharkHealthUIPanels[currentSharkIndex].SetActive(true);
    }

    public void SubscriptionContinueButtonClick()
    {
        SoundManager.Instance.PlayAudioClip(SharkGameDataModel.Sound.Button);
        _subscriptionPage.SetActive(false);

        if(mainMenuPanelOpened)
        {
            _sharkSelectionBGPlane.SetActive(true);
            _selectionPanel.SetActive(true);
            currentScreen = SharkGameDataModel.Screen.SelectionPanel;
            _duplicateSharks[currentSharkIndex].SetActive(true);
            ResetAllSharkHealthUIPanels();
            _sharkHealthUIPanels[currentSharkIndex].SetActive(true);
            _duplicateCamera.SetActive(true);
            _underWaterEnvironmentPanel.SetActive(false);
            DisbaleInGameParticleEffects();
        }
        else if(!mainMenuPanelOpened)
        {
            _mainMenuPanel.SetActive(true);
            _sharkSelectionBGPlane.SetActive(false);
            _underWaterEnvironmentPanel.SetActive(false);
            currentScreen = SharkGameDataModel.Screen.MainMenuScreen;
        }
    }

    public void SetMainCameraOriginalPosition()
    {
        _duplicateCamera.SetActive(false);
    }

    public void CameraFollowCallHandleMode()
    {
        _cameraFollow.GetComponent<CameraFollow>().HandleGameMode(SharkGameManager.Instance.CurrentGameMode);
    }

    [Header("Selection Panels Right/Left Buttons")]
    [SerializeField] private Button rightButton;
    [SerializeField] private Button leftButton;

    [Header("Selection Bite Panel")]
    [SerializeField] private GameObject _bitePanel;
    [SerializeField] private Button _biteButton;
    [Header("Selection Purchase Panel")]
    [SerializeField] private GameObject _purchasePanel;
    [SerializeField] private Button _purchaseButton;

    [Header("Unlock All Sharks")]
    [SerializeField] private GameObject _unlockAllSharksPanel;

    [Header("InGame Level Number Reference")]
    [SerializeField] private Text _inGameLevelNumberTMP;

    [Header("5Pack Shark Panel")]
    [SerializeField] private GameObject _5PackSharkUIPanel;

    public void RightArrowClick()
    {
        if (currentSharkIndex < _duplicateSharks.Capacity -1)
        {
            Debug.LogError("RightArrowClick");
            _duplicateSharks[currentSharkIndex].SetActive(false);
            _duplicateSharks[currentSharkIndex].transform.position = new Vector3(_duplicateSharkXValue, _duplicateSharks[currentSharkIndex].transform.position.y, _duplicateSharks[currentSharkIndex].transform.position.z);
            currentSharkIndex += 1;
            _duplicateSharks[currentSharkIndex].SetActive(true);
            _sharkTitleIM.sprite = _sharkTitles[currentSharkIndex];
            ResetAllSharkHealthUIPanels();
            _sharkHealthUIPanels[currentSharkIndex].SetActive(true);
            leftButton.interactable = true;
            _purchasePanel.SetActive(true);
            _bitePanel.SetActive(false);


            Navigation _LeftButton = leftButton.navigation;
            _LeftButton.selectOnDown = _purchaseButton;
            leftButton.navigation = _LeftButton;

            Navigation _Right = rightButton.navigation;
            _Right.selectOnDown = _purchaseButton;
            rightButton.navigation = _Right;

        }
    }

    public void LeftArrowClick()
    {
        if (currentSharkIndex > 0)
        {
            rightButton.interactable = true;
            _duplicateSharks[currentSharkIndex].SetActive(false);
            _duplicateSharks[currentSharkIndex].transform.position = new Vector3(_duplicateSharkXValue, _duplicateSharks[currentSharkIndex].transform.position.y, _duplicateSharks[currentSharkIndex].transform.position.z);
            currentSharkIndex -= 1;
            ResetAllSharkHealthUIPanels();
            _sharkHealthUIPanels[currentSharkIndex].SetActive(true);
            _duplicateSharks[currentSharkIndex].SetActive(true);
            _sharkTitleIM.sprite = _sharkTitles[currentSharkIndex];
            _purchasePanel.SetActive(true);
            _bitePanel.SetActive(false);

            Navigation _LeftButton = leftButton.navigation;
            _LeftButton.selectOnDown = _purchaseButton;
            leftButton.navigation = _LeftButton;

            Navigation _Right = rightButton.navigation;
            _Right.selectOnDown = _purchaseButton;
            rightButton.navigation = _Right;

        }
        if (currentSharkIndex == 0)
        {
            _bitePanel.SetActive(true);
            _purchasePanel.SetActive(false);

            Navigation _LeftButton = leftButton.navigation;
            _LeftButton.selectOnDown = _biteButton;
            leftButton.navigation = _LeftButton;

            Navigation _Right = rightButton.navigation;
            _Right.selectOnDown = _biteButton;
            rightButton.navigation = _Right;

        }
    }

    private void ResetAllSharkHealthUIPanels()
    {
        foreach(var item in _sharkHealthUIPanels)
        {
            item.SetActive(false);
        }
    }
    public void LevelFailContinueButtonClick()
    {
        SharkGameManager.Instance.CurrentGameMode = SharkGameDataModel.GameMode.MissionMode;

        SoundManager.Instance.PlayGameAudioClip(SharkGameDataModel.Sound.MainThemeSound, true);

        _gameOverPanel.SetActive(false);
        DisbaleInGameParticleEffects();
        _selectionPanel.SetActive(true);
        _sharkSelectionBGPlane.SetActive(true);
        _duplicateSharks[currentSharkIndex].SetActive(true);
        _sharkHealthUIPanels[currentSharkIndex].SetActive(true);

        _player.GetComponent<Player>().ResetSharkIdleState();
       
        _underWaterEnvironmentPanel.SetActive(false);
        _duplicateCamera.SetActive(true);

        SharkGameManager.Instance.ResetPlayerAndObjectPooling();
    }

    public void FivePackSharkButtonClick()
    {
        _mainMenuPanel.SetActive(false);
        _5PackSharkUIPanel.SetActive(true);
        mainMenuPanelOpened = true;
        currentScreen = SharkGameDataModel.Screen.FivePackSharkPanel;
        previousScreen = SharkGameDataModel.Screen.FivePackSharkPanel;
    }

    public void FivePackSharkContinueButtonClick()
    {
        _5PackSharkUIPanel.SetActive(false);
        if (previousScreen == SharkGameDataModel.Screen.FivePackSharkPanel)
        {
            _selectionPanel.SetActive(true);
            currentScreen = SharkGameDataModel.Screen.SelectionPanel;
            _duplicateCamera.SetActive(true);
            _sharkSelectionBGPlane.SetActive(true);
            _duplicateSharks[CurrentSharkIndex].SetActive(true);
            ResetAllSharkHealthUIPanels();
            _sharkHealthUIPanels[currentSharkIndex].SetActive(true);
        }
        else if (previousScreen == SharkGameDataModel.Screen.None)
        {
            _loadingPanel.SetActive(true);
            StartCoroutine(LoadTheGame());

            IEnumerator LoadTheGame()
            {
                yield return new WaitForSeconds(2f);

                _loadingPanel.SetActive(false);
                _gameUIPanel.SetActive(true);
                _gameUIPanel.transform.DOScale(Vector3.one, .5f);
                SoundManager.Instance.PlayGameAudioClip(SharkGameDataModel.Sound.MissionPassed, false);
                LoadLevelData();
                _missionPanel.SetActive(true);
                currentScreen = SharkGameDataModel.Screen.MissionPanel;
            }
        }
    }

    public void UnlockFullGameContinueButtonClick()
    {
        _unlockFullGamePanel.SetActive(false);
        _subscriptionPage.SetActive(true);
        CurrentScreen = SharkGameDataModel.Screen.SubscriptionPanel;
        SharkGameManager.Instance.ResetGame();
        SharkGameManager.Instance.CurrentLevel = PlayerPrefs.GetInt("CurrentLevel");
        isFullGamePurchased = true;
    }

    public void EnableUnLockAllSharksPanel()
    {
        _unlockAllSharksPanel.SetActive(true);
        currentScreen = SharkGameDataModel.Screen.UnlockAllSharksPanel;
    }

    public void EnableLoadingScreen()
    {
        _loadingPanel.SetActive(true);
        currentScreen = SharkGameDataModel.Screen.LoadingPanel;

        StartCoroutine(LoadTheGame());

        IEnumerator LoadTheGame()
        {
            yield return new WaitForSeconds(2f);

            _loadingPanel.SetActive(false);
            _gameUIPanel.SetActive(true);
            _gameUIPanel.transform.DOScale(Vector3.one, .5f);
            SoundManager.Instance.PlayGameAudioClip(SharkGameDataModel.Sound.MissionPassed, false);
            LoadLevelData();
            _missionPanel.SetActive(true);
            currentScreen = SharkGameDataModel.Screen.MissionPanel;
        }
    }

    public void UnlockAllSharksContinueButtonClick()
    {
        _unlockAllSharksPanel.SetActive(false);

        _loadingPanel.SetActive(true);
        StartCoroutine(LoadTheGame());

        IEnumerator LoadTheGame()
        {
            yield return new WaitForSeconds(2f);

            _loadingPanel.SetActive(false);
            _gameUIPanel.SetActive(true);
            _gameUIPanel.transform.DOScale(Vector3.one, .5f);
            SoundManager.Instance.PlayGameAudioClip(SharkGameDataModel.Sound.MissionPassed, false);
            LoadLevelData();
            _missionPanel.SetActive(true);
            currentScreen = SharkGameDataModel.Screen.MissionPanel;
        }
    }

    internal void DestroyBombs()
    {
        foreach (var item in SpawnManager.Instance.BombSpawnPoints)
        {
            if (item.childCount > 0)
            {
                GameObject obj = item.GetChild(0).gameObject;
                Debug.LogError("Object Name " + obj.name);
                Destroy(obj);
            }
        }
    }

    public void SetPlayer(GameObject _playerObject)
    {
        _player = _playerObject;
    }

    public void MuteButtonClick()
    {
        if(isMuted)
        {
            muteIM.sprite = _unMuteSprite;
        }
        else
        {
            muteIM.sprite = _muteSprite;
        }
        isMuted = !isMuted;
        SoundManager.Instance.MuteSounds(isMuted);
    }
  
    #endregion
}
