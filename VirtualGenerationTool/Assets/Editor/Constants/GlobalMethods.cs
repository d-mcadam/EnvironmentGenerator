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

            float y = terrain.SampleHeight(new Vector3(0, 0, 0));
            Debug.Log(y);
        }

    }

    public static void GenerateObjectsOnTerrains(Terrain[] terrains, int quantity, Vector3 start_point, Vector3 dimensions)
    {

        for (int i = 0; i < quantity; i++)
        {

        }

    }

}
