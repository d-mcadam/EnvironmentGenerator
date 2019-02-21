using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TerrainObjectGenerator : ScriptableWizard {

    public string _objectTitle = "Title";
    public bool _labelAsBaseTerrain = false;

    public int _x = 0;
    public int _y = 0;
    public int _z = 0;

	[MenuItem(StringConstants.CustomGeneratorToolMenuTitle + "/" + StringConstants.GenerateTerrainButtonText)]
    static void GenerateTerrainButton()
    {
        ScriptableWizard.DisplayWizard<TerrainObjectGenerator>("Terrain Details", "Create");
    }

    void OnWizardCreate()
    {

        TerrainData terrainData = new TerrainData();
        GameObject terrain = Terrain.CreateTerrainGameObject(terrainData);

        terrain.transform.position = new Vector3(_x, _y, _z);
        terrain.name = _objectTitle;
        if (_labelAsBaseTerrain)
        {
            
            try
            {
                GameObject.FindGameObjectWithTag(StringConstants.BaseTerrainTag);
            }
            catch (UnityException e)
            {
                GlobalMethods.CreateTagIfNotPresent(StringConstants.BaseTerrainTag);
            }

            terrain.tag = StringConstants.BaseTerrainTag;

        }

    }
}
