using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WindowGenerateObjectByTerrainObject : ScriptableWizard
{
    public GameObject _terrainTarget;

    public int _objectQuantity = 1;

    public Vector3 _startPosition = new Vector3();

    public Vector3 _dimensions = new Vector3();

    void OnWizardUpdate()
    {
        //'OK' button is enabled if the target is NOT NULL and the target is a TERRAIN object
        isValid = _terrainTarget != null && _terrainTarget.GetComponent<Terrain>() != null;
    }

    void OnWizardCreate()
    {
        //do not need to null-check _terrainTarget as you will not be able to call this method unless it is not null
        GlobalMethods.GenerateObjectsOnTerrain(_terrainTarget, _objectQuantity, _startPosition, _dimensions);
    }

}
