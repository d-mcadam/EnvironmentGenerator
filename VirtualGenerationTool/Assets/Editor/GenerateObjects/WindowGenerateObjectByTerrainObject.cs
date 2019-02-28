using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WindowGenerateObjectByTerrainObject : ScriptableWizard
{

    public Terrain _terrainTarget;

    public int _objectQuantity = 1;
    
    public Vector3 _startPosition = new Vector3();
    
    public Vector3 _dimensions = new Vector3();

    void OnWizardUpdate()
    {
        
        //'OK' button is enabled if the target is NOT NULL
        isValid = _terrainTarget;

        //stop here if the target isn't valid
        if (!isValid)
            return;

        _startPosition = GlobalMethods.CheckStartingPoint(_startPosition, _terrainTarget);

        _dimensions = GlobalMethods.CheckDimensionsAgainstTerrain(_startPosition, _dimensions, _terrainTarget);

    }

    void OnWizardCreate()
    {
        //do not need to null-check _terrainTarget as you will not be able to call this method unless it is not null
        GlobalMethods.GenerateObjectsOnTerrain(_terrainTarget, _objectQuantity, _startPosition, _dimensions);
    }

}
