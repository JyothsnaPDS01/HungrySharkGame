using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SharkGame
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "ScriptableObjects / LevelConfig", order = 1)]
    public class LevelConfig : ScriptableObject
    {
        public int levelNumber;
        public TargetConfig[] targets;  // Array to hold multiple targets
        public SmallObject[] smallObjects;
        public Enemy[] enemies;
    }

    [System.Serializable]
    public class TargetConfig
    {
        public string description;
        public string targetType;  // Example: "smallFishes" or "multiple"
        public int amount;
        public TargetRequirement[] requirements;  // For multi-target requirements
        public float timeLimit;  // Optional: For time-based challenges
    }

    [System.Serializable]
    public class TargetRequirement
    {
        public string type;
        public int count;
    }

    [System.Serializable]
    public class SmallObject
    {
        public string name;
        public float size;
        public int quantity;
    }

    [System.Serializable]
    public class Enemy
    {
        public string name;
        public float size;
        public int quantity;
    }


}
