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

    }
}
