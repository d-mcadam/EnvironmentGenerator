using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GenerateEnvironmentOnTerrainWindow : EditorWindow
{

    private Terrain _terrainTarget;

    private GenerateWorldTheme _generatorTheme = GenerateWorldTheme.Cities;

    private int _maximumNumberOfObjects = 105;
    private int _maximumNumberInSeriesOrCluster = 20;
    private int _maximumRowsOfSeries = 5;

    private int _loopFailCount = 0;
    //10 million times, gives acceptable 'lock-out' time (will need modifying)
    private const int _maxLoopFail = 1000;

    void OnGUI()
    {
        //title
        GUILayout.Label("Virtual Environment Generator Tool ©", EditorStyles.centeredGreyMiniLabel);

        //terrain object
        GUILayout.Label("Terrain object to target", EditorStyles.boldLabel);        EditorGUI.indentLevel++;
        _terrainTarget = (Terrain)EditorGUILayout.ObjectField("Terrain Object:", _terrainTarget, typeof(Terrain), true);
        EditorGUI.indentLevel--;

        //generation type
        GUILayout.Label("Generation Type", EditorStyles.boldLabel);        EditorGUI.indentLevel++;
        _generatorTheme = (GenerateWorldTheme)EditorGUILayout.EnumPopup("Select Type:", _generatorTheme);
        EditorGUI.indentLevel--;

        //limited generator values
        GUILayout.Label("Maximum Generation Values", EditorStyles.boldLabel);        EditorGUI.indentLevel++;
        _maximumNumberOfObjects = EditorGUILayout.IntField("Maximum Total", _maximumNumberOfObjects);
        _maximumNumberInSeriesOrCluster = 
            EditorGUILayout.IntField("Total in " + (_generatorTheme == GenerateWorldTheme.Cities ? "Series" : "Cluster"), _maximumNumberInSeriesOrCluster);
        EditorGUI.indentLevel--;

        //buttons
        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(!_terrainTarget);
        if (GUILayout.Button(StringConstants.GenerateEnvironment_ButtonText) && false)
        {
            BasicPrefabGenerationAlgorithm();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Generate using models"))
        {
            ModelPrefabGenerationAlgorithm();
        }
        EditorGUI.EndDisabledGroup();
    }

    private void ModelPrefabGenerationAlgorithm()
    {
        Object[] models = GlobalMethods.GetPrefabs(StringConstants.ModelPrefabFilePath);
        Debug.Log(models.Length);
    }
    
    private void BasicPrefabGenerationAlgorithm()
    {
        //get an array of all the prefabs
        Object[] prefabs = GlobalMethods.GetPrefabs(StringConstants.BasicPrefabFilePath);

        switch (_generatorTheme)
        {
            case GenerateWorldTheme.Cities:
                BasicPrefabGenerateCities(prefabs);
                break;
            case GenerateWorldTheme.Villages:
                break;
            default:
                DisplayError("Switch statement 'default' hit");
                break;
        }
        
    }
    private void BasicPrefabGenerateCities(Object[] prefabs)
    {
        for (int currentTotalObjects = 0; currentTotalObjects < _maximumNumberOfObjects; currentTotalObjects += 0)
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
            for (int currentSeriesTotal = 1; currentSeriesTotal < _maximumNumberInSeriesOrCluster; currentSeriesTotal++)
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

            currentTotalObjects += _maximumNumberInSeriesOrCluster;

        }//end of _maxObjectQuantity

        outerloop:;//used to break from nested loop if maximum quantity reached

    }
    private void BasicPrefabGenerateVillages(Object[] prefabs){

    }

    private bool ObjectWithinParameters(GameObject previousObject, Object obj)
    {
        GameObject newObject = (GameObject)obj;

        bool colourCondition = WithinColourParameters(previousObject, newObject);

        bool sizeCondition = WithinSizeParameters(previousObject, newObject);

        return colourCondition && sizeCondition;
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

    private bool WithinSizeParameters(GameObject previousObject, GameObject newObject)
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
        catch (UnityException e)
        {
            newObjectLength = newObj.transform.lossyScale.x / 2;
        }

        try
        {
            oldObjectLength = oldObj.transform.GetChild(0).transform.lossyScale.x / 2;
        }
        catch (UnityException e)
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
