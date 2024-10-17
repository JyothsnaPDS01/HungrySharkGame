using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharkGame.Models;

namespace SharkGame
{
    public class BackToPool : MonoBehaviour
    {
        #region SmallFishType
        [SerializeField] private SharkGameDataModel.SmallFishType _smallFishType;
        #endregion

        #region Public Methods
        internal void PushBackToPool()
        {
            ObjectPooling.Instance.ReturnToPool(this.gameObject, _smallFishType);
        }
        #endregion
    }
}