using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WindowGenerateObjectByTerrainTag : ScriptableWizard
{

    public string _tag = "Enter Tag Here";

    public int _objectQuantity = 1;

    public Vector3 _startPosition = new Vector3();

    public Vector3 _dimensions = new Vector3();

    void OnWizardCreate()
    {

        try
        {
            GameObject.FindGameObjectsWithTag(_tag);
        }
        catch(UnityException e)
        {
            GlobalMethods.CreateTagIfNotPresent(_tag);
        }

        GameObject[] terrainObjects = GameObject.FindGameObjectsWithTag(_tag);
        Terrain[] terrains = new Terrain[terrainObjects.Length];

        for (int i = 0; i < terrainObjects.Length; i++)
        {
            terrains[i] = terrainObjects[i].GetComponent<Terrain>();
        }
             
        if (terrains.Length > 0)
        {
            if (terrains.Length > 1)
            {
                GlobalMethods.GenerateObjectsOnTerrains(terrains, _objectQuantity, _startPosition, _dimensions);
            }
            else
            {
                GlobalMethods.GenerateObjectsOnTerrain(terrains[0], _objectQuantity, _startPosition, _dimensions);
            }
        }
        else
        {
            EditorUtility.DisplayDialog("No terrain found by Tag", "Unable to find a terrain with tag \"" + _tag + "\"", "OK");
        }

    }

}
