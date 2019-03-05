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

        //get an array of all the prefabs
        Object[] prefabs = GlobalMethods.GetPrefabs();

        //this possibly a 'type' parameter? along with 'village'?
        bool creatingCityStreets = true;

        if (creatingCityStreets)
        {
            //choose object
            Object obj = prefabs[Random.Range(0, prefabs.Length - 1)];
            VectorBoolReturn startVector = GlobalMethods.GenerateStartingVector(new Vector3(), _terrainTarget.terrainData.size, _terrainTarget);

            if (!startVector.OperationSuccess)
            {
                EditorUtility.DisplayDialog(StringConstants.Error, "FAILED", "OK");
                return;
            }

            //instantiate object
            GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(obj);
            prefab.transform.position = startVector.Vector;
            prefab.transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));

            //save reference to first generated object
            GameObject previousPrefab = prefab;

            for (int i = 0; i < 3; i++)
            {
                //choose new object
                Object newObj = prefabs[Random.Range(0, prefabs.Length - 1)];

                //check if it satisfies restrictions
                while (!ObjectWithinRestrictions(previousPrefab, newObj))
                {
                    //if it does NOT satisfy restrictions, choose new object again
                    newObj = prefabs[Random.Range(0, prefabs.Length - 1)];

                    //exit method if loop limit reached, an error has occurred
                    if (++_loopFailCount >= _maxLoopFail)
                    {
                        EditorUtility.DisplayDialog(StringConstants.Error, StringConstants.Error_ContinousLoopError, "OK");
                        return;
                    }
                }

                prefab = (GameObject)PrefabUtility.InstantiatePrefab(newObj);
                prefab.transform.position = previousPrefab.transform.position;
                prefab.transform.rotation = previousPrefab.transform.rotation;
            }
            
        }
        else
        {

        }

    }

    private bool ObjectWithinRestrictions(GameObject previousObject, Object obj)
    {
        GameObject newObject = (GameObject)obj;

        bool colourCondition = false;

        Color previousObjectColour = Color.white;
        Color newObjectColour = Color.white;

        try
        {
            previousObjectColour = previousObject.GetComponent<MeshRenderer>().sharedMaterial.color;
        }
        catch (MissingComponentException e)
        {
            previousObjectColour = previousObject.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial.color;
        }

        try
        {
            newObjectColour = newObject.GetComponent<MeshRenderer>().sharedMaterial.color;
        }
        catch (MissingComponentException e)
        {
            newObjectColour = newObject.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial.color;
        }


        colourCondition = 
            (previousObjectColour == Color.red && newObjectColour != Color.blue) ||
            (previousObjectColour == Color.blue && newObjectColour != Color.red) ||
            (previousObjectColour == Color.green && newObjectColour != Color.yellow) ||
            (previousObjectColour == Color.yellow && newObjectColour != Color.green);
        
        return colourCondition;
    }
    
    private void DisplayError(string msg)
    {
        EditorUtility.DisplayDialog(StringConstants.Error, msg, "OK");
    }

}
