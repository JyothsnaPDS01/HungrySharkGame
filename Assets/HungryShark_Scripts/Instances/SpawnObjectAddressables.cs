using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SharkGame
{
    public class SpawnObjectAddressables : MonoBehaviour
    {
        #region MonoBehaviour Methods

        private void Start()
        {
            Addressables.LoadAssetAsync<GameObject>("Assets/Addressables_Unity/Addressables_Prefabs/UnderWaterEnvironment.prefab").Completed +=
                (asyncOperationHandle) =>
                {
                    if (asyncOperationHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        //Success to load the object
                        Instantiate(asyncOperationHandle.Result);
                    }
                    else
                    {
                        //Failed to load the object
                        Debug.Log("Failed to load the object");
                    }
                };
               
        }

        #endregion
    }
}
