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

    private SharkGameDataModel.Level _currentLevelData;
    private int _currentLevel;
    #endregion

    #region MonoBehaviour Methods
    private void Start()
    {
        levelConfig = _dataLoadManager.GetLevelConfig();
        LoadLevel();
    }
    #endregion

    #region Button Actions
    public void LevelButtonClick()
    {
        _UIPanel.SetActive(true);
        SharkGameManager.Instance.CurrentGameMode = SharkGameDataModel.GameMode.GameStart;
    }
    #endregion

    #region Private Methods
    private void LoadLevel()
    {
        SharkGameManager.Instance.CurrentGameMode = SharkGameDataModel.GameMode.MissionMode;

        PlayerPrefs.SetInt("CurrentLevel", 1);

        _currentLevel = PlayerPrefs.GetInt("CurrentLevel");

        _currentLevelData = levelConfig.levels[_currentLevel - 1];
        _levelNumberTMP.text = _currentLevelData.levelNumber.ToString();
        _targetDescTMP.text = _currentLevelData.targets[0].description.ToString();

        Debug.Log("pOOL QUANTITY" + _currentLevelData.smallObjects.Capacity);
    }
    #endregion
}
