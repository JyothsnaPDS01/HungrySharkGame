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

    [SerializeField] private GameObject _UIPanel;
    [SerializeField] private SharkGameDataModel.LevelConfig levelConfig;

    [Header("Data Load Manager")]
    [SerializeField] private DataLoadManager _dataLoadManager;

    [Header("Player Reference")]
    [SerializeField] private GameObject _player;

    [SerializeField] private GameObject _fishObject;

    private SharkGameDataModel.Level _currentLevelData;
  
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
        SharkGameManager.Instance.CurrentGameMode = SharkGameDataModel.GameMode.GameStart;
        SharkGameManager.Instance.PlayGameAudio();
        SharkGameManager.Instance.InitializePlayer();
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

        ObjectPooling.Instance.SetPoolData(levelConfig.levels[0].smallObjects.Capacity, levelConfig.levels[0].smallObjects[0].quantity + levelConfig.levels[0].bufferAmount, levelConfig.levels[0].smallObjects[0].name, _fishObject);
    }

    internal int GetTargetAmount(int _level)
    {
        return _dataLoadManager.GetTargetAmount(_level);
    }

    internal void LoadNextLevel()
    {
        _UIPanel.SetActive(true);

        _currentLevelData = levelConfig.levels[SharkGameManager.Instance.CurrentLevel - 1];
        _levelNumberTMP.text = "Level Number : " + _currentLevelData.levelNumber.ToString();
        _targetDescTMP.text = _currentLevelData.targets[0].description.ToString();

    }
    #endregion
}
