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
            None,
            GoldenSmallFish,
            Chaetodon_Collare,
            Chelmon_Rostratus,
            CoralBeauty,
            ZebraSoma,
            Lyretail_Anthias,
            Mandarin_Fish,
            ZebraSoma_2,
            Cown_TriggerFish
        }

        #region PoolClass
        [System.Serializable]
        public class FishPool
        {
            public SmallFishType _smallFishType;
            public int _capacity;
            public GameObject _fishObject;
        }
        #endregion

        [System.Serializable]
        public class SmallFishes
        {
            public SmallFishType _smallFishType;
            public GameObject _fishObject;
        }

        #region SharkTypes
        [System.Serializable]
        public enum SharkType
        {
            GeneralShark,
            LemonShark,
            LeopardShark,
            SandShark,
            WhaleShark,
            TigerShark
        }
        #endregion


        #region Level Config
        [System.Serializable]
        public class Level
        {
            public int levelNumber;
            public List<Target> targets ;
            public List<SmallObject> smallObjects ;
            public List<Bomb> enemies ;
            public int bufferAmount;
            public RewardCoins rewardCoins;
        }
        [System.Serializable]
        public class LevelConfig
        {
            public List<Level> levels ;
        }
        [System.Serializable]
        public class SmallObject
        {
            public string name ;
            public double size ;
            public int quantity ;
        }
        [System.Serializable]
        public class Target
        {
            public string description ;
            public string targetType ;
            public int amount ;
        }

        [System.Serializable]
        public class Bomb
        {
            public string bomb;
        }

        [System.Serializable]
        public class RewardCoins
        {
            public string description;
            public int amount;
        }
        #endregion

        #region Game Modes
        [System.Serializable]
        public enum GameMode
        {
            None,
            GamePause,
            GameOver,
            GameStart,
            MissionMode,
        }
        #endregion

        #region Sounds
        [System.Serializable]
        public enum Sound
        {
            EatingShark,
            SharkMovement,
            WaterSplash,
            MissionPassed,
            MissionFail,
            Button,
            SharkSound,
            BombSound,
            MainThemeSound,
            UnderWaterSound
        }
        [System.Serializable]
        public class SoundModel
        {
            public Sound _soundType;
            public AudioClip _audioClip;
        }
        #endregion

        #region Bomb Types
        [System.Serializable]
        public enum BombType
        {
            None,
            RedBomb,
            TimerBomb,
            SharpBomb
        }

        [System.Serializable]
        public class BombObject
        {
            public Transform bombPosition;
            public BombType _bombType;
            public GameObject _bombObject;
        }
        #endregion
    }
}
