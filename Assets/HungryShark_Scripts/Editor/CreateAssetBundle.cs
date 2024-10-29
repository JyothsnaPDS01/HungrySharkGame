using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public class CreateAssetBundle
{
    [MenuItem("Assets/CreateAssetBundles")]
    private static void BuildAllAssetBundles()
    {
        string assetBundlesDirectoryPath = Application.dataPath + "/../AssetBundles";
        try
        {
            BuildPipeline.BuildAssetBundles(assetBundlesDirectoryPath, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
    }
}
