using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SharkGame.Models;
using SharkGame;

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
    [Header("UI Components")]
    [SerializeField] private Text _levelNumberTMP;
    [SerializeField] private Text _targetDescTMP;

    [Header("Level Panel")]
    [SerializeField] private GameObject _UIPanel;

    [Header("Game UI Panel")]
    [SerializeField] private GameObject _GamePanel;

    [SerializeField] private SharkGameDataModel.LevelConfig levelConfig;

    [Header("Data Load Manager")]
    [SerializeField] private DataLoadManager _dataLoadManager;

    [Header("Player Reference")]
    [SerializeField] private GameObject _player;

    private SharkGameDataModel.Level _currentLevelData;

    [Header("In Game UI")]
    [SerializeField] private Text _killAmountTMP;
    #endregion

    #region MonoBehaviour Methods
    private void Start()
    {
        levelConfig = _dataLoadManager.GetLevelConfig();
        LoadInitialLevel();
    }
    #endregion

    #region Button Actions
    public void LevelButtonClick()
    {
        _UIPanel.SetActive(false);

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
        ObjectPooling.Instance.SetPoolData(_currentLevelData.bufferAmount,_currentLevelData.smallObjects);
    }

    internal int GetTargetAmount(int _level)
    {
        _dataLoadManager.targetAmount = 0;
        return _dataLoadManager.GetTargetAmount(_level);
    }

    internal void LoadNextLevel()
    {
        _UIPanel.SetActive(true);
        _GamePanel.SetActive(false);

        _currentLevelData = levelConfig.levels[SharkGameManager.Instance.CurrentLevel - 1];
        _levelNumberTMP.text = "Level Number : " + _currentLevelData.levelNumber.ToString();
        _targetDescTMP.text = _currentLevelData.targets[0].description.ToString();

        ObjectPooling.Instance.SetPoolData(_currentLevelData.bufferAmount,_currentLevelData.smallObjects);
    }
    #endregion
}
