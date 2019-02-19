using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GeneratorToolBase : ScriptableWizard {

    private const string _menuDropDownName = "Custom Generator Tool";

    [MenuItem(_menuDropDownName + "/Generate Terrain")]
    static void GenerateTerrain()
    {

        GameObject terrain = new GameObject();
        TerrainData terrainData = new TerrainData();
        terrain = Terrain.CreateTerrainGameObject(terrainData);

    }
}
