using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WindowGenerateWorldByTerrainObject : ScriptableWizard
{

    public Terrain _terrainTarget;
    
    public int _maximumNumberOfObjects = 105;
    public int _maximumNumberOfObjectInSeries = 20;

    private int _loopFailCount = 0;
    //10 million times, gives acceptable 'lock-out' time
    private const int _maxLoopFail = 1000;

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
        Object[] prefabs = GlobalMethods.GetPrefabs(StringConstants.BasicPrefabFilePath);

        //this is possibly a 'type' parameter? along with 'village'?
        bool creatingCityStreets = true;

        if (creatingCityStreets)
        {
            for (int currentTotalObjects = 0; currentTotalObjects < _maximumNumberOfObjects; currentTotalObjects+=0)
            {
                //choose object (this is effectively a 'seed' for the generator)
                Object obj = prefabs[Random.Range(0, prefabs.Length - 1)];
                VectorBoolReturn startVector = GlobalMethods.GenerateStartingVector(new Vector3(), _terrainTarget.terrainData.size, _terrainTarget);

                if (!startVector.OperationSuccess)
                {
                    DisplayError("Failed to seed environment with initial start vector\n\nLikely an issue with input vector dimensions compared with the Terrain's vertice coordinates");
                    return;
                }

                //initialise a game object variable and instantiate first object
                GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(obj);
                prefab.transform.position = startVector.Vector;
                prefab.transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
                if (prefab.tag == "Cylinder")
                {
                    prefab.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
                    prefab.transform.Translate(new Vector3(prefab.transform.lossyScale.x / 2, 0f, 0f));
                }

                //save reference to first generated object
                GameObject previousPrefab = prefab;

                //starts at 'one' as the 'seed' is the initial object
                for (int currentSeriesTotal = 1; currentSeriesTotal < _maximumNumberOfObjectInSeries; currentSeriesTotal++)
                {
                    if (currentTotalObjects + currentSeriesTotal >= _maximumNumberOfObjects)
                        goto outerloop;

                    _loopFailCount = 0;

                    //choose new object
                    Object newObject = prefabs[Random.Range(0, prefabs.Length - 1)];

                    //check if it satisfies restrictions
                    while (!ObjectWithinParameters(previousPrefab, newObject))
                    {
                        //if it does NOT satisfy restrictions, choose new object again
                        newObject = prefabs[Random.Range(0, prefabs.Length - 1)];

                        //exit method if loop limit reached, an error has occurred
                        if (++_loopFailCount >= _maxLoopFail)
                        {
                            DisplayError(StringConstants.Error_ContinousLoopError);
                            return;
                        }
                    }

                    //instantiate the new object
                    prefab = (GameObject)PrefabUtility.InstantiatePrefab(newObject);
                    //set the original position
                    prefab.transform.position = previousPrefab.transform.position;
                    //translate the position relative to the previous objects rotation
                    prefab.transform.Translate(NewRelativeObjectPosition(prefab, previousPrefab), previousPrefab.transform);
                    //rotate the new object to line up with others
                    prefab.transform.rotation = previousPrefab.transform.rotation;

                    //save a copy
                    previousPrefab = prefab;

                }//end of _maxSeriesQuantity

                currentTotalObjects += _maximumNumberOfObjectInSeries;

            }//end of _maxObjectQuantity

            outerloop:;//used to break from nested loop if maximum quantity reached

        }
        else
        {

        }

    }

    private bool ObjectWithinParameters(GameObject previousObject, Object obj)
    {
        GameObject newObject = (GameObject)obj;

        bool colourCondition = WithinColourParameters(previousObject, newObject);

        bool shapeCondition = WithinShapeParameters(previousObject, newObject);

        return colourCondition && shapeCondition;
    }

    private bool WithinColourParameters(GameObject previousObject, GameObject newObject)
    {
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
        
        return  //the colour for yellow is slightly different to the custom defined material colour for yellow
            (previousObjectColour == Color.red && newObjectColour != Color.blue) ||
            (previousObjectColour == Color.blue && newObjectColour != Color.red) ||
            (previousObjectColour == Color.green && newObjectColour != new Color(1.0f, 1.0f, 0.0f)) ||
            (previousObjectColour == new Color(1.0f, 1.0f, 0.0f) && newObjectColour != Color.green);
    }

    private bool WithinShapeParameters(GameObject previousObject, GameObject newObject)
    {



        return true;
    }

    private Vector3 NewRelativeObjectPosition(GameObject newObj, GameObject oldObj)
    {
        float newObjectLength = 0;
        float oldObjectLength = 0;

        try
        {
            newObjectLength = newObj.transform.GetChild(0).transform.lossyScale.x / 2;
        }
        catch(UnityException e)
        {
            newObjectLength = newObj.transform.lossyScale.x / 2;
        }

        try
        {
            oldObjectLength = oldObj.transform.GetChild(0).transform.lossyScale.x / 2;
        }
        catch(UnityException e)
        {
            oldObjectLength = oldObj.transform.lossyScale.x / 2;
        }
        
        return new Vector3(newObjectLength + oldObjectLength, 0, 0);
    }
    
    private void DisplayError(string msg)
    {
        EditorUtility.DisplayDialog(StringConstants.Error, msg, "OK");
    }

}
