using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GlobalMethods {
    
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
    
    public static void GenerateObjectsOnTerrain(Terrain terrain, int quantity, Vector3 start_point, Vector3 dimensions)
    {
        
        for (int i = 0; i < quantity; i++)
        {

            float x = Random.Range(start_point.x, start_point.x + dimensions.x);
            float z = Random.Range(start_point.z, start_point.z + dimensions.z);

            float y = terrain.SampleHeight(new Vector3(x, 0, z));
            
            while (y < start_point.y || y > start_point.y + dimensions.y)
            {
                x = Random.Range(start_point.x, start_point.x + dimensions.x);
                z = Random.Range(start_point.z, start_point.z + dimensions.z);

                y = terrain.SampleHeight(new Vector3(x, 0, z));
            }
            
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = new Vector3(x, y, z) + terrain.transform.position;

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
                GenerateObjectsOnTerrain(terrains[Random.Range(0, terrains.Length)], 1, start_point, dimensions);
            }
        }

    }

    public static Vector3 CheckStartingPoint(Vector3 start_point, Terrain terrain)
    {

        Vector3 terrainSize = terrain.terrainData.size;

        if (start_point.x > terrainSize.x)
            start_point.x = terrainSize.x - 1;

        if (start_point.x < 0)
            start_point.x = 0;

        if (start_point.y > terrainSize.y)
            start_point.y = terrainSize.y - 1;

        if (start_point.y < 0)
            start_point.y = 0;

        if (start_point.z > terrainSize.z)
            start_point.z = terrainSize.z - 1;

        if (start_point.z < 0)
            start_point.z = 0;

        return start_point;

    }

    public static Vector3 CheckDimensionsAgainstTerrain(Vector3 start_point, Vector3 dimensions, Terrain terrain)
    {

        Vector3 terrainSize = terrain.terrainData.size;

        if (start_point.x + dimensions.x > terrainSize.x)
            dimensions.x = terrainSize.x - start_point.x;

        if (dimensions.x < 0)
            dimensions.x = 0;

        if (start_point.y + dimensions.y > terrainSize.y)
            dimensions.y = terrainSize.y - start_point.y;

        if (dimensions.y < 0)
            dimensions.y = 0;
        
        if (start_point.z + dimensions.z > terrainSize.z)
            dimensions.z = terrainSize.z - start_point.z;

        if (dimensions.z < 0)
            dimensions.z = 0;
        
        return dimensions;

    }

}
