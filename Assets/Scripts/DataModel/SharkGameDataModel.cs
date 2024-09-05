using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharkGame;

namespace SharkGame.Models
{
    public static class SharkGameDataModel
    {
        [System.Serializable]
        public enum SharkDirection
        {
            None,
            Left,
            Right,
            Up,
            Down,
            UpRight,
            UpLeft,
            DownRight,
            DownLeft
        }

        [System.Serializable]
        public enum SmallFishType
        {
            GoldenSmallFish,
            Chaetodon_Collare,
            Chelmon_Rostratus,
            CoralBeauty
        }

        [System.Serializable]
        public class FishPool
        {
            public SmallFishType _smallFishType;
            public int _capacity;
            public GameObject _fishObject;
        }
    }
}
