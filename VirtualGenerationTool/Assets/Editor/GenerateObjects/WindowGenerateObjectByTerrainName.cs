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

    void OnWizardUpdate()
    {

        if (_dimensions.x < 1)
            _dimensions.x = 1;

        if (_dimensions.y < 1)
            _dimensions.y = 1;

        if (_dimensions.z < 1)
            _dimensions.z = 1;

        isValid = GameObject.Find(_name) && GameObject.Find(_name).GetComponent<Terrain>();

        if (!isValid)
            return;

        _startPosition = GlobalMethods.CheckStartingPoint(_startPosition, GameObject.Find(_name).GetComponent<Terrain>());

        _dimensions = GlobalMethods.CheckDimensionsAgainstTerrain(_startPosition, _dimensions, GameObject.Find(_name).GetComponent<Terrain>());

    }

    void OnWizardCreate()
    {
        Terrain terrain = GameObject.Find(_name).GetComponent<Terrain>();
        GlobalMethods.GenerateObjectsOnTerrain(terrain, _objectQuantity, _startPosition, _dimensions);
    }

}
