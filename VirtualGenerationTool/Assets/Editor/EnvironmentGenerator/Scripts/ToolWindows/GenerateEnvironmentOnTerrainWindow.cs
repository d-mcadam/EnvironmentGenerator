using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GenerateEnvironmentOnTerrainWindow : EditorWindow
{

    private Terrain _terrainTarget;

    //possibly make this an editable list
    private GenerateWorldTheme _generatorTheme = GenerateWorldTheme.ModernCities;

    private int _maximumNumberOfObjects = 20;
    private int _maximumNumberInSeriesOrCluster = 5;
    private int _maximumNumberOfGroups = 5;

    private int _loopFailCount = 0;
    //1 million times, gives unacceptable 'lock-out' time (will need modifying)
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
            EditorGUILayout.IntField("Total in " + 
                (_generatorTheme == GenerateWorldTheme.ModernCitiesWithStreets || 
                _generatorTheme == GenerateWorldTheme.CityStreets ? "Series" : "Cluster"),
                _maximumNumberInSeriesOrCluster);



        EditorGUI.indentLevel--;

        //buttons
        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(!_terrainTarget);
        if (GUILayout.Button(StringConstants.GenerateEnvironment_ButtonText))
        {
            //BasicPrefabGenerationAlgorithm();
            //CalculateModelFaceArea();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Generate using models"))
        {
            ModelPrefabGenerationAlgorithm();
        }
        EditorGUI.EndDisabledGroup();
    }

    private void CalculateModelFaceArea(int[,] points)
    {
        //int[,] points = { { 1, 1 }, { 1, 3 }, { 6, 3 }, { 6, 1 } };
        
        int n = points.GetLength(0) - 1;
        int sum = 0;

        for (int i = 0; i <= n - 1; i++)
        {
            int val = points[i + 1, 0] * points[i, 1];
            sum += val;
        }

        sum += points[0, 0] * points[n, 1];
        
        for (int i = 0; i <= n - 1; i++)
        {
            int val = points[i, 0] * points[i + 1, 1];
            sum -= val;
        }

        sum -= points[n, 0] * points[0, 1];

        sum /= 2;

        if (sum < 0)
            sum = -sum;

        Debug.Log(sum);
    }

    private Vector3 NewModelRelativePosition(GameObject oldObj, GameObject newObj)
    {
        Vector3 movementVector = GlobalMethods.VectorToMoveObjectInLocalAxis(oldObj.name, newObj.name);
        
        return movementVector;
    }

    private void ModelPrefabGenerationAlgorithm()
    {

        //full generation loop
        for (int currentTotal = 0; currentTotal < /*-1*/_maximumNumberOfObjects; currentTotal += 0)
        {

            //get all models
            Object[] models = GlobalMethods.GetPrefabs(StringConstants.ModelPrefabFilePath);

            //select a random model (generator 'seed')
            Object model = models[Random.Range(0, models.Length)];

            //give it a starting position
            VectorBoolReturn startVector = GlobalMethods.StartingVector(new Vector3(), _terrainTarget.terrainData.size, _terrainTarget);
            if (!startVector.OperationSuccess)
            {
                GlobalMethods.DisplayError("Failed to seed environment with initial start vector\n\nLikely an issue with input vector dimensions compared with the Terrain's world vertex coordinates");
                return;
            }

            //instantiate the 'seed' model and initiate the generator
            GameObject newModel = (GameObject)PrefabUtility.InstantiatePrefab(model);
            newModel.transform.position = startVector.Vector;
            newModel.transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));

            //save a reference of the previously generated object
            GameObject previousModel = newModel;      //groupTotal starts at 1

            //series generation loop
            for (int groupTotal = 1; groupTotal < _maximumNumberInSeriesOrCluster; groupTotal++)
            {
                //check if we have reached the maximum, exit full generation loop if so
                if (groupTotal + currentTotal >= _maximumNumberOfObjects)
                    goto outerloop;

                //continue selecting models to generate in series
                Object newObject = models[Random.Range(0, models.Length - 1)];
                
                //check if new model fits parameters
                _loopFailCount = 0;
                while (!ModelWithinParameters(previousModel, newObject))
                {

                    //exit method if loop limit reached, an error has occurred
                    if (++_loopFailCount >= _maxLoopFail)
                    {
                        GlobalMethods.DisplayError(StringConstants.Error_ContinousLoopError);
                        return;
                    }

                    //if it does NOT satisfy restrictions, choose new object
                    newObject = models[Random.Range(0, models.Length - 1)];

                }

                //instantiate the selected model to continue the series
                newModel = (GameObject)PrefabUtility.InstantiatePrefab(newObject);

                //initialise the position to match the previous model
                newModel.transform.position = previousModel.transform.position;

                //move the object to the new relative position
                newModel.transform.Translate(NewModelRelativePosition(previousModel, newModel), previousModel.transform);

                //rotate the object to account for its new position
                newModel.transform.rotation = previousModel.transform.rotation;

                //adjust rotation if the previous object was a Large Industrial building 
                if (previousModel.name.Contains(StringConstants.LargeIndustrial))
                    newModel.transform.Rotate(0.0f, -90.0f, 0.0f);

                //save a reference to the pevious model
                previousModel = newModel;

            }//end of _maximumNumberInSeriesOrCluster loop - series generation loop

            currentTotal += _maximumNumberInSeriesOrCluster;

        }//end of _maximumNumberOfObjects loop - full generation loop

    outerloop:;

        return;
        //TEST DATA
        Vector3[] vs = GlobalMethods.TestData;

        //get all the edges of the model
        List<LineVectorsReturn> lineArrays = GlobalMethods.SortMeshVerticesToLineArrays(vs);

        //get the corners of the model
        List<Vector3> modelCorners = GlobalMethods.FindEdgeCorners(lineArrays);

        List<Vector3> singleFaceVectors = GlobalMethods.FindModelFaceInAxisDirection(modelCorners, AxisDirection.X, true);

        Vector3 faceCenterGroundLevelVector = GlobalMethods.FaceCenterAtGroundLevel(singleFaceVectors);

        foreach (Object o in GlobalMethods.GetPrefabs(StringConstants.ModelPrefabFilePath))
        {
            Debug.Log(o.name);

            GameObject model = (GameObject)o;

            Vector3[] vectors = model.transform.GetChild(0).transform.GetComponent<MeshFilter>().sharedMesh.vertices;

            Debug.Log("vertex array count: " + vectors.Length);

            List<LineVectorsReturn> lines = GlobalMethods.SortMeshVerticesToLineArrays(vectors);

            Debug.Log("Line count: " + lines.Count);

            List<Vector3> corners = GlobalMethods.FindEdgeCorners(lines);

            Debug.Log("Corner count: " + corners.Count);

            List<Vector3> faceVectors = GlobalMethods.FindModelFaceInAxisDirection(corners, AxisDirection.X, true);

            Debug.Log("face vector count: " + faceVectors.Count);

            foreach(Vector3 v in faceVectors)
            {
                Debug.Log("Face vector: " + v);
            }

            Vector3 groundlevel = GlobalMethods.FaceCenterAtGroundLevel(faceVectors);
            Debug.Log("Ground level vector: " + groundlevel);
        }
        
    }

    private bool ModelWithinParameters(GameObject previousObject, Object obj)
    {


        return true;
    }
    

    private Vector3 BasicPrefabNewRelativeObjectPosition(GameObject newObj, GameObject oldObj)
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


    private void BasicPrefabGenerationAlgorithm()
    {
        //get an array of all the prefabs
        Object[] prefabs = GlobalMethods.GetPrefabs(StringConstants.BasicPrefabFilePath);

        switch (_generatorTheme)
        {
            case GenerateWorldTheme.CityStreets:
                BasicPrefabGenerateCityStreets(prefabs);
                break;
            default:
                GlobalMethods.DisplayError("Switch statement 'default' hit");
                break;
        }
        
    }
    private void BasicPrefabGenerateCityStreets(Object[] prefabs)
    {
        for (int currentTotalObjects = 0; currentTotalObjects < _maximumNumberOfObjects; currentTotalObjects += 0)
        {
            //choose object (this is effectively a 'seed' for the generator)
            Object obj = prefabs[Random.Range(0, prefabs.Length - 1)];
            VectorBoolReturn startVector = GlobalMethods.StartingVector(new Vector3(), _terrainTarget.terrainData.size, _terrainTarget);

            if (!startVector.OperationSuccess)
            {
                GlobalMethods.DisplayError("Failed to seed environment with initial start vector\n\nLikely an issue with input vector dimensions compared with the Terrain's vertice coordinates");
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
                while (!BasicPrefabObjectWithinParameters(previousPrefab, newObject))
                {
                    //if it does NOT satisfy restrictions, choose new object again
                    newObject = prefabs[Random.Range(0, prefabs.Length - 1)];

                    //exit method if loop limit reached, an error has occurred
                    if (++_loopFailCount >= _maxLoopFail)
                    {
                        GlobalMethods.DisplayError(StringConstants.Error_ContinousLoopError);
                        return;
                    }
                }

                //instantiate the new object
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(newObject);
                //set the original position
                prefab.transform.position = previousPrefab.transform.position;
                //translate the position relative to the previous objects rotation
                prefab.transform.Translate(BasicPrefabNewRelativeObjectPosition(prefab, previousPrefab), previousPrefab.transform);
                //rotate the new object to line up with others
                prefab.transform.rotation = previousPrefab.transform.rotation;

                //save a copy
                previousPrefab = prefab;

            }//end of _maxSeriesQuantity

            currentTotalObjects += _maximumNumberInSeriesOrCluster;

        }//end of _maxObjectQuantity

        outerloop:;//used to break from nested loop if maximum quantity reached

    }


    private bool BasicPrefabObjectWithinParameters(GameObject previousObject, Object obj)
    {
        GameObject newObject = (GameObject)obj;

        bool colourCondition = BasicPrefabWithinColourParameters(previousObject, newObject);

        bool sizeCondition = BasicPrefabWithinSizeParameters(previousObject, newObject);

        return colourCondition && sizeCondition;
    }
    private bool BasicPrefabWithinColourParameters(GameObject previousObject, GameObject newObject)
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
    private bool BasicPrefabWithinSizeParameters(GameObject previousObject, GameObject newObject)
    {



        return true;
    }
    
}
