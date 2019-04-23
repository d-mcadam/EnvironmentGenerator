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
    private const int _maxLoopFail = 10;

    private bool _showLandRestrictionTools = false;
    private bool _landRestrictionDrawingEnabled = false;
    public Vector2[] _restrictedAreas;

    private void CreateSpace(int space)
    {
        for (int i = 0; i < space; i++)
            EditorGUILayout.Space();
    }
    public void OnGUI()
    {
        //title
        GUILayout.Label("Virtual Environment Generator Tool ©", EditorStyles.centeredGreyMiniLabel);

        CreateTerrainField();
        CreateDropdownFields();
        CreateMaximumValueFields();

        CreateSpace(2);

        CreateLandRestrictionFields();

        CreateSpace(4);

        CreateGeneratorButtons();
    }
    private void CreateTerrainField()
    {

        //terrain object
        GUILayout.Label("Terrain object to target", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        _terrainTarget = (Terrain)EditorGUILayout.ObjectField("Terrain Object:", _terrainTarget, typeof(Terrain), true);
        EditorGUI.indentLevel--;

    }
    private void CreateDropdownFields()
    {

        //generation type
        GUILayout.Label("Generation Type", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        _generatorTheme = (GenerateWorldTheme)EditorGUILayout.EnumPopup("Select Type:", _generatorTheme);
        EditorGUI.indentLevel--;

    }
    private void CreateMaximumValueFields()
    {

        //limited generator values
        GUILayout.Label("Maximum Generation Values", EditorStyles.boldLabel);

        EditorGUI.indentLevel++;

        _maximumNumberOfObjects = EditorGUILayout.IntField("Maximum Total", _maximumNumberOfObjects);

        _maximumNumberInSeriesOrCluster =
            EditorGUILayout.IntField("Total in " +
                (_generatorTheme == GenerateWorldTheme.ModernCitiesWithStreets ||
                _generatorTheme == GenerateWorldTheme.CityStreets ? "Series" : "Cluster"),
                _maximumNumberInSeriesOrCluster);

        _maximumNumberOfGroups = EditorGUILayout.IntField("Maximum groups", _maximumNumberOfGroups);

        EditorGUI.indentLevel--;

    }
    private void CreateLandRestrictionFields()
    {

        //display foldable section
        _showLandRestrictionTools = EditorGUILayout.Foldout(_showLandRestrictionTools, "Land-restriction drawing tools", true);
        if (_showLandRestrictionTools)
        {
            EditorGUI.indentLevel++;
            
            if (GUILayout.Button("Ping camera data"))
            {

                Camera[] cams = SceneView.GetAllSceneCameras();
                Camera sceneCamera = new Camera();
                foreach (Camera c in cams)
                {
                    if (c.name == StringConstants.SceneCameraName)
                    {
                        sceneCamera = c;
                        break;
                    }
                }

                if (sceneCamera == null)
                    return;
                
                Debug.Log(sceneCamera.name);

                Debug.Log("sceneCamera.transform.position : " + sceneCamera.transform.position);//actual camera position?
                //Debug.Log("view.camera.transform.rotation : " + view.camera.transform.rotation);//same as view.rotation

                //Debug.Log("view.position : " + view.position);//view port/window?
                //Debug.Log("view.pivot : " + view.pivot);//degrees? really dont know what this is
                //Debug.Log("view.rotation : " + view.rotation);//radians?
                
            }
            
            if (GUILayout.Button((_landRestrictionDrawingEnabled ? "Disable" : "Enable") + " land-restriction drawing tools"))
            {
                _landRestrictionDrawingEnabled = !_landRestrictionDrawingEnabled;

                if (_landRestrictionDrawingEnabled)
                {

                    SceneView view = SceneView.lastActiveSceneView;

                    view.LookAt(_terrainTarget.transform.position, Quaternion.Euler(90.0f, 0.0f, 0.0f));

                    EditorGUIUtility.PingObject(_terrainTarget);
                    Selection.activeGameObject = _terrainTarget.gameObject;
                    view.FrameSelected();

                }
            }

            GUILayout.Label("(Consider the Y coordinate to be the Z coordinate)", EditorStyles.boldLabel);

            //EditorGUI.indentLevel++;

            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty vectorProperty = so.FindProperty("_restrictedAreas");
            if (vectorProperty != null)
            {
                EditorGUILayout.PropertyField(vectorProperty, true);
                so.ApplyModifiedProperties();
            }
        }
        else
        {
            _landRestrictionDrawingEnabled = false;
        }
        //end of foldable section

    }
    private void CreateGeneratorButtons()
    {

        //generator buttons
        EditorGUI.BeginDisabledGroup(!_terrainTarget);
        if (GUILayout.Button(StringConstants.GenerateEnvironment_ButtonText))
        {
            //BasicPrefabGenerationAlgorithm();
            //CalculateModelFaceArea();
        }
        CreateSpace(1);
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
    

    private Vector3 NewModelRelativePosition(GameObject previousModel, GameObject newModel)
    {
        //rotate the object to account for its new position
        newModel.transform.rotation = previousModel.transform.rotation;
        
        //adjust rotation if the previous object was a Large Industrial building 
        if (previousModel.name.Contains(StringConstants.LargeIndustrial))
            newModel.transform.Rotate(0.0f, -90.0f, 0.0f);

        //return the movement vector for the Translate function
        return GlobalMethods.VectorToMoveObjectInLocalAxis(previousModel.name, newModel.name);
    }


    private void ModelPrefabGenerationAlgorithm()
    {
        //get all models
        Object[] models = GlobalMethods.GetPrefabs(StringConstants.ModelPrefabFilePath);

        int groupCount = 0;

        //full generation loop
        for (int currentTotal = 0; currentTotal < /*-1*/_maximumNumberOfObjects; currentTotal += 0)
        {
            //select a random model (generator 'seed')
            Object obj = models[Random.Range(0, models.Length)];

            //give it a starting position
            VectorBoolReturn startVector = GlobalMethods.StartingVector(new Vector3(), _terrainTarget.terrainData.size, _terrainTarget);
            if (!startVector.OperationSuccess)
            {
                GlobalMethods.DisplayError("Failed to seed environment with initial start vector\n\nLikely an issue with input vector dimensions compared with the Terrain's world vertex coordinates");
                return;
            }

            //instantiate the 'seed' model and initiate the generator
            GameObject newModel = (GameObject)PrefabUtility.InstantiatePrefab(obj);
            newModel.transform.position = startVector.Vector;
            newModel.transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));

            if (!InitialCheckIntersectionForSeriesStart(newModel) && ModelWithinParameters(newModel))
            {
                DestroyImmediate(newModel);
                goto cancelledseries;
            }

            //save a reference of the previously generated object
            GameObject previousModel = newModel;      //groupTotal starts at 1

            //series generation loop
            for (int seriesQuantity = 1; seriesQuantity < _maximumNumberInSeriesOrCluster; seriesQuantity++)
            {
                //check if we have reached the maximum, exit full generation loop if so
                if (seriesQuantity + currentTotal >= _maximumNumberOfObjects)
                    goto finishedgeneration;

                //continue selecting models to generate in series
                Object newObject = models[Random.Range(0, models.Length - 1)];
                
                //check if new model fits parameters
                _loopFailCount = 0;
                bool loopSuccess = false;
                do
                {
                    //exit method if loop limit reached, an error has occurred
                    if (++_loopFailCount > _maxLoopFail)
                    {
                        Debug.Log("model loop parameter failure");
                        currentTotal += seriesQuantity;
                        groupCount++;
                        goto cancelledseries;
                    }

                    //select and instantiate model
                    newObject = models[Random.Range(0, models.Length - 1)];
                    newModel = PrefabUtility.InstantiatePrefab(newObject) as GameObject;

                    //initialise the position to match the previous model
                    newModel.transform.position = previousModel.transform.position;

                    //move the object to the new relative position
                    newModel.transform.Translate(NewModelRelativePosition(previousModel, newModel), previousModel.transform);

                    //check whether parameters matched
                    if (ModelWithinParameters(previousModel, newModel))
                    {
                        loopSuccess = true;
                        previousModel = newModel;
                    }
                    else
                    {
                        DestroyImmediate(newModel);
                    }

                } while (!loopSuccess);
                
            }//end of _maximumNumberInSeriesOrCluster loop - series generation loop

            currentTotal += _maximumNumberInSeriesOrCluster;
            groupCount++;

        cancelledseries:;

            if (groupCount >= _maximumNumberOfGroups)
                goto finishedgeneration;

        }//end of  _maximumNumberOfObjects loop - full generation loop

    finishedgeneration:;
        
    }


    private bool InitialCheckIntersectionForSeriesStart(GameObject newObj)
    {
        foreach (GameObject o in GetAllSceneModels())
        {
            if (o == newObj)
                continue;

            MeshRenderer objMesh = o.transform.GetChild(0).GetComponent<MeshRenderer>();
            MeshRenderer newObjMesh = newObj.transform.GetChild(0).GetComponent<MeshRenderer>();

            if (newObjMesh.bounds.Intersects(objMesh.bounds))
            {
                return false;
            }
        }

        return true;
    }

    private bool ModelWithinParameters(GameObject newObject)
    {
        //get the models mesh (always getchild at 0) and then its vertices
        Vector3[] vertices = newObject.transform.GetChild(0).transform.GetComponent<MeshFilter>().sharedMesh.vertices;

        //check object is within the X and Z bounds of the terrain object
        foreach (Vector3 vertex in vertices)
            if (!VertexWithinTerrainBounds(newObject.transform.TransformPoint(vertex)))
                return false;

        return true;
    }

    private bool CheckIntersectionForNewObjectInSeries(GameObject prevObj, GameObject newObj)
    {
        foreach (GameObject o in GetAllSceneModels())
        {
            if (o == newObj || o == prevObj)
                continue;
            
            MeshRenderer objMesh = o.transform.GetChild(0).GetComponent<MeshRenderer>();
            MeshRenderer newObjMesh = newObj.transform.GetChild(0).GetComponent<MeshRenderer>();

            if (newObjMesh.bounds.Intersects(objMesh.bounds))
                return false;
            
        }
        
        return true;
    }

    private bool ModelWithinParameters(GameObject previousObject, GameObject newObject)
    {
        return ModelWithinParameters(newObject) && CheckIntersectionForNewObjectInSeries(previousObject, newObject);
    }

    private bool VertexWithinTerrainBounds(Vector3 vertex)
    {
        //the Vertex X and Z coordinates need to be within the bounds of the terrain
        //
        //ie. greater than the starting X and Z position of the terrain but less than
        //the starting point plus the dimension along its respective axis
        //
        return vertex.x > _terrainTarget.transform.position.x && vertex.x < _terrainTarget.transform.position.x + _terrainTarget.terrainData.size.x &&
            vertex.z > _terrainTarget.transform.position.z && vertex.z < _terrainTarget.transform.position.z + _terrainTarget.terrainData.size.z;
    }
    

    private List<GameObject> GetAllSceneModels()
    {

        List<GameObject> objectsInScene = new List<GameObject>();

        foreach (GameObject o in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
        {
            if (o.hideFlags == HideFlags.NotEditable || o.hideFlags == HideFlags.HideAndDontSave)
                continue;

            if (EditorUtility.IsPersistent(o.transform.root.gameObject))
                continue;

            if (o.layer == LayerMask.NameToLayer(StringConstants.BaseObjectTag))
                objectsInScene.Add(o);
        }

        return objectsInScene;

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
