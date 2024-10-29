using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

namespace SharkGame
{
    public class SpawnObjectAddressables : MonoBehaviour
    {
        [SerializeField] private AssetReference assetReference;
        [SerializeField] private AssetLabelReference assetLabelReference;

        private GameObject _underwaterObject;

        public event Action<GameObject> OnUnderWaterEnvironmentSetup;

        #region MonoBehaviour Methods

        private void Start()
        {
            Addressables.LoadAssetAsync<GameObject>(assetLabelReference).Completed +=
                (asyncOperationHandle) =>
                {
                    if (asyncOperationHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        //Success to load the object
                        _underwaterObject = Instantiate(asyncOperationHandle.Result);
                        OnUnderWaterEnvironmentSetup?.Invoke(_underwaterObject);
                    }
                    else
                    {
                        //Failed to load the object
                        Debug.Log("Failed to load the object");
                    }
                };
               
        }

        public GameObject LoadGameAsset()
        {
            return _underwaterObject;
        }

        #endregion
    }
}
