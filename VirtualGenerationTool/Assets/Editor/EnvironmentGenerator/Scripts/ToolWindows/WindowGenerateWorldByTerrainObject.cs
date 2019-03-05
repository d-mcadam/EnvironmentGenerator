using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WindowGenerateWorldByTerrainObject : ScriptableWizard
{

    public Terrain _terrainTarget;

    private int _loopFailCount = 0;
    //10 million times, gives acceptable 'lock-out' time
    private const int _maxLoopFail = 10000000;

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

        //get all the prefabs
        Object[] prefabs = GlobalMethods.GetPrefabs();

        //this possibly a 'type' parameter? along with 'village'?
        bool creatingCityStreets = true;

        if (creatingCityStreets)
        {
            
            //random object at first
            GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[Random.Range(0, prefabs.Length - 1)]);
            VectorBoolReturn startVector = GlobalMethods.GenerateStartingVector(new Vector3(), _terrainTarget.terrainData.size, _terrainTarget);

            //wont need to check this yet as vector is randomised based on terrain vectors anyway
            //if (!startVector.OperationSuccess)
            //{

            //}

            int yRotation = Random.Range(0, 359);
            prefab.transform.position = startVector.Vector;
            prefab.transform.rotation = Quaternion.Euler(new Vector3(0, yRotation, 0));

            GameObject previousPrefab = prefab;
            GameObject newPrefab = (GameObject)prefabs[Random.Range(0, prefabs.Length - 1)];

            while (!ObjectWithinRestrictions(previousPrefab, newPrefab))
            {
                newPrefab = (GameObject)prefabs[Random.Range(0, prefabs.Length - 1)];

                //exit method if loop limit reached
                if (++_loopFailCount >= _maxLoopFail)
                {
                    EditorUtility.DisplayDialog(StringConstants.Error, StringConstants.Error_ContinousLoopError, "OK");
                    return;
                }
            }
            
        }
        else
        {

        }

    }

    private bool ObjectWithinRestrictions(GameObject previousObject, GameObject newObject)
    {

        bool colourCondition = false;

        Color previousObjectColour = previousObject.GetComponent<MeshRenderer>().material.color;
        Color newObjectColour = newObject.GetComponent<MeshRenderer>().material.color;

        colourCondition = 
            (previousObjectColour == Color.red && newObjectColour != Color.blue) ||
            (previousObjectColour == Color.blue && newObjectColour != Color.red) ||
            (previousObjectColour == Color.green && newObjectColour != Color.yellow) ||
            (previousObjectColour == Color.yellow && newObjectColour != Color.green);
        
        return colourCondition;
    }

}
