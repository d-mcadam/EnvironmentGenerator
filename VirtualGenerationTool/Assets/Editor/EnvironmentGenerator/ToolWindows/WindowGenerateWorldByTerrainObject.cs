using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WindowGenerateWorldByTerrainObject : ScriptableWizard
{

    public Terrain _terrainTarget;
    
    void OnWizardUpdate()
    {
        isValid = _terrainTarget;
    }

    void OnWizardCreate()
    {
        GenerationAlgorithm();
    }

    private void GenerationAlgorithm()
    {

        //get all the prefabs
        Object[] prefabs = GlobalMethods.GetPrefabs();

        //random object at first
        GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[Random.Range(0, prefabs.Length - 1)]);
        VectorBoolReturn startVector = GlobalMethods.GenerateStartingVector(new Vector3(), _terrainTarget.terrainData.size, _terrainTarget);

        //wont need to check this yet as vector is randomised based on terrain vectors anyway
        //if (!startVector.OperationSuccess)
        //{

        //}

        prefab.transform.position = startVector.Vector;

    }
}
