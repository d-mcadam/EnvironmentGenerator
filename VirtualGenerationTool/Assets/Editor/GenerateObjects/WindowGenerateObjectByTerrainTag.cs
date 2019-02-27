using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WindowGenerateObjectByTerrainTag : ScriptableWizard
{

    public string _tag = "Enter Tag Here";

    public int _objectQuantity = 10;

    public Vector3 _startPosition = new Vector3();

    public Vector3 _dimensions = new Vector3(100, 100, 100);

    void OnWizardCreate()
    {

        try
        {
            GameObject.FindGameObjectsWithTag(_tag);
        }
        catch(UnityException e)
        {

            if (EditorUtility.DisplayDialog("Unidentified Tag",
                    "Tag \"" + _tag + "\" does not exist in this project, would you like to create it?", "Yes", "No"))
                GlobalMethods.CreateTagIfNotPresent(_tag);
            else
                return;

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
                
                GlobalMethods.GenerateObjectsOnTerrains(terrains, _objectQuantity, _startPosition, _dimensions,
                    EditorUtility.DisplayDialog("Multiple Terrains found", 
                    "Would you like to generate the maximum number of objects on each terrain OR distribute them across all terrains?", 
                    "Maximum Per Terrain", "Distribute Across Terrains"));

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
