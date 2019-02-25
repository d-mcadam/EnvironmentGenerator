using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WindowGenerateObjecyByTerrainObject : ScriptableWizard
{
    public GameObject _terrainTarget;

    public int _objectQuantity = 1;

    public Vector3 _startPosition = new Vector3();

    public Vector3 _dimensions = new Vector3();

    void OnWizardCreate()
    {
        GlobalMethods.GenerateObjectsOnTerrain(_terrainTarget, _objectQuantity, _startPosition, _dimensions);
    }

}
