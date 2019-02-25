using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WindowGenerateTerrain : ScriptableWizard {
    
    [Tooltip("The name given to the terrain object")]
    public string _objectTitle = "Enter Title Here";
    [Tooltip("If checked, the new object will be tagged as \"Base Terrain\"")]
    public bool _labelAsBaseTerrain = false;
    
    public int _x = 0;
    public int _y = 0;
    public int _z = 0;
    
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
