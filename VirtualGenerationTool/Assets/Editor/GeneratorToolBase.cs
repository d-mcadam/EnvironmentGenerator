using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class GeneratorToolBase : ScriptableWizard {

    public GameObject _terrainTarget;

    public int _objectQuantity = 1;

    public int _xStartPoint = 1;
    public int _yStartPoint = 1;
    public int _zStartPoint = 1;

    public int _xAxisRange = 1;
    public int _yAxisRange = 1;
    public int _zAxisRange = 1;

    
    private static void GenerateObject(int x, int y, int z)
    {
        GameObject cube = new GameObject();
        //DestroyImmediate(cube);
        //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //cube.transform.position = new Vector3(x, y, z);
    }

    
    [MenuItem(StringConstants.CustomGeneratorTool_MenuTitle + "/Generate Objects")]
    static void GenerateObjectsWizard()
    {
        ScriptableWizard.DisplayWizard<GeneratorToolBase>("Generate Objects", "Generate");
    }

    void OnWizardCreate()
    {
        System.Random rnd = new System.Random();
        for (int i = 0; i < _objectQuantity; i++)
        {
            int x = rnd.Next(_xStartPoint, _xStartPoint + _xAxisRange);
            int y = rnd.Next(_yStartPoint, _yStartPoint + _yAxisRange);
            int z = rnd.Next(_zStartPoint, _zStartPoint + _zAxisRange);

            GenerateObject(x, y, z);
        }
    }
}
