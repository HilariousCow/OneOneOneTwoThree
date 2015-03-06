using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using Object = UnityEngine.Object;

public static class ScriptableObjectUtility
{
    /// <summary>
    //	This makes it easy to create, name and place unique new ScriptableObject asset files.
    /// </summary>
    public static T CreateAsset<T>( string specificPathAndFileName ) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();

        if(specificPathAndFileName!="")
        {
            Debug.Log("trying to create:" + specificPathAndFileName);
            AssetDatabase.CreateAsset(asset, specificPathAndFileName);
            
        }
        else
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject); //director path
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string assetPathAndName =
                AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T).ToString() + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);
            
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        return asset;
    }
    public static T CreateAsset<T>() where T : ScriptableObject
    {
        return CreateAsset<T>("");
    }

    public static T[] LoadAllAssetsOfType<T>(string optionalPath) where T : Object
    {
        string[] GUIDs;
        if(optionalPath != "")
        {
            if(optionalPath.EndsWith("/"))
            {
                optionalPath = optionalPath.TrimEnd('/');
            }
            GUIDs =AssetDatabase.FindAssets("t:" + typeof (T).ToString(),new string[] { optionalPath  });
        }
        else
        {
            GUIDs =AssetDatabase.FindAssets("t:" + typeof (T).ToString());
        }
        T[] objectList = new T[GUIDs.Length];

        Debug.Log("Found " + GUIDs.Length + " for object type:" + typeof(T).ToString());
        for (int index = 0; index < GUIDs.Length; index++)
        {
            string guid = GUIDs[index];
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)) as T;
            objectList[index] = asset;
        }

        return objectList;
    }

    //So, there's no "load all assets in directory". The "LoadAllAssetsAtPath" referrs to compound assets, like maya files. 
    //have to go through each thing in the directory, check if it's a prefab, check if it has our component.
    public static List<T> LoadAllPrefabsOfType<T>(string optionalPath) where T : MonoBehaviour
    {
        if (optionalPath != "")
        {
            if (optionalPath.EndsWith("/"))
            {
                optionalPath = optionalPath.TrimEnd('/');
            }
        }

        DirectoryInfo dirInfo = new DirectoryInfo(optionalPath);
        FileInfo[] fileInf = dirInfo.GetFiles("*.prefab");

        //loop through directory loading the game object and checking if it has the component you want
        List<T> prefabComponents = new List<T>();
        foreach (FileInfo fileInfo in fileInf)
        {
            string fullPath = fileInfo.FullName.Replace(@"\","/");
            string assetPath = "Assets" + fullPath.Replace(Application.dataPath, "");
            GameObject prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;

            if(prefab!= null)
            {
                T hasT = prefab.GetComponent<T>();
                if (hasT !=null)
                {
                    prefabComponents.Add(hasT);
                }
            }
        }
        return prefabComponents;
    }

    public static T FindAtPathOrCreateAsset<T>(string fullPath) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath(fullPath, typeof(T)) as T;
        if (asset != null)
        {
            return asset;
        }

        return CreateAsset<T>(fullPath);


    }
}