using System;
using System.Collections.Generic;
using UnityEditor;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

public class OOOTTAssetUtility : AssetPostprocessor {

    ///Stringy Bits. Point to know directories. Used exclusively by editor tools. 

    //Any time any element is saved/reimported, we can update our one database prefab
    public const string ComponentsPath = "Assets/Components/";
    public const string GamePlayDataPath = ComponentsPath + "GamePlayData/";
    public const string GamePlayPrefabsPath = ComponentsPath + "Prefabs/";
    public const string AssetSuffix = ".asset";
    public const string PrefabSuffix = ".prefab";

    //graphics assets
    public const string FTGMaterialsPath = ComponentsPath + "Materials/Global/";

 

  
    [MenuItem("Assets/Create/11123/CardInSlot")]
    public static void CreateCardAsset()
    {
        ScriptableObjectUtility.CreateAsset<CardSO>();
    }

    [MenuItem("Assets/Create/11123/Player")]
    public static void CreatePlayerAsset()
    {
        ScriptableObjectUtility.CreateAsset<PlayerSO>();
    }


    [MenuItem("Assets/Create/11123/Stack")]
    public static void CreateStackAsset()
    {
        ScriptableObjectUtility.CreateAsset<StackSO>();
    }

    [MenuItem("Assets/Create/11123/MatchSettings")]
    public static void CreateMatchSettingsAsset()
    {
        ScriptableObjectUtility.CreateAsset<MatchSettingsSO>();
    }

    
   

    [MenuItem("11123/UpdateAssetDataBaseAndPrefab")]
    public static void UpdateMainAssetAndPrefab()
    {
        UpdateMainAsset();
        UpdateMainPrefab();
        AssetDatabase.SaveAssets();
        //AssetDatabase.Init();
        EditorUtility.FocusProjectWindow();
    }

    //Updates asset. Creates if none found at path specified above
    public static void UpdateMainAsset()
     {
        /*
         //Get reference to asset
         FTGAssetSO data = AssetDatabase.LoadAssetAtPath(FTGAssetPath, typeof(FTGAssetSO)) as FTGAssetSO;
         if (data == null)//create if not found in the correct place.
         {
             Debug.Log("No Database Asset Found. Creating FTGAssetData at " + FTGAssetPath);
             data = ScriptableObject.CreateInstance<FTGAssetSO>();
             data.Figures = new List<FigureSO>();
             data.Furniture = new List<FurnitureSO>();
             data.Landmarks = new List<LandmarkSO>();
             data.Items = new List<ItemSO>();
             data.Verbs = new List<VerbSO>();
             AssetDatabase.CreateAsset(data, FTGAssetPath);
            
         }

         //Update asset with all known objects
         SetupDatabaseAsset(data);*/

     }
    
     private static void SetupDatabaseAsset(ScriptableObject data)
     {
         /*
         data.Figures.Clear();
         data.Furniture.Clear();
         data.Items.Clear();
         data.Landmarks.Clear();
         
         FigureSO[] figureSos = ScriptableObjectUtility.LoadAllAssetsOfType<FigureSO>(FTGFiguresPath);
         PlayerCharacterSO[] playerCharacterSos = ScriptableObjectUtility.LoadAllAssetsOfType<PlayerCharacterSO>(FTGPlayerCharacterPath);
         NonPlayerCharacterSO[] nonPlayerCharacterSos = ScriptableObjectUtility.LoadAllAssetsOfType<NonPlayerCharacterSO>(FTGNonPlayerCharacterPAth);
         FurnitureSO[] furnitureSos = ScriptableObjectUtility.LoadAllAssetsOfType<FurnitureSO>(FTGFurniturePath);
         ItemSO[] itemSos = ScriptableObjectUtility.LoadAllAssetsOfType<ItemSO>(FTGItemsPath);
         LandmarkSO[] landmarkSos = ScriptableObjectUtility.LoadAllAssetsOfType<LandmarkSO>(FTGLandmarksPath);
         VerbSO[] verbSos = ScriptableObjectUtility.LoadAllAssetsOfType<VerbSO>(FTGVerbsPath);

         data.Figures = new List<FigureSO>(figureSos);
         data.PlayerCharacters = new List<PlayerCharacterSO>(playerCharacterSos);
         data.NonPlayerCharacters = new List<NonPlayerCharacterSO>(nonPlayerCharacterSos);
         data.Items = new List<ItemSO>(itemSos);
         data.Furniture = new List<FurnitureSO>(furnitureSos);
         
         data.Landmarks = new List<LandmarkSO>(landmarkSos);
         data.Verbs = new List<VerbSO>(verbSos);

         foreach (VerbSO VARIABLE in data.Verbs)
         {
             string errorTxt;
             if (VARIABLE.CheckDataErrors(out errorTxt))
             {
                 Debug.LogWarning("Verb: " + VARIABLE.name + " "  + errorTxt);
                 //EditorUtility.FocusProjectWindow();
                 //Selection.activeObject = VARIABLE;
             }
         }
         

         EditorUtility.SetDirty(data);
         AssetDatabase.SaveAssets();
     */
     }
    
    
     public static void UpdateMainPrefab()
     {
       /*  FTGAssetSO data = AssetDatabase.LoadAssetAtPath(FTGAssetPath, typeof(FTGAssetSO)) as FTGAssetSO;
         //Load a prefab reference
         GameObject ftgPrefabReference =
            AssetDatabase.LoadAssetAtPath(FTGPrefabPath, typeof(GameObject)) as GameObject;


         //Instantiate into the scene in order to mess with it
         GameObject ftgPrefabInstance = PrefabUtility.InstantiatePrefab(ftgPrefabReference) as GameObject;

         //get its monobehaviour
         FTGAssetDatabase ftgAssetDatabase = ftgPrefabInstance.GetComponent<FTGAssetDatabase>();
         ftgAssetDatabase.AssetData = data;

         //"apply" prefab
         PrefabUtility.ReplacePrefab(ftgPrefabInstance, ftgPrefabReference);
         EditorUtility.SetDirty(ftgPrefabInstance);
       


         //remove from scene
         Object.DestroyImmediate(ftgPrefabInstance);*/
     }
    //

     private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
     {
         /*
         bool refreshFTG = false;
         bool refreshPaletteEd = false;
         foreach (string importedAsset in importedAssets)
         {
             Object obj = AssetDatabase.LoadAssetAtPath(importedAsset, typeof(Object));
             refreshFTG = refreshFTG || IsFTGAsset(obj);

             refreshPaletteEd = obj is PaletteSO;
         }

         foreach (string importedAsset in movedAssets)
         {
             Object obj = AssetDatabase.LoadAssetAtPath(importedAsset, typeof(Object));
             refreshFTG = refreshFTG || IsFTGAsset(obj);

             refreshPaletteEd = refreshPaletteEd || obj is PaletteSO;
         }
         foreach (string deletedAsset in deletedAssets)
         {
             refreshFTG = refreshFTG ||
                          deletedAsset.Contains(FTGFiguresPath) ||
                          deletedAsset.Contains(FTGFurniturePath) ||
                          deletedAsset.Contains(FTGItemsPath) ||
                          deletedAsset.Contains(FTGPalettePath);

             refreshPaletteEd = refreshPaletteEd || deletedAsset.Contains(FTGPalettePath);
         }

         if(refreshFTG)
         {
             UpdateFTGAssetAndPrefab(); //inifinte loop, sometimes! careful!
         }

         if (refreshPaletteEd && PaletteEditor.IsOpen() )
         {
             PaletteEditor ped = EditorWindow.GetWindow<PaletteEditor>();
             if (ped != null)
             {
                 ped.Refresh();
             }
         }


         if (RoomPreviewEditor.IsOpen())
         {
             RoomPreviewEditor red = EditorWindow.GetWindow<RoomPreviewEditor>();
             if (red != null)
             {
                 red.Refresh();
             }
         }*/

     }

  
}
