﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class GeneratorToolBase : ScriptableWizard {

    //constant variables
    private const string _menuDropDownName = "Custom Generator Tool";
    private const string _terrainTag = "Base Terrain";
    
    
    private static void CreateTerrainTagIfNotPresent()
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
            if (t.stringValue.Equals(_terrainTag)) { found = true; break; }
        }

        //add it if does not exist
        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(0);
            newTag.stringValue = _terrainTag;
        }

        tagManager.ApplyModifiedProperties();
    }

    private static void GenerateTerrain()
    {

        TerrainData terrainData = new TerrainData();
        GameObject terrain = Terrain.CreateTerrainGameObject(terrainData);
        terrain.tag = _terrainTag;

    }


    [MenuItem(_menuDropDownName + "/Generate Terrain")]
    static void GenerateTerrainButton()
    {
        
        //check tag exists
        try
        {
            GameObject.FindGameObjectWithTag(_terrainTag);
        }
        catch (UnityException e)
        {
            CreateTerrainTagIfNotPresent();
        }

        //find any terrains with this tag
        if (GameObject.FindGameObjectsWithTag(_terrainTag).Length > 0)
        {
            //Confirm overwrite of existing terrain object
            if (EditorUtility.DisplayDialog("Terrain already exists",
                "Do you want to replace the existing terrain?",
                "Replace", "Cancel"))
            {

                foreach (GameObject obj in GameObject.FindGameObjectsWithTag(_terrainTag))
                {
                    DestroyImmediate(obj);
                }

                GenerateTerrain();

            }

            return;

        }

        //generate a terrain if none with specified tag exists
        GenerateTerrain();

    }

    [MenuItem(_menuDropDownName + "/Generate Objects")]
    static void GenerateObjectsWizard()
    {
        ScriptableWizard.DisplayWizard<GeneratorToolBase>("Generate Objects", "Generate");
    }

    void OnWizardCreate()
    {
        
    }
}
