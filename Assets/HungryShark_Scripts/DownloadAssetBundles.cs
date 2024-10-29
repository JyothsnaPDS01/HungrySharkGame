using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class DownloadAssetBundles : MonoBehaviour
{
    [Header("UnderWater SpawnPoint")]
    [SerializeField] private Transform _underWaterSpawnPoint;
    void Start()
    {
        StartCoroutine(DownloadAssetBundlesFromServer());
    }
    private IEnumerator DownloadAssetBundlesFromServer()
    {
        GameObject go = null;

        //assetBundle Url
        string url = "https://drive.usercontent.google.com/u/0/uc?id=1_2FgKRlnKSfP-Z-LQv0EcWvG2SoHE_rj&export=download";
        using (UnityWebRequest webRequest = UnityWebRequestAssetBundle.GetAssetBundle(url))
        {
            yield return webRequest.SendWebRequest();

            // Check for any kind of network error or protocol error
            if (webRequest.isHttpError || webRequest.isNetworkError)
            {
                Debug.LogError("Error while downloading AssetBundle: " + webRequest.error);
            }
            else
            {
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(webRequest);
                // Use the loaded AssetBundle

                go = bundle.LoadAsset(bundle.GetAllAssetNames()[0]) as GameObject;
                bundle.Unload(false);

                yield return new WaitForEndOfFrame();
            }

            webRequest.Dispose();
        }
        InstantiateGameObjectFromAssetBundles(go);
    }

    private void InstantiateGameObjectFromAssetBundles(GameObject _go)
    {
        if (_go != null)
        {
            GameObject instantiatedObj = Instantiate(_go, _underWaterSpawnPoint);
            instantiatedObj.transform.position = Vector3.zero;
        }
        else
        {
            Debug.LogError("Your assetbundle object is null");
        }
    }
}
