using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class GlobalMethods {

    private static int _loopFailCount = 0;
    private const int _maxLoopFail = 10000000;//10 million times, results in acceptable lock-out time
    
    public static void CreateTagIfNotPresent(string s)
    {

        //get tag manager
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        //may not needs layers
        //SerializedProperty layersProp = tagManager.FindProperty("layers");

        //check if tag exists
        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(s)) { found = true; break; }
        }

        //add it if does not exist
        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(0);
            newTag.stringValue = s;
        }

        tagManager.ApplyModifiedProperties();

    }

    public static void GenerateObjectOnTerrain(Terrain terrain, Vector3 start_point, Vector3 dimensions)
    {

    }
    
    public static void GenerateObjectsOnTerrain(Terrain terrain, int quantity, Vector3 start_point, Vector3 dimensions)
    {

        for (int i = 0; i < quantity; i++)
        {
            _loopFailCount = 0;

            //generate a random X and Z coordinate between specified boundaries
            float x = Random.Range(start_point.x, start_point.x + dimensions.x);
            float z = Random.Range(start_point.z, start_point.z + dimensions.z);

            //get the height (Y) of the terrain at the X and Z coordinate
            float y = terrain.SampleHeight(new Vector3(x, 0, z));

            //check if the Y coordinate is outside specified boundaries
            while (y < start_point.y || y > start_point.y + dimensions.y)
            {
                //continue to generate values until the Y coordinate is within boundaries
                x = Random.Range(start_point.x, start_point.x + dimensions.x);
                z = Random.Range(start_point.z, start_point.z + dimensions.z);

                y = terrain.SampleHeight(new Vector3(x, 0, z));

                if (++_loopFailCount >= _maxLoopFail)
                {
                    EditorUtility.DisplayDialog("CONTINOUS LOOP ERROR",
                        "Failed to identify suitable vector " + _maxLoopFail + " times on object \"" + terrain.name + "\"", "OK");
                    return;
                }
            }

            //get all prefab assets
            List<string> assetFilePaths = GetPrefabFilePaths();
            Object[] prefabs = new Object[assetFilePaths.Count];
            for (int j = 0; j < prefabs.Length; j++)
            {
                prefabs[j] = AssetDatabase.LoadAssetAtPath(assetFilePaths[j], typeof(GameObject));
            }

            GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[0]);
            prefab.transform.position = new Vector3(x, y, z) + terrain.transform.position;
        }

    }

    public static void GenerateObjectsOnTerrains(Terrain[] terrains, int quantity, Vector3 start_point, Vector3 dimensions, bool quantityPerTerrain)
    {

        if (quantityPerTerrain)
        {
            for (int i = 0; i < terrains.Length; i++)
            {
                GenerateObjectsOnTerrain(terrains[i], quantity, start_point, dimensions);
            }
        }
        else
        {
            for (int i = 0; i < quantity; i++)
            {
                //can possible add features where they are distributed evenly or more are distributed on one terrain than the other
                GenerateObjectsOnTerrain(terrains[Random.Range(0, terrains.Length)], 1, start_point, dimensions);
            }
        }

    }

    public static Vector3 CheckStartingPoint(Vector3 start_point, Terrain terrain)
    {

        Vector3 terrainSize = terrain.terrainData.size;

        if (start_point.x >= terrainSize.x)
            start_point.x = terrainSize.x;

        if (start_point.x < 0)
            start_point.x = 0;

        if (start_point.y >= terrainSize.y)
            start_point.y = terrainSize.y;

        if (start_point.y < 0)
            start_point.y = 0;

        if (start_point.z >= terrainSize.z)
            start_point.z = terrainSize.z;

        if (start_point.z < 0)
            start_point.z = 0;

        return start_point;

    }

    public static Vector3 CheckDimensionsAgainstTerrain(Vector3 start_point, Vector3 dimensions, Terrain terrain)
    {

        Vector3 terrainSize = terrain.terrainData.size;

        if (start_point.x + dimensions.x > terrainSize.x)
            dimensions.x = terrainSize.x - start_point.x;

        if (dimensions.x < 1)
            dimensions.x = 1;

        if (start_point.y + dimensions.y > terrainSize.y)
            dimensions.y = terrainSize.y - start_point.y;

        if (dimensions.y < 1)
            dimensions.y = 1;
        
        if (start_point.z + dimensions.z > terrainSize.z)
            dimensions.z = terrainSize.z - start_point.z;

        if (dimensions.z < 1)
            dimensions.z = 1;
        
        return dimensions;

    }
    
    private static List<string> GetPrefabFilePaths()
    {
        //get a list of all the asset file paths
        List<string> filePaths = new List<string>();
        foreach (string s in Directory.GetFiles("Assets/Editor/EnvironmentGenerator/Prefabs/"))
        {
            filePaths.Add(s);
        }

        //get a collection of file paths to remove (cannot remove from 'filePaths' as Concurrent Modification Exeption thrown)
        List<string> stringsToRemove = new List<string>();
        foreach (string s in filePaths)
        {
            //remove any meta files
            if (s.Contains(".prefab.meta"))
                stringsToRemove.Add(s);

            //remove anything that isn't a prefab
            if (!s.Contains(".prefab"))
                stringsToRemove.Add(s);
        }

        //remove the unwanted file paths
        foreach (string s in stringsToRemove)
        {
            filePaths.Remove(s);
        }
        
        //return ONLY prefab file paths (can be loaded with AssetDatabase.LoadAssetAtPath)
        return filePaths;
    }

}
