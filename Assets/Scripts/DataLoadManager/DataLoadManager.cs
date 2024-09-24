using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharkGame.Models;
using SharkGame;

public class DataLoadManager : MonoBehaviour
{
    [SerializeField] private SharkGameDataModel.LevelConfig _levelConfig;

    [SerializeField] private GameObject _fishObject;

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

#if UNITY_EDIOR
                Debug.Log("Level Config loaded successfully");
#endif
                // Set the pool data after loading the level config
                if (_levelConfig != null && _levelConfig.levels.Count > 0)
                {
                    SharkGameManager.Instance.CurrentGameMode = SharkGameDataModel.GameMode.None;
                    ObjectPooling.Instance.SetPoolData(_levelConfig.levels[0].smallObjects.Capacity, _levelConfig.levels[0].smallObjects[0].quantity, _levelConfig.levels[0].smallObjects[0].name, _fishObject);
                }
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
