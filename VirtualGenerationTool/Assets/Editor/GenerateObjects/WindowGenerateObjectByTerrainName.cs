using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WindowGenerateObjectByTerrainName : ScriptableWizard
{

    public string _name = "Enter Name Here";

    public int _objectQuantity = 1;

    public Vector3 _startPosition = new Vector3();

    public Vector3 _dimensions = new Vector3();

    void OnWizardCreate()
    {

        GameObject terrain = GameObject.Find(_name);

        if (terrain != null)
        {
            GlobalMethods.GenerateObjectsOnTerrain(terrain, _objectQuantity, _startPosition, _dimensions);
        }
        else
        {
            //didnt find anything
        }

    }

}
