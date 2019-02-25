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

        GameObject[] terrains = GameObject.FindGameObjectsWithTag(_tag);

        if (terrains != null)
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
            //didnt find anything
        }

    }

}
