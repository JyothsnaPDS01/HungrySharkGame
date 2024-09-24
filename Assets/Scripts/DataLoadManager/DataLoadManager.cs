using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharkGame.Models;
using SharkGame;

public class DataLoadManager : MonoBehaviour
{
    [SerializeField] private SharkGameDataModel.LevelConfig _levelConfig;

    private void OnValidate()
    {
        // Load the JSON file from Resources
        TextAsset levelTextFile = Resources.Load<TextAsset>("levelData");
        if (levelTextFile != null)
        {
            string levelJsonFile = levelTextFile.text;

            // Deserialize the JSON string to LevelConfig
            if (!string.IsNullOrEmpty(levelJsonFile))
            {
                _levelConfig = JsonUtility.FromJson<SharkGameDataModel.LevelConfig>(levelJsonFile);
                SharkGameManager.Instance.CurrentGameMode = SharkGameDataModel.GameMode.None;

#if UNITY_EDIOR
                Debug.Log("Level Config loaded successfully");
#endif
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log("Unable to load level data: JSON is empty.");
#endif
            }
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log("Unable to load level data: File not found.");
#endif
        }
    }

    // Public method to access the loaded level config
    public SharkGameDataModel.LevelConfig GetLevelConfig()
    {
        return _levelConfig;
    }
}
